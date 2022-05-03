using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class ApiKey
    {
        public int ID;
        public string Exchange;
        public string Key;
        public string Secret;
        public string PassPhrase;
        public bool IsWorking {
            get {
                bool b = false;
                DataTable dt = G.db_select($"select IsWorking from UserKeys where ID={ID}");
                if(dt.Rows.Count > 0)
                    b = G._B(dt.Rows[0]["IsWorking"]);

                return b;
            } 
            set {
                string sql = $"update UserKeys set IsWorking = {(value ? 1 : 0)} where ID={ID}";
                G.db_exec(sql);
            }
        }
        public DateTime CheckedAt {
            get {
                return G._D(G.db_select($"select CheckedAt from UserKeys where ID={ID}"));
            }
            set {
                string sql = $"update UserKeys set CheckedAt = '{value.ToString("yyyy-MM-dd hh:mm:ss")}' where ID={ID}";
                G.db_exec(sql);
            }
        }
        public ApiKey(DataRow k)
        {
            ID = G._I(k["ID"]);
            Exchange = G._S(k["Exchange"]);
            Key = G._S(k["ApiKey"]);
            Secret = G._S(k["ApiSecret"]);
            PassPhrase = G._S(k["ApiPassPhrase"]);
        }
    }
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
        public void CheckApiKeys()
        {
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
        public void CheckOrdersAsynk(int exch, bool spotmarg = true, bool NewOnly = false)
        {
            switch (exch) {
                case 1:
                    if (spotmarg)
                    {
                        if(NewOnly)
                            Task.Run(() => _binaCaller.CheckOrdersSpotNewOnly());
                        else
                            Task.Run(() => _binaCaller.CheckOrdersSpot());
                    }
                    else
                        if(NewOnly)
                            Task.Run(() => _binaCaller.CheckOrdersMargNewOnly());
                        else
                            Task.Run(() => _binaCaller.CheckOrdersMarg());
                    break;
                case 2:
                    Task.Run(() => _kucoCaller.CheckOrdersSpot());
                    break;
                case 3:
                    Task.Run(() => _huobCaller.CheckOrdersSpot());
                    break;
            }
        }
        public bool StartListenSpotBina()
        {
            _binaSocket = new BinaSocket(this);
            _binaSocket.OnMessage += OnOrdersProgress;
            return _binaSocket.InitOrdersListenerSpot();
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
