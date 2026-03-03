using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace calc
{
    public class Form1 : Form
    {
        private WebView2 webView;

        // Dark title bar API
        [DllImport("dwmapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int dwAttribute,
            ref int pvAttribute,
            int cbAttribute);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

        public Form1()
        {
            Text = "Calculator ~M";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            ClientSize = new System.Drawing.Size(260, 383);

            BackColor = System.Drawing.Color.FromArgb(32, 32, 32);

            HandleCreated += Form1_HandleCreated;
            Load += Form1_Load;

            webView = new WebView2();
            webView.Dock = DockStyle.Fill;

            Controls.Add(webView);
        }

        private void Form1_HandleCreated(object sender, EventArgs e)
        {
            int dark = 1;

            // Try attribute 20 first (Win10 2004+ / Win11)
            if (DwmSetWindowAttribute(
                Handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref dark,
                sizeof(int)) != 0)
            {
                // Fallback for older builds
                DwmSetWindowAttribute(
                    Handle,
                    DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1,
                    ref dark,
                    sizeof(int));
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

            // Load embedded HTML
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "calc.index.html"; // namespace + filename

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show("Embedded HTML not found.");
                    return;
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                    webView.NavigateToString(html);
                }
            }
        }
    }
}