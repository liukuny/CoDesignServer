using System;
using System.Collections.Generic;
using System.Text;


using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LK.LKCommon;
using System.Xml;

namespace CoDesignServer
{
    public class PacketHead
    {
        // 升级
        public const int ACTIONCODE_UPDATE = 1;
        // 查询
        public const int ACTIONCODE_LOCATION = 2;
        // 插入和更新
        public const int ACTIONCODE_INSERTANDUPDATE = 3;
        // 上传文件
        public const int ACTIONCODE_UPLOADFILE = 4;

        // 最大ActionCode值
        public const int ACTIONCODE_MAXVALUE = 4;

        //public string FileVersion;
        public PacketHead() { }
        public void Init(int _DataBufLen, int _ActionCode, int _PacketCheck = 0, string _Version = "1.0.0.1")
        {
            ActionCode = _ActionCode;
            PacketDataLength = _DataBufLen;
            PacketCheck = _PacketCheck;
            FileVersion = _Version;
        }

        // 版本
        public string FileVersion { get => _FileVersion; set => _FileVersion = value; }
        // 动作代码[4字节]
        public int ActionCode { get => _ActionCode; set => _ActionCode = value; }
        // 数据长度[4字节]
        public int PacketDataLength { get => _PacketLength; set => _PacketLength = value; }
        // 数据校验和[4字节]
        public int PacketCheck { get => _PacketCheck; set => _PacketCheck = value; }

        // 报头大小
        public static int GetPacketHeaderSize() { return 32; }
        // 版本大小
        public static int GetVersionSize() { return 20; }

        public void HeadFromBuf(byte[] buf, int nOffset, int nSize)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            _FileVersion = encoding.GetString(buf, nOffset, GetVersionSize());
            _ActionCode = BitConverter.ToInt32(buf, nOffset + GetVersionSize());
            _PacketLength = BitConverter.ToInt32(buf, nOffset + GetVersionSize() + 4);
            _PacketCheck = BitConverter.ToInt32(buf, nOffset + GetVersionSize() + 8);
        }
        public void HeadToBuf(byte[] buf, int nOffset)
        {
            byte[] fv = Encoding.GetEncoding("GB2312").GetBytes(_FileVersion);
            Array.Clear(buf, nOffset, GetVersionSize());
            Array.Copy(fv, 0, buf, nOffset, _FileVersion.Length);
            Array.Copy(BitConverter.GetBytes(_ActionCode), 0, buf, nOffset + GetVersionSize(), 4);
            Array.Copy(BitConverter.GetBytes(_PacketLength), 0, buf, nOffset + GetVersionSize() + 4, 4);
            Array.Copy(BitConverter.GetBytes(_PacketCheck), 0, buf, nOffset + GetVersionSize() + 8, 4);
        }
        // 版本[20字节]
        private string _FileVersion;
        // 动作代码[4字节]
        private int _ActionCode;
        // 数据长度[4字节]
        private int _PacketLength;
        // 数据校验和[4字节]
        private int _PacketCheck;
    }
}