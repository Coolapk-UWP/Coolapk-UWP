using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using System;
using System.Text;

namespace CoolapkUWP.Common
{
    /// <summary>
    /// Create a token for Coolapk.
    /// </summary>
    public class TokenCreator
    {
        /// <summary>
        /// The alphabet used for token generation.
        /// </summary>
        private const string alphabet = "./ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        /// <summary>
        /// The blob string used for token generation.
        /// </summary>
        private const string blob = "TTBUOFQsQ0ElLUMkWDEjQSEsUyEmLDNAUy1DPFYuIy0iMUM5IzBUJFctMykhMUQ1JS1DOSUxMzhQLVMwV00sIyhSMTQsWTFDRFEwQ0RVMSQkUS0jKSMtJDhYLVQkUjEzKSQsM0BVLTQwUi00NSMsM2BXLVMlJC1DNFlNLCQ0WTEzLSQxQ0EhLEMlIjEzKFcwMz0lLiNEVzEjPSYuNDkkLFMwVixTKFktQzhXLFMsWC4jLSYtU2BTTTA0LFEuMzhTLjMhIjFDMFMsI0BVLCQkUCwzJSIsIzBSMEM8UCxELSItI2BQLUQoUy0jLFMsIyUkMFQwVk0tRCxYMTMlJC40NSEwU0RQLFMhIi0kNSMwU2BQLjNgUywjOFEsNCxTLEMwWCxULSEtIyRTLDNBIy00JFZNMUMsUDBEJFgtNC0kLDQkWC1DMFMxRCkjLVM0VixTMFgxIy0mLVMxJS0zJFQxM2BQLSQ4WTBDMFMuI0UhTSwzPFgtRDBRMDNEUS0jOFYuJDklMSQ0UzAzNSMwRDkkMTMkVSxUMFQtRCxYMTNFIyxELFYsIzUhLiQ0V00xNCxVLCM0Vy4zJFUxNCxTLFQ4Vy4jYFYxJDUjLUMwUzFDPFIsM0ElLDMsVDA0OSMuNCkmLEQkVixEMSJNLTMoUiwzLFctU0RQLiQwVTFDJSUwUykjMUMhIy0zISUsI0EjMTMwUS1UNFcxRDElLUQtJS4kNFIsIzhZTTBTYFEwQyRYLSMlIS40LFgsVDRZLUQoWTBDQFYtRCxWLjNEVDBDMFktNDUlLFQ4VS1UJFgwMzRWLDQ4UU0wQy0mLUQ4WS1TMSMsUyUiMUNAUSxULFYuI0RULiMsWDFELFMxI2BQLVM0WS4kOFcwM0RXLVMoWC0jKFExLCM0VixTNFAtRDRWLUQlJC1TJFAwNCxgYA";

        /// <summary>
        /// Gets or sets the default device code.
        /// </summary>
        public static string DeviceCode { get; protected set; } = SettingsHelper.Get<DeviceInfo>(SettingsHelper.DeviceInfo).CreateDeviceCode();

        /// <summary>
        /// Gets or sets the default API version.
        /// </summary>
        public static APIVersion APIVersion { get; protected set; } = APIVersion.Create(SettingsHelper.Get<APIVersions>(SettingsHelper.APIVersion));

        /// <summary>
        /// The token version.
        /// </summary>
        private readonly TokenVersion TokenVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCreator"/> class.
        /// </summary>
        /// <param name="version">The token version.</param>
        public TokenCreator(TokenVersion version = TokenVersion.TokenV2) => TokenVersion = version;

        /// <summary>
        /// <see cref="GetToken"/> Generate a token with random device info.
        /// </summary>
        public string GetToken()
        {
            switch (TokenVersion)
            {
                case TokenVersion.TokenV1:
                    return GetCoolapkAppToken(DeviceCode);
                case TokenVersion.TokenV3:
                    return GetTokenWithDeviceCodeAndVersionCode(DeviceCode, APIVersion);
                default:
                case TokenVersion.TokenV2:
                    return GetTokenWithDeviceCode(DeviceCode);
            }
        }

