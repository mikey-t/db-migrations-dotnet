# MikeT.DbMigrations example solution

This is an example solution with a simple .net API with a companion console app to handle DB migrations and package.json commands and gulpfile tasks to simplify the development process.

# Initial setup

- Ensure .net 6 SDK is installed
- Ensure node is installed (version 14 or above)
- `npm install`
- Copy `.env.template` to a new file `.env` and modify any values you'd like to change
- `npm run dockerUp`
- `npm run dbInitialCreate`
- `npm run dbMigrate`

# How to run

- `npm run api`

When you run `npm run api` in a terminal you should see console messages with the loaded environment variables and your browser should open up the open api spec page (swagger) to something like `https://localhost:7012/swagger/index.html`. You can use the swagger UI to try out the sample api endpoints.

| Endpoint                    | Notes                                                                                          |
|-----------------------------|------------------------------------------------------------------------------------------------|
| PUT&nbsp;/api/person/{num}  | Generate {num} random Person objects and inserts them into the database.                       |
| GET&nbsp;/api/person/all    | Returns all person objects from the database.                                                  |
| DELETE&nbsp;/api/person/all | Deletes all person objects from the database.                                                  |
| GET&nbsp;/api/person/random | This does nothing with the database - it just generates a random Person object and returns it. |

# More info

For more info, look at the readme for the MikeyT.DbMigrations project.
