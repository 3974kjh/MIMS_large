using Client.DiviedView;
using Client.Infrastructure;
using Client.Model;
using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static Client.Defines.MIMSMiniMessage;

namespace Client
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ClientEngine _engine;
        private ContentControl _currentView;
        private HomeScreen homeFirst;
        private ViewerScreen viewerFirst;
        private DatabaseScreen databaseFirst;
        private bool _homeView;
        private bool _viewerView;
        private bool _databaseView;

        public MainWindowViewModel(ClientEngine engine)
        {          
            _engine = engine;
        }

        public override bool Load()
        {
			
			RefreshView();
			_engine.NotifyManager.RegObserver(this);
			HomeView = true;

			return true;
        }

        public override void Unload()
        {
            _engine.NotifyManager.UnregObserver(this);
        }

        public override void Update(object sender, NotifyMsg msg)
        {
            if (null == msg || this == sender)
                return;

            var message = (NOTIFY_MESSAGE)msg.Message;

            switch (message)
            {
                case NOTIFY_MESSAGE.ShowDatabaseView:
                    {
                        DatabaseView = true;
                        //CurrentView = databaseFirst;
                    }
                    break;
                case NOTIFY_MESSAGE.ShowViewerView:
                    {
                        ViewerView = true;
                        //CurrentView = viewerFirst;
                    }
                    break;
            }
        }

        public void RefreshView()
        {
            homeFirst = new HomeScreen();
            viewerFirst = new ViewerScreen();
            databaseFirst = new DatabaseScreen();

            CurrentView = homeFirst;
        }

        public ContentControl CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        public bool HomeView
        {
            get { return _homeView; }
            set
            {
                _homeView = value;
                if (true == _homeView)
                    CurrentView = homeFirst;
                OnPropertyChanged("HomeView");
            }
        }

        public bool ViewerView
        {
            get { return _viewerView; }
            set
            {
                _viewerView = value;
                if (true == _viewerView)
                    CurrentView = viewerFirst;
                OnPropertyChanged("ViewerView");
            }
        }

        public bool DatabaseView
        {
            get { return _databaseView; }
            set
            {
                _databaseView = value;
                if (true == _databaseView)
                    CurrentView = databaseFirst;
                OnPropertyChanged("DatabaseView");
            }
        }
    }
}
