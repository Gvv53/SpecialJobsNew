using System;
using System.IO;
using DevExpress.Xpf.Core.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Grid;
using SpecialJobs.Helpers;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Interaction logic for WindowTypeARMMode_3.xaml
    /// </summary>
    public partial class WindowTypeARMMode_3 : Window
    {
        public event Action<object, string, MODE> CellModesUpdated;
        public event Action<bool> solidChanged;
        public event Action<bool> contrEChanged;
        MainWindowViewModel vm;
        public WindowTypeARMMode_3()
        {
            GridControl.AllowInfiniteGridSize = true;
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }
        public void WindowClose()
        {
            this.Close();
        }
        //ввод первого значения в новой строке
       
        private void CheckEdit_Checked(object sender, RoutedEventArgs e)
        {
            gcModes.RefreshData();
        }
        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.DisplayText == "0" || e.DisplayText == "0,")
                e.DisplayText = "";
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //сохранение вида таблицы gcModes
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                File.Create(Environment.CurrentDirectory + @"\layout.xml").Close();
            gcModes.SaveLayoutToXml(Environment.CurrentDirectory + @"\layout.xml");
        }
        void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {
            e.Allow = e.DependencyProperty == GridColumn.VisibleProperty ||
                      e.DependencyProperty == GridColumn.ActualWidthProperty;
        }
        private void gcModes_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var band in gcModes.Bands)
            {
                band.AddHandler(DXSerializer.AllowPropertyEvent,
                    new AllowPropertyEventHandler(column_AllowProperty));
                foreach (var column in band.Columns)
                {
                    column.AddHandler(DXSerializer.AllowPropertyEvent,
                    new AllowPropertyEventHandler(column_AllowProperty));
                }
            }
            if (gcModes.CurrentItem == null)
                return;
            if ((bool)((MODE)gcModes.CurrentItem).MODE_IS_SOLID)
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = false;
            else
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = true;
        }
        //для полей, изменяемых одним кликом(выбор их списка, чек...)
        private void TableViewModes_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            MODE row = e.Row as MODE;
            if (e.Column.FieldName == "MODE_IS_SOLID")
            {
                solidChanged((bool)e.Value);
                ((MODE)gcModes.CurrentItem).MODE_CONTR_E = false;
                if ((bool)e.Value) //покажем(спрячем) столбец MODE_CONTR_E
                    gcModes.Columns["MODE_CONTR_E"].ReadOnly = false;
                else
                {
                    row.MODE_CONTR_E = false;
                    gcModes.Columns["MODE_CONTR_E"].ReadOnly = true;
                }

                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
            }
            if (e.Column.FieldName == "MODE_CONTR_E")
            {
                contrEChanged((bool)e.Value);
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
                //gcModes.RefreshData();
            }
            //для новой строки, в которой только выбран режим
            if (e.Column.FieldName == "MODE_MT_ID")
            {
                row.MODE_MT_ID = (int)e.Value;
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
               // gcModes.RefreshData();
            }
        }
        private void TableViewModes_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            MODE row = e.Row as MODE;
            if (e.Column.FieldName != "MODE_MT_ID" && e.Column.FieldName != "MODE_CONTR_E" && e.Column.FieldName != "MODE_IS_SOLID")
            {
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
                //gcModes.RefreshData();
            }
        }
        private void TableView_Loaded(object sender, RoutedEventArgs e)
        {
           
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                return;
            gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");
           // gcModes.RefreshData();
        }
        private void GcModes_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            if (e.NewItem == null)
                return;
            if ((bool)((MODE)e.NewItem).MODE_IS_SOLID)
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = false;
            else
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = true;
        }
        public void RefreshGcModes()
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                return;
            try
            {
                gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");
                gcModes.RefreshData();
            }
            catch (Exception e)
            {
                //MessageBox.Show("Не удалось восстановить сохранённый вид таблицы Режимов.Это не повлияет на продолжение работы. Попробуйте заново настроить вид таблицы Режимов.");
            }
        }
    }
}
