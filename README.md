# Save Energy

Reduce your carbon footprint by saving energy on GitHub.

## How we can save energy on GitHub

Consider sustainability by saving energy for each inactive GitHub repository.

- Reduce the interval for automatically rebuilding the project or entirely
  remove regular automatic builds, e.g. for non-production code.
- Replace the trigger on every build by a trigger on pull request to main.
  Add a trigger to allow building manually on demand.
- Actively avoid running a CI pipeline if no relevant change was made, see the
  [Skipping workflow runs](https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-workflow-runs/skipping-workflow-runs) article
- Reduce the frequency of dependabot checks or deactivate dependabot.
- Limit the retention duration for uploaded artifacts to the minimum.
- If you don't work on the code any longer, then download the repository to an
  archive on your local machine, archive it on GitHub or delete from GitHub.

## Development and Support Status

I am developing during my spare time and use this project for learning purposes. Please assume that I will need some
days to answer your questions. At some point I might lose interest in the project. Please keep this in mind when using
this project in a production environment.

## Build, Test, Run

### Prerequisites: Coverlet, ReportGenerator

To compile, test and run this project the latest LTS release of the [.NET SDK](https://dotnet.microsoft.com/download)
is required on your machine.

The acceptance tests in [./src/SaveEnergy.Specs](./src/SaveEnergy.Specs) require coverlet to be installed as global
tool:

```shell
dotnet tool install --global coverlet.console
```

If you would like to build HTML reports from the test coverage, install the
[ReportGenerator](https://reportgenerator.io/) tool:

```shell
dotnet tool install --global dotnet-reportgenerator-globaltool
```

### Build, Test, Create Coverage Report

Run the following commands from the folder containing the `.sln` file in order to build and test.

```sh
# Build the project
dotnet build

# Run the tests once
dotnet test
```

To create a coverage report, run the following command:

```sh
# Cleanup test results from previous runs
rm -r ./src/SaveEnergy.Tests/TestResults ./src/SaveEnergy.Specs/TestResults

# Run the tests and create a coverage report in Cobertura format
dotnet test --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput='./TestResults/coverage.cobertura.xml'

# Convert the Cobertura report to LCOV and HTML
reportgenerator "-reports:src/SaveEnergy.Tests/TestResults/*.xml;src/SaveEnergy.Specs/TestResults/*.xml" "-targetdir:report" "-reporttypes:Html;lcov" "-title:Save Energy"

# Open the HTML report in the browser
open report/index.html
```

### Run the Application

#### Prerequisite: Configure GitHub Client Id

This application uses the GitHub API to retrieve information about repositories. You need to configure this mechanism of authentication as described in the [Authentication Setup](./docs/authentication-setup.md) file.

#### Run the Application

The application must be run from a folder containing the [appsettings.json](./src/SaveEnergy/appsettings.json) file.
You can either run it from the `src/SaveEnergy` folder or from the build output folder.

```shell
cd src/SaveEnergy; \
DOTNET_ENVIRONMENT=Development dotnet run --project SaveEnergy.csproj; \
cd ../..
```

Running from inside the `src/SaveEnergy` directory makes sure that the application reads the configuration file linked
in the project file.

The `DOTNET_ENVIRONMENT=Development` parameter is required for the application to receive the user secrets.

## Relevant GitHub API Endpoints

[./docs/README.md](./docs/README.md) shows how you can run a collection of GitHub API requests relevant to the project.

## Acknowledgements

This project is inspired by Uwe Friedrichsen's talk "[Patterns of Sustainability – Going Green in IT](https://speakerdeck.com/ufried/patterns-of-sustainability-going-green-in-it)" held
at the [OOP Conference in 2023](https://www.oop-konferenz.de/).

Many thanks to [JetBrains](https://www.jetbrains.com/?from=save-energy) who provide an [Open Source License](https://www.jetbrains.com/community/opensource/) for this project ❤️.

### Before Creating a Pull Request

#### Fix Static Code Analysis Warnings

Fix static code analysis warnings reported by [SonarLint](https://www.sonarsource.com/products/sonarlint/).

#### Apply Code Formatting Rules

```shell
# Install https://csharpier.io globally, once
dotnet tool install -g csharpier

# Format code
dotnet csharpier .
```

#### Check Code Metrics

Check code metrics using [metrix++](https://github.com/metrixplusplus/metrixplusplus)

- Configure the location of the cloned metrix++ scripts
  ```shell
  export METRIXPP=/path/to/metrixplusplus
  ```

- Collect metrics
  ```shell
  python "$METRIXPP/metrix++.py" collect --std.code.complexity.cyclomatic --std.code.lines.code --std.code.todo.comments --std.code.maintindex.simple -- .
  ```

- Get an overview
  ```shell
  python "$METRIXPP/metrix++.py" view --db-file=./metrixpp.db
  ```

- Apply thresholds
  ```shell
  python "$METRIXPP/metrix++.py" limit --db-file=./metrixpp.db --max-limit=std.code.complexity:cyclomatic:5 --max-limit=std.code.lines:code:25:function --max-limit=std.code.todo:comments:0 --max-limit=std.code.mi:simple:1
  ```

At the time of writing, I want to stay below the following thresholds:

```text
--max-limit=std.code.complexity:cyclomatic:5
--max-limit=std.code.lines:code:25:function
--max-limit=std.code.todo:comments:0
--max-limit=std.code.mi:simple:1
```

#### Remove Code Duplication Where Appropriate

To detect duplicates I use the [CPD Copy Paste Detector](https://docs.pmd-code.org/latest/pmd_userdocs_cpd.html)
tool from the [PMD Source Code Analyzer Project](https://docs.pmd-code.org/latest/index.html).

If you have installed PMD by download & unzip, replace `pmd` by `./run.sh`.
The [homebrew pmd formula](https://formulae.brew.sh/formula/pmd) makes the `pmd` command globally available.

```sh
# Remove temporary and generated files
# 1. dry run
git clean -ndx
```

```shell
# 2. Remove the files shown by the dry run
git clean -fdx
```

```shell
# Identify duplicated code in files to push to GitHub
pmd cpd --minimum-tokens 50 --language cs --dir .
```

## References

- [GitHub REST API](https://docs.github.com/en/rest)
- [GitHub: Skipping workflow runs](https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-workflow-runs/skipping-workflow-runs)
