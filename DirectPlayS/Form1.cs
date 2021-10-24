using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace DirectPlayS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            RomFilename.Text = Program.Config.at("RomFilename");
            TextEditor.Text = Program.Config.at("TextEditor");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (AutoPlay())
            {
                this.Close();
                return;
            }

            SetNowPlaying("");
            if (TextEditor.Text == "")
            {
                TextEditor.Text = U.FindAssociatedExecutable(".txt");
            }

            U.AddCancelButton(this);
            U.AllowDropFilename(this, new string[]{".S"}, (string filename) =>
            {
                Play(filename);
            });
            U.AllowDropFilename(this.BunnerLabel, new string[] { ".S" }, (string filename) =>
            {
                Play(filename);
            });
        }

        bool AutoPlay()
        {
            string filename = U.at(Program.ArgsDic, "--filename");
            if (filename == "")
            {
                return false;
            }
            return Play(filename);
        }

        void SetNowPlaying(string filename)
        {
            if (filename == "")
            {
                NowPlaying.Text = "";
            }
            else
            {
                NowPlaying.Text = "Playing: "+Path.GetFileName(filename);
            }
        }

        bool Play(String filename)
        {
            KillProcessIfRunning();

            if (!IsMusicFile(filename))
            {//音楽ファイルではないので、テキストエディタを起動する
                ExecuteTextEditor(filename);
                return true;
            }

            if (Program.ROM == null)
            {
                if (!LoadROM())
                {
                    return false;
                }
            }

            if (!ImportS(filename))
            {
                return false;
            }

            if (!ExecutePlayer())
            {
                return false;
            }

            SetNowPlaying(filename);
            return true;
        }

        bool IsMusicFile(string filename)
        {
            try
            {
                string txt = File.ReadAllText(filename);
                if (txt.IndexOf("MPlayDef.s") >= 0)
                {
                    return true;
                }
            }
            catch (Exception )
            {
            }
            return false;
        }

        bool LoadROM()
        {
            string romfilename = RomFilename.Text;
            if (romfilename == "")
            {
                R.ShowStopError("Please set \"Select Play ROM\".");
                return false;
            }

            try
            {
                byte[] bin = File.ReadAllBytes(romfilename);

                Program.ROM = new ROM();
                Program.ROM.LoadLow(romfilename, bin, "");
            }
            catch (Exception )
            {
                R.ShowStopError("can't open the ROM({0}).", romfilename);
                return false;
            }

            uint nimapAddr = SearchInstrumentSetLow();
            if (nimapAddr == U.NOT_FOUND)
            {
                R.ShowStopError("This ROM does not have NIMAP installed.\r\nSpecify the ROM where NIMAP or NIMAP2 is installed.");
                Program.ROM = null;
                return false;
            }

            uint songTablePointer = U.NOT_FOUND;
            if (Program.ROM.Version == "BE8J")
            {
                songTablePointer = 0xD5024;
            }
            else if (Program.ROM.Version == "BE8E")
            {
                songTablePointer = 0x28BC;
            }
            else if (Program.ROM.Version == "AE7J")
            {
                songTablePointer = 0x3E2C;
            }
            else if (Program.ROM.Version == "AE7E")
            {
                songTablePointer = 0x3F50;
            }
            else if (Program.ROM.Version == "AFEJ")
            {
                songTablePointer = 0x3748;
            }
            else
            {
                songTablePointer = SongUtil.FindSongTablePointer(Program.ROM.Data);
            }

            if (songTablePointer == U.NOT_FOUND)
            {
                R.ShowStopError("Could not find SongTable.\r\nIs this the correct GBA ROM?");
                Program.ROM = null;
                return false;
            }
            uint songTableAddr = Program.ROM.p32(U.toOffset(songTablePointer));
            if (! U.isSafetyOffset(songTableAddr))
            {
                R.ShowStopError("Could not find SongTable.\r\nIs this the correct GBA ROM?");
                Program.ROM = null;
                return false;
            }

            g_NIMAP_Addr = nimapAddr;
            g_SongTable_Addr = songTableAddr;
            return true;
        }

        uint g_NIMAP_Addr = U.NOT_FOUND;
        static uint SearchInstrumentSetLow()
        {
            uint r;
            byte[] need;

            if (Program.ROM.Version == "BE8J")
            {
                uint searchStartAddr = 0xDB000;
                //NatveInstrumentMap2	FE8J
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x68, 0x27, 0x50, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x18, 0x7D, 0x29, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x3C, 0x8E, 0x28, 0x08, 0xFF, 0xF9, 0x00, 0xA5, 0x01, 0x3C, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x3C, 0x00, 0x00, 0xA4, 0xB4, 0x2A, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x01, 0x3C, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x3C, 0x00, 0x00, 0xD4, 0xF1, 0x27, 0x08, 0xFF, 0xF9, 0x00, 0xA5, 0x00, 0x3C, 0x00, 0x00, 0xD4, 0xF1, 0x27, 0x08, 0xFF, 0xF5, 0x96, 0x96 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }

                //NatveInstrumentMap	FE8J
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x68, 0x27, 0x50, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x18, 0x7D, 0x29, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x3C, 0x8E, 0x28, 0x08 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }
                return SearchOverNIMAP("FE8J", searchStartAddr);
            }

            if (Program.ROM.Version == "BE8E")
            {
                uint searchStartAddr = 0xDB000;
                //NatveInstrumentMap2	FE8U
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0xB8, 0x2A, 0x51, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x68, 0x80, 0x2A, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x8C, 0x91, 0x29, 0x08, 0xFF, 0xF9, 0x00, 0xA5, 0x01, 0x3C, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x3C, 0x00, 0x00, 0xF4, 0xB7, 0x2B, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x01, 0x3C, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x24, 0xF5, 0x28, 0x08, 0xFF, 0xF9, 0x00, 0xA5, 0x00, 0x3C, 0x00, 0x00, 0x24, 0xF5, 0x28, 0x08, 0xFF, 0xF5, 0x96, 0x96 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }

                //NatveInstrumentMap	FE8U
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0xB8, 0x2A, 0x51, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x68, 0x80, 0x2A, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x8C, 0x91, 0x29, 0x08, 0xFF, 0xF9, 0x00, 0xA5, 0x01, 0x3C, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x3C, 0x00, 0x00, 0xF4, 0xB7, 0x2B, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x01, 0x3C, 0x00, 0x00 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }
                return SearchOverNIMAP("FE8U", searchStartAddr);
            }

            if (Program.ROM.Version == "AE7J")
            {
                uint searchStartAddr = 0xC7334;

                //NatveInstrumentMap	FE7J
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x4C, 0x78, 0x85, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0xD0, 0xF0, 0x84, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x98, 0xF9, 0x79, 0x08, 0xFF, 0x00, 0xFF, 0x95, 0x00, 0x3C, 0x00, 0x00, 0xC4, 0xC8, 0x7F, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x68, 0x24, 0x80, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }

                return SearchOverNIMAP("FE7J", searchStartAddr);
            }

            if (Program.ROM.Version == "AE7E")
            {
                uint searchStartAddr = 0xCBEE4;

                //NatveInstrumentMap	FE7U
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x5C, 0xA6, 0x80, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0xE0, 0x1E, 0x80, 0x08, 0xFF, 0xFA, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0xA8, 0x27, 0x75, 0x08, 0xFF, 0x00, 0xFF, 0x95, 0x00, 0x3C, 0x00, 0x00, 0xD4, 0xF6, 0x7A, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00, 0x78, 0x52, 0x7B, 0x08, 0xFF, 0xFD, 0x00, 0xCC, 0x00, 0x3C, 0x00, 0x00 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }

                return SearchOverNIMAP("FE7U", searchStartAddr);
            }

            if (Program.ROM.Version == "AFEJ")
            {
                uint searchStartAddr = 0xF9D80;

                //NatveInstrumentMap	FE6
                need = new byte[] { 0x00, 0x3C, 0x00, 0x00, 0x9C, 0x2E, 0x3F, 0x08, 0xFF, 0xFC, 0x00, 0xC8, 0x00, 0x3C, 0x00, 0x00, 0x9C, 0x2E, 0x3F, 0x08, 0xFA, 0xFA, 0x96, 0xC8, 0x00, 0x3C, 0x00, 0x00, 0x0C, 0x86, 0x43, 0x08, 0xFF, 0xF8, 0x96, 0xB4, 0x00, 0x3C, 0x00, 0x00, 0xF8, 0x6A, 0x43, 0x08, 0xFF, 0xDC, 0x96, 0x96, 0x00, 0x3C, 0x00, 0x00, 0x44, 0xE6, 0x3F, 0x08, 0xFF, 0xFC, 0x64, 0xB4, 0x00, 0x3C, 0x00, 0x00 };
                r = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (r != U.NOT_FOUND)
                {
                    return r;
                }

                return SearchOverNIMAP("FE6", searchStartAddr);
            }

            return U.NOT_FOUND;
        }

        static uint SearchOverNIMAP(string needVersion, uint searchStartAddr)
        {
            string song_instrumentset_ALL = Path.Combine(Program.BaseDirectory, "song_instrumentset_ALL.txt");
            if (!System.IO.File.Exists(song_instrumentset_ALL))
            {
                return U.NOT_FOUND;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(song_instrumentset_ALL);
            }
            catch (Exception)
            {
                return U.NOT_FOUND;
            }

            foreach (string line in lines)
            {
                string[] sp = line.Split(new char[] { '\t' });
                if (sp.Length < 3)
                {
                    continue;
                }
                if (sp[0].IndexOf("//") >= 0)
                {//コメント
                    continue;
                }
                //2番目のカラムがバージョン
                if (needVersion != sp[1])
                {
                    if (sp[1] != "ALL")
                    {
                        continue;
                    }
                }

                string[] hexStrings = sp[2].Split(' ');
                byte[] need = new byte[hexStrings.Length];
                for (int n = 0; n < hexStrings.Length; n++)
                {
                    need[n] = (byte)U.atoh(hexStrings[n]);
                }

                //Grepして調べる 結構重い.
                uint v = U.Grep(Program.ROM.Data, need, searchStartAddr, 0, 4);
                if (v != U.NOT_FOUND)
                {
                    return v;
                }
            }
            return U.NOT_FOUND;
        }

        uint g_SongTable_Addr = U.NOT_FOUND;
        uint g_UseSongID = 0x01;

        bool ImportS(string filename)
        {
            string error = SongUtil.ImportS(filename, g_SongTable_Addr + (g_UseSongID * 8), g_NIMAP_Addr);
            if (error != "")
            {
                R.ShowStopError(error);
                return false;
            }
            return true;
        }

        string PlayerEXE = "VGMusicStudio\\VG Music Studio.exe";
        Process PlayerProcess = null;
        bool ExecutePlayer()
        {
            try
            {
                string args2 = String.Format("-mp2k -songid {0} -filename", g_UseSongID);
                string player = Path.Combine(Program.BaseDirectory, PlayerEXE);
                this.PlayerProcess = MainFormUtil.RunAs(player, args2);
            }
            catch (Exception e)
            {
                R.ShowStopError("VGMusicStudioプロセスを実行できません。\r\n{0}", e.ToString());
                return false;
            }

            return this.PlayerProcess != null;
        }
        void KillProcessIfRunning()
        {
            if (PlayerProcess == null)
            {
                return ;
            }
            if (PlayerProcess.HasExited)
            {
                PlayerProcess = null;
                return;
            }
            PlayerProcess.CloseMainWindow();
            PlayerProcess = null;
        }

        bool ExecuteTextEditor(string filename)
        {
            string editorEXE = TextEditor.Text;
            if (editorEXE == "")
            {
                editorEXE = U.FindAssociatedExecutable(".txt");
            }

            try
            {
                MainFormUtil.ProgramRunAs(editorEXE, filename);
            }
            catch (Exception e)
            {
                R.ShowStopError(R._("can not execute process\r\n{0} {1}\r\n{2}"), editorEXE, filename, e.ToString());
                return false;
            }
            return true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        public static string EXESearch(string first_filter)
        {
            string title = "Please select tool";
            string filter = first_filter + "EXE|*.exe|All files|*";

            OpenFileDialog open = new OpenFileDialog();
            open.Title = title;
            open.Filter = filter;
            open.Multiselect = false;
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.ShowDialog();
            if (open.FileNames.Length <= 0)
            {
                return "";
            }
            return open.FileNames[0];
        }
        public static string ROMSearch(string first_filter)
        {
            string title = "Please select ROM";
            string filter = first_filter + "GBA|*.gba|All files|*";

            OpenFileDialog open = new OpenFileDialog();
            open.Title = title;
            open.Filter = filter;
            open.Multiselect = false;
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.ShowDialog();
            if (open.FileNames.Length <= 0)
            {
                return "";
            }
            return open.FileNames[0];
        }

        private void SelectRomFIlename_Click(object sender, EventArgs e)
        {
            string path = ROMSearch("");
            if (path == "")
            {
                return;
            }
            RomFilename.Text = path;
            Program.Config["RomFilename"] = path;
            Program.Config.Save();
        }

        private void SelectTextEditor_Click(object sender, EventArgs e)
        {
            string path = EXESearch("");
            if (path == "")
            {
                return;
            }
            TextEditor.Text = path;
            Program.Config["TextEditor"] = path;
            Program.Config.Save();
        }

        private void RomFilename_DoubleClick(object sender, EventArgs e)
        {
            SelectRomFIlename.PerformClick();
        }

        private void TextEditor_DoubleClick(object sender, EventArgs e)
        {
            SelectTextEditor.PerformClick();
        }

        public static string SFileSearch(string first_filter)
        {
            string title = "Please select s file";
            string filter = first_filter + "s file|*.s|All files|*";

            OpenFileDialog open = new OpenFileDialog();
            open.Title = title;
            open.Filter = filter;
            open.Multiselect = false;
            open.CheckFileExists = true;
            open.CheckPathExists = true;
            open.ShowDialog();
            if (open.FileNames.Length <= 0)
            {
                return "";
            }
            return open.FileNames[0];
        }
        private void label1_Click(object sender, EventArgs e)
        {
            string path = SFileSearch("");
            if (path == "")
            {
                return;
            }
            Play(path);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.W)
            {
                this.Close();
            }
        }
    }
}
