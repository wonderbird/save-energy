using TestProcessWrapper;
using Xunit;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

[Binding]
public sealed class SaveEnergyStepDefinition : IDisposable
{
    private readonly TestOutputHelper _testOutputHelper;
    private TestProcessWrapper.TestProcessWrapper? _process;
    private bool _isDisposed;

    public SaveEnergyStepDefinition(TestOutputHelper testOutputHelper, ScenarioContext _)
    {
        _testOutputHelper = testOutputHelper;
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

        _process.Start();
        _process.WaitForProcessExit();
        _process.ForceTermination();

        _testOutputHelper.WriteLine("===== Recorded process output =====");
        _testOutputHelper.WriteLine(_process.RecordedOutput);
        _testOutputHelper.WriteLine("===== End of recorded process output =====");
    }

    [Then(@"Hello World is displayed")]
    public void ThenHelloWorldIsDisplayed()
    {
        Assert.Contains("Hello, World!", _process?.RecordedOutput);
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
}
