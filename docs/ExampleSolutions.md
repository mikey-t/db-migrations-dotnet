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
- Install npm dependencies: `npm install`
- Ensure you have the latest `dotnet-ef` tool installed: `swig installOrUpdateDotnetEfTool`
- Optional: copy `.env.template` to `.env` and modify values
- Startup the database container: `swig dockerUp`
- Ensure database is up to date by running migrations: `swig dbMigrate`
- Start the example .NET WebApi project: `swig run`
- Browse to the running service's swagger API

## Other Things To Try

### List Swig Tasks

Get a list of all the available swig commands by simply running `swig` in a shell in an example project's root directory.

### Try Other DB Swig Commands

Try experimenting with the other available `swig` commands that wrap EF migration commands:

```
dbListMigrations
dbAddMigration
dbRemoveMigration
dbMigrate
```

---

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
        return new PostgresSetup(new PostgresEnvKeys { DbName = "DB_NAME_TEST" });
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

---

In a new .NET project without database migrations, try bootstrapping a DB migrations project using the `dbBootstrapMigrationsProject` swig task - see [./NewProjectSetup.md](./NewProjectSetup.md) for details.
