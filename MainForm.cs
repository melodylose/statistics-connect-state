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
                if (buttonModbusConnect.Text == "Modbus Connect") {
                    if (_client == null || !_client.Connected) {
                        _client = new TcpClient();
                        _client.ReceiveTimeout = 500;
                        _client.SendTimeout = 500;
                        _client.Connect("127.0.0.1", 502);
                    }

                    buttonModbusConnect.Text = "Modbus Disconnect";

                    _connectionDuration = Environment.TickCount;
                }
                else {
                    if (_client != null && _client.Connected) {
                        _client.Close();
                        _client = null;

                        var duration = Environment.TickCount - _connectionDuration;
                        DatabaseManager.Instance.LogDisconnection(DateTime.Now, "手動切斷", TimeSpan.FromMilliseconds(duration));

                        buttonModbusConnect.Text = "Modbus Connect";
                    }
                }
            }
            catch (Exception ex) {
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
                    if (_client != null && _client.Connected) {

                        var master = _factory.CreateMaster(_client);
                        master.Transport.RetryOnOldResponseThreshold = 10;

                        master.ReadCoils(1, 0, 1);
                    }
                }
                catch (IOException ex) {
                    var duration = Environment.TickCount - _connectionDuration;
                    DatabaseManager.Instance.LogDisconnection(DateTime.Now, ex.Message, TimeSpan.FromMilliseconds(duration));

                    buttonModbusConnect.ControlSetText("Modbus Connect");
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.ToString());
                }

                Thread.Sleep(1000);
            }
        }
    }
}
