using System;
using System.Windows.Forms;
using System.Windows.Input;
using SpecialJobs.Helpers;


namespace SpecialJobs.ViewModels
{
    public class AddArmWindowViewModel : BaseViewModel
    {
        public event Action closeWindow;
        private ARM arm;
      //  private bool isRename = false;
        public string name { get; set; }

        public AddArmWindowViewModel(ARM arm)
        {
            this.arm = arm;
            if (!String.IsNullOrEmpty(arm.ARM_NUMBER))
                name = arm.ARM_NUMBER;
        }
        //public AddArmWindowViewModel(string armName)
        //{
        //    name = armName;
        //    isRename = true;
        //}
        private void OK(Object o)
        {
           //проверка на соответствие шаблону ХХХХ-YYYYY
            int result;
            if (String.IsNullOrEmpty((name)))
            {
                MessageBox.Show("Введите номер АРМ.");
                return;
            }
            if (name.Length != 10 || (!Int32.TryParse(name.Substring(0,4),out result) || !Int32.TryParse(name.Substring(5,5),out result) || name.Substring(4,1) != "-")) 
                 if (DialogResult.Yes != MessageBox.Show("Формат номера АРМ не соответствует XXXX-YYYYY. Вы уверены в правильности ввода?"," ", MessageBoxButtons.YesNo))
                return;          
                arm.ARM_NUMBER = name;
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
