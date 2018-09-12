using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.EscapeGoat2 {
	public partial class SplitterSettings : UserControl {
		public bool SplitOnEnterPickup { get; set; }
		private bool isLoading;
		public SplitterSettings() {
			isLoading = true;
			InitializeComponent();

			SplitOnEnterPickup = false;

			isLoading = false;
		}

		private void Settings_Load(object sender, EventArgs e) {
			FindForm().Text = "Escape Goat 2 Autosplitter v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
			LoadSettings();
		}
		public void LoadSettings() {
			isLoading = true;
			this.flowMain.SuspendLayout();

			chkSplitOnEnterPickup.Checked = SplitOnEnterPickup;

			isLoading = false;
			this.flowMain.ResumeLayout(true);
		}
		public void ControlChanged(object sender, EventArgs e) {
			UpdateSplits();
		}
		public void UpdateSplits() {
			if (isLoading) return;

			SplitOnEnterPickup = chkSplitOnEnterPickup.Checked;
		}
		public XmlNode UpdateSettings(XmlDocument document) {
			XmlElement xmlSettings = document.CreateElement("Settings");

			XmlElement xmlSplit = document.CreateElement("SplitOnEnterPickup");
			xmlSplit.InnerText = SplitOnEnterPickup.ToString();
			xmlSettings.AppendChild(xmlSplit);

			return xmlSettings;
		}
		public void SetSettings(XmlNode settings) {
			XmlNode splitNode = settings.SelectSingleNode(".//SplitOnEnterPickup");
			SplitOnEnterPickup = false;
			if (splitNode != null && !string.IsNullOrEmpty(splitNode.InnerText)) {
				bool temp = false;
				if (bool.TryParse(splitNode.InnerText, out temp)) {
					SplitOnEnterPickup = temp;
				}
			}
		}
	}
}