using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Defines
{
    public class MIMSMiniMessage
    {
        public enum NOTIFY_MESSAGE : int
        {
            /// <summary>
            /// 환자를 선택한다.
            /// </summary>
            SelectPatient,

            SelectImage,

            NewPatient,

            DeletePatient,

            InsertOrEditPatient,

            ShowDatabaseView,

            ShowViewerView,
        }  
    }
}