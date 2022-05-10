using am.BL;
using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Socket;
using Kucoin.Net.Objects.Models.Spot;
using System.Data;

namespace CaOrdersServer
{
    public class User
    {
        public int ID;
        public string Name;
        public string Email;
        public List<ApiKey> ApiKeys = new();

        BinaSocket? _binaSocket;
        KucoSocket? _kucoSocket;
        HuobSocket? _huobSocket;

        BinaCaller _binaCaller;
        KucoCaller _kucoCaller;
        HuobCaller _huobCaller;

        public event Action<string>? OnProgress;

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
;        }

        // Socket used ------------>
        public void UpdateAccountBina()
        {/******************************************************* 
          * Вызывается при изменении балансов на счету Бинанса
          * если подписались на событие UserDataUpdates в BinaSoket
           */
            List<BinanceBalance> balances = _binaCaller.GetBalances();

            foreach (BinanceBalance b in balances)
            {
                CaBalance userBalanceOnBinance = new CaBalance(b, ID);
                userBalanceOnBinance.Update();
            }        
        }
        public void UpdateAccount(List<BinanceStreamBalance> balances)
        {
            foreach (BinanceStreamBalance b in balances)
            {
                CaBalance userBalanceOnBinance = new CaBalance(b, ID);
                userBalanceOnBinance.Update();
            }
        }
        public bool UpdateOrder(BinanceStreamOrderUpdate ord, bool spotMarg)
        {/******************************************************* 
          * Вызывается при изменении изменении состояния ордера на Бинанса
          * если подписались на событие UserDataUpdates в BinaSoket
           */
            return CaOrder.UpdateStatus(ord, ID, spotMarg);
        }
        //-------------------------<

        // One time call functions ->
        public void CheckApiKeys()
        {/******************************************************* 
          * Проверка ключей доступа всех бирж
           */
            foreach (var key in ApiKeys)
            {
                bool b = false;
                switch (key.Exchange)
                {
                    case "Bina":
                        b = _binaCaller.CheckApiKey();
                        break;
                    case "Kuco":
                        b = _kucoCaller.CheckApiKey();
                        break;
                    case "Huob":
                        b = _huobCaller.CheckApiKey();
                        break;
                }
                key.IsWorking = b;
                key.CheckedAt = DateTime.Now;
            }
        }
        public void CheckOrdersBinaAsync(bool spotMarg = true)
        {/******************************************************* 
          * Проверка ордеров на Бинансе, вызывается только один раз при запуске программы,
          * ордера сохраняются или обновляются в БД, потом их статусы обновляются через сокет.
          * Совмещен вызов для спота и маржина.
           */
            Task.Run(() =>
            {
                List<CaOrder> orders = _binaCaller.GetAllOrders(spotMarg);
                foreach (var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        public void CheckOrdersKucoAsync(bool spotMarg = true)
        {/******************************************************* 
          * Проверка ордеров на Кукоине, вызывается только один раз при запуске программы,
          * ордера сохраняются или обновляются в БД, потом их статусы обновляются через сокет.
          * Совмещен вызов для спота и маржина.
           */
            Task.Run(() =>
            {
                List<CaOrder> orders = _kucoCaller.GetAllOrders(spotMarg);
                foreach (var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        public void CheckOrdersHuobAsync(bool spotMarg = true)
        {
            Task.Run(() => 
            { 
                List<CaOrder> orders = _huobCaller.GetAllOrders(spotMarg);
                foreach(var o in orders)
                {
                    bool newOrUpdated = o.Save();
                }
            });
        }
        public bool StartListenOrdersBina(bool spotMarg = true)
        {
            bool b = false;
            
            _binaSocket = new BinaSocket(this);
            _binaSocket.OnMessage += OnCallerProgress;
            b = _binaSocket.InitOrdersListener(spotMarg);

            if(b)
                OnProgress?.Invoke(Name + " - Start listen spot Bina");
            else
                OnProgress?.Invoke(Name + " - Error listen spot Bina");

            return b;
        }
        public bool KeepAliveSpotBina()
        {
            if(_binaSocket == null) return false;
            return _binaSocket.KeepAlive();
        }
        public bool StartListenSpotKuco()
        {
            _kucoSocket = new KucoSocket(this);
            return _kucoSocket.InitOrdersListenerSpot();
        }
        public bool StartListenSpotHuob()
        {
            _huobSocket = new HuobSocket(this);
            return _huobSocket.InitOrdersListenerSpot();
        }
        void OnCallerProgress(string msg) => OnProgress?.Invoke(msg);
        
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
