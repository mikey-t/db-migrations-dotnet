# db-migrations-dotnet Documentation

This document contains detailed project documentation. For information on using this process in your own application, take a look at [Getting Started](./GettingStarted.md).

## What is it?

This project's main goals (see additional info here: [Project Goals](./ProjectGoals.md)):

- Manage database migrations using EntityFramework for the migrations while only using raw SQL scripts instead of EF model classes
- Automate the setup/teardown of a database on a dev machine
- Allow easily managing database migrations from a central location
- Allow managing multiple databases simultaneously, which makes it easier to manage a test-specific database that mirrors the primary application database

This repository contains a sub-set of the overall functionality necessary to meet these goals. The components in this library are:

- Documentation for the overall process
- A Nuget package (`MikeyT.DbMigrations`) that contains:
  - Base classes and DB specific implementations for `DbSetup`, `DbSettings` and `IDbSetupContext`
  - A CLI method that can be re-used in a plain C# console app that exposes database setup and teardown functionality
- Example projects that show how to wire everything up

Components outside of this project:

- Npm package `swig-cli-modules`, specifically the `EntityFramework` sub-module:
  - Provides simple CLI wrappers and config for automatically bootstrapping the C# console app and running it's commands
  - CLI wrappers for EntityFramework commands so that we don't have to remember all the different `dotnet-ef` commands and can easily manage multiple DbContext classes simultaneously
- Npm package `@mikeyt23/node-cli-utils`, which provides a lot of miscellaneous functionality to the `swig-cli-modules` EF module functionality

See the [Getting Started](./GettingStarted.md) instructions to see how it all fits together.

## Swig EF Module Task Reference

Once you've setup your project using the [Getting Started](./GettingStarted.md) instructions, you'll have a number of available task commands.

The `swig-cli-modules` module `DockerCompose` will provide these tasks (Docker and a valid `docker-compose.yml` in the root of the project are the pre-requisites):

| swig task | description |
| ------------ | ----------- |
| dockerUp | Runs `docker compose up` using the root `docker-compose.yml` file and appropriate options. You can optionally specify a different location by importing the config singleton from `swig-cli-modules/ConfigDockerCompose` into your swigfile and calling `init` with a different path. |
| dockerUpAttached | Same as `dockerUp` but will stay attached to the docker container so you can see raw logging output from the container. |
| dockerDown | Runs `docker compose down` with appropriate options. |

