using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsConnectStateApp.Models
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private static DatabaseManager _instance;

        public static DatabaseManager Instance
        {
            get {
                if (_instance == null) {
                    _instance = new DatabaseManager();
                }
                return _instance;
            }
        }

        private DatabaseManager() {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStats.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase() {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ConnectionLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DisconnectTime TEXT NOT NULL,
                        ReconnectTime TEXT,
                        Reason TEXT,
                        Duration INTEGER,
                        OfflineDuration INTEGER
                    )";
                    command.ExecuteNonQuery();
                }
            }
        }

        public StatisticsData GetAdvancedStatistics(DateTime startDate, DateTime endDate) {
            var stats = new StatisticsData();
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();

                // 基本統計
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    WITH Stats AS (
                        SELECT 
                            COUNT(*) as TotalDisconnects,
                            AVG(CAST(Duration as FLOAT)) as AvgDuration,
                            MAX(Duration) as MaxDuration,
                            MIN(Duration) as MinDuration
                        FROM ConnectionLogs
                        WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)
                    ),
                    Variance AS (
                        SELECT 
                            AVG(POWER(CAST(c.Duration as FLOAT) - s.AvgDuration, 2)) as Variance
                        FROM ConnectionLogs c, Stats s
                        WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)
                    )
                    SELECT 
                        s.TotalDisconnects,
                        s.AvgDuration,
                        s.MaxDuration,
                        s.MinDuration,
                        SQRT(v.Variance) as StdDev
                    FROM Stats s, Variance v";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            stats.TotalDisconnects = reader.GetInt32(0);
                            // 從毫秒轉換為 TimeSpan
                            stats.AverageDuration = TimeSpan.FromMilliseconds(reader.IsDBNull(1) ? 0 : reader.GetDouble(1));
                            stats.MaxDuration = TimeSpan.FromMilliseconds(reader.IsDBNull(2) ? 0 : reader.GetInt64(2));
                            stats.MinDuration = TimeSpan.FromMilliseconds(reader.IsDBNull(3) ? 0 : reader.GetInt64(3));
                            // 標準差也是以毫秒為單位
                            stats.StandardDeviation = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);
                        }
                    }
                }

                // 依中斷原因統計
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    SELECT 
                        COALESCE(Reason, '未知原因') as Reason,
                        COUNT(*) as Count
                    FROM ConnectionLogs
                    WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)
                    GROUP BY Reason";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    stats.DisconnectReasonCounts = new Dictionary<string, int>();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            stats.DisconnectReasonCounts.Add(
                                reader.GetString(0),
                                reader.GetInt32(1)
                            );
                        }
                    }
                }

                // 計算每小時分布
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    SELECT 
                        strftime('%H', datetime(DisconnectTime, 'localtime')) as Hour,
                        COUNT(*) as Count
                    FROM ConnectionLogs
                    WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)
                    GROUP BY Hour
                    ORDER BY Hour";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    stats.HourlyDistribution = new Dictionary<string, double>();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            stats.HourlyDistribution.Add(
                                reader.GetString(0),
                                reader.GetInt32(1)
                            );
                        }
                    }
                }

                // 獲取連接日誌
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                        SELECT 
                            DisconnectTime,
                            ReconnectTime,
                            Reason,
                            Duration,
                            OfflineDuration
                        FROM ConnectionLogs 
                        WHERE DisconnectTime >= @startDate 
                        AND DisconnectTime < @endDate
                        ORDER BY DisconnectTime DESC";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("O"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("O"));

                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            var log = new ConnectionLog {
                                DisconnectTime = DateTime.Parse(reader["DisconnectTime"].ToString()),
                                ReconnectTime = !reader.IsDBNull(reader.GetOrdinal("ReconnectTime")) 
                                    ? DateTime.Parse(reader["ReconnectTime"].ToString())
                                    : (DateTime?)null,
                                Reason = reader["Reason"].ToString(),
                                Duration = TimeSpan.FromMilliseconds(Convert.ToInt64(reader["Duration"])),
                                OfflineDuration = !reader.IsDBNull(reader.GetOrdinal("OfflineDuration"))
                                    ? TimeSpan.FromMilliseconds(Convert.ToInt64(reader["OfflineDuration"]))
                                    : (TimeSpan?)null
                            };
                            stats.Logs.Add(log);
                        }
                    }
                }

                // 計算週環比
                var lastWeekStart = startDate.AddDays(-7);
                var lastWeekEnd = endDate.AddDays(-7);
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM ConnectionLogs 
                    WHERE datetime(DisconnectTime) BETWEEN datetime(@lastWeekStart) AND datetime(@lastWeekEnd)";

                    command.Parameters.AddWithValue("@lastWeekStart", lastWeekStart.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@lastWeekEnd", lastWeekEnd.ToString("yyyy-MM-dd HH:mm:ss"));

                    stats.DisconnectsLastWeek = Convert.ToInt32(command.ExecuteScalar() ?? 0);

                    if (stats.DisconnectsLastWeek > 0) {
                        stats.WeekOverWeekChange =
                            ((double)stats.TotalDisconnects - stats.DisconnectsLastWeek)
                            / stats.DisconnectsLastWeek * 100;
                    }
                }
            }
            return stats;
        }

        public List<ConnectionLog> GetConnectionLogs(DateTime startDate, DateTime endDate) {
            var logs = new List<ConnectionLog>();
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                    SELECT Id, DisconnectTime, Reason, Duration
                    FROM ConnectionLogs 
                    WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)
                    ORDER BY DisconnectTime DESC";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            logs.Add(new ConnectionLog
                            {
                                Id = reader.GetInt32(0),
                                DisconnectTime = DateTime.Parse(reader.GetString(1)),
                                Reason = reader.IsDBNull(2) ? "未知原因" : reader.GetString(2),
                                Duration = TimeSpan.FromSeconds(reader.IsDBNull(3) ? 0 : reader.GetInt32(3))
                            });
                        }
                    }
                }
            }
            return logs;
        }

        public void LogDisconnection(DateTime disconnectTime, string reason, TimeSpan duration) {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = @"
                        INSERT INTO ConnectionLogs (DisconnectTime, Reason, Duration)
                        VALUES (@disconnectTime, @reason, @duration)";
                    command.Parameters.AddWithValue("@disconnectTime", disconnectTime.ToString("O"));
                    command.Parameters.AddWithValue("@reason", reason);
                    command.Parameters.AddWithValue("@duration", (long)duration.TotalMilliseconds);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateReconnection(DateTime reconnectTime) {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    // 檢查是否有斷線記錄
                    var lastDisconnectTime = GetLastDisconnectTime();
                    if (string.IsNullOrEmpty(lastDisconnectTime)) {
                        // 如果沒有斷線記錄，則不需要更新
                        return;
                    }

                    // 獲取最後一條斷線記錄並更新重連時間和離線時間
                    command.CommandText = @"
                        UPDATE ConnectionLogs 
                        SET ReconnectTime = @reconnectTime,
                            OfflineDuration = @offlineDuration
                        WHERE Id = (SELECT MAX(Id) FROM ConnectionLogs)";
                    command.Parameters.AddWithValue("@reconnectTime", reconnectTime.ToString("O"));
                    command.Parameters.AddWithValue("@offlineDuration", 
                        (long)(reconnectTime - DateTime.Parse(lastDisconnectTime)).TotalMilliseconds);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string GetLastDisconnectTime() {
            using (var connection = new SQLiteConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT DisconnectTime FROM ConnectionLogs ORDER BY Id DESC LIMIT 1";
                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }
    }
}
