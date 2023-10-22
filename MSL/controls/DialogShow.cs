using HandyControl.Controls;
using Window = System.Windows.Window;

namespace MSL.controls
{
    public class DialogShow
    {
        public static bool ShowMsg(Window window, string dialogText, string dialogTitle, bool primaryBtnVisible = false, string closeText = "确定", string primaryText = "确定")
        {
            try
            {
                _ = window.Focus();
                var dialog = Dialog.Show(string.Empty);
                MessageDialog messageDialog = new MessageDialog(window, dialogText, dialogTitle, primaryBtnVisible, closeText, primaryText)
                {
                    Owner = window
                };
                _ = messageDialog.ShowDialog();
                _ = window.Focus();
                dialog.Close();
                if (MessageDialog._dialogReturn)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }
        public static bool ShowInput(Window window, string dialogText, out string userInput, string textboxText = "")
        {
            userInput = string.Empty;
            try
            {
                _ = window.Focus();
                var dialog = Dialog.Show(string.Empty);
                InputDialog inputDialog = new InputDialog(window, dialogText, textboxText)
                {
                    Owner = window
                };
                _ = inputDialog.ShowDialog();
                _ = window.Focus();
                dialog.Close();
                if (InputDialog._dialogReturn)
                {
                    userInput = InputDialog._textReturn;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool ShowDownload(Window window, string downloadurl, string downloadPath, string filename, string downloadinfo)
        {
            try
            {
                _ = window.Focus();
                var dialog = Dialog.Show(string.Empty);
                DownloadWindow download = new DownloadWindow(downloadurl, downloadPath, filename, downloadinfo)
                {
                    Owner = window
                };
                _ = download.ShowDialog();
                _ = window.Focus();
                dialog.Close();
                if (DownloadWindow.isStopDwn)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

        }
    }
}
