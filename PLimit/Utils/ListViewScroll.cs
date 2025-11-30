using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    internal static class ListViewScroll
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETTOPINDEX = LVM_FIRST + 39;
        private const int LVM_ENSUREVISIBLE = LVM_FIRST + 19;
        private const int LVM_GETITEMRECT = LVM_FIRST + 14;
        private const int LVM_SCROLL = LVM_FIRST + 20;
        private const int LVIR_BOUNDS = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref RECT lParam);

        /// <summary>
        /// Saves the current top index of the ListView.
        /// </summary>
        /// <param name="lv"></param>
        /// <returns></returns>
        public static int SaveTopIndex(ListView lv)
        {
            if (!lv.IsHandleCreated) lv.CreateControl();
            return SendMessage(lv.Handle, LVM_GETTOPINDEX, 0, 0);
        }

        /// <summary>
        /// Restores the ListView to the specified top index.
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="topIndex"></param>
        public static void RestoreTopIndex(ListView lv, int topIndex)
        {
            if (lv.Items.Count == 0) return;
            topIndex = Math.Clamp(topIndex, 0, lv.Items.Count - 1);

            SendMessage(lv.Handle, LVM_ENSUREVISIBLE, topIndex, 0);

            var rc = new RECT { Left = LVIR_BOUNDS };
            SendMessage(lv.Handle, LVM_GETITEMRECT, topIndex, ref rc);

            if (rc.Top != 0)
                SendMessage(lv.Handle, LVM_SCROLL, 0, -rc.Top); // move it to the top
        }
    }
}
