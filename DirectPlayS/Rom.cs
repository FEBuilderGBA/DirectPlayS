using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace DirectPlayS
{
    public class ROM
    {
        public string Filename { get; private set; }
        public byte[] Data{ get; private set; }
        public bool Modified { get; private set; }
        public bool IsVirtualROM { get; private set; }
        public string Version { get; private set; }


        public bool LoadLow(string name, byte[] data,string version)
        {
            this.Modified = false;
            this.Data = data;
            this.Filename = name;
            this.Version = U.getASCIIString(Program.ROM.Data, 0xAC, 4);

            return true;
        }


        public void Save(string name,bool silent)
        {
            U.WriteAllBytes(name, this.Data);

            if (!silent)
            {
                this.Modified = false;
            }
        }

        [MethodImpl(256)]
        public uint u32(uint addr)
        {
            return U.u32(Data, addr);
        }
        [MethodImpl(256)]
        public uint u16(uint addr)
        {
            return U.u16(Data, addr);
        }
        [MethodImpl(256)]
        public uint u8(uint addr)
        {
            return U.u8(Data, addr);
        }
        [MethodImpl(256)]
        public uint u4(uint addr, bool isHigh)
        {
            return U.u4(Data, addr, isHigh);
        }
        [MethodImpl(256)]
        public uint p32(uint addr)
        {
            if (addr >= this.Data.Length)
            {
                return 0;
            }
            uint a = u32(addr);

            a = U.toOffset(a);
            return a;
        }
        public uint p32p(uint addr)
        {
            uint a = p32(addr);
            return p32(a);
        }

        public void write_p32(uint addr, uint a)
        {
            U.write_u32(Data, addr, U.toPointer(a));
            Modified = true;
        }
        public void write_u32(uint addr, uint a)
        {
            U.write_u32(Data, addr, a);
            Modified = true;
        }
        public void write_u16(uint addr,uint a)
        {
            U.write_u16(Data, addr, a);
            Modified = true;
        }
        public void write_u8(uint addr,uint a)
        {
            U.write_u8(Data, addr, a);
            Modified = true;
        }
        public void write_u4(uint addr, uint a, bool isHigh)
        {
            U.write_u4(Data, addr, a, isHigh);
            Modified = true;
        }

        public bool write_resize_data(uint resize)
        {
            if (this.Data.Length == resize)
            {//サイズが同一なら何もしない
                return true;
            }
            if (resize > 0x02000000)
            {
                R.ShowStopError("32MB(0x02000000)より大きな領域を割り当てることはできません。\r\n要求サイズ:{0}", U.ToHexString(resize));
                return false;
            }

            //C#は refで プロパティを設定したものを渡せない愚かな仕様だから...
            //文句はMSまで.どうぞ.
            //Array.Resize(ref this.Data,(int)resize);
            byte[] _d = this.Data;
            Array.Resize(ref _d, (int)U.Padding4(resize));
            this.Data = _d;
            Modified = true;

            return true;
        }
        public void write_range(uint addr, byte[] write_data)
        {
            U.write_range(Data, addr, write_data);
            Modified = true;
        }


        public uint getBlockDataCount(uint addr, uint blocksize, Func<int, uint, bool> is_data_exists_callback)
        {
            if (addr == 0 || addr == U.NOT_FOUND)
            {
                return 0;
            }

            uint length = (uint)Data.Length;
            int i = 0;
            while (addr + blocksize <= length)
            {
                if (!is_data_exists_callback(i,addr))
                {
                    return (uint)i;
                }
                addr += blocksize;
                i++;
            }

//            R.Error("警告:データが途中で終わってしまいました。 addr:{0} data.Length:{1} countI:{2}", U.ToHexString(addr), U.ToHexString(length), i);
//            Debug.Assert(false);
            return (uint)i;
        }

        public uint getBlockDataCount(uint addr, Func<int, uint, bool> is_data_exists_callback, Func<uint, uint> next_addr_callback,uint minimamLength)
        {
            if (addr == 0 || addr == U.NOT_FOUND)
            {
                return 0;
            }

            uint length = (uint)Data.Length;

            int i = 0;
            while (addr + minimamLength  <= length)
            {
                if (!is_data_exists_callback(i, addr))
                {
                    return (uint)i;
                }
                addr = next_addr_callback(addr);
                i++;
            }

//            R.Error("警告:データが途中で終わってしまいました。 addr:{0} data.Length:{1} countI:{2}", U.ToHexString(addr), U.ToHexString(length), i);
//            Debug.Assert(false);
            return (uint)i;
        }


        //空き領域の探索.
        public uint FindFreeSpace(uint addr, uint needsize)
        {
            if (needsize > (uint)this.Data.Length)
            {
                return U.NOT_FOUND;
            }

            byte filldata = 0;

            uint length = (uint)this.Data.Length - needsize;
            addr = U.Padding4(addr);
            for (; addr < length; addr += 4)
            {
                if (this.Data[addr] == 0)
                {
                    filldata = 0x00;
                }
                else if (this.Data[addr] == 0xff)
                {
                    filldata = 0xff;
                }
                else
                {
                    continue;
                }

                uint start = addr;
                int  matchsize = 1;
                addr++;
                for (; addr < length; addr++)
                {
                    if (this.Data[addr] != filldata)
                    {
                        break;
                    }

                    matchsize++;
                    if (matchsize >= needsize)
                    {
                        return start;
                    }
                }
                addr = U.Padding4(addr);
            }
            return U.NOT_FOUND;
        }
        public void SetVirtualROMFlag(string srcfilename)
        {
            this.Filename = srcfilename + ".VIRTUAL";
            this.IsVirtualROM = true;
        }

        public void ClearModifiedFlag()
        {
            this.Modified = false;
        }

        public ROM Clone()
        {
            ROM newROM = new ROM();
            newROM.Filename = this.Filename;
            newROM.Data = (byte[])this.Data.Clone();
            newROM.Modified = this.Modified;
            newROM.IsVirtualROM = this.IsVirtualROM;
            return newROM;
        }

        public bool SwapNewROMData(byte[] newROMData, string name)
        {
            this.write_range(0, newROMData);

            return true;
        }
        public void SwapNewROMDataDirect(byte[] newROMData)
        {
            this.Data = newROMData;
        }
        public bool CompareByte(uint addr, byte[] bin)
        {
            if (addr + bin.Length >= this.Data.Length)
            {
                return false;
            }

            for (uint i = 0; i < bin.Length; i++)
            {
                if (this.Data[addr + i] != bin[i])
                {
                    return false;
                }
            }
            return true;
        }

    }

}
    