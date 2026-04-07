namespace PLimit
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            logoPictureBox = new PictureBox();
            titleLabel = new Label();
            versionLabel = new Label();
            descriptionLabel = new Label();
            githubLinkLabel = new LinkLabel();
            closeButton = new Button();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            SuspendLayout();
            // 
            // logoPictureBox
            // 
            logoPictureBox.BackColor = Color.Transparent;
            logoPictureBox.Location = new Point(110, 18);
            logoPictureBox.Name = "logoPictureBox";
            logoPictureBox.Size = new Size(200, 200);
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.TabIndex = 0;
            logoPictureBox.TabStop = false;
            // 
            // titleLabel
            // 
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            titleLabel.Location = new Point(20, 226);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(380, 32);
            titleLabel.TabIndex = 1;
            titleLabel.Text = "PLimit — Process Limiter";
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // versionLabel
            // 
            versionLabel.Font = new Font("Segoe UI", 9F);
            versionLabel.Location = new Point(20, 261);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(380, 20);
            versionLabel.TabIndex = 2;
            versionLabel.Text = "Version 1.0.0";
            versionLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // descriptionLabel
            // 
            descriptionLabel.Font = new Font("Segoe UI", 9F);
            descriptionLabel.Location = new Point(20, 286);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new Size(380, 60);
            descriptionLabel.TabIndex = 3;
            descriptionLabel.Text = "A lightweight Windows utility to manage running process priorities,\nCPU affinity, I/O priority, CPU boost, thread priority boost,\nand efficiency mode — from a clean dark-themed UI.";
            descriptionLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // githubLinkLabel
            // 
            githubLinkLabel.Font = new Font("Segoe UI", 9F);
            githubLinkLabel.Location = new Point(20, 350);
            githubLinkLabel.Name = "githubLinkLabel";
            githubLinkLabel.Size = new Size(380, 20);
            githubLinkLabel.TabIndex = 1;
            githubLinkLabel.TabStop = true;
            githubLinkLabel.Text = "github.com/0x78654c/PLimit";
            githubLinkLabel.TextAlign = ContentAlignment.MiddleCenter;
            githubLinkLabel.LinkClicked += githubLinkLabel_LinkClicked;
            // 
            // closeButton
            // 
            closeButton.Location = new Point(165, 382);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(90, 28);
            closeButton.TabIndex = 0;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += closeButton_Click;
            // 
            // AboutForm
            // 
            AcceptButton = closeButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 424);
            Controls.Add(logoPictureBox);
            Controls.Add(titleLabel);
            Controls.Add(versionLabel);
            Controls.Add(descriptionLabel);
            Controls.Add(githubLinkLabel);
            Controls.Add(closeButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "About PLimit";
            Load += AboutForm_Load;
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            ResumeLayout(false);
        }

        private PictureBox logoPictureBox;
        private Label titleLabel;
        private Label versionLabel;
        private Label descriptionLabel;
        private LinkLabel githubLinkLabel;
        private Button closeButton;
    }
}
