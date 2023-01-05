using Client.Defines;
using Client.DiviedView;
using Client.Infrastructure;
using Client.Model;
using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static Client.Defines.MIMSMiniMessage;

namespace Client
{
    public class ViewerScreenViewModel : ViewModelBase
    {
        private ClientEngine _engine;
        private ContentControl _imageView;
        private One one;
        private OneByTwo oneByTwo;
        private TwoByOne twoByOne;
        private TwoByTwo twoByTwo;
        private BitmapImage _firstView;
        private BitmapImage _secondView;
        private BitmapImage _thirdView;
        private BitmapImage _fourthView;
       
        private ObservableCollection<PatientInfoModel> _searchedPatientList;
        private PatientInfoModel _selectedPatient;
        public PatientInfoModel _patientInfo;
        private string _searchKeyword;
        private bool _isOpenSearchedPatientPopup;
        private ImageInfoModel _selectedImage;
        private ObservableCollection<ImageInfoModel> _imageInfoList;
        private List<ImageInfoModel> _saveEntireImageList;
        private string _patientNameAndNumber;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private double _rotateAngle = 0;

        public ViewerScreenViewModel(ClientEngine engine)
        {
            _engine = engine;
            _engine.NotifyManager.RegObserver(this);
        }

        public override bool Load()
        {
            RefreshView();

            return true;
        }

        public override void Unload()
        {
        }

        public void RefreshView()
        {
            one = new One(this);
            twoByOne = new TwoByOne(this);
            twoByTwo = new TwoByTwo(this);
            oneByTwo = new OneByTwo(this);

            ImageView = one;
        }

