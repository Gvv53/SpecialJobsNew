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
    public class FiderViewModel : BaseViewModel
    {
        public event Action RefreshGcFiders;
        public event Action Cancel;
        private FIDER newRow;
        private bool _isEnabled;
        public string defaultF
        {
            get { return selectedRow.F_DEFAULT; }
            set
            {
                if (value == "да") //отменим пердыдущее значвение по умолчанию, если оно есть
                {
                    FIDER temp = fiders.Where(p => p.F_DEFAULT == "да").FirstOrDefault();
                    if (temp != null)
                        temp.F_DEFAULT = "нет";
                }
                selectedRow.F_DEFAULT = value;
                RaisePropertyChanged(() => defaultF);
            }
        }
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

        private ObservableCollection<FIDER_CALIBRATION> _data;
        public ObservableCollection<FIDER_CALIBRATION> data
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

        private ObservableCollection<FIDER> _fiders;
        public ObservableCollection<FIDER> fiders
        {
            get
            {
                return _fiders;
            }
            set
            {
                _fiders = value;
                RaisePropertyChanged(() => fiders);
                if (selectedRow == null && fiders != null && fiders.Count != 0)
                    selectedRow = fiders[0];
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

        private FIDER _selectedRow;
        public FIDER selectedRow {
            get
            {               
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.F_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;               
                RaisePropertyChanged(() => selectedRow);
                if (value != null && selectedRow.FIDER_CALIBRATION != null)
                    data = new ObservableCollection<FIDER_CALIBRATION>(selectedRow.FIDER_CALIBRATION.OrderBy(p => p.F_CAL_F));
                else
                    data = null;
                RaisePropertyChanged(() => type);
                RaisePropertyChanged(() => model);
                RaisePropertyChanged(() => interval);
                RaisePropertyChanged(() => workNumber);
                RaisePropertyChanged(() => sertificat);
                RaisePropertyChanged(() => conditions);
                RaisePropertyChanged(() => owner);
                RaisePropertyChanged(() => date);
                RaisePropertyChanged(() => error);
                RaisePropertyChanged(() => errorUnitId);
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => note);
                RaisePropertyChanged(() => defaultF);
                isEnabled = false;
            }
        }

        public string type
        {
            get { return selectedRow.F_TYPE; }
            set
            {
                selectedRow.F_TYPE = value;
                RaisePropertyChanged(() => type);
            }
        }

        public string model
        {
            get { return selectedRow.F_MODEL; }
            set
            {
                selectedRow.F_MODEL = value;
                RaisePropertyChanged(() => model);
            }
        }

        public string interval
        {
            get { return selectedRow.F_F_INTERVAL; }
            set
            {
                selectedRow.F_F_INTERVAL = value;
                RaisePropertyChanged(() => interval);
            }
        }

        public string workNumber
        {
            get { return selectedRow.F_WORKNUMBER; }
            set
            {
                selectedRow.F_WORKNUMBER = value;
                RaisePropertyChanged(() => workNumber);
            }
        }

        public string sertificat
        {
            get { return selectedRow.F_SERTIFICAT; }
            set
            {
                selectedRow.F_SERTIFICAT = value;
                RaisePropertyChanged(() => sertificat);
            }
        }

        public double error
        {
            get
            {               
                return selectedRow.F_ERROR;
            }
            set
            {
                selectedRow.F_ERROR = value;
                RaisePropertyChanged(() => error);
            }
        }

        public string owner
        {
            get { return selectedRow.F_OWNER; }
            set
            {
                selectedRow.F_OWNER = value;
                RaisePropertyChanged(() => owner);
            }
        }

        public string conditions
        {
            get { return selectedRow.F_CONDITIONS; }
            set
            {
                selectedRow.F_CONDITIONS = value;
                RaisePropertyChanged(() => conditions);
            }
        }

        public int? errorUnitId
        {
            get
            {               
                return selectedRow.F_VALUE_UNIT_ID;
            }
            set
            {
                selectedRow.F_VALUE_UNIT_ID = value;
                RaisePropertyChanged(() => errorUnitId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public int? fUnitId
        {
            get
            {               
                return selectedRow.F_F_UNIT_ID;
            }
            set
            {
                selectedRow.F_F_UNIT_ID = value;
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public int? personId
        {
            get
            {               
                return selectedRow.F_PERSON_ID;
            }
            set
            {
                selectedRow.F_PERSON_ID = value;
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public int? executorId
        {
            get { return  selectedRow.F_EXECUTOR_ID; }
            set
            {
                selectedRow.F_EXECUTOR_ID = value;
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => selectedRow);
            }
        }

        public DateTime? date
        {
            get
            {                
                return selectedRow.F_DATE;                
            }
            set
            {
                selectedRow.F_DATE = value;
                RaisePropertyChanged(() => date);
            }
        }

        public string note
        {
            get { return selectedRow.F_COMMENT; }
            set
            {
                selectedRow.F_COMMENT = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public FiderViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            fiders = new ObservableCollection<FIDER>(methodsEntities.FIDER.OrderBy(p => p.F_TYPE));
            persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
            units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
        }


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
                RaisePropertyChanged(() => type);
                RaisePropertyChanged(() => model);
                RaisePropertyChanged(() => interval);
                RaisePropertyChanged(() => workNumber);
                RaisePropertyChanged(() => sertificat);
                RaisePropertyChanged(() => conditions);
                RaisePropertyChanged(() => owner);
                RaisePropertyChanged(() => date);
                RaisePropertyChanged(() => error);
                RaisePropertyChanged(() => errorUnitId);
                RaisePropertyChanged(() => fUnitId);
                RaisePropertyChanged(() => personId);
                RaisePropertyChanged(() => executorId);
                RaisePropertyChanged(() => note);
                RaisePropertyChanged(() => defaultF);
            }
            RefreshGcFiders();
            //восстановим фокус после обновления таблиц
            if (selIdCurrent != 0)
                selectedRow = fiders.Where(p => p.F_ID == selIdCurrent).FirstOrDefault();
            isEnabled = false;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            //пробежимся по строкам калибровочной таблицы и удалим незаполненные
            bool key = true;
            while (key && newRow != null)
            {
                key = false;
                foreach (FIDER_CALIBRATION ac in data)
                {
                    if (ac.F_CAL_F == null || ac.F_CAL_VALUE == null || ac.F_CAL_F == 0 || ac.F_CAL_VALUE == 0)
                    {
                        data.Remove(ac);
                        methodsEntities.FIDER_CALIBRATION.Remove(
                            methodsEntities.FIDER_CALIBRATION.Where(p => p.F_CAL_ID == ac.F_CAL_ID)
                                .FirstOrDefault());
                        key = true;
                        break;
                    }
                }
                 //всё просмотрели, ничего не пришлось удалять
            }
            try
            {
                if (newRow != null) //сохранение новой записи
                {
                    methodsEntities.FIDER.Add(newRow);
                    methodsEntities.SaveChanges();
                    fiders = new ObservableCollection<FIDER>(methodsEntities.FIDER.OrderBy(p => p.F_TYPE));
                    RefreshGcFiders(); //перепривязка данных, иначе таблица не обновляется
                    selectedRow = fiders.Where(p => p.F_TYPE == "").FirstOrDefault();
                    isEnabled = true;
                }
                else //сохранение откорректированной записи
                {
                    int selectedId = selectedRow.F_ID;
                    methodsEntities.SaveChanges();
                    fiders = new ObservableCollection<FIDER>(methodsEntities.FIDER.OrderBy(p => p.F_TYPE));
                    RefreshGcFiders(); //перепривязка данных, иначе таблица не обновляется
                    selectedRow = fiders.Where(p => p.F_ID == selectedId).FirstOrDefault();
                    isEnabled = false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохранения изменения в БД. " + e.Message);
            }
            RaisePropertyChanged(() => selectedRow);
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new FIDER();
            newRow.F_TYPE = "";
            newRow.F_VALUE_UNIT_ID = Functions.GetUnitID(units,"дБ");
            fiders.Add(newRow);
            SaveData(null); 
        }

        //удаление строки фидера

        private void DeleteData(FIDER focusedRow)
        {
            if (focusedRow == null || focusedRow.F_ID == 0)
                return;
            if (methodsEntities.MEASURING_DATA.Where(p => p.MDA_F_ID == focusedRow.F_ID).Count() != 0) //есть ссылка на выбранную строку
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;
            }
            methodsEntities.FIDER.Remove(focusedRow);
            fiders.Remove(focusedRow);
            methodsEntities.SaveChanges();
            if (fiders.Count > 0)
                selectedRow = fiders[0];
            RefreshGcFiders();
            isEnabled = false;
        }

        //удаление строки из калибровочной таблицы
        private void DeleteCalibr(FIDER_CALIBRATION focusedRow)
        {
            if (focusedRow == null || focusedRow.F_CAL_ID == 0)
                return;
            methodsEntities.FIDER_CALIBRATION.Remove(
                methodsEntities.FIDER_CALIBRATION.Where(p => p.F_CAL_ID == focusedRow.F_CAL_ID).FirstOrDefault());
            methodsEntities.SaveChanges();
            if (selectedRow != null)
                data = new ObservableCollection<FIDER_CALIBRATION>(selectedRow.FIDER_CALIBRATION.OrderBy(p => p.F_CAL_F));
            else
                data = null;            
        }

        //добавление новой строки в MODE
        public void NewCalibr()
        {
            
            data[data.Count - 1].F_CAL_F_ID = selectedRow.F_ID;
            data[data.Count - 1].F_CAL_F = 0;
            data[data.Count - 1].F_CAL_VALUE = 0;
            methodsEntities.Entry(data[data.Count - 1]).State = EntityState.Added;
            methodsEntities.SaveChanges(); //сохранили в базе строку режима сразу после добавления
            RaisePropertyChanged(() => data);
        }

        #region Commands

        public ICommand DeleteDataCommand { get { return new RelayCommand<FIDER>(DeleteData); } }
        public ICommand DeleteCalibrCommand { get { return new RelayCommand<FIDER_CALIBRATION>(DeleteCalibr); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
