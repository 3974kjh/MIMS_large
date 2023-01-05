using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Common.Foundation
{
    public class SimpleLogger
    {
        public ListBox LogListBox { get; set; }

        public class LogMessageModel
        {
            public SimpleLogger.LOG_LEVEL Level { get; set; }

            public string Message { get; set; }                           
        }

        public delegate void LogWirittenEvent(LogMessageModel logMessage);
        public LogWirittenEvent LogWirittenEventHander;     

        public enum LOG_LEVEL
        {
            TRACE, DEBUG, INFO, WARN, ERROR, FATAL
        }

        public enum LOG_FILE_CREAE_DATE_TERM
        {
            /// <summary>
            /// 1주일 전
            /// </summary>
            BeforeOneWeeks = 1,
            /// <summary>
            /// 2주일 전
            /// </summary>
            BeforeTwoWeeks = 2,
            /// <summary>
            /// 1개월 전
            /// </summary>
            BeforeOneMonth = 3,
            /// <summary>
            /// 3개월 전
            /// </summary>
            BeforeThreeMonths = 4,
            /// <summary>
            /// 6개월 전
            /// </summary>
            BeforeSixMonths = 5,
        }

        private static readonly object _locker = new object();
        private string _logFilePath;

        private static volatile SimpleLogger _instance;

        public static SimpleLogger Instance()
        {
            if (null == _instance)
            {
                lock (_locker)
                {
                    _instance = new SimpleLogger();
                }
            }

            return _instance;
        }

        public string LastMsg { get; private set; }

        public bool Init(string logFilePath)
        {
            try
            {
                _logFilePath = logFilePath;

                var directory = System.IO.Path.GetDirectoryName(_logFilePath);

                if (false == System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                //해당 로그 파일이 오늘날짜가 아니면 해당 마지막 수정일로 파일명을 변경한다.
                var fileInfo = new System.IO.FileInfo(_logFilePath);
                if (true == fileInfo.Exists && fileInfo.LastWriteTime.Date != DateTime.Now.Date)
                {
                    try
                    {
                        System.IO.File.Copy(_logFilePath, System.IO.Path.Combine(directory, String.Format("{0}-{1}.log", _logFilePath, fileInfo.LastWriteTime.ToString("yyyy-MM-dd"))));
                        System.IO.File.Delete(_logFilePath);
                    }
                    catch { }
                }

                if (false == System.IO.File.Exists(_logFilePath))
                {
                    //오늘자 빈 로그파일 생성
                    System.IO.File.Create(_logFilePath).Dispose();
                }

                LogListBox = new ListBox();

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void Terminate()
        {
            var directory = System.IO.Path.GetDirectoryName(_logFilePath);

            if (true == System.IO.Directory.Exists(directory))
            {
                this.ManageLogFileFolderAsCreateDate(directory);
            }
        }

        public void _OutputDebugMsg(string msg)
        {
            this.WriteLog(LOG_LEVEL.DEBUG, msg);
        }

        public void _OutputDebugMsg(string format, params object[] args)
        {
            var message = String.Format(format, args);

            this.WriteLog(LOG_LEVEL.DEBUG, message);
        }

        public void _OutputErrorMsg(string msg)
        {
            this.WriteLog(LOG_LEVEL.ERROR, msg);
        }

        public void _OutputErrorMsg(string format, params object[] args)
        {
            var message = String.Format(format, args);

            this.WriteLog(LOG_LEVEL.ERROR, message);
        }

        public void _OutputMsg(LOG_LEVEL level, string format, params object[] args)
        {
            var message = String.Format(format, args);

            this.WriteLog(level, message);
        }

        protected void WriteLog(LOG_LEVEL level, string message)
        {
            lock (_locker)
            {
                try
                {
                    using (var streamWriter = System.IO.File.AppendText(_logFilePath))
                    {
                        this.LastMsg = message;

                        string fullLogMsg = String.Format("{0} [{1}] {2} - {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), System.Threading.Thread.CurrentThread.ManagedThreadId, level, message);
                        streamWriter.WriteLine(fullLogMsg);                        
                        LogListBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            LogListBox.Items.Add(fullLogMsg);
                            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                        }));

                        if (null != this.LogWirittenEventHander)
                        {
                            this.LogWirittenEventHander(new LogMessageModel()
                            {
                                Level = level,
                                Message = fullLogMsg
                            });

                            System.Diagnostics.Debug.WriteLine(message);
                        }
                    }
                }
                catch { }
            }
        }

        private bool ManageLogFileFolderAsCreateDate(string folderPath, LOG_FILE_CREAE_DATE_TERM dateTerm = LOG_FILE_CREAE_DATE_TERM.BeforeOneMonth, bool isDeleteIfInvalid = true)
        {
            if (false == System.IO.Directory.Exists(folderPath))
            {
                // 경로를 생성한다.                
                if (false == System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);
            }

            var dirInfo = new System.IO.DirectoryInfo(folderPath);

            System.IO.FileInfo[] fileInfos = dirInfo.GetFiles();

            if (null == fileInfos || fileInfos.Count() < 1)
                return true;

            var today = DateTime.Now;
            DateTime basisDate = DateTime.MinValue;
            switch (dateTerm)
            {
                case LOG_FILE_CREAE_DATE_TERM.BeforeOneWeeks:
                    basisDate = today.AddDays(-7);
                    break;

                case LOG_FILE_CREAE_DATE_TERM.BeforeTwoWeeks:
                    basisDate = today.AddDays(-14);
                    break;

                case LOG_FILE_CREAE_DATE_TERM.BeforeThreeMonths:
                    basisDate = today.AddMonths(-3);
                    break;

                case LOG_FILE_CREAE_DATE_TERM.BeforeSixMonths:
                    basisDate = today.AddMonths(-6);
                    break;

                case LOG_FILE_CREAE_DATE_TERM.BeforeOneMonth:
                default:
                    basisDate = today.AddMonths(-1);
                    break;
            }

            if (DateTime.MinValue == basisDate)
                return false;

            var candidateFileList = fileInfos.Where(data => data.LastWriteTime.Date <= basisDate.Date).OrderBy(d => d.LastWriteTime).Select(f => f.FullName).ToList<string>();

            if (candidateFileList.Count < 1)
                return true;

            foreach (var filePath in candidateFileList)
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch { }
            }

            return true;
        }
    }
}
