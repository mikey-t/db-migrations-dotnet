# db-migrations-dotnet POC

This proof of concept is a minimal demonstration of how to use Entity Framework for database migrations with plain sql files. For more info on the db-migrations-dotnet project, refer back to the main readme: https://github.com/mikey-t/db-migrations-dotnet.

## Too Long, Didn't Read

In a nutshell:
- The EF class `MigrationBuilder` (this is the built-in object that's used in the auto-generated C# migrations files) has a method called `Sql` that allows you to pass in raw sql. See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/operations#using-migrationbuildersql.
- This example configures some plain sql files to be embedded scripts that can be executed by their associated migration.
- We generate a migration using the normal built-in EF command: `dotnet ef migrations add SomeMigrationName`
- From the auto-generated C# migration file, we then call the  `MigrationBuilder.Sql()` method using a simple extension method that reads in the raw sql from file and then executes it using the extension method previously mentioned (see [./MigrationBuilderExtensions.cs](./MigrationBuilderExtensions.cs)).

That's it! When we run EF commands to migrate up or down, it'll execute our sql from our embedded sql files! Note that you don't have to embed the files - this is just one way to handle that.

The remainder of the db-migrations-dotnet project is all about automating things, removing boilerplate and providing better command discoverability.

## Entity Framework CLI Documentation

Entity Framework CLI docs: https://learn.microsoft.com/en-us/ef/core/cli/dotnet

Commands to focus on:

| Command | Description |
| ------- | ----------- |
| `dotnet ef migrations list` | List migrations |
| `dotnet ef migrations add` | Add a migration |
| `dotnet ef migrations remove` | Remove last migration (must be pending, not applied) |
| `dotnet ef database update` | Run all pending migrations |
| `dotnet ef migrations bundle` | Package migrations into deployable executable |

## POC Pre-requisites

- Dotnet SDK >= 6
- Docker

## Try It Yourself

Steps to set this project up from scratch:

- Ensure you have EF CLI tool installed: `dotnet tool install --global dotnet-ef`
  - Or update it if already installed: `dotnet tool update --global dotnet-ef`
- Create new directory for your project and navigate to it in a terminal
- Run: `dotnet new sln`
- Run: `dotnet new console -o DbMigrations`
- Install dependencies:
  - Navigate to new console project subdirectory in your terminal and run each of the following commands:
  - `dotnet add package Microsoft.EntityFrameworkCore.Design`
  - `dotnet add package Microsoft.EntityFrameworkCore`
  - `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
- Create a `docker-compose.yml` file in the root of your project and copy the contents of this project's docker compose file
- Create a file called `MyDbContext.cs` in your console project directory and copy the contents of this project's version (don't forget to change the namespace statement)
- Start docker sql server instance in detached mode by running this in your project's root directory: `docker compose up -d`
- Verify your container is running:
  - Run: `docker container ps`
  - Look at output for a sql server instance named similar to your project, for example, this project generated a container called "example-basic-concept-1"
  - Connect to sql server instance using your preferred tool (SSMS, for example) and ensure you can connect to `127.0.0.1,1430` using username `sa` and password `Abc1234!` (note that we're using 1430 instead of 1433 in case you have another sql server instance running already)
- Create the initial database schema and user "dbmigrationsexample" using your preferred tool/method (for example, with SSMS UI)

EF Commands to run (navigate to console project subdirectory in your terminal):

- Create an "Initial" empty migration we'll use for a baseline: `dotnet ef migrations add Initial`
- Execute initial empty migration: `dotnet ef database update`
- Create a migration called "AddPerson": `dotnet ef migrations add AddPerson`
- View list of migrations (should have "AddPerson" migration as "pending"): `dotnet ef migrations list`
- Run migrations: `dotnet ef database update`
- Create new directory `Scripts` and add to it 2 new empty sql files:
  - `AddPerson.sql`
  - `AddPerson_Down.sql`
- Populate new script files with contents from this project's versions of those files
- Ensure script files are embedded into built assembly by adding this to your csproj file:
  ```Xml
  <ItemGroup>
    <EmbeddedResource Include="Scripts\*.sql" />
  </ItemGroup>
  ```
- Add to the `Up` and `Down` methods in the auto-generated C# migration file:
  - `migrationBuilder.RunScript("AddPerson.sql");`
  - `migrationBuilder.RunScript("AddPerson_Down.sql");`
- Migrate your database: `dotnet ef database update`

You can also try the down operation (assuming you followed the previous instructions):
- Update the database to the migration before the most recent, in this case "Initial": `dotnet ef database update Initial`
- Verify the new table got removed and that the output of `dotnet ef migrations list` shows the "AddPerson" migration as "pending"
- Remove the migration by running (this will delete the C# files previously generated): `dotnet ef migrations remove`
