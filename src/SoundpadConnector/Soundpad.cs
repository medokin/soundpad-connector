using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoundpadConnector.Response;

namespace SoundpadConnector {
    public partial class Soundpad : IDisposable {
        private const string PipeName = "sp_remote_control";

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

        public Soundpad() {
            Connected += OnConnected;
            Disconnected += OnDisconnected;
            Connecting += OnConnecting;
            StatusChanged += OnStatusChanged;

            _pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
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
            Connecting?.Invoke(this, EventArgs.Empty);
            try {
                await _pipe.ConnectAsync(ConnectionTimeout);
            } catch {
                if (AutoReconnect) {
                    await Task.Delay(ReconnectInterval);
                    await ConnectAsync();

                    return;
                }
            }

            Connected?.Invoke(this, EventArgs.Empty);
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

                var responseBuffer = new byte[_pipe.OutBufferSize];
                await _pipe.ReadAsync(responseBuffer, 0, responseBuffer.Length);

                var responseText = Encoding.UTF8.GetString(responseBuffer).TrimEnd('\0');

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

            if (eventArgs.Exception != null) await ConnectAsync();
        }

        #endregion

        public class OnDisconnectedEventArgs : EventArgs {
            public Exception Exception { get; set; }
        }
    }
}