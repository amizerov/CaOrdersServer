using am.BL;

namespace CaOrdersServer
{
    
    public static class Log
    {
        public static void Write(string msg, int usr_id)
        {
            G.db_exec($"LogIt '{msg}', {usr_id}");
        }
    }
}
