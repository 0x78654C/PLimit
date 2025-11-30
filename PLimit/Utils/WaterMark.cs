using System.Runtime.InteropServices;

namespace PLimit.Utils
{
    internal static class WaterMark
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        /// <summary>
        /// Set textbox watermark tex.
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="waterMarkText"></param>
        public static void SetWatermark(this TextBox textBox, string waterMarkText) =>
            SendMessage(textBox.Handle, 0x1500 + 1, 0, waterMarkText);
    }
}
