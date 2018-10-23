using System;
using System.Collections.Generic;
using System.Text;

namespace SoundpadConnector {
    /// <summary>
    ///     Indicated the connection status to Soundpad
    /// </summary>
    public enum ConnectionStatus {
        /// <summary>
        ///     Connected to Soundpad
        /// </summary>
        Connected,
        /// <summary>
        ///     Disconnected from Soundpad
        /// </summary>
        Disconnected,
        /// <summary>
        ///     Connecting or reconnecting to Soundpad
        /// </summary>
        Connecting
    }
}
