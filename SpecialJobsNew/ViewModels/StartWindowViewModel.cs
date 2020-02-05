using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialJobs.ViewModels
{
    class StartWindowViewModel : BaseViewModel
    {
        private string _configName;
        public string configName
        {
            get { return _configName; }
            set
            {
                _configName = value;
                RaisePropertyChanged(() => configName);
            }

        }

        public StartWindowViewModel()
        {

        }
        public string GetCurrentNameFromConfig()
        {
            string configName = String.Empty;
            return configName;
        }
        public void SaveCurrentNameFromConfig()
        {

        }
        public List<string> GetAllLoginsFromDB()
        {
            return new List<string>();
        }
    }

}
