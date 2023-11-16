import { copyNewEnvValues, log, overwriteEnvFile, simpleSpawnAsync, spawnAsync } from '@mikeyt23/node-cli-utils'
import fs from 'node:fs'
import fsp from 'node:fs/promises'
import path from 'node:path'
import { series } from 'swig-cli'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'
import { dockerDown } from 'swig-cli-modules/DockerCompose'
import { dbBootstrapMigrationsProject } from 'swig-cli-modules/EntityFramework'

const dbMigrationsProjectPath = 'src/DbMigrations'

efConfig.init(
  dbMigrationsProjectPath,
  [
    { name: 'MainDbContext', cliKey: 'main', useWhenNoContextSpecified: true, dbSetupType: 'PostgresSetup' },
    { name: 'TestDbContext', cliKey: 'test', useWhenNoContextSpecified: true, dbSetupType: 'PostgresSetup' }
  ]
)

export * from 'swig-cli-modules/DockerCompose'

export { dbAddMigration, dbListMigrations, dbMigrate, dbRemoveMigration } from 'swig-cli-modules/EntityFramework'

export const bootstrapMigrationsProject = series(dbBootstrapMigrationsProject, convertNugetReferenceToProjectReference, syncEnv)

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
  await spawnAsync('docker', ['volume', 'remove', 'example-bootstrap_postgresql_data'], { throwOnNonZero: false, stdio: 'pipe' })
}

export async function syncEnv() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(dbMigrationsProjectPath, '.env'))
}

export const callCli = series(syncEnv, directCliCommand)

async function directCliCommand() {
  await spawnAsync('dotnet', ['run', '--', ...process.argv.slice(3)], { cwd: dbMigrationsProjectPath })
}

async function convertNugetReferenceToProjectReference() {
  const result = await spawnAsync('dotnet', ['remove', 'package', 'MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: false, stdio: 'pipe' })
  if (result.code !== 0) {
    log(`failed to remove package - assuming it's already been converted to a project reference`)
    return
  }
  await spawnAsync('dotnet', ['add', 'reference', '../../../../src/MikeyT.DbMigrations'], { cwd: dbMigrationsProjectPath, throwOnNonZero: true })
}
