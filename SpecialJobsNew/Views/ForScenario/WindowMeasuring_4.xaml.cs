using System;
using DevExpress.Xpf.Grid;
using SpecialJobs.Helpers;
using System.Windows;
namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowMeasuring_4.xaml
    /// </summary>
    public partial class WindowMeasuring_4 : Window
    {
        public WindowMeasuring_4()
        {
            GridControl.AllowInfiniteGridSize = true;
            InitializeComponent();
            //динамический стиль привязываем здесь, чтобы не мешал дизайнеру отображать форму
            gcResultsScen.Columns.GetColumnByFieldName("RES_R2_PORTABLE").CellStyle = (Style)gcResultsScen.FindResource("customCellStyle");
            gcResultsScen.Columns.GetColumnByFieldName("RES_R2_PORTABLE_DRIVE").CellStyle = (Style)gcResultsScen.FindResource("customCellStyle");
            gcResultsScen.Columns.GetColumnByFieldName("RES_R2_PORTABLE_CARRY").CellStyle = (Style)gcResultsScen.FindResource("customCellStyle");
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

        }
        public void WindowClose()
        {
            this.Close();
        }
        
        public void RefreshGcResultsScen()
        {
            gcResultsScen.RefreshData();
        }
        public void RefreshGcCollection()
        {
            gcCollection.RefreshData();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Top = (SystemParameters.WorkArea.Height - e.NewSize.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - e.NewSize.Width) / 2;
        }
        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.DisplayText == "0" || e.DisplayText == "0,")
                e.DisplayText = "";
        }
        private void gcCollection_ItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            gcCollection.Dispatcher.BeginInvoke(
                new Action(delegate { ((TableView)gcCollection.View).BestFitColumns(); }),
                System.Windows.Threading.DispatcherPriority.Render);
        }
        private void gcCollectionUF_ItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            gcCollection.Dispatcher.BeginInvoke(
                new Action(delegate { ((TableView)gcCollectionUF.View).BestFitColumns(); }),
                System.Windows.Threading.DispatcherPriority.Render);
        }
        private void gcCollectionU0_ItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            gcCollection.Dispatcher.BeginInvoke(
                new Action(delegate { ((TableView)gcCollectionU0.View).BestFitColumns(); }),
                System.Windows.Threading.DispatcherPriority.Render);
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
    }
}