<h1 align="center">
   <img src="SoundpadConnectorLogo.png" alt="Logo Soundpad Connector" title="Logo Soundpad Connector" />
</h1>

<p align="center">
    Soundpad Connector provides an API to programmatically interact with a local <a href="https://store.steampowered.com/app/629520/Soundpad/">Soundpad</a> instance.
</p>

## Table of contents

  * [Installation](#installation)
  * [QuickStart](#quickstart)
  * [Limitations](#limitations)
  * [Contributing](#contributing)
  * [License](#license)
  * [Special thanks](#special-thanks)

## Installation
TODO

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

## Limitations
SoundpadConnector does **not work** with Soundpad's **Demo** version.


## Contributing
You may contribute in several ways like creating new features, fixing bugs, improving documentation and examples
or translating any document here to your language.

## License
[MIT](LICENSE) - Nikodem Jaworski - 2018

## Special thanks
* [Leppsoft](https://leppsoft.com/soundpad/de/) - The Company behind Soundpad