namespace CaOrdersServer
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnOrder = new System.Windows.Forms.Button();
            this.btnListen = new System.Windows.Forms.Button();
            this.timer_15min = new System.Windows.Forms.Timer(this.components);
            this.btnKeys = new System.Windows.Forms.Button();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.tabCaller = new DevExpress.XtraTab.XtraTabPage();
            this.tabSocket = new DevExpress.XtraTab.XtraTabPage();
            this.txtLog1 = new System.Windows.Forms.TextBox();
            this.tabOrders = new DevExpress.XtraTab.XtraTabPage();
            this.txtLog2 = new System.Windows.Forms.TextBox();
            this.btnKuco = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.tabCaller.SuspendLayout();
            this.tabSocket.SuspendLayout();
            this.tabOrders.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(682, 493);
            this.txtLog.TabIndex = 0;
            // 
            // btnOrder
            // 
            this.btnOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOrder.Location = new System.Drawing.Point(728, 99);
            this.btnOrder.Name = "btnOrder";
            this.btnOrder.Size = new System.Drawing.Size(117, 39);
            this.btnOrder.TabIndex = 1;
            this.btnOrder.Text = "Загрузить ордера";
            this.btnOrder.UseVisualStyleBackColor = true;
            this.btnOrder.Click += new System.EventHandler(this.btnOrder_Click);
            // 
            // btnListen
            // 
            this.btnListen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnListen.Location = new System.Drawing.Point(728, 161);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(117, 39);
            this.btnListen.TabIndex = 1;
            this.btnListen.Text = "Начать слушать ордера";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // timer_15min
            // 
            this.timer_15min.Interval = 900000;
            this.timer_15min.Tick += new System.EventHandler(this.timer_15min_Tick);
            // 
            // btnKeys
            // 
            this.btnKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKeys.Location = new System.Drawing.Point(728, 36);
            this.btnKeys.Name = "btnKeys";
            this.btnKeys.Size = new System.Drawing.Size(117, 39);
            this.btnKeys.TabIndex = 2;
            this.btnKeys.Text = "Проверить ключи";
            this.btnKeys.UseVisualStyleBackColor = true;
            this.btnKeys.Click += new System.EventHandler(this.btnKeys_Click);
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtraTabControl1.Location = new System.Drawing.Point(24, 12);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.tabCaller;
            this.xtraTabControl1.Size = new System.Drawing.Size(684, 518);
            this.xtraTabControl1.TabIndex = 3;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabCaller,
            this.tabSocket,
            this.tabOrders});
            // 
            // tabCaller
            // 
            this.tabCaller.Controls.Add(this.txtLog);
            this.tabCaller.Name = "tabCaller";
            this.tabCaller.Size = new System.Drawing.Size(682, 493);
            this.tabCaller.TabPageWidth = 100;
            this.tabCaller.Text = "Caller";
            // 
            // tabSocket
            // 
            this.tabSocket.Controls.Add(this.txtLog1);
            this.tabSocket.Name = "tabSocket";
            this.tabSocket.Size = new System.Drawing.Size(682, 493);
            this.tabSocket.TabPageWidth = 100;
            this.tabSocket.Text = "Socket";
            // 
            // txtLog1
            // 
            this.txtLog1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog1.Location = new System.Drawing.Point(0, 0);
            this.txtLog1.Multiline = true;
            this.txtLog1.Name = "txtLog1";
            this.txtLog1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog1.Size = new System.Drawing.Size(682, 493);
            this.txtLog1.TabIndex = 1;
            // 
            // tabOrders
            // 
            this.tabOrders.Controls.Add(this.txtLog2);
            this.tabOrders.Name = "tabOrders";
            this.tabOrders.Size = new System.Drawing.Size(682, 493);
            this.tabOrders.TabPageWidth = 100;
            this.tabOrders.Text = "Orders";
            // 
            // txtLog2
            // 
            this.txtLog2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog2.Location = new System.Drawing.Point(0, 0);
            this.txtLog2.Multiline = true;
            this.txtLog2.Name = "txtLog2";
            this.txtLog2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog2.Size = new System.Drawing.Size(682, 493);
            this.txtLog2.TabIndex = 1;
            // 
            // btnKuco
            // 
            this.btnKuco.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKuco.Location = new System.Drawing.Point(728, 318);
            this.btnKuco.Name = "btnKuco";
            this.btnKuco.Size = new System.Drawing.Size(117, 39);
            this.btnKuco.TabIndex = 1;
            this.btnKuco.Text = "Загрузить Куко(4)";
            this.btnKuco.UseVisualStyleBackColor = true;
            this.btnKuco.Click += new System.EventHandler(this.btnKuco_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(728, 363);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 39);
            this.button1.TabIndex = 1;
            this.button1.Text = "Загрузить Хуоб(4)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnKuco_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 561);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.btnKeys);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnKuco);
            this.Controls.Add(this.btnOrder);
            this.Name = "FrmMain";
            this.Text = "Orders checker";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.tabCaller.ResumeLayout(false);
            this.tabCaller.PerformLayout();
            this.tabSocket.ResumeLayout(false);
            this.tabSocket.PerformLayout();
            this.tabOrders.ResumeLayout(false);
            this.tabOrders.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TextBox txtLog;
        private Button btnOrder;
        private Button btnListen;
        private System.Windows.Forms.Timer timer_15min;
        private Button btnKeys;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage tabCaller;
        private DevExpress.XtraTab.XtraTabPage tabSocket;
        private DevExpress.XtraTab.XtraTabPage tabOrders;
        private Button btnKuco;
        private Button button1;
        private TextBox txtLog1;
        private TextBox txtLog2;
    }
}