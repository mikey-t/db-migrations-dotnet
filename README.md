# db-migrations-dotnet

This is an example of how to manage DB migrations using plain SQL scripts and the built-in dotnet ef tool (Entity Framework) using some simple wrapper code to provide a connection string and give EF your script paths. All this without actually forcing you into using EF for your data access (we're only using it for the DB migrations themselves). An example project also shows how to use simple node package.json commands and gulp tasks to make the process even easier.

You could extend this concept into a small framework, or just copy and paste a few lines from this example into your own project. The point is that you can get DB migrations without being forced into using EF for data access and without using a heavy opinionated DB migrations framework.

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
