# MikeyT.DbMigrations Dev Notes

## Misc

To keep things simple, there is only one namespace: `MikeyT.DbMigrations`

Core/base functionality is in the `Core` directory

Implementations for specific databases are in the `Implementations` directory, but still use the same namespace

Don't forget to terminate existing DB connections in each implementation's `Teardown` method

Example Program.cs for PostgreSQL:

```CSharp
return await new MikeyT.DbMigrations.DbSetupCli().Run(args);
```

For command info, see CLI help text in [DbSetupCli.cs](../src/MikeyT.DbMigrations/Core/DbSetupCli.cs).

## Unit Tests

Swig test command reminders:

- `c`: coverage
- `r`: report
- `o`: only (run tests marked with `[Trait("Category", "only")]`)
- `v`: verbose

Unit test coverage is output to `./coverage`. When running tests with the `coverage` and `report` options, a message will be logged with a link to the html file (currently this is coverage/index.html).

The VSCode extension [Coverage Gutters](https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters) seems to work pretty well for showing coverage info inline.

## Project Reference

When troubleshooting a live project that references the MikeyT.DbMigrations Nuget package, it's helpful to switch the package reference to a local project reference:

```
dotnet remove package MikeyT.DbMigrations
```

Add to csproj instead:

```
<ItemGroup>
  <ProjectReference Include="C:\path\to\db-migrations-dotnet\src\MikeyT.DbMigrations\MikeyT.DbMigrations.csproj" />
</ItemGroup>
```

```
dotnet build
```

## EF Bundle Testing

Create bundle in example-postgres project:

```
cd example-solutions/example-postgres
swig dbCreateRelease
```

Copy file to server:

```
scp -i <path_to_key_file> ./release/MigrateMainDbContext-linux-x64.exe <user@location>:/home/<user>/eftest
```

On server:

```
cd ~/eftest
chmod u+x MigrateMainDbContext-linux-x64.exe
```

Create `.env` with correct values in the same directory, then execute it:

```
./MigrateMainDbContext-linux-x64.exe
```

