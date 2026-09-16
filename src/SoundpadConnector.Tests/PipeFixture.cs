using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundpadConnector.Tests
{
    internal sealed class PipeFixture : IAsyncDisposable
    {
        private readonly CancellationTokenSource _timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        private PipeFixture()
        {
            var name = "soundpad-connector-tests-" + Guid.NewGuid().ToString("N");
            Server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public NamedPipeServerStream Server { get; }
        public NamedPipeClientStream Client { get; }
        public CancellationToken Token => _timeout.Token;

        public static async Task<PipeFixture> ConnectAsync()
        {
            var fixture = new PipeFixture();
            try
            {
                await Task.WhenAll(fixture.Server.WaitForConnectionAsync(fixture.Token), fixture.Client.ConnectAsync(fixture.Token));
                return fixture;
            }
            catch
            {
                await fixture.DisposeAsync();
                throw;
            }
        }

        public async Task WriteResponseAsync(string response, int fragmentSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fragmentSize);
            var bytes = Encoding.UTF8.GetBytes(response);
            for (var offset = 0; offset < bytes.Length; offset += fragmentSize)
            {
                await Server.WriteAsync(bytes.AsMemory(offset, Math.Min(fragmentSize, bytes.Length - offset)), Token);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _timeout.CancelAsync();
            Client.Dispose();
            Server.Dispose();
            _timeout.Dispose();
        }
    }
}
