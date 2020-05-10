namespace LiveSplit.EscapeGoat2 {
    partial class UserSettings {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.btnLog = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.tooltips = new System.Windows.Forms.ToolTip(this.components);
            this.chkSplitOnEnterPickup = new System.Windows.Forms.CheckBox();
            this.chkSheepRooms = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnLog
            // 
            this.btnLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLog.Location = new System.Drawing.Point(329, 4);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(69, 23);
            this.btnLog.TabIndex = 5;
            this.btnLog.TabStop = false;
            this.btnLog.Text = "Debug Log";
            this.tooltips.SetToolTip(this.btnLog, "View all entries in the Log");
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLog.Location = new System.Drawing.Point(404, 4);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(63, 23);
            this.btnClearLog.TabIndex = 6;
            this.btnClearLog.TabStop = false;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // chkLog
            // 
            this.chkLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(261, 8);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(65, 17);
            this.chkLog.TabIndex = 4;
            this.chkLog.Text = "Log Info";
            this.tooltips.SetToolTip(this.chkLog, "Will monitor thegames state and write it to the Log for Debug purposes");
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // chkSplitOnEnterPickup
            // 
            this.chkSplitOnEnterPickup.Location = new System.Drawing.Point(3, 31);
            this.chkSplitOnEnterPickup.Name = "chkSplitOnEnterPickup";
            this.chkSplitOnEnterPickup.Size = new System.Drawing.Size(353, 37);
            this.chkSplitOnEnterPickup.TabIndex = 7;
            this.chkSplitOnEnterPickup.Text = "Checked: Split on entering Door, obtaining Soul, or obtaining Shard    Not Checke" +
    "d: Split on fadeout of Room";
            this.chkSplitOnEnterPickup.UseVisualStyleBackColor = true;
            // 
            // chkSheepRooms
            // 
            this.chkSheepRooms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSheepRooms.AutoSize = true;
            this.chkSheepRooms.Location = new System.Drawing.Point(3, 10);
            this.chkSheepRooms.Name = "chkSheepRooms";
            this.chkSheepRooms.Size = new System.Drawing.Size(119, 17);
            this.chkSheepRooms.TabIndex = 8;
            this.chkSheepRooms.Text = "Sheep Room Patch";
            this.tooltips.SetToolTip(this.chkSheepRooms, "Allow sheep rooms to always be collectible");
            this.chkSheepRooms.UseVisualStyleBackColor = true;
            // 
            // UserSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkSheepRooms);
            this.Controls.Add(this.chkSplitOnEnterPickup);
            this.Controls.Add(this.chkLog);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.btnLog);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UserSettings";
            this.Size = new System.Drawing.Size(470, 73);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.ToolTip tooltips;
        private System.Windows.Forms.CheckBox chkSplitOnEnterPickup;
        private System.Windows.Forms.CheckBox chkSheepRooms;
    }
}
