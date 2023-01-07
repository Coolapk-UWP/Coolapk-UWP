using System;

namespace BCrypt.Net
{
    /// <summary>Exception for signalling parse errors during salt checks. </summary>
    public class SaltParseException : Exception
    {
        /// <summary>Default constructor. </summary>
        public SaltParseException()
        {
        }

        /// <summary>Initializes a new instance of <see cref="SaltParseException" />.</summary>
        /// <param name="message">The message.</param>
        public SaltParseException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SaltParseException" />.</summary>
        /// <param name="message">       The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SaltParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
