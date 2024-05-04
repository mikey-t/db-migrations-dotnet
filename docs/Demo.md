# db-migrations-dotnet Demo

## Project setup

The following clip demonstrates setting up a brand new project that will be utilizing a single PostgreSQL database:

- Create new directory for project
- Initialize [swig-cli](https://github.com/mikey-t/swig) dev task orchestration tool for project
- Verify swig is working
- Add `docker-compose.yml` file - PostgreSQL is used for this example
- Add `.env` file with credentials for DB access
- Update `swigfile.ts` to re-export methods for the DockerCompose module from the referenced npm package [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules)
- Run new swig task `dockerUp` to start PostgreSQL docker container
- Use vscode PostgreSQL extension to connect to newly running database
- Update `swigfile.ts` to:
  - Re-export methods for the EntityFramework module from the referenced npm package [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules)
  - Add config for the single local PostgreSQL database that this project will be managing
- Run newly available swig task `dbBootstrapMigrationsProject` which will generate a C# project
- Copy `.env` to the newly generated C# migrations project
- Run the swig task `dbSetup` which will create users and databases defined in our `swigfile.ts`
- Use vscode PostgreSQL extension to verify that database was created

![db-migrations-dotnet project setup demo](./images/DbMigrationsDotnetDemo_ProjectSetup01.gif)

See [./GettingStarted.md](./GettingStarted.md) for detailed instructions.

## Add Initial Migration

The following clip demonstrates creating an initial empty migration that we can later use to easily migrate our database back to it's initial state if we need to by using the migration name "Initial":

- Run: `swig dbAddMigration Initial`
- Show migration in it's "pending" state by running the "list" command: `swig dbListMigrations`
- Show boilerplate EF C# files and empty "up" and "down" sql script placeholder files
- Apply migrations: `swig dbMigrate`
- Show that migration is applied by running `swig dbListMigrations` again (no "pending" status)

![db-migrations-dotnet project setup demo](./images/DbMigrationsDotnetDemo_InitialMigration01.gif)

## Add Example Migration

TODO

## Remove Migration

TODO

## Generate EF Deployment Bundle

TODO

## Deploy Using Generated EF Deployment Bundle

TODO
