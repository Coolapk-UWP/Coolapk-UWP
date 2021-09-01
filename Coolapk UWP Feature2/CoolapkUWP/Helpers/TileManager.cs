using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace CoolapkUWP.Data
{
    internal static class TileManager
    {
        public static void SetBadgeNumber(string badgeGlyphValue)
        {
            // Get the blank badge XML payload for a badge number
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        /// <param name="nums">顺序 follow, Message, atme, atcommentme, commentme, feedlike</param>
        public static void SetTile(params double[] nums)
        {
            int num = 0;
            string[] s = new string[6];
            for (int i = 0; i < nums.Length; i++)
            {
                if (nums[i] > 0)
                {
                    num++;
                    switch (i)
                    {
                        case 0: s[i] = $"新关注:{nums[i]}"; break;
                        case 1: s[i] = $"新私信:{nums[i]}"; break;
                        case 2: s[i] = $"@我的动态:{nums[i]}"; break;
                        case 3: s[i] = $"@我的评论:{nums[i]}"; break;
                        case 4: s[i] = $"新回复:{nums[i]}"; break;
                        case 5: s[i] = $"收到的赞:{nums[i]}"; break;
                        default: break;
                    }
                }
            }

            TileUpdater tileUpdateManeger = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdateManeger.Clear();
            if (num != 0)
            {
                tileUpdateManeger.EnableNotificationQueue(true);
                if (num <= 3)
                {
                    string message = string.Empty;
                    foreach (var item in s)
                        if (!string.IsNullOrEmpty(item))
                            message += $"<text>{item}</text>";
                    XmlDocument doc = new XmlDocument();
                    string tile = $@"
<tile>
    <visual>
        <binding template='TileMedium'>{message}</binding>
        <binding template='TileWide'>{message}</binding>
        <binding template='TileLarge'>{message}</binding> 
    </visual>
</tile>";
                    doc.LoadXml(tile);
                    tileUpdateManeger.Update(new TileNotification(doc));
                }
                else
                {
                    string message1 = string.Empty, message2 = string.Empty;
                    int i = 0;
                    for (int j = 0; i < 6 && j < 3; i++)
                    {
                        if (!string.IsNullOrEmpty(s[i]))
                        {
                            message1 += $"<text>{s[i]}</text>";
                            j++;
                        }
                    }

                    for (; i < 6; i++)
                    {
                        if (!string.IsNullOrEmpty(s[i]))
                        { message2 += $"<text>{s[i]}</text>"; }
                    }

                    string tile1 = $@"
<tile>
    <visual>
        <binding template='TileMedium'>{message1}</binding>
        <binding template='TileWide'>
            <group>
                <subgroup>{message1}</subgroup>
                <subgroup>{message2}</subgroup>
            </group>
        </binding>
        <binding template='TileLarge'>{message1}{message2}</binding> 
    </visual>
</tile>";
                    string tile2 = $@"
<tile>
    <visual>
        <binding template='TileMedium'>{message2}</binding>
    </visual>
</tile>";
                    XmlDocument doc1 = new XmlDocument();
                    doc1.LoadXml(tile1);
                    tileUpdateManeger.Update(new TileNotification(doc1));
                    doc1.LoadXml(tile2);
                    tileUpdateManeger.Update(new TileNotification(doc1));
                }
            }
        }
    }
}