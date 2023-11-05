# MikeyT.DbMigrations Dev Notes

## Misc

- To keep things simple, there is only one namespace: `MikeyT.DbMigrations`
- Core/base functionality is in the `Core` directory
- Implementations for specific databases are in the `Implementations` directory, but still use the same namespace
- Don't forget to terminate existing DB connections in implementation `drop` command

Example Program.cs for PostgreSQL:

```CSharp
return await new MikeyT.DbMigrations.PostgresSetup().Run(args);
```

Commands:

- `create`
- `drop`

Common env vars to implement (example from PostgresSettings constructor):

```C#
public PostgresSettings()
{
    DotEnv.Load();
    Host =           MiscUtil.GetEnvString("DB_HOST");
    Port =           MiscUtil.GetEnvString("DB_PORT");
    DbName =         MiscUtil.GetEnvString("DB_NAME");
    DbUser =         MiscUtil.GetEnvString("DB_USER");
    DbPassword =     MiscUtil.GetEnvString("DB_PASSWORD");
    DbRootUser =     MiscUtil.GetEnvString("DB_ROOT_USER");
    DbRootPassword = MiscUtil.GetEnvString("DB_ROOT_PASSWORD");
}
```
