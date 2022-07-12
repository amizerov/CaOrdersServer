using am.BL;
using System.Data;

namespace CaOrdersServer
{
    public class User
    {
        public int ID;
        public string Name;
        public string Email;
        public List<ApiKey> ApiKeys = new();
        public List<Exchange> Exchanges = new();

        public User(DataRow r)
        {
            ID = G._I(r["id"]);
            Name = G._S(r["Name"]);
            Email = G._S(r["Email"]);

            string sql = $"select * from UserKeys where usr_id = {ID}";
            DataTable dt = G.db_select(sql);

            foreach (DataRow k in dt.Rows)
            {
                ApiKey key = new ApiKey(k, this);
                ApiKeys.Add(key);

                Exchange exc = new Exchange(key);
                Exchanges.Add(exc);
            }
        }
    }

    public class Users : List<User>
    {
        public Users()
        {
            string sql = "select distinct u.* from Users u join UserKeys k on u.id = k.usr_id where IsConfirmed = 1";
            DataTable dt = G.db_select(sql);
            foreach (DataRow r in dt.Rows)
            {
                User user = new User(r);
                Add(user);
            }
        }
    }
}
