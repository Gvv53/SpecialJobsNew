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
//using DevExpress.Xpf.Mvvm;
using DevExpress.Xpf.Core.Commands;
//using DevExpress.Xpf.Mvvm;
using SpecialJobs.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace SpecialJobs.ViewModels
{
    public class ProducerViewModel : BaseViewModel
    {        
        public event Action focus;
        private PRODUCER newRow;
        private string namePrev, notePrev,countryPrev; //для отката к пред.значениям
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

        public ObservableCollection<PRODUCER> data
        {
            get { return new ObservableCollection<PRODUCER>(methodsEntities.PRODUCER.OrderBy(p => p.PROD_NAME)); }
            set
            {
                methodsEntities.PRODUCER.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private PRODUCER _selectedRow;
        public PRODUCER selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.PROD_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => country);
                RaisePropertyChanged(() => note);
                namePrev = name;
                notePrev = note;
                countryPrev = country;
                isEnabled = false;                
            }
        }

        public string name
        {
            get { return selectedRow.PROD_NAME; }
            set
            {
                selectedRow.PROD_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

          public string country
        {
            get { return selectedRow.PROD_COUNTRY; }
            set
            {
                selectedRow.PROD_COUNTRY = value;
                RaisePropertyChanged(() => country);
            }
        }

        public string note
        {
            get { return selectedRow.PROD_NOTE; }
            set
            {
                selectedRow.PROD_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public ProducerViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<PRODUCER>(methodsEntities.PRODUCER);
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
        private void CancelData(object o)
        {
            name = namePrev;
            note = notePrev;
            country = countryPrev;       
        }

        //сохранение строки
        private void SaveData(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.PROD_NAME))//сохранение новой записи
            {
                if (methodsEntities.PRODUCER.Where(p =>p.PROD_NAME == newRow.PROD_NAME && 
                    (p.PROD_COUNTRY == null && String.IsNullOrEmpty(newRow.PROD_COUNTRY) 
                    || p.PROD_COUNTRY != null && p.PROD_COUNTRY == newRow.PROD_COUNTRY)).Any())
                {
                    MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                methodsEntities.PRODUCER.Add(newRow);
                methodsEntities.SaveChanges();                
                RaisePropertyChanged(() => data);
                //возврат в режим ввода новой записи 
                AddData(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.PROD_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.PROD_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddData(Object o)
        {
            newRow = new PRODUCER();
            newRow.PROD_NAME = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление заказчика и всех его исследований
        private void DeleteData(PRODUCER focusedRow)
        {
            if (focusedRow == null || focusedRow.PROD_ID == 0)
                return;
            try {
            methodsEntities.PRODUCER.Remove(
                methodsEntities.PRODUCER.Where(p => p.PROD_ID == focusedRow.PROD_ID).FirstOrDefault());
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

        public ICommand DeleteDataCommand { get { return new RelayCommand<PRODUCER>(DeleteData); } }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }

        #endregion Commands
    }
}
