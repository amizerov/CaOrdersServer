namespace CaOrdersServer
{
    public partial class FrmMain : Form
    {
        Users users = new Users();
        public FrmMain()
        {
            InitializeComponent();
            users.OnProgress += OnProgress;
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {
            foreach(User u in users)
            {
                u.CheckApiKeys();
                foreach(ApiKey k in u.ApiKeys)
                {
                    txtLog.Text += 
                        "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + 
                        $"{u.Name} - {k.Exchange} - IsWorking: {k.IsWorking}\r\n";
                }
            }
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.CheckOrdersSpotBinaAsynk();
                u.CheckOrdersSpotKucoAsynk();
                u.CheckOrdersSpotHuobAsynk();
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                if (u.StartListenSpotBina())
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Start listen spot Bina";
                else
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Error listen spot Bina";

                if(u.StartListenSpotKuco())
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Start listen spot Kuco";
                else
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Error listen spot Kuco";
                
                if(u.StartListenSpotHuob())
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Start listen spot Huob";
                else
                    txtLog.Text += "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + u.Name + " - Error listen spot Huob";
            }
        }
        void OnProgress(string msg)
        {
            msg = "\r\n[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg;

            Invoke(new Action(() =>
                txtLog.Text += msg
            ));
        }
    }
}