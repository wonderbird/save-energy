## How to Debug HTTP Requests

### Default and Tracing HTTP Client

The main function in [Program.cs](../src/SaveEnergy/Program.cs) declares a default HTTP client and an HTTP client specifically for tracing.

The default client is affected from the general configuration, while the tracing client
is configured to log detailed information to the console.

If you want to analyze HTTP requests, then use the tracing client by adding the parameter "TracingClient" to the `IHttpClientFactory.CreateClient()` call.

### Example: Debug the DeviceFlowAuthenticator

Add the parameter "TracingClient" to the `_httpClientFactory.CreateClient()` call in the [DeviceFlowAuthenticator](../src/SaveEnergy/Adapters/Outbound/DeviceFlowAuthenticator.cs) file.

When running the program, it will log detailed HTTP request and response messages to the console.
