using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Concurrent;
using Xunit;

namespace SoundpadConnector.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public async Task ReconnectsAfterExplicitDisconnect()
        {
            var name = "soundpad-reconnect-" + Guid.NewGuid().ToString("N");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous));

            var accepting = server.WaitForConnectionAsync(timeout.Token);
            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;
            soundpad.Disconnect();
            server.Disconnect();
            accepting = server.WaitForConnectionAsync(timeout.Token);
            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;

            Assert.True(server.IsConnected);
        }

        [Fact]
        public async Task DisconnectCancelsPendingReconnect()
        {
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", "missing-soundpad-" + Guid.NewGuid().ToString("N"), PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                AutoReconnect = true, ConnectionTimeout = 20, ReconnectInterval = 10
            };
            var connecting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            soundpad.Connecting += (_, _) => connecting.TrySetResult();
            var connection = soundpad.ConnectAsync();
            try
            {
                await connecting.Task.WaitAsync(TimeSpan.FromSeconds(2));
                soundpad.Disconnect();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.WaitAsync(TimeSpan.FromSeconds(2)));
                Assert.Equal(ConnectionStatus.Disconnected, soundpad.ConnectionStatus);
            }
            finally
            {
                soundpad.AutoReconnect = false;
                soundpad.Dispose();
                try { await connection; } catch (Exception) { }
            }
        }

        [Fact]
        public async Task DisconnectDoesNotReportCanceledPollAsAnotherFailure()
        {
            var name = "soundpad-poll-" + Guid.NewGuid().ToString("N");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous)) { PollingInterval = 10 };
            var disconnected = 0;
            var errors = new ConcurrentQueue<Exception>();
            soundpad.Disconnected += (_, args) =>
            {
                if (args.Exception != null) errors.Enqueue(args.Exception);
                Interlocked.Increment(ref disconnected);
            };
            var accepting = server.WaitForConnectionAsync(timeout.Token);
            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;
            var request = new byte[Encoding.UTF8.GetByteCount("IsAlive()")];
            await server.ReadExactlyAsync(request, timeout.Token);
            Assert.Equal("IsAlive()", Encoding.UTF8.GetString(request));

            soundpad.Disconnect();
            await Task.Delay(100, timeout.Token);

            Assert.Empty(errors);
            Assert.Equal(1, Volatile.Read(ref disconnected));
            Assert.Equal(ConnectionStatus.Disconnected, soundpad.ConnectionStatus);
        }

        [Fact]
        public async Task ReconnectsAfterBrokenPipeWhenEnabled()
        {
            var name = "soundpad-auto-" + Guid.NewGuid().ToString("N");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                AutoReconnect = true, ReconnectInterval = 10, PollingInterval = 10
            };
            var connected = 0;
            var reconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            soundpad.Connected += (_, _) =>
            {
                if (Interlocked.Increment(ref connected) == 2) reconnected.TrySetResult();
            };
            var accepting = server.WaitForConnectionAsync(timeout.Token);
            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;
            await server.ReadExactlyAsync(new byte[9], timeout.Token);
            server.Disconnect();

            await server.WaitForConnectionAsync(timeout.Token);
            await server.ReadExactlyAsync(new byte[9], timeout.Token);

            await reconnected.Task.WaitAsync(timeout.Token);
            Assert.Equal(2, Volatile.Read(ref connected));
            Assert.Equal(ConnectionStatus.Connected, soundpad.ConnectionStatus);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExplicitDisconnectPreventsFurtherAutomaticReconnect(bool autoReconnect)
        {
            var name = "soundpad-no-retry-" + Guid.NewGuid().ToString("N");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                AutoReconnect = autoReconnect, ReconnectInterval = 200, PollingInterval = 10
            };
            var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var connecting = 0;
            soundpad.Connecting += (_, _) => Interlocked.Increment(ref connecting);
            soundpad.Disconnected += (_, _) => disconnected.TrySetResult();
            var accepting = server.WaitForConnectionAsync(timeout.Token);
            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;
            await server.ReadExactlyAsync(new byte[9], timeout.Token);
            server.Disconnect();
            await disconnected.Task.WaitAsync(timeout.Token);

            soundpad.Disconnect();
            await Task.Delay(350, timeout.Token);

            Assert.Equal(1, Volatile.Read(ref connecting));
            Assert.Equal(ConnectionStatus.Disconnected, soundpad.ConnectionStatus);
        }

        [Fact]
        public async Task CanDisposeFromConnectedStatusHandler()
        {
            var name = "soundpad-status-" + Guid.NewGuid().ToString("N");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var soundpad = new Soundpad(() => new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous));
            soundpad.StatusChanged += (_, _) =>
            {
                if (soundpad.ConnectionStatus == ConnectionStatus.Connected) soundpad.Dispose();
            };
            var accepting = server.WaitForConnectionAsync(timeout.Token);

            await soundpad.ConnectAsync().WaitAsync(timeout.Token);
            await accepting;

            Assert.Equal(ConnectionStatus.Disconnected, soundpad.ConnectionStatus);
        }

        [Fact]
        public async Task DisposedConnectorCannotStartConnecting()
        {
            var soundpad = new Soundpad(() => new NamedPipeClientStream(".", "missing-soundpad-" + Guid.NewGuid().ToString("N"), PipeDirection.InOut));
            var connecting = 0;
            soundpad.Connecting += (_, _) => connecting++;
            soundpad.Dispose();

            await Assert.ThrowsAsync<ObjectDisposedException>(() => soundpad.ConnectAsync());

            Assert.Equal(0, connecting);
        }

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
