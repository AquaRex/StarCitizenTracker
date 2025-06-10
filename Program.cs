using System;
using System.Windows.Forms;
using StarCitizenTracker.Forms;
using StarCitizenTracker.Models;

namespace StarCitizenTracker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainLogFeed());
        }
    }
}
