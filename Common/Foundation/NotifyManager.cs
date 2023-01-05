using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; // ManualResetEvent

namespace Common.Foundation
{
    public class QueueMsg
    {
        private Object _sender;
        public Object Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }

        private NotifyMsg _msg;
        public NotifyMsg Msg
        {
            get { return _msg; }
            set { _msg = value; }
        }
    }

    public class NotifyManager
    {
        protected List<IObserver> _observerList;
        protected volatile Queue<QueueMsg> _msgQueue = new Queue<QueueMsg>();
        protected ManualResetEvent _observerQueueResetEvent = new ManualResetEvent(true);
        private bool _isSetObserverQueueResetEvent;

        public List<IObserver> ObserverList
        {
            get
            {
                if (null == _observerList)
                    _observerList = new List<IObserver>();

                return _observerList;
            }
        }

        public ManualResetEvent ObserverQueueResetEvent
        {
            get { return _observerQueueResetEvent; }
        }

        public bool IsSetObserverQueueResetEvent
        {
            get { return _isSetObserverQueueResetEvent; }
            set { _isSetObserverQueueResetEvent = value; }
        }


        public NotifyManager()
        {
        }

        public void RegObserver(IObserver observer)
        {
            ObserverList.Add(observer);
            System.Diagnostics.Debug.WriteLine("Add observer=" + observer.ToString() + ", ObserverList Count=" + ObserverList.Count.ToString());
        }

        public void UnregObserver(IObserver observer)
        {
            ObserverList.Remove(observer);
            System.Diagnostics.Debug.WriteLine("Remove observer=" + observer.ToString() + ", ObserverList Count=" + ObserverList.Count.ToString());
        }

        public void Notify(IObserver sender, NotifyMsg msg)
        {
            QueueMsg qmsg = new QueueMsg() { Sender = sender, Msg = msg };
            _msgQueue.Enqueue(qmsg);

            IsSetObserverQueueResetEvent = true;
            ObserverQueueResetEvent.Set();
        }

        bool _running = false;

        public void NotifyQueue()
        {
            if (_msgQueue.Count <= 0)
                return;

            if (true == _running)
                return;

            _running = true;

            QueueMsg m = _msgQueue.Dequeue();

            if (null == m)
            {
                _running = false;
                return;
            }

            int nCnt = ObserverList.Count;
            for (int i = nCnt - 1; i >= 0; i--)
            {
                ObserverList[i].Update(m.Sender, m.Msg);
            }

            _running = false;
        }
    }
}
