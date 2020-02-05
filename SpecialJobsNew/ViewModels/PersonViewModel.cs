using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using DevExpress.Xpf.Core.Commands;
using SpecialJobs.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace SpecialJobs.ViewModels
{
    public class PersonViewModel : BaseViewModel
    {        
        public event Action focus;
        private PERSON newRow;
        private string fioPrev, notePrev;
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

        public ObservableCollection<PERSON> data
        {
            get { return new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO)); }
            set
            {
                methodsEntities.PERSON.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private PERSON _selectedRow;
        public PERSON selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.PERSON_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => fio);
                RaisePropertyChanged(() => note);
                fioPrev = fio;
                notePrev = note;
                isEnabled = false;
                
            }
        }

        public string fio
        {
            get { return selectedRow.PERSON_FIO; }
            set
            {
                selectedRow.PERSON_FIO = value;
                RaisePropertyChanged(() => fio);
            }
        }

        public string note
        {
            get { return selectedRow.PERSON_NOTE; }
            set
            {
                selectedRow.PERSON_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public PersonViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p=>p.PERSON_FIO));
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
            fio = fioPrev;
            note = notePrev;
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.PERSON_FIO))//сохранение новой записи
            {
                if (methodsEntities.PERSON.Where(p =>p.PERSON_FIO == newRow.PERSON_FIO && 
                    (p.PERSON_NOTE == null &&
                    String.IsNullOrEmpty(newRow.PERSON_NOTE) || p.PERSON_NOTE != null && p.PERSON_NOTE == newRow.PERSON_NOTE)).Any())
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }

                methodsEntities.PERSON.Add(newRow);
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                //возврат в режим ввода новой записи 
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.PERSON_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.PERSON_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new PERSON();
            newRow.PERSON_FIO = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus(); //установка фокуса на имени
        }

        //Удаление заказчика и всех его исследований
        private void DeleteData(PERSON focusedRow)
        {
            if (focusedRow == null || focusedRow.PERSON_ID == 0)
                return;
            try
            {
            methodsEntities.PERSON.Remove(
                methodsEntities.PERSON.Where(p => p.PERSON_ID == focusedRow.PERSON_ID).FirstOrDefault());
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

        public ICommand DeleteDataCommand { get { return new RelayCommand<PERSON>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
