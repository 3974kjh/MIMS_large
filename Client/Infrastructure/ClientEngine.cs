using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Infrastructure
{
    public class ClientEngine
    {
        public ClientEngine()
        {
            this.NotifyManager = new NotifyManager();
            this.DataManager = new DataManager();
        }

        public bool IsInit { get; private set; }

        public NotifyManager NotifyManager { get; private set; }

        public DataManager DataManager { get; private set; }

        public bool Init()
        {
			if (false == this.DataManager.Init())
            {
                SimpleLogger.Instance()._OutputErrorMsg("[ERROR] DataManager 초기화에 실패하였습니다.");
                this.IsInit = false;
            }

            this.IsInit = true;
            this.DataManager.CreateFolder();

            return this.IsInit;
        }

        public void Term()
        {
            this.IsInit = false;
        }
    }    
}
