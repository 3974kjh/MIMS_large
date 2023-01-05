using Common.Foundation;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Server.Database;
using Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Server
{
    public class ServerViewModel : ViewModelBase
    {
        private ServerEngine _engine;
        private bool _isStopped = false;             

        public ServerViewModel(ServerEngine engine)
        {
            _engine = engine;           
        }

        public override bool Load()
        {
            _engine.StartServer();
            return true;
        }
        public override void Unload()
        {
            _engine.EndServer();
        }

        public bool IsStopped
        {
            get { return _isStopped; }
            set
            {
                _isStopped = value;
                OnPropertyChanged("IsStopped");
            }
        }

        public ICommand StartServerCommand
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    StartServer();
                });
            }
        }

        public ICommand StopServerCommand
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    EndServer();
                });
            }
        }

        private void StartServer()
        {
            if (false == _engine.StartServer())
                return;

            this.IsStopped = false;          
        }

        private void EndServer()
        {
            _engine.EndServer();

            this.IsStopped = true;
        }
    }
}
