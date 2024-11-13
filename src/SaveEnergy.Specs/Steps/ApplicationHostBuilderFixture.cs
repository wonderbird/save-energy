using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaveEnergy.Domain;
using SaveEnergy.TestHelpers;
using Xunit.Sdk;

namespace SaveEnergy.Specs.Steps;

/// <summary>Set up the host application</summary>
/// 
/// <remarks>
/// <para>
/// The implementation is adopted from
/// https://stackoverflow.com/questions/77936032/reuse-host-for-integration-testing-in-a-net-8-console-app
/// </para>
///
/// <para>
/// The logging setup is adopted from
/// https://www.meziantou.net/how-to-view-logs-from-ilogger-in-xunitdotnet.htm
/// </para>
/// </remarks>
public sealed class ApplicationHostBuilderFixture : IDisposable
{
    public ApplicationHostBuilderFixture(TestOutputHelper testOutputHelper)
    {
        TestOutputPresenter = new TestOutputPresenter(testOutputHelper);
        
        var builder = Program.CreateApplicationBuilder([]);
        builder.Services
            .RemoveAll(typeof(ICanPresentOutput))
            .AddSingleton<ICanPresentOutput>(TestOutputPresenter);

        // builder.Configuration
        //     .AddInMemoryCollection(new Dictionary<string, string>
        //     {
        //         ["GitHub:AuthenticationBaseAddress"] = _mockServer.Url
        //     });
        
        builder.Logging
            .ClearProviders()
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));
        
        builder.Environment.EnvironmentName = "Test";
        
        Host = builder.Build();
    }

    public IHost Host { get; }
    public TestOutputPresenter TestOutputPresenter { get; set; }

    public void Dispose() => Host.Dispose();
}