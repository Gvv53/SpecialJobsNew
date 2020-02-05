using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using SpecialJobs.ViewModels;


namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Interaction logic for WindowMeasuring_1.xaml
    /// </summary>
    public partial class WindowMeasuringDiff_1 : Window
    {
        MainWindowViewModel vm;
        public event Action ResultClear;
        public event Action<object, string, MEASURING_DATA> CellUpdated;
        //public event Action<Object> pastingFromExcel;

        public WindowMeasuringDiff_1()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            RefreshSize();
        }
        private void RefreshSize()
        {
            this.Top = (SystemParameters.WorkArea.Height - gcE.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - gcE.Width) / 2;
        }
        public void WindowClose()
        {
            // this.Hide();
            Close();
        }
        public void RefreshGcE()
        {
            gcE.RefreshData();
        }
        public void BackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
        private void cbe_Checked(object sender, RoutedEventArgs e)
        {
            gcE.RefreshData();
        }

        private void TableView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            object value;
            TableView view = sender as TableView;
            MEASURING_DATA row = e.Row as MEASURING_DATA;

            if (e.Value == null)
                return;
            value = e.Value;
            CellUpdated(value, e.Column.FieldName, row);
            if (row.MDA_F == 0)
                gcE.CurrentColumn = ((TableView)sender).Grid.Columns["MDA_F"];
        }

        //выделение содержимого ячейки, получившей фокус
        private void TableView_ShownEditor(object sender, EditorEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Editor is TextEdit && ((TextEdit)e.Editor).SelectAllOnGotFocus)
                        ((TextEdit)e.Editor).SelectAll();
                }), DispatcherPriority.Input);
            }
            catch (Exception ee)
            { }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Top = (SystemParameters.WorkArea.Height - e.NewSize.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - e.NewSize.Width) / 2;
        }
        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "MDA_ECN_VALUE_IZM" || e.Column.FieldName == "MDA_EN_VALUE_IZM")
                return;
            if (e.DisplayText == "0" || e.DisplayText == "0,")
                e.DisplayText = "";
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
