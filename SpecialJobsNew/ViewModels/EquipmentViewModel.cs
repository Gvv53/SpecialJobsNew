using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    class EquipmentViewModel : BaseViewModel
    {
        public MethodsEntities methodsEntities { get; set; }
        private ARM arm_one;
        private int arm_id;
        public ObservableCollection<EQUIPMENT> data { get; set; }
        public EQUIPMENT equipmentNew { get; set; }
        public ObservableCollection<EQUIPMENT_TYPE> EquipmentTypes { get; set; }
        public ObservableCollection<EQUIPMENT> EquipmentsForCopy { get; set; }
        public ObservableCollection<EQUIPMENT> EquipmentChilds { get; set; }
    
        private bool _isEnabled;
        public bool isEnabled
        {             
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(()=>isEnabled);
            } 
        }
       
        private bool _addInRoot;
        public bool addInRoot 
        { 
            get { return _addInRoot; }
            set
            {
                _addInRoot = value;
                RaisePropertyChanged(()=>addInRoot);
            } 
        }
        public event Action RefreshData;

        private int _selectedArmId;

      

        public EQUIPMENT selectedRow { get; set; }

        public int? eqId
        {
            get { return equipmentNew.EQ_EQT_ID; }
            set
            {
                equipmentNew.EQ_EQT_ID = value;
                RaisePropertyChanged(()=>eqId);
            }
        }

        public string eqModel
        {
            get { return equipmentNew.EQ_MODEL; }
            set
            {
                equipmentNew.EQ_MODEL = value;
                RaisePropertyChanged(() => eqModel);
            }
        }

        public string eqSerial
        {
            get { return equipmentNew.EQ_SERIAL; }
            set
            {
                equipmentNew.EQ_SERIAL = value;
                RaisePropertyChanged(() => eqSerial);
            }
        }
        public string eqNote
        {
            get { return equipmentNew.EQ_NOTE; }
            set
            {
                equipmentNew.EQ_NOTE = value;
                RaisePropertyChanged(() => eqNote);
            }
        }
        public bool eqInMode
        {
            get
            {
                if (equipmentNew.EQ_IN_MODE == null)
                    equipmentNew.EQ_IN_MODE = false;
                return (bool)equipmentNew.EQ_IN_MODE; }
            set
            {
                equipmentNew.EQ_IN_MODE = value;
                RaisePropertyChanged(() => eqInMode);
            }
        }
        public double? eqF
        {
            get { return equipmentNew.EQ_F_DEFAULT; }
            set
            {
                equipmentNew.EQ_F_DEFAULT = value;
                RaisePropertyChanged(() => eqF);
            }
        }

        private Image _imаgeCopy;
        public  Image imаgeCopy 
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

        public EquipmentViewModel(MethodsEntities methodsEntities,ARM arm_one)
        {
            this.arm_one = arm_one;
            this.methodsEntities = methodsEntities;
            this.arm_id = arm_one.ARM_ID;
            data =  new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id));
            EquipmentTypes = new ObservableCollection<EQUIPMENT_TYPE>(methodsEntities.EQUIPMENT_TYPE);
            equipmentNew = new EQUIPMENT();
            isEnabled = false;            
        
            //картинка для кнопки           
        }

        private void AddData(Object o)
        {
            isEnabled = true;
        }

        private void CancelData(Object o)
        {
            isEnabled = false;
            equipmentNew = new EQUIPMENT();
            RaisePropertyChanged(() => eqId);
            RaisePropertyChanged(() => eqModel);
            RaisePropertyChanged(() => eqSerial);
        }

        
        //Удаление строки
        private void DeleteData(EQUIPMENT focusedRow)
        {
            if (focusedRow == null)
                return;
            ////во всех режимах выбранного АРМ удалим ссылки на удаляемое оборудование, если оно яв-ся корневым
            if (focusedRow.EQ_PARENT_ID == null && arm_one != null && arm_one.MODE != null)
            {
                foreach (var mode in arm_one.MODE.Where(p => p.MODE_EQ_ID == focusedRow.EQ_ID))
                {
                    mode.MODE_EQ_ID = (int?) null;
                }
                methodsEntities.SaveChanges();
            }
            //сначала удалим все строки, для которых данная является родителем
            foreach (var eq in methodsEntities.EQUIPMENT.Where(p => p.EQ_PARENT_ID == focusedRow.EQ_ID))
            {
                methodsEntities.EQUIPMENT.Remove(eq);
            }
           
            methodsEntities.EQUIPMENT.Remove(focusedRow); //удаление из контекста 

            methodsEntities.SaveChanges();
            data = new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id));
            RaisePropertyChanged(()=>data);
            RefreshData();
        }

        private void SaveData(Object o)
        {
            if (eqId == null || eqId == 0)
            {
                MessageBox.Show("Выберите название оборудования.");
                return;
            }
            if (String.IsNullOrEmpty(eqModel))
            {
                MessageBox.Show("Заполните поле 'Модель'.");
                return;
            }
            if (String.IsNullOrEmpty(eqSerial))
            {
                MessageBox.Show("Заполните поле 'Серийный номер'.");
                return;
            }
            int selectedRowIdPrev = 0 ;
            equipmentNew.EQ_ARM_ID = arm_id;
            if (selectedRow != null)
                selectedRowIdPrev = selectedRow.EQ_ID;
            if (!addInRoot && selectedRow == null)
            {
                MessageBox.Show(
                    "Выберите корневую строку или поставьте признак 'Добавлять оборудование в корень дерева'.");
                return;
            }
            else
                if (!addInRoot && selectedRow != null) 
                    equipmentNew.EQ_PARENT_ID = selectedRow.EQ_ID;
                       
            try
            {
                methodsEntities.EQUIPMENT.Add(equipmentNew);
                methodsEntities.SaveChanges();

            }
            catch (Exception)
            {
                
                throw;
            }
            data = new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id));
            RaisePropertyChanged(() => data);
            RefreshData();
            equipmentNew = new EQUIPMENT();
            RaisePropertyChanged(() => equipmentNew);
            RaisePropertyChanged(() => eqId);
            RaisePropertyChanged(() => eqModel);
            RaisePropertyChanged(() => eqSerial);
            RaisePropertyChanged(() => eqF);
            RaisePropertyChanged(() => eqInMode);
            isEnabled = false;
        }
        //восстановление из системного буфера с последующей его очисткой
        private void PasteModel(Object o)
        {
            var d = System.Windows.Forms.Clipboard.GetText();
            if (String.IsNullOrEmpty(d))
                return;           
            var arStr = d.Split("\r\n".ToCharArray());
            if (arStr.Length > 1)
            {
                string newValue = String.Empty;
                foreach (string str in arStr)
                {
                    newValue += str;
                }
                eqModel = newValue;
            }
            else
                eqModel = d;
            System.Windows.Forms.Clipboard.Clear();
        }
        //восстановление из системного буфера с последующей его очисткой
        private void PasteSeria(Object o)
        {
            var d = System.Windows.Forms.Clipboard.GetText();
            if (String.IsNullOrEmpty(d))
                return;          
            var arStr = d.Split("\r\n".ToCharArray());
            if (arStr.Length > 1)
            {
                string newValue = String.Empty;
                foreach (string str in arStr)
                {
                    newValue += str;
                }
                eqSerial = newValue;
            }
            else
                eqSerial = d;
            System.Windows.Forms.Clipboard.Clear();
        }
        public ICommand AddDataCommand { get { return new RelayCommand<Object>(AddData); } }
      
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand PasteModelCommand { get { return new RelayCommand<Object>(PasteModel); } }
        public ICommand PasteSeriaCommand { get { return new RelayCommand<Object>(PasteSeria); } }
        public ICommand CancelDataCommand { get { return new RelayCommand<Object>(CancelData); } }
        public ICommand DeleteDataCommand { get { return new RelayCommand<EQUIPMENT>(DeleteData); } }
    }
}
