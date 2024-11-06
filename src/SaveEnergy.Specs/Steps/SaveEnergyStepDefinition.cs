using FluentAssertions;
using SaveEnergy.Domain;
using SaveEnergy.Specs.Hooks;
using TestProcessWrapper;
using Xunit;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

[Binding]
public sealed class SaveEnergyStepDefinition : IDisposable
{
    private readonly TestOutputHelper _testOutputHelper;
    private readonly MockServer _mockServer;
    private TestProcessWrapper.TestProcessWrapper? _process;
    private bool _isDisposed;

    public SaveEnergyStepDefinition(
        TestOutputHelper testOutputHelper,
        ScenarioContext context,
        MockServer mockServer
    )
    {
        _testOutputHelper = testOutputHelper;
        _mockServer = mockServer;
    }

    ~SaveEnergyStepDefinition()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed && disposing)
        {
            _process?.Dispose();
        }

        _isDisposed = true;
    }

    [Given("device flow is enabled for the GitHub app")]
    public void GivenDeviceFlowIsEnabledForTheGitHubApp()
    {
        _mockServer.ConfigureSuccessfulDeviceAuthorization();
    }

    [Given("the user owns the following repositories")]
    public void GivenTheUserOwnsTheFollowingRepositories(Table table)
    {
        var repositories = table.CreateSet<Repository>();
        _mockServer.ConfigureRepositories(repositories);
    }

    [Given("the user owns a repository")]
    public void GivenTheUserOwnsARepository()
    {
        _mockServer.ConfigureRepositories(
            new List<Repository>
            {
                new() { Name = "SaveEnergy", HtmlUrl = "https://github.com/wonderbird/save-energy" }
            }
        );
    }

    [Given("the GitHub API returns internal errors")]
    public void GivenTheGitHubApiReturnsInternalErrors()
    {
        _mockServer.ConfigureInternalServerError();
    }

    [When("I run the application")]
    public void WhenIRunTheApplication()
    {
#if DEBUG
        const BuildConfiguration buildConfiguration = BuildConfiguration.Debug;
#else
        const BuildConfiguration buildConfiguration = BuildConfiguration.Release;
#endif
        _process = new TestProcessWrapper.TestProcessWrapper(
            "SaveEnergy",
            true,
            buildConfiguration
        );
        _process.TestOutputHelper = _testOutputHelper;

        _process.AddEnvironmentVariable("GitHub__AuthenticationBaseAddress", _mockServer.Url);
        _process.AddEnvironmentVariable("GitHub__ApiBaseAddress", _mockServer.Url);

        _process.Start();
        _process.WaitForProcessExit();
        _process.ForceTermination();

        _testOutputHelper.WriteLine("===== Recorded process output =====");
        _testOutputHelper.WriteLine(_process.RecordedOutput);
        _testOutputHelper.WriteLine("===== End of recorded process output =====");
    }

    [Then("at least one repository URL is printed to the console")]
    public void ThenAtLeastOneRepositoryUrlIsPrintedToTheConsole()
    {
        Assert.Contains("https://github.com/", _process?.RecordedOutput);
    }

    [Then("it performs the device authorization flow")]
    public void ThenItPerformsTheDeviceAuthorizationFlow()
    {
        _mockServer.VerifyDeviceAuthorizationFlow();
    }

    /// <summary>Identify table of repositories in the recorded output</summary>
    ///
    /// <remarks>
    /// The shape of the output is:
    ///
    /// <code>
    /// ... some text ...
    /// | Repository name | URL | ...
    /// | --- | --- | ...
    /// | repo1 | https://github.com/... | ...
    /// | repo2 | https://github.com/... | ...
    /// ... some text ...
    /// +------------+------+--------+--------+
    /// | Module     | Line | Branch | Method |
    /// +------------+------+--------+--------+
    /// | SaveEnergy | 100% | 75%    | 100%   |
    /// +------------+------+--------+--------+
    /// ... some text ...
    /// </code>
    ///
    /// The second table is the code coverage report, which we want to ignore.
    /// In this case, we want to identify the rows of repo1 and repo2
    /// </remarks>
    [Then("the following repositories table is printed to the console")]
    public void ThenTheFollowingRepositoriesTableIsPrintedToTheConsole(Table table)
    {
        var expectedRepositories = table.CreateSet<Repository>().ToList();
        
        _testOutputHelper.WriteLine("Verifying that the following repositories were printed to the console:");
        foreach (var repository in expectedRepositories)
        {
            _testOutputHelper.WriteLine($"| {repository.Name} | {repository.HtmlUrl} |");
        }
        
        var outputRows = _process?.RecordedOutput.Split('\n') ?? [];
        var tableStartIndex = Array.IndexOf(outputRows, "| Repository name | URL |");
        var tableBodyStartIndex = tableStartIndex + 2;
        var numberOfRepositories = 0;
        var isTableBody = true;
        
        var actualRepositories = new List<Repository>();
        while (tableStartIndex != -1 && isTableBody)
        {
            var outputRow = outputRows[tableBodyStartIndex + numberOfRepositories];
            
            isTableBody = outputRow.StartsWith('|');
            if (isTableBody)
            {
                var parsedRepository = ParseRepositoryFromTableRow(outputRow);
                actualRepositories.Add(parsedRepository);
                numberOfRepositories++;
            }
        }

        _testOutputHelper.WriteLine("We parsed the following repositories from the printed output:");
        foreach (var repository in actualRepositories)
        {
            _testOutputHelper.WriteLine(repository.ToString());
        }
        
        actualRepositories.Should().Equal(expectedRepositories);
    }

    private static Repository ParseRepositoryFromTableRow(string outputRow)
    {
        const int nameColumn = 1;
        const int htmlUrlColumn = 2;
        
        var columns = outputRow.Split('|').Select(c => c.Trim()).ToList();
        
        return new Repository(columns[nameColumn], columns[htmlUrlColumn]);
    }

    [Then("it reports the error to the user")]
    public void ThenItReportsTheErrorToTheUser()
    {
        _process
            ?.RecordedOutput.Should()
            .Contain(
                "An error prevents executing the command. Please check the logs for more information."
            );
    }
}
