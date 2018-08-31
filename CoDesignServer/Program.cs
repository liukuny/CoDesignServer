using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;   // IPEndPoint

namespace CoDesignServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = System.Configuration.ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            string s2 = System.Configuration.ConfigurationManager.AppSettings["UploadPath"];

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 7070);
            WorkServer svr = new WorkServer(100, 2048);
            svr.Init();
            svr.Start(iep);
            Console.ReadKey();
        }
    }
}
