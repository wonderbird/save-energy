using System.Text.Json;
using FluentAssertions;
using SaveEnergy.Domain;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SaveEnergy.Specs.Hooks;

public class MockServer
{
    private WireMockServer? _mockServer;
    public string Url => _mockServer?.Urls[0] ?? "Error: WireMockServer has no URL";

    public void Start()
    {
        _mockServer = WireMockServer.Start();
    }

    public void Stop()
    {
        _mockServer?.Stop();
    }

    public void ConfigureSuccessfulDeviceAuthorization()
    {
        ConfigureDeviceCodeResponse();
        ConfigureAccesTokenResponse();
    }

    private void ConfigureDeviceCodeResponse()
    {
        _mockServer
            ?.Given(Request.Create().WithPath("/login/device/code").UsingPost())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(
                        new
                        {
                            device_code = "mockserver_device_code",
                            user_code = "mockserver_user_code",
                            verification_uri = "https://example.com/mockserver_response",
                            expires_in = 300,
                            interval = 0,
                        }
                    )
            );
    }

    private void ConfigureAccesTokenResponse()
    {
        _mockServer
            ?.Given(Request.Create().WithPath("/login/oauth/access_token").UsingPost())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(
                        new
                        {
                            access_token = "mockserver_access_token",
                            expires_in = 3600,
                            refresh_token = "mockserver_refresh_token",
                            token_type = "bearer",
                            scope = "",
                        }
                    )
            );
    }

    public void ConfigureRepositories(IEnumerable<Repository> repositories)
    {
        var repositoriesJson = JsonSerializer.Serialize(repositories);

        _mockServer
            ?.Given(Request.Create().WithPath("/user/repos").UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(repositoriesJson)
            );
    }

    public void ConfigureInternalServerError()
    {
        _mockServer
            ?.Given(Request.Create().UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));
    }

    public void VerifyDeviceAuthorizationFlow()
    {
        var receivedCalls =
            _mockServer
                ?.LogEntries.Select(x => $"{x.RequestMessage.Method} {x.RequestMessage.Path}")
                .ToList() ?? [];

        receivedCalls.Should().HaveCount(3);

        receivedCalls[0].Should().Be("POST /login/device/code");
        receivedCalls[1].Should().Be("POST /login/oauth/access_token");
        receivedCalls[2].Should().StartWith("GET /user/repos");
    }
}
