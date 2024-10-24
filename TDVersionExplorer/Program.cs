using System;
using System.Windows.Forms;

namespace TDVersionExplorer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            if (true)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormExplorer());
            }
        }
    }
}
