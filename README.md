<h1 align="center">
   <img src="docfx/images/SoundpadConnectorLogo.png" alt="Logo SoundpadConnector .NET" title="SoundpadConnector .NET" />
</h1>

<p align="center">
    SoundpadConnector provides an .NET API to programmatically interact with a local <a href="https://store.steampowered.com/app/629520/Soundpad/">Soundpad</a> instance.
</p>

## Table of contents

  * [Requirements](#requirements)
  * [Installation](#installation)
  * [QuickStart](#quickstart)
  * [Documentation](#documentation)
    * [API docs](#api-docs)
    * [Build the docs](#build-the-docs)
  * [Examples](#examples)
  * [Limitations](#limitations)
  * [Troubleshooting](#troubleshooting)
  * [Contributing](#contributing)
  * [License](#license)
  * [Special thanks](#special-thanks)

## Requirements
This library is build on .NET Standard 2.0. Following plattforms are [supported](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support):

* .NET Core 2.0 or higher
* .NET Framework 4.6.1 or higher


## Installation
Get the NuGet package [SoundpadConnector](https://www.nuget.org/packages/SoundpadConnector) or install via NuGet console:
```bash
PM> Install-Package SoundpadConnector
```

## QuickStart
```csharp
using System;
using SoundpadConnector;

namespace Examples {
    class Program {
        public static Soundpad Soundpad;

        static void Main(string[] args)
        {
            Soundpad = new Soundpad();
            Soundpad.StatusChanged += SoundpadOnStatusChanged;

            // Note that the API is asynchronous. Make sure that Soundpad is connected before executing commands.
            Soundpad.ConnectAsync();

            Console.ReadLine();

        }

        private static void SoundpadOnStatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine(Soundpad.ConnectionStatus);

            if (Soundpad.ConnectionStatus == ConnectionStatus.Connected)
            {
                Soundpad.PlaySound(1);              
            }
        }
    }
}

```

## Documentation

### Api docs
Read the [Docs](https://medokin.github.io/soundpad-connector/api/index.html) online.
This is still work-in-progress!

### Build the docs
1. Install [Chocolatey](https://chocolatey.org/)
2. Install [Docfx]() via [Chocolatey](https://chocolatey.org/) `choco install docfx -y`
3. Run `docfx docfx/docfx.json` in project root
4. Browse the output in `/docs`

## Examples
Browse the [Examples](examples).

## Limitations
- SoundpadConnector does **not work** with Soundpad's **Demo** version 3 and below.
- UWP is not supported/tested. The sandbox refuses pipe connections. Users reported that it works from Windows 10 version 2004 and above.

## Troubleshooting
### Unexpected result when performing multiple calls?
Soundpad calls are not transactional. You may get a response before the action happens in Soundpad. For example:
```csharp
var countResult = await soundpad.GetSoundFileCount();
Console.WriteLine(countResult.Value); // 9

await soundpad.AddSound(newSoundPath);

var newCountResult = await soundpad.GetSoundFileCount();
Console.WriteLine(newCountResult.Value); // 9 again, but we're expecting 10, right?
```

You can wait a certain amount of time between the calls, but that won't be safe either and makes your app slow.
Another way is to loop until the value changes:

```csharp
var countResult = await soundpad.GetSoundFileCount();
Console.WriteLine(countResult.Value); // 9

await soundpad.AddSound(newSoundPath);

while(true) {
    var newCountResult = await soundpad.GetSoundFileCount();
    
    if(newCountResult.Value == countResult.Value) {
        Console.WriteLine(newCountResult.Value); // 10
        break;
    }
}
```

## Contributing
You may contribute in several ways like creating new features, fixing bugs, improving documentation and examples
or translating any document here to your language. Read our [Code of Conduct](CODE_OF_CONDUCT.md).

### Development

Install the .NET SDK selected by `global.json` (10.0.401 or a newer patch in the 10.0.4xx feature band).
The library remains on .NET Standard 2.0; the test and example projects use .NET 10.
The repository's `NuGet.Config` uses nuget.org without inheriting machine-specific package feeds.

Run these commands from the repository root:

```powershell
dotnet build src/SoundpadConnector.sln --configuration Release
dotnet build examples/Examples.sln --configuration Release
dotnet test src/SoundpadConnector.sln --configuration Release --no-build
dotnet list src/SoundpadConnector.sln package --vulnerable --include-transitive
```

Ordinary test runs execute parser and isolated named-pipe tests without Soundpad installed.
Real-Soundpad integration tests are skipped unless `SOUNDPAD_INTEGRATION_TESTS` is exactly `1`.
They can launch Soundpad and replace its soundlist. The example app also plays a sound.
Only opt in against a Soundpad session you intend to modify:

```powershell
$env:SOUNDPAD_INTEGRATION_TESTS = '1'
try {
    dotnet test src/SoundpadConnector.IntegrationTests/SoundpadConnector.IntegrationTests.csproj --configuration Release
} finally {
    Remove-Item Env:SOUNDPAD_INTEGRATION_TESTS
}
```

The fake pipe fixture uses unique pipe names, bounded waits, and disposable connections.
Its response-delivery tests exercise parsers, not the connector's transport framing.

## License
[MIT](LICENSE) - Nikodem Jaworski - 2018

## Special thanks
* [Leppsoft](https://leppsoft.com/soundpad/de/) - The Company behind Soundpad
