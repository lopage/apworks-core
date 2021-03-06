﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Apworks.Tests.Integration.Fixtures
{
    public class PostgreSQLFixture
    {
        public const string ConnectionString = "User ID=test;Password=oe9jaacZLbR9pN;Host=localhost;Port=5432;Database=test;";

        public PostgreSQLFixture()
        {
            if (CheckTableExists("Customers") || CheckTableExists("Addresses"))
            {
                DropTable();
            }

            CreateTable();
        }

        public void ClearTable()
        {
            ExecuteCommand("DELETE FROM public.\"Addresses\"");
            ExecuteCommand("DELETE FROM public.\"Customers\"");
        }

        private static bool CheckTableExists(string tableName)
        {
            var sql = $"SELECT * FROM information_schema.tables WHERE table_name = '{tableName}'";
            var tableExists = false;
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var cmd = new NpgsqlCommand(sql))
                {
                    if (cmd.Connection == null)
                        cmd.Connection = con;
                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    lock (cmd)
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            try
                            {
                                if (rdr != null && rdr.HasRows)
                                    tableExists = true;
                                else
                                    tableExists = false;
                            }
                            catch
                            {
                                tableExists = false;
                            }
                        }
                    }
                }
            }
            return tableExists;
        }

        private static void DropTable()
        {
            ExecuteCommand("DROP TABLE public.\"Addresses\"");
            ExecuteCommand("DROP TABLE public.\"Customers\"");
        }

        private static void CreateTable()
        {
            var createScript = @"
CREATE TABLE public.""Customers""
(
  ""Id"" integer NOT NULL,
  ""Email"" text,
  ""Name"" text,
  CONSTRAINT ""PK_Customers"" PRIMARY KEY(""Id"")
)
WITH(
  OIDS = FALSE
);

ALTER TABLE public.""Customers""
  OWNER TO test;

CREATE TABLE ""Addresses"" (
    ""Id"" serial NOT NULL,
    ""City"" text,
    ""Country"" text,
    ""CustomerId"" int4,
    ""State"" text,
    ""Street"" text,
    ""ZipCode"" text,
    CONSTRAINT ""PK_Addresses"" PRIMARY KEY(""Id""),
    CONSTRAINT ""FK_Addresses_Customers_CustomerId"" FOREIGN KEY(""CustomerId"") REFERENCES ""Customers""(""Id"") ON DELETE NO ACTION
);

CREATE INDEX ""IX_Addresses_CustomerId"" ON ""Addresses"" (""CustomerId"");

ALTER TABLE public.""Addresses""
  OWNER TO test;

";
            ExecuteCommand(createScript);
        }

        private static void ExecuteCommand(string sql)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    con.Open();
                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (PostgresException)
            { }
        }
    }
}
