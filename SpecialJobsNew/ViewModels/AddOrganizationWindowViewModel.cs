using System;
using System.Windows;
using System.Windows.Input;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    public class AddOrganizationWindowViewModel: BaseViewModel
    {
        public event Action closeWindow;
        public ORGANIZATION org { get; set; }
        public string name { get; set; }
       

        public AddOrganizationWindowViewModel(ORGANIZATION org)
        {
            this.org = org;
            this.name = String.Empty;
        }

        private void OK(Object o)
        {
            if (String.IsNullOrEmpty(name))
            {
                MessageBox.Show("Заполните поле 'Название работы'.");
            }
            org.ORG_NAME = name;
           // org.ORG_ADDRES = adres;
            closeWindow();
        }
        private void Cancel(Object o)
        {
            closeWindow();
        }

        public void TextUpdated(string text)
        {
            name = text;
        }

        public ICommand OKCommand { get { return new RelayCommand<Object>(OK); } }
        public ICommand CancelCommand { get { return new RelayCommand<Object>(Cancel); } }
    }
}
