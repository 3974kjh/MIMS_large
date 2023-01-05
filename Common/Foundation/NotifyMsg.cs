using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Foundation
{
    public class NotifyMsg
    {
        private object _message;
        private object _mainValue;
        private object _subValue;
        private object _extraValue;

        public object Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public object MainValue
        {
            get { return _mainValue; }
            set { _mainValue = value; }
        }

        public object SubValue
        {
            get { return _subValue; }
            set { _subValue = value; }
        }

        public object extraValue
        {
            get { return _extraValue; }
            set { _extraValue = value; }
        }

        public NotifyMsg(object message, object mainValue = null, object subValue = null, object extraValue = null)
        {
            _message = message;
            _mainValue = mainValue;
            _subValue = subValue;
            _extraValue = extraValue;
        }
    }
}
