using Common.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    public class PatientInfoModel :BindableBase, ICloneable
    {
        private int _patientNumber;
        private string _patientName;
        private string _patientResnum;
        private string _patientPhonenum;
        private bool _checkPatientInfoChange;
        private string _patientImage;

        public int PatientNumber
        {
            get { return _patientNumber; }
            set
            {
                _patientNumber = value;
                OnPropertyChanged("PatientNumber");
            }
        }
        public string PatientName
        {
            get { return _patientName; }
            set
            {
                _patientName = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientName");
            }
        }
        public string PatientResnum
        {
            get { return _patientResnum; }
            set
            {
                _patientResnum = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientResnum");
                OnPropertyChanged("PatientBirthday");
                OnPropertyChanged("PatientAge");
            }
        }

        public string PatientBirthday
        {
            get
            {
                if (null == _patientResnum || _patientResnum.Length < 14)
                    return string.Empty;

                StringBuilder birthDay = new StringBuilder();
                int century = 0;
                int.TryParse(_patientResnum.Substring(7, 1), out century);

                if (1 <= century && century <= 4)
                {
                    if (1 <= century && century <= 2)
                        birthDay.Append("19");
                    else
                        birthDay.Append("20");

                    birthDay.Append(_patientResnum.Substring(0, 2));
                    birthDay.Append("년 ");
                    birthDay.Append(_patientResnum.Substring(2, 2));
                    birthDay.Append("월 ");
                    birthDay.Append(_patientResnum.Substring(4, 2));
                    birthDay.Append("일");
                }
                else
                    birthDay.Append("");

                //string.Format("{0}년 {1}월 {2}일", _patientResnum.Substring(0,2),);

                return birthDay.ToString();
            }
        }

        public string PatientAge
        {
            get
            {
                if (null == _patientResnum)
                    return string.Empty;

                StringBuilder year = new StringBuilder();

                int patientYear = 0;
                int century = 0;
                int nowYear = DateTime.Now.Year;

                int.TryParse(_patientResnum.Substring(7, 1), out century);

                if (1 <= century && century <= 2)
                    year.Append("19");
                else
                    year.Append("20");

                year.Append(_patientResnum.Substring(0, 2));
                int.TryParse(year.ToString(), out patientYear);

                return ((nowYear - patientYear + 1).ToString() + "세");
            }
        }

        public string PatientPhonenum
        {
            get { return _patientPhonenum; }
            set
            {
                _patientPhonenum = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientPhonenum");
            }
        }
        public bool CheckPatientInfoChange
        {
            get { return _checkPatientInfoChange; }
            set
            {
                _checkPatientInfoChange = value;
            }
        }
        public string PatientImage
        {
            get 
            {
                if (null == _patientImage)
                    _patientImage = string.Empty;
                return _patientImage;
            }
            set
            {
                _patientImage = value;
                OnPropertyChanged("PatientImage");
            }
        }

        public  object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
