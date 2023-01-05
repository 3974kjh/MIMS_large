using Client.Infrastructure;
using Client.Model;
using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Client.Defines.MIMSMiniMessage;

namespace Client
{
    public class HomeViewModel : ViewModelBase 
    {
        private ClientEngine _engine;
        private ObservableCollection<PatientInfoModel> _searchedPatientList;
        private PatientInfoModel _selectedPatient;
        public PatientInfoModel _patientInfo;
        private string _searchKeyword;
        private bool _isOpenSearchedPatientPopup;

        public HomeViewModel(ClientEngine engine)
        {
            _engine = engine;
            _engine.NotifyManager.RegObserver(this);
        }

        public override bool Load()
        {
            this.SearchKeyword = null;

            return true;
        }

        public override void Unload()
        {
        }

        protected override void DisposeManaged()
        {
            _engine.NotifyManager.UnregObserver(this);
        }

        #region ICommand

        public ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(this.SearchedPatientInfoListCommand);
            }
        }
        #endregion

        #region ICommand_Function

        public void SearchedPatientInfoListCommand(object obj)
        {
            if (null != SearchedPatientList)
                SearchedPatientList.Clear();
            SearchedPatientList = null;

            var PatientInfolist = _engine.DataManager.GetPatientInfoListByPatientName(SearchKeyword);

            if (null == PatientInfolist || PatientInfolist.Count <= 0)
                return;

            SearchedPatientList = new ObservableCollection<PatientInfoModel>(PatientInfolist);
            IsOpenSearchedPatientPopup = true;
        }
      
        #endregion

        public bool IsOpenSearchedPatientPopup
        {
            get { return _isOpenSearchedPatientPopup; }
            set
            {
                _isOpenSearchedPatientPopup = value;
                OnPropertyChanged("IsOpenSearchedPatientPopup");
            }
        }

        #region Property
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged("SearchKeyword");
            }
        }
        #endregion

        public ObservableCollection<PatientInfoModel> SearchedPatientList
        {
            get { return _searchedPatientList; }
            set
            {
                _searchedPatientList = value;
                OnPropertyChanged("SearchedPatientList");
            }
        }

        public PatientInfoModel SelectedPatient
        {
            get { return _selectedPatient; }
            set
            {
                _selectedPatient = value;

                if (null == _selectedPatient)
                    return;

                PatientInfo = _selectedPatient;
                SearchKeyword = _selectedPatient.PatientName;

                IsOpenSearchedPatientPopup = false;
                var target = Clone(_selectedPatient);  // _patientInfo -> 클론해서 밑에 로직 탐
                _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectPatient, target));
                _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.ShowDatabaseView));

                OnPropertyChanged("SelectedPatient");
            }
        }

        public PatientInfoModel PatientInfo
        {
            get
            {
                if (null == _patientInfo)
                    _patientInfo = new PatientInfoModel();
                return _patientInfo;
            }
            set
            {
                _patientInfo = value;
                _patientInfo.CheckPatientInfoChange = true;

                OnPropertyChanged("PatientInfo");
            }
        }

        #region Function

        private PatientInfoModel Clone(PatientInfoModel source)
        {
            var props = source.GetType().GetProperties();
            var target = new PatientInfoModel();

            foreach (var prop in props)
            {
                if (prop.SetMethod != null)
                {
                    target.GetType().GetProperty(prop.Name).SetValue(target, prop.GetValue(source));
                }
            }
            return target;
        }

        private void ShowDatabaseScrean()
        {

        }
        #endregion
    }
}
