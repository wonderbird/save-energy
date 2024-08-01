# Save Energy

Reduce your carbon footprint by saving energy on GitHub.

## How we can save energy on GitHub

Consider sustainability by saving energy for each inactive GitHub repository.

- Remove trigger building the project at a regular time interval.
- Replace the trigger on every build by a trigger on pull request to main.
- Deactivate dependabot.
- Download to local machine, archive it there and delete from GitHub.

## Relevant GitHub API Endpoints

[./docs/README.md](./docs/README.md) shows how you can run a collection of GitHub API requests relevant to the project.

## Acknowledgements

This project is inspired by Uwe Friedrichsen's talk "[Patterns of Sustainability – Going Green in IT](https://speakerdeck.com/ufried/patterns-of-sustainability-going-green-in-it)" held at the [OOP Conference in 2023](https://www.oop-konferenz.de/).

Many thanks to [JetBrains](https://www.jetbrains.com/?from=save-energy) who provide an [Open Source License](https://www.jetbrains.com/community/opensource/) for this project ❤️.

## References

- [GitHub REST API](https://docs.github.com/en/rest)
