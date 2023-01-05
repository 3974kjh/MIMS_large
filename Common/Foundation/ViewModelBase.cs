using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Common.Foundation
{
    /// <summary>
    /// MVVM 패턴에서의 ViewModel 베이스 추상 클래스
    /// </summary>
    public abstract class ViewModelBase : BindableBase, IDisposable, IObserver
    {
        /// <summary>
        /// 현재 ViewModel에 연결된 View 객체
        /// </summary>
        public UIElement OwnerView { get; set; }

        public virtual bool Load()
        {
            return false;
        }

        public virtual void Unload()
        {

        }

        #region IDisposable 관련 추가

        private bool isDisposed;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //Managed
                    this.DisposeManaged();
                }

                //Unmanaged
                this.DisposeUnmanaged();

                isDisposed = true;
            }
        }

        /// <summary>
        /// 관리되는 자원을 해제한다. 
        /// ViewModelBase 클래스를 상속받는 ViewModel 클래스에서는 이 메소드를 상속 받아 사용한다.
        /// </summary>
        protected virtual void DisposeManaged()
        {

        }

        /// <summary>
        /// 비관리되는 자원을 해제한다.
        /// ViewModelBase 클래스를 상속받는 ViewModel 클래스에서는 이 메소드를 상속 받아 사용한다.
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {

        }

        public virtual void Update(object sender, NotifyMsg msg)
        {
            
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~ViewModelBase()
        {
            string msg = string.Format("{0} ({1}) Finalized", this.GetType().Name, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);

            this.Dispose(false);
        }

        #endregion
    }
}
