using StatisticsConnectStateApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StatisticsConnectStateApp
{
    public partial class StatisticsForm : Form
    {
        private TabControl tabControl;
        private DateTimePicker dateTimePickerStart;
        private DateTimePicker dateTimePickerEnd;
        private Label labelDateRange;
        private DataGridView dataGridView;
        private Chart chartHourly;
        private Chart chartReasons;
        private Label labelTotalDisconnects;
        private Label labelAvgDuration;
        private Label labelMaxDuration;
        private Label labelMinDuration;
        private Label labelStdDev;
        private Label labelWeekChange;

        public StatisticsForm() {
            InitializeComponent();
            this.Text = "連線中斷統計";
            this.Size = new Size(800, 600);
            this.Load += StatisticsForm_Load;
        }

        private void InitializeComponent() {
            this.SuspendLayout();

            // 創建主容器 TableLayoutPanel
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // 日期選擇區域高度
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // TabControl 區域

            // 初始化日期選擇區域
            var datePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50
            };

            labelDateRange = new Label
            {
                Text = "日期範圍:",
                AutoSize = true,
                Location = new Point(10, 15)
            };

            dateTimePickerStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(100, 12),
                Width = 120
            };

            dateTimePickerEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(240, 12),
                Width = 120
            };

            datePanel.Controls.AddRange(new Control[] {
            labelDateRange,
            dateTimePickerStart,
            dateTimePickerEnd
        });

            // 初始化 TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // 創建圖表頁面
            var chartsTab = CreateChartsTab();
            var detailsTab = CreateDetailsTab();

            // 添加頁面到 TabControl
            tabControl.TabPages.AddRange(new TabPage[] {
                detailsTab,
                chartsTab
            });

            // 將控制項添加到主布局
            mainLayout.Controls.Add(datePanel, 0, 0);
            mainLayout.Controls.Add(tabControl, 0, 1);

            // 將主布局添加到表單
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private TabPage CreateChartsTab() {
            var chartsTab = new TabPage("統計圖表");
            var chartsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            chartsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            chartsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // 時間分布圖表
            chartHourly = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var hourlyArea = new ChartArea();
            hourlyArea.BackColor = Color.White;
            
            // 設置 X 軸
            hourlyArea.AxisX.Title = "時間 (小時)";
            hourlyArea.AxisX.Interval = 1;  // 每小時顯示一個刻度
            hourlyArea.AxisX.Minimum = 0;
            hourlyArea.AxisX.Maximum = 23;
            hourlyArea.AxisX.LabelStyle.Format = "00";  // 兩位數顯示
            hourlyArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            
            // 設置 Y 軸
            hourlyArea.AxisY.Title = "中斷次數 (次)";
            hourlyArea.AxisY.LabelStyle.Format = "N0";  // 整數顯示
            hourlyArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            
            chartHourly.ChartAreas.Add(hourlyArea);

            var hourlySeries = new Series
            {
                Name = "每小時中斷次數",
                ChartType = SeriesChartType.Column,
                ToolTip = "#VALX:00 時\n中斷次數: #VALY{N0} 次"  // 添加 tooltip
            };
            
            chartHourly.Series.Add(hourlySeries);
            chartHourly.Titles.Add(new Title("每小時中斷分布"));

            // 初始化中斷原因圓餅圖
            chartReasons = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // 設置圖表區域
            var chartArea = new ChartArea();
            chartArea.BackColor = Color.White;
            chartReasons.ChartAreas.Add(chartArea);

            // 設置圖例
            var legend = new Legend();
            legend.Docking = Docking.Right;
            legend.Alignment = StringAlignment.Center;
            legend.MaximumAutoSize = 50;
            legend.LegendStyle = LegendStyle.Column;
            legend.IsTextAutoFit = true;
            chartReasons.Legends.Add(legend);

            // 添加初始系列
            var series = new Series
            {
                ChartType = SeriesChartType.Pie,
                Name = "中斷原因分布",
                ToolTip = "#LEGENDTEXT\n次數: #VALY{N0}\n佔比: #PERCENT{P1}"  // 使用 LEGENDTEXT
            };
            series["PieLabelStyle"] = "Disabled";
            chartReasons.Series.Add(series);

            chartsPanel.Controls.Add(chartHourly, 0, 0);
            chartsPanel.Controls.Add(chartReasons, 0, 1);
            chartsTab.Controls.Add(chartsPanel);

            return chartsTab;
        }

        private TabPage CreateDetailsTab() {
            var detailsTab = new TabPage("詳細資料");

            // 創建主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));  // 概覽區域高度
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // DataGridView 區域

            // 創建概覽面板
            var summaryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(5)
            };

            // 設置列和行的樣式為百分比
            for (int i = 0; i < 3; i++)
            {
                summaryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            }
            for (int i = 0; i < 2; i++)
            {
                summaryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            }

            // 設置概覽標籤
            labelTotalDisconnects = new Label { Text = "總中斷次數: 0 次", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            labelAvgDuration = new Label { Text = "平均中斷時間: 0 秒", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            labelMaxDuration = new Label { Text = "最長中斷時間: 0 秒", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            labelMinDuration = new Label { Text = "最短中斷時間: 0 秒", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            labelStdDev = new Label { Text = "標準差: 0 秒", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
            labelWeekChange = new Label { Text = "週環比變化: 0%", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };

            // 按順序添加控件到面板的指定位置
            summaryPanel.Controls.Add(labelTotalDisconnects, 0, 0);
            summaryPanel.Controls.Add(labelAvgDuration, 1, 0);
            summaryPanel.Controls.Add(labelMaxDuration, 2, 0);
            summaryPanel.Controls.Add(labelMinDuration, 0, 1);
            summaryPanel.Controls.Add(labelStdDev, 1, 1);
            summaryPanel.Controls.Add(labelWeekChange, 2, 1);

            // 初始化 DataGridView
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            // 設置列
            dataGridView.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn
                {
                    Name = "DisconnectTime",
                    HeaderText = "斷線時間",
                    DataPropertyName = "DisconnectTime",
                    Width = 150
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "ReconnectTime",
                    HeaderText = "重連時間",
                    DataPropertyName = "ReconnectTime",
                    Width = 150
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Reason",
                    HeaderText = "斷線原因",
                    DataPropertyName = "Reason",
                    Width = 200
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "Duration",
                    HeaderText = "連線持續時間",
                    DataPropertyName = "Duration",
                    Width = 100
                },
                new DataGridViewTextBoxColumn
                {
                    Name = "OfflineDuration",
                    HeaderText = "離線持續時間",
                    DataPropertyName = "OfflineDuration",
                    Width = 100
                }
            });

            // 將控件添加到主面板
            mainPanel.Controls.Add(summaryPanel, 0, 0);
            mainPanel.Controls.Add(dataGridView, 0, 1);

            detailsTab.Controls.Add(mainPanel);
            return detailsTab;
        }

        private void StatisticsForm_Load(object sender, EventArgs e) {
            // 設定初始日期
            dateTimePickerStart.Value = DateTime.Today.AddDays(-30);
            dateTimePickerEnd.Value = DateTime.ParseExact($"{DateTime.Now:yyyy-MM-dd} 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            // 綁定事件
            dateTimePickerStart.ValueChanged += DatePicker_ValueChanged;
            dateTimePickerEnd.ValueChanged += DatePicker_ValueChanged;

            // 初始更新資料
            UpdateStatistics();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e) {
            UpdateStatistics();
        }

        private void UpdateStatistics() {
            try {
                var startDate = dateTimePickerStart.Value;
                var endDate = dateTimePickerEnd.Value;

                var stats = DatabaseManager.Instance.GetAdvancedStatistics(startDate, endDate);

                // 更新概觀標籤
                labelTotalDisconnects.Text = $"總中斷次數: {stats.TotalDisconnects} 次";
                labelAvgDuration.Text = $"平均中斷時間: {stats.AverageDuration.TotalSeconds:F1} 秒";
                labelMaxDuration.Text = $"最長中斷時間: {stats.MaxDuration.TotalSeconds:F1} 秒";
                labelMinDuration.Text = $"最短中斷時間: {stats.MinDuration.TotalSeconds:F1} 秒";
                labelStdDev.Text = $"標準差: {TimeSpan.FromMilliseconds(stats.StandardDeviation).TotalSeconds:F1} 秒";
                labelWeekChange.Text = $"週環比變化: {stats.WeekOverWeekChange:P1}";

                // 更新圖表
                UpdateCharts(stats);

                // 更新資料表格
                var formattedLogs = stats.Logs.Select(log => new
                {
                    DisconnectTime = log.DisconnectTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReconnectTime = log.ReconnectTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-",
                    Reason = log.Reason,
                    Duration = $"{log.Duration.TotalSeconds:F1} 秒",
                    OfflineDuration = log.OfflineDuration.HasValue ? $"{log.OfflineDuration.Value.TotalSeconds:F1} 秒" : "-"
                }).ToList();

                dataGridView.DataSource = formattedLogs;
            }
            catch (Exception ex) {
                MessageBox.Show($"更新統計資料時發生錯誤：{ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCharts(StatisticsData stats) {
            // 更新時間分布圖表
            chartHourly.Series.Clear();
            var hourlyDisconnectSeries = new Series("每小時中斷次數")
            {
                ChartType = SeriesChartType.Column,
                ToolTip = "#VALX:00 時\n中斷次數: #VALY{N0} 次"
            };

            // 設置每小時分布圖表的軸標題和格式
            chartHourly.ChartAreas[0].AxisX.Title = "時間 (小時)";
            chartHourly.ChartAreas[0].AxisY.Title = "中斷次數 (次)";
            chartHourly.ChartAreas[0].AxisX.Interval = 1;
            chartHourly.ChartAreas[0].AxisX.Minimum = 0;
            chartHourly.ChartAreas[0].AxisX.Maximum = 23;
            chartHourly.ChartAreas[0].AxisX.LabelStyle.Format = "00";
            chartHourly.ChartAreas[0].AxisY.LabelStyle.Format = "N0";

            // 確保顯示所有24小時
            for (int hour = 0; hour < 24; hour++)
            {
                string hourStr = hour.ToString("00");
                double count = stats.HourlyDistribution.ContainsKey(hourStr) ? stats.HourlyDistribution[hourStr] : 0;
                int pointIndex = hourlyDisconnectSeries.Points.AddXY(hour, count);
                hourlyDisconnectSeries.Points[pointIndex].AxisLabel = hourStr;  // 使用索引取得數據點物件
            }

            chartHourly.Series.Add(hourlyDisconnectSeries);

            // 更新中斷原因圓餅圖
            chartReasons.Series.Clear();
            var reasonSeries = new Series("中斷原因分布")
            {
                ChartType = SeriesChartType.Pie
            };

            // 設置圖例位置和格式
            chartReasons.Legends[0].Docking = Docking.Right;
            chartReasons.Legends[0].Alignment = StringAlignment.Center;
            chartReasons.Legends[0].MaximumAutoSize = 50; // 允許圖例佔用更多空間
            
            // 設置圓餅圖的樣式
            reasonSeries["PieLabelStyle"] = "Disabled"; // 關閉圓餅圖上的標籤
            reasonSeries.IsValueShownAsLabel = false;
            reasonSeries.ToolTip = "#LEGENDTEXT\n次數: #VALY{N0}\n佔比: #PERCENT{P1}";  // 使用 LEGENDTEXT

            foreach (var reason in stats.DisconnectReasonCounts.OrderByDescending(x => x.Value))
            {
                var point = reasonSeries.Points.Add(reason.Value);
                point.LegendText = reason.Key;  // 簡化圖例文字，只顯示原因
                point.ToolTip = $"{reason.Key}\n次數: {reason.Value:N0}\n佔比: {(double)reason.Value / stats.TotalDisconnects:P1}";  // 為每個點設置特定的 tooltip
            }

            chartReasons.Series.Add(reasonSeries);
        }
    }
}
