# db-migrations-dotnet

The MikeyT.DbMigrations Nuget package contains:

- `DbSetupCli` class that exposes commands `setup`, `teardown`, `list`, and `bootstrap`
- `MigrationScriptRunner` class to be used for running pure SQL migrations with simple placeholder replacement
- Base classes to allow for creating custom implementations: `DbSetup`, `DbSettings`, `IDbSetupContext`

See the db-migrations-dotnet project readme for usage information: https://github.com/mikey-t/db-migrations-dotnet
