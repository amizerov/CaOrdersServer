using Kucoin.Net.Clients;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
    public class KucoCaller : IApiCaller
    {
        public event Action<Message>? OnProgress;
        void Progress(Message msg) => OnProgress?.Invoke(msg);

        User _user;
        ApiKey? _apiKey;
        KucoinClient _restClient = new();

        public KucoCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exch == Exch.Kuco);
        }
        public bool CheckApiKey()
        {
            bool res = false;
            if (_apiKey != null)
            {
                try
                {
                    _restClient = new KucoinClient(
                        new KucoinClientOptions()
                        {
                            ApiCredentials = new KucoinApiCredentials(_apiKey.Key!, _apiKey.Secret!, _apiKey.PassPhrase)
                        });
                }
                catch (Exception ex)
                {
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                        "CheckApiKey", $"Error: {ex.Message}"));
                    
                    return false; 
                }
                // Если получен доступ к балансам, ключ считается рабочим
                List<KucoinAccount> bs = GetBalances();
                res = bs.Count > 0;

                _apiKey.IsWorking = res;

                OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, "CheckApiKey", $"Key.IsWorking: {_apiKey.IsWorking}"));
            }
            
            return res;
        }
        List<KucoinAccount> GetBalances()
        {
            List<KucoinAccount> balances = new();
            var res = _restClient!.SpotApi.Account.GetAccountsAsync().Result;
            if (res.Success)
            {
                // TODO: может быть 0 акаунтов почемуто
                balances = res.Data.ToList();
                if(balances.Count == 0)
                {
                    var r = _restClient!.SpotApi.Trading.GetOrdersAsync().Result;
                    if (r.Success)
                    {
                        var c = r.Data.TotalItems;
                        Log.Write("qq");
                    }
                }
            }
            else
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                    "GetBalances", $"Error GetAccountsAsync: {res.Error?.Message}"));
            }
            return balances;
        }
        public Order GetOrder(string oid, string symbol = "")
        {
            KucoinOrder order = new KucoinOrder();
            if (_apiKey != null && _apiKey.IsWorking)
            {
                var res = _restClient.SpotApi.Trading.GetOrderAsync(oid).Result;
                if (res.Success)
                {
                    order = res.Data;
                }
            }
            return new Order(order, _user.ID);
        }
        public Orders GetOrders()
        {
            Orders orders = new(_user); orders.OnProgress += Progress;

            if (_apiKey != null && _apiKey.IsWorking)
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, "GetOrders", "Started"));
                
                var r = _restClient.SpotApi.Trading.GetRecentOrdersAsync().Result;
                if(r.Success)
                {
                    foreach (var ord in r.Data)
                    {
                        orders.Add(new Order(ord, _user.ID));
                    }
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, "GetRecentOrders", $"Count={orders.Count}"));
                    
                    var rr = _restClient.SpotApi.Trading.GetOrdersAsync(
                            startTime: DateTime.Now.AddDays(-30)
                        ).Result;

                    if (rr.Success)
                    {
                        int countPages = rr.Data.TotalPages;
                        int currPage = rr.Data.CurrentPage;
                        int totalOrders = rr.Data.TotalItems;

                        // Получаем ордера по страницам начиная с 1-ой
                        while (currPage <= countPages)
                        {
                            KucoinPaginated<KucoinOrder> po = rr.Data;
                            foreach (var ord in po.Items)
                            {
                                orders.Add(new Order(ord, _user.ID));
                            }
                            // получаем следующую старницу ордеров
                            rr = _restClient.SpotApi.Trading.GetOrdersAsync(currentPage: ++currPage).Result;
                            if (!rr.Success)
                            {
                                OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                                    $"GetOrders(page:{currPage})", $"Error: {rr.Error?.Message}"));

                                return orders;
                            }
                        }
                        OnProgress?.Invoke(new Message(1, _user, Exch.Kuco,
                            "GetOrders", $"Count={orders.Count}|{totalOrders}"));
                    }
                    else
                    {
                        OnProgress?.Invoke(new Message(1, _user, Exch.Kuco,
                            "GetOrders", $"Error: {rr.Error?.Message}"));
                    }
                }
                else
                {
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco,
                        "GetRecentOrders", $"Error: {r.Error?.Message}"));
                }
            }
            return orders;
        }
    }
}
