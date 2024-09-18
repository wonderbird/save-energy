using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace SaveEnergy.Specs.Hooks;

[Binding]
public class EnableCoverageMeasurement
{
    [BeforeScenario]
    public void EnableCoverage(ITestOutputHelper testOutputHelper)
    {
        // TODO: Can the coverage measurement be enabled / disabled for all tests by a command line flag or configuration file?
        testOutputHelper.WriteLine("Coverage measurement is enabled");
    }
}