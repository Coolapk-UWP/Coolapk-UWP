using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 酷安_UWP
{
    class CoolApkSDK
    {
        //超级感谢！！！👉 https://github.com/ZCKun/CoolapkTokenCrack
        public static string GetToken()
        {
            String DEVICE_ID = "8513efac-09ea-3709-b214-95b366f1a185";
            long UnixDate = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string t = UnixDate.ToString();
            string hex_t = "0x" + Ten2Hex(t);
            // 时间戳加密
            string md5_t = GetMD5(t);
            // 不知道什么鬼字符串拼接
            string a = "token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?" + md5_t + "$" + DEVICE_ID + "&com.coolapk.market";
            // 不知道什么鬼字符串拼接 后的字符串再次加密
            //md5_a = hashlib.md5(base64.b64encode(a.encode('utf-8)).hexdigest()
            string md5_a = GetMD5(Convert.ToBase64String(Encoding.UTF8.GetBytes(a)));
            string token = md5_a + DEVICE_ID + hex_t;

            return token;
        }

        public static string GetMD5(string inputString)
        {
            //来源：https://blog.csdn.net/lindexi_gd/article/details/48951849

            // 创建一个可重用的CryptographicHash对象           
            CryptographicHash objHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5).CreateHash();
            
            IBuffer buffMsg1 = CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8);
            objHash.Append(buffMsg1);
            IBuffer buffHash1 = objHash.GetValueAndReset();
            return CryptographicBuffer.EncodeToHexString(buffHash1);
        }

        public static string Ten2Hex(string ten)
        {
            ulong tenValue = Convert.ToUInt64(ten);
            ulong divValue, resValue;
            string hex = "";
            do
            {
                //divValue = (ulong)Math.Floor(tenValue / 16);

                divValue = (ulong)Math.Floor((decimal)(tenValue / 16));

                resValue = tenValue % 16;
                hex = TenValue2Char(resValue) + hex;
                tenValue = divValue;
            }
            while (tenValue >= 16);
            if (tenValue != 0)
                hex = TenValue2Char(tenValue) + hex;
            return hex;
        }
        public static string TenValue2Char(ulong ten)
        {
            switch (ten)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return ten.ToString();
                case 10:
                    return "A";
                case 11:
                    return "B";
                case 12:
                    return "C";
                case 13:
                    return "D";
                case 14:
                    return "E";
                case 15:
                    return "F";
                default:
                    return "";
            }
        }


        public static async Task<string> GetCoolApkMessage(string url)
        {
            //这里感谢https://github.com/bjzhou/Coolapk-kotlin提供的 HTTP 头！！！！！！！！！！！！！

            try
            {
                var mClient = new HttpClient();

                mClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 9; MI 8 SE MIUI/9.5.9) (#Build; Xiaomi; MI 8 SE; PKQ1.181121.001; 9) +CoolMarket/9.2.2-1905301");
                mClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                mClient.DefaultRequestHeaders.Add("X-Sdk-Int", "28");
                mClient.DefaultRequestHeaders.Add("X-Sdk-Locale", "zh-CN");
                mClient.DefaultRequestHeaders.Add("X-App-Id", "com.coolapk.market");
                mClient.DefaultRequestHeaders.Add("X-App-Token", GetToken());
                mClient.DefaultRequestHeaders.Add("X-App-Version", "9.2.2");
                mClient.DefaultRequestHeaders.Add("X-App-Code", "1905301");
                mClient.DefaultRequestHeaders.Add("X-Api-Version", "9");
                mClient.DefaultRequestHeaders.Add("X-App-Device", "QRTBCOgkUTgsTat9WYphFI7kWbvFWaYByO1YjOCdjOxAjOxEkOFJjODlDI7ATNxMjM5MTOxcjMwAjN0AyOxEjNwgDNxITM2kDMzcTOgsTZzkTZlJ2MwUDNhJ2MyYzM");
                mClient.DefaultRequestHeaders.Add("X-Dark-Mode", "0");
                mClient.DefaultRequestHeaders.Add("Host", "api.coolapk.com");

                return await mClient.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ex={e.Message}\n" +
                    $"{e.Source}\n" +
                    $"{e.InnerException}\n");
                return "";
            }
        }

        /*
        public static async Task<string> GetUserIDByName(String name)
        {
            try
            {
                string uid = await Web.GetHttp("https://www.coolapk.com/n/" + name);
                uid = uid.Split("coolmarket://www.coolapk.com/u/")[1];
                uid = uid.Split(@"""")[0];
                return uid;
            }
            catch (Exception e)
            {
                return null;
            }
        }*/

        public static async Task<JObject> GetUserProfileByID(dynamic uid)
        {
            string result = await GetCoolApkMessage("https://api.coolapk.com/v6/user/profile?uid=" + uid);
            return (JObject)JsonConvert.DeserializeObject(result);
        }

        /**
            * 根據用戶名獲得用戶信息，失敗返回null
            *
            * @param name 用戶名
            * @return 用戶信息
            *
        public static async Task<JObject> GetUserProfileByName(string name)
        {
            try
            {
                string uid = await GetUserIDByName(name);
                return await GetUserProfileByID(uid);
            }
            catch (Exception)
            {
                return null;
            }
        }*/

        public static async Task<JArray> GetFeedListByID(dynamic uid, dynamic page, dynamic firstItem)
        {
            try
            {
                string str = await GetCoolApkMessage("https://api.coolapk.com/v6/user/feedList?uid=" + uid + "&page=" + page + "&firstItem=" + firstItem);
                JObject jo = (JObject)JsonConvert.DeserializeObject(str);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                return new JArray();
            }
        }

        public static async Task<JArray> GetIndexList(dynamic page, dynamic firstItem)
        {
            try
            {
                string str = await GetCoolApkMessage("https://api.coolapk.com/v6/main/indexV8?page=" + page);
                JObject jo = (JObject)JsonConvert.DeserializeObject(str);
                return (JArray)jo["data"];
            }
            catch (Exception)
            {
                return new JArray();
            }
        }
        /*
        public static DataItem GetFeedDetailByJson(string feed)
        {
            try
            {
                DataItem feed1 = JsonConvert.DeserializeObject<DataItem>(feed);
                return feed1;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<DataItem> getFeedDetailById(dynamic feedId)
        {
            try
            {
                string result = await GetCoolApkMessage("https://api.coolapk.com/v6/feed/detail?id=" + feedId);
                DataItem feed1 = JsonConvert.DeserializeObject<DataItem>(result);
                return feed1;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static async Task<string> GetCoolApkUserFaceUri(dynamic NameOrID)
        {
            String body = await Web.GetHttp("https://www.coolapk.com/u/" + NameOrID);
            body = Regex.Split(body, @"<div class=""msg_box"">")[1];
            body = Regex.Split(body, @"src=""")[1];
            return Regex.Split(body, @"""")[0];
        }
        */
    }
}