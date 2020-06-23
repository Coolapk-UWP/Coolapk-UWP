using System;
using System.Collections.Immutable;
using System.Linq;

namespace CoolapkUWP.Helpers
{
    internal static class EmojiHelper
    {
        public static readonly ImmutableArray<string> oldEmojis = new string[]
{
"[doge]",
"[doge原谅ta]",
"[doge呵斥]",
"[doge笑哭]",
"[OK]",
"[qqdoge]",
"[二哈]",
"[亲亲]",
"[傲慢]",
"[再见]",
"[凋谢]",
"[发呆]",
"[发怒]",
"[可怜]",
"[可爱]",
"[呵呵]",
"[喵喵]",
"[喷血]",
"[嘿哈]",
"[坏笑]",
"[委屈]",
"[害羞]",
"[弱]",
"[强]",
"[心碎]",
"[惊讶]",
"[我最美]",
"[托腮]",
"[抠鼻]",
"[抱拳]",
"[捂脸]",
"[撇嘴]",
"[无奈]",
"[机智]",
"[流泪]",
"[爱心]",
"[玫瑰]",
"[疑问]",
"[白眼]",
"[皱眉]",
"[睡]",
"[笑哭]",
"[耶]",
"[色]",
"[菜刀]",
"[鄙视]",
"[酷]",
"[酷币100块]",
"[酷币10块]",
"[酷币1€]",
"[酷币1分]",
"[酷币1块]",
"[酷币1毛]",
"[酷币20块]",
"[酷币2€]",
"[酷币2分]",
"[酷币2块]",
"[酷币2毛]",
"[酷币50块]",
"[酷币5€]",
"[酷币5分]",
"[酷币5块]",
"[酷币5毛]",
"[酷币]",
"[阴险]",
"[难过]"       }.ToImmutableArray();


