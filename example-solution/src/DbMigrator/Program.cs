using DbMigrator;
using MikeyT.DbMigrations.Postgres;

await new PostgresDbMigratorCli().Run(args, new MainDbContext());
