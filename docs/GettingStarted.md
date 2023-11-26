# Getting Started

This document has getting started instructions. For more in-depth documentation, see [DbMigrationsDotnetDocumentation.md](./DbMigrationsDotnetDocumentation.md).

## Pre-requisites

This project is completely cross-platform due to `dotnet` SDK/CLI and EntityFramework Core being cross-platform and should work on Windows, Mac and Linux. Before getting started, ensure you have the following:

- .NET SDK 6
- NodeJS >= 18
- Docker

## Getting Started Overview

> ℹ️ These instructions assume you have an existing .NET solution that you're adding migrations to, but note that you can start from scratch as well by simply creating a new directory and running commands from the new location.

Follow these high level steps using the detailed instructions in subsequent sections:

- Setup your project to be able to use the `swig-cli` npm package
- Add `DbContext` metadata to a config init method in your `swigfile`
- Run a swig command to generate a new C# project with the necessary dependencies and boilerplate
- Setup docker and start container(s)
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

Be sure to review the [swig-cli](https://github.com/mikey-t/swig) documentation for more info.

## Add DbContext Config to Swigfile

Import config from swig-cli-modules and re-export swig tasks from the swig-cli module EntityFramework by adding the following to `swigfile.ts`:

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
  ]
)

export * from 'swig-cli-modules/EntityFramework'

```

Now when you run `swig`, you should see a new list of available commands, for example:

```
[ Command: list ][ Swigfile: swigfile.ts ][ Version: 0.0.15 ]
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
[ Result: success ][ Total duration: 126 ms ]
```

## Bootstrap Your new DbMigrations Project

Bootstrap your new DbMigrations project by running:

```
swig dbBootstrapMigrationsProject
```

## Setup and Start Docker

## Run DB Migration Commands with Swig Tasks
