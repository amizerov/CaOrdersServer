using am.BL;
using DevExpress.XtraEditors;

namespace CaOrdersServer
{
    public partial class FrmMain : XtraForm 
    {/* Доступ ко всем функциям через пользователей,
      * сообщения через из вложенных через евент OnProgress
       */
        // Все юзеры сразу создаются
        Users users = new();
        User? SelectedUser;
        Exchange? SelectedExch;
        bool needToRestoreTableOrdersLayout = true;

        public FrmMain()
        {
            InitializeComponent();
            Text += " v." + Application.ProductVersion;
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            users.OnProgress += Progress;

            LoadTreeList();

            /* Update all orders
             Один раз при запуске грузим все ордера со всех бирж по запросу
             далее, следим за изменением статусов через сокет */
            //btnOrder_Click(sender, e);

            /* Start listen orders
             после первичной загрузки ордеров включаем сокет */
            //btnListen_Click(sender, e);

            /* Каждые 15 минут check api keys */
            //timer_15min.Start();  

            treeList.RestoreLayoutFromXml("tree.xml");
            //gvOrders.RestoreLayoutFromXml("orde.xml");
        }

        private void btnKeys_Click(object sender, EventArgs e)
        {
            if (SelectedExch != null)
            {
                // Выбрана конкретная Биржа конкретного Юзера
                SelectedExch.CheckApiKeys();
            }
            else if (SelectedUser != null)
            {
                // Выбран только Юзер, обновим все биржи для него
                foreach (Exchange ex in SelectedUser.Exchanges)
                    ex.CheckApiKeys();
            }
            else
            {
                // Обновляем ордера всех Юзеров по всем Биржам
                foreach (User u in users)
                    foreach (Exchange ex in u.Exchanges)
                        ex.CheckApiKeys();
            }
        }
        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (SelectedExch != null)
            {
                // Выбрана конкретная Биржа конкретного Юзера
                SelectedExch.UpdateOrders();
            }
            else if (SelectedUser != null)
            {
                // Выбран только Юзер, обновим все биржи для него
                foreach (Exchange ex in SelectedUser.Exchanges)
                    ex.UpdateOrders();
            }
            else
            {
                // Обновляем ордера всех Юзеров по всем Биржам
                foreach (User u in users)
                    foreach (Exchange ex in u.Exchanges)
                        ex.UpdateOrders();
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            if (SelectedExch != null)
            {
                // Выбрана конкретная Биржа конкретного Юзера
                SelectedExch.InitOrdersListener();
            }
            else if (SelectedUser != null)
            {
                // Выбран только Юзер, обновим все биржи для него
                foreach (Exchange ex in SelectedUser.Exchanges)
                    ex.InitOrdersListener();
            }
            else
            {
                // Обновляем ордера всех Юзеров по всем Биржам
                foreach (User u in users)
                    foreach (Exchange ex in u.Exchanges)
                        ex.InitOrdersListener();
            }
        }

        private void btnTimer_Click(object sender, EventArgs e)
        {
            /* Каждые 15 минут check api keys */
            timer_15min.Start();
        }

        private void timer_15min_Tick(object sender, EventArgs e)
        {
            btnKeys_Click(sender, e);
        }

        private void treeList_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            int uid = (int)e.Node[1];
            int eid = (int)e.Node[2];
            if (uid == 0 && e.Node.ParentNode != null)
                uid = (int)e.Node.ParentNode[1];

            SelectedExch = null;
            SelectedUser = users.Find(u => u.ID == uid);
            if(SelectedUser != null)
                SelectedExch = SelectedUser.Exchanges.Find(e => e.ID == eid);

            gcOrders.DataSource = Db.GetOrdersDt(uid, eid);

            if(needToRestoreTableOrdersLayout)
                gvOrders.RestoreLayoutFromXml("orde.xml");
            needToRestoreTableOrdersLayout = false;
        }

        void LoadTreeList()
        {
            List<TreeNode> tree = new();
            tree.Add(new TreeNode { id = 0, pid = 0, Name = "Все" });
            foreach (var u in users)
            {
                tree.Add(new TreeNode { id = u.ID, pid = u.ID, Name = u.Name, user_id = u.ID });
                foreach (var e in u.Exchanges)
                {
                    tree.Add(new TreeNode { id = u.ID * 10 + e.ID, pid = u.ID, Name = e.Name, exch_id = e.ID });
                }
            }
            treeList.DataSource = tree;
            treeList.KeyFieldName = "id";
            treeList.ParentFieldName = "pid";
            treeList.Columns[0].Caption = "Users";
            treeList.OptionsBehavior.Editable = false;
            treeList.RowHeight = 20;
            treeList.ExpandAll();
        }

        void Progress(Message msg)
        {
            string m = 
                $"[{DateTime.Now.ToString("hh:mm:ss")}] {msg.exch}({msg.user.Name}) {msg.src} {msg.msg}\r\n";
            
            Invoke(new Action(() =>
            {
                switch(msg.type)
                {
                    case 1:
                        txtLog.Text = m + txtLog.Text;
                        break;
                    case 2:
                        txtLog1.Text = m + txtLog1.Text;
                        break;
                    case 3:
                        txtLog2.Text = m + txtLog2.Text;
                        break;
                }
            }));
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            treeList.SaveLayoutToXml("tree.xml");
            gvOrders.SaveLayoutToXml("orde.xml");
        }

        private void btnUpdateOneOrderByID_Click(object sender, EventArgs e)
        {
            string? ord_id = gvOrders.GetFocusedDataRow()["ord_id"].ToString();
            string? symbol = gvOrders.GetFocusedDataRow()["symbol"].ToString();
            if (SelectedExch != null && ord_id != null && symbol != null)
            {
                Order o = SelectedExch.GetOrder(ord_id, symbol);
                o.Update("Manual");
            }
        }
    }
}