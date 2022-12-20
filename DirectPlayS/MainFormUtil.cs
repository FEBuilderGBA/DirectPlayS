using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace DirectPlayS
{
    //メインフォームが GBA 3部作でわかれてしまうのでコピペするより共通部分を切り出す
    public class MainFormUtil
    {
        public static string GetExplainOfSFileURL()
        {
            string url = "https://dw.ngmansion.xyz/doku.php?id=en:guide:explanation_of_s_file_en";
            return url;
        }
        public static Process RunAs(string run_name, string arg2 = "")
        {
            if (InputFormRef.IsPleaseWaitDialog(null))
            {//2重割り込み禁止
                return null;
            }
            using (InputFormRef.AutoPleaseWait wait = new InputFormRef.AutoPleaseWait())
            {
                wait.DoEvents(R._("Running:{0}", run_name));

                string tempfilename;
                try
                {
                    tempfilename = U.WriteTempROM("sappy");
                }
                catch (Exception e)
                {
                    R.ShowStopError("Can not write File\r\n{0}", e.ToString());
                    return null;
                }

                string args;
                if (arg2 == "")
                {
                    args = U.escape_shell_args(tempfilename);
                }
                else
                {
                    args = arg2 + " " + U.escape_shell_args(tempfilename);
                }

                Process p;
                try
                {
                    p = ProgramRunAs(run_name, args);
                }
                catch (Exception e)
                {
                    R.ShowStopError("can not execute process\r\n{0} {1}\r\n{2}", run_name, args, e.ToString());
                    return null;
                }
                return p;
            }
        }
        public static Process ProgramRunAs(string appPath, string args, int waitMainwindowMiriSec = 60000)
        {
            //see
            //http://www.slotware.net/blog/2009/11/processstart.html

            Process p = new Process();
            p.StartInfo.FileName = appPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            p.WaitForInputIdle(waitMainwindowMiriSec);

            if (waitMainwindowMiriSec <= 0)
            {
                return p;
            }
            //ウィンドウハンドルが取得できるか、
            //生成したプロセスが終了するまで待ってみる。
            int waitLoopMiriSec = 0;
            while (true)
            {
                if (p.HasExited == true)
                {//プロセスは既に終了した
                    break;
                }
                if (p.MainWindowHandle != IntPtr.Zero)
                {//メインハンドルが作られた
                    break;
                }

                System.Threading.Thread.Sleep(1);
                waitLoopMiriSec += 1;
                if (waitLoopMiriSec > waitMainwindowMiriSec)
                {//タイムオーバー
                    break;
                }
            }
            return p;
        }
    }
}
