using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core.Commands;
//using DevExpress.Xpf.Mvvm;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    public class UnitViewModel : BaseViewModel
    {
        public event Action focus;
        private UNIT newRow;
        private string ValuePrev, notePrev;
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

        public ObservableCollection<UNIT> data
        {
            get { return new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE)); }
            set
            {
                methodsEntities.UNIT.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private UNIT _selectedRow;
        public UNIT selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.UNIT_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => Value);
                RaisePropertyChanged(() => note);
                ValuePrev = Value;
                notePrev = note;            
                isEnabled = false;
                
            }
        }

        public string Value
        {
            get { return selectedRow.UNIT_VALUE; }
            set
            {
                selectedRow.UNIT_VALUE = value;
                RaisePropertyChanged(() => Value);
                }
        }

        public string note
        {
            get { return selectedRow.UNIT_NOTE; }
            set
            {
                selectedRow.UNIT_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public UnitViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p=>p.UNIT_VALUE));
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
            Value = ValuePrev;
            note = notePrev;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.UNIT_VALUE))//сохранение новой записи
            {
                var t =
                   methodsEntities.UNIT.Where(p =>p.UNIT_VALUE == newRow.UNIT_VALUE);
                if (t.Count() != 0)
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.UNIT.Add(newRow);
                string newValue = newRow.UNIT_VALUE;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                //возврат в режим ввода новой записи 
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.UNIT_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.UNIT_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new UNIT();
            newRow.UNIT_VALUE = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus(); //установка фокуса на имени
        }

        //Удаление заказчика и всех его исследований
        private void DeleteData(UNIT focusedRow)
        {
            if (focusedRow == null || focusedRow.UNIT_ID == 0)
                return;
            try
            {
                methodsEntities.UNIT.Remove(methodsEntities.UNIT.Where(p => p.UNIT_ID == focusedRow.UNIT_ID).FirstOrDefault());
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

        public ICommand DeleteDataCommand { get { return new RelayCommand<UNIT>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
