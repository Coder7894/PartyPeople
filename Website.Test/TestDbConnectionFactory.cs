using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Persistence;

namespace Website.Test
{
    public class TestDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString = $"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared";
        //private readonly string _connectionString = "Data Source=TestDb;Mode=Memory;Cache=Shared";

        public IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