        /// <summary>
        /// Generate a token v1 with your device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns>The generated token.</returns>
        private static string GetTokenWithDeviceCode(string deviceCode)
        {
            string timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string base64TimeStamp = timeStamp.GetBase64();
            string md5TimeStamp = timeStamp.GetMD5();
            string md5DeviceCode = deviceCode.GetMD5();

            string token = $"token://com.coolapk.market/dcf01e569c1e3db93a3d0fcf191a622c?{md5TimeStamp}${md5DeviceCode}&com.coolapk.market";
            string base64Token = token.GetBase64();
            string md5Base64Token = base64Token.GetMD5();
            string md5Token = token.GetMD5();

            string bcryptSalt = $"{$"$2y$10${base64TimeStamp}/{md5Token}".Substring(0, 31)}u";
            string bcryptResult = BCrypt.Net.BCrypt.HashPassword(md5Base64Token, bcryptSalt);

            string appToken = $"v2{bcryptResult.GetBase64()}";
            return appToken;
        }

        /// <summary>
        /// Generate a token v2 with your device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns>The generated token.</returns>
        private static string GetCoolapkAppToken(string deviceCode)
        {
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string hex_timeStamp = $"0x{timeStamp:x}";

            // 时间戳加密
            string md5_timeStamp = timeStamp.ToString().GetMD5();
            string md5_deviceCode = deviceCode.GetMD5();

            string token = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_timeStamp}${md5_deviceCode}&com.coolapk.market";
            string md5_token = token.GetBase64(true).GetMD5();

            string appToken = $"{md5_token}{md5_deviceCode}{hex_timeStamp}";
            return appToken;
        }

        /// <summary>
        /// Generate a token v3 with your device code and version code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <param name="version">The version with version code.</param>
        /// <returns>The generated token.</returns>
        private static string GetTokenWithDeviceCodeAndVersionCode(string deviceCode, APIVersion version)
        {
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            int versionCode = version.VersionCode;
            int offset = checked(((int)((timeStamp + versionCode) % 100) * 4) + 128);
            string template = blob.Substring(offset, 128);
            string internalData = Encoding.UTF8.GetString(Convert.FromBase64String(template));

            string md5DeviceCode = deviceCode.GetMD5();

            string input = $"com.coolapk.market&{internalData}&{md5DeviceCode}&{timeStamp}&{versionCode}";
            string password = input.GetBase64(true).GetMD5();
            string saltBody = $"{timeStamp:x}/{input.GetMD5()}";
            string saltBase64 = saltBody.GetBase64();
            string salt22 = saltBase64.Substring(0, 22);
            int lastIndex = alphabet.IndexOf(salt22[salt22.Length - 1]);
            if (lastIndex < 0) { throw new InvalidOperationException($"Invalid bcrypt salt character: {salt22[salt22.Length - 1]}"); }

            string bcryptSalt = $"$2y$04${salt22.Substring(0, salt22.Length - 1)}{alphabet[lastIndex & 0x30]}";
            string bcryptResult = BCrypt.Net.BCrypt.HashPassword(password, bcryptSalt);
            return $"v3{bcryptResult.GetBase64()}";
        }

        /// <summary>
        /// Update the device info.
        /// </summary>
        /// <param name="deviceInfo">The device info to update.</param>
        public static void UpdateDeviceInfo(DeviceInfo deviceInfo) => DeviceCode = deviceInfo.CreateDeviceCode();

        /// <summary>
        /// Update the API version.
        /// </summary>
        /// <param name="apiVersion">The API version to update.</param>
        public static void UpdateAPIVersion(APIVersions apiVersion) => APIVersion = APIVersion.Create(apiVersion);

        /// <inheritdoc/>
        public override string ToString() => GetToken();
    }

    /// <summary>
    /// The versions of token.
    /// </summary>
    public enum TokenVersion
    {
        TokenV1 = 1,
        TokenV2,
        TokenV3
    }
}
