using NModbus;
using StatisticsConnectStateApp.Extensions;
using StatisticsConnectStateApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatisticsConnectStateApp
{
    public partial class MainForm : Form
    {
        private TcpClient _client;
        private IModbusFactory _factory;
        private int _connectionDuration = 0;

        public MainForm() {
            InitializeComponent();
        }

        private void buttonStatisticsForm_Click(object sender, EventArgs e) {
            var window = new StatisticsForm();
            window.Show();
        }

        private void buttonModbusConnect_Click(object sender, EventArgs e) {
            try {
                // 檢查按鈕當前狀態
                if (buttonModbusConnect.Text == "Modbus Connect") {
                    // 如果客戶端不存在或未連接，則創建新的TcpClient並連接
                    if (_client == null || !_client.Connected) {
                        _client = new TcpClient();
                        _client.ReceiveTimeout = 500;
                        _client.SendTimeout = 500;
                        _client.Connect("127.0.0.1", 502);

                        // 記錄重新連接時間
                        DatabaseManager.Instance.UpdateReconnection(DateTime.Now);
                    }

                    // 更新按鈕文字
                    buttonModbusConnect.Text = "Modbus Disconnect";

                    // 記錄連接開始時間
                    _connectionDuration = Environment.TickCount;
                }
                else {
                    // 如果客戶端存在且已連接，則關閉連接
                    if (_client != null && _client.Connected) {
                        _client.Close();
                        _client = null;

                        // 計算連接持續時間
                        var duration = Environment.TickCount - _connectionDuration;
                        // 記錄手動斷開連接事件
                        DatabaseManager.Instance.LogDisconnection(DateTime.Now, "手動切斷", TimeSpan.FromMilliseconds(duration));

                        // 更新按鈕文字
                        buttonModbusConnect.Text = "Modbus Connect";
                    }
                }
            }
            catch (Exception ex) {
                // 捕獲並記錄任何異常
                Debug.WriteLine(ex.ToString());
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            _factory = new ModbusFactory();

            ThreadPool.QueueUserWorkItem(OnPollingModbusWork);
        }

        private void OnPollingModbusWork(object state) {
            while (true) {
                try {
                    // 檢查客戶端是否存在且已連接
                    if (_client != null && _client.Connected) {
                        // 創建 Modbus 主設備
                        var master = _factory.CreateMaster(_client);
                        // 設置舊回應的重試閾值
                        master.Transport.RetryOnOldResponseThreshold = 10;

                        // 讀取線圈狀態
                        master.ReadCoils(1, 0, 1);
                    }
                }
                catch (IOException ex) {
                    // 計算連接持續時間
                    var duration = Environment.TickCount - _connectionDuration;
                    // 記錄斷開連接事件
                    DatabaseManager.Instance.LogDisconnection(DateTime.Now, ex.Message, TimeSpan.FromMilliseconds(duration));

                    // 更新按鈕文字
                    buttonModbusConnect.ControlSetText("Modbus Connect");
                }
                catch (Exception ex) {
                    // 記錄一般異常
                    Debug.WriteLine(ex.ToString());
                }

                // 暫停 1 秒後繼續下一次輪詢
                Thread.Sleep(1000);
            }
        }
    }
}
