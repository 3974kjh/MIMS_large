using Client.Defines;
using Client.Infrastructure;
using Client.Model;
using Common.Foundation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Client.Defines.MIMSMiniMessage;

namespace Client
{
    public class DatabaseScreenViewModel : ViewModelBase
    {
        private ClientEngine _engine;
        private ObservableCollection<PatientInfoModel> _searchedPatientList;
        private ObservableCollection<PatientInfoModel> _beforeSearchedPatientList;
        private ObservableCollection<ImageInfoModel> _imageInfoList;
        private List<ImageInfoModel> _copyImageInfoList;
        private List<ImageInfoModel> _saveEntireImageList;
        private List<PatientInfoModel> _allSelectedPatient;
        private PatientInfoModel _selectedPatient;
        private PatientInfoModel _beforeSearchedPatient;
        private ImageInfoModel _selectedImage;
        private PatientInfoModel _patientInfo;
        private string _searchKeyword;
        private bool _isOpenSearchedPatientPopup;
        private string _filePath;
        private int _curNewPatientNumber;
        private bool _isEdit = false;
        private MIMSMiniMODALITYDefines.IMAGE_MODALITY _state = 0;
        private int imageCategory = 0;
        private bool _isDescendingOrder;
        private bool _isXray;
        private bool _isCt;
        private bool _isMri;

        public DatabaseScreenViewModel(ClientEngine engine)
        {
            _engine = engine;
            _engine.NotifyManager.RegObserver(this);
        }

        #region Load & UnLoad
        public override bool Load()
        {
            return true;
        }

        public override void Unload()
        {
        }

        protected override void DisposeManaged()
        {
            _engine.NotifyManager.UnregObserver(this);
        }

        #endregion

