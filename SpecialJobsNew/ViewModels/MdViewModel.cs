using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors;
using SpecialJobs.Helpers;

namespace SpecialJobs.ViewModels
{
    class MdViewModel : BaseViewModel
    {
        public event Action<string> getSelectedRow;
        public MethodsEntities methodsEntities { get; set; }
        private int arm_id;
        public ObservableCollection<MEASURING_DEVICE> mdArm { get; set; }
        public ObservableCollection<MEASURING_DEVICE> mdAll { get; set; }
        private ObservableCollection<MEASURING_DEVICE> _selectedItemsMdArm;
        public ObservableCollection<MEASURING_DEVICE> selectedItemsMdArm
        {
            get
            {
                return _selectedItemsMdArm == null ? new ObservableCollection<MEASURING_DEVICE>() : _selectedItemsMdArm;
            }
            set
            {
                _selectedItemsMdArm = value;
                RaisePropertyChanged(() => selectedItemsMdArm);
            }
        }
        private ObservableCollection<MEASURING_DEVICE> _selectedItemsMdAll;
        public ObservableCollection<MEASURING_DEVICE> selectedItemsMdAll
        {
            get
            {
                return _selectedItemsMdAll == null ? new ObservableCollection<MEASURING_DEVICE>() : _selectedItemsMdAll;
            }
            set
            {
                _selectedItemsMdAll = value;
                RaisePropertyChanged(() => selectedItemsMdAll);
            }
        }
        private ObservableCollection<ANTENNA> _selectedItemsAntArm;
        public ObservableCollection<ANTENNA> selectedItemsAntArm
        {
            get
            {
                return _selectedItemsAntArm == null ? new ObservableCollection<ANTENNA>() : _selectedItemsAntArm;
            }
            set
            {
                _selectedItemsAntArm = value;
                RaisePropertyChanged(() => selectedItemsAntArm);
            }
        }
        private ObservableCollection<ANTENNA> _selectedItemsAntAll;
        public ObservableCollection<ANTENNA> selectedItemsAntAll
        {
            get
            {
                return _selectedItemsAntAll == null ? new ObservableCollection<ANTENNA>() : _selectedItemsAntAll;
            }
            set
            {
                _selectedItemsAntAll = value;
                RaisePropertyChanged(() => selectedItemsAntAll);
            }
        }

