namespace Dos.Data
{
    using Newtonsoft.Json;
    using RethinkDb.Driver;
    using RethinkDb.Driver.Net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static RethinkDb.Driver.Net.Connection;

    public class DataConfig
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string AuthKey { get; set; }

        public int Timeout { get; set; }

        public string Database { get; set; }

        private IEnumerable<Func<Builder, Builder>> Blocks
        {
            get
            {
                yield return builder => null == Hostname ? builder : builder.Hostname(Hostname);
                yield return builder => 0 == Port ? builder : builder.Port(Port);
                yield return builder => null == AuthKey ? builder : builder.AuthKey(AuthKey);
                yield return builder => null == Database ? builder : builder.Db(Database);
                yield return builder => 0 == Timeout ? builder : builder.Timeout(Timeout);
            }
        }

        public IConnection CreateConnection() =>
            Blocks.Aggregate(RethinkDB.R.Connection(), (builder, block) => block(builder)).Connect();

        public static DataConfig FromJson(string json) => JsonConvert.DeserializeObject<DataConfig>(json);
    }
}