using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Server.Database
{
    public delegate void ChangeDbConnectionEvent(bool isOpen);

    public interface IDatabaseProvider
    {
        ChangeDbConnectionEvent ChangeDbConnectionEventHandler { get; set; }

        bool IsOpen { get; }
        
        bool OpenDB(string connectionString);

        void CloseDB();

        DbDataReader GetDataReader(string queryString);

        DataTable GetDataTable(string queryString);

        DbDataAdapter GetDataAdapter(string queryString);

        bool Execute(string queryString);

        /// <summary>
        /// 쿼리문의 첫번째 행, 첫번째 필드를 리턴한다. 쿼리에 의해 계산된 값을
        /// 반환할 때 사용한다.
        /// </summary>
        /// <param name="queryString">하나의 행과 필드를 반환하는 쿼리문.</param>
        /// <returns>쿼리의 결과로 나온 필드값으로 가상의 결과필드일 수 있다.</returns>
        object ExecuteScalar(string queryString);

        bool LoadDBScript(string path);

        bool BeginTransaction();

        void Rollback();

        void Commit();

        bool BackupDB(string filePath);

        bool RestoreDB(string filePath);
    }
}
