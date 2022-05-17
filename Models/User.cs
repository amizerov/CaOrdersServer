using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class User
    {
        public event Action<string>? OnProgress;

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
                    exc.OnCallerProgress += OnCallerProgress;
                    exc.OnSocketMessage += OnSpcketMessage;
                }
            }

            OnProgress?.Invoke($"User {Name} created");
        }

        /******************************************************* 
         * One time call functions ->
         *******************************************************/
        public bool CheckApiKeys(int exc) 
        {
            Exchange? exch = Exchanges.Find(e => e.ID == exc);
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
        public void UpdateOrders(int exc)
        {
            Exchange? exch = Exchanges.Find(e => e.ID == exc);
            if(exch != null)
                Task.Run(() =>
                {
                    OnProgress?.Invoke($"{exch.Name}({Name}) UpdateOrders start");
                    exch.UpdateOrders();
                    OnProgress?.Invoke($"{exch.Name}({Name}) UpdateOrders end");
                });
        }
        // <--------------------------

        public bool StartListenOrders(int exc)
        {
            Exchange? exch = Exchanges.Find(e => e.ID == exc);
            if (exch == null)
                return false;
            else
                return exch.InitOrdersListener();
        }

        void OnCallerProgress(string msg) => OnProgress?.Invoke("C| " + msg);
        void OnSpcketMessage(string msg) => OnProgress?.Invoke("S| " + msg);
        void OnOrderProgress(string msg) => OnProgress?.Invoke("O| " + msg);
    }

    public class Users : List<User>
    {
        public event Action<string>? OnProgress;
        public Users()
        {
            string sql = "select distinct u.* from Users u join UserKeys k on u.id = k.usr_id where IsConfirmed = 1";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                User user = new User(r);
                Add(user);

                user.OnProgress += OnUserProgress;
            }
        }
        void OnUserProgress(string msg) => OnProgress?.Invoke(msg);
    }
 }
