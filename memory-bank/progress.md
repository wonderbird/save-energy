# Progress

## Current Status

The project is currently **blocked**. The CI build on the `main` branch is failing, which prevents any new changes from being validated and merged.

## What Works

-   The application builds successfully.
-   The tests pass locally (as inferred from the CI log, where the failure occurs after the test step).

## What's Left to Build

-   The immediate task is to fix the CI pipeline.
-   Once the build is fixed, we can proceed with the planned product increment.

## Known Issues

-   The `paambaati/codeclimate-action@v9.0.0` in the GitHub Actions workflow is failing due to a `404` error when attempting to download the test reporter.
