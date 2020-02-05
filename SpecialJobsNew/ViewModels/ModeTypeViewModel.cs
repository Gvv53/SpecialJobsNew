using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core.Commands;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    public class ModeTypeViewModel : BaseViewModel
    {
        public event Action focus;
        private MODE_TYPE newRow;
        private string namePrev, modeCKPrev, notePrev;
        private double modeKNPrev, modeMPrev, modeNPrev, fPrev;
        private bool _isEnabled, isVisilablePrev;
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

        public ObservableCollection<MODE_TYPE> data
        {
            get
            {   
                return new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p=>p.MT_NAME));
            }
            set
            {
                methodsEntities.MODE_TYPE.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private MODE_TYPE _selectedRow;
        public MODE_TYPE selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.MT_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => modeCK);
                RaisePropertyChanged(() => modeKN);
                RaisePropertyChanged(() => modeN);
                RaisePropertyChanged(() => modeM);
                RaisePropertyChanged(() => f);
                RaisePropertyChanged(() => isVisilable); 
                namePrev = name;
                modeCKPrev = modeCK;
                isVisilablePrev = isVisilable;
                modeKNPrev = modeKN;
                modeMPrev = modeM;
                modeNPrev = modeN;
                fPrev = f;
                isEnabled = false;
                
            }
        }
        private bool _cbenEnabled;

        public bool cbenEnabled
        {
            get { return _cbenEnabled; }
            set
            {
                _cbenEnabled = value;
                RaisePropertyChanged(() => cbenEnabled);
            }
        }

        private bool _cbemEnabled;

        public bool cbemEnabled
        {
            get { return _cbemEnabled; }
            set
            {
                _cbemEnabled = value;
                RaisePropertyChanged(() => cbemEnabled);
            }
        }
        public string name
        {
            get
            {
                if (selectedRow == null)
                    return String.Empty;
                return selectedRow.MT_NAME;
            }
            set
            {
                selectedRow.MT_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

        public double modeKN
        {
            get
            {
                if (selectedRow == null || selectedRow.MT_KN == null)
                    return 1;
                return (double) selectedRow.MT_KN;
            }
            set
            {
                selectedRow.MT_KN = value;
                RaisePropertyChanged(() => modeKN);
            }
        }

        public string modeCK
        {
            get
            {
                if (selectedRow == null)
                    return String.Empty;
                return selectedRow.MT_CK;
            }
            set
            {
                selectedRow.MT_CK = value;
                RaisePropertyChanged(() => modeCK);
                if (value == "Условно-Параллельный")
                {
                    cbemEnabled = true;
                    cbenEnabled = false;
                    modeN = 0;
                }
                else
                {
                    if (value == "Параллельный")
                    {
                        cbenEnabled = true;
                        cbemEnabled = false;
                        modeM = 0;
                    }
                    else
                    {
                        modeN = 0;
                        modeM = 0;
                        cbenEnabled = false;
                        cbemEnabled = false;
                    }
                }
                modeKN = (modeCK == "Параллельный" && modeN != 0 ? modeN / 2 : 1);
                RaisePropertyChanged(() => modeCK);
            }
        }

        public double modeN
        {
            get
            {
                if (selectedRow == null || selectedRow.MT_N == null)
                    return 0;
                return (double)selectedRow.MT_N;
            }
            set
            {
                selectedRow.MT_N = value;
                RaisePropertyChanged(() => modeN);
                modeKN = (modeCK == "Параллельный" && modeN != 0 ? modeN / 2 : 1);
            }
        }
        public double modeM
        {
            get
            {
                if (selectedRow == null || selectedRow.MT_M == null)
                    return 0;
                return (double)selectedRow.MT_M;
            }
            set
            {
                selectedRow.MT_M = value;
                RaisePropertyChanged(() => modeM);
               // modeKN = (modeCK == "Параллельный" && modeN != 0 ? modeN / 2 : 1);
            }
        }
        public bool isVisilable
        {
            get { return selectedRow.MT_ISVISILABLE; }
            set
            {
                selectedRow.MT_ISVISILABLE = value;
                RaisePropertyChanged(() => isVisilable);
            }
        }
        public double f
        {
            get
            {
                if (selectedRow == null )
                    return 0;
                return (double)selectedRow.MT_F_DEFAULT;
            }
            set
            {
                selectedRow.MT_F_DEFAULT = value;
                RaisePropertyChanged(() => f);
            }
        }
        private MethodsEntities methodsEntities;
        public ModeTypeViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p=>p.MT_NAME));
            if (data != null && data.Count != 0)
            {
                selectedRow = data.FirstOrDefault();
            }
        }

        //отмена редактирования, без сохранения введённых изменений
        private void EditData(Object o)
        {
            if (selectedRow != null)
                isEnabled = true;
        }

        //отмена редактирования, без сохранения введённых изменений
        private void CancelData(Object o)
        {
            name = namePrev;
            modeCK = modeCKPrev;
            isVisilable = isVisilablePrev;
            modeKN = modeKNPrev;
            modeM = modeMPrev;
            modeN = modeNPrev;
            f = fPrev;
        }

        //сохранение строки
        private bool canSaveData(Object o)
        {
            return !String.IsNullOrEmpty(modeCK);
        }
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.MT_NAME))//сохранение новой записи
            {
                var t =
                    methodsEntities.MODE_TYPE.Where(p => p.MT_NAME == newRow.MT_NAME);
                if (t.Count() != 0)
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.MODE_TYPE.Add(newRow);                
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                //возврат в режим ввода новой записи 
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.MT_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.MT_ID == selectedId).FirstOrDefault();
            }         
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            modeCK = "Последовательный";
            newRow = new MODE_TYPE();
            newRow.MT_NAME = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление режима и всех его исследований
        private void DeleteData(MODE_TYPE focusedRow)
        {
            if (focusedRow == null || focusedRow.MT_ID == 0)
                return;
            if (methodsEntities.MODE.Where(p => p.MODE_MT_ID == focusedRow.MT_ID).Count() != 0) //есть ссылка на выбранную строку
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;
            }
            try
            {
                methodsEntities.MODE_TYPE.Remove(methodsEntities.MODE_TYPE.Where(p => p.MT_ID == focusedRow.MT_ID).FirstOrDefault());
                methodsEntities.SaveChanges();
               
            }
            catch (Exception)
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;  
            }
            RaisePropertyChanged(() => data);  
        }

        #region Commands

        public ICommand DeleteDataCommand { get { return new RelayCommand<MODE_TYPE>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData,canSaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