        public override void Update(object sender, NotifyMsg msg)
        {
            if (null == msg || this == sender)
                return;

            var message = (NOTIFY_MESSAGE)msg.Message;

            switch (message)
            {
                case NOTIFY_MESSAGE.SelectPatient: // 환자 선택했을 때
                    {
                        Thread.Sleep(400);

                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _patientInfo = msg.MainValue as PatientInfoModel;    // 검색한 환자 정보 받아오기

                            if (null == _patientInfo)
                            {
                                _patientInfo = null;
                                return;
                            }

                            PatientInfo = _patientInfo;
                            SearchKeyword = _patientInfo.PatientName;

                            BeforePatientListView(_patientInfo);
                            BeforeSearchedPatientList = new ObservableCollection<PatientInfoModel>(_allSelectedPatient);
                            IsEdit = true;

                            ClearCurImageLIst();      // 초기화 로직
                            SetOffsetCategory();           // 카테고리 초기값설정

                            UpdateImageListAsync();
                        }));
                    }
                    break;
            }
        }

        #region PatientInfo ICommand
        public ICommand GetFileCommand
        {
            get
            {
                return new DelegateCommand(this.GetPortraitByFileCommand);
            }
        }
        public ICommand NewCommand
        {
            get
            {
                return new DelegateCommand(this.NewPatientInfoCommand);
            }
        }
        public ICommand InsertOrEditCommand
        {
            get
            {
                return new DelegateCommand(this.InsertOrEditPatientInfoCommand);
            }
        }
        public ICommand DeleteAllCommand
        {
            get
            {
                return new DelegateCommand(this.DeleteAllPatientInfoBySelectedCommand);
            }
        }
        public ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(this.SearchedPatientInfoListCommand);
            }
        }
        #endregion

        #region PatientInfo ICommand_Function

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

        private void GetPortraitByFileCommand(object obj)
        {
            try
            {
                if (PatientInfo.PatientNumber == CurNewPatientNumber)
                    return;

                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Multiselect = false;
                dialog.RestoreDirectory = true;

                if (false == dialog.ShowDialog())
                    return;

                if (false == dialog.CheckPathExists)
                    return;

                FilePath = dialog.FileName;

                if (false == File.Exists(FilePath))
                {
                    MessageBox.Show("파일이 없습니다.");
                    FilePath = null;
                    return;
                }

                _patientInfo.PatientImage = FilePath;

                if (false == _engine.DataManager.InsertPortrait(_patientInfo))
                    return;
            }
            catch (Exception ex)
            {
            }
        }

        private void NewPatientInfoCommand(object obj)
        {
            ClearCurImageLIst();
            ClearAllImageLIst();
            SetOffsetCategory();
            RefreshPatient();
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.NewPatient));
        }

        private void RefreshPatient()
        {
            CreateNewPatientInfo();
            IsEdit = false;
        }

        private void InsertOrEditPatientInfoCommand(object obj)
        {
            if (null == PatientInfo)
                return;

            if (string.IsNullOrEmpty(PatientInfo.PatientName) == true || string.IsNullOrEmpty(PatientInfo.PatientResnum) == true ||
                string.IsNullOrEmpty(PatientInfo.PatientBirthday) == true || string.IsNullOrEmpty(PatientInfo.PatientPhonenum) == true)
            {
                MessageBox.Show("환자 정보를 다 작성해 주세요.");
                return;
            }

            if (false == _isEdit)
            {
                if (MessageBox.Show("환자 정보를 저장하시겠습니까?", "환자 정보 저장", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            if (true == _isEdit)
            {
                if (MessageBox.Show("환자 정보를 수정하시겠습니까?", "환자 정보 수정", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            if (true == this.PatientInfo.CheckPatientInfoChange)
            {
                if (true == _isEdit)
                {
                    MessageBox.Show("변경사항이 없습니다.");
                    return;
                }
                if (false == _isEdit)
                {
                    MessageBox.Show("이미 존재하는 환자입니다.");
                    return;
                }

            }

            if (false == _isEdit)
            {
                if (false == _engine.DataManager.InsertPatientInfo(_patientInfo))
                    return;

                this.CurNewPatientNumber = _engine.DataManager.GetNewPatientNumber() + 1;
                IsEdit = true;
            }

            else
            {
                if (false == _engine.DataManager.EditPatientInfo(_patientInfo))
                    return;
            }

            BeforePatientListView(_patientInfo);
            BeforeSearchedPatientList = new ObservableCollection<PatientInfoModel>(_allSelectedPatient);

            _patientInfo.CheckPatientInfoChange = true;
            SearchKeyword = _patientInfo.PatientName;
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.InsertOrEditPatient, _patientInfo));
        }

        private void DeleteAllPatientInfoBySelectedCommand(object obj)
        {
            if (null == PatientInfo)
                return;

            if (MessageBox.Show("환자 정보를 삭제하시겠습니까?", "환자 정보 삭제", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            if (false == _engine.DataManager.DeleteAllImageInfoLIst(_patientInfo.PatientNumber))
                return;

            if (false == _engine.DataManager.DeletePatientInfoList(_patientInfo.PatientNumber))
                return;

            DeleteBeforePatientListView(_patientInfo);
            BeforeSearchedPatientList = new ObservableCollection<PatientInfoModel>(_allSelectedPatient);
            ClearCurImageLIst();
            ClearAllImageLIst();
            RefreshPatient();

            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.DeletePatient));
        }
        #endregion

        #region PatientInfo SearchedPatientList, BeforeSearchedPatientList, SelectedPatient, BeforeSelectedPatient 
        public ObservableCollection<PatientInfoModel> SearchedPatientList
        {
            get { return _searchedPatientList; }
            set
            {
                _searchedPatientList = value;
                OnPropertyChanged("SearchedPatientList");
            }
        }

        public ObservableCollection<PatientInfoModel> BeforeSearchedPatientList
        {
            get { return _beforeSearchedPatientList; }
            set
            {
                _beforeSearchedPatientList = value;
                OnPropertyChanged("BeforeSearchedPatientList");
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

                BeforePatientListView(_selectedPatient);
                BeforeSearchedPatientList = new ObservableCollection<PatientInfoModel>(_allSelectedPatient);
                var target = _selectedPatient.Clone() as PatientInfoModel;
                _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectPatient, target));
                IsEdit = true;

                ClearCurImageLIst();            // 초기화 로직
                SetOffsetCategory();           // 카테고리 초기값설정

                UpdateImageListAsync();

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

        public PatientInfoModel BeforeSelectedPatient
        {
            get { return _beforeSearchedPatient; }
            set
            {
                _beforeSearchedPatient = value;

                OnPropertyChanged("BeforeSelectedPatient");
            }
        }
        #endregion

        #region PatientInfo Property

        public bool IsOpenSearchedPatientPopup
        {
            get { return _isOpenSearchedPatientPopup; }
            set
            {
                _isOpenSearchedPatientPopup = value;
                OnPropertyChanged("IsOpenSearchedPatientPopup");
            }
        }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged("SearchKeyword");
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public int CurNewPatientNumber
        {
            get { return _curNewPatientNumber; }
            set
            {
                _curNewPatientNumber = value;
                OnPropertyChanged("CurNewPatientNumber");
            }
        }

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                _isEdit = value;
                OnPropertyChanged("IsEdit");
            }
        }
        #endregion

        #region PatientInfo Function

        private void CreateNewPatientInfo()
        {
            this.CurNewPatientNumber = _engine.DataManager.GetNewPatientNumber() + 1;

            this.PatientInfo = new PatientInfoModel();
            this.SearchKeyword = null;
            this.PatientInfo.PatientNumber = CurNewPatientNumber;
        }

        private void BeforePatientListView(PatientInfoModel target)
        {
            if (null == target)
                return;

            // if (null == _allSelectedPatient)
            // {
            //     _allSelectedPatient = new List<PatientInfoModel>();
            // }

            //var foundedPatientInfo =  _allSelectedPatient.Where(patient => patient.PatientNumber == target.PatientNumber).FirstOrDefault();

            // if (null == foundedPatientInfo)
            // {
            //     _allSelectedPatient.Add(target);
            //     _allSelectedPatient.Reverse();
            //     return;
            // }

            // foundedPatientInfo = target.Clone() as PatientInfoModel;


            int flag = 0;

            if (null == _allSelectedPatient)
            {
                _allSelectedPatient = new List<PatientInfoModel>();
                _allSelectedPatient.Add(target);
            }

            for (int i = 0; i < _allSelectedPatient.Count; i++)
            {
                if (_allSelectedPatient[i].PatientNumber == target.PatientNumber)
                {
                    flag = 1;

                    if (_allSelectedPatient[i].PatientPhonenum != target.PatientPhonenum
                        || _allSelectedPatient[i].PatientResnum != target.PatientResnum
                        || _allSelectedPatient[i].PatientName != target.PatientName)
                    {
                        _allSelectedPatient[i] = target;
                    }
                    else
                        break;
                }
            }

            if (flag == 0)
            {
                _allSelectedPatient.Insert(0, target);
            }
        }

        private void DeleteBeforePatientListView(PatientInfoModel target)
        {
            for (int i = 0; i < _allSelectedPatient.Count; i++)
            {
                if (_allSelectedPatient[i].PatientNumber == target.PatientNumber)
                {
                    _allSelectedPatient.RemoveAt(i);
                    break;
                }
            }
        }
        #endregion

        //==============================================================================================================================

        #region ImageInfo ICommand
        public ICommand GetImageFileCommand
        {
            get
            {
                return new DelegateCommand(this.GetPatientImage);
            }
        }

        public ICommand DeleteOneCommand
        {
            get
            {
                return new DelegateCommand(this.DeleteOnePatientImageInfo);
            }
        }

        public ICommand DescendingOrderCommand
        {
            get
            {
                return new DelegateCommand(this.DescendingOrderToPatientImage);
            }
        }

        public ICommand AscendingOrderCommand
        {
            get
            {
                return new DelegateCommand(this.AscendingOrderToPatientImage);
            }
        }

        public ICommand TotalCommand
        {
            get
            {
                return new DelegateCommand(this.TotalImageLIst);
            }
        }

        public ICommand XrayCommand
        {
            get
            {
                return new DelegateCommand(this.XrayImageLIst);
            }
        }

        public ICommand CtCommand
        {
            get
            {
                return new DelegateCommand(this.CtImageLIst);
            }
        }

        public ICommand MriCommand
        {
            get
            {
                return new DelegateCommand(this.MriImageLIst);
            }
        }
        #endregion

        #region ImageInfo ICommand_Function
        private void GetPatientImage(object obj)
        {
            try
            {
                if (string.IsNullOrEmpty(PatientInfo.PatientName) == true || string.IsNullOrEmpty(PatientInfo.PatientResnum) == true ||
                string.IsNullOrEmpty(PatientInfo.PatientBirthday) == true || string.IsNullOrEmpty(PatientInfo.PatientPhonenum) == true)
                {
                    MessageBox.Show("환자 정보가 없습니다.");
                    return;
                }
                //if (imageCategory < 1)
                //{
                //    MessageBox.Show("카테고리를 선택해 주세요.");
                //    return;
                //}

                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Multiselect = false;
                dialog.RestoreDirectory = true;

                if (false == dialog.ShowDialog())
                    return;
                if (false == dialog.CheckPathExists)
                    return;

                var filePath = dialog.FileName;

                CreateNewImageInfo(filePath, imageCategory);
            }
            catch (Exception ex)
            {
            }
        }

        private void DeleteOnePatientImageInfo(object obj)
        {
            var imageModel = obj as ImageInfoModel;

            if (null == imageModel)
                return;

            _selectedImage = imageModel;

            if (MessageBox.Show("환자 정보를 삭제하시겠습니까?", "환자 정보 삭제", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            if (false == _engine.DataManager.DeleteOneImageInfoLIst(_selectedImage.TImageID))
                return;

            UpdateImageListAsync();
        }

        private void DescendingOrderToPatientImage(object obj)
        {
            if (_patientInfo == null)
                return;

            if (ImageInfoList == null)
                return;

            var descending = ImageInfoList.OrderByDescending(x => x.ImageDate).ToList();
            _copyImageInfoList = descending;

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(descending);
            }));
        }

        private void AscendingOrderToPatientImage(object obj)
        {
            if (_patientInfo == null)
                return;

            if (ImageInfoList == null)
                return;

            var accending = ImageInfoList.OrderBy(x => x.ImageDate).ToList();
            _copyImageInfoList = accending;

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(accending);
            }));
        }

        private void TotalImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void XrayImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void CtImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void MriImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }
        #endregion

        #region ImageInfo ImageInfoList, SelectedImage
        public ObservableCollection<ImageInfoModel> ImageInfoList
        {
            get { return _imageInfoList; }
            set
            {
                _imageInfoList = value;
                OnPropertyChanged("ImageInfoList");
            }
        }

        public ImageInfoModel SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                OnPropertyChanged("SelectedImage");
            }
        }
        #endregion

        #region ImageInfo Property

        public bool IsDescendingOrder
        {
            get { return _isDescendingOrder; }
            set
            {
                _isDescendingOrder = value;
                OnPropertyChanged("IsDescendingOrder");
            }
        }

        public bool IsXray
        {
            get { return _isXray; }
            set
            {
                _isXray = value;
                if (_isXray == true)
                    imageCategory = 1;
                OnPropertyChanged("IsXray");
            }
        }

        public bool IsCt
        {
            get { return _isCt; }
            set
            {
                _isCt = value;
                if (_isCt == true)
                    imageCategory = 2;
                OnPropertyChanged("IsCt");
            }
        }

        public bool IsMri
        {
            get { return _isMri; }
            set
            {
                _isMri = value;
                if (_isMri == true)
                    imageCategory = 4;
                OnPropertyChanged("IsMri");
            }
        }

        public MIMSMiniMODALITYDefines.IMAGE_MODALITY State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("Total");
                OnPropertyChanged("Xray");
                OnPropertyChanged("Ct");
                OnPropertyChanged("Mri");
            }
        }

        public bool Total
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL); }
            set
            {
                State = value ? MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL : 0;
            }
        }

        public bool Xray
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY;
            }
        }

        public bool Ct
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT;
            }
        }

        public bool Mri
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI;
            }
        }
        #endregion

        #region ImageInfo Function

        private void SetOffsetCategory()
        {
            State = MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL;
            IsDescendingOrder = true;
            IsXray = true;
        }

        private void ClearAllImageLIst()
        {
            if (null != this._saveEntireImageList)
            {
                this._saveEntireImageList.Clear();
                this._saveEntireImageList = null;
            }
        }

        private void ClearCurImageLIst()
        {
            if (null != ImageInfoList)
            {
                ImageInfoList.Clear();
                ImageInfoList = null;
            }
        }

        private async void CreateNewImageInfo(string filePath, int imageCategory)
        {
            await _engine.DataManager.InsertImageInfo(filePath, imageCategory, _patientInfo);

            await UpdateImageList();
        }

        private async void UpdateImageListAsync()
        {
            await UpdateImageList();
        }

        private async Task UpdateImageList()                      // 함수명 고려
        {
            ClearAllImageLIst();                            // 기존에 담아뒀던 이미지 리스트 비워주기 ClearAllImageLIst
            await GetPatientImageInfoList();                      // 새로 받아오기
            UpdatePatientImageInfoList();                   // 이미지 뿌려주기
        }

        private List<ImageInfoModel> FilteringImageInfoList()
        {
            var filteringImageList = new List<ImageInfoModel>();

            foreach (var imageInfo in _saveEntireImageList)
            {
                if (false == State.HasFlag((MIMSMiniMODALITYDefines.IMAGE_MODALITY)imageInfo.ImageCategory))
                    continue;

                filteringImageList.Add(imageInfo);
            }

            return filteringImageList;
        }

        private bool UpdatePatientImageInfoList()
        {
            if (null == _saveEntireImageList)
                return false;

            ClearCurImageLIst();
            IsDescendingOrder = true;                                            // 초기값 최신순 버튼 클릭상태

            var selectedCategoryList = FilteringImageInfoList();

            _copyImageInfoList = selectedCategoryList;

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(selectedCategoryList);

                foreach (var imageInfo in ImageInfoList)
                {
                    imageInfo.ThumbnailImage = GetImageSourceFromFilePath(imageInfo.TImagePath);
                }
            }));

            return true;
        }

        private BitmapImage GetImageSourceFromFilePath(string filePath)
        {
            using (var s = File.OpenRead(filePath))
            {
                BitmapImage image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = s;
                image.EndInit();
                image.Freeze();

                return image.Clone();
            }
        }

        public async void ShowBeforePatientInfo()
        {
            PatientInfo = BeforeSelectedPatient;
            SearchKeyword = PatientInfo.PatientName;

            IsEdit = true;

            ClearCurImageLIst();      // 초기화 로직
            SetOffsetCategory();           // 카테고리 초기값설정
            
            if (false == _engine.DataManager.IsConnectedserver())
                return;
            
            await UpdateImageList();

            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectPatient, PatientInfo));
        }

        private async Task GetPatientImageInfoList()
        {
            var list =  await _engine.DataManager.GetTimageInfoList(_patientInfo.PatientNumber);
            this._saveEntireImageList = list.OrderByDescending(x => x.ImageDate).ToList();
        }

        public void ShowImageViewer()
        {
            if (false == _engine.DataManager.IsConnectedserver())
                return;

            var target = IClone(_selectedImage);
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectImage, target, _copyImageInfoList));
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.ShowViewerView));
        }

        private ImageInfoModel IClone(ImageInfoModel source)
        {
            var props = source.GetType().GetProperties();
            var target = new ImageInfoModel();

            foreach (var prop in props)
            {
                if (prop.SetMethod != null)
                {
                    target.GetType().GetProperty(prop.Name).SetValue(target, prop.GetValue(source));
                }
            }
            return target;
        }
        #endregion
    }
}