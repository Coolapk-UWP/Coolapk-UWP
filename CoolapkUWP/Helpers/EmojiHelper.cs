using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Helpers
{
    internal static class EmojiHelper
    {
        private static readonly ResourceLoader emojiIdLoader = ResourceLoader.GetForViewIndependentUse("EmojiId");
        private static readonly ResourceLoader oldEmojiIdLoader = ResourceLoader.GetForViewIndependentUse("OldEmojiId");

        [DebuggerNonUserCode]
        public static bool Contains(string key, bool useOldEmoji = false)
        {
            try
            {
                if (useOldEmoji)
                {
                    return !string.IsNullOrEmpty(oldEmojiIdLoader.GetString(key));
                }
                else
                {
                    return !string.IsNullOrEmpty(emojiIdLoader.GetString(key));
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        [DebuggerNonUserCode]
        public static Uri Get(string key, bool useOldEmoji = false)
        {
            try
            {
                string id = useOldEmoji ? oldEmojiIdLoader.GetString(key) : emojiIdLoader.GetString(key);
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