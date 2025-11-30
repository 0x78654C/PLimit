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
            processesListBox = new DoubleBufferedListView();
            processName = new ColumnHeader();
            processId = new ColumnHeader();
            priority = new ColumnHeader();
            afinity = new ColumnHeader();
            ioPriority = new ColumnHeader();
            boost = new ColumnHeader();
            efficiencyMode = new ColumnHeader();
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
            afinityToolStripMenuItem = new ToolStripMenuItem();
            efficiencyModeToolStripMenuItem = new ToolStripMenuItem();
            enableToolStripMenuItem1 = new ToolStripMenuItem();
            disableToolStripMenuItem1 = new ToolStripMenuItem();
            reloadProcess = new System.Windows.Forms.Timer(components);
            actionMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // processesListBox
            // 
            processesListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            processesListBox.Columns.AddRange(new ColumnHeader[] { processName, processId, priority, afinity, ioPriority, boost, efficiencyMode });
            processesListBox.HideSelection = true;
            processesListBox.Location = new Point(12, 41);
            processesListBox.MultiSelect = false;
            processesListBox.Name = "processesListBox";
            processesListBox.Size = new Size(951, 388);
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
            // searchProcessBtn
            // 
            searchProcessBtn.Anchor = AnchorStyles.Top;
            searchProcessBtn.Enabled = false;
            searchProcessBtn.Location = new Point(507, 12);
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
            searchProcessTxt.Location = new Point(268, 13);
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
            refreshProcessListBtn.Location = new Point(583, 12);
            refreshProcessListBtn.Name = "refreshProcessListBtn";
            refreshProcessListBtn.Size = new Size(94, 23);
            refreshProcessListBtn.TabIndex = 5;
            refreshProcessListBtn.Text = "Refresh List (R)";
            refreshProcessListBtn.UseVisualStyleBackColor = true;
            refreshProcessListBtn.Click += refreshProcessListBtn_Click;
            refreshProcessListBtn.MouseHover += refreshProcessListBtn_MouseHover;
            // 
            // actionMenuStrip
            // 
            actionMenuStrip.Items.AddRange(new ToolStripItem[] { boostToolStripMenuItem, iOPriorityToolStripMenuItem, priorityToolStripMenuItem, afinityToolStripMenuItem, efficiencyModeToolStripMenuItem });
            actionMenuStrip.Name = "contextMenuStrip1";
            actionMenuStrip.Size = new Size(160, 114);
            // 
            // boostToolStripMenuItem
            // 
            boostToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem, disableToolStripMenuItem });
            boostToolStripMenuItem.Name = "boostToolStripMenuItem";
            boostToolStripMenuItem.Size = new Size(159, 22);
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
            iOPriorityToolStripMenuItem.Size = new Size(159, 22);
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
            priorityToolStripMenuItem.Size = new Size(159, 22);
            priorityToolStripMenuItem.Text = "Priority";
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
            // afinityToolStripMenuItem
            // 
            afinityToolStripMenuItem.Name = "afinityToolStripMenuItem";
            afinityToolStripMenuItem.Size = new Size(159, 22);
            afinityToolStripMenuItem.Text = "Afinity";
            afinityToolStripMenuItem.MouseHover += afinityToolStripMenuItem_MouseHover;
            // 
            // efficiencyModeToolStripMenuItem
            // 
            efficiencyModeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem1, disableToolStripMenuItem1 });
            efficiencyModeToolStripMenuItem.Name = "efficiencyModeToolStripMenuItem";
            efficiencyModeToolStripMenuItem.Size = new Size(159, 22);
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
            // reloadProcess
            // 
            reloadProcess.Enabled = true;
            reloadProcess.Interval = 1050;
            reloadProcess.Tick += reloadProcess_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(978, 441);
            Controls.Add(searchProcessBtn);
            Controls.Add(searchProcessTxt);
            Controls.Add(refreshProcessListBtn);
            Controls.Add(processesListBox);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Process Limiter";
            Load += MainForm_Load;
            MouseHover += MainForm_MouseHover;
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
    }
}
