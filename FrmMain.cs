using System.Linq;

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
            btnOrder_Click(sender, e); // spot
            btnMarg_Click(sender, e);  // marg

            // Start listen orders
            // ����� ��������� �������� ������� �������� �����
            btnListen_Click(sender, e);

            // Keep alive sockets
            // ������ 15 ����� �������������� ������ �� ���� ������
            timer_15min.Start(); // will also check api keys
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {   
            foreach(User u in users)
            {
                u.CheckApiKeys();
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.CheckOrdersBinaAsync();
                //u.CheckOrdersAsynk(2);
                //u.CheckOrdersAsynk(3);
            }
        }

        private void btnMarg_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.CheckOrdersBinaAsync(false);
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.StartListenSpotBina();
                
                /*
                if(u.StartListenSpotKuco())
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Start listen spot Kuco";
                else
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Error listen spot Kuco";
                
                if(u.StartListenSpotHuob())
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Start listen spot Huob";
                else
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Error listen spot Huob";
                */
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
                u.KeepAliveSpotBina();
            }
        }
    }
}