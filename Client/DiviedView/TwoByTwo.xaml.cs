using Client.Model;
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

namespace Client.DiviedView
{
    /// <summary>
    /// FourthView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TwoByTwo : UserControl
    {
        private readonly App _app;
        private Point originDragPoint;
        private ViewerScreenViewModel VVM { get; set; }

        public TwoByTwo(ViewerScreenViewModel viewModel)
        {
            InitializeComponent();

            _app = Application.Current as App;

            if (null != _app)
            {
                this.VVM = viewModel;
                this.DataContext = VVM;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;

            var bitMapImage = image.Source as BitmapImage;

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                this.VVM.ShowFirstView(bitMapImage);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            originDragPoint = e.GetPosition(border);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            Point currentPoint = e.GetPosition(border);
            Vector diff = originDragPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed == true && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var image = border.Child as Image;
                DragDrop.DoDragDrop(border, image, DragDropEffects.Move);
            }
        }

        private async void Border_Drop(object sender, DragEventArgs e)
        {
            var border = sender as Border;

            if (null == border)
                return;

            var targetImage = border.Child as Image;
            var originImage = e.Data.GetData(typeof(Image)) as Image;

            if (null != originImage && null != targetImage)
            {
                var temp = originImage.Source;
                originImage.Source = targetImage.Source;
                targetImage.Source = temp;
                return;
            }

            var selectedItems = e.Data.GetData(typeof(List<ImageInfoModel>)) as List<ImageInfoModel>;

            if (null != selectedItems)
            {
                if (selectedItems.Count == 1)
                {
                    //targetImage.Source =  selectedItems.First().OriginalImage as BitmapImage;
                    //targetImage.Source = await VVM.GetTargetImageSource(selectedItems.First());
                    var checkImage = await VVM.GetTargetImageSource(selectedItems.First());
                    VVM.CheckSameImage(checkImage);
                    targetImage.Source = checkImage;
                }
                else
                {
                    // MultiSelect
                    VVM.DragMultiItem(selectedItems);
                }
            }
        }
    }
}
