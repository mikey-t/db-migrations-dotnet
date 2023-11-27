# db-migrations-dotnet

The db-migrations-dotnet project describes a process for managing database migrations in a dotnet project using EntityFramework with plain SQL files.

In addition to documenting the process, this project provides a set of scripting tools and examples to streamline the process in addition to providing database setup and teardown automation.

## Project Goals

For a detailed list of project goals, see [ProjectGoals.md](./docs/ProjectGoals.md).

## Documentation

For getting started, see [Getting Started](./docs/GettingStarted.md).

For detailed documentation, see [DbMigrationsDotnet](./docs/DbMigrationsDotnet.md).

## Supported Databases

PostgreSQL is currently the only supported database type, with SQL Server support being added soon.

## Roadmap

- Deployment scripting. Need a light wrapper around the `dotnet ef migrations bundle` command. See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#bundles.
- Add SQL Server support. Create DbSetup, DbSettings and base DbContext class implementation. Add example solution.
- Additional unit testing. Coverlet is wired up and working - just need to spend the time to continue increasing unit test coverage.
- `DbSetupCli` integration testing. For each database type, spin up a docker container with fresh DB and run tests to exercise `setup`, `teardown`, `list` and `bootstrap` commands.
