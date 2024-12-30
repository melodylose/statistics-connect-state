using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsConnectStateApp.Models
{
    public class StatisticsData
    {
        public List<ConnectionLog> Logs { get; set; }
        public int TotalDisconnects { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public Dictionary<string, int> DisconnectReasonCounts { get; set; }
        public Dictionary<string, double> HourlyDistribution { get; set; }
        public double StandardDeviation { get; set; }
        public int DisconnectsLastWeek { get; set; }
        public double WeekOverWeekChange { get; set; }

        public StatisticsData() {
            Logs = new List<ConnectionLog>();
            DisconnectReasonCounts = new Dictionary<string, int>();
            HourlyDistribution = new Dictionary<string, double>();
        }
    }
}
