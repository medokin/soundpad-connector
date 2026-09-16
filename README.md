# SoundpadConnector .NET

<p align="center">
   <img src="https://raw.githubusercontent.com/medokin/soundpad-connector/4a9daae40d3c09dc11830c139ac89bcadd222207/docfx/images/SoundpadConnectorLogo.png" alt="Logo SoundpadConnector .NET" title="SoundpadConnector .NET" />
</p>

SoundpadConnector provides a .NET API to control a local [Soundpad](https://www.leppsoft.com/soundpad/) instance.

## Requirements

The library targets .NET Standard 2.0. Soundpad communication requires Windows named pipes; the soundlist launch helper also uses the Windows registry. Use a supported .NET runtime. Repository examples and tests use .NET 10 on Windows.

Target-framework compatibility is not a guarantee that every Windows or Soundpad version has been tested. Maintenance verification uses isolated Windows pipes, not a real Steam or standalone Soundpad installation. Distribution-specific compatibility in [#12](https://github.com/medokin/soundpad-connector/issues/12) remains unconfirmed.

## Installation

Install the published [NuGet package](https://www.nuget.org/packages/SoundpadConnector):

```powershell
dotnet add package SoundpadConnector --version 1.5.0
```

Version `1.5.0` includes the V4 API calls introduced in GitHub v1.4.0, subsequent API additions, and maintenance fixes. Older NuGet 1.3.1 does not contain these changes. Building locally does not publish a package.

## QuickStart

The following examples and lifecycle/parsing guarantees describe version `1.5.0`. Older packages may behave differently. Repository examples use a local project reference; `build.ps1` also produces the package in `artifacts`.

This .NET 10 example only reads the remote control API version. Start Soundpad first.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using SoundpadConnector;

using var soundpad = new Soundpad();
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    await soundpad.ConnectAsync().WaitAsync(timeout.Token);
    var version = await soundpad.GetVersion().WaitAsync(timeout.Token);
    if (!version.IsSuccessful) throw new InvalidOperationException(version.ErrorMessage);
    Console.WriteLine(version.Value);
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}
```

Await connection establishment before sending commands and check `IsSuccessful` before using values. Failed connections throw and report `Disconnected`, not `Connected`. `GetVersion` returns the remote control API version; `GetSoundpadVersion` returns the Soundpad product version.

`Seek(milliseconds)` sets an absolute playback position, unlike relative `Jump`. `PlaySoundFromCategory(categoryIndex, soundIndex, renderLine, captureLine)` uses a one-based position within the category, not a global sound index; category `-1` means the selected category. `GetMainFrameTitleText` uses the vendor's `GetTitleText()` command. Soundlist entries preserve optional `Color` and `Tag` XML attributes as raw strings (null when absent, empty when supplied empty); no color encoding is assumed.

`AutoReconnect` defaults to false. When enabled, connection attempts can keep retrying. `Disconnect` cancels connection/retry/poll work; a later `ConnectAsync` creates a new pipe. `Dispose` ends the connector's lifetime. `WaitAsync` only bounds the caller's wait, not the underlying operation: disconnect or dispose the connector when abandoning a timed-out request, as the example's `using` scope does.

## Examples

[Example source](https://github.com/medokin/soundpad-connector/tree/master/examples/Examples) includes the read-only console app and a tested count-polling helper. Running the console app contacts Soundpad but does not play sounds or replace its soundlist:

```powershell
dotnet run --project examples/Examples/Examples.csproj --configuration Release
```

## Troubleshooting

### Non-transactional calls

Soundpad can acknowledge an action before its state changes. A count request immediately after `AddSound` may still return the old value. Do not use an unbounded busy loop or treat the unchanged count as success.

Copy the [SoundlistPolling helper](https://github.com/medokin/soundpad-connector/blob/master/examples/Examples/SoundlistPolling.cs) into your application. It checks successful responses, returns when the count differs, delays between requests, and accepts cancellation. For example, with an already connected `soundpad` and a valid `newSoundPath`:

```csharp
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    var previous = await soundpad.GetSoundFileCount().WaitAsync(timeout.Token);
    if (!previous.IsSuccessful) throw new InvalidOperationException(previous.ErrorMessage);
    var added = await soundpad.AddSound(newSoundPath).WaitAsync(timeout.Token);
    if (!added.IsSuccessful) throw new InvalidOperationException("Soundpad rejected the add request.");
    var count = await SoundlistPolling.WaitForCountChangeAsync(
        soundpad.GetSoundFileCount, previous.Value, timeout.Token);
    Console.WriteLine(count);
}
catch (OperationCanceledException)
{
    soundpad.Disconnect();
    throw;
}
```

A changed count is only an observation, not proof that this specific addition completed if another client changes the soundlist concurrently.

### Large responses and paths

Message-mode pipes are read through their message boundary, preserving large XML and split UTF-8 characters. Byte-mode pipes do not provide response boundaries and retain a single-read limitation. Real Soundpad framing across supported versions is not confirmed; [#13](https://github.com/medokin/soundpad-connector/issues/13) remains open for real-instance confirmation.

`LoadSoundlist` uses the registered Soundpad executable and a quoted absolute path. It can launch Soundpad and replace its soundlist. Path encoding is covered by safe tests, but real loading in [#10](https://github.com/medokin/soundpad-connector/issues/10) remains unverified.

UWP is not part of the test matrix. Previous reports about sandbox support and demo/trial editions are not current compatibility guarantees. Consult the [vendor remote control manual](https://www.leppsoft.com/soundpad/help/manual/tutorial/rc/) for product requirements.

## Development

Install the SDK selected by `global.json` (10.0.401 or a newer patch in the same feature band). `NuGet.Config` uses nuget.org without inheriting machine-specific feeds. From the repository root:

```powershell
./build.ps1
./build-docs.ps1
```

The first script restores and audits dependencies, builds both solutions in Release, runs safe tests, and packs `artifacts/SoundpadConnector.1.5.0.nupkg`. Known dependency vulnerabilities fail restore. Push/PR CI uploads build artifacts without publishing. NuGet publishing is separate: an approved stable `v1.5.0` GitHub release triggers verification and a protected publishing job. Manual runs of the publishing workflow only verify and never publish. See the release checklist for setup and approval requirements. No workflow deploys documentation.

Normal tests do not require Soundpad. Parser and transport tests use unique isolated pipes, bounded waits and disposable connections. Example polling tests do not contact Soundpad.

See the [release checklist](https://github.com/medokin/soundpad-connector/blob/master/RELEASE.md) for version `1.5.0`, compatibility checks, known live-test gaps and the publishing procedure. `./verify-release.ps1` verifies the stable package without publishing; normal builds use the same version.

Live tests are skipped unless `SOUNDPAD_INTEGRATION_TESTS` is exactly `1`. They can launch Soundpad and replace its soundlist. `build.ps1` temporarily disables them even if your environment opts in. Only run them against a session you intend to modify:

```powershell
$previousOptIn = $env:SOUNDPAD_INTEGRATION_TESTS
try {
    $env:SOUNDPAD_INTEGRATION_TESTS = '1'
    dotnet test src/SoundpadConnector.IntegrationTests/SoundpadConnector.IntegrationTests.csproj --configuration Release
} finally {
    $env:SOUNDPAD_INTEGRATION_TESTS = $previousOptIn
}
```

### Documentation

[Online API docs](https://medokin.github.io/soundpad-connector/api/index.html) may lag the current source. `build-docs.ps1` restores pinned local DocFX, regenerates library-only metadata, treats warnings as errors, and recreates the generated `artifacts/docfx-api` and `artifacts/docs` directories. It does not delete documentation sources or publish a site.

Preview locally after building:

```powershell
dotnet docfx serve artifacts/docs --hostname localhost --port 8080
```

Browse [localhost:8080](http://localhost:8080). The rendered guide comes from this README rather than a separate stale copy.

## Contributing and license

Contributions are welcome. See the [Code of Conduct](https://github.com/medokin/soundpad-connector/blob/master/CODE_OF_CONDUCT.md) and [MIT license](https://github.com/medokin/soundpad-connector/blob/master/LICENSE).

Thanks to [Leppsoft](https://www.leppsoft.com/) for Soundpad and @itsameshaw for investigating large responses in [#15](https://github.com/medokin/soundpad-connector/pull/15).
