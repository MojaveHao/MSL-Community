using HandyControl.Controls;
using MSL.controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace MSL.pages
{
    /// <summary>
    /// FrpcPage.xaml 的交互逻辑
    /// </summary>
    public partial class FrpcPage : Page
    {
        public delegate void DelReadStdOutput(string result);
        public static Process FRPCMD = new Process();
        public event DelReadStdOutput ReadStdOutput;
        public bool add_log = true;
        public JArray jArray;
        string _dnfrpc;
        public FrpcPage()
        {
            MainWindow.AutoOpenFrpc += AutoStartFrpc;
            InitializeComponent();
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
        }

        private void StartFrpc()
        {
            string frpconfig = File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}MSL\frpc.ini");
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}MSL\\frpc_windows_amd64.exe") == false)
            {
                GetLatestOfc();
                var mwindow = (MainWindow)Window.GetWindow(this);
                _ = DialogShow.ShowDownload(mwindow, _dnfrpc, AppDomain.CurrentDomain.BaseDirectory + "MSL", "frpc.zip", "正在下载Frpc");
                _dnfrpc = "";
            }

            #region 传统方式启动
            if (frpconfig.IndexOf("[common]") + 1 != 0)
            {
                try
                {
                    startfrpc.Content = "关闭Frpc";
                    FRPCMD.StartInfo.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}MSL\frpc_windows_amd64.exe";
                    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "MSL");
                    FRPCMD.StartInfo.CreateNoWindow = true;
                    FRPCMD.StartInfo.UseShellExecute = false;
                    FRPCMD.StartInfo.RedirectStandardInput = true;
                    FRPCMD.StartInfo.RedirectStandardOutput = true;
                    FRPCMD.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    FRPCMD.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                    _ = FRPCMD.Start();
                    FRPCMD.BeginOutputReadLine();
                }
                catch (Exception e)
                {
                    _ = MessageBox.Show(e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            #endregion

            #region OpenFrp隧道启动
            else
            {
                try
                {
                    startfrpc.Content = "关闭Frpc";
                    FRPCMD.StartInfo.FileName = $@"{AppDomain.CurrentDomain.BaseDirectory}MSL\frpc_windows_amd64.exe";
                    FRPCMD.StartInfo.Arguments = frpconfig;
                    Directory.SetCurrentDirectory($"{AppDomain.CurrentDomain.BaseDirectory}MSL");
                    FRPCMD.StartInfo.CreateNoWindow = true;
                    FRPCMD.StartInfo.UseShellExecute = false;
                    FRPCMD.StartInfo.RedirectStandardInput = true;
                    FRPCMD.StartInfo.RedirectStandardOutput = true;
                    FRPCMD.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    FRPCMD.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                    _ = FRPCMD.Start();
                    FRPCMD.BeginOutputReadLine();
                }
                catch (Exception e)
                {
                    _ = MessageBox.Show(e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            #endregion

        }
        private void AutoStartFrpc()
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    startfrpc.Content = "关闭Frpc";
                }));
                FRPCMD.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc_windows_amd64.exe";
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "MSL");
                FRPCMD.StartInfo.CreateNoWindow = true;
                FRPCMD.StartInfo.UseShellExecute = false;
                FRPCMD.StartInfo.RedirectStandardInput = true;
                FRPCMD.StartInfo.RedirectStandardOutput = true;
                FRPCMD.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                _ = FRPCMD.Start();
                FRPCMD.BeginOutputReadLine();
            }
            catch (Exception e)
            {
                _ = MessageBox.Show(e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }//the same of above
        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _ = Dispatcher.Invoke(ReadStdOutput, new object[] { e.Data });
            }
        }
        private void ReadStdOutputAction(string msg)
        {
            if (add_log == true)
            {
                add_log = false;
                frpcOutlog.Text = frpcOutlog.Text + msg + "\n";
                if (msg.IndexOf("login") + 1 != 0)
                {
                    if (msg.IndexOf("failed") + 1 != 0)
                    {
                        frpcOutlog.Text += "桥接失败\n";
                        Growl.Error("桥接失败");
                        
                        if (msg.IndexOf("invalid meta token") + 1 != 0)
                        {
                            //frpcOutlog.Text += "QQ号密码填写错误或付费资格已过期,请重新配置或续费\n";
                        }
                        
                        else if (msg.IndexOf("user or meta token can not be empty") + 1 != 0)
                        {
                            frpcOutlog.Text += "用户名或密码不能为空\n";
                        }
                        else if (msg.IndexOf("i/o timeout") + 1 != 0)
                        {
                            frpcOutlog.Text += "连接超时,该节点可能下线\n";
                        }
                        try
                        {
                            FRPCMD.Kill();
                            Thread.Sleep(200);
                            FRPCMD.CancelOutputRead();
                            FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                            setfrpc.IsEnabled = true;
                            startfrpc.Content = "启动Frpc";
                        }
                        catch
                        {
                            try
                            {
                                FRPCMD.CancelOutputRead();
                                FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                            catch
                            {
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                        }
                    }
                    if (msg.IndexOf("success") + 1 != 0)
                    {
                        frpcOutlog.Text += "登录服务器成功！\n";
                    }
                    if (msg.IndexOf("match") + 1 != 0 && msg.IndexOf("token") + 1 != 0)
                    {
                        try
                        {
                            frpcOutlog.Text += "重新连接服务器\n";
                            Thread.Sleep(200);
                            //string frpcserver = GetFrpcIP().Substring(0, GetFrpcIP().IndexOf(".")) + "*";
                            //int frpcserver2 = GetFrpcIP().IndexOf(".") + 1;
                            WebClient MyWebClient = new WebClient
                            {
                                Credentials = CredentialCache.DefaultCredentials
                            };
                            byte[] pageData = MyWebClient.DownloadData(MainWindow.serverLink + @"/msl/CC/frpcserver.txt");
                            string @string = Encoding.UTF8.GetString(pageData);
                            //int IndexofA = @string.IndexOf(frpcserver);
                            //string Ru = @string.Substring(IndexofA + frpcserver2);
                            //string a111 = Ru.Substring(0, Ru.IndexOf("*"));
                            //byte[] pageData2 = new WebClient().DownloadData(a111);
                            //string pageHtml = Encoding.UTF8.GetString(pageData2);
                            string aaa = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\frpc.ini");
                            int IndexofA2 = aaa.IndexOf("token = ");
                            string Ru2 = aaa.Substring(IndexofA2);
                            string a112 = Ru2.Substring(0, Ru2.IndexOf("\n"));
                            //aaa = aaa.Replace(a112, "token = " + pageHtml);
                            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\frpc.ini", aaa);
                            FRPCMD.CancelOutputRead();
                            FRPCMD.OutputDataReceived -= p_OutputDataReceived;
                            StartFrpc();
                        }
                        catch (Exception aa)
                        {
                            _ = MessageBox.Show("Frpc桥接失败\n" + aa.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                            Growl.Error("Frpc桥接失败", "");
                            try
                            {
                                FRPCMD.CancelOutputRead();
                                FRPCMD.OutputDataReceived -= p_OutputDataReceived;
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                            catch
                            {
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                        }
                    }
                }
                if (msg.IndexOf("reconnect") + 1 != 0 && msg.IndexOf("error") + 1 != 0 && msg.IndexOf("token") + 1 != 0)
                {
                    try
                    {
                        frpcOutlog.Text += "重新连接服务器\n";
                        Thread.Sleep(200);
                        //string frpcserver = GetFrpcIP().Substring(0, GetFrpcIP().IndexOf(".")) + "*";
                        //int frpcserver2 = GetFrpcIP().IndexOf(".") + 1;
                        WebClient MyWebClient = new WebClient
                        {
                            Credentials = CredentialCache.DefaultCredentials
                        };
                        byte[] pageData = MyWebClient.DownloadData(MainWindow.serverLink + @"/msl/CC/frpcserver.txt");
                        string @string = Encoding.UTF8.GetString(pageData);
                        //int IndexofA = @string.IndexOf(frpcserver);
                        //string Ru = @string.Substring(IndexofA + frpcserver2);
                        //string a111 = Ru.Substring(0, Ru.IndexOf("*"));
                        //byte[] pageData2 = new WebClient().DownloadData(a111);
                        //string pageHtml = Encoding.UTF8.GetString(pageData2);
                        string aaa = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\frpc.ini");
                        int IndexofA2 = aaa.IndexOf("token = ");
                        string Ru2 = aaa.Substring(IndexofA2);
                        string a112 = Ru2.Substring(0, Ru2.IndexOf("\n"));
                        //aaa = aaa.Replace(a112, "token = " + pageHtml);
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\frpc.ini", aaa);
                        FRPCMD.CancelOutputRead();
                        FRPCMD.OutputDataReceived -= p_OutputDataReceived;
                        StartFrpc();
                    }
                    catch (Exception aa)
                    {
                        _ = MessageBox.Show("Frpc桥接失败\n" + aa.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        try
                        {
                            FRPCMD.CancelOutputRead();
                            FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                            setfrpc.IsEnabled = true;
                            startfrpc.Content = "启动Frpc";
                        }
                        catch
                        {
                            setfrpc.IsEnabled = true;
                            startfrpc.Content = "启动Frpc";
                        }
                    }
                }
                if (msg.IndexOf("start") + 1 != 0)
                {
                    if (msg.IndexOf("success") + 1 != 0)
                    {
                        frpcOutlog.Text += "Frpc桥接成功\n";
                    }
                    if (msg.IndexOf("error") + 1 != 0)
                    {
                        frpcOutlog.Text += "Frpc桥接失败\n";
                        if (msg.IndexOf("port already used") + 1 != 0) frpcOutlog.Text += "本地端口被占用\n";
                        else if (msg.IndexOf("port not allowed") + 1 != 0) frpcOutlog.Text += "远程端口被占用";
                        else if (msg.IndexOf("proxy name") + 1 != 0 && msg.IndexOf("already in use") + 1 != 0) frpcOutlog.Text += "此QQ号已被占用";
                        try
                        {
                            FRPCMD.Kill();
                            Thread.Sleep(200);
                            FRPCMD.CancelOutputRead();
                            //ReadStdOutput = null;
                            FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                            setfrpc.IsEnabled = true;
                            startfrpc.Content = "启动Frpc";
                        }
                        catch
                        {
                            try
                            {
                                FRPCMD.CancelOutputRead();
                                //ReadStdOutput = null;
                                FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                            catch
                            {
                                setfrpc.IsEnabled = true;
                                startfrpc.Content = "启动Frpc";
                            }
                        }
                    }
                }
            
            }
            else add_log = true;
            frpcOutlog.ScrollToEnd();
        }

        private void Setfrpc_Click(object sender, RoutedEventArgs e)
        {
            SetFrpc fw = new SetFrpc();
            var mainwindow = (MainWindow)Window.GetWindow(this);
            fw.Owner = mainwindow;
            _ = fw.ShowDialog();
            /*
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc.ini"))
                {
                    Thread thread = new Thread(GetFrpcInfo);
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
            */
        }

        /*
        void RefreshLink()
        {
            WebClient MyWebClient = new WebClient();
            byte[] pageData = MyWebClient.DownloadData(MainWindow.serverLink + @"/msl/otherdownload.json");
            string _javaList = Encoding.UTF8.GetString(pageData);

            JObject javaList0 = JObject.Parse(_javaList);
            _dnfrpc = javaList0["frpc"].ToString();
        }
        */

        private void GetLatestOfc()
        {
            string url = "https://of-dev-api.bfsea.xyz/commonQuery/get?key=software";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            string latest_url;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseMessage = reader.ReadToEnd();
                    JObject jo = (JObject)JsonConvert.DeserializeObject(responseMessage);
                    var latest = jo["data"]["latest"].ToString();
                    latest_url = $"https://d.of.gs/client{latest}frpc_windows_amd64.zip";
                    _ = DialogShow.ShowMsg((MainWindow)Window.GetWindow(this), "准备下载Frpc,可能需要等待一会\n获取到的下载链接:\n" + latest_url, "下载Frpc");
                    _dnfrpc = latest_url;
                    reader.Close();
                }
                response.Close();
                WebClient client = new WebClient();
                client.DownloadFile(latest_url, $@"{AppDomain.CurrentDomain.BaseDirectory}MSL\frpc.zip");
                string zipPath = $@"{AppDomain.CurrentDomain.BaseDirectory}MSL\frpc.zip";
                string extractPath = $@"{AppDomain.CurrentDomain.BaseDirectory}MSL\";

                ZipFile.ExtractToDirectory(zipPath, extractPath);
                // _ = DialogShow.ShowMsg((MainWindow)Window.GetWindow(this), extractPath, "debug");

            }
            catch (Exception ex)
            { var mwindow = (MainWindow)Window.GetWindow(this);
                if (ex.Message.Contains("病毒"))
                {
                    _ = DialogShow.ShowMsg(mwindow, "请在杀毒软件中将MSL文件夹添加至白名单中", "失败");
                }
                else if (ex.Message.Contains("已经存在"))
                {
                    ;
                }
                else
                {
                    _ = DialogShow.ShowMsg(mwindow, "下载失败\n" + ex.Message, "失败");
                }
            }
        }

        private void Startfrpc_Click(object sender, RoutedEventArgs e)
        {
            if (startfrpc.Content.ToString().Contains("启动"))
            {
                //DialogShow.ShowMsg((MainWindow)Window.GetWindow(this), $"{AppDomain.CurrentDomain.BaseDirectory}MSL\\frpc_windows_amd64.exe","debug"); 输出frpc应在路径
                //Frpc版本检测
                StreamReader reader = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "MSL\\config.json");
                JObject jobject2 = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                reader.Close();

                string url = "https://of-dev-api.bfsea.xyz/commonQuery/get?key=software";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader html_reader = new StreamReader(dataStream);
                        string responseMessage = html_reader.ReadToEnd();
                        JObject jo = (JObject)JsonConvert.DeserializeObject(responseMessage);
                        var latest = jo["data"]["latest_full"].ToString();
                    }
                }
                catch { }
                if (jobject2["frpcversion"] == null) {; } //此处预留为将来的版本检查
                if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}MSL\\frpc_windows_amd64.exe") == false)
                {
                    //RefreshLink();
                    GetLatestOfc();
                    _ = DialogShow.ShowDownload((MainWindow)Window.GetWindow(this), _dnfrpc, AppDomain.CurrentDomain.BaseDirectory + "MSL", "frpc.zip", "正在下载Frpc");
                    _dnfrpc = "";
                }

                StartFrpc();
                Growl.Success("Frpc启动成功");
                setfrpc.IsEnabled = false;
                frpcOutlog.Text = "启动中";
            }
            else
            {
                try
                {
                    FRPCMD.Kill();
                    Thread.Sleep(200);
                    Growl.Success("Frpc关闭成功");
                    FRPCMD.CancelOutputRead();
                    FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                    setfrpc.IsEnabled = true;
                    startfrpc.Content = "启动Frpc";
                }
                catch
                {
                    try
                    {
                        FRPCMD.CancelOutputRead();
                        FRPCMD.OutputDataReceived -= new DataReceivedEventHandler(p_OutputDataReceived);
                        setfrpc.IsEnabled = true;
                        startfrpc.Content = "启动Frpc";
                    }
                    catch
                    {
                        setfrpc.IsEnabled = true;
                        startfrpc.Content = "启动Frpc";
                    }
                }
            }
        }
        /*
        void GetFrpcInfo()
        {
            try
            {

                string configText = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc.ini");
                // 读取每一行
                string[] lines = configText.Split('\n');

                // 节点名称
                string nodeName = lines[0].TrimStart('#').Trim();

                // 服务器地址
                string serverAddr = "";
                int serverPort = 0;
                string remotePort = "";
                string frpcType = "";
                bool readServerInfo = true;  // 是否继续读取服务器信息
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("type") && frpcType != "")
                    {
                        // 遇到第二个type时停止读取服务器信息
                        readServerInfo = false;
                        break;
                    }
                    else if (lines[i].StartsWith("type") && readServerInfo) frpcType = lines[i].Split('=')[1].Trim();

                    else if (lines[i].StartsWith("server_addr") && readServerInfo) serverAddr = lines[i].Split('=')[1].Trim();

                    else if (lines[i].StartsWith("server_port") && readServerInfo) serverPort = int.Parse(lines[i].Split('=')[1].Trim());

                    else if (lines[i].StartsWith("remote_port") && readServerInfo) remotePort = lines[i].Split('=')[1].Trim();

                }

                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(serverAddr, 2000); // 替换成您要 ping 的 IP 地址
                if (reply.Status == IPStatus.Success)
                {
                    // 节点在线，可以获取延迟等信息
                    int roundTripTime = (int)reply.RoundtripTime;
                    Dispatcher.Invoke(new Action(delegate
                    {
                        //currentStat.Text = nodeName + "  延迟:" + roundTripTime + "ms";
                    }));
                }
                else
                {
                    // 节点离线
                    Dispatcher.Invoke(new Action(delegate
                    {
                        //currentStat.Text = nodeName + "  节点离线";
                    }));
                }
            }
            catch
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    //currentStat.Text = "获取节点信息失败";
                }));
            }
        }
        string GetFrpcIP()
        {
            string configText = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc.ini");
            // 读取每一行
            string[] lines = configText.Split('\n');

            // 服务器地址
            string serverAddr = "";
            bool readServerInfo = true;  // 是否继续读取服务器信息
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("server_addr") && readServerInfo)
                {
                    serverAddr = lines[i].Split('=')[1].Trim();
                    break;
                }
            }
            return serverAddr;
        }
        */

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                /*
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc.ini"))
                {
                    Thread thread = new Thread(GetFrpcInfo);
                    thread.Start();
                }
                else
                {
                    startfrpc.IsEnabled = false;
                    //currentStat.Text = "未检测到Frpc配置";
                }
                
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"MSL\frpc_windows_amd64.exe"))
                {
                    DialogShow.ShowMsg((MainWindow)Window.GetWindow(this), "未检测到Frpc,即将下载", "信息");
                }
                */
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show("出现错误\n"+ex.Message);
            }
        }
    }
}
