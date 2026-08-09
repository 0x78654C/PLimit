using PLimit.Utils;
using System.Reflection;

namespace PLimit
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            DarkTheme.Apply(this);
            ApplyLinkLabelStyle();
            LoadLogo();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = version != null
                ? $"Version {version.Major}.{version.Minor}.{version.Build}"
                : "Version 1.0.0";
        }

        private void ApplyLinkLabelStyle()
        {
            githubLinkLabel.BackColor = Color.Transparent;
            githubLinkLabel.ForeColor = Color.FromArgb(100, 160, 240);
            githubLinkLabel.LinkColor = Color.FromArgb(100, 160, 240);
            githubLinkLabel.ActiveLinkColor = Color.FromArgb(140, 190, 255);
            githubLinkLabel.VisitedLinkColor = Color.FromArgb(100, 160, 240);
        }

        private void LoadLogo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("PLimit.logo1.png");
            if (stream == null) return;

            using var image = Image.FromStream(stream);
            logoPictureBox.Image?.Dispose();
            logoPictureBox.Image = new Bitmap(image);
        }

        private void closeButton_Click(object sender, EventArgs e) => Close();

        private void githubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/0x78654c/PLimit",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
