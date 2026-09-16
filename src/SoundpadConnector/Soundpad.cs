using System;
using System.IO.Pipes;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoundpadConnector.Response;

namespace SoundpadConnector {
    /// <summary>
    /// Main SoundpadConnector class.
    ///  Contains all methods for connection handling and requests
    /// </summary>
    public partial class Soundpad : IDisposable {
        private const string PipeName = "sp_remote_control";
        private const uint MessagePipeType = 4;

        private NamedPipeClientStream _pipe;
        private readonly Func<NamedPipeClientStream> _pipeFactory;
        private readonly object _lifecycleLock = new object();
        private readonly SemaphoreSlim _connectMutex = new SemaphoreSlim(1);
        private CancellationTokenSource _connectionCancellation = new CancellationTokenSource();
        private bool _pipeClosed;
        private bool _disposed;

        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        /// <summary>
        ///     Tries to reconnect when ungracefully closed
        /// </summary>
        public bool AutoReconnect = false;

        /// <summary>
        ///     Defines the connection timeout to Soundpad
        /// </summary>
        public int ConnectionTimeout = 1000;

        /// <summary>
        ///     Defines polling interval to check the connection to Soundpad
        /// </summary>
        public int PollingInterval = 1000;

        /// <summary>
        ///     Defines the interval between AutoReconnect retries
        /// </summary>
        public int ReconnectInterval = 100;

        /// <summary>
        ///     Indicates the current connection state
        /// </summary>
        public ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

        public Soundpad() : this(() => new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous)) {
        }

        internal Soundpad(NamedPipeClientStream pipe) : this(() => pipe) {
        }

        internal Soundpad(Func<NamedPipeClientStream> pipeFactory) {
            Connected += OnConnected;
            Disconnected += OnDisconnected;
            Connecting += OnConnecting;
            StatusChanged += OnStatusChanged;

            _pipeFactory = pipeFactory;
            _pipe = pipeFactory();
        }

        /// <inheritdoc />
        public void Dispose() {
            lock (_lifecycleLock) {
                if (_disposed) return;
                _disposed = true;
                Disconnect();
                _connectionCancellation.Dispose();
            }
        }

        /// <summary>
        ///     Fires when the status changes
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        ///     Fires when connection to Soundpad is established
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        ///     Fires when tries reconnect to Soundpad
        /// </summary>
        public event EventHandler Connecting;

        /// <summary>
        ///     Fires when connection to Soundpad is closed
        /// </summary>
        public event EventHandler<OnDisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     Tries to establich a connection to Soundpad
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync() {
            CancellationToken token;
            lock (_lifecycleLock) {
                if (_disposed) throw new ObjectDisposedException(nameof(Soundpad));
                if (_connectionCancellation.IsCancellationRequested) {
                    _connectionCancellation.Dispose();
                    _connectionCancellation = new CancellationTokenSource();
                }
                token = _connectionCancellation.Token;
            }

            await _connectMutex.WaitAsync(token);
            try {
                token.ThrowIfCancellationRequested();
                if (_pipe.IsConnected) return;
                await ConnectCoreAsync(token);
            } finally {
                _connectMutex.Release();
            }
        }

