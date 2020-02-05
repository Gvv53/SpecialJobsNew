using System;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;
using System.Windows.Threading;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowMeasuring_3.xaml
    /// </summary>
    
    public partial class WindowMeasuring_3 : Window
    {
        MainWindowViewModel vm;
        public event Action<object, string, MEASURING_DATA> CellUpdated;
        //public event Action<Object> pastingFromExcel;
        public event Action ResultClear;

        public WindowMeasuring_3()
        {
            InitializeComponent();
        }
        public void RefreshGcEHs()
        {
            gcEHs.RefreshData();
        }
        public void WindowClose()
        {
            this.Close();
        }
        private void TableView_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {

            object value;
            TableView view = sender as TableView;
            MEASURING_DATA row = e.Row as MEASURING_DATA;

            if (e.Value == null)
                return;
            value = e.Value;
            CellUpdated(value, e.Column.FieldName, row);
            if (row.MDA_F == 0)
                gcEHs.CurrentColumn = ((TableView)sender).Grid.Columns["MDA_F"];
            RefreshGcEHs();
        }
        private void TableView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            vm = (MainWindowViewModel)DataContext;
            if (e.Column.FieldName != "MDA_F")
            {
                vm.newMeas = false;
            }
            //если что-то поменялось в таблице измерений - очищаем результаты расчёта
            ResultClear();
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
