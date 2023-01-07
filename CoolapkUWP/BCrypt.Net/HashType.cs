namespace BCrypt.Net
{
    /// <summary>
    ///     Type of SHA implementation to use
    ///     Keys will be hashed, then base64 encoded before being passed to crypt.
    ///     Unless legacy is selected in which case simply SHA384 hashed.
    /// </summary>
    public enum HashType
    {
        None = -1,
        SHA256 = 0,
        SHA384 = 1,
        SHA512 = 2
    }
}
