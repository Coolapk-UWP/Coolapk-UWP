using System;

namespace BCrypt.Net
{
    /// <inheritdoc />
    /// <summary>Exception for signalling hash validation errors. </summary>
    [Serializable]
    public class BcryptAuthenticationException : Exception
    {
        /// <inheritdoc />
        /// <summary>Default constructor. </summary>
        public BcryptAuthenticationException()
        {
        }

        /// <inheritdoc />
        /// <summary>Initializes a new instance of <see cref="T:BCrypt.Net.BcryptAuthenticationException" />.</summary>
        /// <param name="message">The message.</param>
        public BcryptAuthenticationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>Initializes a new instance of <see cref="T:BCrypt.Net.BcryptAuthenticationException" />.</summary>
        /// <param name="message">       The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BcryptAuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
