using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    public class AddAnalysisWindowViewModel : BaseViewModel
    {
        public event Action closeWindow;
        private ANALYSIS analysis;
        public string invoice { get; set; }

        public AddAnalysisWindowViewModel(ANALYSIS analysis)
        {
            this.analysis = analysis;
            if (!String.IsNullOrEmpty(analysis.ANL_INVOICE))
                invoice = analysis.ANL_INVOICE;
        }

        private void OK(Object o)
        {
            analysis.ANL_INVOICE = invoice;
            closeWindow();
        }
        private void Cancel(Object o)
        {
            closeWindow();
        }
        public void TextUpdated(string text)
        {
            invoice = text;
        }
        public ICommand OKCommand { get { return new RelayCommand<Object>(OK); } }
        public ICommand CancelCommand { get { return new RelayCommand<Object>(Cancel); } }
    }
}
