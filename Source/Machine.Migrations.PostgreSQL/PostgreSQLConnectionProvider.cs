using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Machine.Migrations.Services;
using Machine.Migrations.Services.Impl;
using Npgsql;

namespace Machine.Migrations.PostgreSQL
{
    public class PostgreSQLConnectionProvider : AbstractConnectionProvider
    {
        public PostgreSQLConnectionProvider(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override IDbConnection CreateConnection(IConfiguration configuration, string key)
        {
            return new NpgsqlConnection(configuration.ConnectionStringByKey(key));
        }
    }
}
