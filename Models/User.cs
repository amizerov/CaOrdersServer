using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class User
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        public int ID;
        public string Name;
        public string Email;
        public List<ApiKey> ApiKeys = new();

        public List<Exchange> Exchanges = new();

        public User(DataRow r)
        {
            ID = G._I(r["id"]);
            Name = G._S(r["Name"]);
            Email = G._S(r["Email"]);

            string sql = $"select * from UserKeys where usr_id = {ID}";
            DataTable dt = G.db_select(sql);

            foreach (DataRow k in dt.Rows)
            {
                ApiKey key = new ApiKey(k);
                ApiKeys.Add(key);

                Exchange exc = new Exchange(key, this);
                if (exc.CheckApiKeys())
                {
                    Exchanges.Add(exc);
                    exc.OnProgress += Progress;
                }
            }

            OnProgress?.Invoke(new Message(4, this, Exch.none, "User", $"User created"));
        }

        /******************************************************* 
         * One time call functions ->
         *******************************************************/
        public bool CheckApiKeys(Exch exc) 
        {
            Exchange? exch = Exchanges.Find(e => e.ID == (int)exc);
            if(exch == null)
                return false;
            else
                return exch.CheckApiKeys();
        }
         /* Проверка ордеров на Бинансе, Кукоине и Хуоби, 
         * вызывается только один раз при запуске программы,
         * ордера сохраняются или обновляются в БД, 
         * потом их статусы обновляются через сокет.
         * Совмещен вызов для спота и маржина.
         */
        public void UpdateOrders(Exch exc)
        {
            Exchange? excha = Exchanges.Find(e => e.ID == (int)exc);
            if(excha != null)
                Task.Run(() =>
                {
                    OnProgress?.Invoke(new Message(4, this, exc, "UpdateOrders", "start"));
                    excha.UpdateOrders();
                    OnProgress?.Invoke(new Message(4, this, exc, "UpdateOrders", "end"));
                });
        }
        // <--------------------------

        public bool StartListenOrders(Exch exc)
        {
            Exchange? exch = Exchanges.Find(e => e.ID == (int)exc);
            if (exch == null)
                return false;
            else
                return exch.InitOrdersListener();
        }
    }

    public class Users : List<User>
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        public Users()
        {
            string sql = "select distinct u.* from Users u join UserKeys k on u.id = k.usr_id where IsConfirmed = 1";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                User user = new User(r);
                Add(user);

                user.OnProgress += Progress;
            }
        }
    }
}
