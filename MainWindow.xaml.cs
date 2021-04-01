using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Threading.Tasks;

namespace AutoWallpaper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon notifyIcon;
        public bool IsSetWallpaper = false;
        private static string file = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            SetNotifyIcon();
            InitWallpaper();
        }

        private async void InitWallpaper()
        {
            await Task.Run(() =>
            {
                file = BingImg.GetImgAndSetWallpaper(this);
            });
            System.Timers.Timer timer = new System.Timers.Timer();
            if (file == string.Empty)
            {
                timer.Enabled = true;
                timer.Interval = 60000;//执行间隔时间,单位为毫秒;此时时间间隔为1分钟  
                timer.Start();
                timer.Elapsed += new ElapsedEventHandler(CheckTime);
            }
            else
            {

                timer.Interval = 60000 * 60;
                BingImage.DataContext = file;
            }
        }

        private void SetNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Bing壁纸自动更换";//最小化到托盘时，鼠标点击时显示的文本
            notifyIcon.Icon = FromImageSource(Icon);//程序图标
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            BalloonTips("AutoWallpaperByBing");


            MenuItem open = new MenuItem("显示");
            open.Click += new EventHandler(Show);
            //退出菜单项
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(Close);
            //关于菜单项
            MenuItem about = new MenuItem("关于");
            about.Click += new EventHandler(About);

            //开机启动菜单项
            MenuItem start = new MenuItem("开机启动");
            start.RadioCheck = false;
            start.Click += new EventHandler(Start);

            //关联托盘控件
            MenuItem[] childen = new MenuItem[] { open, start, about, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);

            if (SetBootStartUp())
            {
                start.Checked = true;
            }

            //BingImage.ContextMenu = new System.Windows.Controls.ContextMenu();
        }
        public void BalloonTips(string msg)
        {
            notifyIcon.BalloonTipText = msg; //设置托盘提示显示的文本
            notifyIcon.ShowBalloonTip(500);
        }

        private void Start(object sender, EventArgs e)
        {
            var start = sender as MenuItem;
            try
            {
                if (!start.Checked) //设置开机自启动  
                {
                    //System.Windows.MessageBox.Show("设置开机自启动，需要修改注册表", "提示");
                    string path = System.Windows.Forms.Application.ExecutablePath;
                    RegistryKey rk = Registry.LocalMachine;
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    rk2.SetValue("AutoWallpaperStart", path);
                    rk2.Close();
                    rk.Close();
                    start.Checked = true;
                    BalloonTips("设置开机启动成功");
                }
                else //取消开机自启动  
                {
                    //System.Windows.Forms.MessageBox.Show("取消开机自启动，需要修改注册表", "提示");
                    string path = System.Windows.Forms.Application.ExecutablePath;
                    RegistryKey rk = Registry.LocalMachine;
                    RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                    rk2.DeleteValue("AutoWallpaperStart", false);
                    rk2.Close();
                    rk.Close();
                    start.Checked = false;
                    BalloonTips("取消开机启动成功");
                }
            }
            catch (Exception)
            {
                if (!start.Checked) //设置开机自启动  
                {
                    if (SetBootStartUp())
                    {
                        BalloonTips("设置开机启动成功");
                        start.Checked = true;
                        //System.Windows.Forms.MessageBox.Show("设置开机启动成功", "提示");
                    }
                    else
                    {
                        BalloonTips("设置开机启动失败");
                        //System.Windows.Forms.MessageBox.Show("设置开机启动失败", "提示");
                    }
                }
                else //取消开机自启动  
                {
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\AutoWallpaperByBing.lnk"))
                    {
                        try
                        {
                            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\AutoWallpaperByBing.lnk");
                            BalloonTips("取消开机启动成功");
                            start.Checked = false;
                            //System.Windows.Forms.MessageBox.Show("取消开机启动成功", "提示");
                        }
                        catch (Exception)
                        {
                        }
                    }
                    
                }
            }



        }

        public bool SetBootStartUp()
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\AutoWallpaperByBing.lnk"))
            {
                try
                {
                    CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "AutoWallpaperByBing", System.Windows.Forms.Application.ExecutablePath);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool CreateShortcut(string directory, string shortcutName, string targetPath,
    string description = null, string iconLocation = null)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
                shortcut.TargetPath = targetPath;//指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);//设置起始位置
                shortcut.WindowStyle = 1;//设置运行方式，默认为常规窗口
                shortcut.Description = description;//设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;//设置图标路径
                shortcut.Save();//保存快捷方式

                return true;
            }
            catch
            { }
            return false;
        }

        private void About(object sender, EventArgs e)
        {
            AboutWindow about_win = new AboutWindow();
            about_win.Show();
        }

        private void Show(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                WindowState = WindowState.Normal;
                if (BingImage.Source == null)
                {
                    BingImage.DataContext = file;
                }                
            }
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private static Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }
            Uri iconUri = new Uri(icon.ToString());
            return new Icon(System.Windows.Application.GetResourceStream(iconUri).Stream);
        }
        

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                WindowState = WindowState.Normal;
                if (BingImage.Source == null)
                {
                    BingImage.DataContext = file;
                }
            }
        }        

        private void Window_Closed(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
        }

        private static int thisPage = 0;
        private bool taskRunning = false;
        private async void LastButton_Click(object sender, RoutedEventArgs e)
        {
            if (taskRunning) return;
            if (thisPage < 8)
            {
                string imageUrl = string.Empty;
                taskRunning = true;
                await Task.Run(() =>
                {
                    imageUrl = BingImg.GetBingImageUrl(++thisPage);
                    imageUrl = BingImg.DownLoadImage(imageUrl);
                    taskRunning = false;
                });
                BingImage.DataContext = imageUrl;
                NextButton.IsEnabled = true;
            }
            if (thisPage >= 7)
            {
                LastButton.Visibility = Visibility.Hidden;
                LastTextBlock.Visibility = Visibility.Hidden;
            }
            if (thisPage > 0 && NextButton.Visibility == Visibility.Hidden)
            {
                NextButton.Visibility = Visibility.Visible;
                NextTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (taskRunning) return;
            if (thisPage > 0)
            {
                string imageUrl = string.Empty;
                taskRunning = true;
                await Task.Run(() =>
                {
                    imageUrl = BingImg.GetBingImageUrl(--thisPage);
                    imageUrl = BingImg.DownLoadImage(imageUrl);
                    taskRunning = false;
                });
                BingImage.DataContext = imageUrl;
            }
            if (thisPage == 0)
            {
                NextButton.Visibility = Visibility.Hidden;
                NextTextBlock.Visibility = Visibility.Hidden;
            }
            if (thisPage < 8 && LastButton.Visibility == Visibility.Hidden)
            {
                LastButton.Visibility = Visibility.Visible;
                LastTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void SaveAs_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileRoute = GetFileRouteByDialog();


            BitmapSource source = BingImage.Source as BitmapSource;
            if (source == null)
                return;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));

            FileStream fileStream = new FileStream(fileRoute, FileMode.Create, FileAccess.ReadWrite);
            encoder.Save(fileStream);
            fileStream.Close();
        }

        private void SetWallPaper_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = BingImage.Source as BitmapSource;
            if (source == null)
                return;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            var path = Directory.GetCurrentDirectory() + $"\\{DateTime.Now.ToString("yyyy-MM-dd")}.jpg";
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            encoder.Save(fileStream);
            fileStream.Close();
            BingImg.SetWallpaper(path);
        }

        private string GetFileRouteByDialog()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}.jpg"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "图片文件 |*.jpg"; // Filter files by extension
            var txtPlace = Directory.GetCurrentDirectory();
            var dir = Path.GetDirectoryName(txtPlace);
            dlg.InitialDirectory = dir;

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                txtPlace = dlg.FileName;
            }

            return txtPlace;
        }

        private void Move_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Application.Current.Shutdown();
            Hide();
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
            
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Minimized;
            }            
        }
        
        private void CheckTime(object sender, ElapsedEventArgs e)
        {
            var timer = sender as System.Timers.Timer;
            if (IsConnectedInternet())
            {
                Random rd = new Random(1);
                var index = rd.Next(0, 8);
                file = BingImg.GetImgAndSetWallpaper(this, index);            
            }
        }

        [System.Runtime.InteropServices.DllImport("wininet")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测本机是否联网
        /// </summary>
        /// <returns></returns>
        private static bool IsConnectedInternet()
        {
            int i = 0;
            if (InternetGetConnectedState(out i, 0))
            {
                //已联网
                return true;
            }
            else
            {
                //未联网
                return false;
            }
        }
    }
}
