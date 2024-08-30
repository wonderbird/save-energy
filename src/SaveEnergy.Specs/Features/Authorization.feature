Feature: Authorize the application to use the GitHub API

    The application needs the users' permission to read all repositories owned by them.
    Technically this requires that the users authorize the application to read information
    about their repositories. The application uses the device authorization flow for
    this purpose.
    
    Details are described in the section "Device flow" at
    https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps
    
    @NonParallelizable
    Scenario: Successful Device Flow Authorization
        Given device flow is enabled for the GitHub app
        When I run the application
        Then it performs the device authorization flow
