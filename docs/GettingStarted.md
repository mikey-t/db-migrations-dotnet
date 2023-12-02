# Getting Started

This document has getting started instructions. For more in-depth documentation, see [DbMigrationsDotnet](./DbMigrationsDotnet.md).

## Pre-requisites

This project is completely cross-platform due to `dotnet` SDK/CLI and EntityFramework Core being cross-platform and should work on Windows, Mac and Linux. Before getting started, ensure you have the following:

- NodeJS >= 18
- Docker
- .NET SDK version 8 (6 and 7 also supported with additional config value)

## Getting Started Overview

> ℹ️ These instructions assume you have an existing .NET solution that you're adding migrations to, but note that you can start from scratch as well by simply creating a new directory and running commands from the new location.

Follow these high level steps using the detailed instructions in subsequent sections:

- Setup your project to be able to use the `swig-cli` npm package
- Setup docker and start container(s)
- Add `DbContext` metadata to a config `init` method in your new swigfile
- Run a swig command to generate a new C# project with the necessary dependencies and boilerplate
- Run swig commands for database migration operations, as needed: `dbAddMigration`, `dbRemoveMigration`, `dbMigrate`, `dbListMigrations`

## Swig Setup

> ℹ️ If your project already has NodeJS support and an existing `package.json` file, you will need to taylor some of the specifics to your particular configuration. Swig supports any combination of project type (CJS or ESM via package.json `"type": "commonjs"` or `"type": "module"`) and any flavor of syntax, including Typescript. For more info, check out the swig-cli [Syntax Options Matrix](https://github.com/mikey-t/swig#swigfile-syntax-options-matrix).

> ℹ️ Optional: install swig-cli globally so you can use `swig` instead of `npx swig` to run swig tasks: `npm i -g swig-cli@latest`

> ℹ️ These instructions assume you're using NodeJS version 18.

Example steps to setup a project with NodeJS support and a Typescript swigfile:

- Add a package.json file if your project does not already have one:
  ```
  npm init -y
  ```
- *(optional - if using [Volta](https://docs.volta.sh/guide/getting-started))* Pin NodeJS to version 18:
  ```
  volta pin node@18
  ```
- Update the package.json to be ESM:
  ```
  "type": "module"
  ```
- Add a basic `tsconfig.json` file:
  ```json
  {
    "compilerOptions": {
      "target": "ESNext",
      "module": "NodeNext",
      "moduleResolution": "NodeNext",
      "allowSyntheticDefaultImports": true,
      "esModuleInterop": true,
      "forceConsistentCasingInFileNames": true,
      "strict": true,
      "skipLibCheck": true,
      "types": [
        "node"
      ],
      "noEmit": true,
      "baseUrl": ".",
      "rootDir": "."
    },
    "include": [
      "swigfile.ts"
    ],
    "exclude": [
      "node_modules"
    ]
  }
  
  ```
- Add NodeJS dev dependencies:
  ```
  npm i -D typescript tsx swig-cli swig-cli-modules @types/node@18
  ```
- Create a new file at the root of the project called `swigfile.ts`
- Add a "hello world" task to `swigfile.ts`:
  ```typescript
  export async function hello() {
    console.log('hello world!')
  }
  ```
- Verify swig is working by running your new task:
  ```
  swig hello
  ```
- Add or update your `.gitignore` file:
  ```
  .env
  node_modules
  src/DbMigrations/bin
  src/DbMigrations/obj
  ```

Be sure to review the [swig-cli](https://github.com/mikey-t/swig) documentation for more info.

## Setup and Start Docker

> ℹ️ If you already have something listening on port 5432, you can change the `.env` value for the key `DB_PORT` in the instructions below. If you choose a port that is already in use, you'll get an error like this when you run `swig dockerUp`: "Error response from daemon: driver failed programming external connectivity on endpoint". Simply change the `DB_PORT` in your `.env` (both root and `./src/DbMigrations/.env`) and re-run `swig dockerUp`.

Now we need a database to operate on. We are going to use PostgreSQL running in a docker container.

- Create a file at the root of your project called `docker-compose.yml` with the following content:
  ```yaml
  version: "3.7"  

  services:
    postgresql:
      image: postgres:15.3
      volumes:
        - postgresql_data:/var/lib/postgresql/data
      environment:
        POSTGRES_USER: "${DB_ROOT_USER:?}"
        POSTGRES_PASSWORD: "${DB_ROOT_PASSWORD:?}"
      ports:
        - "${DB_PORT:-5432}:5432"

  volumes:
    postgresql_data:

  ```
- Update your swigfile with this line to re-export swig tasks from swig module `DockerCompose`:
  ```
  export * from 'swig-cli-modules/DockerCompose'
  ```
  Now when you run `swig`, you should see 3 new tasks:
  ```
  dockerDown
  dockerUp
  dockerUpAttached
  ```
- Create a `.env` file at the root of your project (you can use alternate values if you'd like):
  ```
  DB_HOST=localhost
  DB_PORT=5432
  DB_USER=dbmigrationsexample
  DB_NAME=dbmigrationsexample
  DB_PASSWORD=Abc1234!
  DB_ROOT_USER=postgres
  DB_ROOT_PASSWORD=Abc1234!
  ```
- Run `swig dockerUp`

You should now have a PostgreSQL database running in a docker container! You can verify using a tool like [pgAdmin](https://www.pgadmin.org/download/) or the VSCode extension [PostgreSQL](https://marketplace.visualstudio.com/items?itemName=ckolkman.vscode-postgres). Be sure to use the values from your `.env` to connect (`DB_ROOT_USER` and `DB_ROOT_PASSWORD`). Note that the application specific user hasn't been setup yet - we'll execute a command to set that up after we define our config below.

## Add DbContext Config to Swigfile

Import config from the swig module `EntityFramework` and re-export swig tasks by adding the below config to your `swigfile.ts`. This will point all the swig tasks to the location where your new project will be generated and will define metadata about your DbContext classes (change `dotnetSdkVersion` if you're using 6 or 7):

```typescript
import efConfig from 'swig-cli-modules/ConfigEntityFramework'

const dbMigrationsProjectPath = 'src/DbMigrations'

efConfig.init(
  dbMigrationsProjectPath,
  [
    {
      name: 'MainDbContext',
      cliKey: 'main',
      dbSetupType: 'PostgresSetup',
      useWhenNoContextSpecified: true
    }
  ],
  { dotnetSdkVersion: 8 }
)

export * from 'swig-cli-modules/EntityFramework'
export * from 'swig-cli-modules/DockerCompose'

```

Now when you run `swig`, you should see a new list of available commands, for example:

```
[ Command: list ][ Swigfile: swigfile.ts ][ Version: 0.0.16 ]
Available tasks:
  dbAddMigration
  dbBootstrapDbContext
  dbBootstrapMigrationsProject
  dbListMigrations
  dbMigrate
  dbRemoveMigration
  dbSetup
  dbShowConfig
  dbTeardown
  dockerDown
  dockerUp
  dockerUpAttached
[ Result: success ][ Total duration: 126 ms ]
```

## Bootstrap Your new DbMigrations Project

Bootstrap your new DbMigrations project by running:

```
swig dbBootstrapMigrationsProject
```

Copy the project root `.env` file you created earlier to the DbMigrations project at `./src/DbMigrations/.env`.

> ℹ️ It's recommended to automate syncing of env files when you run database/docker/migrations related commands on a dev machine. Swig is a good tool for this type of dev automation. For a simple example, see the `syncEnvFiles` method in one of the example project's `swigfile.ts`: [../example-solutions/example-postgres/swigfile.ts](../example-solutions/example-postgres/swigfile.ts).

Assuming you used the example swigfile content from above, this bootstrap command will create a new C# console project at `./src/DbMigrations/` with our new `MainDbContext`.

## Run DB Migration Commands with Swig Tasks

Now that we have a database migrations project and a running database, we are going to:

- Setup the database (create a role and schema)
- Create an initial empty migration called "Initial"

First run:

```
swig dbSetup
```

After the application database schema and user have been setup, the output should finish with something like:

```
✅ setup complete
```

Create an initial empty migration called "Initial":

```
swig dbAddMigration Initial
```

This will essentially run the dotnet-ef command `dotnet ef migrations add Initial` (it also adds the context name and project path params for you).

Now if we run `swig dbListMigrations`, we can see output like the following that will tell us there is a pending migration:

```
20231126221937_Initial (Pending)
```

If this was a normal migration, we'd go update the automatically generated sql files for the up and down operations:

```
- 📄src\DbMigrations\Scripts\Initial.sql
- 📄src\DbMigrations\Scripts\Initial_Down.sql
```

But in this case we want an empty migration, so we are going to apply the migration to the database with this command:

```
swig dbMigrate
```

## Conclusion

You did it, good job!

As mentioned above, the helper methods provided via swig tasks will also add context and project path params for you. This is part of what allows working with multiple contexts/databases simultaneously.

By defining a small amount of config in our swigfile, we have:

- Complete control over our database migrations for one or many databases from a central location
- The ability to easily setup or teardown the database on any dev machine (or ephemeral test environments)
- Boilerplate creation taken care of (creating `up` and `down` sql files for each migration)
- The ability to use plain SQL for our database migrations
- Shortened migration commands that we can retrieve easily if we forget by simply running `swig` from our project root
