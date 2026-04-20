# MikeyT.DbMigrations Dev Notes<!-- omit in toc -->

- [Misc](#misc)
- [Unit Tests](#unit-tests)
- [Project Reference](#project-reference)
- [EF Bundle Testing](#ef-bundle-testing)
- [Local Dotnet Tool Installation Change](#local-dotnet-tool-installation-change)
- [Docker Compose Filename Change](#docker-compose-filename-change)
- [Postgres Docker Compose Change](#postgres-docker-compose-change)
- [Roadmap](#roadmap)


# Misc

To keep things simple, there is only one namespace: `MikeyT.DbMigrations`

Core/base functionality is in the `Core` directory

Implementations for specific databases are in the `Implementations` directory, but still use the same namespace

Don't forget to terminate existing DB connections in each implementation's `Teardown` method

Example Program.cs for PostgreSQL:

```CSharp
return await new MikeyT.DbMigrations.DbSetupCli().Run(args);
```

For command info, see CLI help text in [DbSetupCli.cs](../src/MikeyT.DbMigrations/Core/DbSetupCli.cs).

# Unit Tests

Swig `test` command reminders:

- `c`: coverage
- `r`: report (first run on a new machine requires first running `dotnet tool restore`)
- `o`: only (run tests marked with `[Trait("Category", "only")]`)
- `v`: verbose

Unit test coverage is output to `./coverage`. When running tests with the `coverage` and `report` options, a message will be logged with a link to the html file (currently this is coverage/index.html).

The VSCode extension [Coverage Gutters](https://marketplace.visualstudio.com/items?itemName=ryanluker.vscode-coverage-gutters) seems to work pretty well for showing coverage info inline.



# Project Reference

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

# EF Bundle Testing

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

# Local Dotnet Tool Installation Change

Dotnet 10 introduced a change to how local dotnet tool installation works:

- No longer need to pass `--create-manifest-if-needed` to `dotnet tool install dotnet-ef --local` command - it now automatically passes this option
- They changed the default path of the local tool manifest from `./config/dotnet-tools.json` to `./dotnet-tools.json`

I didn't previously have the manifest checked into source control, but for now I've decided to commit that now. Note that requires running `dotnet tool restore` with a fresh clone. I haven't decided whether to try and automate that into swig tasks or not. Adding it would incur a small performance penalty and there isn't currently a fast "is it already restored" check. Right now it'll throw a detailed error telling you to run the restore command, so I'm leaving it that way for now.

# Docker Compose Filename Change

The default for docker compose files is no longer docker-compose.yml - it was changed to `compose.yaml`.

# Postgres Docker Compose Change

Version 18 of Postgres containers now have the data directory of `/var/lib/postgresql` (they removed `/data`), so the `volumes` entry in the docker compose file should now be `- postgresql_data:/var/lib/postgresql`.

# Roadmap

- Research way to avoid needing to copy .env file into DbMigrations directory
- Additional unit testing. Coverlet is wired up and working and ready for more tests.
- `DbSetupCli` integration testing. For each database type, spin up a docker container with fresh DB and run tests to exercise `setup`, `teardown`, `list` and `bootstrap` commands.
- Add support for additional database types
- Additional diagrams and documentation showing how each component of this architecture is composed together:
  - [swig-cli](https://github.com/mikey-t/swig)
  - [swig-cli-modules](https://github.com/mikey-t/swig-cli-modules)
  - [node-cli-utils](https://github.com/mikey-t/node-cli-utils)
  - [db-migrations-dotnet](https://github.com/mikey-t/db-migrations-dotnet)
- Create plan for how to support alternative methods of injecting connection string information into DbContext classes (instead of relying solely on `.env` files)
