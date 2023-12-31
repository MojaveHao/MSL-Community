﻿using System;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Controls;

namespace MSL.pages
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
            AppVersionLab.Content += string.Format("(msl v{0}-community)", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Hash.Text += Assembly.GetExecutingAssembly().GetHashCode().ToString();
            OSVersion.Text += Environment.OSVersion.ToString();
            CurrentPath.Text += Environment.CurrentDirectory.ToString();
            dotNetVersion.Text += Environment.Version.ToString();
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            RunAsAdmin.Text += IsAdmin.ToString();
        }
    }
}
