
<img src="images/SoundpadConnectorLogo.png" alt="Logo SoundpadConnector .NET" title="SoundpadConnector .NET" />

<br>

SoundpadConnector provides an .NET API to programmatically interact with a local <a href="https://store.steampowered.com/app/629520/Soundpad/">Soundpad</a> instance.


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
3. Run `docfx docfx/docfx` in project root
4. Browse the output in `/docs`

## Examples
Browse the [Examples](https://github.com/medokin/soundpad-connector/tree/master/examples).

## Limitations
- SoundpadConnector does **not work** with Soundpad's **Demo** version.
- UWP is not supported. The sandbox refuses pipe connections.

## Troubleshooting
### Unexpected result when performing multiple calls?
Soundpad calls are not transactional. You may get a response before the action happened in Soundpad. For example:
```csharp
var countResult = await soundpad.GetSoundFileCount();
Console.WriteLine(countResult.Value); // 9

await soundpad.AddSound(newSoundPath);

var newCountResult = await soundpad.GetSoundFileCount();
Console.WriteLine(newCountResult.Value); // 9 again, but we're expecting 10, right?
```

You can wait a certain amount of time between the calls, but that wont be safe eighter and makes you app slow.
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
or translating any document here to your language. Read our [Code of Conduct](https://github.com/medokin/soundpad-connector/blob/master/CODE_OF_CONDUCT.md).

## License
[MIT](https://github.com/medokin/soundpad-connector/blob/master/LICENSE) - Nikodem Jaworski - 2018

## Special thanks
* [Leppsoft](https://leppsoft.com/soundpad/de/) - The Company behind Soundpad
