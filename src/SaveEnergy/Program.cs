using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveEnergy;
using SaveEnergy.Adapters.Inbound;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("TracingClient");

builder.Services.AddTransient<ICanAuthenticate, DeviceFlowAuthenticator>();
builder.Services.AddTransient<IRepositoriesQuery, RepositoriesQuery>();

builder.Services.AddHostedService<CommandLineInterface>();

using var host = builder.Build();
await host.RunAsync();
