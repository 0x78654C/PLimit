namespace PLimit
{
    internal static class Program
    {
        private const string MutexName = "plimit@xcoding";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(false, MutexName);
            bool ownsMutex;
            try
            {
                ownsMutex = mutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException)
            {
                ownsMutex = true;
            }

            if (!ownsMutex)
            {
                MessageBox.Show("Another instance of PLimit is already running.", "PLimit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
