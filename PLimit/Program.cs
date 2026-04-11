namespace PLimit
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "plimit@xcoding");
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
            else
            {
                MessageBox.Show("Another instance of PLimit is already running.", "PLimit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}