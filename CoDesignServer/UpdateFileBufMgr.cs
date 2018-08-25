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
        public bool LoadFile(string strFileName)
        {
            bool bRet = false;
            FileStream sFile = new FileStream(strFileName, FileMode.Open);
            if (sFile.Length > 0)
            {
                m_buffer = new byte[sFile.Length];
                m_nLength = sFile.Read(m_buffer, 0, (int)sFile.Length);
                bRet = (m_nLength > 0);
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
            XmlDocument xmlDoc = new XmlDocument();//  
            if (strConfigFileName == "")
            {
                strConfigFileName = System.Environment.CurrentDirectory + "\\config.cfg";
            }
            xmlDoc.Load(strConfigFileName);
            XmlNode root = xmlDoc.SelectSingleNode("update");//查找<update>
            if (root != null)
            {
                XmlElement ud = xmlDoc["update"];
                _FileVersion = ud.GetAttribute("version");
                string fn = ud.GetAttribute("filename");
                bRet = LoadFile(fn);
            }

            ////root.get
            ////ASCIIEncoding encoding = new ASCIIEncoding();
            ////string strTmp = "李赞红";
            ////byte[] bs = Encoding.GetEncoding("GB2312").GetBytes(strTmp);
            ////bs = Encoding.Convert(Encoding.GetEncoding("GB2312"), Encoding.GetEncoding("UTF-8"), bs);
            ////string strTmp2 = Encoding.GetEncoding("UTF-8").GetString(bs);

            //XmlElement xe1 = xmlDoc.CreateElement("update");//创建一个<update>节点
            //xe1.SetAttribute("version", "1.0.0.1");//设置该节点genre属性
            //xe1.SetAttribute("filename", "d:\\1.pdf");//设置该节点ISBN属性
            ////XmlElement xesub1 = xmlDoc.CreateElement("title");
            ////xesub1.InnerText = "CS从入门到精通";//设置文本节点
            ////xe1.AppendChild(xesub1);//添加到<book>节点中
            ////XmlElement xesub2 = xmlDoc.CreateElement("author");
            ////xesub2.InnerText = "候捷";
            ////xe1.AppendChild(xesub2);
            ////XmlElement xesub3 = xmlDoc.CreateElement("price");
            ////xesub3.InnerText = "58.3";
            ////xe1.AppendChild(xesub3);

            ////root.AppendChild(xe1);//添加到<bookstore>节点中
            //xmlDoc.AppendChild(xe1);
            //xmlDoc.Save(System.Environment.CurrentDirectory + "\\xx.config");
            return bRet;
        }
        // 版本比较
        // >0 strVer1 > strVer2
        // =0 strVer1 > strVer2
        // <0 strVer1 < strVer2
        public int CompareVersion(string cstrVer1, string cstrVer2)
        {
            string strVer1 = cstrVer1;
            string strVer2 = cstrVer2;
            int nRet = 0;
            char cSplit1 = '.';
            int nIndex1 = strVer1.IndexOf(cSplit1);
            if (nIndex1 < 0)
            {
                cSplit1 = ',';
                nIndex1 = strVer1.IndexOf(cSplit1);
            }

            char cSplit2 = '.';
            int nIndex2 = strVer1.IndexOf(cSplit2);
            if (nIndex2 < 0)
            {
                cSplit2 = ',';
                nIndex2 = strVer1.IndexOf(cSplit2);
            }

            int nVer1 = 0;
            int nVer2 = 0;
            string strTemp;
            for (int i = 0; i < 4; i++)
            {
                strTemp = strVer1.Substring(0, nIndex1);
                nVer1 = Convert.ToInt32(strTemp);
                strTemp = strVer2.Substring(0, nIndex2);
                nVer2 = Convert.ToInt32(strTemp);
                if (nVer1 > nVer2)
                {
                    nRet = 1;
                    break;
                }
                else if (nVer1 < nVer2)
                {
                    nRet = -1;
                    break;
                }
                strVer1 = strVer1.Substring(nIndex1 + 1, strVer1.Length - nIndex1 - 1);
                nIndex1 = strVer1.IndexOf(cSplit1);
                strVer2 = strVer2.Substring(nIndex2 + 1, strVer2.Length - nIndex2 - 1);
                nIndex2 = strVer2.IndexOf(cSplit2);
                if (i == 2)
                {
                    nVer1 = Convert.ToInt32(strVer1);
                    nVer2 = Convert.ToInt32(strVer2);
                    if (nVer1 > nVer2)
                    {
                        nRet = 1;
                    }
                    else if (nVer1 < nVer2)
                    {
                        nRet = -1;
                    }
                    break;
                }
            }
            return nRet;

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