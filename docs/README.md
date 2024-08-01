# Relevant GitHub API Endpoints

[./github-rest-api.http](./github-rest-api.http) contains a collection of GitHub API requests relevant to the project.

Before you can run the REST requests, you need to register the JetBrains HTTP client as a GitHub App and enable the device flow for authentication. Then update the **Client ID** in the [./http-client.env.json](./http-client.env.json) file.

For more information, see

- [HTTP Client: OAuth 2.0 authorization (IntelliJ IDEA)](https://www.jetbrains.com/help/idea/oauth-2-0-authorization.html)
- [Authenticating to the REST API](https://docs.github.com/en/rest/authentication/authenticating-to-the-rest-api) - [[githubincAuthenticating2024]]
- [Authenticating with a GitHub App on behalf of a user](https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/authenticating-with-a-github-app-on-behalf-of-a-user) - [[githubincAppAsUser2024]]
- [Generating a user access token for a GitHub App](https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/generating-a-user-access-token-for-a-github-app) - [[githubincGenerateToken2024]]
- [Registering a GitHub App](https://docs.github.com/en/apps/creating-github-apps/registering-a-github-app/registering-a-github-app) - [[githubincRegisterApp2024]]