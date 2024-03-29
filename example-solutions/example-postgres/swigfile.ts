import { copyNewEnvValues, log, overwriteEnvFile, simpleSpawnAsync, spawnAsync, spawnAsyncLongRunning } from '@mikeyt23/node-cli-utils'
import fs from 'node:fs'
import fsp from 'node:fs/promises'
import path from 'node:path'
import { series } from 'swig-cli'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'
import { dockerDown, dockerUp } from 'swig-cli-modules/DockerCompose'
import { dbBootstrapMigrationsProject, dbSetup } from 'swig-cli-modules/EntityFramework'

const exampleApiPath = 'src/ExampleApiWrapper'
const dbMigrationsProjectPath = 'src/DbMigrations'

// Simple example with one DbContext. See README.md before switching example config.
efConfig.init(dbMigrationsProjectPath, [
  { name: 'MainDbContext', cliKey: 'main', dbSetupType: 'PostgresSetup' }
])

// Example with 2 DbContexts (2 databases). See README.md before switching example config.
// efConfig.init(dbMigrationsProjectPath, [
//   { name: 'MainDbContext', cliKey: 'main', dbSetupType: 'PostgresSetup' },
//   { name: 'TestDbContext', cliKey: 'test', dbSetupType: 'PostgresSetup' }
// ])

// Example with 2 DbContexts (2 databases) that use different sql scripts in segregated subdirectories. See README.md before switching example config
// efConfig.init(dbMigrationsProjectPath, [
//   { name: 'MainDbContext', cliKey: 'main', dbSetupType: 'PostgresSetup', scriptsSubdirectory: 'Main' },
//   { name: 'TestDbContext', cliKey: 'test', dbSetupType: 'PostgresSetup', scriptsSubdirectory: 'Test' }
// ])

export * from 'swig-cli-modules/DockerCompose'
export * from 'swig-cli-modules/EntityFramework'

export async function syncEnvFiles() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(exampleApiPath, '.env'))
  await overwriteEnvFile('.env', path.join(dbMigrationsProjectPath, '.env'))
}

// Run the ExampleApi project
export const run = series(syncEnvFiles, runExampleApi)

// For quickly testing bootstrapping of a new DbMigrations project
export async function deleteMigrationsProject() {
  if (!fs.existsSync(dbMigrationsProjectPath)) {
    log('project does not exist - exiting')
  }
  log('removing sln reference')
  await simpleSpawnAsync('dotnet', ['sln', 'remove', dbMigrationsProjectPath])
  log('deleting project')
  await fsp.rm(dbMigrationsProjectPath, { recursive: true, force: true })
  log('bringing docker containers down')
  await dockerDown()
  log('deleting data volume if it exists')
  await spawnAsync('docker', ['volume', 'remove', 'example-postgres_postgresql_data'], { throwOnNonZero: false, stdio: 'pipe' })
}

// You can just run "dbBootstrapMigrationsProject", but this is an example of how to automate the next steps (env files, dockerUp, dbSetup).
// Also converts the nuget reference to a project reference for easy testing of changes to the DbSetupCli C# lib.
export const bootstrapMigrationsProject = series(
  dbBootstrapMigrationsProject,
  convertNugetReferenceToProjectReference,
  syncEnvFiles,
  dockerUp,
  dbSetup
)

export async function convertNugetReferenceToProjectReference() {
  const result = await spawnAsync('dotnet', ['remove', 'package', 'MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: false, stdio: 'pipe' })
  if (result.code !== 0) {
    log(`failed to remove package - assuming it's already been converted to a project reference`)
    return
  }
  await spawnAsync('dotnet', ['add', 'reference', '../../../../src/MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: true })
}

async function runExampleApi() {
  await spawnAsyncLongRunning('dotnet', ['watch'], exampleApiPath)
}
