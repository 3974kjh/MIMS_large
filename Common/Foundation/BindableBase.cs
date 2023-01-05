using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Common.Foundation
{
    /// <summary>
    /// INotifyPropertyChanged 인터페이스를 구현하는 추상 클래스
    /// MVVM 패턴에서의 ViewModel과 Model은 이 클래스를 상속받아 프로퍼티 변경을 View에 알린다.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                this.VerifyPropertyName(propertyName);

                var eventHandler = this.PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        public void ChangedAllProperties()
        {
            foreach (PropertyDescriptor dec in TypeDescriptor.GetProperties(this))
            {
                OnPropertyChanged(dec.Name);
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name : " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
    }
}
