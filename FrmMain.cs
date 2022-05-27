using DevExpress.XtraEditors;

namespace CaOrdersServer
{
    public partial class FrmMain : XtraForm 
    {/* ƒоступ ко всем функци€м через пользователей,
      * сообщени€ через из вложенных через евент OnProgress
       */
        // ¬се юзеры сразу создаютс€
        Users users = new();

        public FrmMain()
        {
            InitializeComponent();
            Text += " v." + Application.ProductVersion;
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            users.OnProgress += OnProgress;

            /* Update all orders
             ќдин раз при запуске грузим все ордера со всех бирж по запросу
             далее, следим за изменением статусов через сокет */
            btnOrder_Click(sender, e);

            /* Start listen orders
             после первичной загрузки ордеров включаем сокет */
            btnListen_Click(sender, e);

            /*  аждые 15 минут check api keys */
            timer_15min.Start();  
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.CheckApiKeys(Exch.Bina);
                u.CheckApiKeys(Exch.Kuco);
                u.CheckApiKeys(Exch.Huob);
            }
        }
        private void btnOrder_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";

            foreach (User u in users)
            {
                u.UpdateOrders(Exch.Bina);
                u.UpdateOrders(Exch.Kuco);
                u.UpdateOrders(Exch.Huob);
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            foreach (User u in users)
            {
                u.StartListenOrders(Exch.Bina);
                u.StartListenOrders(Exch.Kuco);
                u.StartListenOrders(Exch.Huob);
            }
        }

        private void timer_15min_Tick(object sender, EventArgs e)
        {
            btnKeys_Click(sender, e);
        }

        private void btnKuco_Click(object sender, EventArgs e)
        {
            users.Find(u => u.ID == 4)!.UpdateOrders(Exch.Kuco);
        }

        void OnProgress(Message msg)
        {
            string m = "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + msg.msg + "\r\n";
            
            Invoke(new Action(() =>
            {
                switch(msg.type)
                {
                    case 1:
                        txtLog.Text = msg + txtLog.Text;
                        break;
                    case 2:
                        txtLog1.Text = msg + txtLog1.Text;
                        break;
                    case 3:
                        txtLog2.Text = msg + txtLog2.Text;
                        break;
                }
            }));
        } 
    }
}