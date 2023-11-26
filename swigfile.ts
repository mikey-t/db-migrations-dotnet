import { Emoji, SpawnResult, emptyDirectory, getRequiredEnvVar, log, requireValidPath, spawnAsync, spawnAsyncLongRunning, which } from '@mikeyt23/node-cli-utils'
import { dotnetBuild, installOrUpdateReportGeneratorTool } from '@mikeyt23/node-cli-utils/dotnetUtils'
import 'dotenv/config'
import fs from 'node:fs'
import fsp, { readdir } from 'node:fs/promises'
import path from 'node:path'
import { pathToFileURL } from 'node:url'
import { series } from 'swig-cli'

const namespace = 'MikeyT.DbMigrations'
const projectPath = `src/${namespace}`
const testProjectPath = `test/${namespace}.Test`
const testCoverageDir = `coverage`
const testResultsDir = `${testProjectPath}/TestResults`
const examplesBaseDir = 'example-solutions'
const exampleSolutionNames = [
  'example-postgres',
  // 'example-sqlserver'
]
const exampleSolutionDirs = exampleSolutionNames.map(exampleName => path.join(examplesBaseDir, exampleName))

export const packOnly = series(buildDbMigrations, packDbMigrations)

export const publish = series(
  cleanProject,
  buildDbMigrations,
  ['testPackage', () => test(true)],
  testExamples,
  packDbMigrations,
  nugetPublishDbMigrations
)

// Decorate tests with the following attribute for "only" functionality:
// [Trait("Category", "only")]
export async function test(withCoverageReportOverride = false) {
  await deleteTestCoverage()

  const verboseFlags = oneOfArgsPassed('verbose', 'v') ? ['--logger', 'console;verbosity=detailed'] : []
  const onlyFlags = oneOfArgsPassed('only', 'o') ? ['--filter', 'Category=only'] : []
  const coverageArgs = oneOfArgsPassed('coverage', 'c') || withCoverageReportOverride ? ['--collect:"XPlat Code Coverage"'] : []

  let result: SpawnResult
  if (oneOfArgsPassed('watch', 'w')) {
    result = await spawnAsyncLongRunning('dotnet', ['watch', 'test', ...verboseFlags, ...onlyFlags, ...coverageArgs], testProjectPath)
  } else {
    result = await spawnAsync('dotnet', ['test', ...verboseFlags, ...onlyFlags, ...coverageArgs], { cwd: testProjectPath })
  }
  
  if (result.code !== 0) {
    throw new Error('Tests failed')
  }

  if (oneOfArgsPassed('report', 'r') || withCoverageReportOverride) {
    if (coverageArgs.length === 0) {
      log(`${Emoji.Warning} You must also pass "coverage" (or "c") along with "report" (or "r") for the coverage report to be generated`)
      return
    }
    await genTestCoverageReport()
  }
}

export async function testExamples() {
  log(`Not yet implemented`)
}

export async function deleteTestCoverage() {
  log(`emptying test results directory: ${testResultsDir}`)
  await emptyDirectory(testResultsDir, { force: true })
  log(`emptying test coverage directory: ${testCoverageDir}`)
  await emptyDirectory(testCoverageDir, { force: true })
}

export async function exampleParseNames() {
  const firstNameLines = (await fsp.readFile('./example-resources/FirstNamesOriginal.txt', 'utf-8')).split('\n')
  let firstNames = []
  for (const line of firstNameLines) {
    const parts = line.split('\t')
    firstNames.push(parts[1])
    firstNames.push(parts[3])
  }
  await fsp.writeFile('./example-resources/FirstNames.txt', firstNames.join('\n'))

  let lastNameLines = (await fsp.readFile('./example-resources/LastNamesOriginal.txt', 'utf-8')).split('\n')
  lastNameLines = lastNameLines.map(l => l.replace('\r', '')).filter(l => !!l && l.length > 0)

  let lastNames = []
  for (let i = 0; i < lastNameLines.length; i++) {
    if (i % 4 === 1) {
      lastNames.push(lastNameLines[i])
    }
  }

  await fsp.writeFile('./example-resources/LastNames.txt', lastNames.join('\n'))
}

export async function installTestReportGenerator() {
  await installOrUpdateReportGeneratorTool()
}

