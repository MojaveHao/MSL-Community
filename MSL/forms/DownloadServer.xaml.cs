﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MSL.controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Window = HandyControl.Controls.Window;

namespace MSL.pages
{
    /// <summary>
    /// DownloadServer.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadServer : Window
    {
        //public static event DeleControl DownComplete;
        //List<string> serverurl = new List<string>();
        List<string> serverdownurl = new List<string>();
        //string autoupdate;
        //string mserversurl;
        public static string downloadServerBase;
        public static string downloadServerName;
        public static string downloadServerJava;
        //public static string autoupdateserver="&";
        public DownloadServer()
        {
            downloadServerName = string.Empty;
            InitializeComponent();
        }
        string downPath = "";
        string filename = "";
        //服务端下载
        private void serverlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (serverlist1.SelectedIndex != -1)
            {
                int url = serverlist1.SelectedIndex;
                //string filename = serverlist.SelectedItem.ToString();
                string downUrl = serverdownurl[url].ToString();

                //MessageBox.Show(downUrl);

                if (serverlist.SelectedItem.ToString().IndexOf("（") + 1 != 0)
                {
                    if (serverlist1.SelectedItem.ToString().IndexOf("（") + 1 != 0)
                    {
                        downPath = downloadServerBase;
                        filename = serverlist.SelectedItem.ToString().Substring(0, serverlist.SelectedItem.ToString().IndexOf("（")) + "-" + serverlist1.SelectedItem.ToString().Substring(0, serverlist1.SelectedItem.ToString().IndexOf("（")) + ".jar";
                    }
                    else
                    {
                        downPath = downloadServerBase;
                        filename = serverlist.SelectedItem.ToString().Substring(0, serverlist.SelectedItem.ToString().IndexOf("（")) + "-" + serverlist1.SelectedItem.ToString() + ".jar";
                    }

                }
                else
                {
                    if (serverlist1.SelectedItem.ToString().IndexOf("（") + 1 != 0)
                    {
                        downPath = downloadServerBase;
                        filename = serverlist.SelectedItem.ToString() + "-" + serverlist1.SelectedItem.ToString().Substring(0, serverlist1.SelectedItem.ToString().IndexOf("（")) + ".jar";
                    }
                    else
                    {
                        downPath = downloadServerBase;
                        filename = serverlist.SelectedItem.ToString() + "-" + serverlist1.SelectedItem.ToString() + ".jar";
                    }
                }
                bool dwnDialog = DialogShow.ShowDownload(this, downUrl, downPath, filename, "下载服务端中……");
                if (!dwnDialog)
                {
                    _ = DialogShow.ShowMsg(this, "下载取消！", "提示");
                    return;
                }
                if (File.Exists(downPath + @"\" + filename))
                {
                    if (filename.IndexOf("Forge") + 1 != 0)
                    {
                        _ = DialogShow.ShowMsg(this, "检测到您下载的是Forge端，开服器将自动进行安装操作，稍后请您不要随意移动鼠标且不要随意触碰键盘，耐心等待安装完毕！", "提示");
                        InstallForge();
                    }
                    else
                    {
                        downloadServerName = filename;
                        Close();
                    }
                }
                else
                {
                    _ = DialogShow.ShowMsg(this, "下载失败！", "错误");
                }
            }
        }
        void GetServer()
        {
            Ping pingSender = new Ping();
            string serverAddr = MainWindow.serverLink;
            if (serverAddr != "https://msl.waheal.top")
            {
                if (serverAddr.Contains("http://")) { serverAddr = serverAddr.Remove(0, 7); }
                PingReply reply = pingSender.Send(serverAddr, 2000); // 替换成您要 ping 的 IP 地址
                if (reply.Status != IPStatus.Success)
                {
                    MainWindow.serverLink = "https://msl.waheal.top";
                }
            }
            Dispatcher.Invoke(() =>
            {
                serverlist.ItemsSource = null;
                serverlist1.ItemsSource = null;
                //serverurl.Clear();
                serverdownurl = null;
            });
            try
            {
                /*
                string url;
                if (MainWindow.serverLink != "https://msl.waheal.top")
                {
                    url = MainWindow.serverLink + ":5000";
                }
                else
                {
                    url = "https://api.waheal.top";
                }
                WebClient webClient = new WebClient();
                //webClient.Encoding = Encoding.UTF8;
                webClient.Credentials = CredentialCache.DefaultCredentials;
                //byte[] pageData = webClient.DownloadData(MainWindow.serverLink + @"/msl/CC/getserver.txt");
                byte[] pageData = webClient.DownloadData(url);
                string jsonData = Encoding.UTF8.GetString(pageData);
                */
                string jsonData = Functions.Get("serverlist");
                string[] serverTypes = JsonConvert.DeserializeObject<string[]>(jsonData);
                Dispatcher.Invoke(() =>
                {
                    /*
                    foreach (var serverType in serverTypes)
                    {
                        serverlist.Items.Add(serverType);
                    }*/
                    serverlist.ItemsSource = serverTypes;

                    serverlist.SelectedIndex = 0;
                    getservermsg.Visibility = Visibility.Hidden;
                    lCircle.Visibility = Visibility.Hidden;
                });
            }
            catch (Exception a)
            {
                Dispatcher.Invoke(() =>
                {
                    getservermsg.Text = "获取服务端失败！请重试" + a.Message;
                    lCircle.Visibility = Visibility.Hidden;
                });
            }
            //return serverTypes;
            /*
            try
            {
                string pageHtml1 = "";
                try
                {
                    WebClient MyWebClient1 = new WebClient();
                    MyWebClient1.Credentials = CredentialCache.DefaultCredentials;
                    byte[] pageData1 = MyWebClient1.DownloadData(MainWindow.serverLink + @"/msl/CC/getserver.txt");
                    pageHtml1 = Encoding.UTF8.GetString(pageData1);
                }
                catch
                {
                    try
                    {
                        MainWindow.serverLink = "http://msl.waheal.top";
                        WebClient MyWebClient = new WebClient();
                        MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                        byte[] pageData = MyWebClient.DownloadData(MainWindow.serverLink + @"/msl/CC/getserver.txt");
                        pageHtml1 = Encoding.UTF8.GetString(pageData);
                    }
                    catch
                    {
                        MessageBox.Show("连接服务器失败！", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        pageHtml1 = "";
                        Close();
                    }
                }
                //MessageBox.Show(pageHtml1);
                int IndexofA0 = pageHtml1.IndexOf("*");
                string Ru0 = pageHtml1.Substring(IndexofA0 + 1);
                string pageHtml = Ru0.Substring(0, Ru0.IndexOf("*"));
                //MessageBox.Show(pageHtml);
                try
                {
                    mserversurl = pageHtml;
                    WebClient MyWebClient = new WebClient();
                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                    byte[] pageData = MyWebClient.DownloadData(mserversurl);
                    string aa = Encoding.UTF8.GetString(pageData);
                    //MessageBox.Show(servers);
                    //分类服务端
                    JObject jsonObject = JObject.Parse(aa);
                    //MessageBox.Show(jsonObject.ToString());
                    foreach (var x in jsonObject)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            serverlist.Items.Add(x.Key);
                        });
                        //MessageBox.Show( x.Value.ToString(), x.Key);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        serverlist.SelectedIndex = 0;
                        getservermsg.Visibility = Visibility.Hidden;
                        lCircle.Visibility = Visibility.Hidden;
                    });
                }
                catch
                {
                }
            }
            catch (Exception a)
            {
                Dispatcher.Invoke(() =>
                {
                    getservermsg.Text = "获取服务端失败！请重试" + a.Message;
                    lCircle.Visibility = Visibility.Hidden;
                    //File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"MSL/serverlist.json");
                });
            }
            */
        }

        private void serverlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (serverlist.Items.Count != 0)
            {
                Thread thread = new Thread(GetServerVersionList);
                thread.Start();
            }
        }
        void GetServerVersionList()
        {
            try
            {
                int serverName = 0;
                Dispatcher.Invoke(() =>
                {
                    serverlist1.ItemsSource = null;
                    //serverurl.Clear();
                    serverdownurl = null;
                    getservermsg.Visibility = Visibility.Visible;
                    lCircle.Visibility = Visibility.Visible;
                    serverName = serverlist.SelectedIndex;
                    //serverName = serverlist.SelectedItem.ToString();
                });
                try
                {
                    JObject patientinfo = new JObject
                    {
                        ["server_name"] = serverName
                    };
                    string sendData = JsonConvert.SerializeObject(patientinfo);
                    string resultData = Functions.Post("serverlist", 0, sendData);
                    JObject serverDetails = JObject.Parse(resultData);
                    List<JProperty> sortedProperties = serverDetails.Properties().OrderByDescending(p => Functions.VersionCompare(p.Name)).ToList();
                    Dispatcher.Invoke(() =>
                    {
                        serverlist1.ItemsSource = sortedProperties.Select(p => p.Name).ToList();
                        serverdownurl = sortedProperties.Select(p => p.Value.ToString()).ToList();
                        //serverlist.SelectedIndex = 0;
                        getservermsg.Visibility = Visibility.Hidden;
                        lCircle.Visibility = Visibility.Hidden;
                    });
                }
                catch
                {
                    try
                    {
                        JObject patientinfo = new JObject
                        {
                            ["server_name"] = serverName
                        };
                        string sendData = JsonConvert.SerializeObject(patientinfo);
                        string resultData = Functions.Post("serverlist", 0, sendData, "https://api.waheal.top");
                        JObject serverDetails = JObject.Parse(resultData);
                        List<JProperty> sortedProperties = serverDetails.Properties().OrderByDescending(p => Functions.VersionCompare(p.Name)).ToList();
                        Dispatcher.Invoke(() =>
                        {
                            serverlist1.ItemsSource = sortedProperties.Select(p => p.Name).ToList();
                            serverdownurl = sortedProperties.Select(p => p.Value.ToString()).ToList();
                            //serverlist.SelectedIndex = 0;
                            getservermsg.Visibility = Visibility.Hidden;
                            lCircle.Visibility = Visibility.Hidden;
                        });
                    }
                    catch (Exception a)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            getservermsg.Text = "获取服务端失败！请重试" + a.Message;
                            lCircle.Visibility = Visibility.Hidden;
                        });
                    }
                }
            }
            catch (Exception a)
            {
                Dispatcher.Invoke(() =>
                {
                    getservermsg.Text = "获取服务端失败！请重试" + a.Message;
                    lCircle.Visibility = Visibility.Hidden;
                });
            }
            /*
            Dispatcher.Invoke(() =>
            {
                try
                {
                    //MessageBox.Show(mserversurl);
                    //StreamReader reader = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + @"MSL/serverlist.json");
                    autoupdate = serverlist.SelectedItem.ToString();
                    WebClient MyWebClient = new WebClient();
                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                    byte[] pageData = MyWebClient.DownloadData(mserversurl);

                    string pageHtml = Encoding.UTF8.GetString(pageData);
                    //MessageBox.Show(servers);
                    //分类服务端
                    JObject jsonObject = JObject.Parse(pageHtml);
                    //MessageBox.Show(serverlist.SelectedItem.ToString());
                    string abc = serverlist.SelectedItem.ToString();
                    JObject jsonObject1 = (JObject)jsonObject[abc];
                    serverlist1.Items.Clear();
                    serverdownurl.Clear();
                    foreach (var x in jsonObject1)
                    {
                        serverlist1.Items.Add(x.Key);
                        serverdownurl.Add(x.Value.ToString());
                        //MessageBox.Show(x.Value.ToString(), x.Key);
                    }
                    pageHtml = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("获取下载链接失败！" + ex.Message);
                }
            });
            */
        }

        void InstallForge()
        {
            string forgeVersion;
            string serverDownUrl = serverdownurl[serverlist1.SelectedIndex].ToString();

            if (serverDownUrl.Contains("bmcl"))
            {
                Match match = Regex.Match(serverDownUrl, @"&version=([\w.-]+)&category");
                if (serverlist1.SelectedItem.ToString().Contains("-"))
                {
                    string version = serverlist1.SelectedItem.ToString().Split('-')[0];
                    forgeVersion = version + "-" + match.Groups[1].Value;
                }
                else
                {
                    forgeVersion = serverlist1.SelectedItem.ToString() + "-" + match.Groups[1].Value;
                }
            }
            else
            {
                Match match = Regex.Match(serverDownUrl, @"forge-([\w.-]+)-installer");
                forgeVersion = match.Groups[1].Value;
            }
            Directory.SetCurrentDirectory(downloadServerBase);
            Process process = new Process();
            process.StartInfo.FileName = downloadServerJava;
            process.StartInfo.Arguments = "-jar " + downPath + @"\" + filename + " -installServer";
            _ = process.Start();
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            try
            {

                while (!process.HasExited)
                {
                    Thread.Sleep(1000);
                }
                /*
                string text = File.ReadAllText(downloadPath + @"\" + "run.bat");
                text = text.Substring(text.IndexOf("java"), text.IndexOf("*") + 1- text.IndexOf("java"));
                text = text.Replace("java", "");
                text = text.Replace("@user_jvm_args.txt", "");*/
                if (File.Exists(downloadServerBase + "\\libraries\\net\\minecraftforge\\forge\\" + forgeVersion + "\\win_args.txt"))
                {
                    downloadServerName = "@libraries/net/minecraftforge/forge/" + forgeVersion + "/win_args.txt %*";
                    //CreateServer.isCreateForge = true;
                    Close();
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(downloadServerBase);
                    FileInfo[] fileInfo = directoryInfo.GetFiles();
                    foreach (FileInfo file in fileInfo)
                    {
                        if (file.Name.IndexOf("forge-" + forgeVersion) + 1 != 0)
                        {
                            downloadServerName = file.FullName.Replace(downloadServerBase + @"\", "");
                            break;
                        }
                        else
                        {
                            _ = DialogShow.ShowMsg(this, "下载失败,请多次尝试或使用代理再试！", "错误");
                        }
                    }
                    Close();
                }
            }
            catch
            {
                _ = DialogShow.ShowMsg(this, "下载失败！", "错误");
            }
        }

        private void openSpigot_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("https://www.spigotmc.org/");
        }

        private void openPaper_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("https://papermc.io/");
        }

        private void openMojang_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("https://www.minecraft.net/zh-hans/download/server");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(GetServer);
            thread.Start();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(GetServer);
            thread.Start();
        }
    }
}
