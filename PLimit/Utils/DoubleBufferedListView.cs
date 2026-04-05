using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLimit
{
    public class DoubleBufferedListView : ListView
    {
        // ── Dark-theme colours ────────────────────────────────────────────
        private static readonly Color HeaderBg      = Color.FromArgb(35, 35, 35);
        private static readonly Color HeaderFg      = Color.FromArgb(200, 200, 200);
        private static readonly Color HeaderBdr     = Color.FromArgb(60, 60, 60);
        private static readonly Color HeaderSortBg  = Color.FromArgb(50, 50, 65);
        private static readonly Color RowAlt        = Color.FromArgb(44, 44, 44);
        private static readonly Color SelBg         = Color.FromArgb(38, 79, 148);
        private static readonly Color SelFg         = Color.White;

        // Columns whose values should be compared numerically (0-based index).
        // Column 1 = PID, Column 3 = CPU affinity count.
        private static readonly HashSet<int> NumericColumns = new() { 1, 3 };

        private int  _sortColumn    = -1;
        private bool _sortAscending = true;

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
            ColumnClick      += OnColumnClick;
        }

        // ── Sorting ───────────────────────────────────────────────────────

        private void OnColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn    = e.Column;
                _sortAscending = true;
            }

            ListViewItemSorter = new ColumnComparer(_sortColumn, _sortAscending, NumericColumns);
            Sort();
            Invalidate(); // repaint headers to update arrow
        }



        // ── Header drawing ────────────────────────────────────────────────

        private void OnDrawHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            bool isSort = e.ColumnIndex == _sortColumn;

            using var bgBrush = new SolidBrush(isSort ? HeaderSortBg : HeaderBg);
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

            // Reserve room on the right for the sort arrow when this is the active column.
            int arrowReserve = isSort ? 14 : 0;
            var textBounds   = Rectangle.Inflate(e.Bounds, -5, 0);
            textBounds.Width -= arrowReserve;

            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= e.Header?.TextAlign switch
            {
                HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
                HorizontalAlignment.Right  => TextFormatFlags.Right,
                _                          => TextFormatFlags.Left
            };

            TextRenderer.DrawText(e.Graphics, e.Header?.Text, e.Font ?? Font, textBounds, HeaderFg, flags);

            if (isSort)
                DrawSortArrow(e.Graphics, e.Bounds, _sortAscending);
        }

        private static void DrawSortArrow(Graphics g, Rectangle headerBounds, bool ascending)
        {
            const int arrowW = 8;
            const int arrowH = 5;

            int cx = headerBounds.Right - 9;
            int cy = headerBounds.Top + (headerBounds.Height - arrowH) / 2;

            using var pen = new Pen(Color.FromArgb(180, 180, 220), 1.5f);

            if (ascending)
            {
                // ▲  (tip up)
                Point[] pts =
                {
                    new(cx,              cy + arrowH),
                    new(cx + arrowW / 2, cy),
                    new(cx + arrowW,     cy + arrowH),
                };
                g.DrawLines(pen, pts);
            }
            else
            {
                // ▼  (tip down)
                Point[] pts =
                {
                    new(cx,              cy),
                    new(cx + arrowW / 2, cy + arrowH),
                    new(cx + arrowW,     cy),
                };
                g.DrawLines(pen, pts);
            }
        }

        // ── Row drawing ───────────────────────────────────────────────────

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

    internal sealed class ColumnComparer : IComparer
    {
        private readonly int          _col;
        private readonly bool         _asc;
        private readonly HashSet<int> _numericCols;

        public ColumnComparer(int col, bool asc, HashSet<int> numericCols)
        {
            _col         = col;
            _asc         = asc;
            _numericCols = numericCols;
        }

        public int Compare(object? x, object? y)
        {
            var ix = (ListViewItem)x!;
            var iy = (ListViewItem)y!;

            string tx = _col < ix.SubItems.Count ? ix.SubItems[_col].Text : string.Empty;
            string ty = _col < iy.SubItems.Count ? iy.SubItems[_col].Text : string.Empty;

            int result;
            if (_numericCols.Contains(_col)
                && long.TryParse(tx, out long nx)
                && long.TryParse(ty, out long ny))
                result = nx.CompareTo(ny);
            else
                result = string.Compare(tx, ty, StringComparison.OrdinalIgnoreCase);

            return _asc ? result : -result;
        }
    }
}

