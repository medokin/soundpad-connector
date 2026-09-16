[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $args failed with exit code $LASTEXITCODE" }
}

Push-Location $PSScriptRoot
try {
    $version = '1.5.0'
    $baselineCommit = 'fbc5b8c32a2519370b1688abbdf5321d5d810162'
    ./build.ps1 -Version $version
    ./build-docs.ps1
    foreach ($solution in @('src/SoundpadConnector.sln', 'examples/Examples.sln')) {
        Invoke-DotNet package list --project $solution --vulnerable --include-transitive --no-restore
    }
    for ($run = 1; $run -le 5; $run++) {
        Invoke-DotNet test src/SoundpadConnector.Tests/SoundpadConnector.Tests.csproj --configuration Release --no-build --no-restore --filter 'FullyQualifiedName~TransportTests|FullyQualifiedName~ConnectionTests'
    }

    $baselineRoot = Join-Path $PSScriptRoot ("artifacts/release-baseline-" + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $baselineRoot | Out-Null
    $archive = Join-Path $baselineRoot 'source.zip'
    & git archive --format=zip --output $archive $baselineCommit src/SoundpadConnector
    if ($LASTEXITCODE -ne 0) { throw 'Could not archive the fixed API baseline' }
    Expand-Archive -LiteralPath $archive -DestinationPath $baselineRoot
    Invoke-DotNet build (Join-Path $baselineRoot 'src/SoundpadConnector/SoundpadConnector.csproj') --configuration Release '-p:Version=1.4.0' '-p:GeneratePackageOnBuild=false'
    $baselineAssembly = Join-Path $baselineRoot 'src/SoundpadConnector/bin/Release/netstandard2.0/SoundpadConnector.dll'
    $assembly = Join-Path $PSScriptRoot 'src/SoundpadConnector/bin/Release/netstandard2.0/SoundpadConnector.dll'
    # Compare baseline to candidate for backwards compatibility, allowing additive APIs and the version bump.
    Invoke-DotNet apicompat -l $baselineAssembly -r $assembly --enable-rule-cannot-change-parameter-name --enable-rule-attributes-must-match --noWarn CP0003

    $package = Join-Path $PSScriptRoot "artifacts/SoundpadConnector.$version.nupkg"
    $zip = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $nuspecReader = [IO.StreamReader]::new($zip.GetEntry('SoundpadConnector.nuspec').Open())
        try { [xml] $nuspec = $nuspecReader.ReadToEnd() } finally { $nuspecReader.Dispose() }
        $metadata = $nuspec.package.metadata
        if ($metadata.version -ne $version -or $metadata.license.InnerText -ne 'MIT' -or $metadata.readme -ne 'README.md') {
            throw 'Unexpected package version, license, or readme metadata'
        }
        $libraryEntries = @($zip.Entries | Where-Object { $_.FullName.StartsWith('lib/') })
        if ($libraryEntries.Count -ne 1 -or $libraryEntries[0].FullName -ne 'lib/netstandard2.0/SoundpadConnector.dll') {
            throw 'Unexpected package library contents'
        }
        $stream = $libraryEntries[0].Open()
        try { $packedHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
        if ($packedHash -ne (Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash) {
            throw 'Packaged assembly differs from the verified Release build'
        }
        if ([Reflection.AssemblyName]::GetAssemblyName($assembly).Version.ToString() -ne '1.5.0.0') {
            throw 'Candidate assembly has an unexpected version'
        }
        $readmeReader = [IO.StreamReader]::new($zip.GetEntry('README.md').Open())
        try { $packedReadme = $readmeReader.ReadToEnd() } finally { $readmeReader.Dispose() }
        if ($packedReadme -cne (Get-Content -LiteralPath README.md -Raw)) { throw 'Packaged README differs from the source' }
    } finally {
        $zip.Dispose()
    }
    $hash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  SoundpadConnector.$version.nupkg" | Set-Content -LiteralPath "$package.sha256" -Encoding utf8
    Write-Host "Verified release candidate $version. No package was published. SHA256: $hash"
} finally {
    Pop-Location
}
