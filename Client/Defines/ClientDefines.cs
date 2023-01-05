using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Defines
{
    public class ClientDefines
    {
        public static readonly string PROGRAM_NAME = "Client";

        public static readonly string LOG_FOLDER_NAME = "log";

        public static readonly string LOG_FILE_NAME = "MIMSClient.log";

        public static readonly string SERVER_IP = "127.0.0.1";

        public static readonly string SERVER_PORT = "9002";
    }

    public class MIMSMiniMODALITYDefines
    {
        public enum IMAGE_MODALITY : int
        {
            /// <summary>
            /// 다운받을 파일이 x-ray인지 ct인지 mri인지 선택한다.
            /// </summary>
            XRAY = 1,

            CT = 2,

            MRI = 4,

            TOTAL = 7,
        }
    }
}
