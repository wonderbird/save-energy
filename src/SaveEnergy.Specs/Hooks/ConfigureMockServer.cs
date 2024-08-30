using Xunit.Abstractions;

namespace SaveEnergy.Specs.Hooks;

[Binding]
public class ConfigureMockServer
{
    [BeforeTestRun]
    public static void Start(MockServer mockServer)
    {
        // TODO: Check if multiple mock servers can be started in parallel. If so, we can remove the NonParallelizable attribute from the feature files and from the configuration.
        mockServer.Start();
    }
    
    [BeforeScenario]
    public void Reset(MockServer mockServer, ITestOutputHelper testOutputHelper)
    {
        mockServer.Reset();
        testOutputHelper.WriteLine($"Mock server is listening on {mockServer.Url}");
    }
    
    [AfterTestRun]
    public static void Stop(MockServer mockServer)
    {
        mockServer.Stop();
    }
}