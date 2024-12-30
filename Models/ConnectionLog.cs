using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsConnectStateApp.Models
{
    /// <summary>
    /// 表示連接斷開事件的日誌條目。
    /// </summary>
    public class ConnectionLog
    {
        /// <summary>
        /// 獲取或設置日誌條目的唯一標識符。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 獲取或設置斷開連接發生的時間。
        /// </summary>
        public DateTime DisconnectTime { get; set; }

        /// <summary>
        /// 獲取或設置斷開連接的原因。
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 獲取或設置斷開連接前的連接持續時間。
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
