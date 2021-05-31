using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Helpers
{
    internal static class EmojiHelper
    {
        private static readonly ResourceLoader emojiIdLoader = ResourceLoader.GetForViewIndependentUse("EmojiId");
        private static readonly ResourceLoader oldEmojiIdLoader = ResourceLoader.GetForViewIndependentUse("OldEmojiId");

        [DebuggerStepThrough]
        public static bool Contains(string key, bool useOldEmoji = false)
        {
            string _key = key[0] == '#' ? key.Substring(1) : key;
            try
            {
                return useOldEmoji ? !string.IsNullOrEmpty(oldEmojiIdLoader.GetString(_key)) : !string.IsNullOrEmpty(emojiIdLoader.GetString(_key));
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        [DebuggerStepThrough]
        public static Uri Get(string key, bool useOldEmoji = false)
        {
            string _key = key[0] == '#' ? key.Substring(1) : key;
            try
            {
                string id = useOldEmoji ? oldEmojiIdLoader.GetString(_key) : emojiIdLoader.GetString(_key);
                return string.IsNullOrEmpty(id) ? ImageCacheHelper.NoPic.UriSource : new Uri($"ms-appx:///Assets/Emoji/{id}.png");
            }
            catch (ArgumentException)
            {
                return ImageCacheHelper.NoPic.UriSource;
            }
        }
    }
}