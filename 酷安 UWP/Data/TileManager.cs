using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace CoolapkUWP.Data
{
    internal static class TileManager
    {
        public static void SetBadgeNum(double num)
        {
            BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Clear();
            if (num != 0)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml($"<badge value=\"{num}\"/>");
                badgeUpdater.Update(new BadgeNotification(xml));
            }
        }

        /// <param name="nums">顺序 follow, message, atme, atcommentme, commentme, feedlike</param>
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
                        default:
                            break;
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