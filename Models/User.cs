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

        private BinaSocket _binaSocket;
        private KucoSocket _kucoSocket;
        private HuobSocket _huobSocket;

        private BinaCaller _binaCaller;
        private KucoCaller _kucoCaller;
        private HuobCaller _huobCaller;

        public User(DataRow r)
        {
            _binaCaller = new(this); _binaCaller.OnProgress += OnCallerProgress;
            _kucoCaller = new(this); _kucoCaller.OnProgress += OnCallerProgress;
            _huobCaller = new(this); _huobCaller.OnProgress += OnCallerProgress;
            
            _binaSocket = new(this); _binaSocket.OnMessage += OnSpcketProgress;
            _kucoSocket = new(this); _kucoSocket.OnMessage += OnSpcketProgress;
            _huobSocket = new(this); _huobSocket.OnMessage += OnSpcketProgress;

            ID = G._I(r["id"]);
            Name = G._S(r["Name"]);
            Email = G._S(r["Email"]);

            string sql = $"select * from UserKeys where usr_id = {ID}";
            DataTable dt = G.db_select(sql);
            foreach (DataRow k in dt.Rows)
            {
                ApiKey key = new ApiKey(k);
                if(key.Exchange == "Bina") _binaCaller.CheckApiKey();
                if(key.Exchange == "Kuco") _kucoCaller.CheckApiKey();
                if(key.Exchange == "Huob") _huobCaller.CheckApiKey();

                ApiKeys.Add(key);
            }
;        }

        // One time call functions ->
        public void CheckOrdersBinaAsync(bool spotMarg = true)
        {/******************************************************* 
          * Проверка ордеров на Бинансе, вызывается только один раз при запуске программы,
          * ордера сохраняются или обновляются в БД, потом их статусы обновляются через сокет.
          * Совмещен вызов для спота и маржина.
           */
            Task.Run(() =>
            {
                List<CaOrder> orders = _binaCaller.GetAllOrders(spotMarg);
                foreach (var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        public void CheckOrdersKucoAsync(bool spotMarg = true)
        {/******************************************************* 
          * Проверка ордеров на Кукоине, вызывается только один раз при запуске программы,
          * ордера сохраняются или обновляются в БД, потом их статусы обновляются через сокет.
          * Совмещен вызов для спота и маржина.
           */
            Task.Run(() =>
            {
                List<CaOrder> orders = _kucoCaller.GetAllOrders(spotMarg);
                foreach (var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        public void CheckOrdersHuobAsync(bool spotMarg = true)
        {
            Task.Run(() => 
            { 
                List<CaOrder> orders = _huobCaller.GetAllOrders(spotMarg);
                foreach(var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        // <--------------------------

        public bool StartListenOrdersBina(bool spotMarg = true)
        {
            return _binaSocket.InitOrdersListener(spotMarg); ;
        }
        public bool StartListenOrdersKuco(bool spotMarg = true)
        {

            return _kucoSocket.InitOrdersListener();
        }
        public bool StartListenOrdersHuob(bool spotMarg = true)
        {

            return _huobSocket.InitOrdersListener();
        }

        void OnCallerProgress(string msg) => OnProgress?.Invoke(msg);
        void OnSpcketProgress(string msg) => OnProgress?.Invoke(msg);
    }

    public class Users : List<User>
    {
        public event Action<string>? OnProgress;
        public Users()
        {
            string sql = "select distinct u.* from Users u join Pairs p on u.id = p.usr_id";
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
