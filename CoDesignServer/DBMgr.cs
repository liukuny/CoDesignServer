using LK.LKCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoDesignServer
{
    // 数据库连接类
    class DBMgr: CSingleton<DBMgr>, IDisposable
    {
        public string ConnString { get { return System.Configuration.ConfigurationManager.ConnectionStrings["DB"].ConnectionString; } }
        private SqlConnection _SqlDrConn = null;//连接对象

        // 执行查询语句
        public LocationResult Query(string strSql, string strSplit = "\x1")
        {
            LocationResult rt = new LocationResult();
            using (SqlConnection con = new SqlConnection(ConnString))
            {
                try
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = con;
                    command.CommandText = strSql;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        try
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                rt.ColNames += reader.GetName(i);
                                if(i < reader.FieldCount - 1)
                                {
                                    rt.ColNames += strSplit;
                                }
                            }
                            string s = "";
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (reader[i] != null)
                                    {
                                        s += reader[i].ToString();
                                    }
                                    s += strSplit;
                                }
                                //rt.ltData.Add(s);
                            }
                            if (s.Length > 0)
                            {
                                s.TrimEnd(strSplit.ToArray());
                                rt.btData = Packet.StringToBuf(s);
                            }
                        }
                        finally
                        {
                            // Always call Close when done reading.
                            reader.Close();
                        }
                    }
                }
                catch (Exception e) { }
            }
            return rt;
        }
        // 执行插入或更新语句,返回受影响的行数
        public int QueryNoRet(string strSql)
        {
            int nRowCount = 0;
            using (SqlConnection con = new SqlConnection(ConnString))
            {
                SqlCommand command = new SqlCommand(strSql, con);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                    try
                    {
                        nRowCount = command.ExecuteNonQuery();
                    }
                    catch(Exception e)
                    {
                        // Always call Close when done reading.
                    }
                }
            }
            return nRowCount;
        }
        private DBMgr()
        {
            _SqlDrConn = new SqlConnection(ConnString);
            _SqlDrConn.Open();
        }

        public void Dispose()
        {
            if (_SqlDrConn != null) _SqlDrConn.Dispose();
        }
    }
}
