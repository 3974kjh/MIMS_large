using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server.Database
{
    /// <summary>
    /// MS SQL 관리자 클래스
    /// </summary>
    public class MsSqlDbProvider : ADatabaseProvider
    {
        public MsSqlDbProvider()
            :base()
        {
            
        }

        public static bool GenerateConnectionString(string dbName, string serverNameOrIP, int port, string userID, string password, out string connectionString)
        {
            connectionString = String.Empty;

            try
            {
                var tempString = @"Initial Catalog=" + dbName + "; Data Source=" + serverNameOrIP + "; User ID=" + userID + "; Password=" + password + "; Trusted_Connection=no";
                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = tempString;

                connectionString = tempString;
            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }

        public override bool OpenDB(string connectionString)
        {
            bool isSuccess = false;

            try
            {
                if (true == String.IsNullOrEmpty(connectionString))
                    throw new ArgumentNullException("The connection string is empty.");

                if (null != _con)
                {
                    this.CloseDB();
                }

                _con = new SqlConnection(connectionString);
                _con.StateChange += ConnectionStateChanged;
                _con.Open();

                isSuccess = true;
            }
            catch (SqlException ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + ex.Message);
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
                
                if (false == String.IsNullOrEmpty(connectionString))
                    SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + String.Format("CONNECTION STRING : {0}", connectionString));
            }

            return isSuccess;
        }

        public override DbDataReader GetDataReader(string queryString)
        {
            if (false == this.IsOpen)
                throw new ApplicationException("현재 DB 서버와 연결되어 있지 않습니다. ");

            DbDataReader reader = null;

            try
            {
                using(var cmd = new SqlCommand(queryString, (SqlConnection)_con))
                {
                    if (true == _isTransacted && null != _trans)
                        cmd.Transaction = (SqlTransaction)_trans;

                    reader = cmd.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return reader;
        }

        public override DataTable GetDataTable(string queryString)
        {
            if (false == this.IsOpen)
                throw new ApplicationException("현재 DB 서버와 연결되어 있지 않습니다. ");

            DataTable dt = new DataTable();
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(queryString, (SqlConnection)_con);

                if (da.Fill(dt) <= 0)
                    return dt;
            }
            catch (SqlException ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + ex.Message);
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return dt;
        }

        public override DbDataAdapter GetDataAdapter(string queryString)
        {
            if (false == this.IsOpen)
                throw new ApplicationException("현재 DB 서버와 연결되어 있지 않습니다. ");

            SqlDataAdapter da = null;

            try
            {
                da = new SqlDataAdapter(queryString, (SqlConnection)_con);
            }
            catch (SqlException ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + ex.Message);
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return da;
        }

        public override bool Execute(string queryString)
        {
            if (false == this.IsOpen)
                throw new ApplicationException("현재 DB 서버와 연결되어 있지 않습니다. ");

            bool success = false;

            try
            {
                var cmd = new SqlCommand(queryString, (SqlConnection)_con);

                if (true == _isTransacted && null != _trans)
                    cmd.Transaction = (SqlTransaction)_trans;

                cmd.ExecuteNonQuery();

                cmd.Dispose();

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return success;
        }

        public override object ExecuteScalar(string queryString)
        {
            if (false == this.IsOpen)
                throw new ApplicationException("현재 DB 서버와 연결되어 있지 않습니다. ");

            object result = null;

            try
            {
                var cmd = new SqlCommand(queryString, (SqlConnection)_con);

                if (true == _isTransacted && null != _trans)
                    cmd.Transaction = (SqlTransaction)_trans;

                result = cmd.ExecuteScalar();

                cmd.Dispose();
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return result;
        }

        public override bool BackupDB(string filePath)
        {
            throw new NotImplementedException();
        }

        public override bool RestoreDB(string filePath)
        {
            throw new NotImplementedException();
        }

        public static string GetDateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
