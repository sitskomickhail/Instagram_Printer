using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaLog
{
    public class Log
    {
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public string Method { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Date} {UserName}: {Message} - {Method} ";
        }
    }
}
