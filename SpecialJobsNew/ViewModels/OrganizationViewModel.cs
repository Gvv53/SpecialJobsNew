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

namespace SpecialJobs.ViewModels
{
    public class OrganizationViewModel : BaseViewModel
    {
        public event Action Cancel;
        public event Action focus;
        private ORGANIZATION newRow;
        private string namePrev, notePrev, abbrevPrev,addresPrev; //для отката к пред.значениям
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

        public ObservableCollection<ORGANIZATION> data
        {
            get { return new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME)); }
            set
            {
                methodsEntities.ORGANIZATION.Load();
                RaisePropertyChanged(() => data); 
            }
        }
        private ORGANIZATION _selectedRow;
        public ORGANIZATION selectedRow {
            get
            {
                if (_selectedRow == null && data != null && data.Count != 0)
                    _selectedRow = data[0];
                return _selectedRow;
            }
            set
            {
                if (newRow != null && value != null && value.ORG_ID != 0)
                {
                    newRow = null;
                }
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                RaisePropertyChanged(() => name);
                RaisePropertyChanged(() => abbrev);
                RaisePropertyChanged(() => addres);
                RaisePropertyChanged(() => note);
                namePrev = name;
                notePrev = note;
                abbrevPrev = abbrev;
                addresPrev = addres;
                isEnabled = false;                
            }
        }

        public string name
        {
            get { return selectedRow.ORG_NAME; }
            set
            {
                selectedRow.ORG_NAME = value;
                RaisePropertyChanged(() => name);
            }
        }

        public string abbrev
        {
            get { return selectedRow.ORG_ABBREV; }
            set
            {
                selectedRow.ORG_ABBREV = value;
                RaisePropertyChanged(() => abbrev);
            }
        }

        public string addres
        {
            get { return selectedRow.ORG_ADDRES; }
            set
            {
                selectedRow.ORG_ADDRES = value;
                RaisePropertyChanged(() => addres);
            }
        }

        public string note
        {
            get { return selectedRow.ORG_NOTE; }
            set
            {
                selectedRow.ORG_NOTE = value;
                RaisePropertyChanged(() => note);
            }
        }
        private MethodsEntities methodsEntities;
        public OrganizationViewModel(MethodsEntities methodsEntities)
        {
            this.methodsEntities = methodsEntities;
            data = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p=>p.ORG_NAME));
            if (data != null && data.Count != 0)
            {
                selectedRow = data.FirstOrDefault();
            }
        }

        //отмена редактирования, без сохранения введённых изменений
        private void EditOrganization(Object o)
        {
            if (selectedRow != null)
                isEnabled = true;
        }

        //отмена редактирования, без сохранения введённых изменений
        private void CancelOrganization(Object o)
        {
            name = namePrev;
            note = notePrev;
            abbrev = abbrevPrev;
            addres = addresPrev;
        }

        //сохранение строки
        private void SaveOrganization(Object o)
        {
            if (newRow != null && !String.IsNullOrEmpty(newRow.ORG_NAME))//сохранение новой записи
            {
                if (methodsEntities.ORGANIZATION.Where(
                           p => p.ORG_NAME == newRow.ORG_NAME && (p.ORG_ADDRES == null && String.IsNullOrEmpty(newRow.ORG_ADDRES) || p.ORG_ADDRES != null && p.ORG_ADDRES == newRow.ORG_ADDRES)).Any())
                {
                    System.Windows.MessageBox.Show("Такая строка уже сущестует в таблице.");
                    return;
                }
                string newName = newRow.ORG_NAME;
                string newAddres = newRow.ORG_ADDRES;
                methodsEntities.ORGANIZATION.Add(newRow);
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                //возврат к режиму ввода
                AddOrganization(null);
            }
            else //сохранение откорректированной записи
            {
                int selectedId = selectedRow.ORG_ID;
                methodsEntities.SaveChanges();
                RaisePropertyChanged(() => data);
                selectedRow = data.Where(p => p.ORG_ID == selectedId).FirstOrDefault();
            }
        }
        
        //добавление строки
        private void AddOrganization(Object o)
        {
            newRow = new ORGANIZATION();
            newRow.ORG_NAME = "";
            data.Add(newRow);
            selectedRow = newRow;
            isEnabled = true;
            focus();
        }

        //Удаление заказчика и всех его исследований
        private void DeleteOrganization(ORGANIZATION focusedRow)
        {
            if (focusedRow == null || focusedRow.ORG_ID == 0)
                return;
            if (DialogResult.Yes !=
                MessageBox.Show("Все исследования выбранного заказчика в БД будут удалены. Вы согласны?",
                    " ", MessageBoxButtons.YesNo))
                return;
            foreach (ANALYSIS anl in methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == focusedRow.ORG_ID))
            {
                methodsEntities.ANALYSIS.Remove(anl);
            }
            methodsEntities.ORGANIZATION.Remove(
                methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == focusedRow.ORG_ID).FirstOrDefault());
            methodsEntities.SaveChanges();
            RaisePropertyChanged(() => data);  
         
        }

        #region Commands

        public ICommand DeleteOrganizationCommand { get { return new RelayCommand<ORGANIZATION>(DeleteOrganization); } }
        public ICommand AddOrganizationCommand { get { return new RelayCommand<Object>(AddOrganization); } }
        public ICommand SaveOrganizationCommand { get { return new RelayCommand<Object>(SaveOrganization); } }
        public ICommand CancelOrganizationCommand { get { return new RelayCommand<Object>(CancelOrganization); } }
        public ICommand EditOrganizationCommand { get { return new RelayCommand<Object>(EditOrganization); } }

        #endregion Commands
    }
}