        protected override void DisposeManaged()
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
                case NOTIFY_MESSAGE.SelectPatient: // 환자 검색해서 선택했을 때
                    {
                        Thread.Sleep(800);

                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _patientInfo = msg.MainValue as PatientInfoModel;    // 검색한 환자 정보 받아오기

                            if (null == _patientInfo)
                            {
                                _patientInfo = null;
                                return;
                            }

                            SearchKeyword = _patientInfo.PatientName;
                            ClearCurImageLIst();      // 초기화 로직
                            UpdateImageList();
                        }));
                    }
                    break;
                case NOTIFY_MESSAGE.SelectImage: // 환자 이미지 선택했을 때
                    {
                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _saveEntireImageList = msg.SubValue as List<ImageInfoModel>;
                            ImageInfoList = new ObservableCollection<ImageInfoModel>(_saveEntireImageList);
                            SelectedImage = msg.MainValue as ImageInfoModel;
                        }));
                    }
                    break;
                case NOTIFY_MESSAGE.InsertOrEditPatient:
                    {
                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _patientInfo = msg.MainValue as PatientInfoModel;    // 검색한 환자 정보 받아오기

                            if (null == _patientInfo)
                            {
                                _patientInfo = null;
                                return;
                            }

                            this.SearchKeyword = _patientInfo.PatientName;
                        }));
                    }
                    break;
                case NOTIFY_MESSAGE.DeletePatient:
                    {
                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ClearCurImageLIst();
                            ClearAllImageLIst();
                            this.SearchKeyword = null;
                        }));
                    }
                    break;
                case NOTIFY_MESSAGE.NewPatient:
                    {
                        OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ClearCurImageLIst();
                            ClearAllImageLIst();
                            this.SearchKeyword = null;
                        }));
                    }
                    break;
            }
        }

        #region ICommand

        public ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(this.SearchedPatientInfoListCommand);
            }
        }

        public ICommand DeleteOneCommand
        {
            get
            {
                return new DelegateCommand(this.DeleteOnePatientImageInfo);
            }
        }

        public ICommand TurnRightCommand
        {
            get
            {
                return new DelegateCommand(TurnRightImage);
            }
        }

        public ICommand TurnLeftCommand
        {
            get
            {
                return new DelegateCommand(TurnLeftImage);
            }
        }

        public ICommand FlipImageHorizontallyCommand
        {
            get
            {
                return new DelegateCommand(FlipImageHorizontally);
            }
        }

        public ICommand FlipImageVerticallyCommand
        {
            get
            {
                return new DelegateCommand(FlipImageVertically);
            }
        }

        public ICommand OneView
        {
            get
            {
                return new DelegateCommand(ShowOneView);
            }
        }

        public ICommand OneByTwoView
        {
            get
            {
                return new DelegateCommand(ShowOneByTwoView);
            }
        }

        public ICommand TwoByOneView
        {
            get
            {
                return new DelegateCommand(ShowTwoByOneView);
            }
        }

        public ICommand TwoByTwoView
        {
            get
            {
                return new DelegateCommand(ShowTwoByTwoView);
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

            UpdateImageList();
        }

        public void TurnRightImage(object obj)
        {
            RotateAngle += 90;
            //if (ScaleX == -1 && ScaleY == -1)
            //{
            //    TurnClockWise();
            //    return;
            //}

            //if (ScaleX == -1 || ScaleY == -1)
            //{
            //    TurnAntiClockWise();
            //    return;
            //}

            //TurnClockWise();
        }

        public void TurnLeftImage(object obj)
        {
            RotateAngle -= 90;
           
            //if (ScaleX == -1 && ScaleY == -1)
            //{
            //    TurnAntiClockWise();
            //    return;
            //}

            //if (ScaleX == -1 || ScaleY == -1)
            //{
            //    TurnClockWise();
            //    return;
            //}

            //TurnAntiClockWise();
        }

        public void FlipImageHorizontally(object obj)
        {
            if (0 == Math.Abs(_rotateAngle)%180)
                ScaleX *= -1;
            else
                ScaleY *= -1;
        }

        public void FlipImageVertically(object obj)
        {
            if (0 == Math.Abs(_rotateAngle)%180)
                ScaleY *= -1;
            else
                ScaleX *= -1;
        }

        public void ShowOneView(object obj)
        {
            ResetViewerImage();
            ImageView = one;
        }

        public void ShowOneByTwoView(object obj)
        {
            ResetViewerImage();
            ImageView = oneByTwo;
        }

        public void ShowTwoByOneView(object obj)
        {
            ResetViewerImage();
            ImageView = twoByOne;
        }

        public void ShowTwoByTwoView(object obj)
        {
            ResetViewerImage();
            ImageView = twoByTwo;
        }

        public void ShowFirstView(BitmapImage image)
        {
            RotateAngle = 0;
            ScaleX = 1;
            ScaleY = 1;
            ImageView = one;
            FirstView = image;
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

        public double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                OnPropertyChanged("ScaleX");
            }
        }

        public double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                OnPropertyChanged("ScaleY");
            }
        }

        public string PatientNameAndNumber
        {
            get { return _patientNameAndNumber; }
            set
            {
                _patientNameAndNumber = value;
                OnPropertyChanged("PatientNameAndNumber");
            }
        }
        public ContentControl ImageView
        {
            get { return _imageView; }
            set
            {
                _imageView = value;
                OnPropertyChanged("ImageView");
            }
        }

        public BitmapImage FirstView
        {
            get { return _firstView; }
            set
            {
                _firstView = value;
                OnPropertyChanged("FirstView");
            }
        }

        public BitmapImage SecondView
        {
            get { return _secondView; }
            set
            {
                _secondView = value;
                OnPropertyChanged("SecondView");
            }
        }

        public BitmapImage ThirdView
        {
            get { return _thirdView; }
            set
            {
                _thirdView = value;
                OnPropertyChanged("ThirdView");
            }
        }

        public BitmapImage FourthView
        {
            get { return _fourthView; }
            set
            {
                _fourthView = value;
                OnPropertyChanged("FourthView");
            }
        }

        public double RotateAngle
        {
            get { return _rotateAngle; }
            set
            {
                _rotateAngle = value;
                OnPropertyChanged("RotateAngle");
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
                var target = _selectedPatient.Clone() as PatientInfoModel;
                _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectPatient, target));

                //=========================================================================================
                ClearCurImageLIst();   // 초기화 로직
                UpdateImageList();

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
                if (null == _selectedImage)
                {
                    OnPropertyChanged("SelectedImage");
                    return;
                }
                SetImagePath(_selectedImage.ImageID);
            }
        }

        #region Function

        public void MouseWeelControl(double zoom)
        {
            if (zoom > 0)
            {
                ScaleX *= 1.1;
                ScaleY *= 1.1;
            }
            else
            {
                ScaleX /= 1.1;
                ScaleY /= 1.1;

                //if (ScaleX < 1 || ScaleY < 1)
                //{
                //    ScaleX = 1;
                //    ScaleY = 1;
                //    return;
                //}
            }
        }
        private async void SetImagePath(string imageID)
        {
            if (false == _engine.DataManager.IsImageConnectedserver())
                return;

            _selectedImage = _saveEntireImageList.Where(p => p.ImageID == imageID).FirstOrDefault();

            if (null == _selectedImage)
            {
                return;
            }

            _selectedImage.ImagePath = await _engine.DataManager.GetImagePath(imageID);
            _selectedImage.OriginalImage = GetImageSourceFromFilePath(_selectedImage.ImagePath);
            FirstView = _selectedImage.OriginalImage;
            ScaleX = 1;
            ScaleY = 1;

            //using (var s = File.OpenRead(_selectedImage.ImagePath))
            //{
            //    stream = new MemoryStream();

            //    s.CopyTo(stream);
            //}

            OnPropertyChanged("SelectedImage");
        }

        //private PatientInfoModel Clone(PatientInfoModel source)
        //{
        //    var props = source.GetType().GetProperties();
        //    var target = new PatientInfoModel();

        //    foreach (var prop in props)
        //    {
        //        if (prop.SetMethod != null)
        //        {
        //            target.GetType().GetProperty(prop.Name).SetValue(target, prop.GetValue(source));
        //        }
        //    }
        //    return target;
        //}

        //private void TurnClockWise()
        //{
        //    Rotation rotation = ZeroView.Rotation;
        //    int a = 0;

        //    if (rotation == Rotation.Rotate270)
        //    {
        //        rotation = Rotation.Rotate0;
        //        a = -1;
        //    }

        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        BitmapImage image = new BitmapImage();

        //        stream.Position = 0;
        //        stream.CopyTo(memory);
        //        memory.Seek(0, SeekOrigin.Begin);
        //        image.BeginInit();

        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.StreamSource = memory;
        //        image.Rotation = rotation + 1 + a;
        //        image.EndInit();
        //        image.Freeze();
        //        ZeroView = image.Clone();
        //    }
        //}

        //private void TurnAntiClockWise()
        //{
        //    Rotation rotation = ZeroView.Rotation;
        //    int a = 0;

        //    if (rotation == Rotation.Rotate0)
        //    {
        //        rotation = Rotation.Rotate270;
        //        a = 1;
        //    }

        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        BitmapImage image = new BitmapImage();

        //        stream.Position = 0;
        //        stream.CopyTo(memory);
        //        memory.Seek(0, SeekOrigin.Begin);
        //        image.BeginInit();

        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.StreamSource = memory;
        //        image.Rotation = rotation - 1 + a;
        //        image.EndInit();
        //        image.Freeze();
        //        ZeroView = image.Clone();
        //    }
        //    return;
        //}

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
            if (null != FirstView)
                FirstView = null;
            if (null != SecondView)
                SecondView = null;
            if (null != ThirdView)
                ThirdView = null;
            if (null != FourthView)
                FourthView = null;
        }

        private async void UpdateImageList()                      // 함수명 고려
        {
            if (false == _engine.DataManager.IsConnectedserver())
                return;

            ClearAllImageLIst();                            // 기존에 담아뒀던 이미지 리스트 비워주기 ClearAllImageLIst
            await GetPatientImageInfoList();                      // 새로 받아오기
            UpdatePatientImageInfoList();                   // 이미지 뿌려주기
            ShowFirstOriginalImage();
        }

        private void ShowFirstOriginalImage()
        {
            if (null == _saveEntireImageList || 0 == _saveEntireImageList.Count)
                return;

            _selectedImage = _saveEntireImageList.First<ImageInfoModel>();
            SetImagePath(_selectedImage.ImageID);
        }

        private async Task GetPatientImageInfoList()
        {
            var list = await _engine.DataManager.GetTimageInfoList(_patientInfo.PatientNumber);
            this._saveEntireImageList = list.OrderByDescending(x => x.ImageDate).ToList();
        }

        private bool UpdatePatientImageInfoList()
        {
            if (null == _saveEntireImageList)
                return false;

            ClearCurImageLIst();

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(_saveEntireImageList);

                foreach (var imageInfo in ImageInfoList)
                {
                    imageInfo.ThumbnailImage = GetImageSourceFromFilePath(imageInfo.TImagePath);
                }
            }));

            return true;
        }

        private BitmapImage GetImageSourceFromFilePath(string filePath)
        {
            if (false == File.Exists(filePath))
            {
                return null;
            }

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

        public async void DragMultiItem(List<ImageInfoModel> imageInfos)
        {
            if (null == imageInfos || imageInfos.Count == 0)
                return;

            var count = imageInfos.Count;

            if (_imageView == one && 2 == count)
            {
                ResetViewerImage();
                ImageView = oneByTwo;
            }

            if ((_imageView == one || _imageView == oneByTwo || _imageView == twoByOne) && 2 < count)
            {
                ResetViewerImage();
                ImageView = twoByTwo;
            }

            for (var i = 0; i < imageInfos.Count; i++)
            {
                imageInfos[i].ImagePath = await _engine.DataManager.GetImagePath(imageInfos[i].ImageID);
                
                var checkimage = GetImageSourceFromFilePath(imageInfos[i].ImagePath);

                CheckSameImage(checkimage);

                if (i == 0) FirstView = checkimage;
                if (i == 1) SecondView = checkimage;
                if (i == 2) ThirdView = checkimage;
                if (i == 3) FourthView = checkimage;
            }
        }

        public async Task<BitmapImage> GetTargetImageSource(ImageInfoModel selectedItems)
        {
            selectedItems.ImagePath = await _engine.DataManager.GetImagePath(selectedItems.ImageID);
            var bitmapimage = GetImageSourceFromFilePath(selectedItems.ImagePath);

            return bitmapimage;
        }

        public void CheckSameImage(BitmapImage checkImage)
        {
            ScaleX = 1;
            ScaleY = 1;
            RotateAngle = 0;

            if (_firstView != null)
            {
                if (checkImage.Height == _firstView.Height)
                {
                    FirstView = null;
                }
            }

            if (_secondView != null)
            {
                if (checkImage.Height == _secondView.Height)
                {
                    SecondView = null;
                }
            }

            if (_thirdView != null)
            {
                if (checkImage.Height == _thirdView.Height)
                {
                    ThirdView = null;
                }
            }

            if (_fourthView != null)
            {
                if (checkImage.Height == _fourthView.Height)
                {
                    FourthView = null;
                }
            }
        }

        public void ResetViewerImage()
        {
            FirstView = null;
            SecondView = null;
            ThirdView = null;
            FourthView = null;
        }
        #endregion
    }
}
