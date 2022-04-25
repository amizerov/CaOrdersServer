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
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnKeys = new System.Windows.Forms.Button();
            this.btnOrder = new System.Windows.Forms.Button();
            this.btnListen = new System.Windows.Forms.Button();
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
            this.txtLog.Size = new System.Drawing.Size(608, 551);
            this.txtLog.TabIndex = 0;
            // 
            // btnKeys
            // 
            this.btnKeys.Location = new System.Drawing.Point(641, 12);
            this.btnKeys.Name = "btnKeys";
            this.btnKeys.Size = new System.Drawing.Size(136, 43);
            this.btnKeys.TabIndex = 1;
            this.btnKeys.Text = "Проверить ключи";
            this.btnKeys.UseVisualStyleBackColor = true;
            this.btnKeys.Click += new System.EventHandler(this.btnKeys_Click);
            // 
            // btnOrder
            // 
            this.btnOrder.Location = new System.Drawing.Point(641, 61);
            this.btnOrder.Name = "btnOrder";
            this.btnOrder.Size = new System.Drawing.Size(136, 43);
            this.btnOrder.TabIndex = 1;
            this.btnOrder.Text = "Загрузить SPOT ордера";
            this.btnOrder.UseVisualStyleBackColor = true;
            this.btnOrder.Click += new System.EventHandler(this.btnOrder_Click);
            // 
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(641, 110);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(136, 43);
            this.btnListen.TabIndex = 1;
            this.btnListen.Text = "Начать слушать SPOT ордера";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 575);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.btnOrder);
            this.Controls.Add(this.btnKeys);
            this.Controls.Add(this.txtLog);
            this.Name = "FrmMain";
            this.Text = "Orders checker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txtLog;
        private Button btnKeys;
        private Button btnOrder;
        private Button btnListen;
    }
}