using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveEnergy.Adapters.Inbound;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;

var builder = CreateApplicationBuilder(args);
using var host = builder.Build();
await host.RunAsync();

// Make the Program class public so that it can be used in the integration tests
// See https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
public static partial class Program
{
    /// <summary>Setup host application builder</summary>
    public static HostApplicationBuilder CreateApplicationBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("TracingClient");

        builder.Services.AddTransient<ICanPresentOutput, ConsoleOutputPresenter>();
        builder.Services.AddTransient<ICanAuthenticate, DeviceFlowAuthenticator>();
        builder.Services.AddTransient<IRepositoriesQuery, RepositoriesQuery>();

        builder.Services.AddHostedService<CommandLineInterface>();

        return builder;
    }
}
