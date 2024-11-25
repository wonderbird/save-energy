using System.Globalization;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaveEnergy.Domain;
using SaveEnergy.Specs.Hooks;
using SaveEnergy.TestHelpers;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

[Binding]
public sealed class SaveEnergyStepDefinition : IDisposable
{
    private const int ExpectedColumnsInResultTable = 6;

    private readonly TestOutputHelper _testOutputHelper;
    private readonly MockServer _mockServer;
    private readonly TestOutputPresenter _testOutputPresenter;
    private readonly IHost _host;

    private bool _isDisposed;

    public SaveEnergyStepDefinition(
        TestOutputHelper testOutputHelper,
        ScenarioContext context,
        MockServer mockServer
    )
    {
        _testOutputHelper = testOutputHelper;
        _mockServer = mockServer;
        _testOutputPresenter = new TestOutputPresenter(_testOutputHelper);

        _host = CreateApplicationHost();
    }

    private IHost CreateApplicationHost()
    {
        var builder = Program.CreateApplicationBuilder([]);
        builder
            .Services.RemoveAll(typeof(ICanPresentOutput))
            .AddSingleton<ICanPresentOutput>(_testOutputPresenter);

        builder.Configuration.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    "GitHub:AuthenticationBaseAddress",
                    _mockServer.Url
                ),
                new KeyValuePair<string, string?>("GitHub:ApiBaseAddress", _mockServer.Url)
            ]
        );

        builder.Logging.ClearProviders().AddProvider(new XUnitLoggerProvider(_testOutputHelper));

        builder.Environment.EnvironmentName = "Test";

        return builder.Build();
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
            _host.Dispose();
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

    [Given("the GitHub API returns internal errors")]
    public void GivenTheGitHubApiReturnsInternalErrors()
    {
        _mockServer.ConfigureInternalServerError();
    }

    [When(@"I run the application")]
    public async Task WhenIRunTheApplication()
    {
        await _host.RunAsync();
    }

    [Then("the following repositories table is printed to the console")]
    public void ThenTheFollowingRepositoriesTableIsPrintedToTheConsole(Table table)
    {
        TableShouldMatchExpectedFormat(table.Header.Count);

        var expectedRepositories = table.CreateSet<Repository>().ToList();

        LogRepositoriesWithMessage(expectedRepositories, "Repositories expected in output:");

        const char tableIdentifier = '|';
        const int headerAndSeparator = 2;

        var actualRepositories = _testOutputPresenter
            .RecordedOutput.Where(r => r.StartsWith(tableIdentifier))
            .Skip(headerAndSeparator)
            .Select(ParseRepositoryFromTableRow);

        LogRepositoriesWithMessage(actualRepositories, "Repositories parsed from output:");

        actualRepositories.Should().Equal(expectedRepositories);
    }

    [Then("it performs the device authorization flow")]
    public void ThenItPerformsTheDeviceAuthorizationFlow()
    {
        _mockServer.VerifyDeviceAuthorizationFlow();
    }

    [Then("it reports the error to the user")]
    public void ThenItReportsTheErrorToTheUser()
    {
        _testOutputPresenter
            .RecordedOutput.Should()
            .Contain(
                "An error prevents executing the command. Please check the logs for more information."
            );
    }

    private void LogRepositoriesWithMessage(
        IEnumerable<Repository> expectedRepositories,
        string message
    )
    {
        _testOutputHelper.WriteLine(message);
        foreach (var repository in expectedRepositories)
        {
            _testOutputHelper.WriteLine(repository.ToString());
        }
    }

    private static void TableShouldMatchExpectedFormat(int numberOfColumns)
    {
        numberOfColumns
            .Should()
            .Be(
                ExpectedColumnsInResultTable,
                $"this function handles only specification tables with {{ExpectedColumnsInResultTable}} columns"
            );
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
            DateTime.Parse(
                columns[pushedAtColumn],
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind
            ),
            columns[descriptionColumn],
            columns[htmlUrlColumn],
            columns[sshUrlColumn],
            columns[cloneUrlColumn]
        );
    }
}
