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
    /// DatabaseScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DatabaseScreen : UserControl
    {
        private readonly App _app;

        public DatabaseScreenViewModel DM { get; set; }
        public DatabaseScreen()
        {
            InitializeComponent();

            _app = Application.Current as App;

            if (null != _app)
            {
                this.DM = new DatabaseScreenViewModel(_app.Engine);
                DM.OwnerView = this;
                this.DataContext = this.DM;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == DM)
                return;

            DM.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (null == DM)
                return;

            DM.Unload();
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DM.SearchedPatientInfoListCommand(null);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DM.ShowImageViewer();
            e.Handled = true;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DM.ShowBeforePatientInfo();
            e.Handled = true;
        }
    }
}