The `swig-cli-modules` module `EntityFramework` will provide these tasks (all commands except `dbBootstrapMigrationsProject` require a valid DbMigration C# console app to exist at the location specified in swigfile config):

> Below, the `CLI_KEY` is the `cliKey` key associated with a DbContext that you setup in your swigfile (see [Getting Started](./GettingStarted.md) for example config). You can also pass `all` instead of a CLI key to operate on all DbContexts. Or you can omit that parameter and any DbContext with swigfile config of `useWhenNoContextSpecified: true` will be operated on.

| swig task | description |
| ------------ | ----------- |
| `dbShowConfig` | Prints swig EF config from your swigfile. |
| `dbListMigrations [<CLI_KEY>\|all]` | List migrations. |
| `dbAddMigration [<CLI_KEY>\|all] <MIGRATION_NAME>` | Add a new DB migration. |
| `dbRemoveMigration [<CLI_KEY>\|all]` | Remove the last migration, but only if it hasn't been applied. If it has already been applied, first run `swig dbMigrate <MIGRATION_NAME_BEFORE_LAST>` to trigger it's "down" migration and then re-run `swig dbRemoveMigration`. It will delete C# migration files (which only contain boilerplate), but not SQL files, unless they're empty. You will have to delete those manually if you don't plan on re-creating the migration with the same name with the intention of using the same SQL files. |
| `dbMigrate [<CLI_KEY>\|all] [MIGRATION_NAME]` | Run whichever up or down migrations are required to get to the `MIGRATION_NAME` specified, or to latest if not. |
| `dbSetup` | Creates the database user and schema for all DbContexts specified in your swigfile config. Safe to re-run. The database must be running and accessible. |
| `dbTeardown` | Drops the user and schema for each of the DbContexts defined in your swigfile. It will prompt you to confirm for each DbContext. The database must be running and accessible. Note that this will only operate on the database - all C# and SQL files will be left untouched. |
| `dbBootstrapDbContext <FULL_DB_CONTEXT_CLASS_NAME> <DB_SETUP_TYPE_CLASS_NAME>` | Bootstrap a new DbContext class in your DbMigrations console project. Example: `swig dbBootstrapDbContext TestDbContext PostgresSetup`. |
| `dbBootstrapMigrationsProject` | Bootstrap a new console project and set everything up based on swigfile config. Not re-runnable -it will simply exit if the project directory already exists. If you're experimenting with a brand new project, you can delete the project, update your swigfile config and re-run it as many times as needed, but be sure your database is also reset if you've applied migrations and want to start over (delete the docker volume in between calls, for example). |

## Bootstrap a new DbMigrations Project

To bootstrap a new DbMigrations console app, follow the instructions in [Getting Started](./GettingStarted.md). This will lead you to use a swig task from the swig EF module called `dbBootstrapMigrationsProject`. It will default to using dotnet 8, but you can also specify dotnet 6 or 7. The dotnet SDK version you specify must be installed before running the bootstrap command.

## Bootstrap a New DbContext Into an Existing Project

If you bootstrapped your project before adding an entry for DbContext metadata or if you just want to add another DbContext, you just need to do 2 things:

- Add an entry to the swigfile config `init` method with appropriate values
- Run swig task (replace `TestDbContext` and `PostgresSetup` with appropriate values):
  ```
  `swig dbBootstrapDbContext TestDbContext PostgresSetup`
  ```

If the intention for the new DbContext class is to be a duplicate test context, you'll want to catch up the migrations for the new context by running `swig dbAddMigration <new_context_cli_key> <migration_name>` for each of the migrations you want to catch up for, then run `swig dbMigrate <new_context_cli_key>`. You'll also want to ensure your newly generated DbContext uses different environment variables, such as a different database name. In the case of `PostgresSetup`, you can do this by updating the existing context which might look like this:
```csharp
public class TestDbContext : PostgresMigrationsDbContext { }
```
to override the env keys desired:
```csharp
public class TestDbContext : PostgresMigrationsDbContext
{
    public override PostgresSetup GetDbSetup()
    {
        return new PostgresSetup(new PostgresEnvKeys { DbNameKey = "DB_NAME_TEST" });
    }
}
```

## Remove a DbContext Class

You can manually remove a DbContext class with these steps:

- Delete the C# class
- Remove the `DbContextInfo` entry from your swigfile config `init` call
- Delete the Migrations subdirectory created for the new context
- Delete the Scripts subdirectory if you configured it to have one
- Remove relevant folder references in the DbMigrations project `.csproj` file

## Using `db-migrations-dotnet` in Non-Dotnet Projects

This project is geared towards using dotnet and EntityFramework for migrations, but there is no direct connection between the migrations project and the application that requires access to your database. This means you can use this project to manage a database for any type of project, as long as there is an EntityFramework driver for the database type and you have an implementation for the `DbSetup`, `DbSettings` and `IDbSetupContext` C# classes from this project.

## Specify a Different Dotnet Version for Bootstrapped Console App

If you'd like to use a different dotnet version for the generated console app, you can pass additional config to the `init` method in your swigfile, for example, to use dotnet `6` instead of the default `8`:

```typescript
efConfig.init(dbMigrationsProjectPath,
  [
    {
      name: 'MainDbContext',
      cliKey: 'main',
      dbSetupType: 'PostgresSetup',
      useWhenNoContextSpecified: true
    }
  ],
  {
    dotnetSdkVersion: 6
  }
)
```

## DbSettings GetLogSafeConnectionString

The `DbSettings` base class provides a `GetLogSafeConnectionString` method to get a "log safe" version of the connection string for logging purposes. The way this works is that it will look for any instance fields or properties on the implementation with the word "password" in the name (case insensitive) and will replace any instances of that value in the passed `connectionString` param with "*****".

You can alternatively add the C# attribute `[DoNotLog]` to your field or property if you want the value to be redacted when calling `GetLogSafeConnectionString`.

## How is Entity Framework Used?

Entity Framework has DB migrations capability, but most people assume you have to use model classes and opt-in to everything EF related, but that's not actually true. You can very easily run plain raw SQL files. For example, see this simple method in `MigrationScriptRunner`:

```csharp
public static void RunScript(MigrationBuilder migrationBuilder, string relativePath)
{
    var path = Path.Combine(AppContext.BaseDirectory, $"Scripts/{relativePath}");
    var sql = File.ReadAllText(path);
    sql = _replacer.GetSqlWithPlaceholderReplacements(sql);
    migrationBuilder.Sql(sql);
}
```

The automation from this project will automatically populate new C# migration files with something like this:

```csharp
namespace DbMigrations.Migrations.MainDbContextMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Initial.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            MigrationScriptRunner.RunScript(migrationBuilder, "Initial_Down.sql");
        }
    }
}
```

So you won't actually have to go in and edit anything. Instead, you can just modify the also-automatically-generated SQL files:

```
./src/DbMigrations/Scripts/Initial.sql
./src/DbMigrations/Scripts/Initial_Down.sql
```

And now you've magically got an entire DB migrations framework that allows using plain SQL without having to write any code!

This framework is designed with ultimate flexibility in mind. You can create your own `DbSetup`/`DbSettings`/`IDbSetupContext` implementations which can allow you to use your own script runner and your own `ISqlPlaceholderReplacer`.

## Dotnet Tool Usage for `dotnet-ef`

The swig EF module commands all use the dotnet tool `dotnet-ef` under the hood. If you use the `  dbBootstrapMigrationsProject` command to create your project, it will automatically select an appropriate version of the tool and install it as a "local" tool (with a tool manifest at `./.config/dotnet-tools.json`).

If you need to install this separately, you can install it yourself either locally:

```
dotnet tool install dotnet-ef --local
```

or globally:

```
dotnet tool install dotnet-ef --global
```

Feel free to check in the `./.config/dotnet-tools.json` file into source control if you'd like. That way others will be able to get the same version of the tool.

> The tool is intended to be installed as a "local" tool at the root of your project so that swig tasks that also run at the root of your project have it in context. If you install it only in the DbMigrations project, the swig tasks will fail.

## DbSettings Connection Strings

The `DbSettings` base class requires implementing 2 connection string methods:

- `GetMigrationsConnectionString`
- `GetDbSetupConnectionString`

In the case of the `PostgresSettings` class, the only difference is that for the `GetDbSetupConnectionStringImpl` it connects to the "postgres" database instead of the application specific database, like it does for `GetMigrationsConnectionString`. This is because the application specific database won't exist yet. But note that both postgres connection string methods use the root user/password since they both require elevated permissions.

This is a little confusing since PostgresSettings exposes the application specific username/password that are needed during setup, but the base type doesn't have those. This might end up on the base class if it turns out that all database implementations follow the same pattern.

## Postgres Optional Env Var

If you don't want the connection string to include error detail, you can add `POSTGRES_INCLUDE_ERROR_DETAIL=false` to your .env file (in the root and in the database migrations project).
