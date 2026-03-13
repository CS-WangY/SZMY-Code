using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.Utils
{
    public class SqlDbService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string connStr, string sql, params SqlParameter[] param)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (param != null)
                {
                    cmd.Parameters.AddRange(param);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string connStr, string sql, params SqlParameter[] param)
        {
            SqlDataReader result = null;
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (param != null)
                {
                    cmd.Parameters.AddRange(param);
                }
                result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string connStr, string sql, params SqlParameter[] param)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (param != null)
                {
                    cmd.Parameters.AddRange(param);
                }
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                da.Fill(ds);
                return ds.Tables[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connStr"></param>
        /// <param name="sql"></param>
        /// <param name="defaultValue"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(string connStr, string sql, T defaultValue, params SqlParameter[] param)
        {
            T result = default(T);
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                //MySqlCommand cmd = new MySqlCommand(sql, conn);
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                if (param != null)
                {
                    cmd.Parameters.AddRange(param);
                }

                var r = cmd.ExecuteScalar();
                if (r == null || r is DBNull)
                {
                    result = defaultValue;
                }
                else
                {
                    try
                    {
                        result = DBReaderUtils.ConvertTo<T>(r, null, defaultValue);
                    }
                    catch (Exception)
                    {
                        result = defaultValue;
                    }
                }
            }

            return result;
        }
    }
}
