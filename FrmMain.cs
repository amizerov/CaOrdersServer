namespace CaOrdersServer
{
    public partial class FrmMain : Form
    {/* ������ �� ���� �������� ����� �������������,
      * ��������� ����� �� ��������� ����� ����� OnProgress
       */
        // ��� ����� ����� ��������� � ��������� �����
        Users users = new();

        public FrmMain()
        {
            InitializeComponent();
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            users.OnProgress += OnProgress;

            // Update all orders
            // ���� ��� ��� ������� ������ ��� ������ �� ���� ���� �� �������
            // �����, ������ �� ���������� �������� ����� �����
            //btnOrder_Click(sender, e); // spot

            // Start listen orders
            // ����� ��������� �������� ������� �������� �����
            btnListen_Click(sender, e);

            // Keep alive sockets
            // ������ 15 ����� �������������� ������ �� ���� ������
            timer_15min.Start(); // will also check api keys
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.CheckApiKeys(1);
                u.CheckApiKeys(2);
                u.CheckApiKeys(3);
            }
        }
        private void btnOrder_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.UpdateOrders(1);
                u.UpdateOrders(2);
                u.UpdateOrders(3);
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                //u.StartListenOrders(1);
                u.StartListenOrders(2);
                //u.StartListenOrders(3);
            }
        }
        void OnProgress(string msg)
        {
            msg = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg + "\r\n";
            
            Invoke(new Action(() =>
            {
                txtLog.Text = msg + txtLog.Text;

                if (txtLog.Text.Count(c => c == '\r') > 100)
                {
                    txtLog.Text = txtLog.Text.Substring(0, 100);
                }
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