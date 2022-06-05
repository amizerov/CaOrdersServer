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
            this.tabControl = new DevExpress.XtraTab.XtraTabControl();
            this.tabCaller = new DevExpress.XtraTab.XtraTabPage();
            this.tabTable = new DevExpress.XtraTab.XtraTabPage();
            this.gcOrders = new DevExpress.XtraGrid.GridControl();
            this.gvOrders = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.tabSocket = new DevExpress.XtraTab.XtraTabPage();
            this.txtLog1 = new System.Windows.Forms.TextBox();
            this.tabOrders = new DevExpress.XtraTab.XtraTabPage();
            this.txtLog2 = new System.Windows.Forms.TextBox();
            this.btnKuco = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeList = new DevExpress.XtraTreeList.TreeList();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnTimer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabCaller.SuspendLayout();
            this.tabTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcOrders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvOrders)).BeginInit();
            this.tabSocket.SuspendLayout();
            this.tabOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeList)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(568, 553);
            this.txtLog.TabIndex = 0;
            // 
            // btnOrder
            // 
            this.btnOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOrder.Location = new System.Drawing.Point(892, 96);
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
            this.btnListen.Location = new System.Drawing.Point(892, 156);
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
            this.btnKeys.Location = new System.Drawing.Point(892, 36);
            this.btnKeys.Name = "btnKeys";
            this.btnKeys.Size = new System.Drawing.Size(117, 39);
            this.btnKeys.TabIndex = 2;
            this.btnKeys.Text = "Проверить ключи";
            this.btnKeys.UseVisualStyleBackColor = true;
            this.btnKeys.Click += new System.EventHandler(this.btnKeys_Click);
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedTabPage = this.tabCaller;
            this.tabControl.Size = new System.Drawing.Size(570, 578);
            this.tabControl.TabIndex = 3;
            this.tabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabTable,
            this.tabCaller,
            this.tabSocket,
            this.tabOrders});
            // 
            // tabCaller
            // 
            this.tabCaller.Controls.Add(this.txtLog);
            this.tabCaller.Name = "tabCaller";
            this.tabCaller.Size = new System.Drawing.Size(568, 553);
            this.tabCaller.TabPageWidth = 100;
            this.tabCaller.Text = "Caller";
            // 
            // tabTable
            // 
            this.tabTable.Controls.Add(this.gcOrders);
            this.tabTable.Name = "tabTable";
            this.tabTable.Size = new System.Drawing.Size(568, 553);
            this.tabTable.TabPageWidth = 100;
            this.tabTable.Text = "Table";
            // 
            // gcOrders
            // 
            this.gcOrders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcOrders.Location = new System.Drawing.Point(0, 0);
            this.gcOrders.MainView = this.gvOrders;
            this.gcOrders.Name = "gcOrders";
            this.gcOrders.Size = new System.Drawing.Size(568, 553);
            this.gcOrders.TabIndex = 0;
            this.gcOrders.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvOrders});
            // 
            // gvOrders
            // 
            this.gvOrders.GridControl = this.gcOrders;
            this.gvOrders.Name = "gvOrders";
            this.gvOrders.OptionsView.ShowGroupPanel = false;
            // 
            // tabSocket
            // 
            this.tabSocket.Controls.Add(this.txtLog1);
            this.tabSocket.Name = "tabSocket";
            this.tabSocket.Size = new System.Drawing.Size(568, 553);
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
            this.txtLog1.Size = new System.Drawing.Size(568, 553);
            this.txtLog1.TabIndex = 1;
            // 
            // tabOrders
            // 
            this.tabOrders.Controls.Add(this.txtLog2);
            this.tabOrders.Name = "tabOrders";
            this.tabOrders.Size = new System.Drawing.Size(568, 553);
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
            this.txtLog2.Size = new System.Drawing.Size(568, 553);
            this.txtLog2.TabIndex = 1;
            // 
            // btnKuco
            // 
            this.btnKuco.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKuco.Location = new System.Drawing.Point(892, 318);
            this.btnKuco.Name = "btnKuco";
            this.btnKuco.Size = new System.Drawing.Size(117, 39);
            this.btnKuco.TabIndex = 1;
            this.btnKuco.Text = "Загрузить Куко(4)";
            this.btnKuco.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(892, 363);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 39);
            this.button1.TabIndex = 1;
            this.button1.Text = "Загрузить Хуоб(4)";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl);
            this.splitContainer1.Size = new System.Drawing.Size(859, 578);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 4;
            // 
            // treeList
            // 
            this.treeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeList.Location = new System.Drawing.Point(0, 0);
            this.treeList.Name = "treeList";
            this.treeList.BeginUnboundLoad();
            this.treeList.AppendNode(new object[0], -1);
            this.treeList.EndUnboundLoad();
            this.treeList.Size = new System.Drawing.Size(285, 578);
            this.treeList.TabIndex = 0;
            this.treeList.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeList_FocusedNodeChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 604);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1028, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(52, 17);
            this.toolStripStatusLabel1.Text = "Progress";
            // 
            // btnTimer
            // 
            this.btnTimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTimer.Location = new System.Drawing.Point(892, 216);
            this.btnTimer.Name = "btnTimer";
            this.btnTimer.Size = new System.Drawing.Size(117, 39);
            this.btnTimer.TabIndex = 1;
            this.btnTimer.Text = "Запустить таймер 15 минут";
            this.btnTimer.UseVisualStyleBackColor = true;
            this.btnTimer.Click += new System.EventHandler(this.btnTimer_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 626);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnKeys);
            this.Controls.Add(this.btnTimer);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnKuco);
            this.Controls.Add(this.btnOrder);
            this.Name = "FrmMain";
            this.Text = "Orders checker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabCaller.ResumeLayout(false);
            this.tabCaller.PerformLayout();
            this.tabTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcOrders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvOrders)).EndInit();
            this.tabSocket.ResumeLayout(false);
            this.tabSocket.PerformLayout();
            this.tabOrders.ResumeLayout(false);
            this.tabOrders.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeList)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txtLog;
        private Button btnOrder;
        private Button btnListen;
        private System.Windows.Forms.Timer timer_15min;
        private Button btnKeys;
        private DevExpress.XtraTab.XtraTabControl tabControl;
        private DevExpress.XtraTab.XtraTabPage tabCaller;
        private DevExpress.XtraTab.XtraTabPage tabSocket;
        private DevExpress.XtraTab.XtraTabPage tabOrders;
        private Button btnKuco;
        private Button button1;
        private TextBox txtLog1;
        private TextBox txtLog2;
        private SplitContainer splitContainer1;
        private DevExpress.XtraTreeList.TreeList treeList;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button btnTimer;
        private DevExpress.XtraTab.XtraTabPage tabTable;
        private DevExpress.XtraGrid.GridControl gcOrders;
        private DevExpress.XtraGrid.Views.Grid.GridView gvOrders;
    }
}