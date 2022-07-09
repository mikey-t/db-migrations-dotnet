const {series} = require('gulp')
const {dotnetBuild, dotnetPack, dotnetNugetPublish} = require('@mikeyt23/node-cli-utils')
require('dotenv').config()

const namespace = 'MikeyT.DbMigrations'
const projPath = `./src/${namespace}`

exports.packOnly = series(dotnetBuild, () => dotnetPack(projPath))
exports.publish = series(dotnetBuild, () => dotnetPack(projPath), () => dotnetNugetPublish(projPath, `${namespace}.csproj`))
