using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

namespace WFA
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {


             bool isAppRunning = false;
             Mutex mutex = new Mutex(true, System.Diagnostics.Process.GetCurrentProcess().ProcessName, out isAppRunning);
             if (!isAppRunning)
             {
                    MessageBox.Show("程序已运行!");
                    Environment.Exit(1);
             }
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                SysConfig.SysLoadConfig();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
            }

        }
    }
}
