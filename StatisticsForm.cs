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

            // 創建概覽頁面
            var summaryTab = CreateSummaryTab();
            var chartsTab = CreateChartsTab();
            var detailsTab = CreateDetailsTab();

            // 添加頁面到 TabControl
            tabControl.TabPages.AddRange(new TabPage[] {
            summaryTab,
            chartsTab,
            detailsTab
        });

            // 將控制項添加到主布局
            mainLayout.Controls.Add(datePanel, 0, 0);
            mainLayout.Controls.Add(tabControl, 0, 1);

            // 將主布局添加到表單
            this.Controls.Add(mainLayout);

            this.ResumeLayout(false);
        }

        private TabPage CreateSummaryTab() {
            var summaryTab = new TabPage("概覽");
            var summaryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            labelTotalDisconnects = new Label { Text = "總中斷次數: 0", Dock = DockStyle.Fill };
            labelAvgDuration = new Label { Text = "平均中斷時間: 0分鐘", Dock = DockStyle.Fill };
            labelMaxDuration = new Label { Text = "最長中斷時間: 0分鐘", Dock = DockStyle.Fill };
            labelMinDuration = new Label { Text = "最短中斷時間: 0分鐘", Dock = DockStyle.Fill };
            labelStdDev = new Label { Text = "標準差: 0秒", Dock = DockStyle.Fill };
            labelWeekChange = new Label { Text = "週環比: 0%", Dock = DockStyle.Fill };

            summaryPanel.Controls.AddRange(new Control[] {
            labelTotalDisconnects,
            labelAvgDuration,
            labelMaxDuration,
            labelMinDuration,
            labelStdDev,
            labelWeekChange
        });

            summaryTab.Controls.Add(summaryPanel);
            return summaryTab;
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
                Dock = DockStyle.Fill
            };
            chartHourly.ChartAreas.Add(new ChartArea("HourlyArea"));
            chartHourly.Series.Add(new Series
            {
                Name = "每小時分布",
                ChartType = SeriesChartType.Column
            });
            chartHourly.Titles.Add(new Title("每小時中斷分布"));

            // 中斷原因圖表
            chartReasons = new Chart
            {
                Dock = DockStyle.Fill
            };
            chartReasons.ChartAreas.Add(new ChartArea("ReasonsArea"));
            chartReasons.Series.Add(new Series
            {
                Name = "中斷原因",
                ChartType = SeriesChartType.Pie
            });
            chartReasons.Titles.Add(new Title("中斷原因分布"));

            chartsPanel.Controls.Add(chartHourly, 0, 0);
            chartsPanel.Controls.Add(chartReasons, 0, 1);
            chartsTab.Controls.Add(chartsPanel);

            return chartsTab;
        }

        private TabPage CreateDetailsTab() {
            var detailsTab = new TabPage("詳細資料");

            // 初始化 DataGridView
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // 設定 DataGridView 的欄位
            dataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
            new DataGridViewTextBoxColumn
            {
                Name = "DisconnectTime",
                HeaderText = "中斷時間",
                DataPropertyName = "DisconnectTime",
                Width = 150
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Reason",
                HeaderText = "中斷原因",
                DataPropertyName = "Reason",
                Width = 200
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Duration",
                HeaderText = "持續時間(分鐘)",
                DataPropertyName = "Duration",
                Width = 100
            }
            });

            detailsTab.Controls.Add(dataGridView);
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

                // 更新概覽資訊
                labelTotalDisconnects.Text = $"總中斷次數: {stats.TotalDisconnects}";
                labelAvgDuration.Text = $"平均中斷時間: {stats.AverageDuration.TotalMinutes:F2} 分鐘";
                labelMaxDuration.Text = $"最長中斷時間: {stats.MaxDuration.TotalMinutes:F2} 分鐘";
                labelMinDuration.Text = $"最短中斷時間: {stats.MinDuration.TotalMinutes:F2} 分鐘";
                labelStdDev.Text = $"標準差: {stats.StandardDeviation:F2} 秒";
                labelWeekChange.Text = $"週環比: {stats.WeekOverWeekChange:F2}%";

                // 更新圖表
                UpdateCharts(stats);

                // 更新資料表格
                var logs = DatabaseManager.Instance.GetConnectionLogs(startDate, endDate);
                dataGridView.DataSource = logs;
            }
            catch (Exception ex) {
                MessageBox.Show($"更新統計資料時發生錯誤：{ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCharts(StatisticsData stats) {
            // 更新時間分布圖表
            chartHourly.Series[0].Points.Clear();
            foreach (var hourData in stats.HourlyDistribution.OrderBy(x => x.Key)) {
                chartHourly.Series[0].Points.AddXY(
                    $"{hourData.Key}時",
                    hourData.Value
                );
            }

            // 更新中斷原因圖表
            chartReasons.Series[0].Points.Clear();
            foreach (var reasonData in stats.DisconnectReasonCounts) {
                chartReasons.Series[0].Points.AddXY(
                    reasonData.Key,
                    reasonData.Value
                );
            }
        }
    }
}
