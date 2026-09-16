[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $args failed with exit code $LASTEXITCODE"
    }
}

Push-Location $PSScriptRoot
try {
    Invoke-DotNet tool restore
    $artifactRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'artifacts'))
    foreach ($name in @('docfx-api', 'docs')) {
        $target = [IO.Path]::GetFullPath((Join-Path $artifactRoot $name))
        if (-not $target.StartsWith($artifactRoot + [IO.Path]::DirectorySeparatorChar)) {
            throw "Unexpected documentation output path: $target"
        }
        foreach ($path in @($artifactRoot, $target)) {
            if ((Test-Path -LiteralPath $path) -and ((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                throw "Refusing to clean a linked documentation directory: $path"
            }
        }
        if (Test-Path -LiteralPath $target) {
            Remove-Item -LiteralPath $target -Recurse -Force
        }
    }
    Invoke-DotNet docfx docfx.json --warningsAsErrors
    $guide = Get-Content -LiteralPath artifacts/docs/articles/README.html -Raw
    if (-not $guide.Contains('src="https://raw.githubusercontent.com/medokin/soundpad-connector/4a9daae40d3c09dc11830c139ac89bcadd222207/docfx/images/SoundpadConnectorLogo.png"')) {
        throw 'README banner is missing from the generated guide'
    }
} finally {
    Pop-Location
}
