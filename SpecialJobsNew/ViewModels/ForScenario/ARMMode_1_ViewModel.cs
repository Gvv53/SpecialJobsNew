using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecialJobs.Helpers;
using System.Windows.Input;
using System.Windows;
using SpecialJobs.Views.ForScenario;

namespace SpecialJobs.ViewModels.ForScenario
{
    class ARMMode_1_ViewModel:BaseViewModel
    {
        public bool prev { get; set; }  //признак восстановления предыдущего окна при закрытии текущего
        public bool error { get; set; }  //признак ошибочного ввода или сохранения
        string userName { get; set; } 
        MethodsEntities methodsEntities;
        string _header;
        public string Header 
        { 
         get
            {
                return _header;
            }
            set
            {
                _header = value;
                RaisePropertyChanged(() => Header);
            }
        }

        private ObservableCollection<ARM_TYPE> _armTypes;
        public ObservableCollection<ARM_TYPE> armTypes
        {
            get
            { 
                return _armTypes;
            }
            set
            {
                _armTypes = value;
                RaisePropertyChanged(() => armTypes);
            }
        }
        public event Action closeWindow;
        public event Action hideWindow;
        public event Action focusName;
        public event Action closeWindow2;
        public event Action focusName2;
        public ARMMode_1_ViewModel(MethodsEntities methodsEntities, string userName)
        {
            this.methodsEntities = methodsEntities;
            this.userName = userName;
            //подготовка к новому вводу
        }
        private void RefreshARMTypes()
        {
            if (at_id != 0)            
                armTypes = new ObservableCollection<ARM_TYPE>(methodsEntities.ARM_TYPE.Where(p => p.AT_ID == at_id));  
        }
        private void DeleteAnalysis()
        {
            if (anl_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ANL_ID == anl_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            methodsEntities.ANALYSIS.Remove(methodsEntities.ANALYSIS.Where(p => p.ANL_ID == anl_id).FirstOrDefault());
            methodsEntities.SaveChanges();
            RefreshAnalysis();
            if (analysis.Count > 0)
                anl_id = analysis[0].ANL_ID;
        }
        private void OK() //сохранить и подготовить форму к новому вводу
        {
            Save();
            PrepareNewOrganization();
        }
        private void PrepareNewOrganization()
        { //подготовка к новому вводу
            TextOrganization = String.Empty;
            RaisePropertyChanged(() => TextOrganization);
            organization_new = new ORGANIZATION();
            org_id = 0;
            error = false;           
        }
        private void PrepareNewAnalysis()
        {
            //подготовка к новому вводу
            TextAnalysis = String.Empty;
            RaisePropertyChanged(() => TextAnalysis);
            analysis_new = new ANALYSIS();
            focusName2(); //установка фокуса на поле ввода
        }
        private void OK_Anl() //сохранить и подготовить форму к новому вводу
        {
            Save_Anl();
            PrepareNewAnalysis();
        }
        //сохранение добавленных работ в БД
        private void Save()
        {
            if (String.IsNullOrEmpty(TextOrganization))
                return;
            else
            {
                if (methodsEntities.ORGANIZATION.Where(p => p.ORG_NAME == TextOrganization).Any())
                {
                    MessageBox.Show("В базе данных уже имеется работа с таким названием.");
                    error = true;
                    return;
                }
                this.organization_new.ORG_NAME = TextOrganization;
                methodsEntities.ORGANIZATION.Add(this.organization_new);
                try
                {
                    methodsEntities.SaveChanges();
                    org_id = this.organization_new.ORG_ID;
                    //analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ORGANIZATION.ORG_ID == org_id));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + e.InnerException);
                }
               
            }
        }
        private void Save_Anl()
        {
            if (String.IsNullOrEmpty(TextAnalysis))
                return;
            else
            {
                if (methodsEntities.ANALYSIS.Where(p => p.ORGANIZATION.ORG_ID == org_id && p.ANL_INVOICE == TextAnalysis).Any())
                {
                    MessageBox.Show("В базе данных уже имеется счёт с таким названием для выбранной работы.");
                    return;
                }
                analysis_new.ANL_INVOICE = TextAnalysis;
                analysis_new.ANL_ORG_ID = org_id;
                methodsEntities.ANALYSIS.Add(this.analysis_new);
                try
                {
                    methodsEntities.SaveChanges();
                    //обновление списка введённых счетов
                    RefreshAnalysis();
                    anl_id = this.analysis_new.ANL_ID;

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + e.InnerException);
                }
            }
        }
        private void Cancel()
        {
            closeWindow();
        }
     
        private void Prev()
        {
            prev = true;
            PrepareNewOrganization();
            //закрываем текущую форму
            closeWindow2();                        
        }

        private void Next()
        {
            Save();
            if (error)
            {
                error = false;
                return;
            }
            WindowOrgAnalysis_2 wo2 = new WindowOrgAnalysis_2();
            wo2.Title = "Работа - " + TextOrganization;
            wo2.DataContext = this;

            analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p=>p.ANL_ORG_ID == org_id));
            anl_id = 0;
            analysis_new = new ANALYSIS();
            closeWindow2 += wo2.WindowClose;
            focusName2 += wo2.focusName;
            
            wo2.ShowDialog();
            if (prev)
            {
                prev = false;                
                focusName(); 
            }
            else
               closeWindow();
        }
        public ICommand PrevCommand { get { return new DelegateCommand(Prev); } }
        public ICommand OK_AnlCommand { get { return new DelegateCommand(OK_Anl); } }
        public ICommand OKCommand { get { return new DelegateCommand(OK); } }
        public ICommand NextCommand { get { return new DelegateCommand(Next); } }        
        public ICommand CancelCommand { get { return new DelegateCommand(Cancel); } }
        public ICommand DeleteAnalysisCommand { get { return new DelegateCommand(DeleteAnalysis); } }
    }
}
