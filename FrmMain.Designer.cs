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
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 12);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(894, 551);
            this.txtLog.TabIndex = 0;
            // 
            // btnOrder
            // 
            this.btnOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOrder.Location = new System.Drawing.Point(927, 114);
            this.btnOrder.Name = "btnOrder";
            this.btnOrder.Size = new System.Drawing.Size(136, 45);
            this.btnOrder.TabIndex = 1;
            this.btnOrder.Text = "Загрузить ордера";
            this.btnOrder.UseVisualStyleBackColor = true;
            this.btnOrder.Click += new System.EventHandler(this.btnOrder_Click);
            // 
            // btnListen
            // 
            this.btnListen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnListen.Location = new System.Drawing.Point(927, 186);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(136, 45);
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
            this.btnKeys.Location = new System.Drawing.Point(927, 42);
            this.btnKeys.Name = "btnKeys";
            this.btnKeys.Size = new System.Drawing.Size(136, 45);
            this.btnKeys.TabIndex = 2;
            this.btnKeys.Text = "Проверить ключи";
            this.btnKeys.UseVisualStyleBackColor = true;
            this.btnKeys.Click += new System.EventHandler(this.btnKeys_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 575);
            this.Controls.Add(this.btnKeys);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.btnOrder);
            this.Controls.Add(this.txtLog);
            this.Name = "FrmMain";
            this.Text = "Orders checker";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txtLog;
        private Button btnOrder;
        private Button btnListen;
        private System.Windows.Forms.Timer timer_15min;
        private Button btnKeys;
    }
}