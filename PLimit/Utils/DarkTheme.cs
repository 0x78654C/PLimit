using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    internal static class DarkTheme
    {
        // ── Palette (matches SystemMonitorPanel) ──────────────────────────
        public static readonly Color Background  = Color.FromArgb(28, 28, 28);
        public static readonly Color Surface     = Color.FromArgb(38, 38, 38);
        public static readonly Color ControlBg   = Color.FromArgb(52, 52, 52);
        public static readonly Color Border      = Color.FromArgb(68, 68, 68);
        public static readonly Color TextPrimary = Color.FromArgb(220, 220, 220);

        // ── Dark title bar (Windows 10 1903+ / Windows 11) ────────────────
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void EnableDarkTitleBar(IntPtr handle)
        {
            int value = 1;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        // ── Entry point ───────────────────────────────────────────────────

        public static void Apply(Form form, params ContextMenuStrip[] menus)
        {
            EnableDarkTitleBar(form.Handle);
            form.BackColor = Background;
            form.ForeColor = TextPrimary;
            ApplyToControls(form.Controls);
            foreach (var menu in menus)
                ApplyToMenu(menu);
        }

        // ── Controls ──────────────────────────────────────────────────────

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                ApplyToControl(c);
                if (c.HasChildren)
                    ApplyToControls(c.Controls);
            }
        }

        private static void ApplyToControl(Control c)
        {
            switch (c)
            {
                case SystemMonitorPanel:
                    break; // already dark — leave untouched

                case ListView lv:
                    lv.BackColor = Surface;
                    lv.ForeColor = TextPrimary;
                    break;

                case Button btn:
                    btn.BackColor = ControlBg;
                    btn.ForeColor = TextPrimary;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = Border;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(68, 68, 68);
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
                    break;

                case TextBox tb:
                    tb.BackColor = ControlBg;
                    tb.ForeColor = TextPrimary;
                    break;

                case Label lbl:
                    lbl.ForeColor = TextPrimary;
                    lbl.BackColor = Color.Transparent;
                    break;

                case CheckBox cb:
                    cb.ForeColor = TextPrimary;
                    cb.BackColor = Color.Transparent;
                    break;
            }
        }

        // ── Context menus ─────────────────────────────────────────────────

        public static void ApplyToMenu(ContextMenuStrip strip)
        {
            strip.Renderer  = new DarkMenuRenderer();
            strip.BackColor = Surface;
            strip.ForeColor = TextPrimary;
            ApplyToItems(strip.Items);
        }

        private static void ApplyToItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.BackColor = Surface;
                // Preserve explicitly-set colours (e.g. the red "Delete" entry)
                if (item.ForeColor.IsSystemColor)
                    item.ForeColor = TextPrimary;

                if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
                    ApplyToItems(mi.DropDownItems);
            }
        }

        // ── ToolStrip renderer ────────────────────────────────────────────

        private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            private static readonly Color MenuBg    = Color.FromArgb(38, 38, 38);
            private static readonly Color SelBg     = Color.FromArgb(50, 100, 175);
            private static readonly Color MarginBg  = Color.FromArgb(46, 46, 46);
            private static readonly Color Sep       = Color.FromArgb(65, 65, 65);

            public DarkMenuRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using var b = new SolidBrush(MenuBg);
                e.Graphics.FillRectangle(b, e.AffectedBounds);
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var color = e.Item.Selected ? SelBg : MenuBg;
                using var b = new SolidBrush(color);
                e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
            }

            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
                using var b = new SolidBrush(MarginBg);
                e.Graphics.FillRectangle(b, e.AffectedBounds);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                int y = e.Item.Height / 2;
                using var p = new Pen(Sep);
                e.Graphics.DrawLine(p, 30, y, e.Item.Width - 4, y);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.ForeColor;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = Color.FromArgb(200, 200, 200);
                base.OnRenderArrow(e);
            }
        }

        private sealed class DarkColorTable : ProfessionalColorTable
        {
            private static readonly Color Bg  = Color.FromArgb(38,  38,  38);
            private static readonly Color Sel = Color.FromArgb(50,  100, 175);
            private static readonly Color Bdr = Color.FromArgb(65,  65,  65);
            private static readonly Color Mrg = Color.FromArgb(46,  46,  46);

            public override Color MenuBorder                       => Bdr;
            public override Color MenuItemBorder                   => Color.Transparent;
            public override Color MenuItemSelected                 => Sel;
            public override Color MenuItemSelectedGradientBegin    => Sel;
            public override Color MenuItemSelectedGradientEnd      => Sel;
            public override Color ToolStripDropDownBackground      => Bg;
            public override Color ImageMarginGradientBegin         => Mrg;
            public override Color ImageMarginGradientMiddle        => Mrg;
            public override Color ImageMarginGradientEnd           => Mrg;
            public override Color SeparatorDark                    => Bdr;
            public override Color SeparatorLight                   => Color.FromArgb(75, 75, 75);
        }
    }
}