        public static readonly ImmutableArray<string> emojis = new string[] {"(cos滑稽",
"(haha",
"(OK",
"(sofa",
"(what",
"(啊",
"(爱心",
"(暗中观察",
"(鄙视",
"(便便",
"(不高兴",
"(彩虹",
"(茶杯",
"(吃瓜",
"(传统滑稽",
"(大拇指",
"(蛋糕",
"(灯泡",
"(斗鸡眼滑稽",
"(乖",
"(哈哈",
"(汗",
"(呵呵",
"(喝酒",
"(黑头瞪眼",
"(黑头高兴",
"(黑线",
"(嘿嘿嘿",
"(哼",
"(红领巾",
"(呼~",
"(花心",
"(滑稽",
"(滑稽炸",
"(欢呼",
"(稽滑",
"(紧张",
"(惊哭",
"(惊讶",
"(开心",
"(柯基暗中观察",
"(酷",
"(狂汗",
"(困成狗",
"(蜡烛",
"(懒得理",
"(泪",
"(冷",
"(礼物",
"(流汗滑稽",
"(玫瑰",
"(勉强",
"(墨镜滑稽",
"(你懂的",
"(怒",
"(喷",
"(噗",
"(钱",
"(钱币",
"(弱",
"(三道杠",
"(胜利",
"(受虐滑稽",
"(睡觉",
"(酸爽",
"(太开心",
"(太阳",
"(摊手",
"(突然兴奋",
"(吐",
"(吐舌",
"(托腮",
"(挖鼻",
"(微微一笑",
"(委屈",
"(捂嘴笑",
"(犀利",
"(香蕉",
"(小乖",
"(小红脸",
"(小嘴滑稽",
"(笑尿",
"(笑眼",
"(心碎",
"(星星月亮",
"(呀咩爹",
"(药丸",
"(咦",
"(疑问",
"(阴险",
"(音乐",
"(真棒",
"(纸巾",
"[cos滑稽]",
"[doge]",
"[doge并不简单]",
"[doge吃瓜]",
"[doge吃惊]",
"[doge飞吻]",
"[doge告辞]",
"[doge汗]",
"[doge呵斥]",
"[doge互粉]",
"[doge口罩]",
"[doge酷]",
"[doge期待]",
"[doge问号]",
"[doge笑哭]",
"[doge疑问]",
"[doge原谅ta]",
"[doge装酷]",
"[dw哭]",
"[NO]",
"[ok]",
"[py交易]",
"[qqdoge]",
"[t耐克嘴]",
"[w→_→]",
"[wpy交易]",
"[wv5]",
"[ww亲亲]",
"[w爱你]",
"[w奥特曼]",
"[w拜拜]",
"[w悲伤]",
"[w鄙视]",
"[w闭嘴]",
"[w并不简单]",
"[w操练]",
"[w馋嘴]",
"[w吃瓜]",
"[w吃惊]",
"[w打哈欠]",
"[w打脸]",
"[w蛋糕]",
"[w定]",
"[w哆啦a梦吃惊]",
"[w哆啦a梦色]",
"[w哆啦a梦笑]",
"[w肥皂]",
"[w感冒]",
"[w干杯]",
"[w给力]",
"[w鼓掌]",
"[w广而告之]",
"[w跪了]",
"[w哈哈]",
"[w害羞]",
"[w呵呵]",
"[w黑线]",
"[w嘿嘿嘿]",
"[w哼]",
"[w互粉]",
"[w花心]",
"[w急眼]",
"[w囧]",
"[w可爱]",
"[w可怜]",
"[w哭]",
"[w酷]",
"[w酷币]",
"[w困]",
"[w懒得理你]",
"[w累]",
"[w旅行]",
"[w萌]",
"[w男孩儿]",
"[w怒~]",
"[w怒骂]",
"[w女孩儿]",
"[w啤酒鸡腿]",
"[w钱]",
"[w傻眼]",
"[w神马]",
"[w神兽]",
"[w生病]",
"[w失望]",
"[w帅]",
"[w睡觉]",
"[w思考]",
"[w太开心]",
"[w摊手]",
"[w舔]",
"[w调皮]",
"[w偷笑]",
"[w吐]",
"[w兔子]",
"[w挖鼻屎]",
"[w委屈]",
"[w我美吗]",
"[w捂脸哭]",
"[w嘻嘻]",
"[w喜]",
"[w小样儿]",
"[w笑哭]",
"[w新浪]",
"[w熊猫]",
"[w嘘]",
"[w压岁钱]",
"[w疑问]",
"[w阴险]",
"[w右哼哼]",
"[w晕]",
"[w再见]",
"[w织毛衣]",
"[w猪头]",
"[w抓狂]",
"[w左哼哼]",
"[w左哼哼哭]",
"[挨打]",
"[爱你]",
"[爱情]",
"[爱心]",
"[傲慢]",
"[白纹酷币]",
"[白眼]",
"[抱拳]",
"[爆怒]",
"[鄙视]",
"[闭嘴]",
"[便便]",
"[表面开心]",
"[表面哭泣]",
"[不开心]",
"[擦汗]",
"[菜刀]",
"[差劲]",
"[吃瓜]",
"[呲牙]",
"[大兵]",
"[大哭]",
"[蛋糕]",
"[刀]",
"[得意]",
"[凋谢]",
"[斗鸡眼滑稽]",
"[二哈]",
"[二哈盯]",
"[发呆]",
"[发抖]",
"[发怒]",
"[饭]",
"[飞吻]",
"[奋斗]",
"[尴尬]",
"[勾引]",
"[鼓掌]",
"[哈哈]",
"[哈哈哈]",
"[哈欠]",
"[害怕]",
"[害羞]",
"[憨笑]",
"[汗]",
"[呵呵]",
"[喝茶]",
"[喝酒]",
"[黑线]",
"[嘿哈]",
"[嘿嘿]",
"[哼唧]",
"[红药丸]",
"[滑稽]",
"[坏笑]",
"[欢呼]",
"[灰色酷币]",
"[火把]",
"[饥饿]",
"[机智]",
"[假笑]",
"[奸笑]",
"[惊恐]",
"[惊喜]",
"[惊讶]",
"[咖啡]",
"[可爱]",
"[可怜]",
"[抠鼻]",
"[骷髅]",
"[酷]",
"[酷安]",
"[酷安钓鱼]",
"[酷安绿帽]",
"[酷币]",
"[酷币1$]",
"[酷币1]",
"[酷币1€]",
"[酷币1分]",
"[酷币1块]",
"[酷币1毛]",
"[酷币2$]",
"[酷币2]",
"[酷币2€]",
"[酷币2分]",
"[酷币2块]",
"[酷币2毛]",
"[酷币5$]",
"[酷币5]",
"[酷币5€]",
"[酷币5分]",
"[酷币5块]",
"[酷币5毛]",
"[酷币10块]",
"[酷币20块]",
"[酷币50块]",
"[酷币100块]",
"[酷币空]",
"[快哭了]",
"[困]",
"[篮球]",
"[懒得理]",
"[泪奔]",
"[冷汗]",
"[礼物]",
"[流汗]",
"[流汗滑稽]",
"[流泪]",
"[绿帽]",
"[绿色酷币]",
"[绿药丸]",
"[卖萌]",
"[玫瑰]",
"[喵喵]",
"[喵喵鄙视]",
"[喵喵并不简单]",
"[喵喵吃瓜]",
"[喵喵吃惊]",
"[喵喵飞吻]",
"[喵喵告辞]",
"[喵喵汗]",
"[喵喵呵斥]",
"[喵喵互粉]",
"[喵喵口罩]",
"[喵喵酷]",
"[喵喵期待]",
"[喵喵问号]",
"[喵喵笑哭]",
"[喵喵疑问]",
"[喵喵原谅ta]",
"[喵喵再见]",
"[喵喵装酷]",
"[墨镜滑稽]",
"[耐克嘴]",
"[难过]",
"[牛啤]",
"[哦吼吼]",
"[怄火]",
"[喷]",
"[喷血]",
"[啤酒]",
"[瓢虫]",
"[撇嘴]",
"[乒乓]",
"[噗]",
"[强]",
"[敲打]",
"[亲亲]",
"[糗大了]",
"[拳头]",
"[弱]",
"[骚扰]",
"[色]",
"[闪电]",
"[胜利]",
"[示爱]",
"[受虐滑稽]",
"[舒服]",
"[衰]",
"[睡]",
"[太阳]",
"[舔]",
"[挑眉坏笑]",
"[调皮]",
"[跳跳]",
"[偷看]",
"[吐]",
"[吐舌]",
"[托腮]",
"[微微一笑]",
"[微笑]",
"[委屈]",
"[我最美]",
"[握手]",
"[无奈]",
"[无语]",
"[捂脸]",
"[捂嘴笑]",
"[西瓜]",
"[吓]",
"[小纠结]",
"[小嘴滑稽]",
"[笑哭]",
"[笑哭再见]",
"[笑眼]",
"[斜眼笑]",
"[心碎]",
"[新币1分]",
"[新酷币]",
"[新酷币1$]",
"[新酷币1€]",
"[新酷币1块]",
"[新酷币1毛]",
"[新酷币2$]",
"[新酷币2€]",
"[新酷币2分]",
"[新酷币2块]",
"[新酷币2毛]",
"[新酷币5$]",
"[新酷币5€]",
"[新酷币5分]",
"[新酷币5块]",
"[新酷币5毛]",
"[新酷币10块]",
"[新酷币20块]",
"[新酷币50块]",
"[新酷币100块]",
"[嘘]",
"[掩面笑]",
"[耶]",
"[疑问]",
"[阴险]",
"[拥抱]",
"[右哼哼]",
"[月亮]",
"[晕]",
"[再见]",
"[炸弹]",
"[折磨]",
"[咒骂]",
"[皱眉]",
"[猪头]",
"[抓狂]",
"[转圈]",
"[足球]",
"[左哼哼]",
"[Blob滑稽]",
"[Google滑稽]",
"[SegoeUI滑稽]" }.ToImmutableArray();

