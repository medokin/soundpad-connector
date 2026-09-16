[CmdletBinding()]
param([Parameter(Mandatory)][string] $Tag)

$ErrorActionPreference = 'Stop'

if ($Tag -cne 'v1.5.0') {
    throw 'Refusing NuGet publication: only the approved v1.5.0 tag is supported'
}
[xml] $project = Get-Content -LiteralPath src/SoundpadConnector/SoundpadConnector.csproj -Raw
if ($project.Project.PropertyGroup.Version -cne '1.5.0') {
    throw 'Refusing NuGet publication: project version must be stable 1.5.0'
}
$readme = Get-Content -LiteralPath README.md -Raw
if ($readme.Contains('1.5.0-dev') -or -not $readme.Contains('dotnet add package SoundpadConnector --version 1.5.0')) {
    throw 'Refusing NuGet publication: README must describe stable 1.5.0 installation, not development'
}
& git merge-base --is-ancestor HEAD origin/master
if ($LASTEXITCODE -ne 0) {
    throw 'Refusing NuGet publication: release commit must be on master history'
}
Write-Host 'NuGet release preflight passed for v1.5.0'
