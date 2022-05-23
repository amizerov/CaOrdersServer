using am.BL;

namespace CaOrdersServer
{
    public class Exchange
    {
        public event Action<string>? OnCallerProgress;
        public event Action<string>? OnSocketMessage;

        int _id;
        string _name;
        ApiKey _apiKey;
        IApiCaller _caller;
        IApiSocket _socket;

        public int ID { get { return _id; } }
        public string Name { get { return _name; } }
        public Exchange(ApiKey key, User usr)
        {
            _id = key.ExchangeId;
            _name = key.Exchange;
            _apiKey = key;

            if (_id == 1)
            {
                _caller = new BinaCaller(usr); 
                _socket = new BinaSocket(usr);
            }
            else if (_id == 2)
            {
                _caller = new KucoCaller(usr);
                _socket = new KucoSocket(usr);
            }
            else
            {
                _caller = new HuobCaller(usr);
                _socket = new HuobSocket(usr);
            }
            _caller.OnProgress += CallerProgress;
            _socket.OnMessage += SocketMessage;
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
            Orders orders = _caller.GetOrders();
            orders.Update();
        }
        public bool InitOrdersListener()
        {
            return _socket.InitOrdersListener();
        }
        public void CallerProgress(string msg) => OnCallerProgress?.Invoke(msg);
        public void SocketMessage(string msg) => OnSocketMessage?.Invoke(msg);
    }
    public interface IApiCaller
    {
        public event Action<string>? OnProgress;
       
        public bool CheckApiKey();
        public Orders GetOrders();
        public Order GetOrder(string orderId);
    }
    interface IApiSocket
    {
        public event Action<string>? OnMessage;

        bool InitOrdersListener(int minutesBetweenReconnect = 20);
        void KeepAlive(int minutesBetweenReconnect = 20);
        void Dispose(bool setNull = true);
    }
}
