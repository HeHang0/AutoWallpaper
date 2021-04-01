using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AutoWallpaper
{
    public sealed class BingImg
    {
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static string GetImgAndSetWallpaper(MainWindow win, int index=0)
        {
            try
            {
                var ImgUrl = GetBingImageUrl(index);
                var FileRoute = DownLoadImage(ImgUrl);
                if (SetWallpaper(FileRoute))
                {
                    win.IsSetWallpaper = true;
                }

                return FileRoute;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            
        }

        public static BitmapImage ShowImageInWindow(string ImgUrl)
        {
            // Create source.  
            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = new Uri(ImgUrl, UriKind.RelativeOrAbsolute);
            bi.EndInit();

            return bi;
        }

        public static bool SetWallpaper(string path)
        {
            try
            {
                SystemParametersInfo(20, 0, path, 0x01 | 0x02);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string DownLoadImage(string url)
        {
            var filename = $"{Environment.GetEnvironmentVariable("TEMP")}\\{Md5Func(url)}.jpg";
            if (File.Exists(filename))
            {
                return filename;
            }
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, filename);
                return filename;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Md5Func(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = string.Empty;
            for (int i = 0; i < md5Data.Length; i++)
            {
                //返回一个新字符串，该字符串通过在此实例中的字符左侧填充指定的 Unicode 字符来达到指定的总长度，从而使这些字符右对齐。
                // string num=12; num.PadLeft(4, '0'); 结果为为 '0012' 看字符串长度是否满足4位,不满足则在字符串左边以"0"补足
                //调用Convert.ToString(整型,进制数) 来转换为想要的进制数
                destString += Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            //使用 PadLeft 和 PadRight 进行轻松地补位
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        public static string GetBingImageUrl(int idx = 0, int n = 1)
        {
            string url = string.Format(api, idx, n);
            var jsonStr = GetHttpData(url);
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonStr);
            string urlBase = jo["images"][0]["urlbase"].ToString().Replace("/th?id=", "");
            return $"https://www.bing.com/th?id={urlBase}_UHD.jpg&rf=LaDigue_UHD.jpg&pid=hp&w=3840&h=2160&rs=1&c=4";
        }

        public static string GetHttpData(string uri)
        {
            Uri url = new Uri(uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            reader.Close();
            stream.Close();
            return result;
        }

        public static string api { get; set; } = "http://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}";
    }
}
