using Kucoin.Net.Clients;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models;
using Kucoin.Net.Objects.Models.Spot;

namespace CaOrdersServer
{
    public class KucoCaller : IApiCaller
    {
        public event Action<string>? OnProgress;
        
        User _user;
        ApiKey _apiKey;
        KucoinClient _restClient = new();

        public KucoCaller(User usr)
        {
            _user = usr;
            _apiKey = _user.ApiKeys.Find(k => k.Exchange == "Kuco") ?? new();
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
            OnProgress?.Invoke($"CheckApiKey({_apiKey.Exchange}/{_user.Name}) Key.IsWorking: {_apiKey.IsWorking}");
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
        public Orders GetOrders()
        {
            Orders orders = new(_user);
            if (_apiKey.IsWorking)
            {
                OnProgress?.Invoke($"Kucoin({_user.Name}): GetOrders started");
                
                // Получаем ордера по страницам с 1-ой
                var r = _restClient.SpotApi.Trading.GetOrdersAsync().Result;
                if (r.Success)
                {
                    int countOrders = 0;
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
                                orders.Add(new Order(ord));
                                countOrders++;
                            }
                        }
                        // получаем следующую старницу ордеров
                        r = _restClient.SpotApi.Trading.GetOrdersAsync(currentPage: ++currPage).Result;
                        if (!r.Success)
                        {
                            OnProgress?.Invoke($"Kucoin({_user.Name}): GetOrders(page:{currPage}) error \r\n[{r.Error?.Message}]");
                            return orders;
                        }
                    }
                    OnProgress?.Invoke($"Kucoin({_user.Name}): GetOrders orders.Count = {orders.Count}|{countOrders}|{totalOrders}");
                }
                else
                    OnProgress?.Invoke($"Kucoin({_user.Name}): GetOrders error \r\n[{r.Error?.Message}]");

            }
            return orders;
        }
    }
}
