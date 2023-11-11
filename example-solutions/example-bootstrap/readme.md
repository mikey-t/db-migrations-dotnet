# Example Bootstrap DbContext

Running the DbSetupCli command `bootstrap` will do the following:

- Create a new `DbContext` class using the first param after `bootstrap` as the class name
- Get boilerplate for the content of the file from the second param after `bootstrap`, which should be derived from `DbSetup` (for example: `PostgresSetup`)
- Create a new directory at `src/DbMigrations/Migrations/YourDbContextMigrations/`
- Add a `<Folder />` reference to the `.csproj` file for the new folder created

## Example Creation

Example command to bootstrap a new DbContext called `MyDbContext` using the `PostgresSetup` class as the `DbSetup` type by running:

```shell
dotnet run --project src/DbMigrations bootstrap MyDbContext PostgresSetup
```

or by using the swig task wrapper:

```shell
swig callCli bootstrap MyDbContext PostgresSetup
```

Optional if using the [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) module `EntityFramework`: add a new `DbContextInfo` to the config init call

## Delete a DbContext

Delete a DbContext manually:

- Delete the DbContext class file
- Remove the migrations folder:
    - Delete the Migrations folder under `src/DbMigrations/Migrations/NameOfContextMigrations`
    - Remove the folder reference (e.g. `<Folder Include="Migrations/YourDbContextMigrations" />`) from the DbMigrations project csproj file: `src/DbMigrations/DbMigrations.csproj`
- (Optional) If using the [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules) module `EntityFramework`, remove the `DbContextInfo` from the config init call
