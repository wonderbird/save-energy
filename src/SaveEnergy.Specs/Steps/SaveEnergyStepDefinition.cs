using FluentAssertions;
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

    public SaveEnergyStepDefinition(TestOutputHelper testOutputHelper, ScenarioContext context, MockServer mockServer)
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

    [Given(@"device flow is enabled for the GitHub app")]
    public void GivenDeviceFlowIsEnabledForTheGitHubApp()
    {
        _mockServer.ConfigureSuccessfulDeviceAuthorization();
    }

    [Given(@"the user owns the following repositories")]
    public void GivenTheUserOwnsTheFollowingRepositories(Table table)
    {
        var repositories = table.CreateSet<Repository>();
        _mockServer.ConfigureRepositories(repositories);
    }

    [Given(@"the user owns a repository")]
    public void GivenTheUserOwnsARepository()
    {
        _mockServer.ConfigureRepositories(new List<Repository>
            { new("SaveEnergy", "https://github.com/wonderbird/save-energy") });
    }

    [When(@"I run the application")]
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

    [Then(@"at least one repository URL is printed to the console")]
    public void ThenAtLeastOneRepositoryUrlIsPrintedToTheConsole()
    {
        Assert.Contains("https://github.com/", _process?.RecordedOutput);
    }

    [Then(@"it performs the device authorization flow")]
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
    /// | Repository name | URL |
    /// | --- | --- |
    /// | repo1 | https://github.com/... |
    /// | repo2 | https://github.com/... |
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
    [Then(@"(.*) repository URLs are printed to the console")]
    public void ThenRepositoryUrLsArePrintedToTheConsole(int count)
    {
        var outputRows = _process?.RecordedOutput.Split('\n') ?? [];
        var tableStartIndex = Array.IndexOf(outputRows, "| Repository name | URL |");
        var tableBodyStartIndex = tableStartIndex + 2;
        var numberOfRepositories = 0;
        var isTableBody = true;

        _testOutputHelper.WriteLine($"Verifying that {count} repositories were printed to the console. We found:");
        while (tableStartIndex != -1 && isTableBody)
        {
            isTableBody = outputRows[tableBodyStartIndex + numberOfRepositories].StartsWith('|');
            if (isTableBody)
            {
                _testOutputHelper.WriteLine($"{outputRows[tableBodyStartIndex + numberOfRepositories]}");
                numberOfRepositories++;
            }
        }

        numberOfRepositories.Should().Be(count);
    }
}