using System;
using System.Windows.Forms;

namespace TestMatchGame
{

    static class Program
    {
        [STAThread]

        static void Main()
        {
            Settings.InitVerticalLineImages();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
