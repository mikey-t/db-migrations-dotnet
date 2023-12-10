# MikeyT.DbMigrations Example Solutions

Each example solution shows how a project can be configured to allow easy setup and teardown of a containerized database on a developer machine and how to utilize built-in EntityFramework database migrations with a custom thin CLI wrapper for quality of life improvements.

Functionality from the following packages are demonstrated:

- Nuget package [MikeyT.DbMigrations](https://github.com/mikey-t/db-migrations-dotnet) (database setup/teardown functionality)
- Npm package [swig-cli](https://github.com/mikey-t/swig) (for orchestrating dev commands)
- Npm package [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) (EntityFramework and DockerCompose modules)
- Npm package [@mikeyt23/node-cli-utils](https://github.com/mikey-t/node-cli-utils) (collection of misc utility functions)

For more documentation, head back to the [main project readme](../README.md).

## Initial setup

For each example project, these are the general initial setup steps:

- Ensure .NET 6 SDK is installed
- Ensure Node.js is installed (>= 18)
- Ensure Docker is installed and running
- Install swig globally if you haven't already: `npm i -g swig-cli@latest`
- Install npm dependencies: `npm install`
- Optional: copy `.env.template` to `.env` and modify values
  - Make sure you don't have another database running on the same port - update the `.env` if needed
- Startup the database container: `swig dockerUp`
- Ensure the database has it's application user and schema created: `swig dbSetup`
- Ensure database is up to date by running migrations: `swig dbMigrate`
- Start the example .NET WebApi project: `swig run`
- Browse to the running service's swagger API (the root URL is configured to serve this)

## Switching Between Swig EF Config Examples

You can comment and uncomment the different `efConfig.init` in `swigfile.ts` to try out different options. When doing this, you'll need to keep a few things in mind:

- Switching to or from the example where the DbContexts each use a different subdirectory will require you to:
  - Update each migration C# file under `Migrations/MainDbContextMigrations` so that the relative path now points to the correct location
  - If moving to config with subdirectories, create a `src/DbMigrations/Scripts/Main` and move scripts into it, or if going back to non-subdirectories, move scripts out of that `Main` directory back into the `Scripts` directory `Main` directory
- If switching to the multiple DbContext example:
  - Bootstrap the new C# DbContext class by running: `swig dbBootstrapDbContext TestDbContext PostgresSetup`
  - Update `src/DbMigrations/TestDbContext.cs` so it uses a different database name:
    ```
    public class TestDbContext : PostgresMigrationsDbContext
    {
        public override PostgresSetup GetDbSetup()
        {
            return new PostgresSetup(new PostgresEnvKeys { DbNameKey = "DB_NAME_TEST" });
        }
    }
    ```
  - Bring the new TestDbContext in sync with the MainDbContext by running `swig dbAddMigration test Initial` and `swig dbAddMigration test AddPerson` and then adding the example sql from the sql files in the root to the newly generated script files in the DbMigrations project

## Other Things To Try

### Try listing swig tasks

Get a list of all the available swig commands by simply running `swig` in a shell in an example project's root directory.

### Try other DB swig commands

Try experimenting with the other available `swig` commands that wrap EF migration commands:

```
dbListMigrations
dbAddMigration
dbRemoveMigration
dbMigrate
```

### Try bootstrapping a new DbContext

In the `example-postgres` project, try bootstrapping a new DbContext by running something like this:

```
swig dbBootstrapDbContext TestDbContext PostgresSetup
```

and then update the generated file `TestDbContext.cs` so it uses a different database name but all the other same env values. You can do this by overriding the `GetDbSetup` method and returning a newly instantiated PostgresSetup object with a different env key for the database name (any env key not specified will use it's default):

```csharp
using MikeyT.DbMigrations;

namespace DbMigrations;

public class TestDbContext : PostgresMigrationsDbContext
{
    public override PostgresSetup GetDbSetup()
    {
        return new PostgresSetup(new PostgresEnvKeys { DbNameKey = "DB_NAME_TEST" });
    }
}
```

Note that the project's `.env.template` already has an entry for `DB_NAME_TEST`, which is why that will work.

Then add the context info to your swigfile init call so it looks something like this:

```typescript
efConfig.init(
  dbMigrationsPath,
  [
    {
      name: 'MainDbContext',
      cliKey: 'main',
      dbSetupType: 'PostgresSetup',
      useWhenNoContextSpecified: true
    },
    {
      name: 'TestDbContext',
      cliKey: 'test',
      dbSetupType: 'PostgresSetup',
      useWhenNoContextSpecified: true
    }
  ]
)
```

Then add the same migrations to the new TestDbContext that the MainDbContext has by running each of these commands:

```
swig dbAddMigration test Initial
swig dbAddMigration test AddPerson
swig dbMigrate
```

This would allow you to have a unit test project that points to a test version of the database. This is very useful for allowing you to perform destructive operations on your database during integration-style tests that access a real database without affecting your main database's data. This strategy will keep any data you've seeded or manually accumulated on your development machine from getting destroyed, while still allowing you to setup and configure the database in any way necessary during unit tests, so that you can test many types of edge cases.

### Try bootstrapping a whole new DbMigrations project

In a new .NET project without database migrations, try bootstrapping a DB migrations project using the `dbBootstrapMigrationsProject` swig task - see [GettingStarted](./GettingStarted.md) for details.

## How To Create a New Example Solution

Pre-requisite C# class implementations for the database engine type (these can be added to or referenced in the DbMigrations project after it is bootstrapped, or added to the MikeyT.DbMigrations package itself):

- `MikeyT.DbSetup`
- `MikeyT.DbSettings`
- `MikeyT.IDbSetupContext`

For example implementations of these classes, see the PostgreSQL versions under the directory [../src/MikeyT.DbMigrations/Implementations/Postgres/](../src/MikeyT.DbMigrations/Implementations/Postgres/).

ExampleApi changes:
- Add entry to `DbType` enum
- Add connection string logic in ConnectionStringProvider
- Add implementation of `IPersonRepository` and wire up in `ExampleApiService`

New example solution setup steps:

- Create a new directory under `example-solutions`
- Create a src dir for an API project and the DbMigrations project: `mkdir src`
- Create a dotnet solution file from that new solution's root directory: `dotnet new sln`
- Follow all the [GettingStarted](./GettingStarted.md) instructions for setting up the root solution directory
- Replace the `docker-compose.yml` with a version appropriate for your new DB engine
- Copy the contents of the `example-postgres` solution's `swigfile.ts` into the new solution directory and replace the `dbSetupType` value in your config for the EF swig module in your - replace `PostgresSetup` with your new implementation class name
- Create a `.env.template` file in the root of the new solution that has the environment values necessary for your `DbSettings` implementation
- Create an empty dotnet web project: `dotnet new web -o ./src/ExampleApiWrapper`
- Add the API wrapper project to the solution: `dotnet sln add ./src/ExampleApiWrapper`
- Navigate to the API wrapper project and add a reference back to the main example API: `dotnet add reference ..\..\..\..\src\ExampleApi`
- Replace the API project's `Program.cs` (replace the `DbType` param with your database type):
  ```csharp
  using ExampleApi;

  ExampleApiService.Run(args, DbType.Postgres);
  ```
- Bootstrap the DbMigrations project: `swig bootstrapMigrationsProject`
- Setup the DB: `swig dbSetup`
- Add an empty initial migration: `swig dbAddMigration Initial`
- Create "up" and "down" sql file in the root of the solution that can be used to copy into the auto generated migration files - must match the Person object in the example API project:
  ```csharp
  public class Person
  {
      public int Id { get; set; }
      public string FirstName { get; set; } = string.Empty;
      public string LastName { get; set; } = string.Empty;
  }
  ```
- Create the migration for the Person object: `swig dbAddMigration AddPerson`
- Copy the sql into the auto-generated sql scripts in the DbMigrations project
- Migrate the DB: `swig dbMigrate`

Verify setup:

- `swig run`
- Try "put" and "get all" endpoints at `http://localhost:7070` (or whatever port was configured in the .env)
