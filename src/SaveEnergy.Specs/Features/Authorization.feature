Feature: Authorize the application to read information about repositories owned by the user

    The application must request permission to read all repositories owned by the user.
    
    This requirement is fulfilled by implementing the OAuth 2.0 device flow.
    
    Details are described in the section "Device flow" at
    https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps

    Scenario: Successful Device Flow Authorization
        Given device flow is enabled for the GitHub app
        When I run the application
        Then it performs the device authorization flow
