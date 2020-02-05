using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using DevExpress.XtraReports.UI;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels.ForReport
{

    internal class ReportWindowViewModel : BaseViewModel
    {
        public bool keyCancel { get; set; } //ключ прекращения выполнения сценария
        public event Action closeWindow;

        private string reportName;
        public bool buttonEnabled { get; set; }
        public int anl_id { get; set; }
        public bool isDefault_Old;
        public MethodsEntities methodsEntities { get; set; }
        public ObservableCollection<REPORT_DATA> reportData
        {
            get
            {
                var temp = new ObservableCollection<REPORT_DATA>(
                    methodsEntities.REPORT_DATA.Where(p => p.RD_ANL_ID == anl_id && p.RD_REPORT == reportName));
                return temp;
            }            
            set
            { reportData = value; }
        }
        //все возможные варианты переменных полей отчёта
        public ObservableCollection<REPORT_DATA> labelData
        {
            get
            { 
                var temp = new ObservableCollection<REPORT_DATA>(reportData.Where(p => p.RD_LABEL == selectedLabel));
                return temp;
            }
        }
        //все возможные варианты переменных полей отчёта
        public XtraReport xr { get; set; }

        public ObservableCollection<string> listLabel
        {
            get
            {
                var temp = new ObservableCollection<string>(reportData.Select(p => p.RD_LABEL).Distinct());
                return temp;
            }
        } //названия переменных полей отчёта
        public ObservableCollection<string> listDescription 
        {
            get 
            {return new ObservableCollection<string>(labelData.Select(p => p.RD_DESCRIPTION).Distinct()); } } //описания переменных полей отчёта

        private bool _isDefault;

        //public bool isDefault
        //{
        //    get { return (bool) _isDefault; }
        //    set
        //    {
        //        _isDefault = value;
        //        RaisePropertyChanged(() => isDefault);
        //    }
        //}

        public bool isDefault
        {
            get { return (bool)selectedRow.RD_DEFAULT; }
            set
            {
                foreach (var rd in reportData) //только 1 значение м.б. по умолчанию
                    rd.RD_DEFAULT = false;
                selectedRow.RD_DEFAULT = value;
                RaisePropertyChanged(() => isDefault);
            }
        }



        private REPORT_DATA _selectedRow;

        public REPORT_DATA selectedRow
        {
            get
            {
                return _selectedRow;
            }
            set
            {
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);
                selectedText = value.RD_TEXT;
                isDefault = (bool)selectedRow.RD_DEFAULT;
            }
        }

        private string _selectedText;

        public string selectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
                RaisePropertyChanged(() => selectedText);
            }
        }
        //public string selectedText
        //{
        //    get { return selectedRow.RD_TEXT; }
        //    set
        //    {
        //        selectedRow.RD_TEXT = value;
        //        RaisePropertyChanged(() => selectedText);
        //    }
        //}
        private string _selectedLabel;

        public string selectedLabel
        {
            get { return _selectedLabel; }
            set
            {
                _selectedLabel = value;
                RaisePropertyChanged(() => selectedLabel);
                //при изменении выбора метки меняется список описаний текста и текст
                if (value != null)
                {
                   RefreshLabelData();
                }
            }
        }

        private string _selectedDescription;

        public string selectedDescription
        {
            get { return _selectedDescription; }
            set            
            {
                //если выбрали новое значение из списка 
                _selectedDescription = value;
                RaisePropertyChanged(() => selectedDescription);
                if (labelData.Any() &&
                    labelData.Where(
                        p => p.RD_DESCRIPTION == value).Any())
                {
                    selectedRow = labelData.Where(
                        p => p.RD_DESCRIPTION == value).FirstOrDefault();
                }
            }
        }

        public ReportWindowViewModel(MethodsEntities MethodsEntities, XtraReport xr, int anl_id)
        {
            this.methodsEntities = MethodsEntities;
            this.xr = xr;
            this.anl_id = anl_id;
            reportName = xr.GetType().Name;
            RaisePropertyChanged(() => reportData); //все строки таблицы для заданных отчёта и счёта
            RaisePropertyChanged(() => listLabel);  //все метки из пред.коллекции
            if (listLabel.Any())
            {
                selectedLabel = listLabel[0];
            }

        }
        //изменено вручную описание дескриптора
        public void CheckSelectedDescription()
        {
            if (labelData.Any() &&
               !labelData.Where(
                   p => p.RD_DESCRIPTION == selectedDescription).Any()) //новое описание   
            {
                //изменили описание прямо на форме
                //сформируем новую строку с аналогичными полями, кроме описания, добавим в таблицу и покажем.
                REPORT_DATA newRow = new REPORT_DATA()
                {
                    RD_ANL_ID = anl_id,
                    RD_DESCRIPTION = selectedDescription,
                    RD_REPORT = selectedRow.RD_REPORT,
                    RD_LABEL = selectedLabel,
                    RD_TEXT = selectedText,
                    RD_DEFAULT = true
                };
                selectedRow.RD_DEFAULT = false;

                methodsEntities.REPORT_DATA.Add(newRow);
                methodsEntities.SaveChanges();                
                RefreshLabelData();
                selectedRow = newRow;
            }
        }

        private void DeleteRow(Object o)
        {
            methodsEntities.REPORT_DATA.Remove(selectedRow);
            methodsEntities.SaveChanges();
            RaisePropertyChanged(()=>reportData);
            RefreshLabelData();
        }

        private void CloseWindow(Object o)
        {
            //Если добавлено руками и использовано новое описание, то сохраним его в БД
            CheckSelectedDescription();
            keyCancel = false;
            closeWindow(); //
        }
        private void CancelWindow(Object o)
        {
            keyCancel = true;
            closeWindow(); //
        }

        private void RefreshLabelData() //после удаления метки или выбора новой метки
        {            
           
            RaisePropertyChanged(()=> labelData);
            RaisePropertyChanged(() => listDescription);
            if (listDescription.Any()) //есть строки описания(-й) для выбранной метки
            {
                if (labelData.Where(p => p.RD_DEFAULT == true).Any())
                    selectedDescription =
                        labelData.Where(p => p.RD_DEFAULT == true).FirstOrDefault().RD_DESCRIPTION;
                else
                    selectedDescription = labelData[0].RD_DESCRIPTION;
            }
            if (listDescription.Any())
               buttonEnabled = true;
            else
                buttonEnabled = false; 
            RaisePropertyChanged(() => buttonEnabled);

        }

        public ICommand DeleteRowCommand { get { return new RelayCommand<Object>(DeleteRow); } }
        public ICommand ExitWindowCommand { get { return new RelayCommand<Object>(CloseWindow); } }
        public ICommand CancelWindowCommand { get { return new RelayCommand<Object>(CancelWindow); } }

    }

    public class LABEL
    {
        public string label { get; set; }
    }
}
