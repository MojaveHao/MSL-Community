using System.Windows;
using Window = HandyControl.Controls.Window;

namespace MSL.controls
{
    /// <summary>
    /// MessageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : Window
    {
        public static bool _dialogReturn;
        public MessageDialog(System.Windows.Window window, string dialogText, string dialogTitle, bool primaryBtnVisible, string closeText, string primaryText)
        {
            InitializeComponent();
            MaxHeight = window.ActualHeight;
            MaxWidth = window.ActualWidth - 200;
            _dialogReturn = false;
            titleText.Text = dialogTitle;
            bodyText.Text = dialogText;
            closeBtn.Content = closeText;
            primaryBtn.Content = primaryText;
            if (!primaryBtnVisible)
            {
                primaryBtn.Visibility = Visibility.Hidden;
            }
        }

        private void primaryBtn_Click(object sender, RoutedEventArgs e)
        {
            _dialogReturn = true;
            Close();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            _dialogReturn = false;
            Close();
        }
    }
}