using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerBase.Configuration
{
    public class Database
    {
        public const string SectionKey = nameof(Database);
        public string DatabaseName { get; set; }
        public string Host { get; set; }

        public string MysqlConnectionString => new MySqlConnectionStringBuilder()
        {
            AllowZeroDateTime = true,
            CharacterSet = "utf8mb4",
            Database = DatabaseName,
            Password = Password,
            Port = Port,
            Server = Host,
            SslMode = MySqlSslMode.None,
            UserID = UserId,
        }.ConnectionString;

        public string Password { get; set; }
        public uint Port { get; set; }
        public string UserId { get; set; }
    }
}