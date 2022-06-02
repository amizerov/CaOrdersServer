using Kucoin.Net.Clients;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
    public class KucoCaller : IApiCaller
    {
        public event Action<Message>? OnProgress;
        
        User _user;
        ApiKey? _apiKey;
        KucoinClient _restClient = new();

        public KucoCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == Exch.Kuco);
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
        public Order GetOrder(string oid)
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
            Orders orders = new(_user); orders.OnProgress += OnProgress;
            if (_apiKey != null && _apiKey.IsWorking)
            {
                OnProgress?.Invoke(new Message(1, _user, Exch.Kuco,
                    "GetOrders", $"Kuco({_user.Name}): GetOrders started"));
                
                // Получаем ордера по страницам с 1-ой
                var rr = _restClient.SpotApi.Trading.GetRecentOrdersAsync().Result;
                if(rr.Success)
                {
                    foreach (var ord in rr.Data)
                    {
                        if (!orders.Any(o => o.ord_id == ord.Id))
                        {
                            orders.Add(new Order(ord, _user.ID));
                        }
                    }
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                        "GetOrders", $"GetRecentOrders Count={orders.Count}"));
                }
                var r = _restClient.SpotApi.Trading.GetOrdersAsync().Result;
                if (r.Success)
                {
                    int countPages = r.Data.TotalPages;
                    int currPage = r.Data.CurrentPage;
                    int totalOrders = r.Data.TotalItems;
                    while (currPage <= countPages)
                    {
                        KucoinPaginated<KucoinOrder> po = r.Data;
                        foreach (var ord in po.Items)
                        {
                            if (!orders.Any(o => o.ord_id == ord.Id))
                            {
                                orders.Add(new Order(ord, _user.ID));
                            }
                        }
                        // получаем следующую старницу ордеров
                        r = _restClient.SpotApi.Trading.GetOrdersAsync(currentPage: ++currPage).Result;
                        if (!r.Success)
                        {
                            OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, "GetOrders",
                                $"GetOrders(page:{currPage}) error \r\n[{r.Error?.Message}]"));

                            return orders;
                        }
                    }
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                        "GetOrders", $"GetOrders orders.Count = {orders.Count}|{totalOrders}"));
                }
                else
                {
                    OnProgress?.Invoke(new Message(1, _user, Exch.Kuco, 
                        "GetOrders", $"GetOrders error \r\n[{r.Error?.Message}]"));
                }
            }
            return orders;
        }
    }
}
