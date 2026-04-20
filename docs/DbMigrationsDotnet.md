# db-migrations-dotnet<!-- omit in toc -->

- [Introduction](#introduction)
- [Operational Documentation](#operational-documentation)
  - [Getting Started](#getting-started)
    - [Pre-requisites](#pre-requisites)
    - [Getting Started Overview](#getting-started-overview)
    - [Swig Setup](#swig-setup)
    - [Create .env File](#create-env-file)
    - [Setup Swigfile](#setup-swigfile)
    - [Setup and Start Docker](#setup-and-start-docker)
    - [Bootstrap Your new DbMigrations Project](#bootstrap-your-new-dbmigrations-project)
    - [Run DB Migration Commands with Swig Tasks](#run-db-migration-commands-with-swig-tasks)
    - [Update Git Ignore](#update-git-ignore)
    - [Getting Started Conclusion](#getting-started-conclusion)
  - [Swig EntityFramework Module Task Reference](#swig-entityframework-module-task-reference)
  - [Swigfile Config Reference for EntityFramework Swig Module](#swigfile-config-reference-for-entityframework-swig-module)
  - [Bootstrap a new DbMigrations Project](#bootstrap-a-new-dbmigrations-project)
  - [Bootstrap a New DbContext Into an Existing Project](#bootstrap-a-new-dbcontext-into-an-existing-project)
  - [Remove a DbContext Class](#remove-a-dbcontext-class)
  - [Using db-migrations-dotnet in Non-Dotnet Projects](#using-db-migrations-dotnet-in-non-dotnet-projects)
  - [DbSettings GetLogSafeConnectionString](#dbsettings-getlogsafeconnectionstring)
  - [Deploying Migrations](#deploying-migrations)
  - [Extending db-migrations-dotnet CLI For Other Databases](#extending-db-migrations-dotnet-cli-for-other-databases)
    - [DbSettings Connection Strings](#dbsettings-connection-strings)
  - [Postgres Optional Env Var](#postgres-optional-env-var)
- [Informational Documentation](#informational-documentation)
  - [Entity Framework Utilization](#entity-framework-utilization)
  - [Swig Wrapper Task Explanation](#swig-wrapper-task-explanation)
    - [Swig Wrapper Tasks for MikeyT.DbMigrations CLI Commands](#swig-wrapper-tasks-for-mikeytdbmigrations-cli-commands)
    - [Swig Wrapper Tasks for Direct Entity Framework (EF) Commands](#swig-wrapper-tasks-for-direct-entity-framework-ef-commands)
    - [Swig Utility Tasks](#swig-utility-tasks)
    - [Swig Task Source Reference](#swig-task-source-reference)
  - [Entity Framework CLI Reference](#entity-framework-cli-reference)
  - [Dotnet Entity Framework CLI Tool (dotnet-ef) Installation and Usage](#dotnet-entity-framework-cli-tool-dotnet-ef-installation-and-usage)
  - [Csproj File Notes](#csproj-file-notes)


# Introduction

ℹ️ Project goals:

- Manage all database related operations using short intuitive terminal commands from a central location 
- Manage databases for any type of project
- Automate as much as possible
- Utilize plain SQL files for Up and Down migration operations instead of a framework-specific DSL
  - Selectively utilize Microsoft's [Entity Framework](https://learn.microsoft.com/en-us/ef/core/) (EF) as much as possible while avoiding unwanted functionality:
    - ✅ DO use EF migration operations
    - ✅ DO use EF for DbContext definition and connection string functionality
    - ✅ DO use EF bundle generation
    - ✅ DO use EF's `MigrationBuilder` `Sql` method to execute sql files for Up/Down operations
    - 🚫 DON'T use EF model classes
- Automate the setup/teardown of databases on dev machines
- Manage multiple databases simultaneously, simplifying the use of a test database that mirrors the primary application database
- Generate deployment executables for live production database updates

Using provided automation commands, a C# project is generated to house some automatically generated support files, including DbContext definitions and "Up"/"Down" sql files for each migration that is created, similar to other database migration frameworks. Entity Framework is utilized under the hood to execute the Up/Down operations - see [Entity Framework Utilization](#entity-framework-utilization).

For more depth on project goals, see [Project Goals](./ProjectGoals.md).

To get started setting up a new project, see [Getting Started](#getting-started).

For "release notes style" demo gifs, see [Demo](./Demo.md).

Functionality is divided into multiple components:

| Component | Repository | Description |
| --------- | ---------- | ----------- |
| Nuget package `MikeyT.DbMigrations` | [db-migrations-dotnet](https://github.com/mikey-t/db-migrations-dotnet) | A basic CLI to provide functionality that EF lacks, such as database setup/teardown functionality and easier management of multiple DB contexts. Package contains base classes and DB specific implementations for `DbSetup`, `DbSettings` and `IDbSetupContext`. |
| Npm package `swig` | [swig](https://github.com/mikey-t/swig) | A task orchestration library that, together with node-cli-utils, enables spawning processes and organizing of tasks using simple exported TypeScript or JavaScript functions. Pre-built wrapper tasks are provided via the swig-cli-modules package (see below). |
| Npm package `swig-cli-modules`  | [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) | Provides wrapper commands for all necessary docker and EF commands that can be imported. For more info, see sections [Swig EntityFramework Module Task Reference](#swig-entityframework-module-task-reference) and [Swig Wrapper Task Explanation](#swig-wrapper-task-explanation). |
| Npm package `@mikeyt23/node-cli-utils` | [node-cli-utils](https://github.com/mikey-t/node-cli-utils) | This package contains many helpful Node utility functions (e.g. process spawning and .env file management). |

# Operational Documentation

## Getting Started

### Pre-requisites

Operating System: Windows, Mac or Linux

Before getting started, ensure you have the following:

- NodeJS >= 24
- Docker
- Dotnet SDK 10

Other suggested tools:

- [mise](https://mise.jdx.dev/) for managing NodeJS and pnpm versions (for a windows setup guide, see [mise-windows-guide](https://github.com/mikey-t/mise-windows-guide))
- [pnpm](https://pnpm.io/installation) instead of npm for NodeJS dependency management

### Getting Started Overview

Overview of the steps you'll complete in upcoming sections:

- Setup your project to be able to use the task orchestration library `swig`
- Create a .env file
- Update swigfile with config and re-export useful tasks
- Setup docker and start a container (examples will use PostgreSQL)
- Run a swig command to generate a new C# project that will house migrations
- Run swig commands for database migration operations, as needed: `dbAddMigration`, `dbRemoveMigration`, `dbMigrate`, `dbListMigrations`

### Swig Setup

[Swig](https://github.com/mikey-t/swig) is a task orchestration library. Combined with npm packages [@mikeyt23/node-cli-utils](https://github.com/mikey-t/node-cli-utils) and [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules), we will have access to full set of commands we can run in a terminal at the root of our project that will facilitate bootstrapping a database migrations project as well as all the other commands necessary to manage everything.

There are several ways to setup and use swig (see [swig repo](https://github.com/mikey-t/swig)). For this guide, we will install swig globally to avoid the need to type a prefix before swig commands, and we'll set it up using an initialization script that will give us Typescript support out of the box (for more info on this init script, see [swig-cli-init](https://github.com/mikey-t/swig-cli-init)).

If you use mise to manage NodeJS versions, it's recommended to also use it to install swig globally using it's "npm backend" ([mise-windows-guide#install-global-npm-packages](https://github.com/mikey-t/mise-windows-guide#install-global-npm-packages)):

```bash
mise use -g npm:swig-cli@latest
```

> ℹ️ If you don't want to use mise, this is the equivalent pnpm command: `pnpm i -g swig-cli@latest`

To bootstrap a swig project in the current directory, run:
```bash
pnpx swig-cli-init@latest
```

Verify swig is working by running `swig`, which should list available tasks (if you ran the init script, there is a sample task called `hello` that you can execute by running `swig hello`).

### Create .env File

> ❗Note that both the root of the project and the generated DB migrations project will need the same `.env` values. The swigfile setup instructions provide a simple helper task `syncEnvFiles` (see swigfile setup section) to sync changes between projects. There is a roadmap item to research alternatives, but for now this is required to allow a root docker `compose.yaml` file and operations in the DB migrations project from requiring workarounds for gathering `.env` values.

Create a `.env` file at the root of your project. For these getting started instructions, we'll be using a PostgreSQL database.

```ini
DB_HOST=localhost
DB_PORT=5432
DB_USER=dbmigrationsexample
DB_NAME=dbmigrationsexample
DB_PASSWORD=Abc1234!
DB_ROOT_USER=postgres
DB_ROOT_PASSWORD=Abc1234!
```

### Setup Swigfile

If you used the `swig-cli-init` script, your swigfile will be the typescript file `swigfile.ts` in the directory you ran the script.

Swigfile setup description:

- Import config singleton for the swig-cli-modules EntityFramework module
- Call efConfig `init` method with our desired config options
  - Specify migrations project path
  - Specify DbContext metadata
  - Specify dotnet version
- Re-export swig tasks from EntityFramework and DockerCompose swig-cli-modules so they're available to execute via terminal
- (optional but recommended) Import helper method to sync .env values between root and DB migrations project path, along with exported function that utilizes helper. This requires adding dev dependency `@mikeyt23/node-cli-utils`, which will have already been added if you used the `swig-cli-init` script above.

Example swigfile after making changes:

```Typescript
import { overwriteEnvFile } from '@mikeyt23/node-cli-utils'
import path from 'node:path'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'

const dbMigrationsProjectPath = 'src/DbMigrations'

efConfig.init(
  dbMigrationsProjectPath,
  [
    {
      name: 'MainDbContext',
      cliKey: 'main',
      dbSetupType: 'PostgresSetup'
    }
  ],
  { dotnetSdkVersion: 10 }
)

export * from 'swig-cli-modules/EntityFramework'
export * from 'swig-cli-modules/DockerCompose'

export async function syncEnvFiles() {
  await overwriteEnvFile('.env', path.join(dbMigrationsProjectPath, '.env'))
}

```

Now when you run `swig`, you should see a new list of available commands, for example:

```
[ Command: list ][ Swigfile: swigfile.ts ][ Version: 1.0.5 ]
Available tasks:
  dbAddMigration
  dbBootstrapDbContext
  dbBootstrapMigrationsProject
  dbCreateRelease
  dbListMigrations
  dbMigrate
  dbRemoveMigration
  dbSetup
  dbShowConfig
  dbTeardown
  dockerBash
  dockerDown
  dockerUp
  dockerUpAttached
  syncEnvFiles
[ Result: success ][ Total duration: 40 ms ]
```

### Setup and Start Docker

> ℹ️ If you already have something listening on port 5432, you can change the `.env` value for the key `DB_PORT` in the instructions below. If you choose a port that is already in use, you'll get an error like this when you run `swig dockerUp`: ⚠️ "Error response from daemon: driver failed programming external connectivity on endpoint". Simply change the `DB_PORT` in your `.env` (both root and `./src/DbMigrations/.env`) and re-run `swig dockerUp`.

Next we need a database to operate on. We are going to use PostgreSQL running in a docker container:

- Create a file at the root of your project called `compose.yaml` with the following content:
  ```yaml
  services:
    postgresql:
      image: postgres:18.3
      volumes:
        - postgresql_data:/var/lib/postgresql
      environment:
        POSTGRES_USER: "${DB_ROOT_USER:?}"
        POSTGRES_PASSWORD: "${DB_ROOT_PASSWORD:?}"
      ports:
        - "${DB_PORT:-5432}:5432"

  volumes:
    postgresql_data:

  ```
- Run `swig dockerUp`

You should now have a PostgreSQL database running in a docker container! You can verify using a tool like [pgAdmin](https://www.pgadmin.org/download/) or the VSCode extension [PostgreSQL](https://marketplace.visualstudio.com/items?itemName=ckolkman.vscode-postgres). Be sure to use the values from your `.env` to connect (`DB_ROOT_USER` and `DB_ROOT_PASSWORD`). Note that the application specific user hasn't been setup yet - it will happen in another step.

### Bootstrap Your new DbMigrations Project

- Bootstrap your new DbMigrations project by running:
  ```
  swig dbBootstrapMigrationsProject
  ```
- Copy the project root `.env` file you created earlier to the DbMigrations project at `./src/DbMigrations/.env`. You can copy it manually or run the helper task we setup earlier: `swig syncEnvFiles`.

Assuming you used the example swigfile content from above, this bootstrap command will create a new C# console project at `./src/DbMigrations/` with our new `MainDbContext`.

### Run DB Migration Commands with Swig Tasks

Now that we have a database migrations project and a running database, we are going to:

- Setup the database (create a role and schema)
- Create an initial empty migration called "Initial"

Make sure you copied the `.env` file into the new DbMigrations project directory before continuing.

First run:

```
swig dbSetup
```

After the application database schema and user have been setup, the output should finish with "✅ setup complete".

Create an initial empty migration called "Initial":

```
swig dbAddMigration Initial
```

This will essentially run the dotnet-ef command `dotnet ef migrations add Initial` (it also adds the context name and project path params for you).

Now if we run `swig dbListMigrations`, we can see output like the following that will tell us there is a pending migration:

```
20231126221937_Initial (Pending)
```

If this was a normal migration, we'd go update the automatically generated sql files for the up and down operations:

```
- 📄src\DbMigrations\Scripts\Initial.sql
- 📄src\DbMigrations\Scripts\Initial_Down.sql
```

But in this case we want an empty migration, so without adding SQL to those files, we are going to apply the migration to the database with this command:

```
swig dbMigrate
```

### Update Git Ignore

Be sure to add or update your `.gitignore` file to account for new files added to the project:
  ```
  .env
  node_modules
  src/DbMigrations/bin
  src/DbMigrations/obj
  ```

### Getting Started Conclusion

After running through these instructions you should have a basic project setup and ready to go, as well as have a general idea of how this migrations framework is structured.

## Swig EntityFramework Module Task Reference

Once you've setup your project using the [Getting Started](#getting-started) instructions, you'll have a number of available swig tasks.

The `swig-cli-modules` module `DockerCompose` will provide the following tasks (requires a valid docker `compose.yaml` file in the root of your project):

| swig task | description |
| ------------ | ----------- |
| dockerUp | Runs `docker compose up` using the root docker `compose.yaml` file and appropriate options. You can optionally specify a different location by importing the config singleton from `swig-cli-modules/ConfigDockerCompose` into your swigfile and calling `init` with a different path. |
| dockerUpAttached | Same as `dockerUp` but will stay attached to the docker container so you can see raw logging output from the container. |
| dockerDown | Runs `docker compose down` with appropriate options. |

The `swig-cli-modules` module `EntityFramework` will provide these tasks (all commands except `dbBootstrapMigrationsProject` require a valid DbMigration C# console app to exist at the location specified in swigfile config):

> Below, the `CLI_KEY` is the swigfile `cliKey` property on the DbContext config object that you setup in your swigfile (see [Getting Started](#getting-started) for example config). You can also pass `all` instead of a CLI key to operate on all DbContexts. Or you can omit that parameter and any DbContext will be operated on that has swigfile config that either lacks the `useWhenNoContextSpecified` option or has it to `true`.

| swig task | description |
| ------------ | ----------- |
| `dbShowConfig` | Prints swig EF config from your swigfile. |
| `dbListMigrations [<CLI_KEY>\|all]` | List migrations. |
| `dbAddMigration [<CLI_KEY>\|all] <MIGRATION_NAME>` | Add a new DB migration. |
| `dbRemoveMigration [<CLI_KEY>\|all]` | Remove the last migration, but only if it hasn't been applied. If it has already been applied, first run `swig dbMigrate <MIGRATION_NAME_BEFORE_LAST>` to trigger it's "down" migration and then re-run `swig dbRemoveMigration`. It will delete C# migration files (which only contain boilerplate), but not SQL files, unless they're empty. You will have to delete those manually if you don't plan on re-creating the migration with the same name with the intention of using the same SQL files. |
| `dbMigrate [<CLI_KEY>\|all] [MIGRATION_NAME]` | Run whichever up or down migrations are required to get to the `MIGRATION_NAME` specified, or to latest if not. |
| `dbSetup` | Creates the database user and schema for all DbContexts specified in your swigfile config. Safe to re-run. The database must be running and accessible. |
| `dbTeardown` | Drops the user and schema for each of the DbContexts defined in your swigfile. It will prompt you to confirm for each DbContext. The database must be running and accessible. Note that this will only operate on the database - all C# and SQL files will be left untouched. |
| `dbBootstrapDbContext <FULL_DB_CONTEXT_CLASS_NAME> <DB_SETUP_TYPE_CLASS_NAME>` | Bootstrap a new DbContext class in your DbMigrations console project. Example: `swig dbBootstrapDbContext TestDbContext PostgresSetup`. Class must exist, inherit from [DbSetup](../SRC/MikeyT.DbMigrations/Core/DbSetup.cs) and either already be in [db-migrations-dotnet/src/MikeyT.DbMigrations/Implementations/](../src/MikeyT.DbMigrations/Implementations/) or be defined in your own C# project that is local or referenced by the local project. |
| `dbBootstrapMigrationsProject` | Bootstrap a new console project and set everything up based on swigfile config. Not re-runnable -it will simply exit if the project directory already exists. If you're experimenting with a brand new project, you can delete the project, update your swigfile config and re-run it as many times as needed, but be sure your database is also reset if you've applied migrations and want to start over (delete the docker volume in between calls, for example). |
| `dbCreateRelease [<CLI_KEY>\|all]` | Create EF bundle executables for your DbContext(s). See [Deploying Migrations](#deploying-migrations). |

## Swigfile Config Reference for EntityFramework Swig Module

See [Getting Started](#getting-started) instructions for setup steps.

For a guaranteed up-to-date config reference, refer directly to [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) typescript file [EntityFrameworkConfig.ts](https://github.com/mikey-t/swig-cli-modules/blob/main/src/config/EntityFrameworkConfig.ts). Note that you can lean on your IDEs intellisense and the JSDoc on the `init` method when making changes to your config.

Here is a copy of the `init` method signature, for convenience (see above link for more detail):
```Typescript
EntityFrameworkConfig.init(dbMigrationsProjectPath: string, dbContexts: DbContextConfig[], options?: Partial<EntityFrameworkConfigOptions>)
```

And this is a partial copy of the `EntityFrameworkConfigOptions` type with most important values:
```Typescript
export interface EntityFrameworkConfigOptions {
  /** Defaults to 10. */
  dotnetSdkVersion: SupportedDotnetSdkVersion

  /** Defaults to ['linux-x64', 'win-x64'] if not specified. */
  releaseRuntimeIds: DotnetRuntimeIdentifier[]
}
```

These are the primary purposes for the config values that are passed to `init`:
- Specify path for migrations project in first `dbMigrationsProjectPath` param (for example, `src/DbMigrations`)
- Specify a list of DbContext config objects as second `dbContexts` param, each of which mainly needs:
  - `name`
  - `cliKey`
  - `dbSetupType`
- Specify dotnet SDK version via options object `dotnetSdkVersion` property
- Specify platforms to build deployment bundles for via options object `releaseRuntimeIds`

For example swigfile Typescript config including import of the config singleton you need to call `init` on, see getting started doc at section [Setup Swigfile](#setup-swigfile).

For alternate config examples, see example example-postgres project's [swigfile.ts](../example-solutions/example-postgres/swigfile.ts).

## Bootstrap a new DbMigrations Project

To bootstrap a new DbMigrations console app, follow the instructions in [Getting Started](#getting-started). This will lead you to use a swig task from the swig EF module called `dbBootstrapMigrationsProject`. It will default to using dotnet 10, but you can also specify dotnet 6, 7 or 8 (note that 9 is not currently supported). The dotnet SDK version you specify must be installed before running the bootstrap command.

## Bootstrap a New DbContext Into an Existing Project

If you bootstrapped your project before adding an entry for DbContext metadata or if you just want to add another DbContext, you just need to do 2 things:

- Add an entry to the swigfile config `init` method with appropriate values
- Run swig task (replace `TestDbContext` and `PostgresSetup` with appropriate values):
  ```bash
  swig dbBootstrapDbContext TestDbContext PostgresSetup
  ```

If the intention for the new DbContext class is to be a test/mirror context (for integration tests), you'll want to catch up the migrations for the new context by running `swig dbAddMigration <new_context_cli_key> <migration_name>` for each of the migrations you want to catch up for, then run `swig dbMigrate <new_context_cli_key>`. You'll also want to ensure your newly generated DbContext uses different environment variables, such as a different database name. In the case of `PostgresSetup`, you can do this by updating the existing context which might look like this:
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

## Using db-migrations-dotnet in Non-Dotnet Projects

This project is geared towards using dotnet and EntityFramework for migrations, but there is no direct connection between the migrations project and the application that requires access to your database. This means you can use this project to manage a database for any type of project, as long as there is an EntityFramework provider for the database type and you have an implementation for the `DbSetup`, `DbSettings` and `IDbSetupContext` C# classes from this project.

## DbSettings GetLogSafeConnectionString

The `DbSettings` base class provides a `GetLogSafeConnectionString` method to get a "log safe" version of the connection string for logging purposes. The way this works is that it will look for any instance fields or properties on the implementation with the word "password" in the name (case insensitive) and will replace any instances of that value in the passed `connectionString` param with "*****".

You can alternatively add the C# attribute `[DoNotLog]` to your field or property if you want the value to be redacted when calling `GetLogSafeConnectionString`.

## Deploying Migrations

There are multiple ways to deploy EF migrations. This project facilitates use of EF bundles by providing a wrapper command that handles generating and running the correct command for the DbContexts that are relevant.

This is the swig task to generate an EF bundle that is ready for execution against a production database instance:

```bash
swig dbCreateRelease
```

You can pass an optional param to specify which DbContexts to create bundles for:

- Omit the extra param to create bundles for all DbContext entries in swig config that have `useWhenNoContextSpecified` set to `true`
- Pass `all` to create bundles for all DbContexts
- Pass the `cliKey` (as specified in swig config) for a single DbContext to operate on
- Pass the full class name for a single DbContext to operate on

This task will generate executable(s) in a directory called `release` - the directory will be created if it doesn't exist.

This task will generate and run one bundle command (`dotnet ef migrations bundle`) per DbContext and "Dotnet Runtime Identifier" (target architecture).

As an example, if you have a `MainDbContext` and a `TestDbContext`, but you only want to create a release for the main context, you could run this (assuming you've set your `cliKey` in swig config to "main"):

```bash
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
- The database (schema) and database user (role) must already exist in the database instance. This project (db-migrations-dotnet) provides database setup functionality for developers, but this is purposely omitted from the deployment scheme for these reasons:
  - To avoid accidental deletion of a production database with the `teardown` command
  - To avoid forcing opinionated setup of database and user
  - In some cases the mechanism and steps for creating a database and user could be different than simply running a couple of sql statements, and automating every possible case is out of scope for this project

## Extending db-migrations-dotnet CLI For Other Databases

By default this project supports PostgreSQL and SQL Server, but can be extended to support any database engine that Entity Framework supports. For a current list of supported databases, see https://learn.microsoft.com/en-us/ef/core/providers/.

The primary functionalities the dotnet portion of this project provides are:
- Database setup
- Database teardown
- Bootstrap new DbContexts
- Expose settings object for connection string info access

For database implementations already built into this project, see [src/MikeyT.DbMigrations/Implementations/](../src/MikeyT.DbMigrations/Implementations/).

General steps for creating a new database implementation:

- Create setup class that implements abstract class [DbSetup](../src/MikeyT.DbMigrations/Core/DbSetup.cs)
- Create settings class that implements abstract class [DbSettings](../src/MikeyT.DbMigrations/Core/DbSettings.cs)
- Create DB context class that implements Entity Framework class `DbContext` (`Microsoft.EntityFrameworkCore.DbContext`) in addition to interface [IDbSetupContext](../src/MikeyT.DbMigrations/Core/IDbSetupContext.cs) (using your setup class as the generic type for the interface)

The way a particular database implementation for the dotnet CLI portion of this project is "hooked up" to the general overall strategy is via the swig EF config value `dbSetupType` on each DB context settings object. The flow looks like this:
- Swig EF module task executed
- Swig task reads config and calls dotnet CLI with params, including C# setup class name
- Dotnet CLI for MikeyT.DbMigrations reads in the param and dynamically loads assemblies to look for the class

The implementation classes can be located in one of several locations:
- This project and it's associated nuget package, which is referenced by the generated C# migrations project
- A class within the generated C# migrations project
- A class within any project/package that is referenced by the generated C# migrations project

### DbSettings Connection Strings

The `DbSettings` base class requires implementing 2 connection string methods:

- `GetMigrationsConnectionString`
- `GetDbSetupConnectionString`

In the case of the `PostgresSettings` class, the only difference is that for the `GetDbSetupConnectionStringImpl` it connects to the "postgres" database instead of the application specific database, like it does for `GetMigrationsConnectionString`. This is because the application specific database won't exist yet. But note that both postgres connection string methods use the root user/password since they both require elevated permissions.

This is a little confusing since PostgresSettings exposes the application specific username/password that are needed during setup, but the base type doesn't have those. This might end up on the base class if it turns out that all database implementations follow the same pattern.

## Postgres Optional Env Var

If you don't want the connection string to include error detail, you can add `POSTGRES_INCLUDE_ERROR_DETAIL=false` to your .env file (in the root and in the database migrations project).

# Informational Documentation

## Entity Framework Utilization

Microsoft's [Entity Framework](https://learn.microsoft.com/en-us/ef/core/) has DB migrations capability, but most people assume you are forced to use model classes and opt-in to everything EF related, but that's not actually the case. You can very easily run plain raw SQL files using the `MigrationBuilder` method `Sql`. For example, consider this simple usage:

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

So you won't actually have to go in and edit anything. Instead, you can just modify the SQL files that are also automatically generated for you. For example, creating a migration called "Initial" will generate these files:

```
./src/DbMigrations/Scripts/Initial.sql
./src/DbMigrations/Scripts/Initial_Down.sql
```

For info on extending this framework to support other databases that Entity Framework supports, see [Extending db-migrations-dotnet CLI For Other Databases](#extending-db-migrations-dotnet-cli-for-other-databases).

## Swig Wrapper Task Explanation

The following sections describe the categories of swig wrapper tasks and what they map to. For a command reference, go back to the section [Swig EntityFramework Module Task Reference](#swig-entityframework-module-task-reference).

As mentioned above, swig tasks are imported from the [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) npm package.

The swig tasks being imported from the EF swig module have 3 categories (see linked sections below for command mappings and notes):
- [Swig Wrapper Tasks for MikeyT.DbMigrations CLI Commands](#swig-wrapper-tasks-for-mikeytdbmigrations-cli-commands)
- [Swig Wrapper Tasks for Direct Entity Framework (EF) Commands](#swig-wrapper-tasks-for-direct-entity-framework-ef-commands)
- [Utility Tasks](#swig-utility-tasks)

Running dotnet run or dotnet ef directly works—but swig wrapper tasks streamline and standardize the workflow:
- Shorten what you have to type and add extra params based on your config (e.g. `--project`, `--context`). For example, to create a release bundle with swig you only have to type `swig dbCreateRelease` instead of `dotnet ef migrations bundle --project ./src/DbMigrations --context MainDbContext --self-contained -r win-x64 -o ./release/MigrateMainDbContext-win-x64.exe --force`
- Self-document available commands. Instead of fishing around in markdown docs or notes for what commands and options to use, you can just type `swig` and it'll print a simple list of all available commands.
- Utilizes source control controlled config to derive what options to pass to commands. This is also good for self-documentation - JSDoc on the imported config object allows your IDE to easily help you discover all available options and what they do.
- Makes a multi-context setup easy. The most common use case for this is for handling both a "main" and "test" DB context.
- Combine EF and MikeyT.DbMigrations functionality in a simple list of swig tasks instead of having to remember separate sets of commands.

### Swig Wrapper Tasks for MikeyT.DbMigrations CLI Commands

> ℹ️ The swig wrapper commands utilize config to pass all the appropriate options and run in the correct working directory. See [Swigfile Config Reference for EntityFramework Swig Module](#swigfile-config-reference-for-entityframework-swig-module).

| Swig Task | MikeyT.DbMigrations Notes |
| --------- | ------------------- |
| `dbBootstrapDbContext` | Run `dotnet run bootstrap` |
| `dbBootstrapMigrationsProject` | Create a new migrations project. |
| `dbSetup` | Run `dotnet run setup` |
| `dbTeardown` | Run `dotnet run teardown` |
| `dbShowConfig` | Print config object. |

### Swig Wrapper Tasks for Direct Entity Framework (EF) Commands

| Swig Task | Entity Framework (EF) Command |
| --------- | ----------------------------- |
| `dbListMigrations` | `dotnet ef migrations list` |
| `dbAddMigration` | `dotnet ef migrations add` |
| `dbRemoveMigration` | `dotnet ef migrations remove` |
| `dbMigrate` | `dotnet ef database update` |
| `dbCreateRelease` | `dotnet ef migrations bundle` |

### Swig Utility Tasks

| Swig Task | Description |
| --------- | ------------------- |
| `dbShowConfig` | Show config from call to `efConfig.init` function (Javascript/Typescript function in swigfile - see [swig docs](https://github.com/mikey-t/swig) for more info), including defaults not explicitly set. |

### Swig Task Source Reference

Swig EF module has all wrapper tasks in [EntityFramework.ts](https://github.com/mikey-t/swig-cli-modules/blob/main/src/modules/EntityFramework/EntityFramework.ts).

For info on MikeyT.DbMigrations dotnet CLI, see help text in [DbSetupCli.cs](../src/MikeyT.DbMigrations/Core/DbSetupCli.cs).

## Entity Framework CLI Reference

Official microsoft documentation for Entity Framework migrations tool CLI (dotnet-ef):

https://learn.microsoft.com/en-us/ef/core/cli/dotnet

Some anchor links to the most common operations:

- [dotnet tool install --global dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools)
- [dotnet ef migrations list](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-dbcontext-list)
- [dotnet ef migrations add](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-migrations-add)
- [dotnet ef migrations remove](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-migrations-remove)
- [dotnet ef database update](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-database-update)
- [dotnet ef migrations bundle](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-migrations-bundle)
- [Common Options](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#common-options)

## Dotnet Entity Framework CLI Tool (dotnet-ef) Installation and Usage

The swig EF module commands all use the dotnet tool `dotnet-ef` under the hood. If you use the `  dbBootstrapMigrationsProject` command to create your project, it will automatically select an appropriate version of the tool and install it as a "local" tool (with a tool manifest at `./dotnet-tools.json`).

If you need to install this separately, you can install it by running:

```
dotnet tool install dotnet-ef --local
```

If you have an existing project you've checked out of source control, the first time it runs it'll throw an error about missing the tool and to run restore, so do that with it's suggested command:

```
dotnet tool restore
```

It's safe to commit the manifest file (`./dotnet-tools.json`) to source control. This ensures collaborators will be using the same version of the tool.

Notes on changes to how local dotnet tools are installed in dotnet 10: [Local Dotnet Tool Installation Change](./DevNotes.md#local-dotnet-tool-installation-change).

## Csproj File Notes

Using the swig `dbBootstrapMigrationsProject` command will generate a new project with a csproj file similar to this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
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
