﻿using am.BL;
using Binance.Net.Objects.Models.Spot;
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
            _binaCaller = new(this); _binaCaller.OnProgress += OnOrdersProgress;
            _kucoCaller = new(this);
            _huobCaller = new(this); _huobCaller.OnProgress += OnOrdersProgress;
;        }

        public void UpdateAccount()
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
        public void UpdateOrder(string symbo, long oid)
        {/******************************************************* 
          * Вызывается при изменении изменении состояния ордера на Бинанса
          * если подписались на событие UserDataUpdates в BinaSoket
           */
            BinanceOrder? o = _binaCaller.GetOrder(symbo, oid);
            if (o == null) return;

            CaOrder order = new CaOrder(o, ID);
            order.Save();
        }
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
        public void CheckOrdersBinaAsync(bool spotmarg = true)
        {/******************************************************* 
          * Проверка ордеров на Бинансе, вызывается только один раз при запуске программы,
          * ордера сохраняются или обновляются в БД, потом их статусы обновляются через сокет.
          * Совмещен вызов для спота и маржина по параметру spotmarg = true по умолчанию спот.
           */
            if (spotmarg)
            {
                Task.Run(() =>
                    { 
                        List<BinanceOrder>? orders = _binaCaller.GetAllOrdersSpot(); 
                        if(orders == null) return;

                        foreach (var o in orders)
                        {
                            CaOrder order = new CaOrder(o, ID);
                            order.Save();
                            OnProgress?.Invoke($"Bina({Name}): spot Order {o.Id} - {o.Symbol} - state = {o.Status} / {order.state}");
                        }
                    });
            }
            else
                Task.Run(() => _binaCaller.CheckOrdersMarg());
        }
        public void CheckOrdersKucoAsync(bool spotmarg = true, bool NewOnly = false)
        {
            Task.Run(() => _kucoCaller.CheckOrdersSpot());
        }
        public void CheckOrdersHuobAsync(bool spotmarg = true, bool NewOnly = false)
        {
            Task.Run(() => _huobCaller.CheckOrdersSpot());
        }
        public bool StartListenSpotBina()
        {
            bool b = false;
            
            _binaSocket = new BinaSocket(this);
            _binaSocket.OnMessage += OnOrdersProgress;
            b = _binaSocket.InitOrdersListenerSpot();

            if(b)
                OnProgress?.Invoke(Name + " - Start listen spot Bina");
            else
                OnProgress?.Invoke(Name + " - Error listen spot Bina");

            return b;
        }
        public bool KeepAliveSpotBina()
        {
            if(_binaSocket == null) return false;
            return _binaSocket.KeepAliveSocket();
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
        void OnOrdersProgress(string msg)
        {
            OnProgress?.Invoke(msg);
        }
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
        void OnUserProgress(string msg)
        {
            OnProgress?.Invoke(msg);
        }
    }
 }
