# db-migrations-dotnet Demo

## Project setup

The following video demonstrates setting up a brand new project that will be utilizing a single PostgreSQL database:

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

TODO

## Add Example Migration

TODO

## Remove Migration

TODO

## Generate EF Deployment Bundle

TODO

## Deploy Using Generated EF Deployment Bundle

TODO
