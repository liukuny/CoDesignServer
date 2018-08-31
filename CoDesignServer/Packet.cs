using System;
using System.Collections.Generic;
using System.Text;

namespace CoDesignServer
{
    // 报文类
    public class Packet: PacketHead
    {
        public byte[] DataBuf { get; set; }
        public int DataBufLen { get; set; }
        public int GetPacketLen()
        {
            return PacketHead.GetPacketHeaderSize() + DataBufLen;
        }
        public void PacketFromBuf(byte[] buf, int nOffset, int nSize)
        {
            HeadFromBuf(buf, nOffset, PacketHead.GetPacketHeaderSize());
            DataBufLen = nSize - PacketHead.GetPacketHeaderSize();
            DataBuf = new byte[DataBufLen];
            Array.Copy(buf, nOffset + PacketHead.GetPacketHeaderSize(), DataBuf, 0, DataBufLen);
        }
        public int PacketToBuf(byte[] buf, int nOffset)
        {
            Array.Copy(DataBuf, 0, buf, nOffset + PacketHead.GetPacketHeaderSize(), DataBufLen);
            HeadToBuf(buf, nOffset);
            return PacketHead.GetPacketHeaderSize() + DataBufLen;
        }
        public string DataToString()
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetString(DataBuf);
        }
        public static byte[] StringToBuf(string s)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetBytes(s);
        }
        public bool IsValid()
        {
            if (PacketDataLength != DataBufLen)
            {
                return false;
            }

            if (FileVersion.CompareTo("1.0.0.1") != 0)    // 当前版本
            {
                return false;
            }

            if (ActionCode < 1 || ActionCode > ACTIONCODE_MAXVALUE) 
            {
                return false;
            }

            return true;
        }
    }

    // 升级报文
    public class PacketUpdate : Packet
    {
        // 创建比较版本的应答报文
        public void CreateCVAckPacket(int nUpdate, int nFileLen)
        {
            // int 应答版本比较结果（1 - 需要更新； 0 - 不需要更新）
            DataBufLen = 8;
            DataBuf = new byte[DataBufLen];
            Array.Copy(BitConverter.GetBytes(nUpdate), DataBuf, 4);
            Array.Copy(BitConverter.GetBytes(nFileLen), 0, DataBuf, 4, 4);

            Init(DataBufLen, ACTIONCODE_UPDATE);
        }
    }

    // SQL查询报文
    public class PacketQuery : Packet
    {
        public string strSql { get { return DataToString(); } }
    }
    // SQL查询应答报文
    public class PacketQueryAck : Packet
    {
        // 创建SQL查询的应答报文
        public void CreatePQAckPacket(int nColCount, int nRowCount, string colNames, string strSplit = "\x1")
        {
            // 列个数，行个数，分割符长度，分割符，所有列名
            byte[] buf = StringToBuf(colNames);
            byte[] buf2 = StringToBuf(strSplit);
            DataBufLen = 12 + buf.Length + buf2.Length;
            int nSplit = buf2.Length;

            DataBuf = new byte[DataBufLen];
            Array.Copy(BitConverter.GetBytes(nColCount), DataBuf, 4);
            Array.Copy(BitConverter.GetBytes(nRowCount), 0, DataBuf, 4, 4);
            Array.Copy(BitConverter.GetBytes(nSplit), 0, DataBuf, 8, 4);
            Array.Copy(buf2, 0, DataBuf, 12, nSplit);
            Array.Copy(buf, 0, DataBuf, 12 + nSplit, buf.Length);

            Init(DataBufLen, ACTIONCODE_LOCATION);
        }
    }

    // SQL插入更新报文
    public class PacketInsertORUpdate : Packet
    {
        public string strSql { get { return DataToString(); } }
    }
    // SQL插入更新应答报文
    public class PacketInsertORUpdateAck : Packet
    {
        // 创建SQL查询的应答报文
        public void CreatePIUAckPacket(int nResult)
        {
            // Result
            DataBufLen = 4;
            DataBuf = new byte[DataBufLen];
            Array.Copy(BitConverter.GetBytes(nResult), DataBuf, 4);
            Init(DataBufLen, ACTIONCODE_LOCATION);
        }
    }

    // 上传文件请求报文
    public class PacketUploadFile : Packet
    {
        public void ParseToParam(UploadFilePacketProParam param)
        {
            string strReq = DataToString();
            if (strReq.Length > 0)
            {
                param.ActionCode = ActionCode;
                // 取出分割符（放在字符串的最后位置）
                char cSplit = strReq[strReq.Length - 1];
                // 第一个字段是文件名
                // 第二个字段是文件长度
                string[] arr = strReq.Split(cSplit);
                param.FileLen = Convert.ToInt32(arr[1]);
                try
                {
                    param.dt = new System.IO.FileStream(arr[0], System.IO.FileMode.Create);
                }
                catch (Exception e) { }
            }
        }
    }
    // SQL查询应答报文
    public class PacketUploadFileAck : Packet
    {
        // 创建SQL查询的应答报文
        public void CreatePUFAckPacket(int nResult)
        {
            // Result
            DataBufLen = 4;
            DataBuf = new byte[DataBufLen];
            Array.Copy(BitConverter.GetBytes(nResult), DataBuf, 4);
            Init(DataBufLen, ACTIONCODE_LOCATION);
        }
    }

}
