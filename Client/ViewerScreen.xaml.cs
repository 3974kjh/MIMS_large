using Client.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ViewerScreen.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewerScreen : UserControl
    {
        private readonly App _app;
        private bool isMouseDwon = false;
        private Point originRectPoint;
        private Point originDragPoint;

        private ViewerScreenViewModel VVM { get; set; }

        public ViewerScreen()
        {
            InitializeComponent();

            _app = Application.Current as App;

            if (null != _app)
            {
                this.VVM = new ViewerScreenViewModel(_app.Engine);
                this.DataContext = VVM;
                VVM.OwnerView = this;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == VVM)
                return;

            VVM.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (null == VVM)
                return;

            VVM.Unload();
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                VVM.SearchedPatientInfoListCommand(null);
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var image = sender as Image;

            double zoom = e.Delta > 0 ? 1 : -1;
            var position = e.GetPosition(image);
            image.RenderTransformOrigin = new Point(position.X / image.ActualWidth, position.Y / image.ActualHeight);

            VVM.MouseWeelControl(zoom);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas dragCanvas = sender as Canvas;

            if (null == dragCanvas)
                return;

            isMouseDwon = true;

            dragSelectionRect.Visibility = Visibility.Visible;

            originRectPoint = e.GetPosition(dragCanvas);

            dragCanvas.CaptureMouse();
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas dragCanvas = sender as Canvas;

            if (null == dragCanvas)
                return;

            isMouseDwon = false;

            dragSelectionRect.Width = 0;
            dragSelectionRect.Height = 0;
            dragSelectionRect.Visibility = Visibility.Collapsed;

            dragCanvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (false == isMouseDwon)
                return;

            Canvas dragCanvas = sender as Canvas;

            if (null == dragCanvas)
                return;

            var currentPoint = e.GetPosition(dragCanvas);

            DrawRectangle(originRectPoint, currentPoint, dragCanvas);
            SelectListBoxItem(dragCanvas);
        }

        private void DrawRectangle(Point originPoint, Point currentPoint, Canvas canvas)
        {
            double x, y, width, height;

            if (currentPoint.X < 0) currentPoint.X = 0;
            if (currentPoint.Y < 0) currentPoint.Y = 0;
            if (currentPoint.X > canvas.ActualWidth) currentPoint.X = canvas.ActualWidth;
            if (currentPoint.Y > canvas.ActualHeight) currentPoint.Y = canvas.ActualHeight;

            x = currentPoint.X < originPoint.X ? currentPoint.X : originPoint.X;
            y = currentPoint.Y < originPoint.Y ? currentPoint.Y : originPoint.Y;

            width = Math.Abs(currentPoint.X - originPoint.X);
            height = Math.Abs(currentPoint.Y - originPoint.Y);

            Canvas.SetLeft(dragSelectionRect, x);
            Canvas.SetTop(dragSelectionRect, y);
            dragSelectionRect.Width = width;
            dragSelectionRect.Height = height;
        }

        private void SelectListBoxItem(Canvas canvas)
        {
            for (int i = 0; i < ImageListBox.Items.Count; i++)
            {
                var item = ImageListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;

                if (null == item)
                    return;

                Point point = item.TransformToAncestor(canvas).Transform(new Point(0, 0));

                var itemRect = new Rect(point.X, point.Y, item.ActualWidth, item.ActualHeight);

                var dragRect = new Rect(Canvas.GetLeft(dragSelectionRect), Canvas.GetTop(dragSelectionRect), dragSelectionRect.Width, dragSelectionRect.Height);

                if (true == itemRect.IntersectsWith(dragRect))
                {
                    item.IsSelected = true;
                    item.Focus();
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var canvas = sender as Canvas;

            if (null == canvas)
                return;

            ImageListBox.Width = canvas.ActualWidth - 0;
            ImageListBox.Height = canvas.ActualHeight - 0;
        }

        private void ImgaeListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            originDragPoint = e.GetPosition(listBox);
        }

        private void ImgaeListBox_MouseMove(object sender, MouseEventArgs e)
        {
            var listBox = sender as ListBox;
            Point currentPoint = e.GetPosition(listBox);
            Vector diff = originDragPoint - currentPoint;

            if (e.LeftButton == MouseButtonState.Pressed
                && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DragDrop.DoDragDrop(listBox, listBox.SelectedItems.Cast<ImageInfoModel>().ToList(), DragDropEffects.Move);
            }
        }

        private void ImageListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (null == listBox)
                return;

            if (listBox.ActualWidth > 1400)
                RectCanvas.Width = listBox.ActualWidth;
            else
                RectCanvas.Width = 1398;
        }
    }
}
