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
    public class EqTypeViewModel : BaseViewModel
    {        
        public event Action focus;
        private EQUIPMENT_TYPE newRow;
        private string namePrev, notePrev;
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

        public ObservableCollection<EQUIPMENT_TYPE> data
        {
            get { return new ObservableCollection<EQUIPMENT_TYPE>(methodsEntities.EQUIPMENT_TYPE.OrderBy(p => p.EQT_NAME)); }
            set
            {
                methodsEntities.EQUIPMENT_TYPE.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private EQUIPMENT_TYPE _selectedRow;
        public EQUIPMENT_TYPE selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.EQT_ID != 0)
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
            get { return selectedRow.EQT_NAME; }
            set
            {
                selectedRow.EQT_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

        public string note
        {
            get { return selectedRow.EQT_NOTE; }
            set
            {
                selectedRow.EQT_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public EqTypeViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<EQUIPMENT_TYPE>(methodsEntities.EQUIPMENT_TYPE.OrderBy(p=>p.EQT_NAME));
            if (data != null && data.Count != 0)
            {
                selectedRow = data.FirstOrDefault();
            }
        }
        //редактирование выбранной строки
        private void EditData(Object o)
        {
            if (selectedRow != null)
                isEnabled = true;
        }

        //отмена редактирования, без сохранения введённых изменений
        private void CancelData(Object o)
        {
            name = namePrev;
            note = notePrev;
        }
        //сохранение строки
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.EQT_NAME))//сохранение новой записи
            {
                if (methodsEntities.EQUIPMENT_TYPE.Where(p => p.EQT_NAME == newRow.EQT_NAME).Any())
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.EQUIPMENT_TYPE.Add(newRow);
                string newName = newRow.EQT_NAME;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);                
                //возвращение в режим добавления новой строки
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.EQT_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.EQT_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new EQUIPMENT_TYPE();
            newRow.EQT_NAME = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление выбранной строки
        private void DeleteData(EQUIPMENT_TYPE focusedRow)
        {
            if (focusedRow == null || focusedRow.EQT_ID == 0)
                return;
            if (methodsEntities.EQUIPMENT.Where(p => p.EQ_EQT_ID == focusedRow.EQT_ID).Count() != 0) //есть ссылка на выбранную строку
            {
                MessageBox.Show("Строка не может быть удалена, т.к. ссылка на неё используется в других таблицах");
                return;
            }
            try
            {
                methodsEntities.EQUIPMENT_TYPE.Remove(methodsEntities.EQUIPMENT_TYPE.Where(p => p.EQT_ID == focusedRow.EQT_ID).FirstOrDefault());
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

        public ICommand DeleteDataCommand { get { return new RelayCommand<EQUIPMENT_TYPE>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
