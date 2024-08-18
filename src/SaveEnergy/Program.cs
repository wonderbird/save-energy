using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveEnergy;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<CommandLineInterface>();

using var host = builder.Build();
await host.RunAsync();
