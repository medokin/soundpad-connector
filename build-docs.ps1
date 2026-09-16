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
    Invoke-DotNet docfx docfx/docfx.json --warningsAsErrors
} finally {
    Pop-Location
}
