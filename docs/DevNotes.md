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
