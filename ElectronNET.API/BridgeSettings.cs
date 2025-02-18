﻿namespace ElectronNET.API
{
    /// <summary>
    /// 
    /// </summary>
    public static class BridgeSettings
    {
        /// <summary>
        /// Gets the socket port.
        /// </summary>
        /// <value>
        /// The socket port.
        /// </value>
        public static string SocketPort { get; internal set; }

        /// <summary>
        /// Gets the web port.
        /// </summary>
        /// <value>
        /// The web port.
        /// </value>
        public static string WebPort { get; internal set; }

        /// <summary>
        /// Indicates if the instance was started using the vs-debug arguments
        /// </summary>
        public static bool IsVsDebugEnabled { get; internal set; } = false;

    }
}
