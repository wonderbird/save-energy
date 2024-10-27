Feature: Save Energy

    As a GitHub user
    I want to list the number of days since the last commit for each of my repositories
    so that I can see which repositories are active and which are not.

    @NonParallelizable
    Scenario: List GitHub repositories
        Given device flow is enabled for the GitHub app
        And the user owns the following repositories
          | Name            | HtmlUrl                                     |
          | SaveEnergy      | https://github.com/wonderbird/save-energy   |
          | SaveTheWorld    | https://github.com/wonderbird/save-world    |
          | SaveTheUniverse | https://github.com/wonderbird/save-universe |
        When I run the application
        Then 3 repository URLs are printed to the console
