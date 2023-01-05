using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common.Foundation;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly App _app;
        private volatile Thread _notifyQueueThread;
        public bool IsStopNotifyQueue { get; private set; }
        public MainWindowViewModel MM {get; set;}

        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.WorkArea.Height;

            _app = Application.Current as App;

            //알림 관리자의 큐 스레드를 시작한다.
            this._notifyQueueThread = new Thread(NotifyQueueCallBack);
            this._notifyQueueThread.Start();
            this.MM = new MainWindowViewModel(_app.Engine);
            this.DataContext = MM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(3000);
            this.MM.Load();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //알림 큐 스레드를 종료한다.
            if (null != _notifyQueueThread)
            {
                this.IsStopNotifyQueue = true;
                _notifyQueueThread.Join();
                _notifyQueueThread = null;
            }
        }

        private void NotifyQueueCallBack()
        {
            if (false == _app.Engine.IsInit)
                return;

            try
            {
                while (false == this.IsStopNotifyQueue)
                {
                    if (_app.Engine.NotifyManager.ObserverQueueResetEvent.WaitOne(200))
                    {
                        _app.Engine.NotifyManager.IsSetObserverQueueResetEvent = false;
                    }

                    _app.Engine.NotifyManager.NotifyQueue();

                    if (false == _app.Engine.NotifyManager.IsSetObserverQueueResetEvent)
                        _app.Engine.NotifyManager.ObserverQueueResetEvent.Reset();
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }
        }

        //---------------------------------------<< window title bar control >>---------------------------------------------------

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Mimimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                main.BorderThickness = new Thickness(0);
                rectMax.Visibility = Visibility.Hidden;
                rectMin.Visibility = Visibility.Visible;
            }
            else
            {
                main.BorderThickness = new Thickness(1);
                rectMax.Visibility = Visibility.Visible;
                rectMin.Visibility = Visibility.Hidden;
            }
        }

        private Point startPos;

        private void System_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2)
                {
                    var point = PointToScreen(e.GetPosition(null));

                    this.WindowState = WindowState.Normal;

                    this.Left = point.X - this.ActualWidth / 2;
                    this.Top = point.Y - border.ActualHeight / 2;
                }
                DragMove();
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
                                        int nReserved, IntPtr hWnd, IntPtr prcRect);

        private void System_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount >= 2)
                {
                    this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
                }
                else
                {
                    startPos = e.GetPosition(null);
                }
            }

            else if (e.ChangedButton == MouseButton.Right)
            {
                var pos = PointToScreen(e.GetPosition(this));
                IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                IntPtr hMenu = GetSystemMenu(hWnd, false);
                int cmd = TrackPopupMenu(hMenu, 0x100, (int)pos.X, (int)pos.Y, 0, hWnd, IntPtr.Zero);
                if (cmd > 0) SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
            }
        }
    }
}
