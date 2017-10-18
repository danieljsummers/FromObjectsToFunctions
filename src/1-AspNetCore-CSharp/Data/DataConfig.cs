namespace Uno.Data
{
    using RethinkDb.Driver;
    using RethinkDb.Driver.Net;

    public class DataConfig
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string AuthKey { get; set; }

        public int Timeout { get; set; }

        public string Database { get; set; }

        public IConnection CreateConnection()
        {
            var conn = RethinkDB.R.Connection();

            if (null != Hostname) { conn = conn.Hostname(Hostname); }
            if (0 != Port) { conn = conn.Port(Port); }
            if (null != AuthKey) { conn = conn.AuthKey(AuthKey); }
            if (null != Database) { conn = conn.Db(Database); }
            if (0 != Timeout) { conn = conn.Timeout(Timeout); }
            
            return conn.Connect();
        }
    }
}