export async function cleanProject() {
  const csProjDirs = [projectPath, testProjectPath, ...exampleSolutionDirs]
  let dirsToDelete: string[] = []
  csProjDirs.forEach(projDir => {
    dirsToDelete.push(path.join(projDir, 'bin'))
    dirsToDelete.push(path.join(projDir, 'obj'))
  })

  for (const dir of dirsToDelete) {
    log(`deleting directory: ${dir}`)
    await fsp.rm(dir, { force: true, recursive: true })
  }
  await deleteTestCoverage()
}

function oneOfArgsPassed(...argNames: string[]) {
  for (const argName of argNames) {
    if (process.argv.slice(3).includes(argName)) {
      return true
    }
  }
  return false
}

async function buildDbMigrations() {
  await dotnetBuild(projectPath)
}

async function packDbMigrations() {
  await dotnetPack(projectPath)
}

async function nugetPublishDbMigrations() {
  await dotnetNugetPublish(projectPath, `${namespace}.csproj`)
}

async function dotnetPack(projectDirectoryPath: string, release = true) {
  requireValidPath('projectDirectoryPath', projectDirectoryPath)

  let args = ['pack']
  if (release === true) {
    args.push('-c', 'Release')
  }

  await spawnAsync('dotnet', args, { cwd: projectDirectoryPath })
}

async function dotnetNugetPublish(projectDirectoryPath: string, csprojFilename: string, release = true, nugetSource = 'https://api.nuget.org/v3/index.json') {
  const apiKey = getRequiredEnvVar('NUGET_API_KEY')
  const packageDir = path.join(projectDirectoryPath, release ? 'bin/Release' : 'bin/Debug')
  const packageName = await getFullPackageNameWithVersion(projectDirectoryPath, csprojFilename)
  log('publishing package ' + packageName)
  await spawnAsync('dotnet', ['nuget', 'push', packageName, '--api-key', apiKey, '--source', nugetSource], { cwd: packageDir })
}

async function getFullPackageNameWithVersion(projectPath: string, csprojFilename: string) {
  const namespace = csprojFilename.substring(0, csprojFilename.indexOf('.csproj'))
  const csprojPath = path.join(projectPath, csprojFilename)
  const csproj = await fsp.readFile(csprojPath, 'utf-8')
  const versionTag = '<PackageVersion>'
  const xmlVersionTagIndex = csproj.indexOf(versionTag)
  const versionStartIndex = xmlVersionTagIndex + versionTag.length
  const versionStopIndex = csproj.indexOf('<', versionStartIndex)
  const version = csproj.substring(versionStartIndex, versionStopIndex)
  return `${namespace}.${version}.nupkg`
}

async function genTestCoverageReport() {
  const reportGenToolName = 'reportgenerator'
  if (!(await which(reportGenToolName)).location) {
    throw new Error(`The global dotnet tool "${reportGenToolName}" is not installed - try running "swig installTestReportGenerator"`)
  }

  const testResultSubdirectories = (await readdir(testResultsDir)).filter(dirent => fs.lstatSync(path.join(testResultsDir, dirent)).isDirectory())
  if (testResultSubdirectories.length !== 1) {
    throw new Error(`Expected exactly one directory in the test results directory (${testResultsDir}), but found ${testResultSubdirectories.length} directories - cannot generate report`)
  }

  const xmlCoverageFilePath = path.resolve(path.join(testResultsDir, testResultSubdirectories[0], 'coverage.cobertura.xml'))
  if (!fs.existsSync(xmlCoverageFilePath)) {
    throw new Error(`Did not find expected code coverage xml file (${xmlCoverageFilePath}) - cannot generate report`)
  }

  const testCoverageAbsolutePath = path.resolve(testCoverageDir)
  if (!fs.existsSync(testCoverageAbsolutePath)) {
    throw new Error(`Test coverage directory could not be resolved to an absolute path - cannot generate report (testCoverageDir: ${testCoverageDir} | testCoverageAbsolutePath: ${testCoverageAbsolutePath})`)
  }

  // Paths are required to be absolute or "explicitly" a relative path (prefixed with './')
  const reportArgs = [
    `-reports:${xmlCoverageFilePath}`,
    `-targetdir:"${testCoverageAbsolutePath}"`,
    '-reporttypes:Html'
  ]

  await spawnAsync(reportGenToolName, reportArgs, { cwd: testProjectPath })

  log(`${Emoji.Info} Report generated at ${(pathToFileURL(path.join(testCoverageAbsolutePath, 'index.html')))}`)
}
