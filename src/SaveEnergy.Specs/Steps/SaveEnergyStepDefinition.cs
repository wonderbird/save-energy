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
}
