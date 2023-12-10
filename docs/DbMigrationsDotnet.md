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

> Below, the `CLI_KEY` is the `cliKey` key associated with a DbContext that you setup in your swigfile (see [Getting Started](./GettingStarted.md) for example config). You can also pass `all` instead of a CLI key to operate on all DbContexts. Or you can omit that parameter and any DbContext will be operated on that has swigfile config that either lacks the `useWhenNoContextSpecified` option or has it to `true`.

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
| `dbCreateRelease [<CLI_KEY>\|all]` | Create EF bundle executables for your DbContext(s). See [Deploying Migrations](#deploying-migrations). |

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
import efConfig from 'swig-cli-modules/ConfigEntityFramework'

efConfig.init(dbMigrationsProjectPath,
  [
    {
      name: 'MainDbContext',
      cliKey: 'main',
      dbSetupType: 'PostgresSetup'
    }
  ],
  {
    dotnetSdkVersion: 6
  }
)

export * from 'swig-cli-modules/EntityFramework'
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

## Deploying Migrations

There are multiple ways to deploy EF migrations. This project facilitates use of EF bundles by providing a wrapper command that handles generating and running the correct command for the DbContexts that are relevant.

This is the swig task to generate an EF bundle that is ready for execution against a production database instance:

```
swig dbCreateRelease
```

You can pass an optional param to specify which DbContext(s) to create bundles for:

- Omit the extra param to create bundles for all DbContext entries in swig config that have `useWhenNoContextSpecified` set to `true`
- Pass `all` to create bundles for all DbContexts
- Pass the `cliKey` (as specified in swig config) for a single DbContext to operate on
- Pass the full class name for a single DbContext to operate on

This task will generate executable(s) in a directory called `release` - the directory will be created if it doesn't exist.

This task will generate and run one `dotnet ef migrations bundle` command per DbContext and "Dotnet Runtime Identifier" (target architecture).

As an example, if you have a `MainDbContext` and a `TestDbContext`, but you only want to create a release for the main context, you could run this (assuming you've set your `cliKey` in swig config to "main"):

```
swig dbCreateRelease main
```

The filename for each executable will be `Migrate<DbContextName>-<RID>.exe`. For example, if you have the 2 DbContexts mentioned, and you don't specify the `releaseRuntimeIds` option to the `init` method in order to stay with the defaults (`'linux-x64'` and `'win-x64'`), the output files will be:

```
MigrateMainDbContext-linux-x64.exe
MigrateMainDbContext-win-x64.exe
MigrateTestDbContext-linux-x64.exe
MigrateTestDbContext-win-x64.exe
```

Important considerations when executing the migration bundles for production:

- When running these files, the architecture must match the runtime id generated for the executable. You can't run a linux exe on windows or the other way around.
- The appropriate environment variables must exist, or there needs to be a `.env` file in the same directory with the appropriate values. These are used to build the connection string. You can optionally pass `--connection` to the bundle executable to override the connection in the DbContext `OnConfiguring` method: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#efbundle
- The database must be accessible from the location the script is run based on the environment variables or `--connection` option used.
- The database (schema) and database user (role) must already exist in the database instance. This project provides database setup functionality for developers, but this is purposely omitted from the deployment scheme for these reasons:
  - To avoid accidental deletion of a production database with the `teardown` command
  - To avoid forcing opinionated setup of database and user
  - In some cases the mechanism and steps for creating a database and user are significantly different than simply running a couple of sql statements, and automating every possible case is out of scope for this project

## Csproj File Notes

Using the swig `dbBootstrapMigrationsProject` command will generate a new project with a csproj file similar to this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="MikeyT.DbMigrations" Version="0.5.0" />
  </ItemGroup>
  <ItemGroup>
    <None Update=".env" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Scripts/**" />
  </ItemGroup>
  <ItemGroup>
    <Folder Include="Migrations/MainDbContextMigrations" />
  </ItemGroup>
</Project>
```

Explanations:

- The reference to `Microsoft.EntityFrameworkCore.Design` is required for `dotnet-ef` to be able to run migration commands in the project. The major version needs to be compatible with the version `MikeyT.DbMigrations` references. This is automatically detected when generating the project. If you update to a different major version of `Microsoft.EntityFrameworkCore.Design`, you may have to add some other dependencies to get it to work (such as `Microsoft.EntityFrameworkCore.Abstractions`).
- The entry for the `.env` is necessary so `dotnet-ef` commands can get the necessary environment variables to build the connection string. This isn't needed for deployment, so it isn't an embedded resource and won't be added to the bundle executable when running `swig dbCreateRelease`. See [Deploying Migrations](#deploying-migrations) for more info on deployment.
- The `EmbeddedResource` reference to `Scripts/**` results in all your sql files in that directory getting included in both the built dll that is run locally as well as the generated deployment bundle. These scripts are accessed in the `MigrationScriptRunner` by accessing embedded resources in the assembly.
