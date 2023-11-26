# Getting Started

This document has getting started instructions, for more in-depth documentation, see [DbMigrationsDotnetDocumentation.md](./DbMigrationsDotnetDocumentation.md).

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

## Add DbContext Config to Swigfile

## Bootstrap Your new DbMigrations Project

## Setup and Start Docker

## Run DB Migration Commands with Swig Tasks
