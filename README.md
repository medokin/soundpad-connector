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
3. Run `docfx docfx/docfx` in project root
4. Browse the output in `/docs`

## Examples
Browse the [Examples](examples).

## Limitations
- SoundpadConnector does **not work** with Soundpad's **Demo** version.
- UWP is not supported. The sandbox refuses pipe connections.

## Contributing
You may contribute in several ways like creating new features, fixing bugs, improving documentation and examples
or translating any document here to your language. Read our [Code of Conduct](CODE_OF_CONDUCT.md).

## License
[MIT](LICENSE) - Nikodem Jaworski - 2018

## Special thanks
* [Leppsoft](https://leppsoft.com/soundpad/de/) - The Company behind Soundpad
