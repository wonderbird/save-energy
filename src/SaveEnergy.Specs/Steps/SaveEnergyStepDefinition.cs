using TestProcessWrapper;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

[Binding]
public sealed class SaveEnergyStepDefinition
{
    private readonly TestOutputHelper _testOutputHelper;
    
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
        using var process = new TestProcessWrapper.TestProcessWrapper("SaveEnergy", true, buildConfiguration);
        process.TestOutputHelper = _testOutputHelper;

        process.AddReadinessCheck(output => output.Contains("SaveEnergy has completed"));

        process.Start();
        process.WaitForProcessExit();
        process.ForceTermination();

        _testOutputHelper.WriteLine("===== Recorded process output =====");
        _testOutputHelper.WriteLine(process.RecordedOutput);
        _testOutputHelper.WriteLine("===== End of recorded process output =====");
    }

    [Then(@"Hello World is displayed")]
    public void ThenHelloWorldIsDisplayed()
    {
        ScenarioContext.StepIsPending();
    }
}