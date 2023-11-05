import { series } from 'swig-cli'
import { copyNewEnvValues, log, overwriteEnvFile, spawnAsync } from '@mikeyt23/node-cli-utils'
import path from 'node:path'
import fs from 'node:fs'
import fsp from 'node:fs/promises'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'

// "dbInitialCreate": "gulp dbInitialCreate",
// "dbDropAll": "gulp dbDropAll",
// "dbDropAndRecreate": "gulp dbDropAndRecreate",
// "api": "dotnet watch --project ./src/ExampleApi/ExampleApi.csproj",
// "packageDbMigrator": "gulp packageDbMigrator"

const appPath = './src/ExampleApi'
const dbMigrationsPath = 'src/DbMigrations'
const dockerProjectName = 'dbmigrationsexample'
const dockerDbContainerName = 'dbmigrationsexample_postgres'
const mainDbContextName = 'MainDbContext'

efConfig.init('postgres', dbMigrationsPath, [{ name: 'MainDbContext', cliKey: 'main', useWhenNoContextSpecified: true }])

export * from 'swig-cli-modules/EntityFramework'
export * from 'swig-cli-modules/DockerCompose'

export async function syncEnvFiles() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(appPath, '.env'))
  await overwriteEnvFile('.env', path.join(dbMigrationsPath, '.env'))
}

export { installOrUpdateDotnetEfTool } from '@mikeyt23/node-cli-utils/dotnetUtils'


// export async function dbInitialCreate() {
//   await ensureDbMigratorSetup()
// }

export async function clean() {
  await fsp.rm(dbMigrationsPath, { recursive: true, force: true })
}

export async function copyCsproj() {
  const csprojPath = path.join(efConfig.dbMigrationsProjectPath!, `${efConfig.dbMigrationsProjectName}.csproj`)
  const backupPath = csprojPath + '.backup'
  await fsp.copyFile(csprojPath, backupPath)
}

export async function revertCsproj() {
  const csprojPath = path.join(efConfig.dbMigrationsProjectPath!, `${efConfig.dbMigrationsProjectName}.csproj`)
  const backupPath = csprojPath + '.backup'
  await fsp.copyFile(backupPath, csprojPath)
}

export async function copyProgram() {
  const programPath = path.join(efConfig.dbMigrationsProjectPath!, 'Program.cs')
  const backupPath = programPath + '.backup'
  await fsp.copyFile(programPath, backupPath)
}

export async function revertProgram() {
  const programPath = path.join(efConfig.dbMigrationsProjectPath!, 'Program.cs')
  const backupPath = programPath + '.backup'
  await fsp.copyFile(backupPath, programPath)
}

export const dbSetupCommand = series(syncEnvFiles, doDbSetupCommand)

async function doDbSetupCommand() {
  const command = process.argv[3]
  if (!command) {
    log(`error - no command passed (setup, teardown, list)`)
    return
  }
  await spawnAsync('dotnet', ['run', '--project', dbMigrationsPath, command, ...process.argv.slice(4)])
}
