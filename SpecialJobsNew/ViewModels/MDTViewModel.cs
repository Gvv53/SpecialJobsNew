using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core.Commands;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    class MDTViewModel : BaseViewModel
    {
        public event Action Cancel;
        public event Action focus;
        private MEASURING_DEVICE_TYPE newRow;
        private string  namePrev, notePrev; //для отката к пред.значениям

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

        public ObservableCollection<MEASURING_DEVICE_TYPE> data
        {
            get { return new ObservableCollection<MEASURING_DEVICE_TYPE>(methodsEntities.MEASURING_DEVICE_TYPE.OrderBy(p => p.MDT_NAME)); }
            set
            {
                methodsEntities.MEASURING_DEVICE_TYPE.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private MEASURING_DEVICE_TYPE _selectedRow;
        public MEASURING_DEVICE_TYPE selectedRow
        {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.MDT_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => note);
                namePrev = name;
                notePrev = note;
                isEnabled = false;
                
            }
        }

        public string name
        {
            get { return selectedRow.MDT_NAME; }
            set
            {                
                selectedRow.MDT_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

        public string note
        {
            get { return selectedRow.MDT_NOTE; }
            set
            {                
                selectedRow.MDT_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public MDTViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<MEASURING_DEVICE_TYPE>(methodsEntities.MEASURING_DEVICE_TYPE.OrderBy(p => p.MDT_NAME));
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
        private void CancelData (Object o)
        {          
            name = namePrev;
            note = notePrev;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.MDT_NAME))//сохранение новой записи
            {
                if (methodsEntities.EQUIPMENT_TYPE.Where(p => p.EQT_NAME == newRow.MDT_NAME).Any())
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.MEASURING_DEVICE_TYPE.Add(newRow);
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);                
                AddData(null);            
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.MDT_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.MDT_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new MEASURING_DEVICE_TYPE();
            newRow.MDT_NAME = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление выбранной строки
        private void DeleteData(MEASURING_DEVICE_TYPE focusedRow)
        {
            if (focusedRow == null || focusedRow.MDT_ID == 0)
                return;
            if (methodsEntities.MEASURING_DEVICE.Where(p => p.MD_MDT_ID == focusedRow.MDT_ID).Count() != 0) //есть ссылка на выбранную строку
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;
            }
            try
            {
                methodsEntities.MEASURING_DEVICE_TYPE.Remove(methodsEntities.MEASURING_DEVICE_TYPE.Where(p => p.MDT_ID == focusedRow.MDT_ID).FirstOrDefault());
                methodsEntities.SaveChanges();

            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка удаления. " + e.Message);
                return;
            }
            RaisePropertyChanged(() => data);
        }

        #region Commands
       
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }
        public ICommand DeleteDataCommand { get { return new DelegateCommand<MEASURING_DEVICE_TYPE>(DeleteData); } }
        #endregion Commands
    }
}
