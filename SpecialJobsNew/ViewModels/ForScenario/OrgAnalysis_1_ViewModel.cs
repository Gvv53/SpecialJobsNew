using System;
using Forms = System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Linq;
using SpecialJobs.Helpers;
using System.Windows.Input;
using System.Windows;
using SpecialJobs.Views.ForScenario;
using SpecialJobs.Views;

namespace SpecialJobs.ViewModels.ForScenario
{
    class OrgAnalysis_1_ViewModel:BaseViewModel
    {
       // public bool buttonPrev { get; set; }
        public bool prev { get; set; }  //признак восстановления предыдущего окна при закрытии текущего
        public bool error { get; set; }  //признак ошибочного ввода или сохранения
        string userName { get; set; } 
        string invoice { get; set; }
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
        public bool cbeAnalysisParamEnabled
        {
            get
            {
                if (analysis_one == null)
                    return false;
                return true;
            }
        }
        ORGANIZATION organization_new { get; set; }
        public ObservableCollection<PERSON> Persons { get; set; }
        public ObservableCollection<PERSON> Persons_M { get; set; }
        public ObservableCollection<PERSON> Persons_I { get; set; }

        private ObservableCollection<ORGANIZATION> _Organizations;
        public ObservableCollection<ORGANIZATION> Organizations
        {
            get { return _Organizations; }
            set
            {
                _Organizations = value;
                RaisePropertyChanged(() => Organizations);
            }
        }
        ANALYSIS analysis_new { get; set; }
        public DateTime? dateBegin
        {

            get
            {
                if (analysis_one != null)
                {
                    if (analysis_one.ANL_DATE_BEGIN != null)
                        return (DateTime)analysis_one.ANL_DATE_BEGIN;
                    return DateTime.Now;
                }
                return null;
            }
            set
            {
                analysis_one.ANL_DATE_BEGIN = value;
                RaisePropertyChanged(() => dateBegin);
            }
        }
        public DateTime? dateEnd
        {
            get
            {
                if (analysis_one != null)
                {
                    if (analysis_one.ANL_DATE_END != null)
                        return (DateTime)analysis_one.ANL_DATE_END;
                    return DateTime.Now;
                }
                return null;
            }
            set
            {
                analysis_one.ANL_DATE_END = value;
                RaisePropertyChanged(() => dateEnd);
            }

        }
        private ANALYSIS _analysis_one;
        public ANALYSIS analysis_one
        {
            get
            {
                return _analysis_one;
            }
            set
            {
                _analysis_one = value;
                RaisePropertyChanged(() => analysis_one);
                RaisePropertyChanged(() => dateBegin);
                RaisePropertyChanged(() => dateEnd);
                RaisePropertyChanged(() => Header);
                RaisePropertyChanged(() => cbeAnalysisParamEnabled);
            }
        }
        //идентификатор последней добавленой работы
        private int _org_id;
        public int org_id
        {
            get { return _org_id; }
            set
            {
                _org_id = value;
                orgId = value;
                RaisePropertyChanged(() => org_id);
                
            }
        }
        private int _anl_id = 0;
        public int anl_id
        {
            get { return _anl_id; }
            set {
                  _anl_id = value;
                  anlId = value;
                  RaisePropertyChanged(()=> anl_id);
                  if (value != 0 && analysis != null)
                  {
                     
                      analysis_one = analysis.Where(p => p.ANL_ID == value).FirstOrDefault();
                      Header = @"Параметры счёта " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : "";
                  }
                  else
                      analysis_one = null;
            }
        } 
        public string TextOrganization { get; set; }
        public string TextAnalysis { get; set; }
        private ObservableCollection<ANALYSIS> _analysis;        
        public ObservableCollection<ANALYSIS> analysis
        {
            get
            { 
                return _analysis;
            }
            set
            {
                _analysis = value;
                RaisePropertyChanged(() => analysis);
            }
        }
        public event Action closeWindow;
        public event Action focusName;
        public event Action closeWindow2;
        public event Action focusName2;        
        public static int orgId, anlId;
        public OrgAnalysis_1_ViewModel(MethodsEntities methodsEntities, string userName,int org_id,int anl_id,string invoice)
        {
            this.methodsEntities = methodsEntities;
            this.userName = userName;
            this.invoice = invoice;
            //подготовка к новому вводу
            TextOrganization = String.Empty;
            TextAnalysis = String.Empty;
            organization_new = new ORGANIZATION();
            Persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
            Persons_M = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("М") || String.IsNullOrEmpty(p.PERSON_NOTE)));
            Persons_I = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("И") || String.IsNullOrEmpty(p.PERSON_NOTE)));
            Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
            this.org_id = org_id;
            this.anl_id = anl_id;            
        }
        private void RefreshAnalysis()
        {
            if (org_id != 0)
                analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id));
            //else
            //    analysis = new ObservableCollection<ANALYSIS>();
        }
        private bool canDeleteOrNextOrganization(Object o)
        {
            return (org_id != 0);
        }
            private void DeleteOrganization(Object o)
        {
            if (org_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ORG_ID == org_id && p.CUT_USER_NAME != userName).Any())
            {
                Forms.MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            if (Forms.DialogResult.Yes !=
                Forms.MessageBox.Show("Все исследования выбранной работы в БД будут удалены. Вы согласны?",
                    " ", Forms.MessageBoxButtons.YesNo))
                return;
            foreach (ANALYSIS anl in methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id))
            {
                methodsEntities.ANALYSIS.Remove(anl);
            }
            methodsEntities.ORGANIZATION.Remove(
                methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == org_id).FirstOrDefault());
            methodsEntities.SaveChanges();
            Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
            org_id = Organizations.Any() ? Organizations[0].ORG_ID : 0;
            RefreshAnalysis();
            if (analysis.Any())
                anl_id = analysis[0].ANL_ID;          
        }
        private bool canDeleteAnalysis(Object o)
        {
            return (anl_id != 0);
        }
        private void DeleteAnalysis(Object o)
        {
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ANL_ID == anl_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            methodsEntities.ANALYSIS.Remove(methodsEntities.ANALYSIS.Where(p => p.ANL_ID == anl_id).FirstOrDefault());
            methodsEntities.SaveChanges();
            RefreshAnalysis();
            if (analysis.Any())
                anl_id = analysis[0].ANL_ID;
            else
                anl_id = 0;           
        }
        private bool canOK(Object o)
        {
            return (!String.IsNullOrEmpty(TextOrganization));
        }
        private void OK(Object o) //сохранить и подготовить форму к новому вводу
        {
            Save();
            PrepareNewOrganization();
        }
        private void PrepareNewOrganization()
        { //подготовка к новому вводу
            TextOrganization = String.Empty;
            RaisePropertyChanged(() => TextOrganization);
            organization_new = new ORGANIZATION();
            //org_id = 0;
            //orgId = 0;
            error = false;           
        }
        public void PrepareNewAnalysis()
        {
            //подготовка к новому вводу
            TextAnalysis = String.Empty;
            RaisePropertyChanged(() => TextAnalysis);
            analysis_new = new ANALYSIS();
            focusName2(); //установка фокуса на поле ввода
        }
        private bool canOK_Anl(Object o)
        { return (!String.IsNullOrEmpty(TextAnalysis)); }
        private void OK_Anl(Object o) //сохранить и подготовить форму к новому вводу
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
                    Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
                    org_id = this.organization_new.ORG_ID;
                    //orgId = org_id;
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
                    //anlId = anl_id;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + e.InnerException);
                }
            }
        }
        private void Cancel(Object o)
        {
            orgId = org_id;
            anlId = anl_id;
            closeWindow();
        }
        private void Cancel2(Object o)
        {
            orgId = org_id;
            anlId = anl_id;
            closeWindow2();
        }
        private void Prev(Object o)
        {
            prev = true;
            PrepareNewOrganization();
            //закрываем текущую форму
            closeWindow2();                        
        }

        private void Next(Object o)
        {
            prev = true;
            if (!String.IsNullOrEmpty(TextOrganization))
            {
                Save();
                if (error)
                {
                    error = false;
                    return;
                }
            }
            WindowOrgAnalysis_2 wo2 = new WindowOrgAnalysis_2();
            if (org_id != 0)
            {
                wo2.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME;
                analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id));
                anl_id = analysis.Any() ? analysis[0].ANL_ID : 0;
            }
            wo2.DataContext = this;            
            analysis_new = new ANALYSIS();
            TextAnalysis = String.Empty;
            RaisePropertyChanged(() => TextAnalysis);
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
        private bool canRenameAnalysis()
        {
            return (anl_id != 0);
        }
        private void RenameAnalysis(Object o)
        {
            //   ANALYSIS analysis_new = new ANALYSIS();
            //   analysis_new.ANL_ORG_ID = org_id;
            //analysis_one = methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == orgId && p.ANL_ID == anlId && p.ANL_INVOICE == invoice).FirstOrDefault();
            AddAnalisisWindow addWindow = new AddAnalisisWindow(analysis_one); 
            addWindow.ShowDialog();
            if (String.IsNullOrEmpty(analysis_one.ANL_INVOICE))
                return;
            else
            {
                if (methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id && p.ANL_NAME == analysis_one.ANL_NAME).Any())
                {
                    MessageBox.Show("В базе данных уже имеется счёт с таким же названием для выбранной работы.");
                    return;
                }
            }
            methodsEntities.SaveChanges();
            //обновление списка исследований и выбор добавленного счёта
            //RefreshAnalysis(analysis_one.ANL_INVOICE);
        }
        public ICommand DeleteOrganizationCommand { get { return new RelayCommand<Object>(DeleteOrganization, canDeleteOrNextOrganization); } }
        public ICommand PrevCommand { get { return new RelayCommand<Object>(Prev); } }
        public ICommand OK_AnlCommand { get { return new RelayCommand<Object>(OK_Anl,canOK_Anl); } }
        public ICommand OKCommand { get { return new RelayCommand<Object>(OK,canOK); } }
        public ICommand NextCommand { get { return new RelayCommand<Object>(Next, canDeleteOrNextOrganization); } }        
        public ICommand CancelCommand { get { return new RelayCommand<Object>(Cancel); } }
        public ICommand Cancel2Command { get { return new RelayCommand<Object>(Cancel2); } }
        public ICommand DeleteAnalysisCommand { get { return new RelayCommand<Object>(DeleteAnalysis, canDeleteAnalysis); } }
        public ICommand RenameAnalysisCommand { get { return new DelegateCommandMy<Object>(RenameAnalysis, canRenameAnalysis); } }
    }

}
