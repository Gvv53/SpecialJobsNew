using System;
using System.Windows;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;
using System.Windows.Threading;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowMeasuring_2.xaml
    /// </summary>
    public partial class WindowMeasuringDiff_2_phase : Window
    {
        MainWindowViewModel vm;
        public event Action<object, string, MEASURING_DATA> CellUpdated;
       // public event Action<Object> pastingFromExcel;
        public event Action ResultClear;

        public WindowMeasuringDiff_2_phase()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            RefreshSize();
        }
        private void RefreshSize()
        {
            this.Top = (SystemParameters.WorkArea.Height - gcUF.Height) / 2;
            this.Left = (SystemParameters.WorkArea.Width - gcUF.Width) / 2;
        }
        public void WindowClose()
        {
            this.Close();
        }
        public void RefreshGc()
        {
            gcUF.RefreshData();
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
                gcUF.CurrentColumn = ((TableView)sender).Grid.Columns["MDA_F"];
        }
 
        private void TableView_ShownEditor(object sender, EditorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Editor is TextEdit && ((TextEdit)e.Editor).SelectAllOnGotFocus)
                    ((TextEdit)e.Editor).SelectAll();
            }), DispatcherPriority.Input);
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

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
    }
 }
