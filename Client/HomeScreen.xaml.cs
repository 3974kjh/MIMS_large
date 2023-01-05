using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Client
{
    /// <summary>
    /// HomeScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeScreen : UserControl
    {
        private readonly App _app;

        public HomeViewModel HM { get; set; }
        public HomeScreen()
        {
            InitializeComponent();

            _app = Application.Current as App;

            if (null != _app)
            {
                this.HM = new HomeViewModel(_app.Engine);
                this.DataContext = HM;
                HM.OwnerView = this;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == HM)
                return;

            HM.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (null == HM)
                return;

            HM.Unload();
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                HM.SearchedPatientInfoListCommand(null);
        }
    }
}
