# db-migrations-dotnet

This is a NuGet package and sample solution that show how to manage DB migrations using plain SQL scripts and the built-in dotnet ef tool (Entity Framework) using some simple wrapper code to provide a connection string and give EF your script paths. All this without actually forcing you into using EF for your data access (we're only using it for the DB migrations themselves). An example project also shows how to use simple node package.json commands and gulp tasks to make the process even easier.

You could extend this concept into a small framework, or just copy and paste a few lines from this example into your own project. The point is that you can get DB migrations without being forced into using EF for data access and without using a heavy opinionated DB migrations framework.

## NuGet package info

The NuGet package contained in this project (MikeyT.DbMigrations) is not in a production ready state. Use at your own risk.

## What do we need out of a DB migration tool?

At the most basic level, a DB migration framework should really just be responsible for keeping track of what scripts have run and provide simple commands to roll forward or back to a particular migration.

## Why?

There are a number of full-featured DB migration frameworks and libraries out there. Most of the time it's probably wise to go with one of these solutions. However, there may be factors that might lead you to make a different choice in some circumstances.

Here are some opinions on why choosing a large framework for DB migrations might not always be the best option:

- Using a DB migration framework might add unnecessary complexity to your project
- Most DB migration frameworks are highly opinionated about how you treat DB management. The moment you need something they don't agree with, you're looking at a large effort to create a workaround, or you may end up ejecting from their solution completely, wasting valuable time. 
- DB frameworks advertise that they've created the ability to manipulate SQL in your favorite non-SQL programming language. My experience is that the best language for SQL is usually... SQL. Using your favorite programming language to manipulate SQL seems cool, but risks ending up as a novelty that costs more time than it saves.

The goal of this project is to demonstrate that there are alternatives to hitching up with a big opinionated DB migration framework or creating an entire framework of your own. Instead we can leverage some existing tools (dotnet ef) and the tried and tested plain SQL scripts that won't fail because of an intermediary translation layer failure.

## What is the shortest path to implementing this?

Most of the code in this project is just example stuff you may or may not need or want. At a minimum, you need:

- A project that includes references to:
  - Microsoft.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Relational
  - Microsoft.EntityFrameworkCore.Design
- A DbContext class that you provide a DB connection string to
- A helper method that calls the `MigrationBuilder.Sql()` method that you can use in in the Up and Down methods that the dotnet ef tool generates

## What extra stuff is in this example project?

Most of the extra fluff in the example project falls in one of these categories:
- Gather up environment variables to be used to create a DB connection string
- Convenience console app script commands to create the initial DB or drop it on a developer machine
- Code to read in SQL files from a common location and process/replace any script placeholders
- Add convenience scripts to build and package a console app that can be used to migrate DBs in non-dev environments

Most projects/teams are going to have specific requirements and preferences when it comes to environments and deployments, so much of this extra example code may or my not be relevant to your particular situation. The key here is that you can take just the dotnet ef tool use and plain SQL scripts and use your team's other custom processes. Flexibility is the name of the game here.

## Common developer DB related tasks

<table>
<thead>
<th>Task</th>
<th>Notes</th>
</thead>
<tbody>

<tr>
<td>List all DB migrations</td>
<td>
<code>npm run dbMigrationsList</code>
</td>
</tr>

<tr>
<td>Create new DB migration</td>
<td>
Steps:

- <code>npm run dbAddMigration -- --name=YourMigrationName</code>
- Create Up and Down SQL scripts in DbMigrator/Scripts dir
- Add calls to <code>MigrationScriptRunner.RunScript</code> in auto-generated Up and Down methods, referencing the SQL files you just created
- Run <code>npm run dbMigrate</code>
</td>
</tr>

<tr>
<td>Migrate DB to up to current</td>
<td>
<code>npm run dbMigrate</code>
</td>
</tr>

<tr>
<td>Migrate DB to a specific state</td>
<td>
If you run <code>npm run dbMigrationsList</code> and see something like this:

<pre>
20220708193900_Initial
20220708194020_AddPerson
20220708194020_AddMoreStuff
</pre>


then you can migrate to the state after AddPerson but before AddMoreStuff by running:

<code>npm run dbMigrate -- --name=AddPerson</code>

If you were at current this will run the <code>Down()</code> method for the AddMoreStuff DB migration. If you were at Initial, this would run the <code>Up()</code> method for the AddPerson DB migration. Note that you don't have to use the auto-generated timestamps when passing migration name as a parameter.
</td>
</tr>

<tr>
<td>Remove DB migration</td>
<td>
You wouldn't want to do this after having pushed code with a migration. If you've already pushed code and you want to remove the schema you created, you should create a new DB migration that drops the objects in question. You should never delete a migration once it has left your developer machine (someone else could have run the migration or CI/CD could have migrated shared DB).<br/><br/>
Steps:

- To find out the name of the migration before the one you want to remove, you could run <code>npm run dbMigrationsList</code>code><br/>
- If you're already run dbMigrate with the migration you want to remove, rollback using the name of the previous migration, run: <code>npm run dbMigrate -- --name=PreviousMigrationName</code><br/>
- Run <code>npm run dbRemoveMigration -- --name=YourMigration</code><br/>
- Delete SQL scripts you previously created in <code>Dbmigrator/Scripts</code>
</td>
</tr>

<tr>
<td>New project setup</td>
<td>
If using the CLI wrapper from the MikeyT.DbMigrations package, you can create the database and database user with:

<code>npm run dbInitialCreate</code>

After creating the database it's a good idea to create an empty initial DB migration:

- <code>npm run dbAddMigration -- --name=Initial</code><br/>
- <code>npm run dbMigrate</code>
</td>
</tr>

<tr>
<td>Misc</td>
<td>
If you're working on script automation for setting up the DB, you might want to repeatedly drop and recreate after making changes. These commands are useful for this scenario:

<code>npm run dbDropAll</code><br/>
<code>npm run dbDropAndRecreate</code>
</td>
</tr>

</tbody>
</table>

## Example steps for setting up a new project

- Create solution with `dotnet new sln -o example-solution`
- `cd example-solution`
- `mkdir src`
- `cd src`
- `dotnet new webapi -o ExampleApi`
- `dotnet new console -o DbMigrator`
- `cd ..`
- `dotnet sln add src/ExampleApi/ExampleApi.csproj`
- `dotnet sln add src/DbMigrator/DbMigrator.csproj`
- Add NuGet dependencies to ExampleApi
  - Dapper
  - Npgsql
  - MikeyT.EnvironmentSettings
- Add NuGet dependencies to DbMigrator
  - Microsoft.EntityFrameworkCore.Design
  - Npgsql
  - MikeyT.DbMigrations
  - MikeyT.EnvironmentSettings
- In DbMigrator, add MainDbContext class (see example project)
- In DbMigrator `Program.cs`, add one-liner (see example project)
- Create or copy over `package.json`/`gulpfile.js` commands/tasks (see example project)
- Create or copy over `.env.template`
- Copy `.env.template` to `.env` and modify values to whatever is appropriate
- Run `npm run dockerUp`
- Run `npm run dbInitialCreate`
- Create initial DB migration with `npm run dbAddMigration -- --name=Initial`
- Run `npm run dbMigrate`
- Create new migration for test model with `npm run dbAddMigration -- --name=AddPerson`
- Create `Scripts` directory in root of console DbMigrator app and populate with blank `AddPerson.sql` and `AddPerson_Down.sql` files
  - Set scripts directory as content to be copied to bin using entry in DbMigrator.csproj (see example project .csproj)
- Fill in add table statement in `AddPerson.sql` and drop in `AddPerson_Down.sql` (see example project)
- In auto-generated `[date]_AddPerson.cs` methods, add call to extension method:
  - In `Up()` use the example line `MigrationScriptRunner.RunScript(migrationBuilder, "AddPerson.sql");`
  - In `Down()` use the example line `MigrationScriptRunner.RunScript(migrationBuilder, "AddPerson_Down.sql");`
- Run `npm run dbMigrate`
- Add data access and controller code (see example project)
- Add environment settings and dependency injection setup to ExampleApi project `Program.cs` (see example project)
- Run `npm run api` (alias for `dotnet watch` pointing to the api project)