        public static bool Contains(string key, bool useOldEmoji = false) => useOldEmoji ? oldEmojis.Contains(key) : emojis.Contains(key);

        public static Uri Get(string key, bool useOldEmoji = false)
        {
            short id = useOldEmoji ? GetOldEmojiId(key) : GetEmojiID(key);
            if (id == -1)
            {
                return ImageCacheHelper.NoPic.UriSource;
            }
            else
            {
                return new Uri($"ms-appx:///Assets/Emoji/{id:D4}.png");
            }
        }

        private static short GetOldEmojiId(string key)
        {
            switch (key)
            {
                case "[doge]": return 2002;
                case "[doge原谅ta]": return 2004;
                case "[doge呵斥]": return 2009;
                case "[doge笑哭]": return 2014;
                case "[ok]": return 2022;
                case "[qqdoge]": return 2024;
                case "[二哈]": return 2121;
                case "[亲亲]": return 2123;
                case "[傲慢]": return 2127;
                case "[再见]": return 2128;
                case "[凋谢]": return 2130;
                case "[发呆]": return 2134;
                case "[发怒]": return 2135;
                case "[可怜]": return 2138;
                case "[可爱]": return 2139;
                case "[呵呵]": return 2146;
                case "[喵喵]": return 2157;
                case "[喷血]": return 2177;
                case "[嘿哈]": return 2179;
                case "[坏笑]": return 2183;
                case "[委屈]": return 2190;
                case "[害羞]": return 2192;
                case "[弱]": return 2198;
                case "[强]": return 2199;
                case "[心碎]": return 2203;
                case "[惊讶]": return 2208;
                case "[我最美]": return 2211;
                case "[托腮]": return 2212;
                case "[抠鼻]": return 2215;
                case "[抱拳]": return 2216;
                case "[捂脸]": return 2222;
                case "[撇嘴]": return 2225;
                case "[无奈]": return 2250;
                case "[机智]": return 2254;
                case "[流泪]": return 2260;
                case "[爱心]": return 2267;
                case "[玫瑰]": return 2271;
                case "[疑问]": return 2273;
                case "[白眼]": return 2274;
                case "[皱眉]": return 2276;
                case "[睡]": return 2277;
                case "[笑哭]": return 2280;
                case "[耶]": return 2290;
                case "[色]": return 2294;
                case "[菜刀]": return 2295;
                case "[鄙视]": return 2305;
                case "[酷]": return 2306;
                case "[酷币100块]": return 2311;
                case "[酷币10块]": return 2312;
                case "[酷币1€]": return 2314;
                case "[酷币1分]": return 2315;
                case "[酷币1块]": return 2316;
                case "[酷币1毛]": return 2317;
                case "[酷币20块]": return 2319;
                case "[酷币2€]": return 2321;
                case "[酷币2分]": return 2322;
                case "[酷币2块]": return 2323;
                case "[酷币2毛]": return 2324;
                case "[酷币50块]": return 2326;
                case "[酷币5€]": return 2328;
                case "[酷币5分]": return 2329;
                case "[酷币5块]": return 2330;
                case "[酷币5毛]": return 2331;
                case "[酷币]": return 2332;
                case "[阴险]": return 2336;
                case "[难过]": return 2337;
                default: return -1;
            }
        }

