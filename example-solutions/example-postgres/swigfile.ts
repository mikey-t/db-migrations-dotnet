import { copyNewEnvValues, overwriteEnvFile, spawnAsyncLongRunning } from '@mikeyt23/node-cli-utils'
import fsp from 'node:fs/promises'
import path from 'node:path'
import { series } from 'swig-cli'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'

const appPath = 'src/ExampleApiWrapper'
const dbMigrationsPath = 'src/DbMigrations'

efConfig.init(dbMigrationsPath, [{ name: 'MainDbContext', cliKey: 'main', dbSetupType: 'PostgresSetup', useWhenNoContextSpecified: true }])

export * from 'swig-cli-modules/DockerCompose'
export * from 'swig-cli-modules/EntityFramework'

export async function syncEnvFiles() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(appPath, '.env'))
  await overwriteEnvFile('.env', path.join(dbMigrationsPath, '.env'))
}

// Run the API project
export const run = series(syncEnvFiles, runApp)

// Run this first if you've never used dotnet-ef global tool or if it's been a while
export { installOrUpdateDotnetEfTool } from '@mikeyt23/node-cli-utils/dotnetUtils'

// For quickly testing bootstrapping of a new DbMigrations project
export async function deleteMigrationsProject() {
  await fsp.rm(dbMigrationsPath, { recursive: true, force: true })
}

async function runApp() {
  await spawnAsyncLongRunning('dotnet', ['watch'], appPath)
}

