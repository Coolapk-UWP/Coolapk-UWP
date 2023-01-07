using System.Diagnostics.CodeAnalysis;

namespace BCrypt.Net
{
    /// <summary>
    ///     HashInformation : A value object that contains the results of interrogating a hash
    ///     Namely its settings (2a$10 for example); version (2a); workfactor (log rounds), and the raw hash returned
    /// </summary>
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public sealed class HashInformation
    {
        /// <summary>Constructor. </summary>
        /// <param name="settings">The message.</param>
        /// <param name="version">The message.</param>
        /// <param name="workFactor">The message.</param>
        /// <param name="rawHash">The message.</param>
        internal HashInformation(string settings, string version, string workFactor, string rawHash)
        {
            Settings = settings;
            Version = version;
            WorkFactor = workFactor;
            RawHash = rawHash;
        }

        /// <summary>
        ///     Settings string
        /// </summary>
        public string Settings { get; private set; }

        /// <summary>
        ///     Hash Version
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        ///     log rounds used / workfactor
        /// </summary>
        public string WorkFactor { get; private set; }

        /// <summary>
        ///     Raw Hash
        /// </summary>
        public string RawHash { get; private set; }
    }
}
