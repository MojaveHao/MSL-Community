﻿using HandyControl.Controls;
using HandyControl.Media.Effects;
using HandyControl.Themes;
using Microsoft.Win32;
using MSL.controls;
using MSL.forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static MSL.App;
using MessageBox = System.Windows.MessageBox;
using MessageDialog = MSL.controls.MessageDialog;

namespace MSL.pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : System.Windows.Controls.Page
    {
        public static event DeleControl C_NotifyIcon;
        public static event DeleControl DelBackground;
        public static event DeleControl ChangeTitleStyle;
        List<string> _runServerList = new List<string>();
        public SettingsPage()
        {
            InitializeComponent();
        }
        private void mulitDownthread_Click(object sender, RoutedEventArgs e)
        {
            DownloadWindow.downloadthread = int.Parse(downthreadCount.Text);
        }
        private void setdefault_Click(object sender, RoutedEventArgs e)
        {
            var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
            DialogShow.ShowMsg(mainwindow, "恢复默认设置会清除MSL文件夹内的所有文件，请您谨慎选择！", "警告", true, "取消");
            if (MessageDialog._dialogReturn)
            {
                MessageDialog._dialogReturn = false;
                try
                {
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"MSL", true);
                }
                catch
                {
                }
                Process.Start(Application.ResourceAssembly.Location);
                Process.GetCurrentProcess().Kill();
            }
        }
        private void notifyIconbtn_Click(object sender, RoutedEventArgs e)
        {
            if (notifyIconbtn.Content.ToString() == "托盘图标:打开")
            {
                notifyIconbtn.Content = "托盘图标:关闭";
                C_NotifyIcon();
                try
                {
                    string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject["notifyIcon"] = "False";
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                    Growl.Success("关闭成功！");
                    return;
                }
                catch
                {
                    Growl.Error("关闭失败！");
                    return;
                }
            }
            else
            {
                notifyIconbtn.Content = "托盘图标:打开";
                C_NotifyIcon();
                try
                {
                    string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                    JObject jobject = JObject.Parse(jsonString);
                    jobject["notifyIcon"] = "True";
                    string convertString = Convert.ToString(jobject);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                    Growl.Success("打开成功！");
                    return;
                }
                catch
                {
                    Growl.Error("打开失败！");
                    return;
                }
            }
        }
        private void developerSettings_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Window window = new DeveloperSettings();
            window.ShowDialog();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                JObject jsonObject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", Encoding.UTF8));
                if (jsonObject["notifyIcon"].ToString() == "True")
                {
                    notifyIconbtn.Content = "托盘图标:打开";
                }
                else
                {
                    notifyIconbtn.Content = "托盘图标:关闭";
                }
                if (jsonObject["autoOpenServer"].ToString() != "False")
                {
                    openserversOnStart.IsChecked = true;
                    openserversOnStartList.Text = jsonObject["autoOpenServer"].ToString();
                }
                if (jsonObject["autoOpenFrpc"].ToString() == "True")
                {
                    openfrpOnStart.IsChecked = true;
                }
                if (jsonObject["autoGetPlayerInfo"].ToString() == "True")
                {
                    autoGetPlayerInfo.IsChecked = true;
                }
                if (jsonObject["autoGetServerInfo"].ToString() == "True")
                {
                    autoGetServerInfo.IsChecked = true;
                }
                if (jsonObject["darkTheme"].ToString() == "True")
                {
                    autoSetTheme.IsChecked = false;
                    darkTheme.IsChecked = true;
                    darkTheme.IsEnabled = true;
                }
                else if (jsonObject["darkTheme"].ToString() == "False")
                {
                    autoSetTheme.IsChecked = false;
                    darkTheme.IsEnabled = true;
                }
                if (jsonObject["skin"].ToString() != "0")
                {
                    BlueSkinBtn.IsEnabled = true;
                    RedSkinBtn.IsEnabled = true;
                    GreenSkinBtn.IsEnabled = true;
                    OrangeSkinBtn.IsEnabled = true;
                    PurpleSkinBtn.IsEnabled = true;
                    PinkSkinBtn.IsEnabled = true;
                    switch (jsonObject["skin"].ToString())
                    {
                        case "1":
                            autoSetTheme.IsChecked = false;
                            BlueSkinBtn.IsChecked = true;
                            break;
                        case "2":
                            autoSetTheme.IsChecked = false;
                            RedSkinBtn.IsChecked = true;
                            break;
                        case "3":
                            autoSetTheme.IsChecked = false;
                            GreenSkinBtn.IsChecked = true;
                            break;
                        case "4":
                            autoSetTheme.IsChecked = false;
                            OrangeSkinBtn.IsChecked = true;
                            break;
                        case "5":
                            autoSetTheme.IsChecked = false;
                            PurpleSkinBtn.IsChecked = true;
                            break;
                        case "6":
                            autoSetTheme.IsChecked = false;
                            PinkSkinBtn.IsChecked = true;
                            break;
                    }
                }
                if (jsonObject["semitransparentTitle"].ToString() == "True")
                {
                    semitransparentTitle.IsChecked = true;
                }
                serverListBox.Items.Clear();
                try
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"MSL\ServerList.json"))
                    {
                        JObject _json = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\ServerList.json", Encoding.UTF8));
                        foreach (var item in _json)
                        {
                            serverListBox.Items.Add(item.Value["name"]);
                            _runServerList.Add(item.Key);
                            serverListBox.SelectedIndex = 0;
                        }
                    }
                }
                catch { return; }
            }
            catch(Exception ex)
            {
                Growl.Error("Err\n" + ex.Message);
            }
        }

        private void useidea_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://docs.waheal.top/#/?id=msl%e5%bc%80%e6%9c%8d%e6%95%99%e7%a8%8b");
        }

        private void openserversOnStart_Click(object sender, RoutedEventArgs e)
        {
            if (openserversOnStart.IsChecked == true)
            {
                if (openserversOnStartList.Text == "")
                {
                    Growl.Error("请先将服务器添加至启动列表！");
                    openserversOnStart.IsChecked = false;
                    return;
                }
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoOpenServer"] = openserversOnStartList.Text;
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("开启成功！");
            }
            else
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoOpenServer"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("关闭成功！");
            }
        }

        private void openfrpOnStart_Click(object sender, RoutedEventArgs e)
        {
            if (openfrpOnStart.IsChecked == true)
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoOpenFrpc"] = "True";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("开启成功！");
            }
            else
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoOpenFrpc"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("关闭成功！");
            }
        }

        private void ChangeSkin(object sender, RoutedEventArgs e)
        {
            if (BlueSkinBtn.IsChecked == true)
            {
                BrushConverter brushConverter = new BrushConverter();
                ThemeManager.Current.AccentColor = (System.Windows.Media.Brush)brushConverter.ConvertFromString("#0078D4");
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "1";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
            else if (RedSkinBtn.IsChecked == true)
            {
                ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Red;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "2";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
            else if (GreenSkinBtn.IsChecked == true)
            {
                ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Green;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "3";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
            else if (OrangeSkinBtn.IsChecked == true)
            {
                ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Orange;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "4";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
            else if (PurpleSkinBtn.IsChecked == true)
            {
                ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Purple;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "5";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
            else if (PinkSkinBtn.IsChecked == true)
            {
                ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.DeepPink;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["skin"] = "6";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("皮肤切换成功！");
            }
        }

        private void autoGetPlayerInfo_Click(object sender, RoutedEventArgs e)
        {
            if (autoGetPlayerInfo.IsChecked == true)
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoGetPlayerInfo"] = "True";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("开启成功！");
                MainWindow.getPlayerInfo = true;
            }
            else
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoGetPlayerInfo"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("关闭成功！");
                MainWindow.getPlayerInfo = false;
            }
        }

        private void autoGetServerInfo_Click(object sender, RoutedEventArgs e)
        {
            if (autoGetServerInfo.IsChecked == true)
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoGetServerInfo"] = "True";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("开启成功！");
                MainWindow.getServerInfo = true;
            }
            else
            {
                string jsonString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", System.Text.Encoding.UTF8);
                JObject jobject = JObject.Parse(jsonString);
                jobject["autoGetServerInfo"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\config.json", convertString, System.Text.Encoding.UTF8);
                Growl.Success("关闭成功！");
                MainWindow.getServerInfo = false;
            }
        }

        private void autoSetTheme_Click(object sender, RoutedEventArgs e)
        {
            if (autoSetTheme.IsChecked == true)
            {
                //ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.DeepPink;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["darkTheme"] = "Auto";
                jobject["skin"] = "0";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);

                Growl.Success("开启成功！");
                ThemeManager.Current.UsingSystemTheme = true;
                BlueSkinBtn.IsChecked= false;
                darkTheme.IsChecked = false;

                BlueSkinBtn.IsEnabled = false;
                RedSkinBtn.IsEnabled = false;
                GreenSkinBtn.IsEnabled = false;
                OrangeSkinBtn.IsEnabled = false;
                PurpleSkinBtn.IsEnabled = false;
                PinkSkinBtn.IsEnabled = false;
                darkTheme.IsEnabled = false;
            }
            else
            {
                //ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.DeepPink;
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["darkTheme"] = "False";
                jobject["skin"] = "1";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);

                Growl.Success("关闭成功！");
                ThemeManager.Current.UsingSystemTheme = false;
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                BlueSkinBtn.IsChecked = true;

                BlueSkinBtn.IsEnabled = true;
                RedSkinBtn.IsEnabled = true;
                GreenSkinBtn.IsEnabled = true;
                OrangeSkinBtn.IsEnabled = true;
                PurpleSkinBtn.IsEnabled = true;
                PinkSkinBtn.IsEnabled = true;
                darkTheme.IsEnabled = true;
            }
        }

        private void darkTheme_Click(object sender, RoutedEventArgs e)
        {
            if (darkTheme.IsChecked == true)
            {
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["darkTheme"] = "True";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("开启成功！");
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            else
            {
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["darkTheme"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("关闭成功！");
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
        }
        private void semitransparentTitle_Click(object sender, RoutedEventArgs e)
        {
            if (semitransparentTitle.IsChecked == true)
            {
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["semitransparentTitle"] = "True";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("开启成功！"); 
                ChangeTitleStyle();
            }
            else
            {
                JObject jobject = JObject.Parse(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", Encoding.UTF8));
                jobject["semitransparentTitle"] = "False";
                string convertString = Convert.ToString(jobject);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json", convertString, Encoding.UTF8);
                Growl.Success("关闭成功！"); 
                ChangeTitleStyle();
            }
        }

        private void paintedEgg_Click(object sender, RoutedEventArgs e)
        {
            var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
            DialogShow.ShowMsg(mainwindow, "点击此按钮后软件出现任何问题作者概不负责，你确定要继续吗？\n（光敏性癫痫警告！若您患有光敏性癫痫，请不要点击下面任何按钮，并立即使用任务管理器结束此程序或重启电脑！）", "警告", true, "确定");
            MessageDialog._dialogReturn = false;
            ThemeManager.Current.UsingSystemTheme = false;
            Thread thread = new Thread(PaintedEgg);
            thread.Start();
        }
        void PaintedEgg()
        {
            System.Windows.Window mainwindow=null;
            this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
            {
                mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
            });
            while (true)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    mainwindow.Background = System.Windows.Media.Brushes.LightBlue;
                    BrushConverter brushConverter = new BrushConverter();
                    ThemeManager.Current.AccentColor = (System.Windows.Media.Brush)brushConverter.ConvertFromString("#0078D4");
                });
                Thread.Sleep(200);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Red;
                });
                Thread.Sleep(200);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    mainwindow.Background = System.Windows.Media.Brushes.LightGreen;
                    ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Green;
                });
                Thread.Sleep(200);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    mainwindow.Background = System.Windows.Media.Brushes.LightYellow;
                    ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Orange;
                });
                Thread.Sleep(200);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.Purple;
                });
                Thread.Sleep(200);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (ThreadStart)delegate ()
                {
                    mainwindow.Background = System.Windows.Media.Brushes.LightPink;
                    ThemeManager.Current.AccentColor = System.Windows.Media.Brushes.DeepPink;
                });
                Thread.Sleep(200);
            }
        }

        private void changeBackImg_Click(object sender, RoutedEventArgs e)
        {
            var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
            DialogShow.ShowMsg(mainwindow, "该功能搭配暗色模式使用，效果最佳", "提示");
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openfile.Title = "请选择文件";
            openfile.Filter = "所有文件类型|*.*";
            var res = openfile.ShowDialog();
            if (res == true)
            {
                File.Copy(openfile.FileName, AppDomain.CurrentDomain.BaseDirectory + "MSL\\Background.png", true);
                mainwindow.Background = new ImageBrush(GetImage(AppDomain.CurrentDomain.BaseDirectory + "MSL\\Background.png"));     // path为图片路径new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "MSL\\Background.png")));
                mainwindow.SideMenuBorder.BorderThickness = new Thickness(0);
            }
        }
        public static BitmapImage GetImage(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            if (File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
            }
            return bitmap;
        }

        private void delBackImg_Click(object sender, RoutedEventArgs e)
        {
            var mainwindow = (MainWindow)System.Windows.Window.GetWindow(this);
            mainwindow.SetResourceReference(BackgroundProperty, "BackgroundBrush");
            mainwindow.SideMenuBorder.BorderThickness = new Thickness(0, 0, 1, 0);
            try
            {
                DelBackground();
            }
            catch { }
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "MSL\\Background.png");
        }

        private void addServerToRunlist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                openserversOnStartList.Text += _runServerList[serverListBox.SelectedIndex] + ",";
            }
            catch
            {
                Growl.Error("出现错误，您是否选择了一个服务器？");
            }
        }
    }
}