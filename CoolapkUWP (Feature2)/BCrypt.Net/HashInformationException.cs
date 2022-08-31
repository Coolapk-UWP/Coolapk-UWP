using System;

namespace BCrypt.Net
{
    /// <summary>
    ///     Exception used to signal errors that occur during use of the hash information methods
    /// </summary>
    [Serializable]
    public sealed class HashInformationException : Exception
    {
        /// <summary>
        ///     Default Constructor
        /// </summary>
        public HashInformationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="HashInformationException" />.
        /// </summary>
        /// <param name="message"></param>
        public HashInformationException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="HashInformationException" />.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public HashInformationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
