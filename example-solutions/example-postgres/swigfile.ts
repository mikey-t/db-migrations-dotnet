import { copyNewEnvValues, log, overwriteEnvFile, simpleSpawnAsync, spawnAsync, spawnAsyncLongRunning } from '@mikeyt23/node-cli-utils'
import fs from 'node:fs'
import fsp from 'node:fs/promises'
import path from 'node:path'
import { series } from 'swig-cli'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'
import { dockerDown, dockerUp } from 'swig-cli-modules/DockerCompose'
import { dbBootstrapMigrationsProject, dbSetup } from 'swig-cli-modules/EntityFramework'

const appPath = 'src/ExampleApiWrapper'
const dbMigrationsProjectPath = 'src/DbMigrations'

efConfig.init(dbMigrationsProjectPath, [
  { name: 'MainDbContext', cliKey: 'main', dbSetupType: 'PostgresSetup', useWhenNoContextSpecified: true, scriptsSubdirectory: 'Main' },
  { name: 'TestDbContext', cliKey: 'test', dbSetupType: 'PostgresSetup', useWhenNoContextSpecified: true, scriptsSubdirectory: 'Test' }
])

export * from 'swig-cli-modules/DockerCompose'
export * from 'swig-cli-modules/EntityFramework'

export async function syncEnvFiles() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(appPath, '.env'))
  await overwriteEnvFile('.env', path.join(dbMigrationsProjectPath, '.env'))
}

// Run the API project
export const run = series(syncEnvFiles, runApp)

// Run this first if you've never used dotnet-ef global tool or if it's been a while
export { installOrUpdateDotnetEfTool } from '@mikeyt23/node-cli-utils/dotnetUtils'

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

// If trying out multiple DbContexts, don't forget to mod any with different database names, for example:
// public class TestDbContext : PostgresMigrationsDbContext
// {
//     public override PostgresSetup GetDbSetup()
//     {
//         return new PostgresSetup(new PostgresEnvKeys { DbName = "DB_NAME_TEST" });
//     }
// }
// See documentation in db-migrations-dotnet project for additional next steps
export const bootstrapMigrationsProject = series(
  dbBootstrapMigrationsProject,
  convertNugetReferenceToProjectReference,
  syncEnvFiles,
  dockerUp
)

async function runApp() {
  await spawnAsyncLongRunning('dotnet', ['watch'], appPath)
}

async function convertNugetReferenceToProjectReference() {
  const result = await spawnAsync('dotnet', ['remove', 'package', 'MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: false, stdio: 'pipe' })
  if (result.code !== 0) {
    log(`failed to remove package - assuming it's already been converted to a project reference`)
    return
  }
  await spawnAsync('dotnet', ['add', 'reference', '../../../../src/MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: true })
}
