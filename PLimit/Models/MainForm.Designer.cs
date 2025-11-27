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
            processesListBox = new ListView();
            processName = new ColumnHeader();
            processId = new ColumnHeader();
            priority = new ColumnHeader();
            afinity = new ColumnHeader();
            ioPriority = new ColumnHeader();
            boost = new ColumnHeader();
            searchProcessBtn = new Button();
            searchProcessTxt = new TextBox();
            refreshProcessListBtn = new Button();
            SuspendLayout();
            // 
            // processesListBox
            // 
            processesListBox.Columns.AddRange(new ColumnHeader[] { processName, processId, priority, afinity, ioPriority, boost });
            processesListBox.HideSelection = true;
            processesListBox.Location = new Point(12, 41);
            processesListBox.MultiSelect = false;
            processesListBox.Name = "processesListBox";
            processesListBox.Size = new Size(828, 388);
            processesListBox.Sorting = SortOrder.Ascending;
            processesListBox.TabIndex = 1;
            processesListBox.UseCompatibleStateImageBehavior = false;
            processesListBox.View = View.Details;
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
            ioPriority.Text = "I/O priority";
            ioPriority.Width = 120;
            // 
            // boost
            // 
            boost.Text = "Boost";
            boost.Width = 90;
            // 
            // searchProcessBtn
            // 
            searchProcessBtn.Enabled = false;
            searchProcessBtn.Location = new Point(458, 11);
            searchProcessBtn.Name = "searchProcessBtn";
            searchProcessBtn.Size = new Size(75, 23);
            searchProcessBtn.TabIndex = 7;
            searchProcessBtn.Text = "Search";
            searchProcessBtn.UseVisualStyleBackColor = true;
            searchProcessBtn.Click += searchProcessBtn_Click;
            // 
            // searchProcessTxt
            // 
            searchProcessTxt.Location = new Point(219, 12);
            searchProcessTxt.Name = "searchProcessTxt";
            searchProcessTxt.Size = new Size(233, 23);
            searchProcessTxt.TabIndex = 6;
            searchProcessTxt.TextAlign = HorizontalAlignment.Center;
            searchProcessTxt.TextChanged += searchProcessTxt_TextChanged;
            // 
            // refreshProcessListBtn
            // 
            refreshProcessListBtn.Location = new Point(534, 11);
            refreshProcessListBtn.Name = "refreshProcessListBtn";
            refreshProcessListBtn.Size = new Size(75, 23);
            refreshProcessListBtn.TabIndex = 5;
            refreshProcessListBtn.Text = "Refresh List";
            refreshProcessListBtn.UseVisualStyleBackColor = true;
            refreshProcessListBtn.Click += refreshProcessListBtn_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(853, 441);
            Controls.Add(searchProcessBtn);
            Controls.Add(searchProcessTxt);
            Controls.Add(refreshProcessListBtn);
            Controls.Add(processesListBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Process Limiter";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView processesListBox;
        private ColumnHeader processName;
        private ColumnHeader processId;
        private ColumnHeader priority;
        private ColumnHeader afinity;
        private ColumnHeader ioPriority;
        private ColumnHeader boost;
        private Button searchProcessBtn;
        private TextBox searchProcessTxt;
        private Button refreshProcessListBtn;
    }
}
