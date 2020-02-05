using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
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
    public class MDeviceViewModel : BaseViewModel
    {
        public event Action RefreshGcMDevice;
        //public event Action Cancel;
        public event Action focus;
        private MEASURING_DEVICE newRow;
        private int? typeIdPrev;
        private string defaultMdPrev, helperMdPrev, typePrev, modelPrev, intervalPrev, workNumberPrev, notePrev;
        private double errorPrev;
        private DateTime? datePrev;
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

        public ObservableCollection<MEASURING_DEVICE_TYPE> MdTypes { get; set; }

        public string defaultMd
        {
            get { return selectedRow.MD_DEFAULT; }
            set
            {               
                selectedRow.MD_DEFAULT = value;
                RaisePropertyChanged(() => defaultMd);
            }
        }

        public string helperMd
        {
            get { return selectedRow.MD_IS_HELPER; }
            set
            {
                selectedRow.MD_IS_HELPER = value;
                RaisePropertyChanged(() => helperMd);
            }
        }

        public ObservableCollection<MEASURING_DEVICE> data
        {
            get { return new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME)); }
            set
            {
                methodsEntities.MEASURING_DEVICE.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        public ObservableCollection<UNIT> Units
        {
            get { return new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE)); }
            set
            {
                methodsEntities.UNIT.Load();
                RaisePropertyChanged(() => Units);
            }
        }

        private MEASURING_DEVICE _selectedRow;
        public MEASURING_DEVICE selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.MD_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
               // RaisePropertyChanged(() => type);
                RaisePropertyChanged(() => typeId);
                RaisePropertyChanged(() => model);
                RaisePropertyChanged(() => interval);
                RaisePropertyChanged(() => date);
                RaisePropertyChanged(() => workNumber);
                RaisePropertyChanged(() => error);
                RaisePropertyChanged(() => note);
                RaisePropertyChanged(() => defaultMd);
                RaisePropertyChanged(() => helperMd);

               // typePrev = type;
                typeIdPrev = typeId;
                modelPrev = model;
                intervalPrev = interval;
                datePrev = date;
                workNumberPrev = workNumber;
                errorPrev = error;
                notePrev = note;
                defaultMdPrev = defaultMd;
                helperMdPrev = helperMd;

                isEnabled = false;
                
            }
        }

        public int? typeId
        {
            get
            {
                if (selectedRow.MD_MDT_ID == null)
                    return null;
                return (int)selectedRow.MD_MDT_ID;
            }
            set
            {
                selectedRow.MD_MDT_ID = value;
                RaisePropertyChanged(() => typeId);
            }
        }

        //public string type
        //{
        //    get
        //    {
        //        if (selectedRow.MEASURING_DEVICE_TYPE != null)
        //            return selectedRow.MEASURING_DEVICE_TYPE.MDT_NAME;
        //        return String.Empty;
        //    }

        //    set
        //    {
        //        selectedRow.MEASURING_DEVICE_TYPE.MDT_NAME = value;
        //        RaisePropertyChanged(() => type);
        //    }
        //}

        public string model
        {
            get { return selectedRow.MD_MODEL; }
            set
            {
                selectedRow.MD_MODEL = value;
                RaisePropertyChanged(() => model);
            }
        }

        public string interval
        {
            get { return selectedRow.MD_F_INTERVAL; }
            set
            {
                selectedRow.MD_F_INTERVAL = value;
                RaisePropertyChanged(() => interval);
            }
        }

        public string workNumber
        {
            get { return selectedRow.MD_WORKNUMBER; }
            set
            {
                selectedRow.MD_WORKNUMBER = value;
                RaisePropertyChanged(() => workNumber);
            }
        }

        public double error
        {
            get { return selectedRow.MD_ERROR; }
            set
            {
                selectedRow.MD_ERROR = value;
                RaisePropertyChanged(() => error);
            }
        }
      

        public DateTime? date
        {
            get
            {
                if (selectedRow.MD_DATE == null)
                    return null;
                return (DateTime)selectedRow.MD_DATE; }
            set
            {
                selectedRow.MD_DATE = value;
                RaisePropertyChanged(() => date);
            }
        }

        public string note
        {
            get { return selectedRow.MD_NOTE; }
            set
            {
                selectedRow.MD_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public MDeviceViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.OrderBy(p=>p.MEASURING_DEVICE_TYPE.MDT_NAME));
            MdTypes = new ObservableCollection<MEASURING_DEVICE_TYPE>(methodsEntities.MEASURING_DEVICE_TYPE.OrderBy(p=>p.MDT_NAME));
            if (data != null && data.Count != 0)
            {
                selectedRow = data.FirstOrDefault();
            }
        }

        //редактирование выбраной строки
        private void EditData(Object o)
        {
            if (selectedRow != null)
                isEnabled = true;
        }

        //отмена редактирования, без сохранения введённых изменений
        private void CancelData(Object o)
        {
           // type = typePrev;
            typeId = typeIdPrev;
            model = modelPrev;
            interval = intervalPrev;
            date = datePrev;
            workNumber = workNumberPrev;
            error = errorPrev;
            note = notePrev;
            defaultMd = defaultMdPrev;
            helperMd = helperMdPrev;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            if (typeId == null || typeId == 0)
            {
                MessageBox.Show("Выберите тип.");
                return;
            }
         
            if (newRow != null )//сохранение новой записи
            {
                newRow.MD_MDT_ID = typeId;
                var t =
                        methodsEntities.MEASURING_DEVICE.Where(
                            p => p.MD_MDT_ID == newRow.MD_MDT_ID && (p.MD_WORKNUMBER == null && 
                                String.IsNullOrEmpty(newRow.MD_WORKNUMBER) || p.MD_WORKNUMBER != null && p.MD_WORKNUMBER == newRow.MD_WORKNUMBER)&&
                                (p.MD_MODEL == null &&
                                String.IsNullOrEmpty(newRow.MD_MODEL) || p.MD_MODEL != null && p.MD_MODEL == newRow.MD_MODEL));
                if (t.Count() != 0)
                {
                    System.Windows.MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.MEASURING_DEVICE.Add(newRow);
                methodsEntities.SaveChanges();
                int mdIdNew = newRow.MD_ID;
                RaisePropertyChanged(() => data);
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.MD_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);               
                selectedRow = data.Where(p => p.MD_ID == selectedId).FirstOrDefault();
            }

        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new MEASURING_DEVICE();
            //newRow.MD_TYPE = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление строки, если на неё нет ссылок
        //private void DeleteData(MEASURING_DEVICE focusedRow)
        //{
        //    if (focusedRow == null || focusedRow.MD_ID == 0)
        //        return;
        //    if (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MD_ID == focusedRow.MD_ID).Count() != 0 ||
        //        methodsEntities.MEASURING_DEVICE_ARM.Where(p=>p.MDARM_MD_ID == focusedRow.MD_ID).Count() != 0 ) //ссылка на выбранную строку
        //    {
        //        MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
        //        return;
        //    }
        //    methodsEntities.MEASURING_DEVICE.Remove(
        //        methodsEntities.MEASURING_DEVICE.Where(p => p.MD_ID == focusedRow.MD_ID).FirstOrDefault());
        //    methodsEntities.SaveChanges();
        //    RaisePropertyChanged(() => data);  
         
        //}

        #region Commands

       // public ICommand DeleteDataCommand { get { return new RelayCommand<MEASURING_DEVICE>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
