Feature: Save Energy

    As a GitHub user
    I want to list the number of days since the last commit for each of my repositories
    so that I can see which repositories are active and which are not.

    @NonParallelizable
    Scenario: List GitHub repositories (alternative way to run the application)
        Given device flow is enabled for the GitHub app
        And the user owns the following repositories
          | Name            | PushedAt             | Description                        | HtmlUrl                                     | SshUrl                                      | CloneUrl                                        |
          | SaveTheUniverse | 2026-11-05T20:33:00Z | GitHub action to save the universe | https://github.com/wonderbird/save-universe | git@github.com:wonderbird/save-universe.git | https://github.com/wonderbird/save-universe.git |
          | SaveEnergy      | 2024-11-05T20:33:00Z | GitHub action to save energy       | https://github.com/wonderbird/save-energy   | git@github.com:wonderbird/save-energy.git   | https://github.com/wonderbird/save-energy.git   | 
          | SaveTheWorld    | 2025-11-05T20:33:00Z | GitHub action to save the world    | https://github.com/wonderbird/save-world    | git@github.com:wonderbird/save-world.git    | https://github.com/wonderbird/save-world.git    |
        When I run the application
        Then the following repositories table is printed to the console
          | Name            | PushedAt             | Description                        | HtmlUrl                                   | SshUrl                                    | CloneUrl                                      |
          | SaveEnergy      | 2024-11-05T20:33:00Z | GitHub action to save energy       | https://github.com/wonderbird/save-energy | git@github.com:wonderbird/save-energy.git | https://github.com/wonderbird/save-energy.git | 
          | SaveTheWorld    | 2025-11-05T20:33:00Z | GitHub action to save the world    | https://github.com/wonderbird/save-world  | git@github.com:wonderbird/save-world.git  | https://github.com/wonderbird/save-world.git  |
          | SaveTheUniverse | 2026-11-05T20:33:00Z | GitHub action to save the universe | https://github.com/wonderbird/save-universe | git@github.com:wonderbird/save-universe.git | https://github.com/wonderbird/save-universe.git |
    