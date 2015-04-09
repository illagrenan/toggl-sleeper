using Microsoft.Win32;
using System;
using System.Windows.Forms;
using TogglSleeper.Properties;
using TogglSleeper.toggl;

namespace TogglSleeper
{
    public partial class SettingsForm : Form
    {
        // TODO http://stackoverflow.com/a/12657970/752142
        private TogglApi togglApi;

        public SettingsForm()
        {
            InitializeComponent();

            this.togglApi = new TogglApi();

            this.textBox_userToken.Text = Settings.Default.ACCESS_TOKEN;
            this.checkBox1.Checked = Settings.Default.RUN_AT_STARTUP;

            if (this.textBox_userToken.Text == String.Empty)
            {
                this.button_testConnection.Enabled = false;
                this.button3.Enabled = false;

                this.showBallon("Toggl Sleeper", "Click on this icon and set your Toggl Access Token.");
            }

            SystemEvents.PowerModeChanged += OnPowerChange;
            this.notifyIcon1.BalloonTipClicked += new EventHandler(notifyIcon_BalloonTipClicked);
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this.restoreFormFromTray();
        }

        private void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    this.stopRunningTask();
                    break;

                default:
                    // Do nothing
                    break;
            }
        }

        private void stopRunningTask()
        {
            try
            {
                int taskId = this.togglApi.GetRunningTaskId();
                this.togglApi.StopRunningTask(taskId);
            }
            catch (NoRunningTaskException)
            {
                System.Diagnostics.Debug.WriteLine("There is no running task.");
            }
        }

        private void button_testConnection_Click(object sender, EventArgs e)
        {
            this.textBox_userToken.ReadOnly = true;

            try
            {
                String fullName = this.togglApi.GetMe();

                MessageBox.Show("Hi " + fullName,
                                "Authorized",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                this.toolStripStatusLabel1.Text = "Access token is OK";
            }
            catch (ConnectionFailedException)
            {
                MessageBox.Show("Cannot connect to Toggl or given access token is not valid",
                                "Authorization / connection problem",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                this.toolStripStatusLabel1.Text = "Invalid Access token";
            }

            this.textBox_userToken.ReadOnly = false;
        }

        private void textBox_userToken_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_userToken.Text != String.Empty)
            {
                this.button_testConnection.Enabled = true;
                this.button3.Enabled = true;

                this.togglApi.ChangeToken(this.textBox_userToken.Text);
            }
            else
            {
                this.button_testConnection.Enabled = false;
                this.button3.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.stopRunningTask();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.togglApi.StopRunningTask(Int32.Parse(this.textBox1.Text));
        }

        private void showBallon(string title, string text)
        {
            this.notifyIcon1.BalloonTipTitle = title;
            this.notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
            this.notifyIcon1.ShowBalloonTip(Settings.Default.BALLOON_TIMEOUT);
            this.notifyIcon1.Visible = true;
        }

        private void SettingsForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            this.minimizeFormToTray();
        }

        private void minimizeFormToTray()
        {
            WindowState = FormWindowState.Minimized;
            this.Hide();
            this.Visible = false;
            this.ShowInTaskbar = false;

            this.notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.restoreFormFromTray();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (WindowState == FormWindowState.Minimized)
                    {
                        this.restoreFormFromTray();
                    }
                    break;
            }
        }

        private void restoreFormFromTray()
        {
            this.Visible = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;

            this.Focus();

            this.ShowInTaskbar = true;
            this.notifyIcon1.Visible = false;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.minimizeFormToTray();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
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
            Settings.Default.ACCESS_TOKEN = this.textBox_userToken.Text;
            Settings.Default.RUN_AT_STARTUP = this.checkBox1.Checked;

            Properties.Settings.Default.Save();

            this.toolStripStatusLabel1.Text = "Settings saved";
        }

        private void notifyIcon1_MouseDoubleClick(object sender, EventArgs e)
        {
        }
    }
}