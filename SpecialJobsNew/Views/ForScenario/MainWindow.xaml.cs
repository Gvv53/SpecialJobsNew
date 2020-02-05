using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using SpecialJobs.Helpers;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using SpecialJobs.ViewModels;
using MessageBox = System.Windows.Forms.MessageBox;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;


namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        //WorkspaceManager wsm;
        public static double Norma;
        public static string FieldName;        
        private MainWindowViewModel vm;
        public event Action ResultClear;
        public event Action<object,string,MEASURING_DATA> CellUpdated;
        public event Action<MODE> ModeEqUpdated, ModeMtUpdated;
        public event Action<object, string, MODE> CellModesUpdated;
        public event Action<Object> pastingFromExcel;
        public event Action<Object> saveData;
        public event Action cancel; //закрывается главная форма, надо сохранить последние настройки
        public event Action<MODE> newMode;
        public event Action<bool> solidChanged;
        
        public event Action<MODE> refreshI;
        //public event Action ResultClear;
        public event Action<MODE,bool> ModeResultClear;
        private bool newRowMode;

        #endregion Fields
        public MainWindow()
        {
            GridControl.AllowInfiniteGridSize = true;
            vm = new MainWindowViewModel();
            if (vm.Error) //ошибка подключения к БД
            {
                return;
            }
            InitializeComponent();                       
            try
            {
                DataContext = vm;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            vm.RefreshGcResults += RefreshGcResults;
            vm.RefreshGcE += RefreshGcE;
            vm.RefreshGcCollection += RefreshGcCollection;
            vm.RefreshGcUF += RefreshGcUF;
            vm.RefreshGcU0 += RefreshGcU0;
            vm.RefreshGcSaz += RefreshGcSaz;
            vm.RefreshGcModes += RefreshGcModes;
            vm.ReadOnly += ReadOnly;
            vm.NotReadOnly += NotReadOnly;
            vm.tcSelectedItemChanged += tcSelectedItemChanged;

            newMode += vm.NewMode;
            CellUpdated += vm.CellUpdated;
            CellModesUpdated += vm.CellModesUpdated;
            saveData += vm.SaveData;
            cancel += vm.WriteConfig;
            refreshI += vm.RefreshI;
            ResultClear += vm.ResultClear;
            ModeResultClear += vm.ModeResultClear;
           // ModeEqUpdated += vm.ModeEqUpdated;
            ModeMtUpdated += vm.ModeMtUpdated;
            pastingFromExcel += vm.PasteFromExcel;
            solidChanged += vm.solidChanged;
            //динамический стиль привязываем здесь, чтобы не мешал дизайнеру отображать форму
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE").CellStyle = (Style)gcResults.FindResource("customCellStyle");
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE_DRIVE").CellStyle = (Style)gcResults.FindResource("customCellStyle");
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE_CARRY").CellStyle = (Style)gcResults.FindResource("customCellStyle");

            ((MainWindowViewModel)this.DataContext).tcSelectedItem = tcData.Items[0];
            this.WindowState = System.Windows.WindowState.Maximized;
            this.SizeToContent = System.Windows.SizeToContent.Manual;
            vm.backgroundWorkerCalculate.ProgressChanged += BackgroundWorkerCalculate_ProgressChanged;
        }

        //выбор вкладки на основной форме
        public void tcSelectedItemChanged(int numberItem)
        {
            tcData.SelectTabItem(tcData.Items[numberItem]);
            gcE.RefreshData(); //обновление перед отображением
            gcUF.RefreshData();
            gcU0.RefreshData();
        }


        //запрет изменения строки для ШП сигналов
        public void ReadOnly()
        {
            if (gcE.Bands[0].Columns[0].ReadOnly)
                return;
            foreach (var band in gcE.Bands)
            {
                foreach (var col in band.Columns)
                {
                    col.ReadOnly = true;
                }
            }
        }

        //запрет изменения строки для ШП сигналов
        public void NotReadOnly()
        {
            if (!gcE.Bands[0].Columns[0].ReadOnly)
                return;
            foreach (var band in gcE.Bands)
            {
                foreach (var col in band.Columns)
                {
                    col.ReadOnly = false;
                }
            }
        }

        private void RefreshGcE()
        {
            gcE.RefreshData();
        }
        private void RefreshGcCollection()
        {
            gcCollection.RefreshData();//для обновления производной таблицы, когда вводится первая запись                       
            gcCollectionUF.RefreshData();
            gcCollectionU0.RefreshData();
        }
        private void RefreshGcSaz()
        {
            gcEHs.RefreshData();
        }
        private void RefreshGcUF()
        {
            gcUF.RefreshData();
            gcCollection.RefreshData();//для обновления производной таблицы, когда вводится первая запись
        }
        private void RefreshGcU0()
        {
            gcU0.RefreshData();
            gcCollection.RefreshData();//для обновления производной таблицы, когда вводится первая запись
        }

        private void RefreshGcResults()
        {
            gcResults.RefreshData();
        }
        private void RefreshGcModes()
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                return;
            gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");
            gcModes.RefreshData();
        }      
        
        //выделение содержимого ячейки, получившей фокус
        private void TableView_ShownEditor(object sender, EditorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Editor is TextEdit && ((TextEdit)e.Editor).SelectAllOnGotFocus)
                    ((TextEdit)e.Editor).SelectAll();
            }), DispatcherPriority.Input);          
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

        //обновление таблицы после автоперерасчёта RBW
        private void cbe_Checked(object sender, RoutedEventArgs e)
        {
            gcE.RefreshData();
        }
        //BestFit для столбцов таблицы
        private void gcModes_ItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            gcModes.Dispatcher.BeginInvoke(
                new Action(delegate { ((TableView) gcModes.View).BestFitColumns(); }),
                System.Windows.Threading.DispatcherPriority.Render);
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
       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (vm.Error)//ошибка подключения к БД
                return;
            saveData(null);
            if (MainWindowViewModel.dbChenged != null)
                MainWindowViewModel.dbChenged.Abort();
            MainWindowViewModel.dbChenged = null;
            cancel();
        }

        private void gcE_PastingFromClipboard(object sender, PastingFromClipboardEventArgs e)
        {
            pastingFromExcel(null);
        }

        private void CheckEdit_Checked(object sender, RoutedEventArgs e)
        {
            gcModes.RefreshData();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            if (vm.Error)//ошибка подключения к БД
                Application.Current.Shutdown();           
        }
        //ввод первого значения в новой строке
        //для полей, изменяемых одним кликом(выбор их списка, чек...)
        private void TableViewModes_CellValueChanging(object sender, CellValueChangedEventArgs e)
       {           
            MODE row = e.Row as MODE;
            if (e.Column.FieldName == "MODE_IS_SOLID")
            {
                solidChanged((bool)e.Value);
            }
        }
        private void TableViewModes_CellValueChanged(object sender, CellValueChangedEventArgs e)

        {
            MODE row = e.Row as MODE;
            if (e.Column.FieldName == "MODE_IS_SOLID")
            {
                row.MODE_RMAX = row.MODE_IS_SOLID ? Math.Round(Math.Pow(2, 0.5), 4) : 1;
                return;
            }
            CellModesUpdated(e.Value, e.Column.FieldName,row); //сохранение изменений в БД
            gcModes.RefreshData();            
        }
        private void GcModes_Loaded(object sender, RoutedEventArgs e)
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
        }    
        public void BackgroundWorkerCalculate_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }       

        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.DisplayText == "0" || e.DisplayText == "0,")
                e.DisplayText = "";
        }       
        void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {
            
            e.Allow = e.DependencyProperty == GridColumn.VisibleProperty ||
                      e.DependencyProperty == GridColumn.ActualWidthProperty;
        }
        private void Button_Click_Restore(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                return;
            gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");

        }
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                File.Create(Environment.CurrentDirectory + @"\layout.xml").Close();
            gcModes.SaveLayoutToXml(Environment.CurrentDirectory + @"\layout.xml");
        }
        //свойства для сохранения
      
        private void TableView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                return;
            gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");
        }
        //сохранение настройки таблицы при сокрытии окна настройки
        private void TableView_HiddenColumnChooser(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + @"\layout.xml"))
                File.Create(Environment.CurrentDirectory + @"\layout.xml").Close();
            gcModes.SaveLayoutToXml(Environment.CurrentDirectory + @"\layout.xml");

        }
        //изменение ЕИ частоты в измерениях запрещено,т.к. не имеет реального смысла
        //Завершение работы приложения по ESC на главном окне
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }           
        }
       
    }
}
