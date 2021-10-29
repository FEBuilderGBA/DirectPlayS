using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO.Compression;
using System.Reflection;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security;

namespace DirectPlayS
{
    //その他、雑多なもの.
    //名前タイプするのが面倒なので Util -> U とする.
    public static class U
    {
        //見つからない.
        public const uint NOT_FOUND = 0xFFFFFFFF;

        public static string at(string[] list, uint at, string def = "")
        {
            if (at >= list.Length)
            {
                return def;
            }
            return list[(int)at];
        }
        public static uint at(uint[] list, int at, uint def = 0)
        {
            if (at >= list.Length || at < 0)
            {
                return def;
            }
            return list[at];
        }
        public static string at(string[] list, int at, string def = "")
        {
            if (at >= list.Length || at < 0)
            {
                return def;
            }
            return list[at];
        }
        public static string at(List<string> list, uint at, string def = "")
        {
            if (at >= list.Count)
            {
                return def;
            }
            return list[(int)at];
        }
        public static string at(List<string> list, int at, string def = "")
        {
            if (at >= list.Count || at < 0)
            {
                return def;
            }
            return list[at];
        }

        public static string at(Dictionary<string, string> dic, string at, string def = "")
        {
            string a;
            if (!dic.TryGetValue(at, out a))
            {
                return def;
            }
            return a;
        }
        public static string at(Dictionary<uint, string> dic, uint at, string def = "")
        {
            string a;
            if (!dic.TryGetValue(at, out a))
            {
                return def;
            }
            return a;
        }
        public static string at(Dictionary<uint, string> dic, int at, string def = "")
        {
            return U.at(dic, (uint)at, def);
        }
        public static uint at(Dictionary<uint, uint> dic, uint at, uint def = 0)
        {
            uint a;
            if (!dic.TryGetValue(at, out a))
            {
                return def;
            }
            return a;
        }
        public static Bitmap at(Dictionary<string, Bitmap> dic, string at, Bitmap def = null)
        {
            Bitmap a;
            if (!dic.TryGetValue(at, out a))
            {
                return def;
            }
            return a;
        }
        public static bool mkdir(string dir)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception )
                {
                    //ディレクトリがロックされていて消せない場合があるらしい
                    //その場合、作るという目的は達成しているので、まあいいかなあ。
                   return false;
                }
            }
            Directory.CreateDirectory(dir);
            return true;
        }
        public static void DownloadFileByDirect(string save_filename, string download_url, InputFormRef.AutoPleaseWait pleaseWait)
        {
            U.HttpDownload(save_filename, download_url, Path.GetDirectoryName(download_url), pleaseWait);
        }
        public static void HttpDownload(string savefilename, string url, string referer = "", InputFormRef.AutoPleaseWait pleaseWait = null, System.Net.CookieContainer cookie = null)
        {
            HttpWebRequest request = HttpMakeRequest(url, referer, cookie);

            WebResponse rsp = request.GetResponse();
            using (Stream output = File.OpenWrite(savefilename))
            using (Stream input = rsp.GetResponseStream())
            {
                byte[] buffer = new byte[1024 * 8];
                int totalSize = (int)rsp.ContentLength;
                int readTotalSize = 0;
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);

                    if (pleaseWait != null)
                    {
                        readTotalSize += bytesRead;
                        if (totalSize == -1)
                        {
                            pleaseWait.DoEvents("Download: " + readTotalSize + "/" + "???");
                        }
                        else
                        {
                            pleaseWait.DoEvents("Download: " + readTotalSize + "/" + totalSize);
                        }
                    }
                }
            }

            rsp.Close();

            if (cookie != null)
            {
                System.Net.CookieCollection cookies = request.CookieContainer.GetCookies(request.RequestUri);
                cookie.Add(cookies);
            }
        }
        //https://qiita.com/Takezoh/items/3eff6806a59152656ddc
        //MONOには証明書が入っていないので別処理
        private static bool OnRemoteCertificateValidationCallback(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            //危険だけど継続する
            return true;
        }
        public static uint atoi(String a)
        {
            //C#のTryParseはC言語のatoiと違い、後ろに数字以外があると false が変えるので補正する.
            for (int i = 0; i < a.Length; i++)
            {
                if (!isnum(a[i]))
                {
                    a = a.Substring(0, i);
                    break;
                }
            }

            int ret = 0;
            if (!int.TryParse(a, out ret))
            {
                return 0;
            }
            return (uint)ret;
        }
        public static uint atoh(String a)
        {
            //C#のTryParseはC言語のatoiと違い、後ろに数字以外があると false が変えるので補正する.
            for (int i = 0; i < a.Length; i++)
            {
                if (!ishex(a[i]))
                {
                    a = a.Substring(0, i);
                    break;
                }
            }

            int ret = 0;
            if (!int.TryParse(a, System.Globalization.NumberStyles.HexNumber, null, out ret))
            {
                return 0;
            }
            return (uint)ret;
        }
        public static uint atoi0x(String a)
        {
            if (a.Length >= 2 && a[0] == '0' && a[1] == 'x')
            {
                return atoh(a.Substring(2));
            }
            if (a.Length >= 1 && a[0] == '$')
            {
                return atoh(a.Substring(1));
            }
            return atoi(a);
        }
        static string GenUserAgent()
        {
            System.OperatingSystem os = System.Environment.OSVersion;

            uint seed = U.atoi(DateTime.Now.ToString("yyMMddHH"));

            Random rand = new Random((int)seed);
            int SafariMinorVersion = 537;
            int SafariMajorVersion = 36;
            int Chrome1Version = 65;
            int Chrome2Version = 0;
            int Chrome3Version = 2107;
            int Chrome4Version = 108;

            string UserAgent = string.Format("Mozilla/5.0 (Windows NT {0}.{1}; Win64; x64) AppleWebKit/{2}.{3} (KHTML, like Gecko) Chrome/{4}.{5}.{6}.{7} Safari/{2}.{3}"
                , os.Version.Major//Windows 8では、「6」//OSのメジャーバージョン番号を表示する
                , os.Version.Minor//Windows 8では、「2」//OSのマイナーバージョン番号を表示する
                , SafariMinorVersion
                , SafariMajorVersion
                , Chrome1Version
                , Chrome2Version
                , Chrome3Version
                , Chrome4Version
                );
            return UserAgent;
        }
        static HttpWebRequest HttpMakeRequest(string url, string referer, System.Net.CookieContainer cookie = null)
        {
            ServicePointManager.ServerCertificateValidationCallback = OnRemoteCertificateValidationCallback;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2 

            string UserAgent = GenUserAgent(); //"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            //自動プロキシ検出を利用しない.
            //こちらの方が早くなります.
            request.Proxy = null;

            //貴方の好きなUAを使ってね。
            request.UserAgent = UserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (referer != "")
            {
                request.Referer = referer;
            }
            if (cookie != null)
            {
                request.CookieContainer = new System.Net.CookieContainer();
                request.CookieContainer.Add(cookie.GetCookies(request.RequestUri));
            }
            return request;
        }
        public static long GetFileSize(string filename)
        {
            FileInfo info = new FileInfo(filename);
            return info.Length;
        }
        public static void OpenURLOrFile(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ee)
            {
                R.ShowStopError(ee.ToString());
            }
        }
        //httpでそこそこ怪しまれずに通信する
        public static string HttpGet(string url, string referer = "", System.Net.CookieContainer cookie = null)
        {
            HttpWebRequest request = HttpMakeRequest(url, referer, cookie);
            string r = "";

            WebResponse rsp = request.GetResponse();
            Stream stm = rsp.GetResponseStream();
            if (stm != null)
            {
                StreamReader reader = new StreamReader(stm, Encoding.UTF8);
                r = reader.ReadToEnd();
                stm.Close();
            }
            rsp.Close();

            if (cookie != null)
            {
                System.Net.CookieCollection cookies = request.CookieContainer.GetCookies(request.RequestUri);
                cookie.Add(cookies);
            }

            return r;
        }

        public static String var_dump(object obj, int nest = 0)
        {
            if (obj == null)
            {
                return "null";
            }
            if (obj is uint || obj is int
                || obj is ushort || obj is short
                || obj is byte || obj is byte
                || obj is float || obj is double || obj is bool
                || obj is UInt16 || obj is Int16
                || obj is UInt32 || obj is Int32
                || obj is UInt64 || obj is Int64
                )
            {
                return obj.ToString();
            }
            if (obj is string)
            {
                return "\"" + obj.ToString() + "\"";
            }

            if (nest >= 2)
            {
                return "...";
            }

            StringBuilder sb = new StringBuilder();
            IEnumerable ienum = obj as IEnumerable;
            if (ienum != null)
            {
                sb.Append("{");
                foreach (object o in ienum)
                {
                    sb.Append(var_dump(o, nest + 1) + ",");
                }
                sb.Append("}");
                return sb.ToString();
            }

            sb.Append("{");
            const BindingFlags FINDS_FLAG = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo[] infoArray = obj.GetType().GetFields(FINDS_FLAG);
            foreach (FieldInfo info in infoArray)
            {
                object o = info.GetValue(obj);
                sb.Append(info.Name + ": " + var_dump(o, nest + 1) + ",");
            }
            sb.Append("}");
            return sb.ToString();
        }
        [DllImport("kernel32.dll")]
        static extern uint FormatMessage(
          uint dwFlags, IntPtr lpSource,
          uint dwMessageId, uint dwLanguageId,
          StringBuilder lpBuffer, int nSize,
          IntPtr Arguments);
        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        const uint FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;

        //https://www.atmarkit.co.jp/fdotnet/dotnettips/741win32errmsg/win32errmsg.html
        public static string HRESULTtoString(int errCode)
        {
            StringBuilder message = new StringBuilder(1024);

            FormatMessage(
              FORMAT_MESSAGE_FROM_SYSTEM,
              IntPtr.Zero,
              (uint)errCode,
              0,
              message,
              message.Capacity,
              IntPtr.Zero);

            return message.ToString();
        }

        public static bool isalhpa(char a)
        {
            return isalhpa((byte)a);
        }
        public static bool isalhpa(byte a)
        {
            return ((a >= 'a' && a <= 'z')
                || (a >= 'A' && a <= 'Z')
                );
        }
        public static bool isalhpanum(char a)
        {
            return isalhpanum((byte)a);
        }
        public static bool isalhpanum(byte a)
        {
            return (a >= 'a' && a <= 'z')
                || (a >= 'A' && a <= 'Z')
                || (a >= '0' && a <= '9')
                ;
        }
        public static bool isnum_f(char a)
        {
            return isnum_f((byte)a);
        }
        public static bool isnum_f(byte a)
        {
            return ((a >= '0' && a <= '9') || a == '.');
        }
        public static bool isnum(char a)
        {
            return isnum((byte)a);
        }
        public static bool isnum(byte a)
        {
            return (a >= '0' && a <= '9');
        }
        public static bool ishex(char a)
        {
            return ishex((byte)a);
        }
        public static bool ishex(byte a)
        {
            return (a >= '0' && a <= '9') || (a >= 'a' && a <= 'f') || (a >= 'A' && a <= 'F');
        }
        public static bool isAlphaNumString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] >= 'a' && str[i] <= 'z')
                   || (str[i] >= 'A' && str[i] <= 'Z')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static void append_u32(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF)));
            data.Add((byte)((a & 0xFF00) >> 8));
            data.Add((byte)((a & 0xFF0000) >> 16));
            data.Add((byte)((a & 0xFF000000) >> 24));
        }
        public static void append_u24(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF)));
            data.Add((byte)((a & 0xFF00) >> 8));
            data.Add((byte)((a & 0xFF0000) >> 16));
        }
        public static void append_u16(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF)));
            data.Add((byte)((a & 0xFF00) >> 8));
        }
        public static void append_u8(List<byte> data, uint a)
        {
            data.Add((byte)a);
        }
        public static void append_big32(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF000000) >> 24));
            data.Add((byte)((a & 0xFF0000) >> 16));
            data.Add((byte)((a & 0xFF00) >> 8));
            data.Add((byte)((a & 0xFF)));
        }
        public static void append_big24(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF0000) >> 16));
            data.Add((byte)((a & 0xFF00) >> 8));
            data.Add((byte)((a & 0xFF)));
        }
        public static void append_big16(List<byte> data, uint a)
        {
            data.Add((byte)((a & 0xFF00) >> 8));
            data.Add((byte)((a & 0xFF)));
        }
        public static void append_big8(List<byte> data, uint a)
        {
            data.Add((byte)a);
        }
        public static void append_range(List<byte> data, string str)
        {
            byte[] b = System.Text.Encoding.GetEncoding("ASCII").GetBytes(str);
            data.AddRange(b);
        }
        public static bool isAsciiString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= 0x7f)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isAlphaString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                     ((str[i] >= 'a' && str[i] <= 'z')
                   || (str[i] >= 'A' && str[i] <= 'Z')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isHexString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] >= 'a' && str[i] <= 'f')
                   || (str[i] >= 'A' && str[i] <= 'F')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isNumString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    ((str[i] >= '0' && str[i] <= '9')
                   || (str[i] == '\0')
                    ))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool isAscii(byte a)
        {
            return (a >= 0x20 && a <= 0x7e);
        }

        public static string ToHexString(decimal a)
        {
            return ToHexString((uint)a);
        }
        public static string ToHexString(int a)
        {
            if (a <= 0xff)
            {
                return a.ToString("X02");
            }
            if (a <= 0xffff)
            {
                return a.ToString("X04");
            }
            if (a <= 0x7fffffff)
            {
                return a.ToString("X08");
            }
            return "???";
        }
        public static string ToHexString8(int a)
        {
            return a.ToString("X08");
        }
        public static string ToHexString8(uint a)
        {
            return a.ToString("X08");
        }
        public static string ToHexString2(int a)
        {
            return a.ToString("X02");
        }
        public static string ToHexString2(uint a)
        {
            return a.ToString("X02");
        }

        public static string To0xHexString(uint a)
        {
            return "0x" + ToHexString(a);
        }
        public static string To0xHexString(int a)
        {
            return "0x" + ToHexString(a);
        }
        public static string ToHexString(uint a)
        {
            if (a <= 0xff)
            {
                return a.ToString("X02");
            }
            if (a <= 0xffff)
            {
                return a.ToString("X04");
            }
            if (a <= 0xffffff)
            {
                return a.ToString("X06");
            }
            if (a <= 0xffffffff)
            {
                return a.ToString("X08");
            }
            return "???";
        }
        public static string GetRelativePath(string uri1, string uri2)
        {
            Uri u1 = new Uri(uri1);
            Uri u2 = new Uri(uri2);

            Uri relativeUri = u1.MakeRelativeUri(u2);

            string relativePath = relativeUri.ToString();

            relativePath = relativePath.Replace('/', '\\');
            relativePath = Uri.UnescapeDataString(relativePath);

            return (relativePath);
        }
        public static string UrlDecode(string urlString)
        {
            return Uri.UnescapeDataString(urlString);
        }

        //一時的にカレントディレクトリを移動する.
        public class ChangeCurrentDirectory : IDisposable
        {
            string current_dir;
            public ChangeCurrentDirectory(string dir)
            {
                current_dir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Path.GetDirectoryName(dir));
            }
            public void Dispose()
            {
                Directory.SetCurrentDirectory(current_dir);
            }
        }

        public static String convertByteToStringDump(byte[] data)
        {
            String bin = "";
            for (uint i = 0; i < data.Length; i++)
            {
                uint a = u8(data, i);
                bin += a.ToString("X02");
            }
            return bin;
        }
        public static byte[] convertStringDumpToByte(string d)
        {
            byte[] r = new byte[d.Length / 2];
            Array.Clear(r, 0, r.Length);

            int length = r.Length * 2;

            for (int len = 0; len < length; len++)
            {
                if ((d[len] >= '0' && d[len] <= '9'))
                {
                    U.write_u4(r, (uint)(len / 2), (uint)(d[len] - '0'), (len % 2) == 0);
                }
                else if ((d[len] >= 'a' && d[len] <= 'f'))
                {
                    U.write_u4(r, (uint)(len / 2), (uint)(d[len] - 'a' + 10), (len % 2) == 0);
                }
                else if ((d[len] >= 'A' && d[len] <= 'F'))
                {
                    U.write_u4(r, (uint)(len / 2), (uint)(d[len] - 'A' + 10), (len % 2) == 0);
                }
                else
                {
                    break;
                }
            }
            return r;
        }

        public static void write_range(byte[] data, uint addr, byte[] write_data)
        {
            check_safety(data, addr + (uint)write_data.Length);
            Array.Copy(write_data, 0, data, addr, write_data.Length);
        }

        public static uint big32(byte[] data, uint addr)
        {
            check_safety(data, addr + 4);
            return data[addr + 3] + ((uint)data[addr + 2] << 8) + ((uint)data[addr + 1] << 16) + ((uint)data[addr + 0] << 24);
        }
        public static uint big24(byte[] data, uint addr)
        {
            check_safety(data, addr + 3);
            return data[addr + 2] + ((uint)data[addr + 1] << 8) + ((uint)data[addr + 0] << 16);
        }
        public static uint big16(byte[] data, uint addr)
        {
            check_safety(data, addr + 2);
            return data[addr + 1] + ((uint)data[addr + 0] << 8);
        }
        public static uint big8(byte[] data, uint addr)
        {
            check_safety(data, addr);
            return data[addr];
        }

        [MethodImpl(256)]
        public static uint u32(byte[] data, uint addr)
        {
            check_safety(data, addr + 4);
            return data[addr] + ((uint)data[addr + 1] << 8) + ((uint)data[addr + 2] << 16) + ((uint)data[addr + 3] << 24);
        }

        public static uint u24(byte[] data, uint addr)
        {
            check_safety(data, addr + 3);
            return data[addr] + ((uint)data[addr + 1] << 8) + ((uint)data[addr + 2] << 16);
        }

        [MethodImpl(256)]
        public static uint u16(byte[] data, uint addr)
        {
            check_safety(data, addr + 2);
            return data[addr] + ((uint)data[addr + 1] << 8);
        }

        [MethodImpl(256)]
        public static uint u8(byte[] data, uint addr)
        {
            check_safety(data, addr + 1);
            return data[addr];
        }
        [MethodImpl(256)]
        public static uint u4(byte[] data, uint addr, bool isHigh)
        {
            check_safety(data, addr + 1);
            if (isHigh)
            {
                return (uint)((data[addr] >> 4) & 0xf);
            }
            else
            {
                return (uint)(data[addr] & 0xf);
            }
        }

        [MethodImpl(256)]
        public static uint p32(byte[] data, uint addr)
        {
            uint a = U.u32(data, addr);
            a = U.toOffset(a);
            return a;
        }
        public static void write_p32(byte[] data, uint addr, uint a)
        {
            write_u32(data, addr, U.toPointer(a));
        }
        public static void write_u32(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 4);
            data[addr] = (byte)((a & 0xFF));
            data[addr + 1] = (byte)((a & 0xFF00) >> 8);
            data[addr + 2] = (byte)((a & 0xFF0000) >> 16);
            data[addr + 3] = (byte)((a & 0xFF000000) >> 24);
        }
        public static void write_u24(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 3);
            data[addr] = (byte)((a & 0xFF));
            data[addr + 1] = (byte)((a & 0xFF00) >> 8);
            data[addr + 2] = (byte)((a & 0xFF0000) >> 16);
        }
        public static void write_u16(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 2);
            data[addr] = (byte)((a & 0xFF));
            data[addr + 1] = (byte)((a & 0xFF00) >> 8);
        }
        public static void write_u8(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 1);
            data[addr] = (byte)a;
        }
        public static void write_u4(byte[] data, uint addr, uint a, bool isHigh)
        {
            check_safety(data, addr + 1);
            if (isHigh)
            {
                data[addr] = (byte)((byte)(data[addr] & 0xf) | (byte)((a & 0xf) << 4));
            }
            else
            {
                data[addr] = (byte)((byte)(data[addr] & 0xf0) | (byte)(a & 0xf));
            }
        }
        public static void write_big32(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 4);
            data[addr + 0] = (byte)((a & 0xFF000000) >> 24);
            data[addr + 1] = (byte)((a & 0xFF0000) >> 16);
            data[addr + 2] = (byte)((a & 0xFF00) >> 8);
            data[addr + 3] = (byte)((a & 0xFF));
        }
        public static void write_big24(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 3);
            data[addr + 0] = (byte)((a & 0xFF0000) >> 16);
            data[addr + 1] = (byte)((a & 0xFF00) >> 8);
            data[addr + 2] = (byte)((a & 0xFF));
        }
        public static void write_big16(byte[] data, uint addr, uint a)
        {
            check_safety(data, addr + 2);
            data[addr + 0] = (byte)((a & 0xFF00) >> 8);
            data[addr + 1] = (byte)((a & 0xFF));
        }

        //C#が仕事をさぼるので、我々が代わりに仕事をする.
        [MethodImpl(256)]
        static void check_safety(byte[] data, uint addr)
        {
            if (addr > data.Length)
            {
                throw new System.IndexOutOfRangeException(String.Format("Max length:{0}(0x{1}) Access:{2}(0x{3})", data.Length, U.ToHexString(data.Length), addr, U.ToHexString(addr)));
            }
        }

        public static bool is_RAMPointer(uint a)
        {
            return is_03RAMPointer(a) || is_02RAMPointer(a);
        }
        public static bool is_ROMorRAMPointer(uint a)
        {
            return isPointer(a) || is_03RAMPointer(a) || is_02RAMPointer(a);
        }
        public static bool is_ROMorRAMPointerOrNULL(uint a)
        {
            return isPointerOrNULL(a) || is_03RAMPointer(a) || is_02RAMPointer(a);
        }

        public static bool is_03RAMPointer(uint a)
        {
            return (a >= 0x03000000 && a < 0x03007FFF);
        }

        public static bool is_02RAMPointer(uint a)
        {
            return (a >= 0x02000000 && a < 0x0203FFFF);
        }
        public static bool is_0EDiskPointer(uint a)
        {
            return (a >= 0x0E000000 && a < 0x0E008000);
        }
        public static bool isROMPointer(uint a)
        {
            return isPointer(a);
        }
        [MethodImpl(256)]
        public static bool isPointer(uint a)
        {
            return (a >= 0x08000000 && a < 0x0A000000);
        }
        [MethodImpl(256)]
        public static bool isPointerOrNULL(uint a)
        {
            return U.isPointer(a) || a == 0x0;
        }

        [MethodImpl(256)]
        public static bool isOffset(uint a)
        {
            return (a < 0x02000000 && a >= 0x00000000);
        }

        [MethodImpl(256)]
        public static uint toOffset(uint a)
        {
            if (a <= 1)
            {
                return a;
            }
            if (U.isPointer(a))
            {
                return a - 0x08000000;
            }
            return a;
        }
        [MethodImpl(256)]
        public static uint toOffset(decimal a)
        {
            return toOffset((uint)a);
        }

        [MethodImpl(256)]
        public static uint toPointer(uint a)
        {
            if (a <= 1)
            {
                return a;
            }
            if (U.isOffset(a))
            {
                return a + 0x08000000;
            }
            return a;
        }

        static int ClipCommentIndexOf(string str, string need)
        {
            int index = str.IndexOf(need);
            if (index < 0)
            {
                return -1;
            }
            if (index == 0)
            {
                return 0;
            }
            if (str[index - 1] == ' ' || str[index - 1] == '\t')
            {
                return index - 1;
            }
            return -1;
        }
        public static string ClipComment(string str)
        {
            int term = ClipCommentIndexOf(str, "{J}");
            if (term >= 0)
            {//言語指定を飛ばす
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "{U}");
            if (term >= 0)
            {//言語指定を飛ばす
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "//");
            if (term >= 0)
            {//コメント
                str = str.Substring(0, term);
            }
            return str;
        }
        public static string ClipCommentWithCharpAndAtmark(string str)
        {
            int term = ClipCommentIndexOf(str, "{J}");
            if (term >= 0)
            {//言語指定を飛ばす
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "{U}");
            if (term >= 0)
            {//言語指定を飛ばす
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "//");
            if (term >= 0)
            {//コメント
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "#");
            if (term >= 0)
            {//コメント
                str = str.Substring(0, term);
            }
            term = ClipCommentIndexOf(str, "@");
            if (term >= 0)
            {//コメント
                str = str.Substring(0, term);
            }
            return str;
        }
        [MethodImpl(256)]
        public static bool isSafetyOffset(uint a)
        {
            return (a < 0x02000000 && a >= 0x00000100 && a < Program.ROM.Data.Length);
        }

        [MethodImpl(256)]
        public static bool isSafetyPointer(uint a)
        {
            return (a < 0x0A000000 && a >= 0x08000100 && a - 0x08000000 < Program.ROM.Data.Length);
        }

        [MethodImpl(256)]
        public static bool isSafetyOffset(uint a, ROM rom)
        {
            return (a < 0x02000000 && a >= 0x00000100 && a < rom.Data.Length);
        }

        [MethodImpl(256)]
        public static bool isSafetyPointer(uint a, ROM rom)
        {
            return (a < 0x0A000000 && a >= 0x08000100 && a - 0x08000000 < rom.Data.Length);
        }

        public static bool isSafetyZArray(uint a)
        {
            return (a < Program.ROM.Data.Length);
        }

        //偶数か？
        public static bool isEven(int size)
        {
            return (size & 1) == 0;
        }
        public static bool isEven(uint size)
        {
            return (size & 1) == 0;
        }

        public class FunctionalComparer<T> : IComparer<T>
        {
            private Func<T, T, int> comparer;
            public FunctionalComparer(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }
            public int Compare(T x, T y)
            {
                return comparer(x, y);
            }
        };
        public class FunctionalComparerOne<T> : IComparer<T>
        {
            private Func<T, int> toInt;
            public FunctionalComparerOne(Func<T, int> toInt)
            {
                this.toInt = toInt;
            }
            public int Compare(T x, T y)
            {
                return toInt(x) - toInt(y);
            }
        };
        //DICソート
        public static List<KeyValuePair<TKey, TValue>> OrderBy<TKey, TValue>
            (Dictionary<TKey, TValue> dic, Func<KeyValuePair<TKey, TValue>, int> toInt)
        {
            List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
            foreach (KeyValuePair<TKey, TValue> pair in dic)
            {
                list.Add(pair);
            }
            FunctionalComparerOne<KeyValuePair<TKey, TValue>> comp
                = new FunctionalComparerOne<KeyValuePair<TKey, TValue>>(toInt);
            list.Sort(comp);
            return list;
        }
        [MethodImpl(256)]
        public static uint Padding4(uint p)
        {
            uint mod = p % 4;
            if (mod == 0)
            {
                return p;
            }
            else
            {
                return p + (4 - mod);
            }
        }

        [MethodImpl(256)]
        public static int Padding4(int p)
        {
            int mod = p % 4;
            if (mod == 0)
            {
                return p;
            }
            else
            {
                return p + (4 - mod);
            }
        }
        public static uint Grep(byte[] data, byte[] need, uint start = 0x100, uint end = 0, uint blocksize = 1)
        {
            if (end == 0 || end == U.NOT_FOUND)
            {//終端が明記されない場合は、自動的にデータの終端
                end = (uint)data.Length;
            }

            if (need.Length <= 0)
            {
                return U.NOT_FOUND;
            }
            if (start > end)
            {//データ数が足りない
                return U.NOT_FOUND;
            }
            uint length = end;
            if (length < need.Length)
            {//検索する文字列より、検索されるデータのほうが短い
                return U.NOT_FOUND;
            }
            length -= (uint)need.Length;
            byte needfirst = need[0];

            for (uint i = start; i <= length; i += blocksize)
            {
                if (data[i] != needfirst)
                {
                    continue;
                }

                uint match = (uint)need.Length;
                uint n = 1;
                for (; n < match; n++)
                {
                    if (data[i + n] != need[n])
                    {
                        break;
                    }
                }
                if (n >= match)
                {
                    return i;
                }
            }
            return U.NOT_FOUND;
        }
        public static uint GrepPointer(byte[] data, uint needaddr, uint start = 0x100, uint end = 0)
        {
            if (needaddr == 0 || needaddr == U.NOT_FOUND)
            {
                return U.NOT_FOUND;
            }
            if (end == 0 || end == U.NOT_FOUND)
            {
                end = (uint)data.Length;
            }
            else
            {
                end = (uint)Math.Min((uint)data.Length, end);
            }
            if (end < 4)
            {
                return U.NOT_FOUND;
            }
            end -= (uint)4;

            needaddr = U.toPointer(needaddr);

            for (uint i = start; i <= end; i += 4)
            {
                if (data[i + 3] == 0x08 || data[i + 3] == 0x09)
                {
                    if (U.u32(data, i) == needaddr)
                    {
                        return i;
                    }
                }
            }
            return U.NOT_FOUND;
        }
        //CRC32計算
        //see http://kagasu.hatenablog.com/entry/2016/11/21/202302
        public class CRC32
        {
            private const int TABLE_LENGTH = 256;
            private uint[] crcTable;

            public CRC32()
            {
                BuildCRC32Table();
            }

            private void BuildCRC32Table()
            {
                crcTable = new uint[256];
                for (uint i = 0; i < 256; i++)
                {
                    var x = i;
                    for (var j = 0; j < 8; j++)
                    {
                        x = (uint)((x & 1) == 0 ? x >> 1 : -306674912 ^ x >> 1);
                    }
                    crcTable[i] = x;
                }
            }

            public uint Calc(byte[] buf)
            {
                uint num = uint.MaxValue;
                for (var i = 0; i < buf.Length; i++)
                {
                    num = crcTable[(num ^ buf[i]) & 255] ^ num >> 8;
                }

                return (uint)(num ^ -1);
            }
        }
        [SecuritySafeCritical]
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                string error = R.ExceptionToString(e);
                R.ShowStopError(error);
            }
        }
        //まともなsubstr 長さがたりなかったときに例外ではなく、末尾までを返す.
        public static string substr(string str, int start, int length)
        {
            if (start < 0)
            {
                start = str.Length - start;
                if (start < 0)
                {
                    start = 0;
                }
            }
            if (start >= str.Length)
            {
                return "";
            }
            if (start + length >= str.Length)
            {
                if (str.Length < start)
                {
                    length = 0;
                }
                else
                {
                    length = str.Length - start;
                }
            }
            if (length < 0)
            {
                return "";
            }
            return str.Substring(start, length);
        }
        public static string substr(string str, int start)
        {
            if (start < 0)
            {
                start = str.Length + start;
                if (start < 0)
                {
                    start = 0;
                }
            }
            if (start >= str.Length)
            {
                return "";
            }
            return str.Substring(start);
        }

        //オプション引数 --mode=foo とかを、dic["--mode"]="foo" みたいに変換します. 
        public static Dictionary<string, string> OptionMap(string[] args, string defautFilenameOption)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length <= 2)
                {
                    continue;
                }
                if (args[i][0] == '-' && args[i][1] == '-')
                {
                    int a = args[i].IndexOf('=');
                    if (a <= 0)
                    {//値なし 作る予定はないが...
                        dic[args[i]] = "";
                    }
                    else
                    {
                        dic[U.substr(args[i], 0, a)] = args[i].Substring(a + 1);
                    }
                }
                else if (File.Exists(args[i]))
                {//引数がない けど、ファイル名ならば
                    dic[defautFilenameOption] = args[i];
                }
                else
                {
                    dic[""] = args[i];
                }
            }
            return dic;
        }
        public static string MakeFilename(string addname, string override_ext = null)
        {
            String dir = Path.GetDirectoryName(Program.ROM.Filename);
            String filename = Path.GetFileNameWithoutExtension(Program.ROM.Filename);
            String ext;
            if (override_ext == null)
            {
                if (Program.ROM.IsVirtualROM)
                {//仮想ROMの場合、拡張子がないので、便宜上 gbaをつけます.
                    ext = ".gba";
                }
                else
                {
                    ext = Path.GetExtension(Program.ROM.Filename);
                }
            }
            else
            {
                ext = override_ext;
            }

            string ret = Path.Combine(dir, filename + "." + addname + ext);
            return ret;
        }
        //外部アプリで実行するため、一時的に出力します.
        public static string WriteTempROM(string addname)
        {
            string t = MakeFilename(addname);
            Program.ROM.Save(t, true);
            return t;
        }

        public static string escape_shell_args(string str)
        {
            if (str.Length > 0 && str[str.Length - 1] == '\\')
            {//最後に \ があれば \\ として逃げる. 
                str = str + "\\ ";
            }
            str = str.Replace("\"", "\\\"");
            return '"' + str + '"';
        }

        /// <summary>
        /// 指定された拡張子に関連付けられた実行ファイルのパスを取得する。
        /// </summary>
        /// <param name="extName">".txt"などの拡張子。</param>
        /// <returns>見つかった時は、実行ファイルのパス。
        /// 見つからなかった時は、空の文字列。</returns>
        /// <example>
        /// 拡張子".txt"に関連付けられた実行ファイルのパスを取得する例
        /// <code>
        /// string exePath = FindAssociatedExecutable(".txt");
        /// </code>
        /// </example>
        public static string FindAssociatedExecutable(string extName)
        {
            //pszOutのサイズを取得する
            uint pcchOut = 0;
            //ASSOCF_INIT_IGNOREUNKNOWNで関連付けられていないものを無視
            //ASSOCF_VERIFYを付けると検証を行うが、パフォーマンスは落ちる
            AssocQueryString(AssocF.Init_IgnoreUnknown, AssocStr.Executable,
                extName, null, null, ref pcchOut);
            if (pcchOut == 0)
            {
                return string.Empty;
            }
            //結果を受け取るためのStringBuilderオブジェクトを作成する
            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            //関連付けられた実行ファイルのパスを取得する
            AssocQueryString(AssocF.Init_IgnoreUnknown, AssocStr.Executable,
                extName, null, pszOut, ref pcchOut);
            //結果を返す
            return pszOut.ToString();
        }

        [DllImport("Shlwapi.dll",
            SetLastError = true,
            CharSet = CharSet.Auto)]
        private static extern uint AssocQueryString(AssocF flags,
            AssocStr str,
            string pszAssoc,
            string pszExtra,
            [Out] StringBuilder pszOut,
            [In][Out] ref uint pcchOut);

        [Flags]
        private enum AssocF
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_FixedProgId = 0x800,
            IsProtocol = 0x1000,
            InitForFile = 0x2000,
        }

        private enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            SupportedUriProtocols,
            Max,
        }
        public static void AddCancelButton(Form f)
        {
            if (f.CancelButton != null)
            {
                return;
            }
            Button cancelButton = new Button();
            cancelButton.Click += (Object Sender, EventArgs e) =>
            {
                f.DialogResult = DialogResult.Cancel;
                f.Close();
            };
            f.DialogResult = DialogResult.Cancel;
            f.CancelButton = cancelButton;
        }
        public static byte[] getBinaryData(byte[] data, uint addr, uint count)
        {
            if (data.Length <= addr + count)
            {
                if (data.Length == 0)
                {
                    return new byte[0];
                }
                if (addr >= data.Length - 1)
                {
                    addr = (uint)data.Length - 1;
                }
                count = (uint)(data.Length) - addr;
            }
            check_safety(data, addr + count);
            byte[] ret = new byte[count];

            Array.Copy(data, addr, ret, 0, count);
            return ret;
        }
        public static byte[] getBinaryData(byte[] data, uint addr, int count)
        {
            if (count < 0)
            {
                R.Error("U.getBinaryData pointer:{0} count:{1}", U.To0xHexString(addr), count);
                Debug.Assert(false);
                return new byte[0];
            }
            return getBinaryData(data, addr, (uint)count);
        }
        public static String getASCIIString(byte[] data, uint addr, int length)
        {
            if (length <= 0) return "";
            byte[] d = U.getBinaryData(data, addr, length);
            return System.Text.Encoding.GetEncoding("ASCII").GetString(d);
        }
        //拡張子を取得 結果は必ず大文字 .PNG みたいに
        public static string GetFilenameExt(string filename)
        {
            try
            {
                return Path.GetExtension(filename).ToUpper();
            }
            catch (ArgumentException)
            {
                return "";
            }
        }
        public static void AllowDropFilename(Control self
            , string[] allowExts
            , Action<string> callback)
        {
            self.AllowDrop = true;
            self.DragEnter += (sender, e) =>
            {
                //ファイルがドラッグされている場合、カーソルを変更する
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                    if (fileName.Length <= 0)
                    {
                        return;
                    }

                    string ext = U.GetFilenameExt(fileName[0]);
                    if (Array.IndexOf(allowExts, ext) < 0)
                    {
                        return;
                    }

                    e.Effect = DragDropEffects.Copy;
                }
            };
            self.DragDrop += (sender, e) =>
            {
                //ドロップされたファイルの一覧を取得
                string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (fileName.Length <= 0)
                {
                    return;
                }
                string ext = U.GetFilenameExt(fileName[0]);
                if (Array.IndexOf(allowExts, ext) < 0)
                {
                    return;
                }
                e.Effect = DragDropEffects.None;
                for (int i = 0; i < fileName.Length; i++)
                {
                    callback(fileName[i]);
                }
            };
        }
        public static bool SelectedIndexSafety(ListBox list, decimal selectID, bool selectFocus = false)
        {
            return SelectedIndexSafety(list, (int)selectID, selectFocus);
        }
        public static bool SelectedIndexSafety(ListBox list, uint selectID, bool selectFocus = false)
        {
            return SelectedIndexSafety(list, (int)selectID, selectFocus);
        }
        public static bool SelectedIndexSafety(ListBox list, int selectID, bool selectFocus = false)
        {
            if (selectID < 0)
            {
                selectID = 0;
            }

            if (selectID < list.Items.Count)
            {
                list.SelectedIndex = selectID;
                if (selectFocus)
                {
                    list.Focus();
                }
                return true;
            }
            return false;
        }
        public static bool SelectedIndexSafety(ComboBox list, uint selectID, bool selectFocus = false)
        {
            return SelectedIndexSafety(list, (int)selectID, selectFocus);
        }
        public static bool SelectedIndexSafety(ComboBox list, int selectID, bool selectFocus = false)
        {
            if (selectID < 0)
            {
                selectID = 0;
            }

            if (list.Items.Count < 0)
            {//件数が0件
                Debug.Assert(false);
                list.SelectedIndex = -1;
                return false;
            }

            if (list.Items.Count > selectID)
            {
                list.SelectedIndex = selectID;
                if (selectFocus)
                {
                    list.Focus();
                }
                return true;
            }
            return false;
        }
    }
}

