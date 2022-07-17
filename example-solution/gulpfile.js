const {
  createTarball,
  copyNewEnvValues,
  overwriteEnvFile,
  dockerDepsUp,
  dockerDepsUpDetached,
  dockerDepsDown,
  dockerContainerIsRunning,
  dotnetDllCommand,
  dotnetPublish,
  dotnetDbMigrationsList,
  dotnetDbMigrate,
  dotnetDbAddMigration,
  dotnetDbRemoveMigration
} = require('@mikeyt23/node-cli-utils')
const fsp = require('fs').promises
const fse = require('fs-extra')
const {series, parallel} = require('gulp')
const path = require('path')
const yargs = require('yargs/yargs')
const {hideBin} = require('yargs/helpers')
const argv = yargs(hideBin(process.argv)).argv;

const apiAppPath = './src/ExampleApi'
const dbMigratorPath = './src/DbMigrator/'
const dockerPath = './docker'
const dockerProjectName = 'dbmigrationsexample'
const dockerDbContainerName = 'dbmigrationsexample_postgres'
const mainDbContextName = 'MainDbContext'

// Comment out function to undo skipping of throwIfDockerDepsNotUp checks.
// Skipping it saves several seconds, but won't warn you if you forget to run "npm run dockerUp" before starting your work.
throwIfDockerDepsNotUp = async () => {
  console.log('skipping docker deps check')
}

async function syncEnvFiles() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(apiAppPath, '.env'))
  await overwriteEnvFile('.env', path.join(dockerPath, '.env'))
  await overwriteEnvFile('.env', path.join(dbMigratorPath, '.env'))
}

async function throwIfDockerDepsNotUp() {
  const postgresIsRunning = await dockerContainerIsRunning(dockerDbContainerName)
  if (!postgresIsRunning) {
    throw `Docker dependencies are not running (container ${dockerDbContainerName}). Try running 'npm run dockerUp' or 'npm run dockerUpAttached'.`
  }
}

async function emptyMigratorPublishDir() {
  const migratorPublishPath = path.join(__dirname, `${dbMigratorPath}/publish/`)
  console.log('emptying path: ' + migratorPublishPath)
  fse.emptyDirSync(migratorPublishPath)
}

async function removeEnvFromPublishedMigrator() {
  await fse.remove(path.join(dbMigratorPath, 'publish/.env'))
}

async function packageDbMigrator() {
  console.log('creating DbMigrator tarball')
  await createTarball('publish', 'release', 'DbMigrator.tar.gz', dbMigratorPath)
}

async function cleanNameFiles() {
  const firstNameLines = (await fsp.readFile('./src/ExampleApi/FirstNamesOriginal.txt', 'utf-8')).split('\n')
  let firstNames = []
  for (const line of firstNameLines) {
    const parts = line.split('\t')
    firstNames.push(parts[1])
    firstNames.push(parts[3])
  }
  await fsp.writeFile('./src/ExampleApi/FirstNames.txt', firstNames.join('\n'))

  let lastNameLines = (await fsp.readFile('./src/ExampleApi/LastNamesOriginal.txt', 'utf-8')).split('\n')
  lastNameLines = lastNameLines.map(l => l.replace('\r', '')).filter(l => !!l && l.length > 0)

  let lastNames = []
  for (let i = 0; i < lastNameLines.length; i++) {
    if (i % 4 === 1) {
      lastNames.push(lastNameLines[i])
    }
  }

  await fsp.writeFile('./src/ExampleApi/LastNames.txt', lastNames.join('\n'))
}

async function dbMigratorCommand(command) {
  await dotnetDllCommand('publish/DbMigrator.dll', [command], dbMigratorPath, true) 
}

// ***************************************************
// Reusable/composable command sets and method aliases

const publishMigrator = async () => await dotnetPublish(dbMigratorPath)
const prepDbMigration = series(throwIfDockerDepsNotUp, syncEnvFiles)
const prepDbMigratorCli = series(throwIfDockerDepsNotUp, parallel(syncEnvFiles, emptyMigratorPublishDir), publishMigrator)

// *****************************************************
// Exported tasks that can be referenced in package.json

exports.syncEnvFiles = syncEnvFiles

exports.dockerDepsUp = series(syncEnvFiles, async () => await dockerDepsUp(dockerProjectName))
exports.dockerDepsUpDetached = series(syncEnvFiles, async () => await dockerDepsUpDetached(dockerProjectName))
exports.dockerDepsDown = async () => await dockerDepsDown(dockerProjectName)

exports.dbInitialCreate = series(prepDbMigratorCli, async () => await dbMigratorCommand('dbInitialCreate'))
exports.dbDropAll = series(prepDbMigratorCli, async () => await dbMigratorCommand('dbDropAll'))
exports.dbDropAndRecreate = series(
  prepDbMigratorCli,
  async () => await dbMigratorCommand('dbDropAll'),
  async () => await dbMigratorCommand('dbInitialCreate')
)

exports.dbMigrate = series(prepDbMigration, async () => await dotnetDbMigrate(mainDbContextName, dbMigratorPath, argv['name']))
exports.dbAddMigration = series(prepDbMigration, async () => await dotnetDbAddMigration(mainDbContextName, dbMigratorPath, argv['name']))
exports.dbRemoveMigration = series(prepDbMigration, async () => await dotnetDbRemoveMigration(mainDbContextName, dbMigratorPath))
exports.dbMigrationsList = series(prepDbMigration, async () => await dotnetDbMigrationsList(mainDbContextName, dbMigratorPath))

exports.packageDbMigrator = series(publishMigrator, removeEnvFromPublishedMigrator, packageDbMigrator)

exports.cleanNameFiles = cleanNameFiles
