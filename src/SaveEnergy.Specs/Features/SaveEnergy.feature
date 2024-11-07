Feature: Save Energy

    As a GitHub user
    I want to list the number of days since the last commit for each of my repositories
    so that I can see which repositories are active and which are not.

    @NonParallelizable
    Scenario: List GitHub repositories
        Given device flow is enabled for the GitHub app
        And the user owns the following repositories
          | Name            | PushedAt             | HtmlUrl                                     | SshUrl                                      | CloneUrl                                        |
          | SaveTheUniverse | 2026-11-05T20:33:00Z | https://github.com/wonderbird/save-universe | git@github.com:wonderbird/save-universe.git | https://github.com/wonderbird/save-universe.git |
          | SaveEnergy      | 2024-11-05T20:33:00Z | https://github.com/wonderbird/save-energy   | git@github.com:wonderbird/save-energy.git   | https://github.com/wonderbird/save-energy.git   | 
          | SaveTheWorld    | 2025-11-05T20:33:00Z | https://github.com/wonderbird/save-world    | git@github.com:wonderbird/save-world.git    | https://github.com/wonderbird/save-world.git    |
        When I run the application
        Then the following repositories table is printed to the console
          | Name            | PushedAt             | HtmlUrl                                     | SshUrl                                      | CloneUrl                                        |
          | SaveEnergy      | 2024-11-05T20:33:00Z | https://github.com/wonderbird/save-energy   | git@github.com:wonderbird/save-energy.git   | https://github.com/wonderbird/save-energy.git   | 
          | SaveTheWorld    | 2025-11-05T20:33:00Z | https://github.com/wonderbird/save-world    | git@github.com:wonderbird/save-world.git    | https://github.com/wonderbird/save-world.git    |
          | SaveTheUniverse | 2026-11-05T20:33:00Z | https://github.com/wonderbird/save-universe | git@github.com:wonderbird/save-universe.git | https://github.com/wonderbird/save-universe.git |
