using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsConnectStateApp.Models
{
    public class ConnectionLog
    {
        public int Id { get; set; }
        public DateTime DisconnectTime { get; set; }
        public string Reason { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
