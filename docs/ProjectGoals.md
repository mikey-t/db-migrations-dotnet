# MikeyT.DbMigrations Project Goals

### Goal: Use EntityFramework to manage database migrations

No need to re-invent the wheel.

### Goal: Avoid use of EntityFramework for data access

Many developers prefer not to use EntityFramework for data access outside of small projects and prototypes. This project demonstrates how to take advantage of EntityFramework for database migrations without actually using it for data access, so you can use some other solution (i.e. [Dapper](https://github.com/DapperLib/Dapper)).

### Goal: Provide flexible and extensible initial database setup/teardown functionality

EntityFramework migrations work really well, but they omit the very first step - setting up the new database. This repository's Nuget package (MikeyT.DbMigrations) provides this functionality.

### Goal: Use raw SQL for migrations instead of relying on a translation layer from xml/json/yaml or custom code in a non-sql programming language

Many database migration frameworks attempt to add an abstraction layer over the actual SQL that is applied to the database. They do this for a variety of good reasons, but sometimes the cost outweighs the benefit and more time is spent working around limitations or bugs in the abstraction layers than was saved.

Even if this project's setup/teardown functionality breaks or you prefer not to use the NodeJS "wrapper scripting", at the end of the day the actual database migration functionality described in this process is plain vanilla EntityFramework and you can simply run EF commands directly if you prefer.

### Goal: Automate boilerplate generation

EntityFramework handles generating most of the boilerplate, but if you want to wire up the auto-generated Up and Down methods to load and run raw SQL files, you have several more small steps to take for each migration. This project automates these extra steps.

### Goal: Provide SQL script placeholder replacement

This project provides the ability to replace placeholders in your raw SQL scripts before they are executed. A default implementation is provided, but you can also easily override this with your own custom implementation.

### Goal: Ability for a developer to manage multiple databases

It's valuable to write unit tests that access a real database instance to ensure the highest level of confidence in data access code. However, tests often need to seed data or manipulate data and perform destructive operations in order to test edge cases. It's even useful to intentionally put the database into a bad within one of these tests state to test error scenarios. It's painful as a developer to spend time setting up some scenario in your locally running application, only to accidentally delete or overwrite data or otherwise break things because of tests.

Using this project's approach, you can easily manage 2 instances of your database with very little overhead. This helps to keep your local application data isolated from tests.

### Goal: Showcase docker

Containerization is a popular tool for deploying applications in cloud environments, but it's also extremely useful to abstract project dependencies so they're cross-platform and easy to setup for each new developer that would like to setup your project on their dev machine.

Instead of dealing with everyone managing their own copy of database server software or shared dev instances (yuck!), all you need is a docker-compose.yml file and a tiny bit of scripting so people accessing your project don't have to be a docker expert. All they need to do when pulling down you project is to run `npm install` and `swig dockerUp`. They can also easily set different ports to use by simply modifying a .env file, which is especially useful if you frequently jump between many projects that might also be using the same database engine and expecting it to be on the same default port.

### Goal: Showcase developer task automation with [swig-cli](https://github.com/mikey-t/swig)

[Swig-cli](https://github.com/mikey-t/swig) is similar to Gulp.js. It provides ultra-fast dev task automation by allowing you to simply write a function in your "swigfile" and execute it from a terminal by simply specifying the name of your function. The library provides `series` and `parallel` methods to allow you to chain things together, and by "things", it's literally just functions. You don't have to learn any new language or framework. If you know even basic javascript, you already know how to use swig. Swig supports both CommonJS and ESM, in addition to Typescript!
