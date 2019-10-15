using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace 酷安_UWP
{
    class Web
    {

        public static async Task<string> GetHttp(string url)
        {
            try
            {
                return await new HttpClient().GetStringAsync(new Uri(url));
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ReplaceHtml(String str)
        {
            //换行和段落
            String s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ");
            //链接彻底删除！
            try
            {
                while (s.IndexOf("<a") > 0)
                {
                    s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                    s = s.Replace("</a>", "");
                }
            }
            catch (Exception)
            {

            }
            return s;
        }
        public async static Task<object[]> CheckUpdate()
        {
            HttpClient client = new HttpClient();
            string updateDetail = (await client.GetAsync(new Uri("https://api.github.com/repos/Tangent-90/Coolapk-UWP"))).ToString();
            string updatePacketURI = string.Empty;
            return new object[] { true, updateDetail, updatePacketURI };
        }
    }
}
