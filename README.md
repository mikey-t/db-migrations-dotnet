# db-migrations-dotnet

The db-migrations-dotnet project provides a framework to:
- Manage database migrations
- Manage multiple databases with one set of commands (for example, a "main" DB and another "test" DB for integration tests)
- Manage database setup and teardown
- Create deployment executables to migrate live production databases

The primary goal of this project is to allow management of database migrations as ***plain SQL files*** within source control while not reinventing the wheel when it comes to the actual migration management. This is achieved by wrapping Entity Framework's existing functionality while also adding missing pieces and some quality of life improvements.

This framework relies on a task automation library called [swig](https://github.com/mikey-t/swig/). A full set of wrapper tasks for all operations is available for import. More info on swig is provided within the documentation.

## Project Goals

For a detailed list of project goals, see [ProjectGoals](./docs/ProjectGoals.md).

## Documentation

Main documentation: [DbMigrationsDotnet](./docs/DbMigrationsDotnet.md)

Short "release notes style" video clips demoing each common DB migration task: [Demo](./docs/Demo.md)

## Supported Databases

Currently supported databases:

- PostgreSQL
- SQL Server

This project can be extended to support any database that Entity Framework supports. For info on Entity Framework database support, see https://learn.microsoft.com/en-us/ef/core/providers/. For documentation on how this is done, see [Extending db-migrations-dotnet](./docs/DbMigrationsDotnet.md#extending-db-migrations-dotnet-cli-for-other-databases).
