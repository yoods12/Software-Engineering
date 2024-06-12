﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace TP.control
{
    internal class DBController
    {
        private string DB_Server_Info = "Data Source = localhost; " +
            "User ID = DEU; Password = 1234;";

        public DataTable GetDB(string sqltxt)
        {
            using (OracleConnection conn = new OracleConnection(DB_Server_Info))
            {
                try
                {
                    conn.Open();
                    OracleDataAdapter adapt = new OracleDataAdapter();
                    adapt.SelectCommand = new OracleCommand(sqltxt, conn);
                    DataSet ds = new DataSet();
                    DataTable dt = new DataTable();
                    adapt.Fill(ds);
                    dt.Reset();
                    dt = ds.Tables[0];
                    conn.Close();
                    return dt;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
        public void SetDB(string sqltxt)
        {
            using (OracleConnection conn = new OracleConnection(DB_Server_Info))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(sqltxt, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
        }
        public void ExecuteNonQuery(string sqltxt, OracleParameter[] parameters)
        {
            using (OracleConnection conn = new OracleConnection(DB_Server_Info))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(sqltxt, conn))
                    {
                        cmd.BindByName = true;
                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                cmd.Parameters.Add(parameter);
                            }
                        }
                        cmd.ExecuteNonQuery();

                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}