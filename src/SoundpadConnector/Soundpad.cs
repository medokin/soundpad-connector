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

        private readonly NamedPipeClientStream _pipe;

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

        public Soundpad() : this(new NamedPipeClientStream(".", PipeName, PipeDirection.InOut)) {
        }

        internal Soundpad(NamedPipeClientStream pipe) {
            Connected += OnConnected;
            Disconnected += OnDisconnected;
            Connecting += OnConnecting;
            StatusChanged += OnStatusChanged;

            _pipe = pipe;
        }

        /// <inheritdoc />
        public void Dispose() {
            _pipe?.Dispose();
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
            while (true) {
                Connecting?.Invoke(this, EventArgs.Empty);
                try {
                    await _pipe.ConnectAsync(ConnectionTimeout);
                } catch (Exception e) {
                    if (!AutoReconnect) {
                        Disconnected?.Invoke(this, new OnDisconnectedEventArgs { Exception = e });
                        throw;
                    }
                    ConnectionStatus = ConnectionStatus.Disconnected;
                    StatusChanged?.Invoke(this, EventArgs.Empty);
                    await Task.Delay(ReconnectInterval);
                    continue;
                }

                Connected?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        /// <summary>
        ///     Disconnects from Soundpad
        /// </summary>
        public void Disconnect() {
            if (_pipe.IsConnected)
                _pipe.Close();

            _pipe.Dispose();

            Disconnected?.Invoke(this, new OnDisconnectedEventArgs());
        }

        private async Task<TResponse> Send<TResponse>(string request) where TResponse : IResponse, new() {
            await _mutex.WaitAsync();
            try {
                var buffer = Encoding.UTF8.GetBytes(request);

                await _pipe.WriteAsync(buffer, 0, buffer.Length);

                var messageMode = IsMessagePipe();
                if (messageMode) _pipe.ReadMode = PipeTransmissionMode.Message;

                var responseBuffer = new byte[messageMode ? 4096 : _pipe.OutBufferSize];
                string responseText;
                using (var responseBytes = new MemoryStream()) {
                    do {
                        var count = await _pipe.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                        if (count == 0) throw new EndOfStreamException("Soundpad disconnected before returning a response.");
                        responseBytes.Write(responseBuffer, 0, count);
                    } while (messageMode && !_pipe.IsMessageComplete);

                    responseText = Encoding.UTF8.GetString(responseBytes.ToArray()).TrimEnd('\0');
                }

                var response = new TResponse();
                response.Parse(responseText);

                return response;
            } finally {
                _mutex.Release();
            }
        }

        private async void DoPoll() {
            while (ConnectionStatus == ConnectionStatus.Connected) {
                try {
                    await IsAlive();
                } catch (Exception e) {
                    Disconnected?.Invoke(this, new OnDisconnectedEventArgs {
                        Exception = e
                    });
                }

                await Task.Delay(PollingInterval);
            }
        }

        private bool IsMessagePipe() {
            // NamedPipeClientStream.TransmissionMode can return its cached byte mode instead of the server's type.
            if (!GetNamedPipeInfo(_pipe.SafePipeHandle, out var flags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                throw new IOException("Unable to inspect the Soundpad pipe.", new Win32Exception(Marshal.GetLastWin32Error()));

            return (flags & MessagePipeType) != 0;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeInfo(SafePipeHandle pipe, out uint flags, IntPtr outBufferSize, IntPtr inBufferSize, IntPtr maxInstances);

        #region Events

        private void OnStatusChanged(object sender, EventArgs e) {
        }

        private async void OnConnecting(object sender, EventArgs eventArgs) {
            await Task.Delay(0);

            ConnectionStatus = ConnectionStatus.Connecting;
            StatusChanged?.Invoke(this, eventArgs);
        }

        private async void OnConnected(object sender, EventArgs eventArgs) {
            await Task.Delay(0);

            ConnectionStatus = ConnectionStatus.Connected;
            StatusChanged?.Invoke(this, eventArgs);

            DoPoll();
        }

        private async void OnDisconnected(object sender, OnDisconnectedEventArgs eventArgs) {
            ConnectionStatus = ConnectionStatus.Disconnected;
            StatusChanged?.Invoke(this, eventArgs);

            if (AutoReconnect && eventArgs.Exception != null) await ConnectAsync();
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
