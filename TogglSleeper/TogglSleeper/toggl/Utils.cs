using Microsoft.Win32;
using System.Windows.Forms;

namespace TogglSleeper.toggl
{
    internal class Utils
    {
        internal static class RegistryStartup
        {
            public static void EnableStartup()
            {
                RegistryKey registryKey = GetRegistryKey();
                registryKey.SetValue(System.AppDomain.CurrentDomain.FriendlyName, Application.ExecutablePath.ToString());
            }

            public static void DisableStartup()
            {
                RegistryKey registryKey = GetRegistryKey();
                registryKey.DeleteValue(System.AppDomain.CurrentDomain.FriendlyName, false);
            }

            private static RegistryKey GetRegistryKey()
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                
                return registryKey;
            }
        }
    }
}