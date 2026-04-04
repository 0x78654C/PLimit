using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLimit
{
    public class DoubleBufferedListView : ListView
    {
        // ── Dark-theme colours ────────────────────────────────────────────
        private static readonly Color HeaderBg  = Color.FromArgb(35, 35, 35);
        private static readonly Color HeaderFg  = Color.FromArgb(200, 200, 200);
        private static readonly Color HeaderBdr = Color.FromArgb(60, 60, 60);
        private static readonly Color RowAlt    = Color.FromArgb(44, 44, 44);
        private static readonly Color SelBg     = Color.FromArgb(38, 79, 148);
        private static readonly Color SelFg     = Color.White;

        public DoubleBufferedListView()
        {
            DoubleBuffered  = true;
            OwnerDraw       = true;
            FullRowSelect   = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            DrawColumnHeader += OnDrawHeader;
            DrawItem         += (_, e) => e.DrawDefault = false;
            DrawSubItem      += OnDrawSubItem;
        }

        private void OnDrawHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            using var bgBrush = new SolidBrush(HeaderBg);
            using var bdrPen  = new Pen(HeaderBdr);

            e.Graphics.FillRectangle(bgBrush, e.Bounds);
            // bottom border
            e.Graphics.DrawLine(bdrPen,
                e.Bounds.Left, e.Bounds.Bottom - 1,
                e.Bounds.Right, e.Bounds.Bottom - 1);
            // right column separator
            e.Graphics.DrawLine(bdrPen,
                e.Bounds.Right - 1, e.Bounds.Top + 3,
                e.Bounds.Right - 1, e.Bounds.Bottom - 3);

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= e.Header?.TextAlign switch
            {
                HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
                HorizontalAlignment.Right  => TextFormatFlags.Right,
                _                          => TextFormatFlags.Left
            };

            var textBounds = Rectangle.Inflate(e.Bounds, -5, 0);
            TextRenderer.DrawText(e.Graphics, e.Header?.Text, e.Font ?? Font, textBounds, HeaderFg, flags);
        }

        private void OnDrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = (e.ItemState & ListViewItemStates.Selected) != 0;

            Color bg = selected ? SelBg :
                       e.ItemIndex % 2 == 1 ? RowAlt : BackColor;
            Color fg = selected ? SelFg : ForeColor;

            using var bgBrush = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            HorizontalAlignment align = e.ColumnIndex < Columns.Count
                ? Columns[e.ColumnIndex].TextAlign
                : HorizontalAlignment.Left;

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= align switch
            {
                HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
                HorizontalAlignment.Right  => TextFormatFlags.Right,
                _                          => TextFormatFlags.Left
            };

            var textBounds = Rectangle.Inflate(e.Bounds, -3, 0);
            TextRenderer.DrawText(e.Graphics, e.SubItem?.Text, e.SubItem?.Font ?? Font, textBounds, fg, flags);
        }
    }
}

