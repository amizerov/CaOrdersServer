using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaOrdersServer
{
    public class Msg
    {
        public int type;
        public User user;
        public Exch exch;
        public string src = "";
        public string msg = "";
        public Msg(int t, User u, Exch e, string s, string m)
        {
            type = t; user = u; exch = e; src = s; msg = m;
        }
        public static void Send(int t, User u, Exch e, string s, string m)
        {
            LogToForm.Write(new Msg(t, u, e, s, m));
        }
    }
    class LogToForm
    {
        static Action<Msg>? _write;
        public static void Write(Msg msg) => _write?.Invoke(msg);
        static object _lockFlag = new object();
        static LogToForm? _instance;
        public LogToForm()
        {
        }
        public static LogToForm Instance
        {
            get
            {
                lock (_lockFlag)
                {
                    if (_instance == null)
                        _instance = new LogToForm();
                }
                return _instance;
            }
        }
        public void Init(Action<Msg> write) 
        { 
            _write = write;
        }
    }
}
