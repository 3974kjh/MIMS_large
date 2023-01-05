using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.Database
{
    public abstract class ADatabaseProvider : IDatabaseProvider, IDisposable
    {   
        protected DbConnection _con;
        protected DbTransaction _trans;
        protected bool _isTransacted;        

        public ADatabaseProvider()
        {            
        }

        #region IDisposable 구현부

        private bool disposed = false;

        ~ADatabaseProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {

                    if (null != _con)
                    {
                        this.CloseTransaction();
                        this.CloseDB();

                        _con.Dispose();
                        _con = null;
                    }
                }

                disposed = true;
            }
        }

        #endregion

        public bool IsOpen
        {
            get
            {
                if (null == _con)
                    return false;

                return _con.State == ConnectionState.Open ? true : false;
            }
        }

        public ChangeDbConnectionEvent ChangeDbConnectionEventHandler { get; set; }

        public abstract bool OpenDB(string connectionString);

        public void CloseDB()
        {
            if (null == _con || false == this.IsOpen)
                return;

            _con.Close();
        }
        
        public abstract DbDataReader GetDataReader(string queryString);

        public abstract DataTable GetDataTable(string queryString);

        public abstract DbDataAdapter GetDataAdapter(string queryString);

        public abstract bool Execute(string queryString);

        /// <summary>
        /// 쿼리문의 첫번째 행, 첫번째 필드를 리턴한다. 쿼리에 의해 계산된 값을
        /// 반환할 때 사용한다.
        /// </summary>
        /// <param name="queryString">하나의 행과 필드를 반환하는 쿼리문.</param>
        /// <returns>쿼리의 결과로 나온 필드값으로 가상의 결과필드일 수 있다.</returns>
        public abstract object ExecuteScalar(string queryString);

        public virtual bool LoadDBScript(string path)
        {
            if (null == _con)
                return false;

            bool result = true;

            string readLine = "";

            StreamReader stream = null;

            try
            {
                FileInfo fileInfo = new FileInfo(path);
                stream = fileInfo.OpenText();

                int lineNum = 0;

                while (true)
                {
                    readLine = stream.ReadLine();
                    lineNum++;

                    if (readLine == null || readLine == "")
                        break;

                    var cmd = _con.CreateCommand();

                    cmd.CommandText = readLine;
                    cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + ex.Message);
                result = false;
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }

            return result;
        }

        public bool BeginTransaction()
        {
            try
            {
                if (null != _trans)
                    this.CloseTransaction();

                _trans = _con.BeginTransaction();
                _isTransacted = true;
                return true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DB 오류] " + e.Message);
            }

            return false;
        }

        public void Rollback()
        {
            if (null == _trans)
                return;

            _trans.Rollback();

            this.CloseTransaction();
        }

        public void Commit()
        {
            if (null == _trans)
                return;

            _trans.Commit();

            this.CloseTransaction();
        }

        protected void CloseTransaction()
        {
            if (null == _trans)
                return;

            _trans.Dispose();
            _trans = null;

            _isTransacted = false;
        }

        public abstract bool BackupDB(string filePath);

        public abstract bool RestoreDB(string filePath);

        protected void ConnectionStateChanged(object sender, StateChangeEventArgs e)
        {
            if (null != this.ChangeDbConnectionEventHandler)
                this.ChangeDbConnectionEventHandler(this.IsOpen);
        }

        /// <summary>
        /// DataReader로부터 목록을 반환한다.
        /// 
        /// 단, DB 컬럼명과 해당 객체의 프로퍼티명이 같은 경우만 해당한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<T> ConvertDataReaderToList<T>(IDataReader reader)
        {
            if (null == reader)
                return null;

            List<T> list = new List<T>();

            T obj = default(T);

            try
            {

                while (reader.Read())
                {
                    obj = Activator.CreateInstance<T>();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        if (false == Object.Equals(reader[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, reader[prop.Name], null);
                        }
                    }

                    list.Add(obj);
                }
            }
            catch { }
            finally
            {
                reader.Close();
            }

            return list;
        }
    }
}
