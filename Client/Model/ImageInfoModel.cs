using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Client.Model
{
    public class ImageInfoModel : BindableBase, ICloneable
    {
        private string _imageID;
        private string _tImageID;
        private DateTime _imageDate;
        private string _PatientName;
        private long _PatientNumber;
        private int _imageCategory;
        private string _imagePath;
        private string _tImagePath;
        private BitmapImage _originalImage;
        private BitmapImage _thumbnailImage;

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        public string TImagePath
        {
            get { return _tImagePath; }
            set
            {
                _tImagePath = value;
                OnPropertyChanged("TImagePath");
            }
        }

        public BitmapImage OriginalImage
        {
            get { return _originalImage; }
            set
            {
                _originalImage = value;
                OnPropertyChanged("OriginalImage");
            }
        }

        public BitmapImage ThumbnailImage
        {
            get { return _thumbnailImage; }
            set
            {
                _thumbnailImage = value;
                OnPropertyChanged("ThumbnailImage");
            }
        }

        public string ImageID
        {
            get { return _imageID; }
            set
            {
                _imageID = value;
                OnPropertyChanged("ImageID");
            }
        }

        public string TImageID
        {
            get { return _tImageID; }
            set
            {
                _tImageID = value;
                OnPropertyChanged("TImageID");
            }
        }

        public DateTime ImageDate
        {
            get { return _imageDate; }
            set
            {
                _imageDate = value;
                OnPropertyChanged("ImageDate");
                OnPropertyChanged("ShowImageDate");
            }
        }        

        public string PatientName
        {
            get { return _PatientName; }
            set
            {
                _PatientName = value;
                OnPropertyChanged("PatientName");
            }
        }

        public string ShowImageDate
        {
            get 
            {
                return _imageDate.ToString("yyyy.MM.dd"); 
            }            
        }

        public long PatientNumber
        {
            get { return _PatientNumber; }
            set
            {
                _PatientNumber = value;
                OnPropertyChanged("PatientNumber");
            }            
        }        
        public int ImageCategory
        {
            get { return _imageCategory; }
            set
            {
                _imageCategory = value;
                OnPropertyChanged("ImageCategory");
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
