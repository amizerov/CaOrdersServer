using am.BL;

namespace CaOrdersServer
{
    public interface IApiCaller
    {
        public bool CheckApiKey();
        public Orders GetOrders();
        public Order GetOrder(string orderId, string symbol = "");
    }
    interface IApiSocket
    {
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
        }
        public bool CheckApiKeys()
        {
            return _caller.CheckApiKey();
        }
        public Order GetOrder(string orderId, string symbol = "")
        {
            return _caller.GetOrder(orderId, symbol);
        }
        public void UpdateOrders()
        {
            Task.Run(() =>
            {
                Msg.Send(1, _apiKey.User, _apiKey.Exch, "Exchange.UpdateOrders",
                    "-------------------------------------START--ORDER--UPDATE-->");
                
                Orders orders = _caller.GetOrders();
                orders.Update();

                Msg.Send(1, _apiKey.User, _apiKey.Exch, "Exchange.UpdateOrders",
                    "-------------------------------------ORDER--UPDATE--FINISHED-->");
            });
        }
        public bool InitOrdersListener()
        {
            return _socket.InitOrdersListener();
        }
    }
}
