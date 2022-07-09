const {
  defaultSpawnOptions,
  waitForProcess,
  createTarball,
  copyNewEnvValues,
  overwriteEnvFile,
  dockerDepsUp,
  dockerDepsUpDetached,
  dockerDepsDown,
  dockerContainerIsRunning
} = require('@mikeyt23/node-cli-utils')
let {throwIfDockerNotRunning} = require('@mikeyt23/node-cli-utils')
const spawn = require('child_process').spawn
const fsp = require('fs').promises
const fse = require('fs-extra')
const {series, parallel} = require('gulp')
const path = require('path')
const yargs = require('yargs/yargs')
const {hideBin} = require('yargs/helpers')
const argv = yargs(hideBin(process.argv)).argv;

const apiAppPath = './src/ExampleApi'
const dbMigratorPath = './src/DbMigrator/'
const dbMigratorDll = 'DbMigrator.dll'
const dockerPath = './docker'
const dockerProjectName = 'dbmigrationsexample'
const dockerDbContainerName = 'dbmigrationsexample_postgres'
const mainDbContextName = 'MainDbContext'

const migratorSpawnOptions = {...defaultSpawnOptions, cwd: path.resolve(__dirname, dbMigratorPath)}
const migratorSpawnOptionsWithInput = {...migratorSpawnOptions, stdio: 'inherit'}

// Comment out function to undo skipping of throwIfDockerDepsNotUp checks
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

async function publishMigrator() {
  return waitForProcess(spawn('dotnet', ['publish', '-o', 'publish'], migratorSpawnOptions))
}

async function dbInitialCreate() {
  return waitForProcess(spawn('dotnet', [`publish/${dbMigratorDll}`, 'dbInitialCreate'], migratorSpawnOptions))
}

async function dbDropAll() {
  return waitForProcess(spawn('dotnet', [`publish/${dbMigratorDll}`, 'dbDropAll'], migratorSpawnOptionsWithInput))
}

async function dbMigrate() {
  return await dbMigrateWithContext(mainDbContextName)
}

async function dbAddMigration() {
  let name = argv['name']
  if (!name) {
    throw '--name param is required'
  }
  return await dbAddMigrationWithContext(name, mainDbContextName, 'Main')
}

async function dbRemoveMigration() {
  return await dbRemoveMigrationWithContext(mainDbContextName)
}

async function dbMigrationsList() {
  return await dbMigrationsListWithContext(mainDbContextName)
}

async function dbMigrateWithContext(dbContextName) {
  let args = ['ef', 'database', 'update']
  let migrationName = argv['name']
  if (!!migrationName) {
    args.push(migrationName)
  }
  args = [...args, '--context', dbContextName]
  return waitForProcess(spawn('dotnet', args, migratorSpawnOptions))
}

async function dbAddMigrationWithContext(migrationName, dbContextName, outputDirName) {
  await throwIfDockerNotRunning()
  return waitForProcess(spawn('dotnet', ['ef', 'migrations', 'add', migrationName, '--context', dbContextName, '-o', `Migrations/${outputDirName}`], migratorSpawnOptions))
}

async function dbRemoveMigrationWithContext(dbContextName) {
  return waitForProcess(spawn('dotnet', ['ef', 'migrations', 'remove', '--context', dbContextName], migratorSpawnOptions))
}

async function dbMigrationsListWithContext(dbContextName) {
  return waitForProcess(spawn('dotnet', ['ef', 'migrations', 'list', '--context', dbContextName], migratorSpawnOptions))
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

const publishDbMigrator = series(publishMigrator, removeEnvFromPublishedMigrator, packageDbMigrator)

exports.syncEnvFiles = syncEnvFiles
exports.dockerDepsUp = series(syncEnvFiles, () => dockerDepsUp(dockerProjectName))
exports.dockerDepsUpDetached = series(syncEnvFiles, () => dockerDepsUpDetached(dockerProjectName))
exports.dockerDepsDown = () => dockerDepsDown(dockerProjectName)

exports.dbInitialCreate = series(throwIfDockerDepsNotUp, parallel(syncEnvFiles, emptyMigratorPublishDir), publishMigrator, dbInitialCreate)
exports.dbDropAll = series(throwIfDockerDepsNotUp, parallel(syncEnvFiles, emptyMigratorPublishDir), publishMigrator, dbDropAll)
exports.dbDropAndRecreate = series(throwIfDockerDepsNotUp, publishMigrator, dbDropAll, dbInitialCreate)
exports.dbMigrate = series(throwIfDockerDepsNotUp, syncEnvFiles, dbMigrate)
exports.dbAddMigration = series(throwIfDockerDepsNotUp, syncEnvFiles, dbAddMigration)
exports.dbRemoveMigration = series(throwIfDockerDepsNotUp, syncEnvFiles, dbRemoveMigration)
exports.dbMigrationsList = series(throwIfDockerDepsNotUp, syncEnvFiles, dbMigrationsList)
exports.publishDbMigrator = publishDbMigrator

exports.cleanNameFiles = cleanNameFiles
