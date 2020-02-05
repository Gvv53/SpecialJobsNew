using System;
using System.Windows.Input;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    class AddArmTypeWindowViewModel
    {
        public event Action closeWindow;
        private ARM_TYPE armType;
        public string name { get; set; }

        public AddArmTypeWindowViewModel(ARM_TYPE armType)
        {
            this.armType = armType;
            if (!String.IsNullOrEmpty(armType.AT_NAME))
                name = armType.AT_NAME;
        }
        public void TextUpdated(string text)
        {
            name = text;
        }
        private void OK(Object o)
        {
            armType.AT_NAME = name;
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
