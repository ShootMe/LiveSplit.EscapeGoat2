using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
namespace LiveSplit.EscapeGoat2 {
    public partial class UserSettings : UserControl {
        public SplitterSettings Settings { get; set; }
        private LiveSplitState State;
        private LogManager Log;

        public UserSettings(LiveSplitState state, LogManager log) {
            InitializeComponent();
            Settings = new SplitterSettings();
            State = state;
            Log = log;
            Dock = DockStyle.Fill;
        }

        public void AddXmlItem<T>(XmlDocument document, XmlElement xmlSettings, string name, T value) {
            XmlElement xmlItem = document.CreateElement(name);
            xmlItem.InnerText = value.ToString();
            xmlSettings.AppendChild(xmlItem);
        }
        public bool GetXmlBoolItem(XmlNode node, string path, bool defaultValue) {
            XmlNode item = node.SelectSingleNode(path);
            bool value = defaultValue;
            if (item != null) {
                bool.TryParse(item.InnerText, out value);
            }
            return value;
        }
        public XmlNode UpdateSettings(XmlDocument document) {
            XmlElement xmlSettings = document.CreateElement("Settings");

            AddXmlItem<bool>(document, xmlSettings, "LogInfo", chkLog.Checked);
            Log.EnableLogging = chkLog.Checked;

            AddXmlItem<bool>(document, xmlSettings, "SplitOnEnterPickup", chkSplitOnEnterPickup.Checked);
            Settings.SplitOnEnterPickup = chkSplitOnEnterPickup.Checked;

            return xmlSettings;
        }
        public void InitializeSettings(XmlNode node) {
            bool logInfo = GetXmlBoolItem(node, ".//LogInfo", false);
            chkLog.Checked = logInfo;
            Log.EnableLogging = logInfo;

            bool splitPickup = GetXmlBoolItem(node, ".//SplitOnEnterPickup", false);
            chkSplitOnEnterPickup.Checked = splitPickup;
            Settings.SplitOnEnterPickup = splitPickup;
        }
        private void Settings_Load(object sender, EventArgs e) {
            Form form = FindForm();
            form.Text = "Escape Goat 1/2 v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }
        private void btnLog_Click(object sender, EventArgs e) {
            DataTable dt = new DataTable();
            dt.Columns.Add("Event", typeof(string));
            try {
                if (File.Exists(LogManager.LOG_FILE)) {
                    using (StreamReader sr = new StreamReader(LogManager.LOG_FILE)) {
                        string line;
                        while (!string.IsNullOrEmpty(line = sr.ReadLine())) {
                            dt.Rows.Add(line);
                        }
                    }
                }

                using (LogViewer logViewer = new LogViewer() { DataSource = dt }) {
                    logViewer.ShowDialog(this);
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnClearLog_Click(object sender, EventArgs e) {
            Log.Clear(true);
            MessageBox.Show(this, "Debug Log has been cleared.", "Debug Log", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
}