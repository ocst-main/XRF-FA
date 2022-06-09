using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace XRF_FA
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MssqlConnect.Instance.CheckDB();

            bool flag = false;
            int ProgId = Process.GetCurrentProcess().Id;
            Process[] p = Process.GetProcessesByName("XRF_FA");

            if (p.Length > 1)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i].Id != ProgId)
                    {
                        continue;
                    }
                    else
                    {
                        flag = true;
                    }
                }

            }

            if (flag)
            {
                MessageBox.Show("프로그램이 이미 실행되고 있습니다.", "경고");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
