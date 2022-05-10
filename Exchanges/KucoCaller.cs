using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
    public class KucoCaller
    {
        public event Action<string>? OnProgress;
        
        User _user;
        ApiKey _apiKey;
        KucoinClient _restClient = new();

        public KucoCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Kuco") ?? new();

            CheckApiKey();
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey.ID > 0)
            {
                _restClient = new KucoinClient(
                    new KucoinClientOptions()
                    {
                        ApiCredentials = new KucoinApiCredentials(_apiKey.Key!, _apiKey.Secret!, _apiKey.PassPhrase)
                    });
                // Если получен доступ к балансам, ключ считается рабочим
                List<KucoinAccount> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;
            }
            OnProgress?.Invoke($"{_user.Name} - {_apiKey.Exchange} - IsWorking: {_apiKey.IsWorking}");
            return res;
        }
        public List<KucoinAccount> GetBalances()
        {
            List<KucoinAccount> balances = new();
            if (_apiKey.IsWorking)
            {
                var r = _restClient!.SpotApi.Account.GetAccountsAsync().Result;
                if (r != null && r.Success)
                {
                    balances = r.Data.ToList();
                }
            }
            return balances;
        }
        public List<CaOrder> GetAllOrders(bool spotMarg = true)
        {
            List<CaOrder> orders = new();
            var r = _restClient.SpotApi.Trading.GetOrdersAsync().Result;
            if (r.Success)
            {
                foreach (var o in r.Data.Items)
                {
                    bool sm = o.TradeType == TradeType.SpotTrade;
                    if(sm == spotMarg)
                        orders.Add(new CaOrder(o, _user.ID, spotMarg));
                }
            }
            return orders;
        }
    }
}
