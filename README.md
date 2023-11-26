# db-migrations-dotnet

The db-migrations-dotnet project describes a process for managing database migrations in a dotnet project using EntityFramework with plain SQL files.

In addition to documenting the process, this project provides a set of scripting tools and examples to streamline the process and provide missing setup/teardown functionality.

This repository contains:

- High level process documentation
- Nuget package to be imported as a dependency in a C# console project: [MikeyT.DbMigrations](https://www.nuget.org/packages/MikeyT.DbMigrations/)
- Example solutions for several database types

Dependent external projects:
- NodeJS scripting via [swig-cli](https://github.com/mikey-t/swig) and [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules)

## Project Goals

For a detailed list of project goals, see [ProjectGoals.md](./docs/ProjectGoals.md).

## Documentation

For getting started, see [GettingStarted.md](./docs/GettingStarted.md).

For detailed documentation, see [DbMigrationsDotnetDocumentation.md](./docs/DbMigrationsDotnetDocumentation.md).

## Supported Databases

PostgreSQL is currently the only supported database type, with SQL Server support being added soon.

## Roadmap

- Deployment scripting. Need a light wrapper around the `dotnet ef migrations bundle` command. See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#bundles.
- Add SQL Server support. Create DbSetup, DbSettings and base DbContext class implementation. Add example solution.
- Additional unit testing. Coverlet is wired up and working - just need to spend the time to continue increasing unit test coverage.
- `DbSetupCli` integration testing. For each database type, spin up a docker container with fresh DB and run tests to exercise `setup`, `teardown`, `list` and `bootstrap` commands.
