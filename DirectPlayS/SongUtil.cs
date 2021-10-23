using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace DirectPlayS
{
    public class SongUtil
    {
        static int[] WaitCode = new int[]{
            0, //0,
            1, //1,
            2, //2,
            3, //3,
            4, //4,
            5, //5,
            6, //6,
            7, //7,
            8, //8,
            9, //9,
            10, //10,
            11, //11,
            12, //12,
            13, //13,
            14, //14,
            15, //15,
            16, //16,
            17, //17,
            18, //18,
            19, //19,
            20, //20,
            21, //21,
            22, //22,
            23, //23,
            24, //24,
            28, //25,
            30, //26,
            32, //27,
            36, //28,
            40, //29,
            42, //30,
            44, //31,
            48, //32,
            52, //33,
            54, //34,
            56, //35,
            60, //36,
            64, //37,
            66, //38,
            68, //39,
            72, //40,
            76, //41,
            78, //42,
            80, //43,
            84, //44,
            88, //45,
            90, //46,
            92, //47,
            96, //48,
        };

        const uint WAIT_START = 0x80; //W00から
        const uint WAIT_END = 0x80 + 48;
        const uint EOT = 0xCE;
        const uint TIE = 0xCF;
        const uint NOTE_START = 0xD0; //N01から
        const uint NOTE_END = 0xFF;

        //ループのとび先も記録したいので適当なコードをねつ造する.
        const uint LOOP_LABEL_CODE = 0xFEFEFEFE;
        //番兵等の便宜上入れるが、意味をなさないイベント
        const uint DUMMY_CODE = 0xFFEEFFEE;
        //番兵等の便宜上入れるが、意味をなさないイベント
        const uint REVERB_CODE = 0xDDDDDDDD;

        static string[] KEYCODE = new string[]{
            "Cn",//0  CnM2 - Cn8
            "Cs",//1
            "Dn",//2
            "Ds",//3
            "En",//4
            "Fn",//5
            "Fs",//6
            "Gn",//7
            "Gs",//8
            "An",//9
            "As",//10
            "Bn",//11
        };

        static string[] MEMACC = new string[]{
            "mem_set", //0x0;
            "mem_add", //0x1;
            "mem_sub", //0x2;
            "mem_mem_set", //0x3;
            "mem_mem_add", //0x4;
            "mem_mem_sub", //0x5;
            "mem_beq", //0x6;
            "mem_bne", //0x7;
            "mem_bhi", //0x8;
            "mem_bhs", //0x9;
            "mem_bls", //0xA;
            "mem_blo", //0xB;
            "mem_mem_beq", //0xC;
            "mem_mem_bne", //0xD;
            "mem_mem_bhi", //0xE;
            "mem_mem_bhs", //0xF;
            "mem_mem_bls", //0x10;
            "mem_mem_blo", //0x11;
        };

        static uint byteToWait(uint b)
        {
            if (b < WAIT_START)
            {
                return 0;
            }
            if (b - WAIT_START >= WaitCode.Length)
            {
                return 0;
            }
            return (uint)WaitCode[(int)b - WAIT_START];
        }
        static uint byteToNote(uint b)
        {
            if (b + 1 < NOTE_START)
            {
                return 0;
            }
            if (b + 1 - NOTE_START >= WaitCode.Length)
            {
                return 0;
            }
            return (uint)WaitCode[(int)b + 1 - NOTE_START];
        }
        public static string getKeyCode(uint code)
        {
            int key = (int)code % 12;
            int keyN = (int)code / 12;
            if (keyN == 0)
            {
                return KEYCODE[key] + "M2";
            }
            else if (keyN == 1)
            {
                return KEYCODE[key] + "M1";
            }
            else if (keyN >= 2)
            {
                return KEYCODE[key] + (keyN - 2);
            }
            return "";
        }

        class SongInnerDataSt
        {
            public int globalID; //list[n] と同じ値.
            public string name;
            public uint ROMAllocAddr;
            public List<uint> useLabelRegist;
            public List<byte> list;
        };

        static List<KeyValuePair<string, int>> SortedEQU(Dictionary<string, int> equ)
        {
            return U.OrderBy<string, int>(equ, (x) => { return -(x.Key.Length); });
        }


        public static string ImportS(string filename, uint songtable_address, uint instrument_addr)
        {
            uint songheader_address = Program.ROM.p32(songtable_address + 0);

            Dictionary<string, int> equ = new Dictionary<string, int>();
            equ["voicegroup000"] = -1; //楽器テーブル　例外的な扱いをするので適当な値を入れる
            equ["MusicVoices"] = -1; //楽器テーブル　例外的な扱いをするので適当な値を入れる
            equ["mxv"] = (int)0x7F;
            equ["c_v"] = (int)0x40;
            equ["EOT"] = 0xCE;
            equ["FINE"] = 0xB1;
            equ["GOTO"] = 0xb2;
            equ["PATT"] = 0xb3;
            equ["PEND"] = 0xb4;
            equ["MEMACC"] = 0xb9;
            equ["PRIO"] = 0xba;
            equ["TEMPO"] = 0xbb;
            equ["KEYSH"] = 0xbc;
            equ["VOICE"] = 0xbd;
            equ["VOL"] = 0xbe;
            equ["PAN"] = 0xbf;
            equ["BEND"] = 0xc0;
            equ["BENDR"] = 0xc1;
            equ["LFOS"] = 0xc2;
            equ["LFODL"] = 0xc3;
            equ["MOD"] = 0xc4;
            equ["MODT"] = 0xc5;
            equ["TUNE"] = 0xc8;
            equ["gtp1"] = 0x01;
            equ["gtp2"] = 0x02;
            equ["gtp3"] = 0x03;

            for (uint i = WAIT_START; i <= WAIT_END; i++)
            {
                equ["W" + byteToWait(i).ToString("00")] = (int)i;
            }
            equ["TIE"] = (int)TIE;
            for (uint i = TIE + 1; i <= NOTE_END; i++)
            {
                equ["N" + byteToNote(i).ToString("00")] = (int)i;
            }
            for (uint i = 0; i <= 127; i++)
            {
                equ[getKeyCode(i)] = (int)i;
            }
            for (uint i = 0; i <= 127; i++)
            {
                equ["v" + i.ToString("000")] = (int)i;
            }
            for (uint i = 0; i < MEMACC.Length; i++)
            {
                equ[MEMACC[i]] = (int)i;
            }
            //16進数の場合があるらしいので、ある程度のルックアップテーブルを作ろう.
            for (uint i = 0; i < 0xf; i++)
            {
                equ["0x" + i.ToString("X")] = (int)i;
            }
            for (uint i = 0; i < 0xff; i++)
            {
                equ[U.To0xHexString(i)] = (int)i;
            }
            List<KeyValuePair<string, int>> equ_sorted = SortedEQU(equ);

            string[] lines = File.ReadAllLines(filename);

            string globalName = null;
            List<SongInnerDataSt> global = new List<SongInnerDataSt>();
            SongInnerDataSt current = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                //@以降はコメントなので消し去る.
                line = U.ClipCommentWithCharpAndAtmark(line);

                if (line.Length <= 1)
                {
                    continue;
                }

                //トークン分割   RemoveEmptyEntries で空値は無視.
                string[] token = line.Split(new string[] { " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (token.Length <= 0)
                {
                    continue;
                }
                if (token[0] == ".equ")
                {
                    if (token.Length <= 2)
                    {//エラー
                        return R.Error(".equは2つの引数が必要です.\r\n例: .equ name, value\r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }
                    string v = "";
                    for (int n = 2; n < token.Length; n++)
                    {
                        v += token[n];
                    }
                    try
                    {
                        equ[token[1]] = Expr(v, equ, equ_sorted);
                        equ_sorted = SortedEQU(equ);
                    }
                    catch (SyntaxErrorException e)
                    {
                        string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                        return R.Error("{2}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, errorMessage);
                    }
                    catch (EvaluateException e)
                    {
                        string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                        return R.Error("{2}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, errorMessage);
                    }
                    continue;
                }
                if (token[0] == ".global")
                {
                    if (globalName != null)
                    {
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error("global が2回定義されました。\r\n例: global aaa\r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }
                    globalName = token[1];
                    continue;
                }

                if (token[0].IndexOf(":") == token[0].Length - 1)
                {//ラベル
                    if (globalName == null)
                    {
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error("ラベルを書く前に、global 情報を定義してください.\r\nこのラベルがグローバルなのか、ローカルなのか調べるのに必要です。\r\n例: global aaa\r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }

                    string name = token[0].Substring(0, token[0].Length - 1);
                    if (isGlobalLabel(globalName, token[0]))
                    {
                        if (findGlobal(global, name) >= 0)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            return R.Error("グローバルラベル{2} すでに利用されています。\r\n\r\nFile:{0} Line:{1}", filename, i + 1, name);
                        }
                        current = new SongInnerDataSt();
                        current.name = name;
                        current.useLabelRegist = new List<uint>();
                        current.list = new List<byte>();
                        current.globalID = global.Count;
                        global.Add(current);
                    }
                    else
                    {
                        if (current == null)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            return R.Error("グローバルラベルがないのに、ローカルラベルが使われました。\r\nまずはグローバルラベルを定義してください。 \r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                        }
                    }

                    equ[name] = (current.globalID << 24) + current.list.Count; //相対座標で記録.
                    equ_sorted = SortedEQU(equ); 
                    continue;
                }

                if (token[0] == ".byte")
                {//byte単位で書き込む.
                    if (current == null)
                    {
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error("グローバルラベルがないのに、 .byte命令が使われました。\r\nまずはグローバルラベルを定義してください。 \r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }

                    if (token.Length <= 1)
                    {//エラー
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error(".byteは1つ以上の引数が必要です.\r\n例: .byte arg1....\r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }
                    for (int n = 1; n < token.Length; n++)
                    {
                        try
                        {
                            int v = Expr(token[n], equ, equ_sorted);
                            current.list.Add((byte)v);
                        }
                        catch (SyntaxErrorException e)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                            return R.Error(".byteパース中にエラー {2} \r\n{3}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, token[n], errorMessage);
                        }
                        catch (EvaluateException e)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                            return R.Error(".byteパース中にエラー {2} \r\n{3}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, token[n], errorMessage);
                        }
                    }
                    continue;
                }
                if (token[0] == ".word")
                {//wordといったが実際が4バイトポインタ
                    //
                    if (current == null)
                    {
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error("グローバルラベルがないのに、 .word命令が使われました。\r\nまずはグローバルラベルを定義してください。 \r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }

                    if (token.Length <= 1)
                    {//エラー
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error(".wordは1つ以上のラベル引数が必要です.\r\n例: .byte arg1....\r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }
                    for (int n = 1; n < token.Length; n++)
                    {
                        try
                        {
                            uint v = (uint)Expr(token[n], equ, equ_sorted);

                            {//それ以外の相対値 
                                current.useLabelRegist.Add((uint)current.list.Count);
                            }

                            U.append_u32(current.list, v);
                        }
                        catch (SyntaxErrorException e)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                            return R.Error(".wordパース中にエラー {2} \r\n{3}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, token[n], errorMessage);
                        }
                        catch (EvaluateException e)
                        {
                            InputFormRef.DoEvents(null, "Lines:" + i);
                            string errorMessage = e.ToString() + "\r\n" + R._("説明:\r\n") + MainFormUtil.GetExplainOfSFileURL();
                            return R.Error(".wordパース中にエラー {2} \r\n{3}\r\n\r\nFile:{0} Line:{1}", filename, i + 1, token[n], errorMessage);
                        }

                    }
                    continue;
                }
                if (token[0] == ".end")
                {
                    if (current == null)
                    {
                        InputFormRef.DoEvents(null, "Lines:" + i);
                        return R.Error("グローバルラベルがないのに、 .word命令が使われました。\r\nまずはグローバルラベルを定義してください。 \r\n\r\nFile:{0} Line:{1}", filename, i + 1);
                    }
                    U.append_u32(current.list, 0);
                    break;
                }
            }

            //書き込む前の事前チェック
            //ソングテーブルがあるかどうか確認.
            if (findGlobal(global, globalName) < 0)
            {
                return R.Error("ソングテーブルがありません。\r\n\r\nglobal sss\r\nsss:\r\nこのようなデータが必要です。\r\n");
            }

            //とりあえずデータを書き込む
            for (int i = 0; i < global.Count; i++)
            {
                byte[] data = global[i].list.ToArray();
                uint write_addr = InputFormRef.AppendBinaryData(data);

                global[i].ROMAllocAddr = write_addr;

                if (global[i].name == globalName)
                {//グローバルラベルなので、ソングテーブルです.
                    Program.ROM.write_p32(songtable_address + 0, global[i].ROMAllocAddr);
                }
            }
            //相対アドレスで書いている部分を描き戻す.
            for (int i = 0; i < global.Count; i++)
            {
                SongInnerDataSt g = global[i];
                for (int n = 0; n < g.useLabelRegist.Count; n++)
                {
                    uint rewrite_offset = g.useLabelRegist[n];
                    uint rewrite_addr = g.ROMAllocAddr + rewrite_offset;

                    uint rewrite_info = Program.ROM.u32(rewrite_addr);
                    if (rewrite_info == U.NOT_FOUND)
                    {
                        rewrite_info = 0;
                    }
                    int globalID = (int)((rewrite_info >> 24) & 0xFF);
                    uint offset = (rewrite_info & 0xFFFFFF);

                    uint new_pointer = global[globalID].ROMAllocAddr + offset;

                    Program.ROM.write_p32(rewrite_addr, new_pointer);
                }
            }
            //楽器テーブルの更新
            {
                uint songheader = Program.ROM.u32(songtable_address);
                if (! U.isSafetyPointer(songheader + 4))
                {//ソングヘッダーが存在しない.
                    return R.Error("ソングテーブルにソングヘッダがありません。");
                }
                Program.ROM.write_p32( U.toOffset(songheader) + 4, instrument_addr);
            }

            return "";
        }

        static int findGlobal(List<SongInnerDataSt> global, string name)
        {
            for (int i = 0; i < global.Count; i++)
            {
                if (global[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }
        static int Expr(string expr_value, Dictionary<string, int> equ, List<KeyValuePair<string, int>> equ_sorted)
        {
            int ret = 0;
            if (equ.TryGetValue(expr_value, out ret))
            {
                return ret;
            }

            string expr = expr_value;
            //変数を実際の値に置換します.
            foreach (var pair in equ_sorted)
            {
                expr = expr.Replace(pair.Key, pair.Value.ToString());
            }

            if (!isExprString(expr))
            {
                throw new SyntaxErrorException(R.Error("数式以外の文字が入っています。 expr:{0}", expr));
            }

            //see https://dobon.net/vb/dotnet/programing/eval.html
            System.Data.DataTable dt = new System.Data.DataTable();
            object result = dt.Compute(expr, "");
            string str = result.ToString();

            ret = 0;
            if (!int.TryParse(str, out ret))
            {
                ret = (int)U.atoi(str);
            }
            return ret;
        }
        static bool isNormalNumberString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    (str[i] >= '0' && str[i] <= '9'))
                {
                    return false;
                }
            }
            return true;
        }
        static bool isExprString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] == '+' || str[i] == '-' || str[i] == '*' || str[i] == '/' || str[i] == '(' || str[i] == ')' || str[i] == '.')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        static bool isGlobalLabel(string globalName, string token)
        {
            //グローバルラベル
            //global:
            //global_1:
            //global1:
            //
            //非グローバルラベル
            //global_1_1:
            //foo:
            if (token.IndexOf(globalName) != 0)
            {
                return false;
            }
            int i = globalName.Length;
            //skip _
            while (token[i] == '_')
            {
                i++;
            }
            //is number
            while (U.isnum(token[i]))
            {
                i++;
            }
            if (token[i] == ':')
            {//this is global label
                return true;
            }
            return false;
        }
        public static uint FindSongTablePointer(byte[] data)
        {
            byte[] search = new byte[] {
                0x00, 0xB5, 0x00, 0x04, 0x07, 0x4A, 0x08, 0x49,
                0x40, 0x0B, 0x40, 0x18, 0x83, 0x88, 0x59, 0x00,
                0xC9, 0x18, 0x89, 0x00, 0x89, 0x18, 0x0A, 0x68,
                0x01, 0x68, 0x10, 0x1C, 0x00, 0xF0
            };
            uint foundPoint = U.Grep(data, search);
            if (foundPoint == U.NOT_FOUND)
            {//見つからなかった
                return U.NOT_FOUND;
            }
            uint songpointer = foundPoint + (uint)search.Length + 10;
            songpointer = U.toOffset(songpointer);

            uint songlist = U.u32(data, songpointer);
            if (!U.isPointer(songlist))
            {
                return U.NOT_FOUND;
            }
            return songpointer;
        }
    }
}
