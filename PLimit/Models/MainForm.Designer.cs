namespace PLimit
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            systemMonitorPanel = new PLimit.Utils.SystemMonitorPanel();
            processesListBox = new DoubleBufferedListView();
            processName = new ColumnHeader();
            processId = new ColumnHeader();
            priority = new ColumnHeader();
            afinity = new ColumnHeader();
            ioPriority = new ColumnHeader();
            boost = new ColumnHeader();
            efficiencyMode = new ColumnHeader();
            storedSettings = new ColumnHeader();
            wdtpb = new ColumnHeader();
            userRunning = new ColumnHeader();
            searchProcessBtn = new Button();
            searchProcessTxt = new TextBox();
            refreshProcessListBtn = new Button();
            actionMenuStrip = new ContextMenuStrip(components);
            boostToolStripMenuItem = new ToolStripMenuItem();
            enableToolStripMenuItem = new ToolStripMenuItem();
            disableToolStripMenuItem = new ToolStripMenuItem();
            iOPriorityToolStripMenuItem = new ToolStripMenuItem();
            veryLowToolStripMenuItem = new ToolStripMenuItem();
            lowToolStripMenuItem = new ToolStripMenuItem();
            normalToolStripMenuItem = new ToolStripMenuItem();
            highToolStripMenuItem = new ToolStripMenuItem();
            priorityToolStripMenuItem = new ToolStripMenuItem();
            realTimedangerToolStripMenuItem = new ToolStripMenuItem();
            highToolStripMenuItem1 = new ToolStripMenuItem();
            aboveNormalToolStripMenuItem = new ToolStripMenuItem();
            normalToolStripMenuItem1 = new ToolStripMenuItem();
            belowNormalToolStripMenuItem = new ToolStripMenuItem();
            windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem = new ToolStripMenuItem();
            enableToolStripMenuItem2 = new ToolStripMenuItem();
            disableToolStripMenuItem2 = new ToolStripMenuItem();
            afinityToolStripMenuItem = new ToolStripMenuItem();
            efficiencyModeToolStripMenuItem = new ToolStripMenuItem();
            enableToolStripMenuItem1 = new ToolStripMenuItem();
            disableToolStripMenuItem1 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            showSavedSettingsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            killProcessToolStripMenuItem = new ToolStripMenuItem();
            deleteSavedSettingsToolStripMenuItem = new ToolStripMenuItem();
            reloadProcess = new System.Windows.Forms.Timer(components);
            countProcessesLbl = new Label();
            checkBox1 = new CheckBox();
            SaveSettingsCkb = new CheckBox();
            actionMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // systemMonitorPanel
            // 
            systemMonitorPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            systemMonitorPanel.BackColor = Color.FromArgb(22, 22, 22);
            systemMonitorPanel.Location = new Point(12, 527);
            systemMonitorPanel.Name = "systemMonitorPanel";
            systemMonitorPanel.Size = new Size(1290, 76);
            systemMonitorPanel.TabIndex = 11;
            // 
            // processesListBox
            // 
            processesListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            processesListBox.Columns.AddRange(new ColumnHeader[] { processName, processId, priority, afinity, ioPriority, boost, efficiencyMode, storedSettings, wdtpb, userRunning });
            processesListBox.FullRowSelect = true;
            processesListBox.HideSelection = true;
            processesListBox.Location = new Point(12, 41);
            processesListBox.MultiSelect = false;
            processesListBox.Name = "processesListBox";
            processesListBox.OwnerDraw = true;
            processesListBox.Size = new Size(1290, 480);
            processesListBox.Sorting = SortOrder.Ascending;
            processesListBox.TabIndex = 1;
            processesListBox.UseCompatibleStateImageBehavior = false;
            processesListBox.View = View.Details;
            processesListBox.MouseClick += processesListBox_MouseClick;
            processesListBox.MouseHover += processesListBox_MouseHover;
            // 
            // processName
            // 
            processName.Text = "Name";
            processName.Width = 250;
            // 
            // processId
            // 
            processId.Text = "PID";
            processId.Width = 119;
            // 
            // priority
            // 
            priority.Text = "Priority";
            priority.Width = 120;
            // 
            // afinity
            // 
            afinity.Text = "Afinity";
            afinity.Width = 120;
            // 
            // ioPriority
            // 
            ioPriority.Text = "I/O Priority";
            ioPriority.Width = 120;
            // 
            // boost
            // 
            boost.Text = "Boost";
            boost.Width = 90;
            // 
            // efficiencyMode
            // 
            efficiencyMode.Text = "Efficiency Mode";
            efficiencyMode.Width = 120;
            // 
            // storedSettings
            // 
            storedSettings.Text = "Stored Settings";
            storedSettings.TextAlign = HorizontalAlignment.Center;
            storedSettings.Width = 120;
            // 
            // wdtpb
            // 
            wdtpb.Text = "WDTPB";
            wdtpb.TextAlign = HorizontalAlignment.Center;
            wdtpb.Width = 90;
            // 
            // userRunning
            // 
            userRunning.Text = "User";
            userRunning.TextAlign = HorizontalAlignment.Center;
            userRunning.Width = 130;
            // 
            // searchProcessBtn
            // 
            searchProcessBtn.Anchor = AnchorStyles.Top;
            searchProcessBtn.Enabled = false;
            searchProcessBtn.Location = new Point(676, 12);
            searchProcessBtn.Name = "searchProcessBtn";
            searchProcessBtn.Size = new Size(75, 23);
            searchProcessBtn.TabIndex = 7;
            searchProcessBtn.Text = "Search";
            searchProcessBtn.UseVisualStyleBackColor = true;
            searchProcessBtn.Click += searchProcessBtn_Click;
            searchProcessBtn.MouseHover += searchProcessBtn_MouseHover;
            // 
            // searchProcessTxt
            // 
            searchProcessTxt.Anchor = AnchorStyles.Top;
            searchProcessTxt.Location = new Point(437, 13);
            searchProcessTxt.Name = "searchProcessTxt";
            searchProcessTxt.Size = new Size(233, 23);
            searchProcessTxt.TabIndex = 6;
            searchProcessTxt.TextAlign = HorizontalAlignment.Center;
            searchProcessTxt.TextChanged += searchProcessTxt_TextChanged;
            searchProcessTxt.KeyDown += searchProcessTxt_KeyDown;
            searchProcessTxt.MouseHover += searchProcessTxt_MouseHover;
            // 
            // refreshProcessListBtn
            // 
            refreshProcessListBtn.Anchor = AnchorStyles.Top;
            refreshProcessListBtn.Location = new Point(752, 12);
            refreshProcessListBtn.Name = "refreshProcessListBtn";
            refreshProcessListBtn.Size = new Size(102, 23);
            refreshProcessListBtn.TabIndex = 5;
            refreshProcessListBtn.Text = "Refresh List (R)";
            refreshProcessListBtn.UseVisualStyleBackColor = true;
            refreshProcessListBtn.Click += refreshProcessListBtn_Click;
            refreshProcessListBtn.MouseHover += refreshProcessListBtn_MouseHover;
            // 
            // actionMenuStrip
            // 
            actionMenuStrip.Items.AddRange(new ToolStripItem[] { boostToolStripMenuItem, iOPriorityToolStripMenuItem, priorityToolStripMenuItem, windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem, afinityToolStripMenuItem, efficiencyModeToolStripMenuItem, toolStripSeparator1, showSavedSettingsToolStripMenuItem, toolStripSeparator2, killProcessToolStripMenuItem, deleteSavedSettingsToolStripMenuItem });
            actionMenuStrip.Name = "contextMenuStrip1";
            actionMenuStrip.Size = new Size(334, 214);
            // 
            // boostToolStripMenuItem
            // 
            boostToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem, disableToolStripMenuItem });
            boostToolStripMenuItem.Name = "boostToolStripMenuItem";
            boostToolStripMenuItem.Size = new Size(333, 22);
            boostToolStripMenuItem.Text = "Boost";
            // 
            // enableToolStripMenuItem
            // 
            enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            enableToolStripMenuItem.Size = new Size(112, 22);
            enableToolStripMenuItem.Text = "Enable";
            enableToolStripMenuItem.Click += enableToolStripMenuItem_Click;
            // 
            // disableToolStripMenuItem
            // 
            disableToolStripMenuItem.Name = "disableToolStripMenuItem";
            disableToolStripMenuItem.Size = new Size(112, 22);
            disableToolStripMenuItem.Text = "Disable";
            disableToolStripMenuItem.Click += disableToolStripMenuItem_Click;
            // 
            // iOPriorityToolStripMenuItem
            // 
            iOPriorityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { veryLowToolStripMenuItem, lowToolStripMenuItem, normalToolStripMenuItem, highToolStripMenuItem });
            iOPriorityToolStripMenuItem.Name = "iOPriorityToolStripMenuItem";
            iOPriorityToolStripMenuItem.Size = new Size(333, 22);
            iOPriorityToolStripMenuItem.Text = "I/O Priority";
            // 
            // veryLowToolStripMenuItem
            // 
            veryLowToolStripMenuItem.Name = "veryLowToolStripMenuItem";
            veryLowToolStripMenuItem.Size = new Size(118, 22);
            veryLowToolStripMenuItem.Text = "VeryLow";
            veryLowToolStripMenuItem.Click += veryLowToolStripMenuItem_Click;
            // 
            // lowToolStripMenuItem
            // 
            lowToolStripMenuItem.Name = "lowToolStripMenuItem";
            lowToolStripMenuItem.Size = new Size(118, 22);
            lowToolStripMenuItem.Text = "Low";
            lowToolStripMenuItem.Click += lowToolStripMenuItem_Click;
            // 
            // normalToolStripMenuItem
            // 
            normalToolStripMenuItem.Name = "normalToolStripMenuItem";
            normalToolStripMenuItem.Size = new Size(118, 22);
            normalToolStripMenuItem.Text = "Normal";
            normalToolStripMenuItem.Click += normalToolStripMenuItem_Click;
            // 
            // highToolStripMenuItem
            // 
            highToolStripMenuItem.Name = "highToolStripMenuItem";
            highToolStripMenuItem.Size = new Size(118, 22);
            highToolStripMenuItem.Text = "High";
            highToolStripMenuItem.Click += highToolStripMenuItem_Click;
            // 
            // priorityToolStripMenuItem
            // 
            priorityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { realTimedangerToolStripMenuItem, highToolStripMenuItem1, aboveNormalToolStripMenuItem, normalToolStripMenuItem1, belowNormalToolStripMenuItem });
            priorityToolStripMenuItem.Name = "priorityToolStripMenuItem";
            priorityToolStripMenuItem.Size = new Size(333, 22);
            priorityToolStripMenuItem.Text = "CPU Priority";
            // 
            // realTimedangerToolStripMenuItem
            // 
            realTimedangerToolStripMenuItem.Name = "realTimedangerToolStripMenuItem";
            realTimedangerToolStripMenuItem.Size = new Size(168, 22);
            realTimedangerToolStripMenuItem.Text = "RealTime(danger)";
            realTimedangerToolStripMenuItem.Click += realTimedangerToolStripMenuItem_Click;
            // 
            // highToolStripMenuItem1
            // 
            highToolStripMenuItem1.Name = "highToolStripMenuItem1";
            highToolStripMenuItem1.Size = new Size(168, 22);
            highToolStripMenuItem1.Text = "High";
            highToolStripMenuItem1.Click += highToolStripMenuItem1_Click;
            // 
            // aboveNormalToolStripMenuItem
            // 
            aboveNormalToolStripMenuItem.Name = "aboveNormalToolStripMenuItem";
            aboveNormalToolStripMenuItem.Size = new Size(168, 22);
            aboveNormalToolStripMenuItem.Text = "Above Normal";
            aboveNormalToolStripMenuItem.Click += aboveNormalToolStripMenuItem_Click;
            // 
            // normalToolStripMenuItem1
            // 
            normalToolStripMenuItem1.Name = "normalToolStripMenuItem1";
            normalToolStripMenuItem1.Size = new Size(168, 22);
            normalToolStripMenuItem1.Text = "Normal";
            normalToolStripMenuItem1.Click += normalToolStripMenuItem1_Click;
            // 
            // belowNormalToolStripMenuItem
            // 
            belowNormalToolStripMenuItem.Name = "belowNormalToolStripMenuItem";
            belowNormalToolStripMenuItem.Size = new Size(168, 22);
            belowNormalToolStripMenuItem.Text = "Below Normal";
            belowNormalToolStripMenuItem.Click += belowNormalToolStripMenuItem_Click;
            // 
            // windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem
            // 
            windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem2, disableToolStripMenuItem2 });
            windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem.Name = "windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem";
            windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem.Size = new Size(333, 22);
            windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem.Text = "Windows dynamic thread priority boost (WDTPB)";
            // 
            // enableToolStripMenuItem2
            // 
            enableToolStripMenuItem2.Name = "enableToolStripMenuItem2";
            enableToolStripMenuItem2.Size = new Size(180, 22);
            enableToolStripMenuItem2.Text = "Enable";
            enableToolStripMenuItem2.Click += enableToolStripMenuItem2_Click;
            // 
            // disableToolStripMenuItem2
            // 
            disableToolStripMenuItem2.Name = "disableToolStripMenuItem2";
            disableToolStripMenuItem2.Size = new Size(180, 22);
            disableToolStripMenuItem2.Text = "Disable";
            disableToolStripMenuItem2.Click += disableToolStripMenuItem2_Click;
            // 
            // afinityToolStripMenuItem
            // 
            afinityToolStripMenuItem.Name = "afinityToolStripMenuItem";
            afinityToolStripMenuItem.Size = new Size(333, 22);
            afinityToolStripMenuItem.Text = "CPU Afinity";
            afinityToolStripMenuItem.MouseHover += afinityToolStripMenuItem_MouseHover;
            // 
            // efficiencyModeToolStripMenuItem
            // 
            efficiencyModeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem1, disableToolStripMenuItem1 });
            efficiencyModeToolStripMenuItem.Name = "efficiencyModeToolStripMenuItem";
            efficiencyModeToolStripMenuItem.Size = new Size(333, 22);
            efficiencyModeToolStripMenuItem.Text = "Efficiency Mode";
            // 
            // enableToolStripMenuItem1
            // 
            enableToolStripMenuItem1.Name = "enableToolStripMenuItem1";
            enableToolStripMenuItem1.Size = new Size(112, 22);
            enableToolStripMenuItem1.Text = "Enable";
            enableToolStripMenuItem1.Click += enableToolStripMenuItem1_Click;
            // 
            // disableToolStripMenuItem1
            // 
            disableToolStripMenuItem1.Name = "disableToolStripMenuItem1";
            disableToolStripMenuItem1.Size = new Size(112, 22);
            disableToolStripMenuItem1.Text = "Disable";
            disableToolStripMenuItem1.Click += disableToolStripMenuItem1_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(330, 6);
            // 
            // showSavedSettingsToolStripMenuItem
            // 
            showSavedSettingsToolStripMenuItem.Name = "showSavedSettingsToolStripMenuItem";
            showSavedSettingsToolStripMenuItem.Size = new Size(333, 22);
            showSavedSettingsToolStripMenuItem.Text = "Show Saved Settings";
            showSavedSettingsToolStripMenuItem.Click += showSavedSettingsToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(330, 6);
            // 
            // killProcessToolStripMenuItem
            // 
            killProcessToolStripMenuItem.Name = "killProcessToolStripMenuItem";
            killProcessToolStripMenuItem.Size = new Size(333, 22);
            killProcessToolStripMenuItem.Text = "Kill Process";
            killProcessToolStripMenuItem.Click += killProcessToolStripMenuItem_Click;
            // 
            // deleteSavedSettingsToolStripMenuItem
            // 
            deleteSavedSettingsToolStripMenuItem.ForeColor = Color.Red;
            deleteSavedSettingsToolStripMenuItem.Name = "deleteSavedSettingsToolStripMenuItem";
            deleteSavedSettingsToolStripMenuItem.Size = new Size(333, 22);
            deleteSavedSettingsToolStripMenuItem.Text = "Delete Saved Settings";
            deleteSavedSettingsToolStripMenuItem.Click += deleteSavedSettingsToolStripMenuItem_Click;
            // 
            // reloadProcess
            // 
            reloadProcess.Enabled = true;
            reloadProcess.Interval = 1100;
            reloadProcess.Tick += reloadProcess_Tick;
            // 
            // countProcessesLbl
            // 
            countProcessesLbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            countProcessesLbl.Location = new Point(9, 607);
            countProcessesLbl.Name = "countProcessesLbl";
            countProcessesLbl.Size = new Size(194, 15);
            countProcessesLbl.TabIndex = 8;
            countProcessesLbl.Text = "Processes running: 0";
            // 
            // checkBox1
            // 
            checkBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(1115, 607);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(187, 19);
            checkBox1.TabIndex = 9;
            checkBox1.Text = "Load saved settings on Startup";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // SaveSettingsCkb
            // 
            SaveSettingsCkb.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SaveSettingsCkb.AutoSize = true;
            SaveSettingsCkb.Location = new Point(998, 607);
            SaveSettingsCkb.Name = "SaveSettingsCkb";
            SaveSettingsCkb.Size = new Size(97, 19);
            SaveSettingsCkb.TabIndex = 10;
            SaveSettingsCkb.Text = "Save settings ";
            SaveSettingsCkb.UseVisualStyleBackColor = true;
            SaveSettingsCkb.CheckedChanged += SaveSettingsCkb_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1317, 631);
            Controls.Add(SaveSettingsCkb);
            Controls.Add(checkBox1);
            Controls.Add(countProcessesLbl);
            Controls.Add(searchProcessBtn);
            Controls.Add(searchProcessTxt);
            Controls.Add(refreshProcessListBtn);
            Controls.Add(systemMonitorPanel);
            Controls.Add(processesListBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Process Limiter";
            Load += MainForm_Load;
            MouseHover += MainForm_MouseHover;
            Resize += MainForm_Resize;
            actionMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DoubleBufferedListView processesListBox;
        private ColumnHeader processName;
        private ColumnHeader processId;
        private ColumnHeader priority;
        private ColumnHeader afinity;
        private ColumnHeader ioPriority;
        private ColumnHeader boost;
        private Button searchProcessBtn;
        private TextBox searchProcessTxt;
        private Button refreshProcessListBtn;
        private ContextMenuStrip actionMenuStrip;
        private ToolStripMenuItem boostToolStripMenuItem;
        private ToolStripMenuItem enableToolStripMenuItem;
        private ToolStripMenuItem disableToolStripMenuItem;
        private ToolStripMenuItem iOPriorityToolStripMenuItem;
        private ToolStripMenuItem veryLowToolStripMenuItem;
        private ToolStripMenuItem lowToolStripMenuItem;
        private ToolStripMenuItem normalToolStripMenuItem;
        private ToolStripMenuItem highToolStripMenuItem;
        private ToolStripMenuItem priorityToolStripMenuItem;
        private ToolStripMenuItem realTimedangerToolStripMenuItem;
        private ToolStripMenuItem highToolStripMenuItem1;
        private ToolStripMenuItem aboveNormalToolStripMenuItem;
        private ToolStripMenuItem normalToolStripMenuItem1;
        private ToolStripMenuItem belowNormalToolStripMenuItem;
        private ToolStripMenuItem afinityToolStripMenuItem;
        private ColumnHeader efficiencyMode;
        private ToolStripMenuItem efficiencyModeToolStripMenuItem;
        private ToolStripMenuItem enableToolStripMenuItem1;
        private ToolStripMenuItem disableToolStripMenuItem1;
        private System.Windows.Forms.Timer reloadProcess;
        private Label countProcessesLbl;
        private CheckBox checkBox1;
        private ColumnHeader storedSettings;
        private ToolStripMenuItem deleteSavedSettingsToolStripMenuItem;
        private ToolStripMenuItem showSavedSettingsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem killProcessToolStripMenuItem;
        private ColumnHeader userRunning;
        private CheckBox SaveSettingsCkb;
        private PLimit.Utils.SystemMonitorPanel systemMonitorPanel;
        private ColumnHeader wdtpb;
        private ToolStripMenuItem windowsDynamicThreadPriorityBoostWDTPBToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem enableToolStripMenuItem2;
        private ToolStripMenuItem disableToolStripMenuItem2;
    }
}
