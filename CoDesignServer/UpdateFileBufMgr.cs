using System;
using System.Collections.Generic;
//using System.Text;


//using System.Threading;
//using System.Net;
//using System.Net.Sockets;
using System.IO;
using LK.LKCommon;
using System.Xml;

namespace CoDesignServer
{
    // 升级文件管理
    // 一次性加载升级包
    class UpdateFileBufMgr : CSingleton<UpdateFileBufMgr>
    {
        // 是否升级
        private bool IsUpdate = true;
        public bool LoadFile(string strFileName)
        {
            bool bRet = false;
            try
            {
                FileStream sFile = new FileStream(strFileName, FileMode.Open);
                if (sFile.Length > 0)
                {
                    m_buffer = new byte[sFile.Length];
                    m_nLength = sFile.Read(m_buffer, 0, (int)sFile.Length);
                    bRet = (m_nLength > 0);
                }                
            }
            catch (FileNotFoundException e)
            {
                // 文件不存在
            }
            catch (DirectoryNotFoundException e)
            {
                // 目录不存在
            }
            return bRet;
        }
        public int GetBuf(byte[] buf, int ndOffset = 0, int nsOffset = 0, int nSize = 1024)
        {
            int nLength = nSize;
            if (m_nLength - nsOffset < nLength)
            {
                nLength = m_nLength - nsOffset;
            }
            Array.Copy(m_buffer, nsOffset, buf, ndOffset, nLength);
            return nLength;
        }
        public bool LoadUpdateVer(string strConfigFileName = "")
        {
            bool bRet = false;
            try
            {
                _FileVersion = System.Configuration.ConfigurationManager.AppSettings["version"];
                string fn = System.Configuration.ConfigurationManager.AppSettings["updatefilename"];
                bRet = LoadFile(fn);
            }
            catch (Exception e)
            {
                // 
            }
            IsUpdate = bRet;
            return bRet;
        }
        public int Length { get => m_nLength; set => m_nLength = value; }
        // 客户端升级版本
        public string FileVersion { get => _FileVersion; set => _FileVersion = value; }
        // 文件缓存
        private byte[] m_buffer;
        // 文件缓存大小（文件大小）
        private int m_nLength = 0;
        // 客户端升级版本
        private string _FileVersion;
        private UpdateFileBufMgr() { }
    }
}