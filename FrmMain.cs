namespace CaOrdersServer
{
    public partial class FrmMain : Form
    {/* Доступ ко всем функциям через пользователей,
      * сообщения через из вложенных через евент OnProgress
       */
        // Все юзеры сразу создаются и проверяют ключи
        Users users = new();

        public FrmMain()
        {
            InitializeComponent();
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            users.OnProgress += OnProgress;

            // Update all orders
            // Один раз при запуске грузим все ордера со всех бирж по запросу
            // далее, следим за изменением статусов через сокет
            //btnOrder_Click(sender, e); // spot

            // Start listen orders
            // после первичной загрузки ордеров включаем сокет
            btnListen_Click(sender, e);

            // Keep alive sockets
            // Каждые 15 минут переподключаем сокеты по всем биржам
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