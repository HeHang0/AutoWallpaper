using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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

        public static string GetImgAndSetWallpaper(MainWindow win)
        {
            try
            {
                var ImgUrl = GetBingImageUrl();
                var FileRoute = DownLoadImage(ImgUrl);
                if (SetWallpaper(FileRoute))
                {
                    win.IsSetWallpaper = true;
                }

                return ImgUrl;
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

        private static string DownLoadImage(string url)
        {
            WebClient webClient = new WebClient();
            var filename = $"{DateTime.Now.ToString("yyyy-MM-dd")}.jpg";
            webClient.DownloadFile(url, filename);
            return Directory.GetCurrentDirectory() + "\\" + filename;
        }

        public static string GetBingImageUrl(int idx = 0, int n = 1)
        {
            string url = string.Format(api, idx, n);
            var jsonStr = GetHttpData(url);
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonStr);

            return "https://www.bing.com" + jo["images"][0]["url"].ToString();
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
