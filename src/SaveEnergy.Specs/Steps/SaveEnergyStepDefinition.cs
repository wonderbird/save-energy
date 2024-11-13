using System.Globalization;
using FluentAssertions;
using SaveEnergy.Domain;
using SaveEnergy.Specs.Hooks;
using SaveEnergy.TestHelpers;
using TestProcessWrapper;
using Xunit;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

[Binding]
public sealed class SaveEnergyStepDefinition : IDisposable, IClassFixture<ApplicationHostBuilderFixture>
{
    private const int ExpectedColumnsInResultTable = 6;
    private readonly TestOutputHelper _testOutputHelper;
    private readonly MockServer _mockServer;
    private TestProcessWrapper.TestProcessWrapper? _process;
    private bool _isDisposed;
    private readonly IHost _host;
    private readonly TestOutputPresenter _testOutputPresenter;

    public SaveEnergyStepDefinition(
        TestOutputHelper testOutputHelper,
        ScenarioContext context,
        MockServer mockServer,
        ApplicationHostBuilderFixture fixture
    )
    {
        _testOutputHelper = testOutputHelper;
        _mockServer = mockServer;
        _host = fixture.Host;
        _testOutputPresenter = fixture.TestOutputPresenter;
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
        TableShouldMatchExpectedFormat(table.Header.Count);
        
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

    // Summary
    // -------
    //
    // The goal of this method is to provide a step which replaces the
    // WhenIRunTheApplication step in SaveEnergyStepDefinition.cs
    //
    // Details
    // -------
    //
    // Provide a Reqnroll when statement to set up the application host
    // using Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory
    //
    // The following articles describe the general approach:
    // - https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
    // - https://stackoverflow.com/questions/77936032/reuse-host-for-integration-testing-in-a-net-8-console-app
    //
    // On GitHub there is a sample application:
    // - https://github.com/dotnet/AspNetCore.Docs.Samples/tree/main/test/integration-tests/8.x/IntegrationTestsSample/tests/RazorPagesProject.Tests
    //
    // This is the documentation for the WebApplicationFactory:
    // - https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-8.0
    [When(@"I run the application with the Microsoft\.AspNetCore\.Mvc\.Testing\.WebApplicationFactory")]
    public async Task WhenIRunTheApplicationWithTheMicrosoftAspNetCoreMvcTestingWebApplicationFactory()
    {
        await _host.RunAsync();
    }
    
    /// <summary>Identify table of repositories in the recorded output</summary>
    ///
    /// <remarks>
    /// The shape of the output is:
    ///
    /// <code>
    /// ... some text ...
    /// | Repository name | Last Pushed | HTML URL | SSH URL | Clone URL |
    /// | --- | --- | --- | --- | --- |
    /// | repo1 | 2022-10-01T00:00:00Z | https://github.com/... | git@github.com:... | https://github.com/... |
    /// | repo2 | 2021-10-01T00:00:00Z | https://github.com/... | git@github.com:... | https://github.com/... |
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
        TableShouldMatchExpectedFormat(table.Header.Count);

        var expectedRepositories = table.CreateSet<Repository>().ToList();
        
        _testOutputHelper.WriteLine("Verifying that the following repositories were printed to the console:");
        foreach (var repository in expectedRepositories)
        {
            _testOutputHelper.WriteLine($"| {repository.Name} | {repository.HtmlUrl} |");
        }
        
        var outputRows = _process?.RecordedOutput.Split('\n') ?? [];
        var tableStartIndex = Array.IndexOf(outputRows,"| Repository name | Last Change | Description | HTML URL | SSH URL | Clone URL |");
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

    [Then("alternative the following repositories table is printed to the console")]
    public void ThenAlternativeTheFollowingRepositoriesTableIsPrintedToTheConsole(Table table)
    {
        TableShouldMatchExpectedFormat(table.Header.Count);

        var expectedRepositories = table.CreateSet<Repository>().ToList();

        _testOutputHelper.WriteLine("Verifying that the following repositories were printed to the console:");
        foreach (var repository in expectedRepositories)
        {
            _testOutputHelper.WriteLine($"| {repository.Name} | {repository.HtmlUrl} |");
        }

        // TODO Later: Probably transition to the design presented in https://stackoverflow.com/questions/77211786/how-to-start-a-full-net-core-worker-service-in-an-integration-end-to-end-test
        var outputRows = _testOutputPresenter.RecordedOutput.Split('\n');
        
        var tableStartIndex = Array.IndexOf(outputRows,
            "| Repository name | Last Change | Description | HTML URL | SSH URL | Clone URL |");
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

    [Then("it reports the error to the user")]
    public void ThenItReportsTheErrorToTheUser()
    {
        _process
            ?.RecordedOutput.Should()
            .Contain(
                "An error prevents executing the command. Please check the logs for more information."
            );
    }

    private static void TableShouldMatchExpectedFormat(int numberOfColumns)
    {
        numberOfColumns.Should().Be(ExpectedColumnsInResultTable,
            $"this function handles only specification tables with {{ExpectedColumnsInResultTable}} columns");
    }

    private static Repository ParseRepositoryFromTableRow(string outputRow)
    {
        var columns = outputRow.Split('|').Select(c => c.Trim()).ToList();
        columns = columns.Skip(1).Take(columns.Count - 2).ToList();

        TableShouldMatchExpectedFormat(columns.Count);

        const int nameColumn = 0;
        const int pushedAtColumn = 1;
        const int descriptionColumn = 2;
        const int htmlUrlColumn = 3;
        const int sshUrlColumn = 4;
        const int cloneUrlColumn = 5;

        return new Repository(
            columns[nameColumn],
            DateTime.Parse(columns[pushedAtColumn], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            columns[descriptionColumn],
            columns[htmlUrlColumn],
            columns[sshUrlColumn],
            columns[cloneUrlColumn]);
    }
}