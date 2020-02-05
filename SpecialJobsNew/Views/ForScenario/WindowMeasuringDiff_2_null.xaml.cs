using System;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;
using System.Windows.Threading;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowMeasuring_2_null.xaml
    /// </summary>
    public partial class WindowMeasuringDiff_2_null : Window
    {
        MainWindowViewModel vm;
        public event Action<object, string, MEASURING_DATA> CellUpdated;
        //public event Action<Object> pastingFromExcel;
        public event Action ResultClear;

        public WindowMeasuringDiff_2_null()
        {
            InitializeComponent();
        }
        public void WindowClose()
        {
            this.Close();
        }
        public void RefreshGc()
        {
            gcU0.RefreshData();
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
                gcU0.CurrentColumn = ((TableView)sender).Grid.Columns["MDA_F"];
        }
        private void TableView_ShownEditor(object sender, EditorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Editor is TextEdit && ((TextEdit)e.Editor).SelectAllOnGotFocus)
                    ((TextEdit)e.Editor).SelectAll();
            }), DispatcherPriority.Input);
        }
        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "MDA_U0CN_VALUE_IZM" || e.Column.FieldName == "MDA_U0N_VALUE_IZM")
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
