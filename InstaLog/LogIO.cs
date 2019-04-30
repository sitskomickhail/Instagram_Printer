using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaLog
{
    public static class LogIO
    {
        public static string mainLog = "Log.log";
        public static string easyPath = "EasyLog.log";
        public delegate void Logging(string text, Log log);
        public static object[] locker = new object[1];
        public static void WriteLog(string path, Log log)
        {
            try
            {
                lock (locker) File.AppendAllText(path, log.ToString() + "\n");
            }
            catch { }
        }
    }
}
