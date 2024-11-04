using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace SaveEnergy.Specs.Hooks;

[Binding]
public class EnableCoverageMeasurement
{
    [BeforeScenario]
    public void EnableCoverage(ITestOutputHelper testOutputHelper)
    {
        testOutputHelper.WriteLine("Coverage measurement is enabled");
    }
}
