using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class ApiKey
    {
        public int ID = 0;
        public User User;
        public Exch Exchange;
        public string Key = "1";
        public string Secret = "1";
        public string PassPhrase = "";
        public bool IsWorking
        {
            get
            {
                bool b = false;
                DataTable dt = G.db_select($"select IsWorking from UserKeys where ID={ID}");
                if (dt.Rows.Count > 0)
                    b = G._B(dt.Rows[0]["IsWorking"]);

                return b;
            }
            set
            {
                string sql = @$"
                    update UserKeys set 
                        IsWorking = {(value ? 1 : 0)},
                        CheckedAt = getdate()
                    where ID={ID}";

                G.db_exec(sql);
            }
        }
        public ApiKey(DataRow k, User user)
        {
            ID = G._I(k["ID"]);
            Key = G._S(k["ApiKey"]);
            Secret = G._S(k["ApiSecret"]);
            Exchange = (Exch)G._I(k["ExchangeId"]);
            PassPhrase = G._S(k["ApiPassPhrase"]);
            User = user;
        }
    }

}
