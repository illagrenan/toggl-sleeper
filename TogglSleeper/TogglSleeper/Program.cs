using System;
using System.Windows.Forms;

namespace TogglSleeper
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // http://stackoverflow.com/a/6486341/752142
            bool result;
            var mutex = new System.Threading.Mutex(true, System.AppDomain.CurrentDomain.FriendlyName, out result);

            if (!result)
            {
                // MessageBox.Show("Another instance is already running.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SettingsForm());

            // mutex shouldn't be released - important line
            GC.KeepAlive(mutex);
        }
    }
}