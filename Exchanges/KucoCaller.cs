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
                _kucoClient = new KucoinClient(
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
                var r = _kucoClient!.SpotApi.Account.GetAccountsAsync().Result;
                if (r != null && r.Success)
                {
                    balances = r.Data.ToList();
                }
            }
            return balances;
        }
        public List<KucoinOrder> GetAllOrders(bool spotMarg = true)
        {
            List<KucoinOrder> orders = new List<KucoinOrder>();
            var res = _restClient.SpotApi.Trading.GetOrdersAsync().Result;
            if (res.Success)
            {
                orders = res.Data.Items.ToList();
            }
            return orders;
        }
    }
}
