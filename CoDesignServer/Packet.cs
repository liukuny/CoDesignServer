using System;
using System.Collections.Generic;
using System.Text;


//using System.Threading;
//using System.Net;
//using System.Net.Sockets;
//using System.IO;
//using LK.LKCommon;
//using System.Xml;

namespace CoDesignServer
{
    public class PacketUpdate
    {
        // 创建比较版本的应答报文
        public void CreateCVAckPacket(int nUpdate, int nFileLen)
        {
            // int 应答版本比较结果（1 - 需要更新； 0 - 不需要更新）
            _DataBufLen = 8;
            DataBuf = new byte[_DataBufLen];
            Array.Copy(BitConverter.GetBytes(nUpdate), DataBuf, 4);
            Array.Copy(BitConverter.GetBytes(nFileLen), 0, DataBuf, 4, 4);

            _head.ActionCode = 1;
            _head.FileVersion = "1.0.0.1";
            _head.PacketDataLength = _DataBufLen;
            _head.PacketCheck = 0;
        }
        public int GetPacketLen()
        {
            return PacketHead.GetPacketHeaderSize() + _DataBufLen;
        }
        public void PacketFromBuf(byte[] buf, int nOffset, int nSize)
        {
            _head.HeadFromBuf(buf, nOffset, PacketHead.GetPacketHeaderSize());
            _DataBufLen = nSize - PacketHead.GetPacketHeaderSize();
            DataBuf = new byte[_DataBufLen];
            Array.Copy(buf, nOffset + PacketHead.GetPacketHeaderSize(), DataBuf, 0, _DataBufLen);
        }
        public int PacketToBuf(byte[] buf, int nOffset)
        {
            //_head.HeadFromBuf(buf, nOffset, FileReqHead.GetPacketHeaderSize());
            //_DataBufLen = nSize - FileReqHead.GetPacketHeaderSize();
            //DataBuf = new byte[_DataBufLen];
            Array.Copy(DataBuf, 0, buf, nOffset + PacketHead.GetPacketHeaderSize(), _DataBufLen);
            _head.HeadToBuf(buf, nOffset);
            return PacketHead.GetPacketHeaderSize() + _DataBufLen;
        }
        public string DataToString()
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetString(_DataBuf);
        }
        public bool IsValid()
        {
            if (_head.PacketDataLength != _DataBufLen)
            {
                return false;
            }

            if (_head.FileVersion.CompareTo("1.0.0.1") != 0)    // 当前版本
            {
                return false;
            }

            if (_head.ActionCode != 1)  // 版本比较
            {
                return false;
            }

            return true;
        }
        public PacketHead Head { get => _head; set => _head = value; }
        public byte[] DataBuf { get => _DataBuf; set => _DataBuf = value; }
        public int DataBufLen { get => _DataBufLen; set => _DataBufLen = value; }

        private PacketHead _head = new PacketHead();
        private byte[] _DataBuf;
        private int _DataBufLen;
    }
}
