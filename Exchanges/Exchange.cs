using am.BL;

namespace CaOrdersServer
{
    public class Message
    {
        public int type;
        public User user;
        public Exch exch;
        public string src = "";
        public string msg = "";
        public Message(int t, User u, Exch e, string s, string m) { 
            type = t; user = u; exch = e; src = s; msg = m; 
        }
    }
    public interface IApiCaller
    {
        public event Action<Message>? OnProgress;

        public bool CheckApiKey();
        public Orders GetOrders();
        public Order GetOrder(string orderId);
    }
    interface IApiSocket
    {
        public event Action<Message>? OnProgress;

        bool InitOrdersListener(int minutesBetweenReconnect = 20);
        void KeepAlive(int minutesBetweenReconnect = 20);
        void Dispose(bool setNull = true);
    }
    public enum Exch
    {
        Bina = 1,
        Kuco = 2,
        Huob = 3,
        none = 4
    }
    public class Exchange
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        ApiKey _apiKey;
        IApiCaller _caller;
        IApiSocket _socket;

        public int ID { get { return (int)_apiKey.Exch; } }
        public string Name { get { return _apiKey.Exch.ToString(); } }
        public Exchange(ApiKey key)
        {
            _apiKey = key;
            User usr = key.User;

            if (_apiKey.Exch == Exch.Bina)
            {
                _caller = new BinaCaller(usr);
                _socket = new BinaSocket(usr);
            }
            else if (_apiKey.Exch == Exch.Kuco)
            {
                _caller = new KucoCaller(usr);
                _socket = new KucoSocket(usr);
            }
            else
            {
                _caller = new HuobCaller(usr);
                _socket = new HuobSocket(usr);
            }

            _caller.OnProgress += Progress;
            _socket.OnProgress += Progress;
        }
        public bool CheckApiKeys()
        {
            return _caller.CheckApiKey();
        }
        public Order GetOrder(string orderId)
        {
            return _caller.GetOrder(orderId);
        }
        public void UpdateOrders()
        {
            Task.Run(() =>
            {
                Orders orders = _caller.GetOrders();
                orders.OnProgress += Progress;
                orders.Update();
            });
        }
        public bool InitOrdersListener()
        {
            return _socket.InitOrdersListener();
        }
    }
}
