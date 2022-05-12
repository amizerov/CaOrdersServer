namespace CaOrdersServer
{
    public interface ApiCaller
    {
        public bool CheckApiKey();// { return false; }
        public CaOrders GetOrders();// { return new List<CaOrder>(); }
    }
    interface ApiSocket
    {
        bool InitOrdersListener(int minutesBetweenReconnect = 20);
        void KeepAlive(int minutesBetweenReconnect = 20);
        void Dispose(bool setNull = true);
    }
}
