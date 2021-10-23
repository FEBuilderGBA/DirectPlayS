using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DirectPlayS
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //オプション引数 --mode=foo とかを、dic["--mode"]="foo" みたいに変換します. 
            ArgsDic = U.OptionMap(args, "--filename");

            //自プロセスのパスから、ベースディレクトリを特定します.
            Program.BaseDirectory = MakeBaseDirectory();
            Program.Config = new Config();
            Program.Config.Load();

            Application.Run(new Form1());
        }

        static string MakeBaseDirectory()
        {
            string[] args = Environment.GetCommandLineArgs();

            //コマンドライン引数の最初の引数にはプロセスへのパスが入っています.
            string selfPath = args[0];
            //コマンドライン引数で渡されると、相対パスになるので、一度フルパスに変換します.
            selfPath = Path.GetFullPath(selfPath);
            //ディレクトリ名の取得
            string currentDir = System.IO.Path.GetDirectoryName(selfPath);
            //現在のプロセスがあるディレクトリがベースディレクトリです.
            return Path.GetFullPath(currentDir);
        }
        public static string BaseDirectory;

        public static Dictionary<string, string> ArgsDic { get; private set; }
        public static ROM ROM;
        public static Config Config;
    }
}
