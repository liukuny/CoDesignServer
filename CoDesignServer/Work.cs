using LK.LKCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoDesignServer
{
    class Work
    {
        public delegate void OnRecvDelegate(SocketAsyncEventArgs e, Packet fp, ref bool IsSend);
        public static OnRecvDelegate[] OnRecvFun = new OnRecvDelegate[] { null, RecvUpdate, RecvLocation, RecvInsertAndUpdate, RecvUploadFile };
        // 已收到请求包
        public static bool OnRecvEvent(SocketAsyncEventArgs e, ref bool IsSend)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (token.bFirstProc)
            {
                // 接受连接后收到的第一个报文
                token.bFirstProc = false;
                Packet fp = new Packet();
                fp.PacketFromBuf(e.Buffer, e.Offset, e.BytesTransferred);
                if (!fp.IsValid())
                {
                    // 校验失败
                    return false;
                }
                //
                if (OnRecvFun[fp.ActionCode] != null)
                {
                    OnRecvFun[fp.ActionCode](e, fp, ref IsSend);
                }
                //e.SetBuffer(e.Offset, fp.GetPacketLen());
                return true;
            }
            else
            {
                if (OnRecvFun[token.Param.ActionCode] != null)
                {
                    OnRecvFun[token.Param.ActionCode](e, null, ref IsSend);
                    return true;
                }
                // 接受连接后收到的其他报文
            }
            return false;
        }
        // 异步已发送应答包
        public static bool OnSendEvent(SocketAsyncEventArgs e)
        {
            // SendWork
            if (e.SocketError == SocketError.Success)
            {
                PacketProParam param = ((AsyncUserToken)e.UserToken).Param;
                if(param != null)
                {
                    if (param.ActionCode == PacketHead.ACTIONCODE_UPDATE)
                    {
                        // 异步已发送升级应答包
                        return SendUpdate(e);
                    } else if (param.ActionCode == PacketHead.ACTIONCODE_LOCATION)
                    {
                        // 异步已发送SQL查询数据
                        return SendSqlQuery(e);
                    }
                }
            }
            return false;
        }
        // 收到升级请求
        public static void RecvUpdate(SocketAsyncEventArgs e, Packet fp, ref bool IsSend)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            UpdatePacketProParam param = new UpdatePacketProParam();
            token.Param = param;
            param.ActionCode = fp.ActionCode;
            string fv = fp.DataToString();
            int nUpdate = 0;
            // 比较版本
            if (Function.CompareVersion(UpdateFileBufMgr.Instance.FileVersion, fv) > 0)
            {
                // 可以升级
                nUpdate = 1;
            }
            else
            {
                // 不需要升级
            }
            ((PacketUpdate)fp).CreateCVAckPacket(nUpdate, UpdateFileBufMgr.Instance.Length);
            fp.PacketToBuf(e.Buffer, e.Offset);
            param.SendType = 0;
            e.SetBuffer(e.Offset, fp.GetPacketLen());
        }
        // 处理查询请求
        public static void RecvLocation(SocketAsyncEventArgs e, Packet fp, ref bool IsSend)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            QueryPacketProParam param = new QueryPacketProParam();
            token.Param = param;
            param.ActionCode = fp.ActionCode;
            param.dt = DBMgr.Instance.Query(((PacketQuery)fp).strSql);

            PacketQueryAck ack = new PacketQueryAck();
            ack.CreatePQAckPacket(param.dt.ColCount, param.dt.RowCount, param.dt.ColNames);
            fp.PacketToBuf(e.Buffer, e.Offset);
            param.SendType = 0;
            e.SetBuffer(e.Offset, ack.GetPacketLen());
        }
        // 处理插入和更新请求
        public static void RecvInsertAndUpdate(SocketAsyncEventArgs e, Packet fp, ref bool IsSend)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            QueryPacketProParam param = new QueryPacketProParam();
            token.Param = param;
            param.ActionCode = fp.ActionCode;
            int nResult = DBMgr.Instance.QueryNoRet(((PacketInsertORUpdate)fp).strSql);

            PacketInsertORUpdateAck ack = new PacketInsertORUpdateAck();
            ack.CreatePIUAckPacket(nResult);
            fp.PacketToBuf(e.Buffer, e.Offset);
            e.SetBuffer(e.Offset, ack.GetPacketLen());
        }
        // 处理上传文件请求
        public static void RecvUploadFile(SocketAsyncEventArgs e, Packet fp, ref bool IsSend)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if(null == token.Param)
            {
                UploadFilePacketProParam param = new UploadFilePacketProParam();
                token.Param = param;
                ((PacketUploadFile)fp).ParseToParam(param);
                IsSend = false;
            }
            else
            {
                UploadFilePacketProParam param = (UploadFilePacketProParam)token.Param;
                param.dt.Write(e.Buffer, e.Offset, e.BytesTransferred);
                if (param.dt.Length >= param.FileLen)
                {
                    param.dt.Close();
                    param.dt = null;
                    int nResult = 1;
                    PacketInsertORUpdateAck ack = new PacketInsertORUpdateAck();
                    ack.CreatePIUAckPacket(nResult);
                    fp.PacketToBuf(e.Buffer, e.Offset);
                    e.SetBuffer(e.Offset, ack.GetPacketLen());
                    //IsSend = true;
                }
                else {
                    IsSend = false;
                }
            }
        }
        // 异步已发送升级应答包
        public static bool SendUpdate(SocketAsyncEventArgs e)
        {
            UpdatePacketProParam param = (UpdatePacketProParam)((AsyncUserToken)e.UserToken).Param;
            if (param.SendType == 1)
            {
                param.DownOffsert += e.BytesTransferred;
            }
            if (UpdateFileBufMgr.Instance.Length <= param.DownOffsert)
            {
                // 发送完成
                param.DownOffsert = 0;
            }
            else
            {
                param.SendType = 1;
                int nLen = UpdateFileBufMgr.Instance.GetBuf(e.Buffer, e.Offset, param.DownOffsert);
                e.SetBuffer(e.Offset, nLen);
                return true;
            }
            return false;
        }
        // 异步已发送SQL查询数据
        public static bool SendSqlQuery(SocketAsyncEventArgs e)
        {
            QueryPacketProParam param = (QueryPacketProParam)((AsyncUserToken)e.UserToken).Param;
            if (param.SendType == 1)
            {
                param.DownOffsert += e.BytesTransferred;
            }
            if (param.dt.btData.Length <= param.DownOffsert)
            {
                // 发送完成
                param.DownOffsert = 0;
                param.dt = null;
            }
            else
            {
                param.SendType = 1;
                int nLen = param.GetBuf(e.Buffer, e.Offset, param.DownOffsert);
                if (nLen > 0)
                {
                    e.SetBuffer(e.Offset, nLen);
                    return true;
                }
            }
            return false;
        }
    }
}
