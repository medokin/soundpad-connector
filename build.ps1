[CmdletBinding()]
param(
    [ValidatePattern('^[0-9]+[.][0-9]+[.][0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $Version
)

$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $args failed with exit code $LASTEXITCODE"
    }
}

$previousIntegrationOptIn = $env:SOUNDPAD_INTEGRATION_TESTS
Push-Location $PSScriptRoot
try {
    $env:SOUNDPAD_INTEGRATION_TESTS = '0'
    $versionArguments = @()
    if ($Version) { $versionArguments = @("-p:Version=$Version") }
    Invoke-DotNet restore src/SoundpadConnector.sln --force
    Invoke-DotNet restore examples/Examples.sln --force
    Invoke-DotNet build src/SoundpadConnector.sln --configuration Release --no-restore @versionArguments
    Invoke-DotNet build examples/Examples.sln --configuration Release --no-restore @versionArguments
    Invoke-DotNet test src/SoundpadConnector.sln --configuration Release --no-build --no-restore
    Invoke-DotNet pack src/SoundpadConnector/SoundpadConnector.csproj --configuration Release --no-build --no-restore --output artifacts @versionArguments
} finally {
    $env:SOUNDPAD_INTEGRATION_TESTS = $previousIntegrationOptIn
    Pop-Location
}
