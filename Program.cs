using System;
using System.Windows.Forms;

namespace csbattleship
{
    static class Program
    {
        public static Form1 f; // TODO: fix it ! 
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
