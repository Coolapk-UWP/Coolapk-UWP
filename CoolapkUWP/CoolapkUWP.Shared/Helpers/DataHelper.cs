using CoolapkUWP.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace CoolapkUWP.Helpers
{
    public static partial class DataHelper
    {
        public static string GetMD5(this string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            using (MD5 md5Hasher = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

                string results = BitConverter.ToString(data).ToLowerInvariant();

                return results.Replace("-", "");
            }
        }

        public static string GetBase64(this string input, bool israw = false)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);
            if (israw) { result = result.Replace("=", ""); }
            return result;
        }

        public static string Reverse(this string text)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetSizeString(this double size)
        {
            int index = 0;
            while (index <= 11)
            {
                index++;
                size /= 1024;
                if (size > 0.7 && size < 716.8) { break; }
                else if (size >= 716.8) { continue; }
                else if (size <= 0.7)
                {
                    size *= 1024;
                    index--;
                    break;
                }
            }
            string str = string.Empty;
            switch (index)
            {
                case 0: str = "B"; break;
                case 1: str = "KB"; break;
                case 2: str = "MB"; break;
                case 3: str = "GB"; break;
                case 4: str = "TB"; break;
                case 5: str = "PB"; break;
                case 6: str = "EB"; break;
                case 7: str = "ZB"; break;
                case 8: str = "YB"; break;
                case 9: str = "BB"; break;
                case 10: str = "NB"; break;
                case 11: str = "DB"; break;
                default:
                    break;
            }
            return $"{size:0.##}{str}";
        }

        public static string GetNumString(this double num)
        {
            string str = string.Empty;
            if (num < 1000) { }
            else if (num < 10000)
            {
                str = "k";
                num /= 1000;
            }
            else if (num < 10000000)
            {
                str = "w";
                num /= 10000;
            }
            else
            {
                str = "kw";
                num /= 10000000;
            }
            return $"{num:N2}{str}";
        }

        public static string CSStoString(this string str)
        {
            try
            {
                HtmlToText HtmlToText = new HtmlToText();
                return HtmlToText.Convert(str);
            }
            catch
            {
                //换行和段落
                string s = str.Replace("<br>", "\n").Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br/>", "\n").Replace("<p>", "").Replace("</p>", "\n").Replace("&nbsp;", " ").Replace("<br />", "").Replace("<br />", "");
                //链接彻底删除！
                while (s.IndexOf("<a", StringComparison.Ordinal) > 0)
                {
                    s = s.Replace(@"<a href=""" + Regex.Split(Regex.Split(s, @"<a href=""")[1], @""">")[0] + @""">", "");
                    s = s.Replace("</a>", "");
                }
                return s;
            }
        }

        public static string ConvertJsonString(this string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = null;
            try { obj = serializer.Deserialize(jtr); } catch { }
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public static IBuffer GetBuffer(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBuffer();
            }
        }

        public static byte[] GetBytes(this IRandomAccessStream randomStream)
        {
            using (Stream stream = WindowsRuntimeStreamExtensions.AsStreamForRead(randomStream.GetInputStreamAt(0)))
            {
                return stream.GetBytes();
            }
        }

        public static IBuffer GetBuffer(this Stream stream)
        {
            byte[] bytes = new byte[0];
            if (stream != null)
            {
                bytes = stream.GetBytes();
            }
            return bytes.AsBuffer();
        }

        public static byte[] GetBytes(this Stream stream)
        {
            if (stream.CanSeek) // stream.Length 已确定
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return bytes;
            }
            else // stream.Length 不确定
            {
                int initialLength = 32768; // 32k

                byte[] buffer = new byte[initialLength];
                int read = 0;

                int chunk;
                while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += chunk;

                    if (read == buffer.Length)
                    {
                        int nextByte = stream.ReadByte();

                        if (nextByte == -1)
                        {
                            return buffer;
                        }

                        byte[] newBuffer = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }
                }

                byte[] ret = new byte[read];
                Array.Copy(buffer, ret, read);
                return ret;
            }
        }
    }
}
