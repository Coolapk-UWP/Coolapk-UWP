using System;

namespace BCrypt.Net
{
    internal static class HashParser
    {
        private static readonly HashFormatDescriptor OldFormatDescriptor = new HashFormatDescriptor(versionLength: 1);
        private static readonly HashFormatDescriptor NewFormatDescriptor = new HashFormatDescriptor(versionLength: 2);

        public static HashInformation GetHashInformation(string hash)
        {
            if (!IsValidHash(hash, out HashFormatDescriptor format))
            {
                ThrowInvalidHashFormat();
            }

            return new HashInformation(
                hash.Substring(0, format.SettingLength),
                hash.Substring(1, format.VersionLength),
                hash.Substring(format.WorkfactorOffset, 2),
                hash.Substring(format.HashOffset));
        }

        public static int GetWorkFactor(string hash)
        {
            if (!IsValidHash(hash, out HashFormatDescriptor format))
            {
                ThrowInvalidHashFormat();
            }

            int offset = format.WorkfactorOffset;

            return (10 * (hash[offset] - '0')) + (hash[offset + 1] - '0');
        }

        private static bool IsValidHash(string hash, out HashFormatDescriptor format)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            if (hash.Length != 59 && hash.Length != 60)
            {
                // Incorrect full hash length
                format = null;
                return false;
            }

            if (!hash.StartsWith("$2"))
            {
                // Not a bcrypt hash
                format = null;
                return false;
            }

            // Validate version
            int offset = 2;
            if (IsValidBCryptVersionChar(hash[offset]))
            {
                offset++;
                format = NewFormatDescriptor;
            }
            else
            {
                format = OldFormatDescriptor;
            }

            if (hash[offset++] != '$')
            {
                format = null;
                return false;
            }

            // Validate workfactor
            if (!IsAsciiNumeric(hash[offset++])
                || !IsAsciiNumeric(hash[offset++]))
            {
                format = null;
                return false;
            }

            if (hash[offset++] != '$')
            {
                format = null;
                return false;
            }

            // Validate hash
            for (int i = offset; i < hash.Length; ++i)
            {
                if (!IsValidBCryptBase64Char(hash[i]))
                {
                    format = null;
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidBCryptVersionChar(char value)
        {
            return value == 'a'
                   || value == 'b'
                   || value == 'x'
                   || value == 'y';
        }

        private static bool IsValidBCryptBase64Char(char value)
        {
            // Ordered by ascending ASCII value
            return value == '.'
                   || value == '/'
                   || (value >= '0' && value <= '9')
                   || (value >= 'A' && value <= 'Z')
                   || (value >= 'a' && value <= 'z');
        }

        private static bool IsAsciiNumeric(char value)
        {
            return value >= '0' && value <= '9';
        }

        private static void ThrowInvalidHashFormat()
        {
            throw new SaltParseException("Invalid Hash Format");
        }

        private class HashFormatDescriptor
        {
            public HashFormatDescriptor(int versionLength)
            {
                VersionLength = versionLength;
                WorkfactorOffset = 1 + VersionLength + 1;
                SettingLength = WorkfactorOffset + 2;
                HashOffset = SettingLength + 1;
            }

            public int VersionLength { get; }

            public int WorkfactorOffset { get; }

            public int SettingLength { get; }

            public int HashOffset { get; }
        }
    }
}
