namespace SampleApp.Ado.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using SampleApp.Extensions;
    
    public static class AdoDAL
    {
        private const Int32 _defaultSizeForStringParam = 500;
        private const Byte _defaultPrecisionForDecimalParam = 18;
        private const Byte _defaultScaleForDecimalParam = 8;

        public const Int32 SQL_COMMANDTIMEOUT = 120;

        public static IEnumerable<DataRow> GetRows(String spName)
        {
            return AdoDAL.GetRows(AdoDAL.GetConnectionString(), spName);
        }

        
        public static IEnumerable<DataRow> GetRows(String connectionString, String spName)
        {
            return AdoDAL.GetRows(connectionString, spName, null);
        }

        public static IEnumerable<DataRow> GetRows(String spName, Dictionary<String, Object> parameters)
        {
            return AdoDAL.GetRows(AdoDAL.GetConnectionString(), spName, parameters);
        }

      
        public static IEnumerable<DataRow> GetRows(String connectionString, String spName, Dictionary<String, Object> parameters)
        {
            DataSet ds = AdoDAL.GetDataSet(connectionString, spName, parameters);

            return (ds != null && ds.Tables.Count > 0)
                ? ds.Tables[0].AsEnumerable() : null;
        }

        
     

        public static DataRow GetSingleRow(String spName, Dictionary<String, Object> parameters)
        {
            return AdoDAL.GetSingleRow(AdoDAL.GetConnectionString(), spName, parameters);
        }

        public static DataRow GetSingleRow(String connectionString, String spName, Dictionary<String, Object> parameters)
        {
            DataSet ds = AdoDAL.GetDataSet(connectionString, spName, parameters);

            return (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                ? ds.Tables[0].Rows[0] : null;
        }

        public static void ExecuteProc(String spName)
        {
            AdoDAL.ExecuteProc(AdoDAL.GetConnectionString(), spName);
        }

        public static void ExecuteProc(String connectionString, String spName)
        {
            AdoDAL.ExecuteProc(connectionString, spName, null);
        }

        public static void ExecuteProc(String spName, Dictionary<String, Object> parameters)
        {
            AdoDAL.ExecuteProc(AdoDAL.GetConnectionString(), spName, parameters);
        }

        public static void ExecuteProc(String connectionString, String spName, Dictionary<String, Object> parameters)
        {
            DataSet ds = null;
            AdoDAL.Execute(connectionString, spName, parameters, ref ds);
        
        }

        public static DataSet GetDataSet(String spName)
        {
            return AdoDAL.GetDataSet(AdoDAL.GetConnectionString(), spName);
        }

        public static DataSet GetDataSet(String connectionString, String spName)
        {
            return AdoDAL.GetDataSet(connectionString, spName, null);
        }

        public static DataSet GetDataSet(String spName, Dictionary<String, Object> parameters)
        {
            return AdoDAL.GetDataSet(AdoDAL.GetConnectionString(), spName, parameters);
        }

        public static DataSet GetDataSet(String connectionString, String spName, Dictionary<String, Object> parameters)
        {

            DataSet resultDataSet = new DataSet();

            AdoDAL.Execute(connectionString, spName, parameters, ref resultDataSet);

            return resultDataSet.Copy();
        }

        private static void Execute(String connectionString, String spName, Dictionary<String, Object> parameters, ref DataSet ds)
        {
            DateTime startSp = DateTime.Now;

            String tableName = spName;
            SqlCommand cmd = null;
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter adapt = null;

            #region Fill date

            try
            {
                conn.Open();

                cmd = new SqlCommand(spName);
                cmd.CommandTimeout = SQL_COMMANDTIMEOUT;
                adapt = new SqlDataAdapter();

                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = spName;
                cmd.Parameters.Clear();

                if (ds != null)
                {
                    adapt.SelectCommand = cmd;
                }
                else
                {
                    adapt.UpdateCommand = cmd;
                }

                #region Create and fill value

                // Round collection parameters and convert his to value planning DB.
                if (parameters != null)
                {
                    foreach (KeyValuePair<String, Object> kvp in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        if (kvp.Key.StartsWith("@"))
                        {
                            parameter.Direction = ParameterDirection.Output;
                            parameter.ParameterName = kvp.Key;
                            if (kvp.Value != null)
                            {
                                SqlDbType dbType = kvp.Value.GetType().GetSqlDbType();
                                parameter.SqlDbType = dbType;
                                if (dbType == SqlDbType.Decimal)
                                {
                                    parameter.Precision = AdoDAL._defaultPrecisionForDecimalParam;
                                    parameter.Scale = AdoDAL._defaultScaleForDecimalParam;
                                }
                            }
                            if (kvp.Value is String)
                            {
                                if (String.IsNullOrEmpty((String)kvp.Value))
                                {
                                    parameter.Size = AdoDAL._defaultSizeForStringParam;
                                }
                                else
                                {
                                    parameter.Size = ((String)kvp.Value).Length;
                                }
                            }
                        }
                        else
                        {
                            parameter.Direction = ParameterDirection.Input;
                            parameter.ParameterName = String.Format("@{0}", kvp.Key);
                        }
                        parameter.Value = kvp.Value ?? DBNull.Value;

                        cmd.Parameters.Add(parameter);
                    }
                }

                #endregion Create and fill value

                SqlParameterCollection parametersFromCommand = null;
                if (ds != null)
                {
                    ds.Clear();
                    adapt.Fill(ds, tableName);
                    parametersFromCommand = adapt.SelectCommand.Parameters;
                }
                else
                {
                    cmd.ExecuteNonQuery();
                    parametersFromCommand = adapt.UpdateCommand.Parameters;
                }

                foreach (SqlParameter p in parametersFromCommand)
                {
                    if (p.Direction == ParameterDirection.Output | p.Direction == ParameterDirection.InputOutput)
                    {
                        if (parameters != null && parameters.ContainsKey(p.ParameterName))
                        {
                            parameters[p.ParameterName] = p.Value == DBNull.Value ? null : p.Value;
                        }
                    }
                }

               
            }
            catch
            {
                throw;
            }
            finally
            {
                if (adapt != null)
                {
                    adapt.Dispose();
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                }

                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            #endregion Fill date
        }

        private static String GetConnectionString()
        {
            return global::System.Configuration.ConfigurationManager.ConnectionStrings["localDB"].ConnectionString;
        }
    }
}