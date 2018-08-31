using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace CoDesignServer
{
    // 报文处理参数类
    public class PacketProParam
    {
        // 同报文头的 ActionCode
        public int ActionCode { set; get; }
    }
    // 升级报文处理参数类
    public class UpdatePacketProParam : PacketProParam
    {
        // 下载偏移
        public int DownOffsert { get => m_nDownOffsert; set => m_nDownOffsert = value; }
        // 发送数据类型(0 - 发送头部； 1 - 发送数据)
        public int SendType { get => _SendType; set => _SendType = value; }
        // 下载偏移
        private int m_nDownOffsert = 0;
        // 发送数据类型(0 - 发送头部； 1 - 发送数据)
        private int _SendType = 0;
    }
    // SQL查询报文处理参数类
    public class QueryPacketProParam : PacketProParam
    {
        // 下载偏移
        public int DownOffsert = 0;
        // 发送数据类型(0 - 发送头部； 1 - 发送数据)
        public int SendType = 0;
        // 数据源
        public LocationResult dt = null;
        public int GetBuf(byte[] buf, int ndOffset = 0, int nsOffset = 0, int nSize = 1024)
        {
            if (null == dt)
            {
                return 0;
            }
            int nLength = nSize;
            if (dt.btData.Length - nsOffset < nLength)
            {
                nLength = dt.btData.Length - nsOffset;
            }
            Array.Copy(dt.btData, nsOffset, buf, ndOffset, nLength);
            return nLength;
        }
    }

    // 上传文件报文处理参数类
    public class UploadFilePacketProParam : PacketProParam
    {
        // 文件长度
        public int FileLen = 0;
        // 上传偏移
        public int UploadOffsert = 0;
        // 发送数据类型(0 - 发送头部； 1 - 发送数据)
        public int SendType = 0;
        // 数据源
        public FileStream dt = null;
    }

    // 查询数据库结果类
    public class LocationResult
    {
        // 列数
        public int ColCount = 0;
        // 行数
        public int RowCount = 0;
        // 列名列表
        //public List<string> ltColName;
        // 列名,以分割符分割
        public string ColNames = "列名";
        // 数据
        public byte[] btData = null;
        public LocationResult()
        {
            //ltData = new List<string>();
        }
    }

}