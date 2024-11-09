using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TDVersionExplorer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Disable DPI scaling for the application
            if (Environment.OSVersion.Version.Major >= 6) // Check OS version for compatibility
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormExplorer());
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
