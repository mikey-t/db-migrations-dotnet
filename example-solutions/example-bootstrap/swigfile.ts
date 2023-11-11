import { series } from 'swig-cli'
import { copyNewEnvValues, log, overwriteEnvFile, simpleSpawnAsync, spawnAsync } from '@mikeyt23/node-cli-utils'
import efConfig from 'swig-cli-modules/ConfigEntityFramework'
import fs from 'node:fs'
import fsp from 'node:fs/promises'
import path from 'node:path'

const dbMigrationsProjectPath = 'src/DbMigrations'

efConfig.init(
  'postgres',
  dbMigrationsProjectPath,
  [
    { name: 'MainDbContext', cliKey: 'main', useWhenNoContextSpecified: true },
    { name: 'TestDbContext', cliKey: 'test' }
  ]
)

export * from 'swig-cli-modules/EntityFramework'
export * from 'swig-cli-modules/DockerCompose'

export async function deleteMigrationsProject() {
  if (fs.existsSync(dbMigrationsProjectPath)) {
    log('removing sln reference')
    await simpleSpawnAsync('dotnet', ['sln', 'remove', dbMigrationsProjectPath])
    log('deleting project')
    await fsp.rm(dbMigrationsProjectPath, { recursive: true, force: true })
  } else {
    log('project does not exist - exiting')
  }
}

export async function syncEnv() {
  await copyNewEnvValues('.env.template', '.env')
  await overwriteEnvFile('.env', path.join(dbMigrationsProjectPath, '.env'))
}

export const callCli = series(syncEnv, directCliCommand)

async function directCliCommand() {
  await spawnAsync('dotnet', ['run', ...process.argv.slice(3)], { cwd: dbMigrationsProjectPath })
}
