# MikeT.DbMigrations example solution - PostgreSQL

See [ExampleSolutions.md](../../docs/ExampleSolutions.md).

## Switching Between Swig EF Config Examples

You can comment and uncomment the different `efConfig.init` in `swigfile.ts` to try out different options. When doing this, you'll need to keep a few things in mind:

- Switching to or from the example where the DbContexts each use a different subdirectory will require you to:
  - Update each migration C# file under `Migrations/MainDbContextMigrations` so that the relative path now points to the correct location
  - If moving to config with subdirectories, create a `src/DbMigrations/Scripts/Main` and move scripts into it, or if going back to non-subdirectories, move scripts out of that `Main` directory back into the `Scripts` directory `Main` directory
- If switching to the multiple DbContext example:
  - Bootstrap the new C# DbContext class by running: `swig dbBootstrapDbContext TestDbContext PostgresSetup`
  - Update `src/DbMigrations/TestDbContext.cs` so it uses a different database name:
    ```
    public class TestDbContext : PostgresMigrationsDbContext
    {
        public override PostgresSetup GetDbSetup()
        {
            return new PostgresSetup(new PostgresEnvKeys { DbNameKey = "DB_NAME_TEST" });
        }
    }
    ```
  - Bring the new TestDbContext in sync with the MainDbContext by running `swig dbAddMigration test Initial` and `swig dbAddMigration test AddPerson` and then adding the example sql from the sql files in the root to the newly generated script files in the DbMigrations project
