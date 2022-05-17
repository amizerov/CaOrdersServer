using DevExpress.XtraEditors;

namespace CaOrdersServer
{
    public partial class FrmMain : XtraForm 
    {/* ������ �� ���� �������� ����� �������������,
      * ��������� ����� �� ��������� ����� ����� OnProgress
       */
        // ��� ����� ����� ���������
        Users users = new();

        public FrmMain()
        {
            InitializeComponent();
            Text += " v." + Application.ProductVersion;
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            users.OnProgress += OnProgress;

            //btnKeys_Click(sender, e);

            /* Update all orders
             ���� ��� ��� ������� ������ ��� ������ �� ���� ���� �� �������
             �����, ������ �� ���������� �������� ����� ����� */
            //btnOrder_Click(sender, e);

            /* Start listen orders
             ����� ��������� �������� ������� �������� ����� */
            //btnListen_Click(sender, e);

            /* Keep alive sockets
             ������ 15 ����� �������������� ������ �� ���� ������ */
            //timer_15min.Start(); // will also check api keys
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";

            foreach (User u in users)
            {
                u.CheckApiKeys(1);
                u.CheckApiKeys(2);
                u.CheckApiKeys(3);
            }
        }
        private void btnOrder_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";

            foreach (User u in users)
            {
                u.UpdateOrders(1);
                u.UpdateOrders(2);
                u.UpdateOrders(3);
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            
            foreach (User u in users)
            {
                u.StartListenOrders(1);
                u.StartListenOrders(2);
                u.StartListenOrders(3);
            }
        }
        void OnProgress(string msg)
        {
            msg = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg + "\r\n";
            
            Invoke(new Action(() =>
            {
                txtLog.Text = msg + txtLog.Text;
            }));
        }

        private void timer_15min_Tick(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                //u.KeepAliveSpotBina();
            }
        }

    }
}