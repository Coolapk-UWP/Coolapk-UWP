using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Globalization;
using Windows.System.UserProfile;

namespace CoolapkUWP.Helpers
{
    public static class LanguageHelper
    {
        public const string AutoLanguageCode = "auto";
        public const string FallbackLanguageCode = "zh-CN";

        public static readonly List<string> SupportLanguages = new List<string>()
        {
            "zh-CN",
        };

        private static readonly List<string> SupportLanguageCodes = new List<string>()
        {
            "zh-Hans, zh-cn, zh-hans-cn, zh-sg, zh-hans-sg",
        };

        public static readonly List<CultureInfo> SupportCultures = SupportLanguages.Select(x => new CultureInfo(x)).ToList();

        public static int FindIndexFromSupportLanguageCodes(string language) => SupportLanguageCodes.FindIndex(code => code.ToLowerInvariant().Split(',', ' ').Contains(language.ToLowerInvariant()));

        public static string GetCurrentLanguage()
        {
            IReadOnlyList<string> languages = GlobalizationPreferences.Languages;
            foreach (string language in languages)
            {
                int temp = FindIndexFromSupportLanguageCodes(language);
                if (temp != -1)
                {
                    return SupportLanguages[temp];
                }
            }
            return FallbackLanguageCode;
        }

        public static string GetPrimaryLanguage()
        {
            string language = ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrWhiteSpace(language)) { return GetCurrentLanguage(); }
            int temp = FindIndexFromSupportLanguageCodes(language);
            return temp == -1 ? FallbackLanguageCode : SupportLanguages[temp];
        }
    }
}
