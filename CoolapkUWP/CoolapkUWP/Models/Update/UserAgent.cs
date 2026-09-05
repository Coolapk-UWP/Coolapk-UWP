using Microsoft.Toolkit.Uwp.Helpers;
using System.Text.RegularExpressions;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace CoolapkUWP.Models.Update
{
    public sealed class UserAgent
    {
        public static UserAgent Default
        {
            get
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                return new UserAgent
                {
                    Header = $"Dalvik/2.1.0 (Windows NT {SystemInformation.Instance.OperatingSystemVersion.Major}.{SystemInformation.Instance.OperatingSystemVersion.Minor}; Win{(SystemInformation.Instance.OperatingSystemArchitecture.ToString().Contains("64") ? "64" : "32")}; {SystemInformation.Instance.OperatingSystemArchitecture.ToString().ToLower()}; WebView/3.0)",
                    Manufacturer = deviceInfo.SystemManufacturer,
                    ProductName = deviceInfo.SystemProductName,
                    FullProductName = deviceInfo.SystemSku ?? $"{deviceInfo.SystemManufacturer}_{deviceInfo.SystemProductName}".Replace(' ', '_'),
                    OSVersion = SystemInformation.Instance.OperatingSystemVersion.ToString()
                };
            }
        }

        public string Header { get; set; }
        public string Manufacturer { get; set; }
        public string ProductName { get; set; }
        public string FullProductName { get; set; }
        public string OSVersion { get; set; }

        public static UserAgent Parse(string line)
        {
            UserAgent result = new UserAgent();
            int index = line.IndexOf("(#Build;");
            if (index != -1)
            {
                result.Header = line.Substring(0, index).Trim();
                Match match = Regex.Match(line, @"\((#Build; .*?)\)");
                if (match.Success && match.Groups.Count >= 2)
                {
                    string device = match.Groups[1].Value;
                    string[] lines = device.Split(';');
                    if (lines.Length >= 5)
                    {
                        result.Manufacturer = lines[1].Trim();
                        result.ProductName = lines[2].Trim();
                        result.FullProductName = lines[3].Trim();
                        result.OSVersion = lines[4].Trim();
                    }
                }
            }
            return result;
        }

        public override string ToString() => $"{Header} (#Build; {Manufacturer}; {ProductName}; {FullProductName}; {OSVersion})";
    }
}
