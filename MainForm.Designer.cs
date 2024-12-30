namespace StatisticsConnectStateApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.buttonStatisticsForm = new System.Windows.Forms.Button();
            this.buttonModbusConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonStatisticsForm
            // 
            this.buttonStatisticsForm.Location = new System.Drawing.Point(288, 190);
            this.buttonStatisticsForm.Name = "buttonStatisticsForm";
            this.buttonStatisticsForm.Size = new System.Drawing.Size(225, 45);
            this.buttonStatisticsForm.TabIndex = 0;
            this.buttonStatisticsForm.Text = "統計";
            this.buttonStatisticsForm.UseVisualStyleBackColor = true;
            this.buttonStatisticsForm.Click += new System.EventHandler(this.buttonStatisticsForm_Click);
            // 
            // buttonModbusConnect
            // 
            this.buttonModbusConnect.AutoSize = true;
            this.buttonModbusConnect.Location = new System.Drawing.Point(288, 99);
            this.buttonModbusConnect.Name = "buttonModbusConnect";
            this.buttonModbusConnect.Size = new System.Drawing.Size(225, 45);
            this.buttonModbusConnect.TabIndex = 1;
            this.buttonModbusConnect.Text = "Modbus Connect";
            this.buttonModbusConnect.UseVisualStyleBackColor = true;
            this.buttonModbusConnect.Click += new System.EventHandler(this.buttonModbusConnect_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonModbusConnect);
            this.Controls.Add(this.buttonStatisticsForm);
            this.Name = "MainForm";
            this.Text = "連線狀態統計";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStatisticsForm;
        private System.Windows.Forms.Button buttonModbusConnect;
    }
}