        private static short GetEmojiID(string key)
        {
            switch (key)
            {
                case "(cos滑稽": return 0000;
                case "(haha": return 0001;
                case "(OK": return 0002;
                case "(sofa": return 0003;
                case "(what": return 0004;
                case "(三道杠": return 0005;
                case "(不高兴": return 0006;
                case "(乖": return 0007;
                case "(传统滑稽": return 0008;
                case "(你懂的": return 0009;
                case "(便便": return 0010;
                case "(冷": return 0011;
                case "(勉强": return 0012;
                case "(受虐滑稽": return 0013;
                case "(吃瓜": return 0014;
                case "(吐": return 0015;
                case "(吐舌": return 0016;
                case "(呀咩爹": return 0017;
                case "(呵呵": return 0018;
                case "(呼~": return 0019;
                case "(咦": return 0020;
                case "(哈哈": return 0021;
                case "(哼": return 0022;
                case "(啊": return 0023;
                case "(喝酒": return 0024;
                case "(喷": return 0025;
                case "(嘿嘿嘿": return 0026;
                case "(噗": return 0027;
                case "(困成狗": return 0028;
                case "(墨镜滑稽": return 0029;
                case "(大拇指": return 0030;
                case "(太开心": return 0031;
                case "(太阳": return 0032;
                case "(委屈": return 0033;
                case "(小乖": return 0034;
                case "(小嘴滑稽": return 0035;
                case "(小红脸": return 0036;
                case "(开心": return 0037;
                case "(弱": return 0038;
                case "(彩虹": return 0039;
                case "(微微一笑": return 0040;
                case "(心碎": return 0041;
                case "(怒": return 0042;
                case "(惊哭": return 0043;
                case "(惊讶": return 0044;
                case "(懒得理": return 0045;
                case "(托腮": return 0046;
                case "(挖鼻": return 0047;
                case "(捂嘴笑": return 0048;
                case "(摊手": return 0049;
                case "(斗鸡眼滑稽": return 0050;
                case "(星星月亮": return 0051;
                case "(暗中观察": return 0052;
                case "(柯基暗中观察": return 0053;
                case "(欢呼": return 0054;
                case "(汗": return 0055;
                case "(泪": return 0056;
                case "(流汗滑稽": return 0057;
                case "(滑稽": return 0058;
                case "(滑稽炸": return 0059;
                case "(灯泡": return 0060;
                case "(爱心": return 0061;
                case "(犀利": return 0062;
                case "(狂汗": return 0063;
                case "(玫瑰": return 0064;
                case "(疑问": return 0065;
                case "(真棒": return 0066;
                case "(睡觉": return 0067;
                case "(礼物": return 0068;
                case "(稽滑": return 0069;
                case "(突然兴奋": return 0070;
                case "(笑尿": return 0071;
                case "(笑眼": return 0072;
                case "(紧张": return 0073;
                case "(红领巾": return 0074;
                case "(纸巾": return 0075;
                case "(胜利": return 0076;
                case "(花心": return 0077;
                case "(茶杯": return 0078;
                case "(药丸": return 0079;
                case "(蛋糕": return 0080;
                case "(蜡烛": return 0081;
                case "(鄙视": return 0082;
                case "(酷": return 0083;
                case "(酸爽": return 0084;
                case "(钱": return 0085;
                case "(钱币": return 0086;
                case "(阴险": return 0087;
                case "(音乐": return 0088;
                case "(香蕉": return 0089;
                case "(黑头瞪眼": return 0090;
                case "(黑头高兴": return 0091;
                case "(黑线": return 0092;

                case "[Blob滑稽]": return 1000;
                case "[cos滑稽]": return 1001;
                case "[doge]": return 1002;
                case "[doge互粉]": return 1003;
                case "[doge原谅ta]": return 1004;
                case "[doge口罩]": return 1005;
                case "[doge吃惊]": return 1006;
                case "[doge吃瓜]": return 1007;
                case "[doge告辞]": return 1008;
                case "[doge呵斥]": return 1009;
                case "[doge并不简单]": return 1010;
                case "[doge期待]": return 1011;
                case "[doge汗]": return 1012;
                case "[doge疑问]": return 1013;
                case "[doge笑哭]": return 1014;
                case "[doge装酷]": return 1015;
                case "[doge酷]": return 1016;
                case "[doge问号]": return 1017;
                case "[doge飞吻]": return 1018;
                case "[dw哭]": return 1019;
                case "[Google滑稽]": return 1020;
                case "[NO]": return 1021;
                case "[ok]": return 1022;
                case "[py交易]": return 1023;
                case "[qqdoge]": return 1024;
                case "[SegoeUI滑稽]": return 1025;
                case "[t耐克嘴]": return 1026;
                case "[wpy交易]": return 1027;
                case "[wv5]": return 1028;
                case "[ww亲亲]": return 1029;
                case "[w→_→]": return 1030;
                case "[w互粉]": return 1031;
                case "[w偷笑]": return 1032;
                case "[w傻眼]": return 1033;
                case "[w兔子]": return 1034;
                case "[w再见]": return 1035;
                case "[w压岁钱]": return 1036;
                case "[w可怜]": return 1037;
                case "[w可爱]": return 1038;
                case "[w右哼哼]": return 1039;
                case "[w吃惊]": return 1040;
                case "[w吃瓜]": return 1041;
                case "[w吐]": return 1042;
                case "[w呵呵]": return 1043;
                case "[w哆啦a梦吃惊]": return 1044;
                case "[w哆啦a梦笑]": return 1045;
                case "[w哆啦a梦色]": return 1046;
                case "[w哈哈]": return 1047;
                case "[w哭]": return 1048;
                case "[w哼]": return 1049;
                case "[w啤酒鸡腿]": return 1050;
                case "[w喜]": return 1051;
                case "[w嘘]": return 1052;
                case "[w嘻嘻]": return 1053;
                case "[w嘿嘿嘿]": return 1054;
                case "[w囧]": return 1055;
                case "[w困]": return 1056;
                case "[w太开心]": return 1057;
                case "[w失望]": return 1058;
                case "[w奥特曼]": return 1059;
                case "[w女孩儿]": return 1060;
                case "[w委屈]": return 1061;
                case "[w定]": return 1062;
                case "[w害羞]": return 1063;
                case "[w小样儿]": return 1064;
                case "[w左哼哼]": return 1065;
                case "[w左哼哼哭]": return 1066;
                case "[w帅]": return 1067;
                case "[w干杯]": return 1068;
                case "[w并不简单]": return 1069;
                case "[w广而告之]": return 1070;
                case "[w怒~]": return 1071;
                case "[w怒骂]": return 1072;
                case "[w思考]": return 1073;
                case "[w急眼]": return 1074;
                case "[w悲伤]": return 1075;
                case "[w感冒]": return 1076;
                case "[w懒得理你]": return 1077;
                case "[w我美吗]": return 1078;
                case "[w打哈欠]": return 1079;
                case "[w打脸]": return 1080;
                case "[w抓狂]": return 1081;
                case "[w拜拜]": return 1082;
                case "[w挖鼻屎]": return 1083;
                case "[w捂脸哭]": return 1084;
                case "[w摊手]": return 1085;
                case "[w操练]": return 1086;
                case "[w新浪]": return 1087;
                case "[w旅行]": return 1088;
                case "[w晕]": return 1089;
                case "[w熊猫]": return 1090;
                case "[w爱你]": return 1091;
                case "[w猪头]": return 1092;
                case "[w生病]": return 1093;
                case "[w男孩儿]": return 1094;
                case "[w疑问]": return 1095;
                case "[w睡觉]": return 1096;
                case "[w神兽]": return 1097;
                case "[w神马]": return 1098;
                case "[w笑哭]": return 1099;
                case "[w累]": return 1100;
                case "[w织毛衣]": return 1101;
                case "[w给力]": return 1102;
                case "[w肥皂]": return 1103;
                case "[w舔]": return 1104;
                case "[w花心]": return 1105;
                case "[w萌]": return 1106;
                case "[w蛋糕]": return 1107;
                case "[w调皮]": return 1108;
                case "[w跪了]": return 1109;
                case "[w鄙视]": return 1110;
                case "[w酷]": return 1111;
                case "[w酷币]": return 1112;
                case "[w钱]": return 1113;
                case "[w闭嘴]": return 1114;
                case "[w阴险]": return 1115;
                case "[w馋嘴]": return 1116;
                case "[w黑线]": return 1117;
                case "[w鼓掌]": return 1118;
                case "[不开心]": return 1119;
                case "[乒乓]": return 1120;
                case "[二哈]": return 1121;
                case "[二哈盯]": return 1122;
                case "[亲亲]": return 1123;
                case "[便便]": return 1124;
                case "[假笑]": return 1125;
                case "[偷看]": return 1126;
                case "[傲慢]": return 1127;
                case "[再见]": return 1128;
                case "[冷汗]": return 1129;
                case "[凋谢]": return 1130;
                case "[刀]": return 1131;
                case "[勾引]": return 1132;
                case "[卖萌]": return 1133;
                case "[发呆]": return 1134;
                case "[发怒]": return 1135;
                case "[发抖]": return 1136;
                case "[受虐滑稽]": return 1137;
                case "[可怜]": return 1138;
                case "[可爱]": return 1139;
                case "[右哼哼]": return 1140;
                case "[吃瓜]": return 1141;
                case "[吐]": return 1142;
                case "[吐舌]": return 1143;
                case "[吓]": return 1144;
                case "[呲牙]": return 1145;
                case "[呵呵]": return 1146;
                case "[咒骂]": return 1147;
                case "[咖啡]": return 1148;
                case "[哈哈]": return 1149;
                case "[哈哈哈]": return 1150;
                case "[哈欠]": return 1151;
                case "[哦吼吼]": return 1152;
                case "[哼唧]": return 1153;
                case "[啤酒]": return 1154;
                case "[喝茶]": return 1155;
                case "[喝酒]": return 1156;
                case "[喵喵]": return 1157;
                case "[喵喵互粉]": return 1158;
                case "[喵喵再见]": return 1159;
                case "[喵喵原谅ta]": return 1160;
                case "[喵喵口罩]": return 1161;
                case "[喵喵吃惊]": return 1162;
                case "[喵喵吃瓜]": return 1163;
                case "[喵喵告辞]": return 1164;
                case "[喵喵呵斥]": return 1165;
                case "[喵喵并不简单]": return 1166;
                case "[喵喵期待]": return 1167;
                case "[喵喵汗]": return 1168;
                case "[喵喵疑问]": return 1169;
                case "[喵喵笑哭]": return 1170;
                case "[喵喵装酷]": return 1171;
                case "[喵喵鄙视]": return 1172;
                case "[喵喵酷]": return 1173;
                case "[喵喵问号]": return 1174;
                case "[喵喵飞吻]": return 1175;
                case "[喷]": return 1176;
                case "[喷血]": return 1177;
                case "[嘘]": return 1178;
                case "[嘿哈]": return 1179;
                case "[嘿嘿]": return 1180;
                case "[噗]": return 1181;
                case "[困]": return 1182;
                case "[坏笑]": return 1183;
                case "[墨镜滑稽]": return 1184;
                case "[大兵]": return 1185;
                case "[大哭]": return 1186;
                case "[太阳]": return 1187;
                case "[奋斗]": return 1188;
                case "[奸笑]": return 1189;
                case "[委屈]": return 1190;
                case "[害怕]": return 1191;
                case "[害羞]": return 1192;
                case "[小嘴滑稽]": return 1193;
                case "[小纠结]": return 1194;
                case "[尴尬]": return 1195;
                case "[左哼哼]": return 1196;
                case "[差劲]": return 1197;
                case "[弱]": return 1198;
                case "[强]": return 1199;
                case "[得意]": return 1200;
                case "[微微一笑]": return 1201;
                case "[微笑]": return 1202;
                case "[心碎]": return 1203;
                case "[快哭了]": return 1204;
                case "[怄火]": return 1205;
                case "[惊喜]": return 1206;
                case "[惊恐]": return 1207;
                case "[惊讶]": return 1208;
                case "[憨笑]": return 1209;
                case "[懒得理]": return 1210;
                case "[我最美]": return 1211;
                case "[托腮]": return 1212;
                case "[抓狂]": return 1213;
                case "[折磨]": return 1214;
                case "[抠鼻]": return 1215;
                case "[抱拳]": return 1216;
                case "[拥抱]": return 1217;
                case "[拳头]": return 1218;
                case "[挑眉坏笑]": return 1219;
                case "[挨打]": return 1220;
                case "[捂嘴笑]": return 1221;
                case "[捂脸]": return 1222;
                case "[掩面笑]": return 1223;
                case "[握手]": return 1224;
                case "[撇嘴]": return 1225;
                case "[擦汗]": return 1226;
                case "[敲打]": return 1227;
                case "[斗鸡眼滑稽]": return 1228;
                case "[斜眼笑]": return 1229;
                case "[新币1分]": return 1230;
                case "[新酷币1$]": return 1231;
                case "[新酷币100块]": return 1232;
                case "[新酷币10块]": return 1233;
                case "[新酷币1€]": return 1234;
                case "[新酷币1块]": return 1235;
                case "[新酷币1毛]": return 1236;
                case "[新酷币2$]": return 1237;
                case "[新酷币20块]": return 1238;
                case "[新酷币2€]": return 1239;
                case "[新酷币2分]": return 1240;
                case "[新酷币2块]": return 1241;
                case "[新酷币2毛]": return 1242;
                case "[新酷币5$]": return 1243;
                case "[新酷币50块]": return 1244;
                case "[新酷币5€]": return 1245;
                case "[新酷币5分]": return 1246;
                case "[新酷币5块]": return 1247;
                case "[新酷币5毛]": return 1248;
                case "[新酷币]": return 1249;
                case "[无奈]": return 1250;
                case "[无语]": return 1251;
                case "[晕]": return 1252;
                case "[月亮]": return 1253;
                case "[机智]": return 1254;
                case "[欢呼]": return 1255;
                case "[汗]": return 1256;
                case "[泪奔]": return 1257;
                case "[流汗]": return 1258;
                case "[流汗滑稽]": return 1259;
                case "[流泪]": return 1260;
                case "[滑稽]": return 1261;
                case "[火把]": return 1262;
                case "[灰色酷币]": return 1263;
                case "[炸弹]": return 1264;
                case "[爆怒]": return 1265;
                case "[爱你]": return 1266;
                case "[爱心]": return 1267;
                case "[爱情]": return 1268;
                case "[牛啤]": return 1269;
                case "[猪头]": return 1270;
                case "[玫瑰]": return 1271;
                case "[瓢虫]": return 1272;
                case "[疑问]": return 1273;
                case "[白眼]": return 1274;
                case "[白纹酷币]": return 1275;
                case "[皱眉]": return 1276;
                case "[睡]": return 1277;
                case "[示爱]": return 1278;
                case "[礼物]": return 1279;
                case "[笑哭]": return 1280;
                case "[笑哭再见]": return 1281;
                case "[笑眼]": return 1282;
                case "[篮球]": return 1283;
                case "[糗大了]": return 1284;
                case "[红药丸]": return 1285;
                case "[绿帽]": return 1286;
                case "[绿色酷币]": return 1287;
                case "[绿药丸]": return 1288;
                case "[耐克嘴]": return 1289;
                case "[耶]": return 1290;
                case "[胜利]": return 1291;
                case "[舒服]": return 1292;
                case "[舔]": return 1293;
                case "[色]": return 1294;
                case "[菜刀]": return 1295;
                case "[蛋糕]": return 1296;
                case "[表面哭泣]": return 1297;
                case "[表面开心]": return 1298;
                case "[衰]": return 1299;
                case "[西瓜]": return 1300;
                case "[调皮]": return 1301;
                case "[足球]": return 1302;
                case "[跳跳]": return 1303;
                case "[转圈]": return 1304;
                case "[鄙视]": return 1305;
                case "[酷]": return 1306;
                case "[酷安]": return 1307;
                case "[酷安绿帽]": return 1308;
                case "[酷安钓鱼]": return 1309;
                case "[酷币1$]": return 1310;
                case "[酷币100块]": return 1311;
                case "[酷币10块]": return 1312;
                case "[酷币1]": return 1313;
                case "[酷币1€]": return 1314;
                case "[酷币1分]": return 1315;
                case "[酷币1块]": return 1316;
                case "[酷币1毛]": return 1317;
                case "[酷币2$]": return 1318;
                case "[酷币20块]": return 1319;
                case "[酷币2]": return 1320;
                case "[酷币2€]": return 1321;
                case "[酷币2分]": return 1322;
                case "[酷币2块]": return 1323;
                case "[酷币2毛]": return 1324;
                case "[酷币5$]": return 1325;
                case "[酷币50块]": return 1326;
                case "[酷币5]": return 1327;
                case "[酷币5€]": return 1328;
                case "[酷币5分]": return 1329;
                case "[酷币5块]": return 1330;
                case "[酷币5毛]": return 1331;
                case "[酷币]": return 1332;
                case "[酷币空]": return 1333;
                case "[闪电]": return 1334;
                case "[闭嘴]": return 1335;
                case "[阴险]": return 1336;
                case "[难过]": return 1337;
                case "[飞吻]": return 1338;
                case "[饥饿]": return 1339;
                case "[饭]": return 1340;
                case "[骚扰]": return 1341;
                case "[骷髅]": return 1342;
                case "[黑线]": return 1343;
                case "[鼓掌]": return 1344;
                default: return -1;
            }
        }
    }
}