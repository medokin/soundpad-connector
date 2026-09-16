using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public async Task FailedConnectionDoesNotRaiseConnected()
        {
            using var pipe = new NamedPipeClientStream(".", "missing-soundpad-" + Guid.NewGuid().ToString("N"), PipeDirection.InOut);
            using var soundpad = new Soundpad(pipe) { ConnectionTimeout = 20 };
            var connected = 0;
            var disconnectedStatuses = 0;
            soundpad.Connected += (_, _) => connected++;
            soundpad.StatusChanged += (_, _) =>
            {
                if (soundpad.ConnectionStatus == ConnectionStatus.Disconnected) disconnectedStatuses++;
            };

            await Assert.ThrowsAsync<TimeoutException>(() => soundpad.ConnectAsync());

            Assert.Equal(0, connected);
            Assert.Equal(1, disconnectedStatuses);
            Assert.Equal(ConnectionStatus.Disconnected, soundpad.ConnectionStatus);
        }
    }
}
