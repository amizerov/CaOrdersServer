using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class ApiKey
    {
        public int ID = 0;
        public string Exchange = "";
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
                string sql = $"update UserKeys set IsWorking = {(value ? 1 : 0)} where ID={ID}";
                G.db_exec(sql);
            }
        }
        public DateTime CheckedAt
        {
            get
            {
                return G._D(G.db_select($"select CheckedAt from UserKeys where ID={ID}"));
            }
            set
            {
                string sql = $"update UserKeys set CheckedAt = '{value.ToString("yyyy-MM-dd hh:mm:ss")}' where ID={ID}";
                G.db_exec(sql);
            }
        }
        public ApiKey() { }
        public ApiKey(DataRow k)
        {
            ID = G._I(k["ID"]);
            Exchange = G._S(k["Exchange"]);
            Key = G._S(k["ApiKey"]);
            Secret = G._S(k["ApiSecret"]);
            PassPhrase = G._S(k["ApiPassPhrase"]);
        }
    }

}
