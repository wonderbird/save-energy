using Xunit.Abstractions;

namespace SaveEnergy.Specs.Hooks;

[Binding]
public static class ConfigureMockServer
{
    [BeforeScenario]
    public static void Start(MockServer mockServer, ITestOutputHelper testOutputHelper)
    {
        mockServer.Start();
        testOutputHelper.WriteLine($"Mock server is listening on {mockServer.Url}");
    }

    [AfterScenario]
    public static void Stop(MockServer mockServer)
    {
        mockServer.Stop();
    }
}
