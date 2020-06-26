using System;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Helpers
{
    [System.Diagnostics.DebuggerNonUserCode]
    internal static class EmojiHelper
    {
        private static readonly ResourceLoader emojiIdLoader = ResourceLoader.GetForViewIndependentUse("EmojiId");
        private static readonly ResourceLoader oldEmojiIdLoader = ResourceLoader.GetForViewIndependentUse("OldEmojiId");

        public static bool Contains(string key, bool useOldEmoji = false)
        {
            var _key = key[0] == '#' ? key.Substring(1) : key;
            try
            {
                if (useOldEmoji)
                {
                    return !string.IsNullOrEmpty(oldEmojiIdLoader.GetString(_key));
                }
                else
                {
                    return !string.IsNullOrEmpty(emojiIdLoader.GetString(_key));
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static Uri Get(string key, bool useOldEmoji = false)
        {
            var _key = key[0] == '#' ? key.Substring(1) : key;
            try
            {
                string id = useOldEmoji ? oldEmojiIdLoader.GetString(_key) : emojiIdLoader.GetString(_key);
                if (string.IsNullOrEmpty(id))
                {
                    return ImageCacheHelper.NoPic.UriSource;
                }
                else
                {
                    return new Uri($"ms-appx:///Assets/Emoji/{id}.png");
                }
            }
            catch (ArgumentException)
            {
                return ImageCacheHelper.NoPic.UriSource;
            }
        }
    }
}