using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using DevExpress.Xpf.Core.Commands;
using SpecialJobs.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace SpecialJobs.ViewModels
{
    public class AntennaViewModel : BaseViewModel
    {
        public event Action RefreshGcAntennas;
        public event Action<string> FocusUI;

        private ANTENNA newRow;
        private bool _isEnabled;
        public bool isEnabled { 
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(()=> isEnabled);
            } }
        private bool _isButtonEnabled;
        public bool isButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                _isButtonEnabled = value;
                RaisePropertyChanged(() => isButtonEnabled);
            }
        }



        private ObservableCollection<ANTENNA_CALIBRATION> _data;
        public ObservableCollection<ANTENNA_CALIBRATION> data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                RaisePropertyChanged(() => data); 
            }
        }

        private ObservableCollection<ANTENNA> _antennas;
        public ObservableCollection<ANTENNA> antennas
        {
            get
            {
                return _antennas;
            }
            set
            {
                _antennas = value;
                RaisePropertyChanged(() => antennas);
            }
        }

        private ObservableCollection<UNIT> _units;
        public ObservableCollection<UNIT> units
        {
            get
            {
                return _units;
            }
            set
            {
                _units = value;
                RaisePropertyChanged(() => units);
            }
        }

        private ObservableCollection<PERSON> _persons;
        public ObservableCollection<PERSON> persons
        {
            get
            {
                return _persons;
            }
            set
            {
                _persons = value;
                RaisePropertyChanged(() => persons);
            }
        }

        private ANTENNA _selectedRow;
        public ANTENNA selectedRow {
            get
            {               
                return _selectedRow;
            }
            set
            {
                //if (newRow != null && value != null && value.ANT_ID != 0)
                //{
                //    newRow = null;
                //}
                _selectedRow = value;               
                RaisePropertyChanged(() => selectedRow);
                if (value != null && selectedRow.ANTENNA_CALIBRATION != null)
                    data = new ObservableCollection<ANTENNA_CALIBRATION>(selectedRow.ANTENNA_CALIBRATION.OrderBy(p => p.ANT_CAL_F));
                else
                    data = new ObservableCollection<ANTENNA_CALIBRATION>();//null;
                RaisePropertyChanged(() => type);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => model);
                RaisePropertyChanged(() => interval);
                RaisePropertyChanged(() => workNumber);
                RaisePropertyChanged(() => sertificat);
                RaisePropertyChanged(() => conditions);
                RaisePropertyChanged(() => owner);
                RaisePropertyChanged(() => date);
                RaisePropertyChanged(() => error);
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => note);
                RaisePropertyChanged(() => defaultAnt);
                isEnabled = false;               
            }
        }

        public string type
        {
            get { return selectedRow.ANT_TYPE; }
            set
            {
                selectedRow.ANT_TYPE = value;
                RaisePropertyChanged(() => type);
            }
        }
        public string name
        {
            get { return selectedRow.ANT_NAME; }
            set
            {
                selectedRow.ANT_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

        public string model
        {
            get { return selectedRow.ANT_MODEL; }
            set
            {
                selectedRow.ANT_MODEL = value;
                RaisePropertyChanged(() => model);
            }
        }

        public string interval
        {
            get { return selectedRow.ANT_F_INTERVAL; }
            set
            {
                selectedRow.ANT_F_INTERVAL = value;
                RaisePropertyChanged(() => interval);
            }
        }

        public string workNumber
        {
            get { return selectedRow.ANT_WORKNUMBER; }
            set
            {
                selectedRow.ANT_WORKNUMBER = value;
                RaisePropertyChanged(() => workNumber);
            }
        }

        public string sertificat
        {
            get { return selectedRow.ANT_SERTIFICAT; }
            set
            {
                selectedRow.ANT_SERTIFICAT = value;
                RaisePropertyChanged(() => sertificat);
            }
        }

        public double error
        {
            get
            {               
                return selectedRow.ANT_ERROR;
            }
            set
            {
                selectedRow.ANT_ERROR = value;
                RaisePropertyChanged(() => error);
            }
        }

        public string owner
        {
            get { return selectedRow.ANT_OWNER; }
            set
            {
                selectedRow.ANT_OWNER = value;
                RaisePropertyChanged(() => owner);
            }
        }

        public string conditions
        {
            get { return selectedRow.ANT_CONDITIONS; }
            set
            {
                selectedRow.ANT_CONDITIONS = value;
                RaisePropertyChanged(() => conditions);
            }
        }
       

        public int? fUnitId
        {
            get
            {               
                return selectedRow.ANT_F_UNIT_ID;
            }
            set
            {
                selectedRow.ANT_F_UNIT_ID = value;
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public int? personId
        {
            get
            {               
                return selectedRow.ANT_PERSON_ID;
            }
            set
            {
                selectedRow.ANT_PERSON_ID = value;
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public int? executorId
        {
            get { return  selectedRow.ANT_EXECUTOR_ID; }
            set
            {
                selectedRow.ANT_EXECUTOR_ID = value;
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public DateTime? date
        {
            get
            {                
                return selectedRow.ANT_DATE;                
            }
            set
            {
                selectedRow.ANT_DATE = value;
                RaisePropertyChanged(() => date);
            }
        }

        public string note
        {
            get { return selectedRow.ANT_COMMENT; }
            set
            {
                selectedRow.ANT_COMMENT = value;
                RaisePropertyChanged(() => note);
            }
        }

        public string defaultAnt
        {
            get { return selectedRow.ANT_DEFAULT; }
            set
            {
                if (value == "да") //отменим пердыдущее значвение по умолчанию, если оно есть
                {
                    ANTENNA temp = antennas.Where(p => p.ANT_DEFAULT == "да").FirstOrDefault();
                    if (temp != null)
                        temp.ANT_DEFAULT = "нет";
                }
                selectedRow.ANT_DEFAULT = value;
                RaisePropertyChanged(() => defaultAnt);
            }
        }

        private MethodsEntities methodsEntities;
        public AntennaViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            antennas = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.OrderBy(p=>p.ANT_TYPE));
            if (selectedRow == null && antennas != null && antennas.Any())
            {
                selectedRow = antennas[0];
            }
            persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
            units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
        }

        //отмена редактирования, без сохранения введённых изменений
        private void EditData(Object o)
        {
            if (selectedRow != null)
                isEnabled = true;
        }

        //отмена изменений
        private void CancelData(Object o)
        {
            int selIdCurrent = 0;
            foreach (DbEntityEntry entry in methodsEntities.ChangeTracker.Entries().Where(p => p.State == EntityState.Modified || p.State == EntityState.Added || p.State == EntityState.Deleted))
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    // If the EntityState is the Deleted, reload the date from the database.   
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
            if (selectedRow != null)
            {
                selIdCurrent = selectedRow.ANT_ID;
                RaisePropertyChanged(() => type);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => model);
                RaisePropertyChanged(() => interval);
                RaisePropertyChanged(() => workNumber);
                RaisePropertyChanged(() => sertificat);
                RaisePropertyChanged(() => conditions);
                RaisePropertyChanged(() => owner);
                RaisePropertyChanged(() => date);
                RaisePropertyChanged(() => error);
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => note);
                RaisePropertyChanged(() => defaultAnt);
            }
            
            RefreshGcAntennas();
            //восстановим фокус после обновления таблиц
            if (selIdCurrent != 0)
                selectedRow = antennas.Where(p => p.ANT_ID == selIdCurrent).FirstOrDefault();
            isEnabled = false;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            //пробежимся по строкам калибровочной таблицы и удалим незаполненные
            bool key = true;
            while (key && newRow == null) //только для сохранения не вновь добавленной строки
            {
                key = false;
                foreach (ANTENNA_CALIBRATION ac in data)
                {
                    if (ac.ANT_CAL_F == null || ac.ANT_CAL_VALUE == null || ac.ANT_CAL_F == 0 || ac.ANT_CAL_VALUE == 0)
                    {
                        data.Remove(ac);
                        methodsEntities.ANTENNA_CALIBRATION.Remove(
                            methodsEntities.ANTENNA_CALIBRATION.Where(p => p.ANT_CAL_ID == ac.ANT_CAL_ID)
                                .FirstOrDefault());
                        key = true;
                        break;
                    }
                }
                 //всё просмотрели, ничего не пришлось удалять
            }
            try
            {               
                int selectedRowId;
                if (newRow != null) //сохранение новой записи
                {
                    methodsEntities.SaveChanges();
                    selectedRowId = newRow.ANT_ID;
                    antennas = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.OrderBy(p => p.ANT_TYPE)); //.OrderBy(p => p.ANT_TYPE)
                    isEnabled = true;
                    newRow = null; //запись уже не новая
                    //RefreshGcAntennas?.Invoke(); //перепривязка данных, иначе таблица не обновляется
                   // selectedRow = antennas.Where(p => p.ANT_ID == selectedRowId).FirstOrDefault();
                }
                else //сохранение откорректированной записи
                {
                    methodsEntities.Entry(selectedRow).State = EntityState.Modified;
                    selectedRowId = selectedRow.ANT_ID;
                    if (String.IsNullOrEmpty(selectedRow.ANT_TYPE))
                    {
                        MessageBox.Show("Заполните поле 'Тип антенны'");
                        FocusUI("AntType");
                        return;
                    }
                    if (String.IsNullOrEmpty(selectedRow.ANT_MODEL))
                    {
                        MessageBox.Show("Заполните поле 'Модель'");
                        FocusUI("Model");
                        return;
                    }
                    if (selectedRow.ANT_F_UNIT_ID == null || selectedRow.ANT_F_UNIT_ID == 0)
                    {
                        MessageBox.Show("Заполните поле 'Единица измерения частоты'");
                        FocusUI("UnitId");
                        return;
                    }
                    methodsEntities.SaveChanges();
                    antennas = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.OrderBy(p => p.ANT_TYPE));
                    isEnabled = false;
                    RefreshGcAntennas?.Invoke(); //перепривязка данных, иначе связанные данные не обновляется
                }
               // RefreshGcAntennas?.Invoke(); //перепривязка данных, иначе связанные данные не обновляется
                //selectedRow = antennas.Where(p => p.ANT_ID == selectedRowId).FirstOrDefault();
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохранения изменения в БД. " + e.Message);
            }
            RaisePropertyChanged(() => selectedRow);
          
        }
        
        //добавление строки
        private void AddData(object o)
        {
            newRow = new ANTENNA();
            newRow.ANT_TYPE = "";           
            methodsEntities.ANTENNA.Add(newRow);
            SaveData(null);  //сохранение в БД новой строки и предоставление её на корректировку          
        }

        //удаление строки антенны
        private void DeleteData(ANTENNA focusedRow)
        {
            if (focusedRow == null || focusedRow.ANT_ID == 0)
                return;
            if (methodsEntities.MEASURING_DATA.Where(p => p.MDA_ANT_ID == focusedRow.ANT_ID).Count() != 0) //есть ссылка на выбранную строку
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;
            }
            methodsEntities.ANTENNA.Remove(focusedRow);
            antennas.Remove(focusedRow);
            methodsEntities.SaveChanges();            
            RefreshGcAntennas?.Invoke();
            if (antennas.Any())
            {
                selectedRow = antennas[0];
            }
            isEnabled = false;
        }

        //удаление строки из калибровочной таблицы
        private void DeleteCalibr(ANTENNA_CALIBRATION focusedRow)
        {
            if (focusedRow == null || focusedRow.ANT_CAL_ID == 0)
                return;
            methodsEntities.ANTENNA_CALIBRATION.Remove(
                methodsEntities.ANTENNA_CALIBRATION.Where(p => p.ANT_CAL_ID == focusedRow.ANT_CAL_ID).FirstOrDefault());
            methodsEntities.SaveChanges();
            if (selectedRow != null)
                data = new ObservableCollection<ANTENNA_CALIBRATION>(selectedRow.ANTENNA_CALIBRATION.OrderBy(p => p.ANT_CAL_F));
            else
                data = null;            
        }

        //добавление новой строки в MODE
        public void NewCalibr()
        {
            
            data[data.Count - 1].ANT_CAL_ANT_ID = selectedRow.ANT_ID;
            selectedRow.ANTENNA_CALIBRATION.Add(data[data.Count - 1]);
           // data[data.Count - 1].ANT_CAL_F = 0;
           // data[data.Count - 1].ANT_CAL_VALUE = 0;
            methodsEntities.Entry(data[data.Count - 1]).State = EntityState.Added;
           // methodsEntities.SaveChanges(); //сохранили в базе строку режима сразу после добавления
           // RaisePropertyChanged(() => data);
        }

        #region Commands        

        public ICommand DeleteDataCommand { get { return new RelayCommand<ANTENNA>(DeleteData); } }
        public ICommand DeleteCalibrCommand { get { return new RelayCommand<ANTENNA_CALIBRATION>(DeleteCalibr); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
       
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
