# db-migrations-dotnet

The db-migrations-dotnet project provides a process for managing database migrations in any type of project using a simple C# console app and a thin wrapper around Microsoft's Entity Framework and plain SQL files.

In addition to documenting the process, this project provides a set of scripting tools and examples to streamline the process.

## Project Goals

For a detailed list of project goals, see [./docs/ProjectGoals.md](./docs/ProjectGoals.md).

## Documentation

Instructions for getting started: [./docs/GettingStarted.md](./docs/GettingStarted.md).

Main documentation: [./docs/DbMigrationsDotnet.md](./docs/DbMigrationsDotnet.md).

Short video clips demoing each common DB migration task: [./docs/Demo.md](./docs/Demo.md).

## Supported Databases

Currently supported databases:

- PostgreSQL
- SQL Server

This project can be extended to support any database that Entity Framework supports. For info on Entity Framework database support, see https://learn.microsoft.com/en-us/ef/core/providers/.

## Roadmap

- Additional unit testing. Coverlet is wired up and working and ready for more tests.
- `DbSetupCli` integration testing. For each database type, spin up a docker container with fresh DB and run tests to exercise `setup`, `teardown`, `list` and `bootstrap` commands.
- Add support for additional database types
- Additional diagrams and documentation showing how each component of this architecture is composed together:
  - [swig-cli](https://github.com/mikey-t/swig)
  - [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules)
  - [node-cli-utils](https://github.com/mikey-t/node-cli-utils)
  - db-migrations-dotnet (you are here)
- Create plan for how to support alternative methods of injecting connection string information into DbContext classes (instead of relying solely on `.env` files)
