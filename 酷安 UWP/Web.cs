using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace 酷安_UWP
{
    class Web
    {
        public static async Task<string> GetHttp(string url) => await new HttpClient().GetStringAsync(new Uri(url));

        public static string ReplaceHtml(string str)
        {
            //换行和段落
            string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ");
            //链接彻底删除！
            while (s.IndexOf("<a") > 0)
            {
                s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                s = s.Replace("</a>", "");
            }
            return s;
        }
    }
}
