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
            ID = G._I(r["id"]);
            Name = G._S(r["Name"]);
            Email = G._S(r["Email"]);

            string sql = $"select * from UserKeys where usr_id = {ID}";
            DataTable dt = G.db_select(sql);

            foreach (DataRow k in dt.Rows)
            {
                ApiKey key = new ApiKey(k);
                ApiKeys.Add(key);
            }

            _binaCaller = new(this); _binaCaller.OnProgress += OnCallerProgress;
            _kucoCaller = new(this); _kucoCaller.OnProgress += OnCallerProgress;
            _huobCaller = new(this); _huobCaller.OnProgress += OnCallerProgress;

            _binaSocket = new(this); _binaSocket.OnMessage += OnSpcketProgress;
            _kucoSocket = new(this); _kucoSocket.OnMessage += OnSpcketProgress;
            _huobSocket = new(this); _huobSocket.OnMessage += OnSpcketProgress;
        }

        /******************************************************* 
         * One time call functions ->
         ******************************************************* 
         * Проверка ордеров на Бинансе, Кукоине и Хуоби, 
         * вызывается только один раз при запуске программы,
         * ордера сохраняются или обновляются в БД, 
         * потом их статусы обновляются через сокет.
         * Совмещен вызов для спота и маржина.
         */
        public void UpdateOrders(int Exchange)
        {
            ApiCaller caller = 
                Exchange == 1 ? _binaCaller : 
                Exchange == 2 ? _kucoCaller : _huobCaller;

            Task.Run(() =>
            {
                CaOrders orders = caller.GetOrders();
                orders.Update();
            });
        }
        public bool CheckApiKeys(int Exchange) 
        {
            ApiCaller caller =
                Exchange == 1 ? _binaCaller :
                Exchange == 2 ? _kucoCaller : _huobCaller;

            return caller.CheckApiKey(); 
        }
        // <--------------------------

        public bool StartListenOrders(int Exchange)
        {
            ApiSocket socket = 
                Exchange == 1 ? _binaSocket :
                Exchange == 2 ? _kucoSocket : _huobSocket;

            return socket.InitOrdersListener();
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