        private async Task ConnectCoreAsync(CancellationToken token) {
            while (true) {
                NamedPipeClientStream pipe;
                lock (_lifecycleLock) {
                    token.ThrowIfCancellationRequested();
                    if (_pipeClosed) {
                        _pipe = _pipeFactory();
                        _pipeClosed = false;
                    }
                    pipe = _pipe;
                }
                Connecting?.Invoke(this, EventArgs.Empty);
                try {
                    await pipe.ConnectAsync(ConnectionTimeout, token);
                } catch (Exception e) {
                    token.ThrowIfCancellationRequested();
                    if (!AutoReconnect) {
                        Disconnected?.Invoke(this, new OnDisconnectedEventArgs { Exception = e });
                        throw;
                    }
                    ConnectionStatus = ConnectionStatus.Disconnected;
                    StatusChanged?.Invoke(this, EventArgs.Empty);
                    await Task.Delay(ReconnectInterval, token);
                    continue;
                }

                lock (_lifecycleLock) {
                    token.ThrowIfCancellationRequested();
                    Connected?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
        }

        /// <summary>
        ///     Disconnects from Soundpad
        /// </summary>
        public void Disconnect() {
            lock (_lifecycleLock) {
                if (_pipeClosed && _connectionCancellation.IsCancellationRequested) return;
                _connectionCancellation.Cancel();
                _pipe.Dispose();
                _pipeClosed = true;
                Disconnected?.Invoke(this, new OnDisconnectedEventArgs());
            }
        }

        private async Task<TResponse> Send<TResponse>(string request, CancellationToken token = default) where TResponse : IResponse, new() {
            await _mutex.WaitAsync(token);
            try {
                var pipe = _pipe;
                var buffer = Encoding.UTF8.GetBytes(request);

                await pipe.WriteAsync(buffer, 0, buffer.Length, token);

                var messageMode = IsMessagePipe(pipe);
                if (messageMode) pipe.ReadMode = PipeTransmissionMode.Message;

                var responseBuffer = new byte[messageMode ? 4096 : Math.Max(4096, pipe.OutBufferSize)];
                string responseText;
                using (var responseBytes = new MemoryStream()) {
                    do {
                        var count = await pipe.ReadAsync(responseBuffer, 0, responseBuffer.Length, token);
                        if (count == 0) throw new EndOfStreamException("Soundpad disconnected before returning a response.");
                        responseBytes.Write(responseBuffer, 0, count);
                    } while (messageMode && !pipe.IsMessageComplete);

                    responseText = Encoding.UTF8.GetString(responseBytes.ToArray()).TrimEnd('\0');
                }

                var response = new TResponse();
                response.Parse(responseText);

                return response;
            } finally {
                _mutex.Release();
            }
        }

        private async Task DoPollAsync(NamedPipeClientStream pipe, CancellationToken token) {
            while (!token.IsCancellationRequested) {
                try {
                    await Send<NoContentResponse>("IsAlive()", token);
                    await Task.Delay(PollingInterval, token);
                } catch (Exception) when (token.IsCancellationRequested) {
                    return;
                } catch (Exception e) {
                    try {
                        await _connectMutex.WaitAsync(token);
                        try {
                            lock (_lifecycleLock) {
                                token.ThrowIfCancellationRequested();
                                if (_pipe != pipe) return;
                                pipe.Dispose();
                                _pipeClosed = true;
                                Disconnected?.Invoke(this, new OnDisconnectedEventArgs { Exception = e });
                            }
                            if (!AutoReconnect) return;
                            await Task.Delay(ReconnectInterval, token);
                            await ConnectCoreAsync(token);
                        } finally {
                            _connectMutex.Release();
                        }
                    } catch (OperationCanceledException) when (token.IsCancellationRequested) {
                    }
                    return;
                }
            }
        }

        private static bool IsMessagePipe(NamedPipeClientStream pipe) {
            // NamedPipeClientStream.TransmissionMode can return its cached byte mode instead of the server's type.
            if (!GetNamedPipeInfo(pipe.SafePipeHandle, out var flags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                throw new IOException("Unable to inspect the Soundpad pipe.", new Win32Exception(Marshal.GetLastWin32Error()));

            return (flags & MessagePipeType) != 0;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeInfo(SafePipeHandle pipe, out uint flags, IntPtr outBufferSize, IntPtr inBufferSize, IntPtr maxInstances);

        #region Events

        private void OnStatusChanged(object sender, EventArgs e) {
        }

        private void OnConnecting(object sender, EventArgs eventArgs) {
            ConnectionStatus = ConnectionStatus.Connecting;
            StatusChanged?.Invoke(this, eventArgs);
        }

        private void OnConnected(object sender, EventArgs eventArgs) {
            var pipe = _pipe;
            var token = _connectionCancellation.Token;
            ConnectionStatus = ConnectionStatus.Connected;
            StatusChanged?.Invoke(this, eventArgs);

            if (!token.IsCancellationRequested) _ = DoPollAsync(pipe, token);
        }

        private void OnDisconnected(object sender, OnDisconnectedEventArgs eventArgs) {
            ConnectionStatus = ConnectionStatus.Disconnected;
            StatusChanged?.Invoke(this, eventArgs);

        }

        #endregion

        /// <summary>
        /// Extented EventArgs for disconected event
        /// </summary>
        public class OnDisconnectedEventArgs : EventArgs {
            public Exception Exception { get; set; }
        }
    }
}
