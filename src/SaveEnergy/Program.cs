using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveEnergy;
using SaveEnergy.Adapters.Inbound;
using SaveEnergy.Adapters.Outbound;
using SaveEnergy.Domain;

var builder = Host.CreateApplicationBuilder(args);

// To allow detailed HTTP request tracing, we differentiate between the default client
// and a client specifically for tracing.
// The default client is affected from the general configuration, while the tracing client
// is configured to log detailed information.
// To use the tracing client, add the parameter "TracingClient" to the
// IHttpClientFactory.CreateClient method.
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("TracingClient");

builder.Services.AddTransient<ICanAuthenticate, DeviceFlowAuthenticator>();
builder.Services.AddTransient<IRepositoriesQuery, RepositoriesQuery>();

builder.Services.AddHostedService<CommandLineInterface>();

using var host = builder.Build();
await host.RunAsync();
