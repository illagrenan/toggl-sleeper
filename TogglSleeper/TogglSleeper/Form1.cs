#region

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using TogglSleeper.Properties;
using TogglSleeper.toggl;

#endregion

namespace TogglSleeper
{
    public partial class SettingsForm : Form
    {
        // TODO http://stackoverflow.com/a/12657970/752142
        private readonly TogglApi _togglApi;

        public SettingsForm()
        {
            InitializeComponent();

            _togglApi = new TogglApi();

            textBox_userToken.Text = Settings.Default.ACCESS_TOKEN;
            checkBox1.Checked = Settings.Default.RUN_AT_STARTUP;

            if (textBox_userToken.Text == String.Empty)
            {
                button_testConnection.Enabled = false;
                button3.Enabled = false;

                ShowBallon("Toggl Sleeper", "Click on this icon and set your Toggl Access Token.");
            }

            SystemEvents.PowerModeChanged += OnPowerChange;
            notifyIcon1.BalloonTipClicked += notifyIcon_BalloonTipClicked;
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            RestoreFormFromTray();
        }

        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    StopRunningTask();
                    break;

                default:
                    // Do nothing
                    break;
            }
        }

        private void StopRunningTask()
        {
            try
            {
                int taskId = _togglApi.GetRunningTaskId();
                _togglApi.StopRunningTask(taskId);
            }
            catch (NoRunningTaskException)
            {
                Debug.WriteLine("There is no running task.");
            }
        }

        private void button_testConnection_Click(object sender, EventArgs e)
        {
            textBox_userToken.ReadOnly = true;

            try
            {
                String fullName = _togglApi.GetMe();

                MessageBox.Show("Hi " + fullName,
                    "Authorized",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                toolStripStatusLabel1.Text = "Access token is OK";
            }
            catch (ConnectionFailedException)
            {
                MessageBox.Show("Cannot connect to Toggl or given access token is not valid",
                    "Authorization / connection problem",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                toolStripStatusLabel1.Text = "Invalid Access token";
            }

            textBox_userToken.ReadOnly = false;
        }

        private void textBox_userToken_TextChanged(object sender, EventArgs e)
        {
            if (textBox_userToken.Text != String.Empty)
            {
                button_testConnection.Enabled = true;
                button3.Enabled = true;

                _togglApi.ChangeToken(textBox_userToken.Text);
            }
            else
            {
                button_testConnection.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void ShowBallon(string title, string text)
        {
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
            notifyIcon1.ShowBalloonTip(Settings.Default.BALLOON_TIMEOUT);
            notifyIcon1.Visible = true;
        }

        private void SettingsForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            MinimizeFormToTray();
        }

        private void MinimizeFormToTray()
        {
            WindowState = FormWindowState.Minimized;
            Hide();
            Visible = false;
            ShowInTaskbar = false;

            notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFormFromTray();
        }

        private void RestoreFormFromTray()
        {
            Visible = true;
            Show();
            WindowState = FormWindowState.Normal;

            Focus();

            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            MinimizeFormToTray();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                RegistryStartup.EnableStartup();
            }
            else
            {
                RegistryStartup.DisableStartup();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings.Default.ACCESS_TOKEN = textBox_userToken.Text;
            Settings.Default.RUN_AT_STARTUP = checkBox1.Checked;

            Settings.Default.Save();

            toolStripStatusLabel1.Text = "Settings saved";
        }

        private void notifyIcon1_MouseDoubleClick(object sender, EventArgs e)
        {
        }
    }
}