        public ObservableCollection<ANTENNA> antAll { get; set; }
        public ObservableCollection<ANTENNA> antArm { get; set; }
        public ObservableCollection<MEASURING_DEVICE_TYPE> mdTypes { get; set; }
        public MEASURING_DEVICE mdNew { get; private set; }
        public ObservableCollection<MEASURING_DEVICE> mdForCopy { get; set; }
        //для ИО
        private bool _isEnabled;
        public bool isEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => isEnabled);
            }
        }
        private bool _isEnabledRight;
        public bool isEnabledRight
        {
            get { return _isEnabledRight; }
            set
            {
                _isEnabledRight = value;
                RaisePropertyChanged(() => isEnabledRight);
            }
        }
        private bool _isEnabledLeft;
        public bool isEnabledLeft
        {
            get { return _isEnabledLeft; }
            set
            {
                _isEnabledLeft = value;
                RaisePropertyChanged(() => isEnabledLeft);
            }
        }
        //для антенн
        private bool _isEnabled1;
        public bool isEnabled1
        {
            get { return _isEnabled1; }
            set
            {
                _isEnabled1 = value;
                RaisePropertyChanged(() => isEnabled1);
            }
        }
        private bool _isEnabledRight1;
        public bool isEnabledRight1
        {
            get { return _isEnabledRight1; }
            set
            {
                _isEnabledRight1 = value;
                RaisePropertyChanged(() => isEnabledRight1);
            }
        }
        private bool _isEnabledLeft1;
        public bool isEnabledLeft1
        {
            get { return _isEnabledLeft1; }
            set
            {
                _isEnabledLeft1 = value;
                RaisePropertyChanged(() => isEnabledLeft1);
            }
        }


        public event Action RefreshData;
        

        public MEASURING_DEVICE focusedRowAll { get; set; }

        public MEASURING_DEVICE focusedRowArm { get; set; }

        public ANTENNA focusedRowAntAll { get; set; }

        public ANTENNA focusedRowAntArm { get; set; }

        public int mdId
        {
            get { return (int)mdNew.MD_MDT_ID; }
            set
            {
                mdNew.MD_MDT_ID = value;
                RaisePropertyChanged(() => mdId);
            }
        }

        public string mdModel
        {
            get { return mdNew.MD_MODEL; }
            set
            {
                mdNew.MD_MODEL = value;
                RaisePropertyChanged(() => mdModel);
            }
        }

        public string mdWorknumber
        {
            get { return mdNew.MD_WORKNUMBER; }
            set
            {
                mdNew.MD_WORKNUMBER = value;
                RaisePropertyChanged(() => mdWorknumber);
            }
        }

        private Image _imаgeCopy;
        public Image imаgeCopy
        {
            get
            {
                if (_imаgeCopy == null)
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\edit-copy.png", UriKind.Absolute));
                    Image imаgeCopy = new Image() { Source = bitmap };
                }
                return _imаgeCopy;
            }
            set
            {
                _imаgeCopy = value;
                RaisePropertyChanged(() => imаgeCopy);
            }
        }

        private void PrepareData()
        {
            //измерительная аппаратура
            mdArm = new ObservableCollection<MEASURING_DEVICE>();            
            var temp =
                methodsEntities.MEASURING_DEVICE_ARM.Where(p => p.MDARM_ARM_ID == arm_id);
            foreach (var t in temp)
            {
                mdArm.Add(t.MEASURING_DEVICE);
            }
            mdAll = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
            //уберём уже выбранные строки
            foreach (var t in temp)
            {
                mdAll.Remove(mdAll.Where(p => p.MD_ID == t.MDARM_MD_ID).FirstOrDefault());
            }
            RaisePropertyChanged(() => mdAll);
            RaisePropertyChanged(() => mdArm);
            //антенны
            antArm = new ObservableCollection<ANTENNA>();
            var temp1 =
                methodsEntities.ANTENNA_ARM.Where(p => p.ANTARM_ARM_ID == arm_id);
            foreach (var t in temp1)
            {
                antArm.Add(t.ANTENNA);
            }
            antAll = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.OrderBy(p => p.ANT_TYPE));
            //уберём уже выбранные строки
            foreach (var t in temp1)
            {
                antAll.Remove(antAll.Where(p => p.ANT_ID == t.ANTARM_ANT_ID).FirstOrDefault());
            }
            RaisePropertyChanged(() => antAll);
            RaisePropertyChanged(() => antArm);

        }

        public MdViewModel(MethodsEntities methodsEntities, ARM arm_one)
        {
            this.methodsEntities = methodsEntities;
            this.arm_id = arm_one.ARM_ID;
            mdTypes = new ObservableCollection<MEASURING_DEVICE_TYPE>(methodsEntities.MEASURING_DEVICE_TYPE);
            mdArm = new ObservableCollection<MEASURING_DEVICE>();

            PrepareData();
            CheckRightLeft();

            isEnabled = false;

           
        }

        private void AddData(Object o)
        {
            isEnabled = true;
        }

        private void AddData1(Object o)
        {
            isEnabled1 = true;
        }

        private void DataClear()
        {
            //удалим то, что было в БД для текущего АРМ
            foreach (var mda in methodsEntities.MEASURING_DEVICE_ARM.Where(p => p.MDARM_ARM_ID == arm_id))
            {
                methodsEntities.MEASURING_DEVICE_ARM.Remove(mda);
            }
            foreach (var aa in methodsEntities.ANTENNA_ARM.Where(p => p.ANTARM_ARM_ID == arm_id))
            {
                methodsEntities.ANTENNA_ARM.Remove(aa);
            }
        }

    
        //активность кнопок переноса строк
        private void CheckRightLeft()
        {
            if (mdArm.Count > 0)
                isEnabledLeft = true;
            else
                isEnabledLeft = false;
            if (mdAll.Count > 0)
                isEnabledRight = true;
            else
                isEnabledRight = false;

            if (antArm.Count > 0)
                isEnabledLeft1 = true;
            else
                isEnabledLeft1 = false;
            if (antAll.Count > 0)
                isEnabledRight1 = true;
            else
                isEnabledRight1 = false;
        }

        private void RightData(Object o)
        {
            getSelectedRow("gcMdAll");
            if (selectedItemsMdAll == null)
            {
                MessageBox.Show("Выберите строки для добавления.");
                return;
            }
            while (selectedItemsMdAll != null && selectedItemsMdAll.Count != 0)
            {
                mdArm.Add(selectedItemsMdAll[0]);
                mdAll.Remove(selectedItemsMdAll[0]);
            }

            CheckRightLeft();
            RefreshData();
        }

        private void LeftData(Object o)
        {
            getSelectedRow("gcMdArm");
            if (selectedItemsMdArm == null)
            {
                MessageBox.Show("Выберите строки для удаления.");
                return;
            }
            while (selectedItemsMdArm != null && selectedItemsMdArm.Count != 0)
            {
                mdAll.Add(selectedItemsMdArm[0]);
                mdArm.Remove(selectedItemsMdArm[0]);
            }
            CheckRightLeft();
            RefreshData();
        }

        private void RightData1(Object o)
        {
            getSelectedRow("gcAntAll");
            if (selectedItemsAntAll == null)
            {
                MessageBox.Show("Выберите строки для добавления.");
                return;
            }
            while (selectedItemsAntAll != null && selectedItemsAntAll.Count != 0)
            {
                antArm.Add(selectedItemsAntAll[0]);
                antAll.Remove(selectedItemsAntAll[0]);
            }
            CheckRightLeft();
            RefreshData();
        }

        private void LeftData1(Object o)
        {
            getSelectedRow("gcAntArm");
            if (selectedItemsAntArm == null)
            {
                MessageBox.Show("Выберите строки для удаления.");
                return;
            }
            while (selectedItemsAntArm != null && selectedItemsAntArm.Count != 0)
            {
                antAll.Add(selectedItemsAntArm[0]);
                antArm.Remove(selectedItemsAntArm[0]);
            }
            CheckRightLeft();
            RefreshData();
        }

        public void SaveData()
        {
            //удалим то, что было и сохраним окончательный выбор
            DataClear();
            foreach (var md in mdArm)
            {
               
                methodsEntities.MEASURING_DEVICE_ARM.Add(new MEASURING_DEVICE_ARM()
                                                         {
                                                             MDARM_ARM_ID = arm_id,
                                                             MDARM_MD_ID = md.MD_ID
                                                         });
            }
            foreach (var aa in antArm)
            {

                methodsEntities.ANTENNA_ARM.Add(new ANTENNA_ARM()
                {
                    ANTARM_ARM_ID = arm_id,
                    ANTARM_ANT_ID = aa.ANT_ID
                });
            }
            if (methodsEntities.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Modified  || p.State == EntityState.Added || p.State == EntityState.Deleted).Count() == 0)
                return; //нет данных для сохранения
            try
            {
                methodsEntities.SaveChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохранения данных используемой аппаратуры.");
            }
        }

        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
        public ICommand RightDataCommand { get { return new RelayCommand<Object>(RightData); } }
        public ICommand LeftDataCommand { get { return new RelayCommand<Object>(LeftData); } }

        public ICommand AddData1Command { get { return new RelayCommand<Object>(AddData1); } }
        public ICommand RightData1Command { get { return new RelayCommand<Object>(RightData1); } }
        public ICommand LeftData1Command { get { return new RelayCommand<Object>(LeftData1); } }

    }
}
