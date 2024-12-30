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
                        Reason TEXT,
                        Duration INTEGER
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
                    SELECT 
                        COUNT(*) as TotalDisconnects,
                        AVG(CAST(Duration as FLOAT)) as AvgDuration,
                        MAX(Duration) as MaxDuration,
                        MIN(Duration) as MinDuration
                    FROM ConnectionLogs
                    WHERE datetime(DisconnectTime) BETWEEN datetime(@startDate) AND datetime(@endDate)";

                    command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    using (var reader = command.ExecuteReader()) {
                        if (reader.Read()) {
                            stats.TotalDisconnects = reader.GetInt32(0);
                            stats.AverageDuration = TimeSpan.FromSeconds(reader.IsDBNull(1) ? 0 : reader.GetDouble(1));
                            stats.MaxDuration = TimeSpan.FromSeconds(reader.IsDBNull(2) ? 0 : reader.GetInt32(2));
                            stats.MinDuration = TimeSpan.FromSeconds(reader.IsDBNull(3) ? 0 : reader.GetInt32(3));
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
                        strftime('%H', DisconnectTime) as Hour,
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

                    command.Parameters.AddWithValue("@disconnectTime", disconnectTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@reason", reason ?? "未知原因");
                    command.Parameters.AddWithValue("@duration", (int)duration.TotalSeconds);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
