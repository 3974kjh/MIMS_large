using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// OneVueSplashWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OneVueSplashWindow : Window
    {
        public OneVueSplashWindow()
        {
            this.Topmost = true;
            InitializeComponent();
            //Thread.Sleep(3000);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			this.Topmost = false;
            
			//메인 윈도우를 실행한다.
			System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate ()
            {
                var mainWindow = new MainWindow();
				mainWindow.IsTabStop = true;
				mainWindow.Show();
				mainWindow.IsTabStop = false;
				App.Current.MainWindow = mainWindow;
			});
            this.Close();
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
