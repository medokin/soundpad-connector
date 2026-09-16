using System;
using System.Threading;
using System.Threading.Tasks;
using SoundpadConnector;

namespace Examples
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var soundpad = new Soundpad();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            soundpad.StatusChanged += (_, _) => Console.WriteLine(soundpad.ConnectionStatus);
            try
            {
                await soundpad.ConnectAsync().WaitAsync(timeout.Token);
                var version = await soundpad.GetVersion().WaitAsync(timeout.Token);
                if (!version.IsSuccessful) throw new InvalidOperationException(version.ErrorMessage);
                Console.WriteLine("Remote control API version: " + version.Value);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
