using System;
using System.Collections.ObjectModel;
using SpecialJobs.Helpers;
using System.Windows.Input;
using System.Windows;

namespace SpecialJobs.ViewModels
{
    public class SelectWindowViewModel : BaseViewModel
    {
        public event Action closeWindow;
        public ObservableCollection<AllArmsView> allArms { get; set; }
        //public AllArmsView selectedRow { get; set; }
        private AllArmsView _selectedRow;
        public AllArmsView selectedRow //SelectedItem
        {
            get
            {
                return _selectedRow;
            }
            set
            {
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
            }
        }
        private AllArmsView aav;
        public string armTypeNew { get; set; }
        public SelectWindowViewModel()
        { }
        public SelectWindowViewModel(ObservableCollection<AllArmsView> allArmsView,AllArmsView aav)
        {
            allArms = allArmsView;
            this.aav = aav;
        }
        private void OK(Object o)
        {
            if (selectedRow == null)
            {
                MessageBox.Show("Строка для копирования не выбрана");
                //aav = null;
                return;
            }
            if (String.IsNullOrEmpty(armTypeNew))
            {
                MessageBox.Show("Заполните название нового типа АРМ.");
                aav = null;
                return;
            }
            aav.ORG_ID = selectedRow.ORG_ID;
            aav.ORG_NAME = selectedRow.ORG_NAME;
            aav.AT_ID = selectedRow.AT_ID;
            aav.AT_NAME = selectedRow.AT_NAME;
            aav.ANL_ID = selectedRow.ANL_ID;
            aav.ANL_INVOICE = selectedRow.ANL_INVOICE;
            aav.ARM_ID = selectedRow.ARM_ID;
            aav.ARM_NUMBER = selectedRow.ARM_NUMBER;
            aav.armTypeNew = armTypeNew;
            closeWindow();
        }
        private void Cancel(Object o)
        {
           
            closeWindow();
        }

        public ICommand OKCommand { get { return new RelayCommand<Object>(OK); } }
        public ICommand CancelCommand { get { return new RelayCommand<Object>(Cancel); } }
    }
}
