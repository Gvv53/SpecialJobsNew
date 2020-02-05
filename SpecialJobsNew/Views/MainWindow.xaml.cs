using System;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using SpecialJobs.ViewModels;
using MessageBox = System.Windows.Forms.MessageBox;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using System.Diagnostics;


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
        public event Action<object,string,MEASURING_DATA> CellUpdated;
        public event Action<object, string, MODE> CellModesUpdated;                
        public event Action cancel; //закрывается главная форма, надо сохранить последние настройки
        public event Action<bool> solidChanged;
        public event Action<bool> contrEChanged;        

        #endregion Fields
        public MainWindow()
        {
            
            GridControl.AllowInfiniteGridSize = true;
            vm = new MainWindowViewModel();
            if (vm.Error) //ошибка подключения к БД
            {
                return;
            }
            ApplicationThemeHelper.ApplicationThemeName = "DevExpress.Xpf.Themes.Office2016White.v19.1";
            InitializeComponent();
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();          
            Title = "Версия № " + assemblyVersion.Substring(4, assemblyVersion.Length-4);
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
            vm.Exit += Exit;
            vm.MultiSelect += MultiSelect;
           // vm.buttonAutoMeasuring += buttonAutoMeasuring;

            CellUpdated += vm.CellUpdated;
            CellModesUpdated += vm.CellModesUpdated;
           // saveData += vm.SaveData;
            cancel += vm.WriteConfig;
           // refreshI += vm.RefreshI;
            //pastingFromExcel += vm.PasteFromExcel;
            solidChanged += vm.solidChanged;
            contrEChanged += vm.contrEChanged;
            //динамический стиль привязываем здесь, чтобы не мешал дизайнеру отображать форму
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE").CellStyle = (Style)gcResults.FindResource("customCellStyle");
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE_DRIVE").CellStyle = (Style)gcResults.FindResource("customCellStyle");
            gcResults.Columns.GetColumnByFieldName("RES_R2_PORTABLE_CARRY").CellStyle = (Style)gcResults.FindResource("customCellStyle");

            ((MainWindowViewModel)this.DataContext).tcSelectedItem = tcData.Items[0];
            this.WindowState = System.Windows.WindowState.Maximized;
            this.SizeToContent = System.Windows.SizeToContent.Manual;
            //vm.backgroundWorkerCalculate.ProgressChanged += BackgroundWorkerCalculate_ProgressChanged;
            
        }
        //состояние выполняемой операции
       
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
            try
            {
                gcModes.RestoreLayoutFromXml(Environment.CurrentDirectory + @"\layout.xml");
                gcModes.RefreshData();
            }
            catch (Exception e)
            {
               // MessageBox.Show("Не удалось восстановить сохранённый вид таблицы Режимов.Это не повлияет на продолжение работы. Попробуйте заново настроить вид таблицы Режимов.");
            }
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
        private void MultiSelect(bool isMultiSelect)
        {
            gcModes.SelectionMode = isMultiSelect ? MultiSelectMode.Row : MultiSelectMode.None;
            if (isMultiSelect)
            {
                if (vm.selectedItemsMode.Count == gcModes.SelectedItems.Count )
                    return;
                vm.selectedItemsMode.Clear();
                ObservableCollection<MODE> temp = new ObservableCollection<MODE>();
                foreach (MODE mode in gcModes.SelectedItems)
                    temp.Add(mode);
                vm.selectedItemsMode = temp;
            }
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
            cancel();
            //saveData(null);
            if (MainWindowViewModel.dbChanged != null)
            {
                if ((int)MainWindowViewModel.dbChanged.ThreadState == 68)//background | suspend
                    MainWindowViewModel.dbChanged.Resume();
                MainWindowViewModel.dbChanged.Abort();
                MainWindowViewModel.dbChanged = null;
            }
           
            string processName = "PeminSpectrumAnalyser";
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length != 0)
                processes[0].Kill();

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
                ((MODE)gcModes.CurrentItem).MODE_CONTR_E = false;
                if ((bool)e.Value) //покажем(спрячем) столбец MODE_CONTR_E
                    gcModes.Columns["MODE_CONTR_E"].ReadOnly = false;
                else
                {
                    row.MODE_CONTR_E = false;
                    gcModes.Columns["MODE_CONTR_E"].ReadOnly = true;
                }

                 CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
               // gcModes.RefreshData();
            }
            if (e.Column.FieldName == "MODE_CONTR_E")
            {
                contrEChanged((bool)e.Value);
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
                //gcModes.RefreshData();
            }
            //для новой строки, в которой только выбран режим
            if (e.Column.FieldName == "MODE_MT_ID" )
            {
                row.MODE_MT_ID = (int)e.Value;
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
                //gcModes.RefreshData();
            }
        }
        private void TableViewModes_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            MODE row = e.Row as MODE;
            if (e.Column.FieldName != "MODE_MT_ID"  && e.Column.FieldName != "MODE_CONTR_E" && e.Column.FieldName != "MODE_IS_SOLID")
            {
                CellModesUpdated(e.Value, e.Column.FieldName, row); //сохранение изменений в БД
               // gcModes.RefreshData();
                
            }
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
            if (gcModes.CurrentItem == null)
                return;
            if ((bool)((MODE)gcModes.CurrentItem).MODE_IS_SOLID)
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = false;
            else            
                gcModes.Columns["MODE_CONTR_E"].ReadOnly = true;            
        }    
        public void BackgroundWorkerCalculate_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }       
        private void gc_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "MDA_ECN_VALUE_IZM" || e.Column.FieldName == "MDA_EN_VALUE_IZM" ||
                e.Column.FieldName == "MDA_UFCN_VALUE_IZM" || e.Column.FieldName == "MDA_UFN_VALUE_IZM" ||
                e.Column.FieldName == "MDA_U0CN_VALUE_IZM" || e.Column.FieldName == "MDA_U0N_VALUE_IZM" ||
                e.Column.FieldName == "MDA_ES_VALUE_IZM")
                return;

            if (e.DisplayText == "0" || e.DisplayText == "0,")
                e.DisplayText = "";
        }       
        void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {            
            e.Allow = e.DependencyProperty == GridColumn.VisibleProperty ||
                      e.DependencyProperty == GridColumn.ActualWidthProperty;
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
        
        public void Exit()
        {
                this.Close();
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
        //

    }
}
