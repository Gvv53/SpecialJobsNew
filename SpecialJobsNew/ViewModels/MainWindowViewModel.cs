using DevExpress.Data.Linq;
using DevExpress.Xpf.Core;
using DevExpress.XtraReports.UI;
using PeminSpectrumData;
using Communications;
using XceedW = Xceed.Words.NET;
using System.Data.Entity.Infrastructure;
using SpecialJobs.Converters;
using SpecialJobs.Helpers;
using SpecialJobs.Reports;
using SpecialJobs.ViewModels.ForReport;
using SpecialJobs.ViewModels.ForScenario;
using SpecialJobs.Views;
using SpecialJobs.Views.ForReport;
using SpecialJobs.Views.ForScenario;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Xceed.Words.NET;
using Z.EntityFramework.Plus;
using MessageBox = System.Windows.Forms.MessageBox;
using DevExpress.Xpf.Editors;
using System.Text;

//LINQ Delete() требует пересоздания контекста, иначе контекст не обновить!!!

namespace SpecialJobs.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields
        PIPEClient client;
        public BackgroundWorker backgroundWorker, backgroundWorkerUF, backgroundWorkerU0, backgroundWorkerAuto, backgroundWorkerUtilite, backgroundWorkerExcel,
            backgroundWorkerCalculate, backgroundWorkerCalculateMode, backgroundWorkerGenerateSAZ, backgroundWorkerClearSAZ,
            backgroundWorkerDelete, backgroundWorkerDeleteAll, backgroundWorkerI, backgroundWorkerE, backgroundWorkerRecalc1, backgroundWorkerRecalc2;
        bool keySuspend, keySuspendParent; // для отслеживания состояния фонового потока (чтобы не запустить его во вложенной функции раньше запуска во внешней ф-ии)
        bool keyWindowMeasuring_4; //активно окно. когда true, фоновый поток не надо запускать
        bool refreshAfterReload = false; //перезагрузка таблиц, изменённых другими пользователями
        bool refreshAfterConfig = false; //восстановление выбора из косфига
        public static Thread dbChanged;
        public string userName { get; set; }
        public string currentUser { get; set; }
        DateTime dtPrev; //время последнего обработанного изменения в БД
        bool prev;
        System.Timers.Timer aTimer, iTimer;
        public XtraReport xr;
        public bool newMeas = false; //устанавливается при переходе на новую строку и сбрасывается при окончании редактирования частоты
        private bool search;
        private OrganizationWindow ow;
        private PersonWindow pw;
        private ProducerWindow prw;
        private UnitWindow uw;
        private EqTypeWindow etw;
        private ModeTypeWindow mtw;
        private MDeviceWindow mdw;
        private AntennaWindow aw;
        private MDTWindow mdtw;

        public event Action RefreshGcResults;
        //public event Action buttonAutoMeasuring;
        public event Action RefreshGcCollection;
        public event Action RefreshGcResultsScen;
        public event Action RefreshGcE;
        public event Action RefreshGcSaz;
        public event Action RefreshGcUF, RefreshGcU0;
        public event Action RefreshGcModes;
        public event Action ReadOnly;
        public event Action Exit;
        public event Action NotReadOnly;
        public event Action<int> tcSelectedItemChanged; //выбираем вкладку, в которую будут вставляться данные измерений(через Tag)
        public event Action<bool> MultiSelect;


        #endregion Fields
        #region Свойства        

        public List<ExchangeContract> dataList { get; set; }
        public bool keyModeEnabled { get { return arm_id != 0; } }
        public string dFromExcel { get; set; }
        public string keyInsert { get; set; }
        private bool _canScenarioF5Enabled;
        public bool canScenarioF5Enabled
        {
            get
            {
                return _canScenarioF5Enabled;
            }
            set
            {
                _canScenarioF5Enabled = value;
                RaisePropertyChanged(() => canScenarioF5Enabled);
            }
        }
        private bool _isMultiSelect;
        public bool isMultiSelect
        {
            get { return _isMultiSelect; }
            set
            {
                _isMultiSelect = value;
                RaisePropertyChanged(() => isMultiSelect);
                canScenarioF5Enabled = !isMultiSelect;
                // RaisePropertyChanged(() => canScenarioF5Enabled);
                MultiSelect?.Invoke(_isMultiSelect);
                if (keySuspend && !keyWindowMeasuring_4 && !_isMultiSelect) // отмена режима автоизмерений
                {
                    keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }
        private int _tcSelectedIndex;
        public int tcSelectedIndex
        {
            get { return _tcSelectedIndex; }
            set
            {
                _tcSelectedIndex = value;
                RaisePropertyChanged(() => tcSelectedIndex);
            }
        }
        public bool AppendMode { get; set; }
        private bool _includeSAZ;
        public bool IncludeSAZ
        {
            get { return _includeSAZ; }
            set
            {
                _includeSAZ = value;
                RaisePropertyChanged(() => IncludeSAZ);
            }
        }
        //расчёт I с перераспределением частот по интервалам(если их в лепестке > 2)
        //признак задаётся в окне измерений, выполняется перед расчётом ОЗ
        private bool _IRemake;
        public bool IRemake
        {
            get { return _IRemake; }
            set
            {
                _IRemake = value;
                RaisePropertyChanged(() => IRemake);
                if (value)
                    ResultClearForDS(); //очистка результатов расчёта ОЗ для режимов диф.спектра
            }
        }
        public string angle { get; set; } //угол поворотного стола
        public List<EqForReport> eqForReport { get; set; }
        //копирование данных широкополосных сигналов
        public bool isSP { get; set; }

        //выделенное помещение - для отчёта
        private bool _isVP;
        public bool isVP
        {
            get { return _isVP; }
            set
            {
                _isVP = value;
                RaisePropertyChanged(() => isVP);
            }
        }

        private bool _ContraintE;
        public bool ContraintE
        {
            get
            {
                return _ContraintE;
            }
            set
            {
                if (_ContraintE == value)
                    return;
                _ContraintE = value;
                RaisePropertyChanged(() => ContraintE);
                // RemakeE();
            }
        }
        private string _MeasuringType;
        public string MeasuringType
        {
            get { return _MeasuringType; }
            set
            {
                _MeasuringType = value;
                RaisePropertyChanged(() => MeasuringType);
            }
        }
        private bool _isSolid;
        public bool isSolid
        {
            get
            {
                return _isSolid;
            }
            set
            {
                if (_isSolid == value)
                    return;
                _isSolid = value;
                RaisePropertyChanged(() => isSolid);

                gcMeasuringDiscr = !value;
                gcMeasuringSolid = value;
                RaisePropertyChanged(() => gcMeasuringDiscr);
                RaisePropertyChanged(() => gcMeasuringSolid);
            }
        }

        //ТС с USB
        private bool _withUSB;
        public bool withUSB
        {
            get { return _withUSB; }
            set
            {
                _withUSB = value;
                RaisePropertyChanged(() => withUSB);
            }
        }

        private ObservableCollection<ARM_TYPE> _ArmTypes;
        public ObservableCollection<ARM_TYPE> ArmTypes //все id и названия типов АРМ,выбранной работы(организации)
        {
            get { return _ArmTypes; }
            set
            {
                _ArmTypes = value;
                RaisePropertyChanged(() => ArmTypes);
            }
        }
        public string Tag { get; set; }
        private object _tcSelectedItem;
        public object tcSelectedItem
        {
            get { return _tcSelectedItem; }
            set
            {
                _tcSelectedItem = value;
                RaisePropertyChanged(() => tcSelectedItem);
                if (value != null)
                    Tag = ((DXTabItem)_tcSelectedItem).Tag.ToString();

            }
        }

        public double R2_P_2 { get; set; }
        public double R2_P_3 { get; set; }
        public double R2_C_2 { get; set; }
        public double R2_C_3 { get; set; }

        public double R2_D_2 { get; set; }
        public double R2_D_3 { get; set; }

        public double R1_SOSR_2 { get; set; }
        public double R1_SOSR_3 { get; set; }
        //максимальные значения R2 для категорий

        public double R2_MAX_P_2 { get; set; }
        private double _R2_MAX_D_2;
        public double R2_MAX_D_2
        {
            get { return _R2_MAX_D_2; }
            set
            {
                _R2_MAX_D_2 = value;
                RaisePropertyChanged(() => R2_MAX_D_2);
            }
        }
        public double R2_MAX_C_2 { get; set; }
        public double R2_MAX_P_3 { get; set; }
        public double R2_MAX_D_3 { get; set; }
        public double R2_MAX_C_3 { get; set; }
        public double R2_MAX_1 { get; set; }
        public double R2_MAX_2 { get; set; }
        public double R2_MAX_3 { get; set; }
        //максимальные значения R1 для категорий
        public double R1_SOSR_MAX_1 { get; set; }
        public double R1_SOSR_MAX_2 { get; set; }
        public double R1_SOSR_MAX_3 { get; set; }
        public double K_MAX_D_F_2 { get; set; }
        public double K_MAX_C_F_2 { get; set; }
        public double K_MAX_D_0_2 { get; set; }
        public double K_MAX_C_0_2 { get; set; }
        public double K_MAX_D_F_3 { get; set; }
        public double K_MAX_C_F_3 { get; set; }
        public double K_MAX_D_0_3 { get; set; }
        public double K_MAX_C_0_3 { get; set; }

        public double K_D_F_2 { get; set; }
        public double K_C_F_2 { get; set; }
        public double K_D_0_2 { get; set; }
        public double K_C_0_2 { get; set; }
        public double K_D_F_3 { get; set; }
        public double K_C_F_3 { get; set; }
        public double K_D_0_3 { get; set; }
        public double K_C_0_3 { get; set; }


        private MODE _focusedMode;
        public MODE focusedMode
        {
            get { return _focusedMode; }
            set
            {
                _focusedMode = value;
                RaisePropertyChanged(() => focusedMode);
            }
        }

        public ANTENNA AntennaE
        { get; set; }
        public ANTENNA AntennaH
        { get; set; }
        public ANTENNA AntennaF //антенна для измерения наводок(фаза)
        { get; set; }

        public ANTENNA Antenna0 //антенна для измерения наводок(ноль)
        { get; set; }

        public bool gcMeasuringEnabled
        {
            get
            {
                return selectedMode != null && paramFT != 0;
            }
        }
        public bool gcMeasuringSAZEnabled
        {
            get
            {
                return selectedMode != null && paramFT != 0;// && selectedMode.MODE_RBW != 0 && selectedMode.MODE_RBW_UNIT_ID != 0;
            }
        }

        public bool gcMeasuringDiscr { get; set; }
        public bool gcMeasuringSolid { get; set; }

        // признак автоопределения полосы пропускания ИП                
        public bool? AutoRBW
        {
            get
            {
                if (selectedMode != null)
                    return selectedMode.MODE_AUTORBW;
                return false;
            }
            set
            {
                selectedMode.MODE_AUTORBW = value;
                RaisePropertyChanged(() => AutoRBW);
                allowEditingRBW = !(bool)value;
                readOnlyRBW = (bool)value;

            }
        }
        private void MakeRBW(MEASURING_DATA md)
        {
            if (md.MDA_F == 0 || md.MDA_F_UNIT_ID == 0)
                return;
            string val = Functions.GetUnitValue(Units, (int)md.MDA_F_UNIT_ID);
            double f =
                val == "кГц" ? md.MDA_F : (val == "Гц" ? Math.Round(md.MDA_F / 1000, 3) : Math.Round(md.MDA_F * 1000, 3));

            md.MDA_RBW = MakeRBW(f, selectedMode.MODE_IS_SOLID);
            if (md.MDA_RBW == 0)
            {
                MessageBox.Show("Не определёна RBW для частоты   " + f.ToString());
                return;
            }
            md.MDA_RBW_UNIT_ID = Units.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID;
        }
        private bool _allowEditingRBW;
        public bool allowEditingRBW
        {
            get { return _allowEditingRBW; }
            set
            {
                _allowEditingRBW = value;
                RaisePropertyChanged(() => allowEditingRBW);
            }
        }
        private bool _readOnlyRBW;
        public bool readOnlyRBW
        {
            get { return _readOnlyRBW; }
            set
            {
                _readOnlyRBW = value;
                RaisePropertyChanged(() => readOnlyRBW);
            }
        }
        public double modeKN
        {
            get
            {
                if (selectedMode == null || selectedMode.MODE_TYPE == null)
                    return 1;
                return (double)selectedMode.MODE_TYPE.MT_KN;
            }
        }
        public string armSVT
        {
            get
            {
                if (arm_one == null)
                    return String.Empty;
                if (arm_one.ARM_SVT == null)
                    arm_one.ARM_SVT = "Универсальное";
                return arm_one.ARM_SVT;
            }
            set
            {
                arm_one.ARM_SVT = value;
                RaisePropertyChanged(() => armSVT);
                RaisePropertyChanged(() => armTT);
                RaisePropertyChanged(() => cbeTTEnabled);
            }
        }
        public int? armGS
        {
            get
            {
                if (arm_one == null)
                    return null;
                else
                    return arm_one.ARM_ANT_ID;
            }
            set
            {
                arm_one.ARM_ANT_ID = value;
                RaisePropertyChanged(() => armGS);
                RaisePropertyChanged(() => arm_one);
                if (AntennasGS.Where(p => p.ANT_ID == value && p.ANT_MODEL == "Без ГШ").Any()) //удаление измерений ГШ, если выбрано "Без ГШ"
                {
                    ClearSAZ();
                }
                else
                {
                    if (DialogResult.Yes == MessageBox.Show("Выбран ГШ.Сгенерировать шум для всех режимов АРМ? В случае отказа шум будет сгенерирован во время расчёта ", "", MessageBoxButtons.YesNo))
                        GenerateSAZ();
                    else
                        ResultClear(); //очистка результатов всех режимов  АРМ      
                }
            }
        }
        public string armTT
        {
            get
            {
                if (arm_one == null)
                    return String.Empty;
                return arm_one.ARM_TT;
            }
            set
            {
                arm_one.ARM_TT = value;
                RaisePropertyChanged(() => armTT);
                RaisePropertyChanged(() => cbeIKEnabled);
                RaisePropertyChanged(() => cbeNKEnabled);
            }
        }
        public int armKategoria
        {
            get
            {
                if (arm_one == null)
                    return 2;
                return arm_one.ARM_KATEGORIA;
            }
            set
            {
                arm_one.ARM_KATEGORIA = value;
                RaisePropertyChanged(() => armKategoria);
                switch (armKategoria)
                {
                    //case 1:
                    //    foreach (var result in _Results)
                    //    {
                    //        result.RES_NORMA = (double)result.RES_NORMA_1;
                    //        //  result.RES_R1_RASPR = (double)result.RES_R1_RASPR_1;
                    //        result.RES_R1_SOSR = (double)result.RES_R1_SOSR_1;
                    //        result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_1;
                    //        result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_1;
                    //        result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_1;
                    //    }
                    //    break;
                    case 2:
                        foreach (var result in _Results)
                        {
                            result.RES_NORMA = (double)result.RES_NORMA_2;
                            //   result.RES_R1_RASPR = (double)result.RES_R1_RASPR_2;
                            result.RES_R1_SOSR = (double)result.RES_R1_SOSR_2;
                            result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_2;
                            result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_2;
                            result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_2;
                            result.RES_CARRY_K0 = result.RES_CARRY_K0_2;
                            result.RES_CARRY_KF = result.RES_CARRY_KF_2;
                            result.RES_DRIVE_K0 = result.RES_DRIVE_K0_2;
                            result.RES_DRIVE_KF = result.RES_DRIVE_KF_2;
                            //САЗ
                            result.RES_R2_PORTABLE_1 = (double)result.RES_R2_PORTABLE_DRIVE_1;
                        }

                        break;

                    case 3:
                        foreach (var result in _Results)
                        {
                            result.RES_NORMA = (double)result.RES_NORMA_3;
                            //    result.RES_R1_RASPR = (double)result.RES_R1_RASPR_3;
                            result.RES_R1_SOSR = (double)result.RES_R1_SOSR_3;
                            result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_3;
                            result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_3;
                            result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_3;
                            result.RES_CARRY_K0 = result.RES_CARRY_K0_3;
                            result.RES_CARRY_KF = result.RES_CARRY_KF_3;
                            result.RES_DRIVE_K0 = result.RES_DRIVE_K0_3;
                            result.RES_DRIVE_KF = result.RES_DRIVE_KF_3;
                            //САЗ
                            result.RES_R2_PORTABLE_1 = (double)result.RES_R2_PORTABLE_CARRY_1;

                        }
                        break;

                }
                //обновляем результаты расчёта в соответствии с выбранной категорией и режимом
                RefreshResults();
                // DeleteResults();
            }
        }
        public double paramFT
        {
            get
            {
                if (selectedMode == null || selectedMode.MODE_ID == 0) //
                    return 0;
                //выполняется для не новых строк
                return Functions.F_kGc(selectedMode.MODE_FT, Functions.GetUnitValue(Units, (int)selectedMode.MODE_FT_UNIT_ID));
            }
        }
        public double paramTAU //нсек, исп-ся при расчёте I
        {
            get
            {
                if (selectedMode == null)
                    return 0;
                return Functions.Tau_nsek(selectedMode.MODE_TAU, Functions.GetUnitValue(Units, (int)selectedMode.MODE_TAU_UNIT_ID));
            }

        }
        public double paramD
        {
            get
            {
                if (selectedMode == null)
                    return 0;
                return selectedMode.MODE_D;
            }
            set
            {
                selectedMode.MODE_D = value;
                RaisePropertyChanged(() => paramD);
                RefreshUI();
            }
        }
        public double paramR
        {
            get
            {
                if (selectedMode == null)
                    return 0;

                return selectedMode.MODE_R;
            }
            set
            {
                selectedMode.MODE_R = value;
                RaisePropertyChanged(() => paramR);
                RefreshUI();
            }
        }
        public double paramRMAX
        {
            get
            {
                if (selectedMode == null)
                    return 0;

                return selectedMode.MODE_RMAX;
            }
            set
            {
                selectedMode.MODE_RMAX = value;
                RaisePropertyChanged(() => paramRMAX);
                RefreshUI();
            }
        }
        public string paramSVT
        {
            get
            {
                if (selectedMode == null)
                    return String.Empty;
                return selectedMode.MODE_SVT;
            }
            set
            {
                selectedMode.MODE_SVT = value;
                RaisePropertyChanged(() => paramSVT);
                RefreshKps(Measurings);
            }
        }
        public double paramL
        {
            get
            {
                if (selectedMode == null)
                    return 0;
                return selectedMode.MODE_L;
            }
            set
            {
                selectedMode.MODE_L = value;
                RaisePropertyChanged(() => paramL);
                RefreshKps(Measurings);
            }
        }
        public bool paramAntGs
        {
            get
            {
                if (selectedMode == null)
                    return false;
                return (bool)selectedMode.MODE_ANT_GS;
            }
            set
            {
                selectedMode.MODE_ANT_GS = value;
                RaisePropertyChanged(() => paramAntGs);
                RefreshKps(Measurings);
            }
        }
        public bool cbeArmParamEnabled
        {
            get
            {
                if (arm_one == null)
                    return false;
                return true;
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
        public bool cbeSVTEnabled
        {
            get
            {
                if (arm_one == null)
                    return false;
                return true;
            }
        }
        public bool cbeTTEnabled
        {
            get
            {
                if (arm_one == null)
                    return false;
                if (arm_one.ARM_SVT == "Специализированное")
                    return true;
                else
                {
                    armTT = "";
                    arm_one.ARM_IK = "";
                    arm_one.ARM_NK = "";
                    RaisePropertyChanged(() => arm_one);
                    return false;
                }
            }
        }
        public bool cbeIKEnabled
        {
            get
            {
                if (arm_one == null)
                    return false;
                if (armSVT == "Специализированное" && armTT == "Восстановление графических документов")
                    return true;
                else
                {
                    arm_one.ARM_IK = "";
                    arm_one.ARM_NK = "";
                    RaisePropertyChanged(() => arm_one);
                    return false;
                }
            }
        }
        public bool cbeNKEnabled
        {
            get
            {
                if (arm_one == null)
                    return false;
                if (armSVT == "Специализированное" && armTT == "Чтение чисел")
                    return true;
                else
                {
                    arm_one.ARM_NK = "";
                    arm_one.ARM_IK = "";
                    RaisePropertyChanged(() => arm_one);
                    return false;
                }
            }
        }
        bool _keyCalculate;
        //сбрасывается во время расчёта        
        public bool keyCalculate
        {
            get { return _keyCalculate; }
            set
            {
                _keyCalculate = value;
                RaisePropertyChanged(() => keyCalculate);
                RaisePropertyChanged(() => buttonCalculateEnabled);
                RaisePropertyChanged(() => buttonScenarioEnabled);
            }
        }
        private bool _buttonCalculateEnabled;
        public bool buttonCalculateEnabled
        {
            get
            {
                if (paramD != 0 && paramFT != 0 && paramR != 0 && paramTAU != 0 && Measurings != null && Measurings.Any() && keyCalculate)
                    _buttonCalculateEnabled = true;
                else
                    _buttonCalculateEnabled = false;
                return _buttonCalculateEnabled;
            }
            set
            {
                _buttonCalculateEnabled = value;
                RaisePropertyChanged(() => buttonCalculateEnabled);
            }
        }
        private bool _buttonScenarioEnabled;
        public bool buttonScenarioEnabled
        {
            get
            {
                _buttonScenarioEnabled = keyCalculate;
                return _buttonScenarioEnabled;
            }
            set
            {
                _buttonScenarioEnabled = value;
                RaisePropertyChanged(() => buttonScenarioEnabled);
            }
        }

        public bool buttonCsvEnabled
        {
            get
            {
                if (selectedMode != null)
                    return true;
                else
                    return false;

            }
        }
        private string _myDateTime;
        public string MyDateTime
        {
            get
            {
                _myDateTime = DateTime.Now.ToLongDateString();
                return _myDateTime;
            }
            set
            {
                if (_myDateTime != value)
                {
                    _myDateTime = value;
                    RaisePropertyChanged(() => MyDateTime);
                }
            }
        }
        private MethodsEntities _methodsEntities;
        public MethodsEntities methodsEntities
        {
            get { return _methodsEntities; }
            set
            {
                _methodsEntities = value;
                RaisePropertyChanged(() => methodsEntities);
            }
        }
        private MEASURING_DATA _focusedRow;
        public MEASURING_DATA FocusedRow
        {
            get { return _focusedRow; }
            set
            {
                _focusedRow = value;
                RaisePropertyChanged(() => FocusedRow);
            }
        }
        //выбранная строка таблицы данных измерений
        private MEASURING_DATA _selectedRow;
        public MEASURING_DATA selectedRow //SelectedItem
        {
            get
            {
                return _selectedRow;
            }
            set
            {
                _selectedRow = value;
                RaisePropertyChanged(() => selectedRow);

                if (value != null)
                {
                    //если выделенная строка  - ШП сигнал, то запретим её редактировать
                    if (value.MDA_F_BEGIN != 0 && value.MDA_F_END != 0)
                    {
                        ReadOnly?.Invoke();
                    }
                    else
                    {
                        NotReadOnly?.Invoke();
                    }
                }
            }
        }
        //выбранная строка таблицы режимов
        private ObservableCollection<MODE> _selectedItemsMode;
        public ObservableCollection<MODE> selectedItemsMode
        {
            get
            {
                return _selectedItemsMode == null ? new ObservableCollection<MODE>() : _selectedItemsMode;
            }
            set
            {
                _selectedItemsMode = value;
                RaisePropertyChanged(() => selectedItemsMode);
            }
        }

        private MODE _selectedMode;
        public MODE selectedMode
        {
            get
            {
                return _selectedMode;
            }
            set
            {
                //если с этим режимом работает другой пользователь, выдаётся предупреждение
                var temp = methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME != userName && p.CUT_MODE_ID == value.MODE_ID);
                if (value != null && userName != null && temp.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var n in temp)
                    {
                        sb.Append(n.CUT_USER_NAME + "  ");
                    }
                    MessageBox.Show("С выбранным режимом уже работают пользователи: " + sb.ToString() + "." + Environment.NewLine + " Согласуйте с ними свои намерения.");
                }
                if (value == null && _selectedMode == null)
                    return; //ничего не изменилось

                //обновление после пересоздания контекста
                if (value != null && _selectedMode != null && value.MODE_ID == _selectedMode.MODE_ID)
                {
                    _selectedMode = value;
                    RaisePropertyChanged(() => selectedMode);
                    return; //ничего не изменилось
                }

                //сохранение новой строки
                _selectedMode = value;
                RaisePropertyChanged(() => selectedMode);

                if (value != null)
                {
                    if (refreshAfterConfig)
                        Results = new ObservableCollection<RESULT>(
                               methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == _arm_id));
                    filterResults = "RES_MODE_ID = " + value.MODE_ID.ToString();
                    if (Antennas != null && Antennas.Any())
                    {
                        AntennaE = Antennas.Where(p => p.ANT_ID == value.MODE_ANT_E_ID).FirstOrDefault();
                        AntennaH = Antennas.Where(p => p.ANT_ID == value.MODE_ANT_H_ID).FirstOrDefault();
                    }
                    else
                    {
                        AntennaE = null;
                        AntennaH = null;
                    }
                    InitModeUI();  //обновление Measurings и Results 
                    isSolid = value.MODE_IS_SOLID;
                    ContraintE = value.MODE_CONTR_E;
                }
                else
                {
                    filterResults = "RES_MODE_ID = 0";
                    InitModeUI();  //обновление Measurings и Results 
                    isSolid = false;
                    AntennaE = null;
                    AntennaH = null;
                }

                RaisePropertyChanged(() => gcMeasuringEnabled);
                RaisePropertyChanged(() => gcMeasuringSAZEnabled);
                RaisePropertyChanged(() => modeKN);
                RaisePropertyChanged(() => buttonCsvEnabled);

                if (!String.IsNullOrEmpty(userName))
                    dbSaveSelectedParameter();
            }
        }
        private ANALYSIS _analysis_one;
        public ANALYSIS analysis_one
        {
            get { return _analysis_one; }
            set
            {
                _analysis_one = value;
                RaisePropertyChanged(() => analysis_one);
                RaisePropertyChanged(() => dateBegin);
                RaisePropertyChanged(() => dateEnd);
                RaisePropertyChanged(() => cbeAnalysisParamEnabled);
            }
        }
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
        private ARM _arm_one;
        public ARM arm_one
        {
            get { return _arm_one; }
            set
            {
                _arm_one = value;
                RaisePropertyChanged(() => arm_one);
                //обновить все свойства, связанные с arm_one
                RaisePropertyChanged(() => armGS);
                RaisePropertyChanged(() => armSVT);
                RaisePropertyChanged(() => armTT);
                RaisePropertyChanged(() => armKategoria);
                RaisePropertyChanged(() => buttonCalculateEnabled);
                RaisePropertyChanged(() => cbeIKEnabled);
                RaisePropertyChanged(() => cbeNKEnabled);
                RaisePropertyChanged(() => cbeTTEnabled);
                RaisePropertyChanged(() => cbeSVTEnabled);
                RaisePropertyChanged(() => cbeArmParamEnabled);

            }
        }
        private int _org_id;
        public int org_id
        {
            get { return _org_id; }
            set
            {
                if (_org_id == value)
                    return;
                _org_id = value;
                RaisePropertyChanged(() => org_id);
                if (value == 0 || !methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == value).Any())
                    return;

                //Все исследования одной работы (с разными счетами)     
                if (refreshAfterConfig && Properties.Settings.Default.anlId != 0 && !String.IsNullOrEmpty(Properties.Settings.Default.Invoice)
                    && methodsEntities.ANALYSIS.Where(p => p.ANL_ID == Properties.Settings.Default.anlId).Any())
                    RefreshAnalysis(Properties.Settings.Default.Invoice);
                else
                    RefreshAnalysis(String.Empty);
            }
        }
        //выбранный тип АРМа
        private int _at_id;
        public int at_id
        {
            get { return _at_id; }
            set
            {
                if (_at_id == value)
                    return;
                _at_id = value;
                arm_Number = String.Empty;
                RaisePropertyChanged(() => at_id);
                //Все АРМы выбранного типа  
                if (refreshAfterConfig && _at_id != 0 && Properties.Settings.Default.armId != 0 && methodsEntities.ARM.Where(p => p.ARM_ID == Properties.Settings.Default.armId).Any())
                {
                    try
                    {
                        arm_Number = methodsEntities.ARM.Where(p => p.ARM_AT_ID == _at_id && p.ARM_ID == Properties.Settings.Default.armId).FirstOrDefault().ARM_NUMBER;
                        RefreshArms(arm_Number);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка при выборе типа АРМ" + ". " + e.Message);
                    }
                }
                RefreshArms(arm_Number);
            }
        }
        private string _arm_Number;
        public string arm_Number
        {
            get { return _arm_Number; }
            set
            {
                _arm_Number = value;
                RaisePropertyChanged(() => arm_Number);

                // ??? RefreshArms(value);
            }
        }
        //id исследования выбранного заказчика по выбранному счёту
        private int _anl_id_Search;
        public int anl_id_Search
        {
            get { return _anl_id_Search; }
            set
            {
                _anl_id_Search = value;
                RaisePropertyChanged(() => anl_id_Search);
            }
        }
        private int _anl_id;
        public int anl_id
        {
            get { return _anl_id; }
            set
            {
                if (_anl_id == value)
                    return;
                _anl_id = value;
                RaisePropertyChanged(() => anl_id);
                //заголовочная информация об исследовании
                if (value != 0)
                {
                    analysis_one = methodsEntities.ANALYSIS.Where(p => p.ANL_ID == _anl_id).FirstOrDefault();
                    if (analysis_one != null)
                        if (refreshAfterConfig && Properties.Settings.Default.atId != 0 && methodsEntities.ARM_TYPE.Where(p => p.AT_ID == Properties.Settings.Default.atId).Any())
                            RefreshArmTypes(Properties.Settings.Default.atId); //восстановление предыдущего значения при первоначальной загрузке
                        else
                            RefreshArmTypes(); // типы АРМов для выбранного счёта с выбором первого типа

                }
                else
                {
                    at_id = 0;
                    analysis_one = null;
                    arm_Number = String.Empty;
                }

            }
        }
        private int _arm_id;
        public int arm_id
        {
            get { return _arm_id; }
            set
            {
                //if (_arm_id == value)
                //    return;
                //if (value != 0)

                R2_MAX_D_2 = 0;
                RaisePropertyChanged(() => R2_MAX_D_2);
                _arm_id = value;
                RaisePropertyChanged(() => arm_id);
                //Results = new ObservableCollection<RESULT>(
                //           methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == _arm_id)); // 
                //filterResults = String.Empty;
                RaisePropertyChanged(() => keyModeEnabled);
                if (value != 0)
                {
                    arm_one = methodsEntities.ARM.Where(p => p.ARM_ID == _arm_id).FirstOrDefault();
                    //MakeMaxValue();
                    //ArmEquipments = new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_PARENT_ID == null));
                    //RaisePropertyChanged(() => armSVT);
                    //RaisePropertyChanged(() => armTT);
                    //RaisePropertyChanged(() => cbeTTEnabled);
                }
                else
                    arm_one = null;

                if (refreshAfterConfig && Properties.Settings.Default.modeId != 0 && methodsEntities.MODE.Where(p => p.MODE_ID == Properties.Settings.Default.modeId).Any())
                    RefreshModes(Properties.Settings.Default.modeId);
                else
                    RefreshModes(0);
            }
        }
        private ObservableCollection<ANALYSIS> _analysis;
        public ObservableCollection<ANALYSIS> analysis
        {
            get { return _analysis; }
            set
            {
                _analysis = value;
                RaisePropertyChanged(() => analysis);
            }
        }
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
        private ObservableCollection<ARM> _Arms;
        public ObservableCollection<ARM> Arms
        {
            get { return _Arms; }
            set
            {
                _Arms = value;
                RaisePropertyChanged(() => Arms);
            }
        }
        private ObservableCollection<MODE> _Modes;
        public ObservableCollection<MODE> Modes
        {
            get { return _Modes; }
            set
            {
                _Modes = value;
                RaisePropertyChanged(() => Modes);
            }
        }
        public ObservableCollection<MODE_NPP> ModesForReport { get; set; }
        private ObservableCollection<EQUIPMENT> _ArmEquipments;
        public ObservableCollection<EQUIPMENT> ArmEquipments
        {
            get { return _ArmEquipments; }
            set
            {
                _ArmEquipments = value;
                RaisePropertyChanged(() => ArmEquipments);
            }
        }
        EntityServerModeSource _esms;
        public EntityServerModeSource esms
        {
            get { return _esms; }
            set
            {
                _esms = value;
                RaisePropertyChanged(() => esms);
            }
        }
        EntityServerModeSource _esmsWithSAZ;
        public EntityServerModeSource esmsWithSAZ
        {
            get { return _esmsWithSAZ; }
            set
            {
                _esmsWithSAZ = value;
                RaisePropertyChanged(() => esmsWithSAZ);
            }
        }

        private List<MEASURING_DATA> _Measurings;
        public List<MEASURING_DATA> Measurings
        {
            get { return _Measurings; }
            set
            {
                _Measurings = value;
                RaisePropertyChanged(() => Measurings);
                RaisePropertyChanged(() => buttonCalculateEnabled);


            }
        }

        private ObservableCollection<MEASURING_DATA> _MeasuringsRandom;
        public ObservableCollection<MEASURING_DATA> MeasuringsRandom
        {
            get { return _MeasuringsRandom; }
            set
            {
                _MeasuringsRandom = value;
                RaisePropertyChanged(() => MeasuringsRandom);
            }
        }
        public ObservableCollection<FIDER> Fiders { get; set; }
        public ObservableCollection<MEASURING_DEVICE> Devices { get; set; }
        public ObservableCollection<ANTENNA> Antennas { get; set; }
        public ObservableCollection<ANTENNA> AntennasE { get; set; }
        public ObservableCollection<ANTENNA> AntennasH { get; set; }
        public ObservableCollection<ANTENNA> AntennasGS { get; set; }
        public ObservableCollection<MODE_TYPE> ModeTypes { get; set; }
        public ObservableCollection<EQUIPMENT_TYPE> EquipmentTypes { get; set; }
        public ObservableCollection<PERSON> Persons { get; set; }
        public ObservableCollection<PERSON> Persons_M { get; set; }
        public ObservableCollection<PERSON> Persons_I { get; set; }
        public ObservableCollection<PRODUCER> Producers { get; set; }
        public ObservableCollection<UNIT> Units { get; set; }
        public ObservableCollection<UNIT> UnitsF { get; set; }
        public ObservableCollection<UNIT> UnitsTau { get; set; }
        public ObservableCollection<IP> AllIP { get; set; }
        public ObservableCollection<IP> AllIPHelper { get; set; }
        private string _filterResults { get; set; }
        public string filterResults
        {
            get { return _filterResults; }
            set
            {
                _filterResults = value;
                RaisePropertyChanged(() => filterResults);
            }
        }
        private ObservableCollection<RESULT> _Results;

        public ObservableCollection<RESULT> Results
        {
            get
            {
                return _Results;
            }
            set
            {
                _Results = value;
                if (value != null)
                {
                    switch (armKategoria)
                    {
                        case 1:
                            foreach (var result in _Results)
                            {
                                result.RES_NORMA = (double)result.RES_NORMA_1;
                                //  result.RES_R1_RASPR = (double)result.RES_R1_RASPR_1;
                                result.RES_R1_SOSR = (double)result.RES_R1_SOSR_1;
                                result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_1;
                                result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_1;
                                result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_1;
                            }
                            break;
                        case 2:
                            foreach (var result in _Results)
                            {
                                result.RES_NORMA = (double)result.RES_NORMA_2;
                                // result.RES_R1_RASPR = (double)result.RES_R1_RASPR_2;
                                result.RES_R1_SOSR = (double)result.RES_R1_SOSR_2;
                                result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_2;
                                result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_2;
                                result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_2;
                            }
                            break;

                        case 3:
                            foreach (var result in _Results)
                            {
                                result.RES_NORMA = (double)result.RES_NORMA_3;
                                // result.RES_R1_RASPR = (double)result.RES_R1_RASPR_3;
                                result.RES_R1_SOSR = (double)result.RES_R1_SOSR_3;
                                result.RES_R2_PORTABLE = (double)result.RES_R2_PORTABLE_3;
                                result.RES_R2_PORTABLE_CARRY = (double)result.RES_R2_PORTABLE_CARRY_3;
                                result.RES_R2_PORTABLE_DRIVE = (double)result.RES_R2_PORTABLE_DRIVE_3;
                            }
                            break;
                    }
                }
                RaisePropertyChanged(() => Results);
            }
        }


        #endregion Свойства
        #region Свойства для сценариев    

        public event Action WindowClose; //для закрытия отдельных окон
        public event Action WindowClose1; //для закрытия вложенных окон
        public event Action WindowClose2;
        public event Action WindowClose3;
        public event Action WindowClose4;
        public event Action refreshGcEHs;
        public bool wmuIsEnabled { get; set; }
        WindowMeasuringUpdate wmu;
        bool bigData = false; //признак больших данных для удаления. исп-ся при автовставке 
        //значения корректировки сигнал+шум
        private double _EcnValue;
        public double EcnValue
        {
            get { return _EcnValue; }
            set
            {
                _EcnValue = value;
                RaisePropertyChanged(() => EcnValue);
            }
        }
        private double _EnValue;
        public double EnValue
        {
            get { return _EnValue; }
            set
            {
                _EnValue = value;
                RaisePropertyChanged(() => EnValue);
            }
        }

        private ObservableCollection<int> _IValues;
        public ObservableCollection<int> IValues
        {
            get { return _IValues; }
            set
            {
                _IValues = value;
                RaisePropertyChanged(() => IValues);
            }
        }

        private List<Object> _selectedIValues;
        public List<Object> selectedIValues
        {
            get { return _selectedIValues; }
            set
            {
                _selectedIValues = value;
                RaisePropertyChanged(() => selectedIValues);
            }
        }


        public bool isWord { get; set; }
        private bool _buttonEnabled;
        public bool buttonEnabled
        {
            get { return _buttonEnabled; }
            set
            {
                _buttonEnabled = value;
                RaisePropertyChanged(() => buttonEnabled);
            }
        }
        private IColumnChooserFactory _columnChooserTemplate;
        public IColumnChooserFactory columnChooserTemplate
        {
            get { return _columnChooserTemplate; }
            set
            {
                _columnChooserTemplate = value;
                RaisePropertyChanged(() => columnChooserTemplate);
            }
        }

        WindowMeasuringSolid_1 wmSolid { get; set; }
        WindowMeasuringDiff_1 wmDiff { get; set; }
        public string pathData { get; set; } //путь в файлам автоизмерений
        public string fileForCopyName { get; set; }
        public double fMin { get; set; }
        public double fMax { get; set; }
        public int UMinEn { get; set; }
        public int UMaxEn { get; set; }
        public int UMinn { get; set; }
        public int UMaxn { get; set; }

        private DateTime? _DateBegin;
        public DateTime? DateBegin
        {
            get
            {
                return _DateBegin;
            }
            set
            {
                _DateBegin = value;
                RaisePropertyChanged(() => DateBegin);
                if (search)
                    SearchPrepare();
            }
        }
        private DateTime? _DateEnd;
        public DateTime? DateEnd
        {
            get
            {
                return _DateEnd;
            }
            set
            {
                _DateEnd = value;
                RaisePropertyChanged(() => DateEnd);
                if (search)
                    SearchPrepare();
            }
        }

        private bool _cbDate;
        public bool cbDate
        {
            get
            { return _cbDate; }
            set
            {
                _cbDate = value;
                RaisePropertyChanged(() => _cbDate);
                SearchPrepare();
            }
        }


        private ObservableCollection<ANALYSIS> _analysisSearch;
        public ObservableCollection<ANALYSIS> analysisSearch
        {
            get { return _analysisSearch; }
            set
            {
                _analysisSearch = value;
                RaisePropertyChanged(() => analysisSearch);
            }
        }
        #endregion Свойства Search
        public bool canAuto { get; set; }
        public bool Error { get; set; }
        public bool initialization { get; set; }
        public MainWindowViewModel()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            //для совместимости с коллекциями режима сервера Entity Framework ( EntityServerModeDataSource)
            CriteriaToEFExpressionConverter.EntityFunctionsType = typeof(System.Data.Entity.DbFunctions);
            CriteriaToEFExpressionConverter.SqlFunctionsType = typeof(System.Data.Entity.SqlServer.SqlFunctions);
            initialization = true;
            //для сжатия мусора сразу
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            try
            {
                IncludeSAZ = true;
                dtPrev = DateTime.Now.AddMilliseconds(-1000);

                methodsEntities = new MethodsEntities();
                DXGridDataController.DisableThreadingProblemsDetection = true;//Обнаружение проблем с поточностью отключено, по умолчанию
                methodsEntities.Configuration.AutoDetectChangesEnabled = true; //по умолчанию 
                methodsEntities.Configuration.ValidateOnSaveEnabled = false;
                var processes = Process.GetProcessesByName("SpecialJobsNew");
                //Удаление неактуальных строк текущего состояния
                if (methodsEntities.CurrentUserTask.Any() && processes.Length == 1) //нет загруженных задач, очищаем строки актуальных пользователей в базе

                // && DialogResult.Yes == MessageBox.Show("Обнаружены строки актуальных пользователей БД, подтвердите их удаление, если вы единственный пользователь задачи в настоящее время?", "", MessageBoxButtons.YesNo))

                {
                    methodsEntities.CurrentUserTask.RemoveRange(methodsEntities.CurrentUserTask);
                    SaveData(null);
                }
                Units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
                UnitsF = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("Гц")));

                UnitsTau = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("сек")));
                ModeTypes = new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p => p.MT_NAME));
                RefreshAntennas();
                Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                //Fiders = new ObservableCollection<FIDER>(methodsEntities.FIDER.OrderBy(p => p.F_TYPE));
                Persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
                Persons_M = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("М") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                Persons_I = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("И") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                RaisePropertyChanged(() => Persons_M);
                RaisePropertyChanged(() => Persons_I);

                Producers = (new ObservableCollection<PRODUCER>(methodsEntities.PRODUCER.OrderBy(p => p.PROD_NAME)));
                Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
                userName = GetUserName(); //имя пользователя в строке подключения к БД   
                //проверим, не повторный ли запуск задачи от имени этого пользователя
                var t = methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault();
                if (t != null && processes.Length > 1)
                    if (MessageBox.Show("Уже есть выполняющаяся задача от пользователь с именем " + userName + "." + Environment.NewLine +
                                   "Действительно ли такой пользователь реально работает?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        methodsEntities.CurrentUserTask.Remove(methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault());
                        SaveData(null);
                    }
                    else
                    {
                        MessageBox.Show("Измените имя пользователя в конфигурационном файле и перезапустите задачу.");
                        Error = true;
                        return;
                    }
                //восстановление последнего(перед закрытием задачи) состояния
                refreshAfterConfig = true;
                try
                {
                    ReadConfig();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось восстановить предыдущий выбор. Это не повлияет на дальнейшую работу.");
                }
                refreshAfterConfig = false;
                if (org_id == 0 && Organizations.Any())
                    org_id = Organizations[0].ORG_ID;
                //RefreshOrganization(org_id); //если значение не восстановлено, то выбирается первая работа из списка
                RaisePropertyChanged(() => AutoRBW);
                RaisePropertyChanged(() => tcSelectedItem);

                RaisePropertyChanged(() => cbeIKEnabled);
                RaisePropertyChanged(() => cbeNKEnabled);
                RaisePropertyChanged(() => cbeTTEnabled);
                RaisePropertyChanged(() => cbeSVTEnabled);
                isMultiSelect = false;
                //почистим таблицу  от записей пред.дней
                var temp = methodsEntities.TableUpdated.Where(p => p.DateTimeUpdate < dtPrev);
                if (temp != null && temp.Count() != 0)
                {
                    methodsEntities.TableUpdated.RemoveRange(temp);
                    SaveData(null);
                }
                //userName = GetUserName(); //имя пользователя в строке подключения к БД   
                ////проверим, не повторный ли запуск задачи от имени этого пользователя
                //var t = methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault();
                //if (t != null && processes.Length > 1)
                //     if (MessageBox.Show("Уже есть выполняющаяся задача от пользователь с именем " + userName + "." + Environment.NewLine +
                //                    "Действительно ли такой пользователь реально работает?","",MessageBoxButtons.YesNo) != DialogResult.Yes)
                //{
                //    methodsEntities.CurrentUserTask.Remove(methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault());
                //    SaveData(null);
                //}
                //else
                //{
                //    MessageBox.Show("Измените имя пользователя в конфигурационном файле и перезапустите задачу.");
                //    Error = true;
                //    return;
                //}

            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
                Error = true;
                return;
            }
            //фиксация в БД восстановленного состояния
            dbSaveSelectedParameter();
            #region background
            //автоизмерения-вставка
            backgroundWorkerAuto = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerAuto.DoWork += BackgroundWorkerAuto_DoWork;
            backgroundWorkerAuto.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            //утилита-вставка
            backgroundWorkerUtilite = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerUtilite.DoWork += BackgroundWorkerUtilite_DoWork;
            backgroundWorkerUtilite.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            //Excel-вставка
            backgroundWorkerExcel = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerExcel.DoWork += BackgroundWorkerExcel_DoWork;
            backgroundWorkerExcel.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            //генерация случайных данных измерений
            backgroundWorker = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            backgroundWorkerUF = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerUF.DoWork += BackgroundWorkerUF_DoWork;
            backgroundWorkerUF.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            backgroundWorkerU0 = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerU0.DoWork += BackgroundWorkerU0_DoWork;
            backgroundWorkerU0.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            //для выполнения расчёта
            backgroundWorkerCalculate = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerCalculate.DoWork += BackgroundWorkerCalculate_DoWork;
            backgroundWorkerCalculate.RunWorkerCompleted += bgWorkerCalculate_RunWorkerCompleted;

            //для выполнения расчёта
            backgroundWorkerCalculateMode = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerCalculateMode.DoWork += BackgroundWorkerCalculateMode_DoWork;
            backgroundWorkerCalculateMode.RunWorkerCompleted += bgWorkerCalculate_RunWorkerCompleted;

            //для удаления данных измерения по виду в режиме
            backgroundWorkerDelete = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerDelete.DoWork += BackgroundWorkerDelete_DoWork;
            backgroundWorkerDelete.RunWorkerCompleted += bgWorkerDelete_RunWorkerCompleted;

            //для удаления всех данных измерения в режиме
            backgroundWorkerDeleteAll = new System.ComponentModel.BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorkerDeleteAll.DoWork += BackgroundWorkerDeleteAll_DoWork;
            backgroundWorkerDeleteAll.RunWorkerCompleted += bgWorkerDeleteAll_RunWorkerCompleted;

            //для генерации САЗ
            backgroundWorkerGenerateSAZ = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerGenerateSAZ.DoWork += BackgroundWorkerGenerateSAZ_DoWork;
            backgroundWorkerGenerateSAZ.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            //для очистки САЗ
            backgroundWorkerClearSAZ = new System.ComponentModel.BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorkerClearSAZ.DoWork += BackgroundWorkerClearSAZ_DoWork;
            backgroundWorkerClearSAZ.RunWorkerCompleted += bgWorkerClearSAZ_RunWorkerCompleted;

            //перерасчёт I
            backgroundWorkerI = new System.ComponentModel.BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorkerI.DoWork += BackgroundWorkerI_DoWork;
            backgroundWorkerI.RunWorkerCompleted += bgWorkerI_RunWorkerCompleted;
            //перерасчёт I
            backgroundWorkerE = new System.ComponentModel.BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            //перерасчёт сигналов после изменения вида спектра или опции ограничения сигнала
            backgroundWorkerE.DoWork += BackgroundWorkerE_DoWork;
            backgroundWorkerE.RunWorkerCompleted += bgWorkerE_RunWorkerCompleted;
            //перерасчёт сигналов после модификации
            backgroundWorkerRecalc1 = new System.ComponentModel.BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorkerRecalc1.DoWork += BackgroundWorkerRecalc1_DoWork;
            backgroundWorkerRecalc1.RunWorkerCompleted += bgWorkerRecalc1_RunWorkerCompleted;
            backgroundWorkerRecalc2 = new System.ComponentModel.BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            backgroundWorkerRecalc2.DoWork += BackgroundWorkerRecalc2_DoWork;
            backgroundWorkerRecalc2.RunWorkerCompleted += bgWorkerRecalc2_RunWorkerCompleted;


            #endregion background
            buttonEnabled = true;
            initialization = false;
            keyCalculate = true;
            //таймер для очиски статусбара через 5 сек. после завершения действия
            aTimer = new System.Timers.Timer(5000);
            aTimer.Elapsed += ATimer_Elapsed;

            iTimer = new System.Timers.Timer(5000);
            iTimer.Elapsed += ITimer_Elapsed;

            //запуск потока для синхронизации данных
            dbChanged = new Thread(dbChangedTracker)
            {
                IsBackground = true
            };
            dbChanged.Start();
        }
        //новый объект контекста
        void NewDBContext()
        {
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (!keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничегог делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            try
            {
                WriteConfig(); //запомнили текущее состояние
            }
            catch (Exception e)
            { MessageBox.Show("Ошибка сохранения параметров выбора. " + e.Message); }
            try
            {
                int selectedModeId = selectedMode != null ? selectedMode.MODE_ID : 0;
                methodsEntities = new MethodsEntities();
                if (methodsEntities != null)
                {
                    int gen = GC.GetGeneration(methodsEntities);
                    GC.Collect(gen, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                }
                methodsEntities.Configuration.ValidateOnSaveEnabled = false;

                Units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
                UnitsF = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("Гц")));

                UnitsTau = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("сек")));
                ModeTypes = new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p => p.MT_NAME));
                RefreshAntennas();
                Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                //Fiders = new ObservableCollection<FIDER>(methodsEntities.FIDER.OrderBy(p => p.F_TYPE));
                Persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
                Persons_M = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("М") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                Persons_I = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("И") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                RaisePropertyChanged(() => Persons_M);
                RaisePropertyChanged(() => Persons_I);

                Producers = (new ObservableCollection<PRODUCER>(methodsEntities.PRODUCER.OrderBy(p => p.PROD_NAME)));
                Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
                //восстановление последнего(перед обновлением контекста) состояния
                refreshAfterConfig = true;
                ReadConfig();
                refreshAfterConfig = false;
                if (org_id == 0 && Organizations.Any())
                    org_id = Organizations[0].ORG_ID;
                RaisePropertyChanged(() => AutoRBW);
                RaisePropertyChanged(() => tcSelectedItem);

                RaisePropertyChanged(() => cbeIKEnabled);
                RaisePropertyChanged(() => cbeNKEnabled);
                RaisePropertyChanged(() => cbeTTEnabled);
                RaisePropertyChanged(() => cbeSVTEnabled);
                //восстановление выбранного режима
                if (selectedModeId != 0)
                    RefreshModes(selectedModeId);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message);
                Error = true;
                return;
            }
            //фиксация в БД восстановленного состояния
            //dbSaveSelectedParameter(); //убрала,т.к. строка при пересоздании контекста не поменялась


            if (!keySuspendParent) // функция вызывалась с работающим потоком, который был остановлен
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        #region Новая методика- сплошной спектр
        //отслеживание генерации измерений
        private string _Value;
        public string Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                RaisePropertyChanged(() => Value);
            }
        }
        //асинхронное удаление измерений по виду  и типу ПЭМИ
        public void BackgroundWorkerDelete_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string mdaType;
            if (String.IsNullOrEmpty(MeasuringType))
            {
                MessageBox.Show("Выберите тип поля.");
                return;
            }
            else
                mdaType = (MeasuringType == "E" || MeasuringType == "H") ? MeasuringType : MeasuringType.Contains("Э") ? "E" : "H";
            Value = "Идёт удаление предыдущих значений ";
            IQueryable<MEASURING_DATA> temp;
            bool prev = methodsEntities.Configuration.AutoDetectChangesEnabled;
            methodsEntities.Configuration.AutoDetectChangesEnabled = true;

            //switch (Tag)
            switch (mdaType)
            {
                case "E":
                    //удаление из контекста строк, для которых есть только измерение ПЭМИ или вообще нет измерений
                    try
                    {
                        //удаление всех строк ПЭМИ

                        //temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID);
                        temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                     p.MDA_TYPE == mdaType &&
                                                                     p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                                                                     p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 &&
                                                                     p.MDA_ES_VALUE_IZM == 0);
                        if (temp.Any())
                            if (temp.Count() < 500)
                            {
                                methodsEntities.MEASURING_DATA.RemoveRange(temp);
                                SaveData(null);
                            }
                            else
                            {
                                temp.Delete();
                                bigData = true;
                            }

                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибка удаления данных измерений E из БД. " + ee.Message);

                    }
                    //обнуление измерений ПЭМИ для строк, у которых есть другие виды измерений

                    foreach (var md in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                 p.MDA_TYPE == mdaType &&
                                                                 (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0) &&
                                                                 (p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0 ||
                                                                 p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0 ||
                                                                 p.MDA_ES_VALUE_IZM != 0)))
                    {
                        md.MDA_ECN_VALUE_IZM = 0;
                        md.MDA_ECN_VALUE_IZM_DB = 0;
                        md.MDA_ECN_VALUE_IZM_MKV = 0;
                        md.MDA_EN_VALUE_IZM = 0;
                        md.MDA_EN_VALUE_IZM_DB = 0;
                        md.MDA_EN_VALUE_IZM_MKV = 0;
                        md.MDA_E = 0;
                        md.MDA_KA = 0;
                    }
                    SaveData(null);
                    break;
                case "UF":
                    temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                     p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                                                                     p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 &&
                                                                     p.MDA_ES_VALUE_IZM == 0);
                    if (temp.Any())
                        if (temp.Count() < 500)
                        {
                            methodsEntities.MEASURING_DATA.RemoveRange(temp);
                            SaveData(null);
                        }
                        else
                        {
                            temp.Delete();
                            bigData = true;
                        }
                    //обнуление измерений UF для строк, у которых есть другие виды измерений
                    foreach (var md in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                                (p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0) &&
                                                                                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                                                                                 p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0 ||
                                                                                 p.MDA_ES_VALUE_IZM != 0)))
                    {
                        md.MDA_UFCN_VALUE_IZM = 0;
                        md.MDA_UFCN_VALUE_IZM_DB = 0;
                        md.MDA_UFCN_VALUE_IZM_MKV = 0;
                        md.MDA_UFN_VALUE_IZM = 0;
                        md.MDA_UFN_VALUE_IZM_DB = 0;
                        md.MDA_UFN_VALUE_IZM_MKV = 0;
                        md.MDA_UF = 0;
                        md.MDA_KF = 0;
                    }
                    SaveData(null);
                    break;
                case "U0":
                    //удаление из контекста строк, для которых есть только измерение U0 или вообще нет измерений
                    temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                     p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                                                                     p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                                                                     p.MDA_ES_VALUE_IZM == 0);
                    if (temp.Any())
                        if (temp.Count() < 500)
                        {
                            methodsEntities.MEASURING_DATA.RemoveRange(temp);
                            SaveData(null);
                        }
                        else
                        {
                            temp.Delete();
                            bigData = true;
                        }                    //обнуление измерений U0 для строк, у которых есть другие виды измерений
                    foreach (var md in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                                (p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0) &&
                                                                                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                                                                                 p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0 ||
                                                                                 p.MDA_ES_VALUE_IZM != 0)))
                    {
                        md.MDA_U0CN_VALUE_IZM = 0;
                        md.MDA_U0CN_VALUE_IZM_DB = 0;
                        md.MDA_U0CN_VALUE_IZM_MKV = 0;
                        md.MDA_U0N_VALUE_IZM = 0;
                        md.MDA_U0N_VALUE_IZM_DB = 0;
                        md.MDA_U0N_VALUE_IZM_MKV = 0;
                        md.MDA_U0 = 0;
                        md.MDA_K0 = 0;
                    }
                    SaveData(null);
                    break;
                case "Saz":
                    //удаление из контекста строк, для которых есть только ES или вообще нет измерений

                    temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                     p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 &&
                                                                     p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                                                                     p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0);
                    if (temp.Any())
                        if (temp.Count() < 500)
                        {
                            methodsEntities.MEASURING_DATA.RemoveRange(temp);
                            SaveData(null);
                        }
                        else
                        {
                            temp.Delete();
                            bigData = true;
                        }                    //обнуление измерений Saz для строк, у которых есть другие виды измерений
                    foreach (var md in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                                                                     p.MDA_ES_VALUE_IZM != 0 &&
                                                                    (p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0 ||
                                                                     p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                                                                     p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0)))
                    {
                        md.MDA_EGS_DB = 0;
                        md.MDA_EGS_MKV = 0;
                        md.MDA_ES_VALUE_IZM = 0;
                        md.MDA_ES_VALUE_IZM_DB = 0;
                        md.MDA_ES_VALUE_IZM_MKV = 0;
                    }
                    SaveData(null);
                    break;
            }
            methodsEntities.Configuration.AutoDetectChangesEnabled = prev;
            Value = "Удаление завершено";
            if (e.Argument != null)
                e.Result = e.Argument;
        }

        public void BackgroundWorkerDeleteAll_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (selectedMode == null)  //при удалении вновь созданного режима
                return;
            Value = "Идёт удаление данных измерений";
            // IQueryable<MEASURING_DATA> temp;
            prev = methodsEntities.Configuration.AutoDetectChangesEnabled;
            methodsEntities.Configuration.AutoDetectChangesEnabled = true;
            try
            {
                //methodsEntities.MODE.Remove(selectedMode); //каскаждное удаление и данных измерений - очень медленное сохранение в БД
                var temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == focusedMode.MODE_ID);
                if (temp != null && temp.Any())
                    if (temp.Count() < 500)
                    {
                        methodsEntities.MEASURING_DATA.RemoveRange(temp);
                        SaveData(null);
                    }
                    else
                    {
                        temp.Delete();
                        bigData = true;
                    }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Ошибка удаления данных измерений E из БД. " + ee.Message);
            }

            // methodsEntities.Configuration.AutoDetectChangesEnabled = prev; //перенесём в _Completed
            Value = "Удаление данных измерений завершено";
        }

        //асинхронное выполнение расчёта всех режимов АРМ
        public void BackgroundWorkerCalculate_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            //обновление Modes
            // RefreshModes(selectedMode.MODE_ID);
            foreach (MODE mode in Modes)
            {

                if (mode.RESULT != null && mode.RESULT.Any() || mode.MEASURING_DATA != null && !mode.MEASURING_DATA.Any() || mode.MODE_FT == 0)
                    continue; //выполняем расчёт только для нерассчитанных режимов
                //чистый расчёт
                Value = "Выполняется расчёт режима - '" + mode.MODE_TYPE.MT_NAME + "'";
                CalculateMode(mode, true);
            }

        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Value))
            {
                Value = "";
                return;
            }
            else

                aTimer.AutoReset = false;
        }
        private void ITimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Value))
            {
                return;
            }
            else

                iTimer.AutoReset = false;
        }

        public void BackgroundWorkerCalculateMode_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            Value = "Выполняется расчёт режима - '" + selectedMode.MODE_TYPE.MT_NAME + "'";

            CalculateMode(selectedMode, true); //только чистый расчёт

        }
        //генерация данных измерений в заданных диапазонах
        public void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            fMin = Math.Max(0.009, fMin); //кГц
            fMax = Math.Min(fMax, armKategoria == 1 ? 10000 : 6000); //кГц
            double fCurrent = fMin;
            double step = 0;
            //int k = 0;
            int angle = 0;
            //заполнение массива случайными данными
            // for (int angle = 0; angle < 360; angle += 30)
            // {
            //k++;
            Value = "Выполняется генерация данных";
            List<MEASURING_DATA> coll = new List<MEASURING_DATA>();
            if (fMin < 0.15)
            {
                step = (double)0.2 / 1000; //шаг измерения(полоса пропускания фильтра)
                fCurrent = fMin;
                if (fMax < 0.15)
                {
                    AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                    //continue;
                }
                else // fMax > 0.15
                {
                    AddRandomData(coll, fCurrent, 0.15, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                    step = (double)9 / 1000; //шаг измерения(полоса пропускания фильтра)
                    fCurrent = 0.15;
                    if (fMax < 30)
                    {
                        AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                        //continue;
                    }
                    else //fMamx >=30
                    {
                        AddRandomData(coll, fCurrent, 30, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                        step = (double)120 / 1000; //шаг измерения(полоса пропускания фильтра)
                        fCurrent = 30;
                        if (fMax < 1000)
                        {
                            AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            //  continue;
                        }
                        else //fMax >= 1000
                        {
                            AddRandomData(coll, fCurrent, 1000, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            if (fMax <= 6000)
                            {
                                step = (double)1000 / 1000; //шаг измерения(полоса пропускания фильтра)
                                fCurrent = 1000;
                                AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            }
                        }
                    }
                }
            }
            else //fMin > 0.15
            {
                if (fMin < 30)
                {
                    step = (double)9 / 1000; //шаг измерения(полоса пропускания фильтра)
                    fCurrent = fMin;
                    if (fMax < 30)
                    {
                        AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                        // continue;
                    }
                    else //fMamx >=30
                    {
                        AddRandomData(coll, fCurrent, 30, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                        step = (double)120 / 1000; //шаг измерения(полоса пропускания фильтра)
                        fCurrent = 30;
                        if (fMax < 1000)
                        {
                            AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            //  continue;
                        }
                        else //fMax >= 1000
                        {
                            AddRandomData(coll, fCurrent, 1000, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            step = (double)1000 / 1000; //шаг измерения(полоса пропускания фильтра)
                            fCurrent = 1000;
                            if (fMax <= 6000)
                            {
                                AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                                //  continue;
                            }
                        }
                    }
                }
                else //fMin >=30
                {
                    if (fMin < 1000)
                    {
                        step = (double)120 / 1000; //шаг измерения(полоса пропускания фильтра)
                        fCurrent = fMin;
                        if (fMax < 1000)
                        {
                            AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            //   continue;
                        }
                        else //fMax >= 1000
                        {
                            AddRandomData(coll, fCurrent, 1000, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            step = (double)1000 / 1000; //шаг измерения(полоса пропускания фильтра)
                            fCurrent = 1000;
                            if (fMax <= 6000)
                            {
                                AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                                //  continue;
                            }
                        }
                    }
                    else  // fMin > 1000
                    {
                        step = (double)1000 / 1000; //шаг измерения(полоса пропускания фильтра)
                        fCurrent = fMin;
                        if (fMax <= 6000)
                        {
                            AddRandomData(coll, fCurrent, fMax, step, angle, UMinEn, UMaxEn, UMinn, UMaxn);
                            //  continue;
                        }
                    }
                }


            }
            Value = "Сформировано " + coll.Count().ToString() + " строк. Идёт сохранение";
            //добавление новых строк в контекст
            try
            {
                Update.CopyAndMerge(coll);
            }
            catch (Exception eM)
            {
                MessageBox.Show("Ошибка сожранения данных. " + eM.Message);
            }
        }
        public void BackgroundWorkerUF_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            int count = Measurings.Count();
            var rnd = new Random();
            //заполнение массива случайными данными
            foreach (var md in selectedMode.MEASURING_DATA)
            {
                md.MDA_UFCN_VALUE_IZM = Math.Round((double)rnd.Next(UMinEn * 100, UMaxEn * 100) / 100, 3);
                md.MDA_UFN_VALUE_IZM = Math.Round((double)rnd.Next(UMinn * 100, UMaxn * 100) / 100, 3);
                if (md.MDA_RBW == 0)
                    MakeRBW(md);
                RefreshKF(md);
                Calculate_UFcn(md);
                Calculate_UFn(md);
                RefreshUF(md);
            }
            // RefreshI(selectedMode);
        }
        //ген-ся значвения наводок на частотах измерений ПЭМИ
        public void BackgroundWorkerU0_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            int count = Measurings.Count();
            var rnd = new Random();
            //заполнение массива случайными данными
            foreach (var md in selectedMode.MEASURING_DATA)
            {

                md.MDA_U0CN_VALUE_IZM = Math.Round((double)rnd.Next(UMinEn * 100, UMaxEn * 100) / 100, 3);
                md.MDA_U0N_VALUE_IZM = Math.Round((double)rnd.Next(UMinn * 100, UMaxn * 100) / 100, 3);
                if (md.MDA_RBW == 0)
                    MakeRBW(md);
                RefreshK0(md);
                Calculate_U0cn(md);
                Calculate_U0n(md);
                RefreshU0(md);
            }
            //if (paramTAU != 0)
            //    RefreshI(selectedMode);
        }
        //вставка автоизмерений
        public void BackgroundWorkerAuto_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            Experiment Exp;
            Value = "Чтение данных из файла.";
            try
            {
                Exp = Experiment.LoadFromFile(fileForCopyName);

            }
            catch (Exception ee)
            {
                MessageBox.Show("Ошибка чтения выбранного файла. " + fileForCopyName + ee.Message);
                return;
            }

            MEASURING_DATA md = new MEASURING_DATA();
            Value = "Копирование данных из файла";
            int countExp = Exp.GetReport().Count();
            int fUnitID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID
                        : 0;
            int RBWUnitId = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID
                        : 0;
            List<MEASURING_DATA> coll = new List<MEASURING_DATA>();
            foreach (var it in Exp.GetReport())
            {

                var noise = it.Noise;
                var signal = it.Signal;
                var f = Math.Round(it.Frequency / 1000000, 3);
                if (AppendMode)
                {
                    md = Measurings.Where(p => p.MDA_F == f).FirstOrDefault();
                }
                if (!AppendMode || md == null)
                {
                    md = new MEASURING_DATA()
                    {
                        MDA_F = f,
                        MDA_MODE_ID = selectedMode.MODE_ID,
                        MDA_F_UNIT_ID = fUnitID,
                        MDA_RBW_UNIT_ID = RBWUnitId,
                        MDA_TYPE = MeasuringType.Contains("Элек") ? "E" : "H"
                    };
                    //добавление остальных реквизитов в строку измерений
                    //if (AutoRBW == true)
                    MakeRBW(md);
                    if (paramTAU != 0)
                        RefreshI(md, paramTAU);
                }
                switch (Tag)
                {
                    case "E":
                        md.MDA_ECN_VALUE_IZM = signal;
                        md.MDA_EN_VALUE_IZM = noise;
                        RefreshKa(md);
                        Calculate_Ecn(md);
                        Calculate_En(md);
                        RefreshE(md);
                        break;
                    case "UF":
                        md.MDA_UFCN_VALUE_IZM = signal;
                        md.MDA_UFN_VALUE_IZM = noise;
                        RefreshKF(md);
                        Calculate_UFcn(md);
                        Calculate_UFn(md);
                        RefreshUF(md);
                        break;
                    case "U0":
                        md.MDA_U0CN_VALUE_IZM = signal;
                        md.MDA_U0N_VALUE_IZM = noise;
                        RefreshK0(md);
                        Calculate_U0cn(md);
                        Calculate_U0n(md);
                        RefreshU0(md);
                        break;
                }
                coll.Add(md);
            }
            Value = "Подготовлено для сохраниения " + coll.Count().ToString() + " строк. Идёт сохранение";
            //добавление новых строк в контекст
            try
            {
                Update.CopyAndMerge(coll);
                Value = "Сохранение завершено";
            }
            catch (Exception eM)
            {
                MessageBox.Show("Ошибка сохранения данных. " + eM.Message);
            }
        }
        //вставка автоизмерений, подготовленных утилитой
        public void BackgroundWorkerUtilite_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            Value = "Обработка полученных данных";
            ExchangeContract data = ((ExchangeContract)e.Argument);
            MODE mode = methodsEntities.MODE.Where(p => p.MODE_ID == data.ID).FirstOrDefault();
            if (mode == null)
            {
                MessageBox.Show("В БД отсутствует режим с MODE_ID = " + data.ID.ToString());
                e.Result = -1;
                return;
            }
            MEASURING_DATA md = new MEASURING_DATA();
            Value = "Вставка в БД данных измерений";

            int fUnitID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID
                        : 0;
            int RBWUnitId = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID
                        : 0;
            List<MEASURING_DATA> coll = new List<MEASURING_DATA>();
            for (int i = 0; i < data.Frequencys.Count; i++)
            {
                if (data.Frequencys[i] == 0)
                    continue;
                var noise = data.Noise[i];
                var signal = data.Signal[i];
                var f = Math.Round(data.Frequencys[i] / 1000000, 3);
                //var f = Math.Round(data.Frequencys[i] , 3);
                md = new MEASURING_DATA()
                {
                    MDA_F = f,
                    MDA_MODE_ID = selectedMode.MODE_ID,
                    MDA_F_UNIT_ID = fUnitID,
                    MDA_RBW_UNIT_ID = RBWUnitId,
                    MDA_TYPE = data.Type as string
                };
                //добавление остальных реквизитов в строку измерений
                //if (AutoRBW == true)
                MakeRBW(md);
                if (paramTAU != 0)
                    RefreshI(md, paramTAU);

                md.MDA_ECN_VALUE_IZM = signal;
                md.MDA_EN_VALUE_IZM = noise;
                RefreshKa(md);
                Calculate_Ecn(md);
                Calculate_En(md);
                RefreshE(md);
                coll.Add(md);
            }
            Value = "Подготовлено для сохраниения " + coll.Count().ToString() + " строк. Идёт сохранение";
            //добавление новых строк в контекст
            try
            {
                Update.CopyAndMerge(coll);
                Value = "Сохранение завершено";
            }
            catch (Exception eM)
            {
                MessageBox.Show("Ошибка сохранения данных. " + eM.Message);
            }
            Value = "Выполняется расчёт ";
            CalculateMode(selectedMode, true); //после расчёта обновляется интерфейс главного окна                   
        }

        public void BackgroundWorkerExcel_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            var arRows = dFromExcel.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            double f, Ucn, Un, f_end, Ucn_end, Un_end;
            //int antId = 0;
            //ANTENNA ant = new ANTENNA();
            MEASURING_DATA md = new MEASURING_DATA();

            List<MEASURING_DATA> coll = new List<MEASURING_DATA>();

            if (!isSP)  //одиночный сигнал
            {
                foreach (var row in arRows)
                {
                    var arStr = row.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arStr.Length < 2 || arStr.Length > 3)
                    {
                        MessageBox.Show("Число копируемых столбцов должно быть = 2 или 3");
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    try
                    {
                        f = Double.Parse(arStr[0]);
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибочные данные в строке " + arStr[0] + ". " + ee.Message);
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    try
                    {
                        Ucn = Double.Parse(arStr[1]);
                        if (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Ucn)))
                        {
                            MessageBox.Show("Значение измерения " + Ucn.ToString() + " выходит за пределы допустимого.");
                            continue;
                        }
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибочные данные в строке " + arStr[1] + ". " + ee.Message);
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    if (arStr.Length == 3)
                    {
                        try
                        {
                            Un = Double.Parse(arStr[2]);
                            if (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Un)))
                            {
                                MessageBox.Show("Значение измерения " + Un.ToString() + " выходит за пределы допустимого.");
                                continue;
                            }

                        }
                        catch (Exception ee)
                        {
                            MessageBox.Show("Ошибочные данные в строке " + arStr[2] + ". " + ee.Message);
                            System.Windows.Clipboard.Clear();
                            return;
                        }
                    }
                    else
                        Un = 0;
                    //обработка скопированных данных
                    md = new MEASURING_DATA()
                    {
                        MDA_F = f,
                        MDA_ANGLE = 0,//Int32.Parse(!String.IsNullOrEmpty(angle) ? angle : "0"),
                        MDA_MODE_ID = selectedMode.MODE_ID,
                        MDA_F_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                            ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID : 0,
                        MDA_RBW_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                            ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID : 0,
                        MDA_TYPE = MeasuringType.Contains("Элек") ? "E" : "H"
                    };
                    MakeRBW(md);
                    if (paramTAU != 0)
                        RefreshI(md, paramTAU);
                    coll.Add(md);
                    switch (Tag)
                    {
                        case "E":
                            md.MDA_ECN_VALUE_IZM = Ucn;
                            md.MDA_EN_VALUE_IZM = Un;
                            RefreshKa(md);
                            Calculate_Ecn(md);
                            Calculate_En(md);
                            RefreshE(md);
                            break;
                        case "UF":
                            md.MDA_UFCN_VALUE_IZM = Ucn;
                            md.MDA_UFN_VALUE_IZM = Un;
                            RefreshKF(md);
                            Calculate_UFcn(md);
                            Calculate_UFn(md);
                            RefreshUF(md);
                            break;
                        case "U0":
                            md.MDA_U0CN_VALUE_IZM = Ucn;
                            md.MDA_U0N_VALUE_IZM = Un;
                            RefreshK0(md);
                            Calculate_U0cn(md);
                            Calculate_U0n(md);
                            RefreshU0(md);

                            break;
                        case "Saz":
                            md.MDA_ES_VALUE_IZM = Ucn;
                            Calculate_Es(md);

                            break;
                    }
                }
            }
            else //широкополосный сингал
            {
                int rowCount = arRows.Length;
                if (rowCount % 2 != 0)
                {
                    MessageBox.Show("Количество строк для копирования ШП сигнала д.б.чётным.");
                    return;
                }

                for (int i = 0; i < rowCount; i = i + 2)
                {
                    var arStr = arRows[i].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    var arStr_end = arRows[i + 1].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arStr.Length != 3 || arStr_end.Length != 3)
                    {
                        MessageBox.Show("Число копируемых столбцов должно быть = 3");
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    try
                    {
                        f = Double.Parse(arStr[0]);
                        f_end = Double.Parse(arStr_end[0]);
                        if (f >= f_end)
                        {
                            MessageBox.Show("Начальная частота д.б. меньше конечной. Строка " + arStr[0] + ", " + arStr_end[0] + "  исключена из обработки");
                            continue;
                        }


                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибочные данные в строке " + arStr[0] + "или " + arStr_end[0] + ". " + ee.Message);
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    try
                    {
                        Ucn = Double.Parse(arStr[1]);
                        Ucn_end = Double.Parse(arStr_end[1]);
                        if (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Ucn)) || (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Ucn_end))))
                        {
                            MessageBox.Show("Значения измерений " + Ucn.ToString() + "," + Ucn_end.ToString() + " выходят за пределы допустимого.");
                            continue;
                        }
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибочные данные в строке " + arStr[1] + "или " + arStr_end[1] + ". " + ee.Message);
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    try
                    {
                        Un = Double.Parse(arStr[2]);
                        Un_end = Double.Parse(arStr_end[2]);
                        if (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Un)) || (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * Un_end))))
                        {
                            MessageBox.Show("Значения измерений " + Un.ToString() + "," + Un_end.ToString() + " выходят за пределы допустимого.");
                            continue;
                        }

                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Ошибочные данные в строке " + arStr[2] + "или " + arStr_end[2] + ". " + ee.Message);
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    //обработка скопированных данных
                    double f_SP = Math.Round((f_end + f) / 2, 3); // средняя частота широкополосного сигнала
                                                                  //сигнал+ шум на средней частоте
                    double Ucn_SP =
                        Math.Round(Ucn + (Math.Log10(f_SP) - Math.Log10(f)) * (Ucn_end - Ucn) / (Math.Log10(f_end) - Math.Log10(f)), 3);
                    double Un_SP =
                        Math.Round(Un + (Math.Log10(f_SP) - Math.Log10(f)) * (Un_end - Un) / (Math.Log10(f_end) - Math.Log10(f)), 3);

                    md = new MEASURING_DATA()
                    {
                        MDA_F = f_SP,
                        MDA_F_BEGIN = f,
                        MDA_F_END = f_end,
                        MDA_MODE_ID = selectedMode.MODE_ID,
                        MDA_F_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                            ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID : 0,
                        MDA_RBW_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                            ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID : 0,
                        MDA_TYPE = MeasuringType.Contains("Элек") ? "E" : "H"
                    };
                    coll.Add(md);
                    if (Tag == "E")
                    {
                        md.MDA_ECN_VALUE_IZM = Ucn_SP;
                        md.MDA_EN_VALUE_IZM = Un_SP;
                        md.MDA_ECN_BEGIN = Ucn;
                        md.MDA_ECN_END = Ucn_end;
                        md.MDA_EN_BEGIN = Un;
                        md.MDA_EN_END = Un_end;
                    }
                    else
                    {
                        MessageBox.Show("Данные широкополосного сигнала могут быть введены только на вкладке 'Сигнал+Помеха - Е/рН' ");
                        System.Windows.Clipboard.Clear();
                        return;
                    }
                    if (AutoRBW != true)
                        AutoRBW = true; //для широкополосного сигнала нужна полоса пропускания
                    MakeRBW(md);

                    if (paramTAU != 0)        //добавлено, нужно?
                        RefreshI(md, paramTAU);

                    RefreshKa(md);
                    Calculate_Ecn(md);
                    Calculate_En(md);
                    RefreshE(md);
                }
            }
            //добавление новых строк в контекст
            Value = "Подготовлено для сохраниения " + coll.Count().ToString() + " строк. Идёт сохранение";
            try
            {
                Update.CopyAndMerge(coll);
                Value = "Сохранение завершено";
            }
            catch (Exception eM)
            {
                MessageBox.Show("Ошибка сохранения данных. " + eM.Message);
            }
        }
        public void BackgroundWorkerGenerateSAZ_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            foreach (MODE mode in Modes)
            {
                Value = "Выполняется генерация данных САЗ для режима - '" + mode.MODE_TYPE.MT_NAME + "'";
                GenerateSAZ(mode, true);
            }

        }

        public void BackgroundWorkerClearSAZ_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            foreach (MODE mode in Modes)
            {
                Value = "Выполняется удаление данных САЗ для режима - '" + mode.MODE_TYPE.MT_NAME + "'";
                ClearSAZ(mode);
            }
        }
        public void BackgroundWorkerI_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            Value = "Выполняется перерасчёт I ";
            RefreshI(selectedMode);
        }
        public void BackgroundWorkerE_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            Value = "Выполняется перерасчёт сигналов ";
            RemakeE();
        }
        public void BackgroundWorkerRecalc1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            wmuIsEnabled = false;
            RaisePropertyChanged(() => wmuIsEnabled);
            keyCalculate = false;
            Value = "Выполняется модификация сигналов ";
            //модифицируем С+Ш и Ш в ДБ и МкВ  для всех строк            
            if (selectedIValues == null || selectedIValues.Count == 0)
            {
                methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID).
                       Update(p => new MEASURING_DATA()
                       {
                           MDA_E = 0,
                           MDA_ECN_VALUE_IZM = p.MDA_ECN_VALUE_IZM + EcnValue,
                           MDA_EN_VALUE_IZM = p.MDA_EN_VALUE_IZM + EnValue,
                           MDA_ECN_VALUE_IZM_DB = p.MDA_ECN_VALUE_IZM + EcnValue + p.MDA_KA,
                           MDA_EN_VALUE_IZM_DB = p.MDA_EN_VALUE_IZM + EnValue + p.MDA_KA,
                           MDA_ECN_VALUE_IZM_MKV = Math.Round(Math.Pow(10, 0.05 * (p.MDA_ECN_VALUE_IZM + EcnValue + p.MDA_KA)) / Math.Pow(p.MDA_RBW, 1 / 2f), 3),
                           MDA_EN_VALUE_IZM_MKV = Math.Round(Math.Pow(10, 0.05 * (p.MDA_EN_VALUE_IZM + EnValue + p.MDA_KA)) / Math.Pow(p.MDA_RBW, 1 / 2f), 3)
                       });
            }
            else
            {
                foreach (var i in selectedIValues)
                {
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID && p.MDA_I == (int)i).
                           Update(p => new MEASURING_DATA()
                           {
                               MDA_E = 0,
                               MDA_ECN_VALUE_IZM = p.MDA_ECN_VALUE_IZM + EcnValue,
                               MDA_EN_VALUE_IZM = p.MDA_EN_VALUE_IZM + EnValue,
                               MDA_ECN_VALUE_IZM_DB = p.MDA_ECN_VALUE_IZM + EcnValue + p.MDA_KA,
                               MDA_EN_VALUE_IZM_DB = p.MDA_EN_VALUE_IZM + EnValue + p.MDA_KA,
                               MDA_ECN_VALUE_IZM_MKV = Math.Round(Math.Pow(10, 0.05 * (p.MDA_ECN_VALUE_IZM + EcnValue + p.MDA_KA)) / Math.Pow(p.MDA_RBW, 1 / 2f), 3),
                               MDA_EN_VALUE_IZM_MKV = Math.Round(Math.Pow(10, 0.05 * (p.MDA_EN_VALUE_IZM + EnValue + p.MDA_KA)) / Math.Pow(p.MDA_RBW, 1 / 2f), 3)
                           });
                }
            }
            //расчёт сигналов,мкВ           
        }
        public void BackgroundWorkerRecalc2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            keyCalculate = false;
            double contr = 0;
            if (ContraintE) //есть ограничение по уровню сигнала
                contr = 3;
            Value = "Выполняется расчёт сигналов ";
            //модифицируем С+Ш и Ш в ДБ и МкВ  для всех строк            
            if (selectedIValues == null || selectedIValues.Count == 0)
            {
                methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID && (p.MDA_ECN_VALUE_IZM - p.MDA_EN_VALUE_IZM) > contr).
                  Update(p => new MEASURING_DATA()
                  {
                      MDA_E = Math.Round(
                              Math.Pow(
                              Math.Pow(p.MDA_ECN_VALUE_IZM_MKV, 2) -
                              Math.Pow(p.MDA_EN_VALUE_IZM_MKV, 2), 1 / 2f), 3)
                  });
            }
            else
            {
                foreach (var i in selectedIValues)
                {
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID && p.MDA_I == (int)i && (p.MDA_ECN_VALUE_IZM - p.MDA_EN_VALUE_IZM) > contr).
                  Update(p => new MEASURING_DATA()
                  {
                      MDA_E = Math.Round(
                              Math.Pow(
                              Math.Pow(p.MDA_ECN_VALUE_IZM_MKV, 2) -
                              Math.Pow(p.MDA_EN_VALUE_IZM_MKV, 2), 1 / 2f), 3)
                  });
                }
            }
        }
        public void bgWorkerClearSAZ_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                methodsEntities.Configuration.AutoDetectChangesEnabled = true;
                keyCalculate = true;
                MeasuringsRefresh();
            }
            catch (Exception ee)
            {
                MessageBox.Show("Ошибка сохранения в БД " + ee.Message);
                (sender as BackgroundWorker).CancelAsync();
            }
            Value = "Удаление данных САЗ завершено";
            canAuto = true;
            RefreshGcE?.Invoke();
            refreshGcEHs?.Invoke();
            RefreshGcCollection?.Invoke();
            //запускаем обработчик таймера, который сотрёт через 5 сек. запись
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            if (keySuspend) // функция вызывалась с остановленным потоком
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }


        public void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (dataList != null && dataList.Any() && dataList.Where(p => p.ID == selectedMode.MODE_ID).FirstOrDefault() != null)
                try
                {
                    if (dataList.Count == 1 && selectedMode != null && selectedMode.MODE_R2 != 0)
                        client.SendR2(selectedMode.MODE_ID, selectedMode.MODE_R2);

                    //удаляем обработанные данные из очереди
                    dataList.RemoveAll(p => p.ID == selectedMode.MODE_ID);
                    //список не обработанных ID
                    var idList = dataList.Select(p => p.ID).Distinct();
                    if (idList.Count() != 0)
                    {
                        PrepareDataFromClient(dataList.Where(p => p.ID == idList.FirstOrDefault()).FirstOrDefault() as ExchangeContract);
                        return;
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Ошибка обновления интерфейса " + ee.Message);
                    (sender as BackgroundWorker).CancelAsync();
                }
            methodsEntities.Configuration.AutoDetectChangesEnabled = true;
            //расчёт I выполняется при заполнении строк, иначе очень медленно
            Value = "Обновление интерфейса";
            MeasuringsRefresh();
            keyCalculate = true;


            canAuto = true;

            RefreshGcE?.Invoke();
            RefreshGcSaz?.Invoke();
            RefreshGcCollection?.Invoke();
            RefreshGcModes?.Invoke();
            MakeMaxValue();
            Results = new ObservableCollection<RESULT>(methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == arm_one.ARM_ID));
            //обновим таблицы
            RefreshGcResults?.Invoke();

            keyInsert = String.Empty;
            Clipboard.Clear();
            //запускаем обработчик таймера, который сотрёт через 5 сек. запись
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        public void bgWorkerDeleteAll_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int modeId = selectedMode.MODE_ID;
            if (e.Error != null)
            {
                Value = "Ошибка удаления. " + e.Error.Message;
            }
            else
            {
                if (bigData) //было удаление из LINQ
                {
                    NewDBContext();
                    bigData = false;
                }
                Value = "Удаление данных измерений завершено.";
                keyCalculate = true;
                //MeasuringsRefresh();
            }
            //удаляем режим
            MODE temp = methodsEntities.MODE.Where(p => p.MODE_ID == modeId).FirstOrDefault();
            if (temp != null)
            {
                methodsEntities.MODE.Remove(temp); //удаление из контекста. Удаляем не selectedMode, т.к. он = null для вновь созданного и сразу удаляемого режима
                SaveData(null);
            }
            Modes = new ObservableCollection<MODE>(methodsEntities.MODE.Where(p => p.MODE_ARM_ID == arm_id));
            if (Modes.Any())
                selectedMode = Modes[0];
            else
            {
                selectedMode = null;
                RaisePropertyChanged(() => gcMeasuringEnabled);
                RaisePropertyChanged(() => gcMeasuringSAZEnabled);
            }
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            RefreshGcModes?.Invoke();
        }
        public void bgWorkerI_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Value = "Ошибка перерасчёта I " + e.Error.Message;
            }
            else
            {
                //пересоздадим контекст
                NewDBContext();
                ModeResultClear(selectedMode, false);//очистка результатов рассчёта с сохранением в БД
                                                     // SaveData(null); //сохранено в пред строке
                MeasuringsRefresh();
                keyCalculate = true;
                RefreshGcResults?.Invoke();
                RefreshGcCollection?.Invoke();
                Value = "Перерасчёт I завершен";
            }
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        public void bgWorkerE_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            if (e.Error != null)
            {
                Value = "Ошибка перерасчёта сигналов " + e.Error.Message;
            }
            else
            {
                //перезагрузка контекста
                NewDBContext();
                keyCalculate = true;
                RefreshGcCollection?.Invoke();
                Value = "Перерасчёт сигналов  завершен";
                RefreshGcModes?.Invoke();
                RaisePropertyChanged(() => buttonScenarioEnabled);
            }
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            if (!keySuspendParent) //поток не был остановлен до вызова ф-ии, поэтому его восстанавливаем
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                keySuspend = false;
            }
        }
        public void bgWorkerRecalc1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            if (e.Error != null)
            {
                MessageBox.Show("Ошибка модификации сигналов " + e.Error.Message);
            }
            else
                backgroundWorkerRecalc2.RunWorkerAsync();
        }
        public void bgWorkerRecalc2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Ошибка модификации сигналов " + e.Error.Message);
            }
            else
            {
                //перезагрузка контекста
                NewDBContext();
                ModeResultClear(selectedMode, false);
                //MeasuringsRefresh();  
                RefreshGcE?.Invoke();
                RefreshGcCollection?.Invoke();
                Value = "Модификация сигналов завершена";
                RefreshGcModes?.Invoke();
                RaisePropertyChanged(() => buttonScenarioEnabled);
                if (wmu != null && wmu.IsActive)
                {
                    wmu.Close();
                    wmu = null;
                }
            }
            keyCalculate = true;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            if (!keySuspendParent) //поток не был остановлен до вызова ф-ии, поэтому его восстанавливаем
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                keySuspend = false;
            }
        }

        public void bgWorkerDelete_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                Value = "Ошибка удаления. " + e.Error.Message;
                keyInsert = String.Empty;
            }
            else
            {
                if (bigData) //было удаление из LINQ
                {
                    NewDBContext();
                    bigData = false;
                }
                Value = "Удаление предыдущих измерений завершено.";

                if (String.IsNullOrEmpty(keyInsert)) //удаление само по себе, не в составе вставки с заменой
                {
                    MeasuringsRefresh();
                }
                else
                    Measurings = new List<MEASURING_DATA>();
            }

            canAuto = true;
            //очистка R2 в режиме
            if (selectedMode.MODE_R2 != 0)
            {
                selectedMode.MODE_R2 = 0;
                RefreshGcModes?.Invoke();
            }
            RefreshGcE?.Invoke();
            RefreshGcCollection?.Invoke();
            //удаление результатов рассчёта
            if (selectedMode.RESULT != null && selectedMode.RESULT.Any())
                ModeResultClear(selectedMode, false);
            if (AutoRBW == false)  //полоса пропускания будет добавлена автоматически при добавлении данных измерений
                AutoRBW = true;
            //после удаления предыдущих измерений вызываем соответствующую вставку
            switch (keyInsert)
            {
                case "Utilite":
                    Utilite_Insert(e.Result);
                    break;
                case "Auto":
                    GetDataAfterMeasuringAuto_Insert();
                    break;
                case "Excel":
                    PasteFromExcel_Insert();
                    break;
                case "Random":
                    switch (Tag)
                    {
                        case "E":
                            backgroundWorker.RunWorkerAsync();
                            break;
                        case "UF":
                            backgroundWorkerUF.RunWorkerAsync();
                            break;
                        case "U0":
                            backgroundWorkerU0.RunWorkerAsync();
                            break;
                    }
                    break;
            }

            //запускаем обработчик таймера, который сотрёт сообщение через 5 сек. запись
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            keyCalculate = true;
        }
        public void bgWorkerCalculate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Value = "Не удалось выполнить расчёт." + e.Error.Message;
            else
            {
                Value = "Расчёт выполнен";

            }
            //запускаем обработчик таймера, который сотрёт сообщение через 5 сек. запись
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            RaisePropertyChanged(() => selectedMode);
            MakeMaxValue(); //формирование и сохранение в БД max R2 в режиме и АРМе
            //обновим таблицы
            RefreshGcResults?.Invoke();
            //int selectedModeId = selectedMode != null ? selectedMode.MODE_ID : 0;
            //RefreshModes(selectedModeId);

            filterResults = "RES_MODE_ID = " + selectedMode.MODE_ID.ToString();

            MeasuringsRefresh();
            keyCalculate = true;
            //обновим таблицы            
            RefreshGcCollection?.Invoke();
            RefreshGcResults?.Invoke();
            RaisePropertyChanged(() => selectedMode);
            RefreshGcModes?.Invoke(); //для обновления R2Max
            if (keySuspend && !keyWindowMeasuring_4 && !isMultiSelect) // функция вызывалась с остановленным фоновым потоком не в режиме автоизмерений
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }


        private bool canRandomData(Object o)
        {
            return fMin != 0 && fMax != 0 && fMin <= fMax &&
                    UMinEn != 0 && UMaxEn != 0 && UMinEn < UMaxEn &&
                    UMinn != 0 && UMaxn != 0 && UMinn < UMaxn;

        }
        private bool canRandomUF0Data(Object o)
        {
            return UMinEn != 0 && UMaxEn != 0 && UMinEn < UMaxEn &&
                   UMinn != 0 && UMaxn != 0 && UMinn < UMaxn;
        }
        private bool canGetDataAfterMeasuringAuto(object modeId)
        {
            return canAuto;
        }
        private void GetDataAfterMeasuring_Delete(Object o)
        {
            if (backgroundWorkerDelete.IsBusy)
                Thread.Sleep(1000);
            backgroundWorkerDelete.RunWorkerAsync(o);
        }
        private void GetDataAfterMeasuringAuto_Insert()
        {
            backgroundWorkerAuto.RunWorkerAsync();
        }
        private void Utilite_Insert(object o)
        {
            backgroundWorkerUtilite.RunWorkerAsync(o);
        }
        //копирование данных из файла, сформированного в процессе автоизмерений
        private void GetDataAfterMeasuringAuto(object o)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            pathData = Properties.Settings.Default.pathData;
            if (!String.IsNullOrEmpty(pathData))
                ofd.InitialDirectory = pathData;
            else
                ofd.InitialDirectory = Environment.CurrentDirectory;
            if (ofd.ShowDialog() == DialogResult.Cancel)
                return;
            canAuto = false;
            fileForCopyName = ofd.FileName;
            //сохранение выбранной директории в настройках
            if (pathData != Path.GetDirectoryName(fileForCopyName))
            {
                pathData = Path.GetDirectoryName(fileForCopyName);
                Properties.Settings.Default.pathData = pathData;
                Properties.Settings.Default.Save();
            }
            //удалим все предыдущие измерения в режиме и результаты рассчёта
            bool keyCurrentMeasurings = false;
            switch (Tag)
            {
                case "E":
                    keyCurrentMeasurings = Measurings.Where(p => p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0).Any();
                    break;
                case "UF":
                    keyCurrentMeasurings = Measurings.Where(p => p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0).Any();
                    break;
                case "U0":
                    keyCurrentMeasurings = Measurings.Where(p => p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0).Any();
                    break;
                case "Saz":
                    keyCurrentMeasurings = Measurings.Where(p => p.MDA_ES_VALUE_IZM != 0).Any();
                    break;
            }
            if (keyCurrentMeasurings && !AppendMode)
            {
                keyInsert = "Auto";
                try
                {
                    GetDataAfterMeasuring_Delete(o); //асинхронное удаление, после окончания вызывается асинхронная вставка
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                    return;
                }
            }
            else
                GetDataAfterMeasuringAuto_Insert();  //когда не надо удалять, только вызываем асинхронную вставку                       
        }



        private bool canMeasuringsUtilite(Object o)
        {
            return isMultiSelect && Modes.Any();
        }
        private void MeasuringsUtilite(Object o)
        {

            //buttonAutoMeasuring?.Invoke(); //заполнение коллекции выбранных режимов
            if (!selectedItemsMode.Any())
            {
                MessageBox.Show("Выберите режимы, для которых будут проводиться измерения.");
                return;
            }

            keySuspend = true; //фоновый поток приостановили. Во всех вложеных ф-ях с потоком ничего не делаем
#pragma warning disable CS0618 // Type or member is obsolete
            dbChanged.Suspend();//остановим фоновый поток, т.к. даные измерений в реальном времени другим пользователям не нужны
#pragma warning restore CS0618 // Type or member is obsolete

            //проверим, запущена ли утилита. Если нет - запустим
            string processName = "PeminSpectrumAnalyser";

            var processExists = Process.GetProcesses().Any(p => p.ProcessName == processName);
            if (!processExists)
            {

                OpenFileDialog ofd = new OpenFileDialog()
                {
                    InitialDirectory = Properties.Settings.Default.pathTemplateUtilite,
                    RestoreDirectory = false
                };
                if (ofd.ShowDialog() == DialogResult.Cancel)
                {

                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    return;
                }
                int index = ofd.FileName.LastIndexOfAny("\\".ToCharArray());
                Properties.Settings.Default.pathTemplateUtilite = ofd.FileName.Substring(0, index + 1);
                Properties.Settings.Default.Save();
                string processPath = ofd.FileName;
                var startInfo = new ProcessStartInfo(processPath);

                try
                {
                    Process.Start(startInfo);
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка запуска утилиты измерений. " + e.Message);
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    return;
                }
            }
            //подключение к утилите и передача списка режимов для выполнения измерений
            if (client == null)
            {
                try
                {
                    client = new PIPEClient();

                }
                catch (Exception e)
                {
                    MessageBox.Show("Не удалось подключиться к утилите измерений.");
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    return;
                }
                client.IncomingExchangeContract += PrepareDataFromClient;
            }
            //пересылаем ID и название режима по одному из выбранных
            //каждый выбранный режим фиксируем в БД (чтобы с ним не могли работать другие пользователи)
            //предварительно удалив из БД строки предыдущего выбора
            var selectedModes = methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName);
            if (selectedModes != null)
            {
                methodsEntities.CurrentUserTask.RemoveRange(selectedModes);
                SaveData(null);
            }
            try
            {
                foreach (MODE row in selectedItemsMode)
                {
                    client.SendExchangeContract(new ExchangeContract() { ID = row.MODE_ID, Description = row.MODE_TYPE.MT_NAME });
                    //фиксируем переданный на измерение режим в БД
                    methodsEntities.CurrentUserTask.Add(new CurrentUserTask()
                    {
                        CUT_USER_NAME = userName + "meas",
                        CUT_ARM_ID = arm_id,
                        CUT_AT_ID = at_id,
                        CUT_ANL_ID = anl_id,
                        CUT_ORG_ID = org_id,
                        CUT_MODE_ID = row.MODE_ID
                    });
                    //if (!methodsEntities.Configuration.AutoDetectChangesEnabled)
                    //    methodsEntities.Entry(t).State = EntityState.Added;                               
                }
                SaveData(null);
            }
            catch (Exception e) //пересоздание клиента
            {
                try
                {
                    client = new PIPEClient();

                }
                catch (Exception ee)
                {
                    MessageBox.Show("Не удалось подключиться к утилите измерений.");
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    return;
                }
                if (client.IncomingExchangeContract == null)
                    client.IncomingExchangeContract += PrepareDataFromClient;
                foreach (MODE row in selectedItemsMode)
                {
                    client.SendExchangeContract(new ExchangeContract() { ID = row.MODE_ID, Description = row.MODE_TYPE.MT_NAME });
                }

            }
            //если получены результаты измерений, разложим их по местам
        }

        void PrepareDataFromClient(ExchangeContract data)
        {
            if (data != null && data.ID != 0 && Modes != null && !Modes.Where(p => p.MODE_ID == data.ID).Any())//нет режима в списке режимов, возможно его удалили, некуда добавлять данные измерений
            {
                return;
            }
            //проверка полученных данных
            if (data.ID == 0)
            {
                MessageBox.Show("Незаполнено поле ID");
                return;
            }
            if (String.IsNullOrEmpty(data.Type))
                data.Type = "E";
            MeasuringType = data.Type as string;
            if (data.Frequencys == null || !data.Frequencys.Any())
            {
                MessageBox.Show("Незаполнен  список частот");
                return;
            }
            if (data.Signal == null || !data.Signal.Any())
            {
                MessageBox.Show("Незаполнен  список сигналов");
                return;
            }
            if (data.Noise == null || !data.Noise.Any())
            {
                MessageBox.Show("Незаполнен  список шумов");
                return;
            }
            if (data.Signal.Count != data.Frequencys.Count || data.Frequencys.Count != data.Noise.Count)
            {
                MessageBox.Show("Количество строк в списках не совпадает");
                return;
            }

            //добавляем данные в очередь
            if (dataList == null)
            {
                dataList = new List<ExchangeContract>();
                dataList.Add(data);
            }
            if (!dataList.Where(p => p.ID == data.ID).Any()) //ф-я вызвана не из очереди
                dataList.Add(data);
            if (backgroundWorkerDelete.IsBusy || backgroundWorkerUtilite.IsBusy)
            {
                return;
            }
            //if (data.ID == 0)
            //{
            //    MessageBox.Show("Незаполнено поле ID");
            //    return;
            //}
            //if (String.IsNullOrEmpty(data.Type))
            //    data.Type = "E";         
            //MeasuringType = data.Type as string;
            //if (data.Frequencys == null || !data.Frequencys.Any())
            //{
            //    MessageBox.Show("Незаполнен  список частот");
            //    return;
            //}
            //if (data.Signal == null || !data.Signal.Any())
            //{
            //    MessageBox.Show("Незаполнен  список сигналов");
            //    return;
            //}
            //if (data.Noise == null || !data.Noise.Any())
            //{
            //    MessageBox.Show("Незаполнен  список шумов");
            //    return;
            //}
            //if (data.Signal.Count != data.Frequencys.Count || data.Frequencys.Count != data.Noise.Count )
            //{
            //    MessageBox.Show("Количество строк в списках не совпадает");
            //    return;
            //}
            selectedMode = methodsEntities.MODE.Where(p => p.MODE_ID == data.ID).FirstOrDefault();
            if (selectedMode == null)
            {
                //ситуация не реальная, т.к. наличие режима проверяется в начале ф-ии
            }
            //удаление пред. измерений, вставка новых записей 
            //удалим все предыдущие измерения в режиме и результаты рассчёта
            bool keyCurrentMeasurings = false;

            //switch ((string)data.Type)
            //{
            //    case "E":
            keyCurrentMeasurings = Measurings.Where(p => (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0) && p.MDA_TYPE == (string)data.Type).Any();
            //  break;
            //case "UF":
            //    keyCurrentMeasurings = Measurings.Where(p => p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0).Any();
            //    break;
            //case "U0":
            //    keyCurrentMeasurings = Measurings.Where(p => p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0).Any();
            //    break;
            //case "Saz":
            //    keyCurrentMeasurings = Measurings.Where(p => p.MDA_ES_VALUE_IZM != 0).Any();
            //    break;
            //  }
            if (keyCurrentMeasurings && !data.Added)
            {
                keyInsert = "Utilite";
                try
                {
                    while (backgroundWorkerDelete.IsBusy || backgroundWorkerUtilite.IsBusy)
                    {
                        Thread.Sleep(1000);
                    }
                    GetDataAfterMeasuring_Delete(data); //асинхронное удаление, после окончания вызывается асинхронная вставка
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                    return;
                }
            }
            else
            {
                backgroundWorkerUtilite.RunWorkerAsync(data); //когда не надо удалять, только вызываем асинхронную вставку                       
            }

        }
        private void RandomData(Object o) // частоты, МГц
        {
            keyInsert = "Random";
            methodsEntities.Configuration.AutoDetectChangesEnabled = false;
            //удалим все предыдущие измерения в режиме и результаты рассчёта
            List<MEASURING_DATA> currentMeasurings = Measurings;
            switch (Tag)
            {
                case "E":
                    //currentMeasurings == Measurings, т.е. все строки ПЭМИ                    
                    //currentMeasurings = new ObservableCollection<MEASURING_DATA>(Measurings.Where(p => p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0));
                    break;
                case "UF":
                    currentMeasurings = Measurings.Where(p => p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "U0":
                    currentMeasurings = Measurings.Where(p => p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "Saz":
                    currentMeasurings = Measurings.Where(p => p.MDA_ES_VALUE_IZM == 0).ToList<MEASURING_DATA>();
                    break;

            }
            if (currentMeasurings.Any() && !AppendMode && Tag == "E") //удаляются данные только измерений ПЭМИ. Наводки формируются на частотах ПЭМИ
            {
                try
                {
                    backgroundWorkerDelete.RunWorkerAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                    return;
                }
            }
            else
                switch (Tag)
                {
                    case "E":
                        backgroundWorker.RunWorkerAsync();
                        break;
                    case "UF":
                        backgroundWorkerUF.RunWorkerAsync();
                        break;
                    case "U0":
                        backgroundWorkerU0.RunWorkerAsync();
                        break;
                }
        }
        private void AddRandomData(List<MEASURING_DATA> coll, double fCurrent, double fMax, double step, int angle, int UMinEn, int UMaxEn, int UMinn, int UMaxn)
        {
            var rnd = new Random();
            MEASURING_DATA md;
            //int antId = 0;
            //ANTENNA ant = new ANTENNA();
            //List<MEASURING_DATA> coll = new List<MEASURING_DATA>();
            while (fCurrent < fMax)
            {
                md = new MEASURING_DATA()
                {
                    MDA_F = Math.Round(fCurrent, 6),
                    MDA_ANGLE = angle,
                    MDA_MODE_ID = selectedMode.MODE_ID,
                    MDA_F_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID : 0,
                    MDA_RBW_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID : 0,
                    MDA_ECN_VALUE_IZM = Math.Round((double)rnd.Next(UMinEn * 100, UMaxEn * 100) / 100, 3),
                    MDA_EN_VALUE_IZM = Math.Round((double)rnd.Next(UMinn * 100, UMaxn * 100) / 100, 3),
                    MDA_TYPE = MeasuringType.Contains("Элек") ? "E" : "H"
                };
                if (paramTAU != 0)
                {
                    RefreshI(md, paramTAU);
                    SaveData(null);
                }
                coll.Add(md);
                //добавление остальных реквизитов в строку измерений                
                if (AutoRBW == true)
                    MakeRBW(md);
                //if (antId == 0)
                //{
                //    IT(md);//антенна + коэф.кал
                //    antId = (int)md.MDA_ANT_ID;
                //    ant = md.ANTENNA;
                //}
                //else
                //{
                //    md.MDA_ANT_ID = antId;
                //    md.ANTENNA = ant;
                //}
                RefreshKa(md);
                Calculate_Ecn(md);
                Calculate_En(md);
                RefreshE(md);
                fCurrent += step; //след. шаг измерения                
            }
        }
        //генерация данных наводок на фазный провод
        private void RandomUFData(Object o) // частоты, МГц
        {
            Value = "Удаление данных предыдущей генерации";
            //очистим все предыдущие измерения в режиме и результаты рассчёта
            try
            {
                ClearMeasuringDataMode_UF(FocusedRow);

            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                return;
            }
            backgroundWorkerUF.RunWorkerAsync();
        }
        private void RandomU0Data(Object o) // частоты, МГц
        {
            Value = "Удаление данных предыдущей генерации";
            //очистим все предыдущие измерения в режиме и результаты рассчёта
            try
            {
                ClearMeasuringDataMode_U0(FocusedRow);

            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                return;
            }
            backgroundWorkerU0.RunWorkerAsync();
        }

        #endregion Новая методика

        public void solidChanged(bool isSolid)
        {
            this.isSolid = isSolid;
            if (selectedMode != null)
            {
                selectedMode.MODE_IS_SOLID = isSolid;
            }
        }
        public void contrEChanged(bool contrE)
        {
            ContraintE = contrE;
            if (selectedMode != null)
                selectedMode.MODE_CONTR_E = contrE;
        }
        //имя пользователя из строки подключения в конфиг-файле
        private string GetUserName()
        {
            string str = ConfigurationManager.ConnectionStrings["MethodsEntities"].ConnectionString;
            foreach (var t in str.Split(";".ToCharArray()))
            {
                if (!t.Contains("user"))
                    continue;
                else
                {
                    return (t.Substring(t.IndexOf("=") + 1)).Trim();
                }
            }
            return String.Empty;
        }
        //читаем сохранённые настройки из конфига, если они имеются или создаём для них в конфиге переменные
        public void ReadConfig()
        {
            if (Properties.Settings.Default.orgId != 0 && Properties.Settings.Default.orgId != 0 && methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == Properties.Settings.Default.orgId).Any())
                if (methodsEntities.ORGANIZATION != null && methodsEntities.ORGANIZATION.Count() != 0 &&
                    methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == Properties.Settings.Default.orgId).Any())
                    org_id = Properties.Settings.Default.orgId;

            //остальное восстановится по цепочке
        }
        //сохранение настроек при закрытии формы
        public void WriteConfig()
        {
            Properties.Settings.Default.orgId = org_id;
            Properties.Settings.Default.anlId = anl_id;
            Properties.Settings.Default.atId = at_id;
            Properties.Settings.Default.armId = arm_id;
            Properties.Settings.Default.Invoice = analysis_one != null ? analysis_one.ANL_INVOICE : String.Empty;
            if (selectedMode != null)
                Properties.Settings.Default.modeId = selectedMode.MODE_ID;
            Properties.Settings.Default.Save();
            //Удаление строки в таблице выбранных позиций
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).Any())
            {
                methodsEntities.CurrentUserTask.Remove(methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault());
                SaveData(null);
            }
        }
        //ф-я для отслеживания в отдельном потоке изменений в БД, выполненных другими пользователями и обновления интерфейса
        private void dbChangedTracker()
        {
            while (true)
            {
                try
                {
                    var temp = methodsEntities.TableUpdated.Where(p => p.DateTimeUpdate > dtPrev && p.UserName != userName).
                        GroupBy(s => s.TableName, (k, g) => g.Select(s => new
                        {
                            TableName = s.TableName,
                            maxData = g.Max(t => t.DateTimeUpdate)
                        })).SelectMany(a => a).ToList();
                    if (temp != null && temp.Count() != 0)
                    {
                        refreshAfterReload = true; // для того, чтобы сохранять выбор в таблицах при синхронизации локальной БД
                        foreach (var row in temp)
                        {
                            switch (row.TableName.Trim())
                            {
                                case "ANALYSIS":
                                    if (analysis_one != null)
                                        RefreshAnalysis(analysis_one.ANL_INVOICE);
                                    else
                                        RefreshAnalysis(String.Empty);
                                    break;
                                case "ORGANIZATION":
                                    RefreshOrganization(org_id);
                                    break;
                                case "ARM":
                                    RefreshArms(String.Empty);
                                    break;
                                case "ARM_TYPE":
                                    RefreshArmTypes(at_id);
                                    break;
                                case "MODE":
                                    if (selectedMode != null)
                                    {
                                        methodsEntities.Entry<MODE>(selectedMode).Reload(); //принудительная перезагрузка изменённой строки режима
                                        RefreshModes(selectedMode.MODE_ID);
                                    }
                                    break;
                                case "MODE_TYPE":
                                    ModeTypes = new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p => p.MT_NAME));
                                    RaisePropertyChanged(() => ModeTypes);
                                    break;
                                case "ANTENNA":
                                    RefreshAntennas();
                                    break;
                                case "ANTENNA_CALIBRATION":
                                    RefreshAntennas();
                                    break;
                                case "PERSON":
                                    Persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
                                    Persons_M = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("М") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                                    Persons_I = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("И") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                                    RaisePropertyChanged(() => Persons_M);
                                    RaisePropertyChanged(() => Persons_I);
                                    RaisePropertyChanged(() => Persons);
                                    break;
                                case "UNIT":
                                    Units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
                                    RaisePropertyChanged(() => Units);
                                    UnitsF = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("Гц")));
                                    RaisePropertyChanged(() => UnitsF);
                                    UnitsTau = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("сек")));
                                    RaisePropertyChanged(() => UnitsTau);
                                    break;
                                case "MEASURING_DEVICE":
                                    Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                                    RaisePropertyChanged(() => Devices);
                                    break;
                                case "MEASURING_DEVICE_TYPE":
                                    Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                                    RaisePropertyChanged(() => Devices);
                                    break;
                            }
                            dtPrev = row.maxData;   //сохраняем время последнего обработанного изменения БД
                        }
                        refreshAfterReload = false;
                    }
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                { Thread.Sleep(2000); }
            }


        }
        //сохранение в БД текущего состояния выбора для блокировки удалений другими пользователями
        private void dbSaveSelectedParameter()
        {
            var t = methodsEntities.CurrentUserTask.Where(p => p.CUT_USER_NAME == userName).FirstOrDefault();
            if (t != null && t.CUT_ORG_ID == org_id && t.CUT_ANL_ID == anl_id && t.CUT_AT_ID == at_id && t.CUT_ARM_ID == arm_id &&
                             t.CUT_MODE_ID == (selectedMode != null ? selectedMode.MODE_ID : 0))
                return; //ничего не изменилось в выборе пользователя
            if (t != null)
            {
                t.CUT_ORG_ID = org_id;
                t.CUT_ANL_ID = anl_id;
                t.CUT_AT_ID = at_id;
                t.CUT_ARM_ID = arm_id;
                t.CUT_MODE_ID = (selectedMode != null ? selectedMode.MODE_ID : 0);
                if (!methodsEntities.Configuration.AutoDetectChangesEnabled)
                    methodsEntities.Entry(t).State = EntityState.Modified;
            }
            else
            {
                methodsEntities.CurrentUserTask.Add(new CurrentUserTask()
                {
                    CUT_USER_NAME = userName,
                    CUT_ARM_ID = arm_id,
                    CUT_AT_ID = at_id,
                    CUT_ANL_ID = anl_id,
                    CUT_ORG_ID = org_id,
                    CUT_MODE_ID = (selectedMode != null ? selectedMode.MODE_ID : 0)
                });
                if (!methodsEntities.Configuration.AutoDetectChangesEnabled)
                    methodsEntities.Entry(t).State = EntityState.Added;
            }
            SaveData(null);
        }


        //Выборочная(с активной вкладки) очистка данных измерений
        public void ClearMeasuringData(Object o)
        {
            foreach (MEASURING_DATA md in Measurings)
            {
                switch (Tag)
                {
                    case "UF":
                        md.MDA_UFCN_VALUE_IZM = 0;
                        md.MDA_UFN_VALUE_IZM = 0;
                        Calculate_UFcn(md);
                        Calculate_UFn(md);
                        // RefreshUF(md);
                        md.MDA_UF = 0;
                        break;
                    case "U0":
                        md.MDA_U0CN_VALUE_IZM = 0;
                        md.MDA_U0N_VALUE_IZM = 0;
                        Calculate_U0cn(md);
                        Calculate_U0n(md);
                        //RefreshU0(md);
                        md.MDA_U0 = 0;
                        break;
                    case "Saz":
                        md.MDA_ES_VALUE_IZM = 0;
                        Calculate_Es(md);
                        break;
                }
            }
            //удалим результаты предыдущего расчёта в режиме
            ModeResultClear(selectedMode, false);
            //сохранено в пред строке
            // SaveData(null);
            //обновим таблицы и активность кнопки
            RaisePropertyChanged(() => buttonCalculateEnabled);
            switch (Tag)
            {
                case "UF":
                    RefreshGcUF?.Invoke();
                    break;
                case "U0":
                    RefreshGcU0?.Invoke();
                    break;
                case "Saz":
                    RefreshGcSaz?.Invoke();
                    refreshGcEHs?.Invoke();
                    break;
            }
        }



        private bool canPasteFromExcel()
        {
            return (!String.IsNullOrEmpty(System.Windows.Forms.Clipboard.GetText().ToString()));
        }
        public void PasteFromExcel(Object o)
        {
            canAuto = false;
            //работаем только в первоначальном контексте
            if (isSolid)
                methodsEntities.Configuration.AutoDetectChangesEnabled = false;
            //удалим все предыдущие измерения в режиме и результаты рассчёта
            List<MEASURING_DATA> currentMeasurings = Measurings;//измерения вида
            switch (Tag)
            {
                case "E":
                    //currentMeasurings == Measurings, т.е. все строки ПЭМИ
                    currentMeasurings = Measurings.Where(p => p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "UF":
                    currentMeasurings = Measurings.Where(p => p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "U0":
                    currentMeasurings = Measurings.Where(p => p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "Saz":
                    currentMeasurings = Measurings.Where(p => p.MDA_ES_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;

            }
            if (currentMeasurings.Any() && !AppendMode)
            {
                keyInsert = "Excel";
                try
                {
                    GetDataAfterMeasuring_Delete(null); //асинхронное удаление, после окончания вызывается асинхронная вставка
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                    return;
                }
            }
            else
                PasteFromExcel_Insert();

        }

        private void PasteFromExcel_Insert()
        {
            dFromExcel = Clipboard.GetText();
            if (String.IsNullOrEmpty(dFromExcel))
            {
                MessageBox.Show("Буфер копирования пуст.");
                return;
            }
            backgroundWorkerExcel.RunWorkerAsync();
        }


        void MakeEQ()
        {
            eqForReport = new List<EqForReport>();
            int npp0 = 0;
            foreach (EQUIPMENT eq0 in new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_PARENT_ID == null)))
            {
                npp0++;
                eqForReport.Add(new EqForReport()
                {
                    NPP = npp0.ToString(),
                    Name = eq0.EQUIPMENT_TYPE.EQT_NAME,
                    Model = eq0.EQ_MODEL,
                    Serial = eq0.EQ_SERIAL,
                    Note = eq0.EQ_NOTE
                });
                if (methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_PARENT_ID == eq0.EQ_ID).Count() != 0)
                    AddReqursive(eq0, npp0.ToString());
            }
        }

        void AddReqursive(EQUIPMENT eq0, string prevNpp)
        {
            int npp = 0;
            foreach (EQUIPMENT eq in new ObservableCollection<EQUIPMENT>(
                        methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_PARENT_ID == eq0.EQ_ID)))
            {
                npp++;
                eqForReport.Add(new EqForReport()
                {
                    NPP = prevNpp + "." + npp.ToString(),
                    Name = eq.EQUIPMENT_TYPE.EQT_NAME,
                    Model = eq.EQ_MODEL,
                    Serial = eq.EQ_SERIAL,
                    Note = eq.EQ_NOTE
                });
                if (eq.EQ_PARENT_ID != null)
                    AddReqursive(eq, prevNpp + "." + npp.ToString() + ".");
            }

        }



        //Основная измерительная аппаратура АРМа
        private void MakeAllIP()
        {
            AllIP = new ObservableCollection<IP>();
            AllIPHelper = new ObservableCollection<IP>();
            int npp = 0;
            foreach (var mda in methodsEntities.MEASURING_DEVICE_ARM.Where(p => p.MDARM_ARM_ID == arm_one.ARM_ID && p.MEASURING_DEVICE.MD_IS_HELPER == "нет"))
            {
                npp++;
                IP ip = new IP()
                {
                    Type = mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE != null && mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME != null
                                          ? mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME : String.Empty
                };
                AllIP.Add(new IP()
                {
                    NPP = npp.ToString() + ".",
                    Type = mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE != null &&
                            mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME != null
                                          ? mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME
                                          : String.Empty,
                    Model = mda.MEASURING_DEVICE.MD_MODEL ?? String.Empty,
                    DiapasonF = mda.MEASURING_DEVICE.MD_F_INTERVAL ?? String.Empty,
                    WorkNumber = mda.MEASURING_DEVICE.MD_WORKNUMBER ?? String.Empty,
                    Date = mda.MEASURING_DEVICE.MD_DATE != null
                                          ? ((DateTime)mda.MEASURING_DEVICE.MD_DATE).ToShortDateString()
                                          : String.Empty
                });
            }
            //добавим антенны
            foreach (var aa in methodsEntities.ANTENNA_ARM.Where(p => p.ANTARM_ARM_ID == arm_one.ARM_ID))
            {
                if (aa.ANTENNA == null)
                    continue;
                npp++;
                AllIP.Add(new IP()
                {
                    NPP = npp.ToString() + ".",
                    Type =
                        aa.ANTENNA.ANT_TYPE ?? String.Empty,
                    Model =
                        aa.ANTENNA.ANT_MODEL ?? String.Empty,
                    DiapasonF =
                        aa.ANTENNA.ANT_F_INTERVAL ?? String.Empty,
                    WorkNumber =
                        aa.ANTENNA.ANT_WORKNUMBER ?? String.Empty,
                    Date =
                        aa.ANTENNA.ANT_DATE != null
                            ? ((DateTime)aa.ANTENNA.ANT_DATE).ToShortDateString()
                            : String.Empty
                });
            }

            foreach (var mda in methodsEntities.MEASURING_DEVICE_ARM.Where(p => p.MDARM_ARM_ID == arm_one.ARM_ID && p.MEASURING_DEVICE.MD_IS_HELPER == "да"))
            {
                if (mda.MEASURING_DEVICE == null)
                    continue;
                npp++;
                int countAll =
                    methodsEntities.MEASURING_DEVICE_ARM.Where(
                        p => p.MDARM_ARM_ID == arm_one.ARM_ID && p.MEASURING_DEVICE.MD_IS_HELPER == "да").Count();
                AllIPHelper.Add(new IP()
                {
                    NPP = npp.ToString() + ".",
                    Type = mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE != null && mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME != null
                         ? mda.MEASURING_DEVICE.MEASURING_DEVICE_TYPE.MDT_NAME
                         : String.Empty,
                    Model = mda.MEASURING_DEVICE.MD_MODEL ?? String.Empty,
                    DiapasonF = mda.MEASURING_DEVICE.MD_F_INTERVAL ?? String.Empty,
                    WorkNumber = mda.MEASURING_DEVICE.MD_WORKNUMBER ?? String.Empty,
                    Date = mda.MEASURING_DEVICE.MD_DATE != null
                                          ? ((DateTime)mda.MEASURING_DEVICE.MD_DATE).ToShortDateString()
                                          : (countAll == 1 ? "begin-end" : (AllIPHelper.Count == 0 ? "begin" : (AllIPHelper.Count == countAll - 1 && AllIPHelper.Count > 0 ? "end" : "")))
                });
            }
        }
        //инициализация комбобоксов выбора устройств ИТ при смене режима
        private void InitModeUI()
        {
            RefreshParam();
            //измерения выбранного режима 
            if (selectedMode == null)
            {
                Measurings = null;
                esms = null;
                esmsWithSAZ = null;
                RefreshResults();
                return;
            }
            Measurings = selectedMode.MEASURING_DATA.ToList<MEASURING_DATA>();//самый быстрый способ обновления коллекции
            esms = new EntityServerModeSource()
            {
                KeyExpression = "MDA_ID",
                QueryableSource = new MethodsEntities().MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                (p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                 p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                 p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 && p.MDA_ES_VALUE_IZM == 0)))
            };

            esmsWithSAZ = new EntityServerModeSource()
            {
                KeyExpression = "MDA_ID",
                QueryableSource = new MethodsEntities().MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 || p.MDA_ES_VALUE_IZM != 0 ||
                (p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                 p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                 p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 && p.MDA_ES_VALUE_IZM == 0)))
            };
            RaisePropertyChanged(() => AutoRBW);
            RefreshResults();
            if (Measurings != null)
            {
                int gen = GC.GetGeneration(Measurings);
                GC.Collect(gen, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }
        }
        private void MeasuringsRefresh()
        {

            if (selectedMode != null)
            {
                Measurings = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID).ToList<MEASURING_DATA>();
            }
            else
                Measurings = null;
            //пересоздание объектов для обновления их содержимого
            esms = new EntityServerModeSource()
            {
                KeyExpression = "MDA_ID",
                QueryableSource = new MethodsEntities().MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                (p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                 p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                 p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 && p.MDA_ES_VALUE_IZM == 0)))
            };

            esmsWithSAZ = new EntityServerModeSource()
            {
                KeyExpression = "MDA_ID",
                QueryableSource = new MethodsEntities().MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID &&
                (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 || p.MDA_ES_VALUE_IZM != 0 ||
                (p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                 p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFN_VALUE_IZM == 0 &&
                 p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0 && p.MDA_ES_VALUE_IZM == 0)))
            };
            if (Measurings != null)
            {
                int gen = GC.GetGeneration(Measurings);
                GC.Collect(gen, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
            }


        }
        //обновить все свойства, связанные с selectedMode
        private void RefreshParam()
        {
            RaisePropertyChanged(() => paramAntGs);
            RaisePropertyChanged(() => paramFT);
            RaisePropertyChanged(() => paramD);
            RaisePropertyChanged(() => paramR);
            // RaisePropertyChanged(() => paramRMAX);
            RaisePropertyChanged(() => paramL);
            RaisePropertyChanged(() => paramTAU);
            RaisePropertyChanged(() => paramSVT);
        }
        //перерасчёт Е после изменения условий ограничения сигнала
        private void RemakeE()
        {

            try
            {
                if (ContraintE && isSolid || !isSolid)
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID
                                                              && p.MDA_ECN_VALUE_IZM_DB > p.MDA_EN_VALUE_IZM_DB
                                                              && (p.MDA_ECN_VALUE_IZM_DB - p.MDA_EN_VALUE_IZM_DB) <= 3).
                        Update(p => new MEASURING_DATA() { MDA_E = 0 });
                else
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID
                                                          && p.MDA_ECN_VALUE_IZM_DB > p.MDA_EN_VALUE_IZM_DB
                                                          && (p.MDA_ECN_VALUE_IZM_DB - p.MDA_EN_VALUE_IZM_DB) <= 3).
                        Update(p => new MEASURING_DATA()
                        {
                            MDA_E = Math.Round(
                        Math.Pow(
                            Math.Pow((double)p.MDA_ECN_VALUE_IZM_MKV, 2) -
                            Math.Pow((double)p.MDA_EN_VALUE_IZM_MKV, 2),
                            1 / 2f), 3)

                        });
                MeasuringsRefresh();
                ModeResultClear(selectedMode, false);
            }
            catch { }

        }

        //сигнал без шума - E  для одной строки
        private void RefreshE(MEASURING_DATA md)
        {
            if (ContraintE) //для всех режимов действует правило - сигнал > 3дБ
            {
                if ((md.MDA_ECN_VALUE_IZM_DB - md.MDA_EN_VALUE_IZM_DB) <= 3)
                {
                    md.MDA_E = 0;
                    return;
                }
                md.MDA_E = Math.Round(
                   Math.Pow(
                       Math.Pow((double)md.MDA_ECN_VALUE_IZM_MKV, 2) -
                       Math.Pow((double)md.MDA_EN_VALUE_IZM_MKV, 2),
                       1 / 2f), 3);
                return;
            }
            if (!isSolid) //дискрспектр
            {
                if ((md.MDA_ECN_VALUE_IZM_DB - md.MDA_EN_VALUE_IZM_DB) <= 3)
                {
                    md.MDA_E = 0;
                    return;
                }
                else
                    md.MDA_E = Math.Round(
                        Math.Pow(
                            Math.Pow((double)md.MDA_ECN_VALUE_IZM_MKV, 2) -
                            Math.Pow((double)md.MDA_EN_VALUE_IZM_MKV, 2),
                            1 / 2f), 3);
            }
            else  //сплошной спектр
            {
                if (md.MDA_ECN_VALUE_IZM_MKV != 0 && (double)md.MDA_ECN_VALUE_IZM_MKV > (double)md.MDA_EN_VALUE_IZM_MKV)

                    md.MDA_E = Math.Round(
                        Math.Pow(
                            Math.Pow((double)md.MDA_ECN_VALUE_IZM_MKV, 2) -
                            Math.Pow((double)md.MDA_EN_VALUE_IZM_MKV, 2),
                            1 / 2f), 3);
                else
                    md.MDA_E = 0;
            }
        }

        //сигнал наводки без шума, фаза - UF
        private void RefreshUF(MEASURING_DATA md)
        {

            if (md.MDA_UFCN_VALUE_IZM_MKV != 0 && (double)md.MDA_UFCN_VALUE_IZM_MKV / (double)md.MDA_UFN_VALUE_IZM_MKV >= Math.Pow(2, 0.5))

                md.MDA_UF = Math.Round(
                    Math.Pow(
                        Math.Pow(md.MDA_UFCN_VALUE_IZM_MKV, 2) -
                        Math.Pow(md.MDA_UFN_VALUE_IZM_MKV, 2),
                        1 / 2f), 3);
            else
                md.MDA_UF = 0;

        }
        //сигнал наводки без шума, ноль - U0
        private void RefreshU0(MEASURING_DATA md)
        {

            if (md.MDA_U0CN_VALUE_IZM_MKV != 0 && (double)md.MDA_U0CN_VALUE_IZM_MKV / (double)md.MDA_U0N_VALUE_IZM_MKV >= Math.Pow(2, 0.5))

                md.MDA_U0 = Math.Round(
                    Math.Pow(
                        Math.Pow(md.MDA_U0CN_VALUE_IZM_MKV, 2) -
                        Math.Pow(md.MDA_U0N_VALUE_IZM_MKV, 2),
                        1 / 2f), 3);
            else
                md.MDA_U0 = 0;
        }

        //коэффициент затухания САЗ для всех строк измерений
        private void RefreshKps(List<MEASURING_DATA> mdCol)
        {
            //сохраним введённые данные
            SaveData(null);
            foreach (var md in mdCol)
            {
                RefreshKps(md);
            }
        }

        //коэффициент затухания САЗ для заданной частоты
        private void RefreshKps(MEASURING_DATA md)
        {
            if (md.UNIT != null && md.MODE.MODE_D != 0 && md.MODE.MODE_L != 0)
                //если шум измерен на границе КЗ, коэфф=1, иначе, если ГШ расположен вблизи СВТ, то коэфф = коэфф.зат опасного сигнала.
                //Если ГШ отнесён от СВТ и измерен на расстоянии L от КЗ, то раассчитывается по формуле затухания
                md.MDA_KPS = (bool)md.MODE.MODE_ANT_GS
                    ? 1
                    : (md.MODE.MODE_SVT == "ГШ вблизи СВТ"
                        ? md.MDA_KP
                        : Math.Round(
                            Functions.K_zatuchanija(md.MODE.MODE_D, md.MODE.MODE_L,
                                Functions.F_kGc(md.MDA_F, md.UNIT.UNIT_VALUE)), 3));
        }
        public void RefreshIAsync(MODE mode)
        {
            keyCalculate = false;
            backgroundWorkerI.RunWorkerAsync();
        }

        //частотный интервал для строк измерений выбранного режима, исп-ся после изменения тактовой частоты Ft
        //без контроля количества попадания частот в один интервал
        public void RefreshI(MODE mode)
        {
            if (mode == null || !mode.MEASURING_DATA.Any())
                return;
            foreach (var md in mode.MEASURING_DATA)
            {
                md.MDA_I = 0;
            }
            //bool isSolid = mode.MODE_IS_SOLID;
            double mode_paramTAU = Functions.Tau_nsek(mode.MODE_TAU, Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID));
            var rowsE = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID); //для E
            calcI(rowsE, "E", mode_paramTAU);
            SaveData(null);
            //наводки и САЗ не рассматриваем
            //var rowsUF = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_E == 0 && p.MDA_UF != 0); //для UF            
            //calcI(rowsUF, "UF", mode_paramTAU, isSolid);
            //var rowsU0 = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_E == 0 && p.MDA_UF == 0 && p.MDA_U0 != 0); //для U0
            //calcI(rowsU0, "U0", mode_paramTAU, isSolid);
            //var rowsSAZ = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_E == 0 && p.MDA_UF == 0 && p.MDA_U0 == 0 && p.MDA_ES_VALUE_IZM_MKV != 0); //для SAZ
            //calcI(rowsSAZ, "SAZ", mode_paramTAU,isSolid);

        }
        private void calcI(IEnumerable<MEASURING_DATA> rows, string type, double mode_paramTAU)
        {
            foreach (var md in rows.Where(p => p.MDA_I == 0).OrderBy(p => p.MDA_F))
            {
                RefreshI(md, mode_paramTAU);
                RefreshKa(md);
                Calculate_Ecn(md);
                Calculate_En(md);
                RefreshE(md);
            }
        }
        private bool canRefreshIWithContrains()
        {
            return !isSolid;
        }
        //частотный интервал для строк измерений выбранного режима, исп-ся после изменения тактовой частоты Ft
        //c контролем  количества попадания частот в один интервал и модификацией частот
        public void RefreshIWithContrains(Object o)
        {
            MODE mode = selectedMode;
            if (o != null)
                mode = o as MODE;

            if (mode == null || !mode.MEASURING_DATA.Any())
                return;
            foreach (var md in mode.MEASURING_DATA)
            {
                md.MDA_I = 0;
            }

            double mode_paramTAU = Functions.Tau_nsek(mode.MODE_TAU, Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID));
            var rowsE = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_E != 0).ToList<MEASURING_DATA>(); //для E
            CalcIIWithContrains(rowsE, mode_paramTAU);
            SaveData(null); //нужно ли сохранение промежуточных рез-в? проверить...



            //не нужно ничего обновлять, т.к. ф-я выполняется в составе расчёта ОЗ
            //MeasuringsRefresh();
            //RefreshGcE?.Invoke();
            //RefreshGcResults?.Invoke();
            //RefreshGcCollection?.Invoke();
        }
        private void CalcIIWithContrains(List<MEASURING_DATA> rows, double mode_paramTAU)
        {
            int t = 0;
            int currentI = 0;
            double f_law = 0;
            double step = 0;
            foreach (var md in rows.Where(p => p.MDA_I == 0).OrderBy(p => p.MDA_F))
            {
                RefreshI(md, mode_paramTAU);
                //проверим, не получилось ли строк в лепестке больше 2-х, только для дискретного спектра
                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
                if (t > 2)
                {
                    currentI = md.MDA_I;
                    f_law = Math.Round(currentI * 1000 / mode_paramTAU, 3); //мГц нижняя граница след. интервала, начнём изменять частоту  с неё
                    step = (Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1);
                    md.MDA_F = f_law;
                    while (true) //пока модифицированная частота не останется единственной в коллекции E
                    {
                        if (rows.Where(p => p.MDA_F == md.MDA_F).Count() == 1)
                        {
                            RefreshI(md, mode_paramTAU);
                            if (md.MDA_I != currentI)
                            {
                                RefreshKa(md);
                                Calculate_Ecn(md);
                                Calculate_En(md);
                                RefreshE(md);
                                break;
                            }
                        }
                        md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
                    }
                }
            }
        }


        //определение № лепестка для всех видов измерений, пока не используется. Модификация частот, если задана такая опция.
        //private void calcI_ForAll(IEnumerable<MEASURING_DATA> rows, string type, double mode_paramTAU,bool isSolid)
        //{
        //    MEASURING_DATA mdAdd = new MEASURING_DATA();
        //    //foreach (var md in rows)
        //    //{
        //    //    md.MDA_I = 0;
        //    //}
        //    bool cicle = true;
        //    while (cicle)
        //    {
        //        cicle = false;
        //        foreach (var md in rows.Where(p => p.MDA_I == 0).OrderBy(p => p.MDA_F))
        //        {
        //            RefreshI(md, mode_paramTAU);
        //            //проверим, не получилось ли строк в лепестке больше 2-х, только для дискретного спектра
        //            if (isSolid || !IRemake)
        //                continue;
        //            int t = 0;
        //            switch (type)
        //            {
        //                case "E":
        //                    t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                    break;
        //                case "UF":
        //                    t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == md.MDA_MODE_ID && p.MDA_UF != 0 && p.MDA_I == md.MDA_I).Count();
        //                    break;
        //                case "U0":
        //                    t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == md.MDA_MODE_ID && p.MDA_U0 != 0 && p.MDA_I == md.MDA_I).Count();
        //                    break;
        //                case "SAZ":
        //                    t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == md.MDA_MODE_ID && p.MDA_EGS != 0 && p.MDA_I == md.MDA_I).Count();
        //                    break;
        //            }

        //            if (t > 2)
        //            {
        //                int currentI = md.MDA_I;
        //                double step = (Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1);
        //                switch (type)
        //                {
        //                    case "E":
        //                        if (md.MDA_UFCN_VALUE_IZM != 0 || md.MDA_UFN_VALUE_IZM != 0 || md.MDA_U0CN_VALUE_IZM != 0 || md.MDA_U0N_VALUE_IZM != 0 || md.MDA_ES_VALUE_IZM_MKV != 0)//есть другие измерения на этой частоте
        //                        {  //создаём новую строку только с Е-измерением на новой частоте
        //                            mdAdd = new MEASURING_DATA();
        //                            mdAdd = md.DeepCopy();
        //                            //в копируемой строке обнулим все Е, они перенесутся в новую
        //                            md.MDA_ECN_VALUE_IZM = 0;
        //                            md.MDA_EN_VALUE_IZM = 0;
        //                            md.MDA_ECN_VALUE_IZM_DB = 0;
        //                            md.MDA_ECN_VALUE_IZM_MKV = 0;
        //                            md.MDA_E = 0;
        //                            //в новой строке оставим только Е
        //                            mdAdd.MDA_UFCN_VALUE_IZM = 0;
        //                            mdAdd.MDA_UFN_VALUE_IZM = 0;
        //                            mdAdd.MDA_UFCN_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_UFCN_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_UF = 0;
        //                            mdAdd.MDA_U0CN_VALUE_IZM = 0;
        //                            mdAdd.MDA_U0N_VALUE_IZM = 0;
        //                            mdAdd.MDA_U0CN_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_U0CN_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_U0 = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_EGS_DB = 0;
        //                            mdAdd.MDA_EGS_MKV = 0;
        //                            mdAdd.MDA_KPS = 0;
        //                            mdAdd.MDA_I = 0;
        //                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                            while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                            methodsEntities.MEASURING_DATA.Add(mdAdd);

        //                            while (true)
        //                            {
        //                                RefreshI(mdAdd, mode_paramTAU);
        //                                if (mdAdd.MDA_I == currentI)
        //                                {
        //                                    mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                    while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                                }
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = mdAdd.MDA_I;
        //                                mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                    mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                                while (true)
        //                                {
        //                                    RefreshI(mdAdd, mode_paramTAU);
        //                                    if (mdAdd.MDA_I == currentI)
        //                                    {
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                        while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                                    }
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            }
        //                            //methodsEntities.MEASURING_DATA.Add(mdAdd);
        //                        }
        //                        else  //нет измерений другого вида на этой частоте
        //                        {
        //                            while (true) //пока модифицированная частота не останется единственной в коллекции E
        //                            {
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                                if (rows.Where(p => p.MDA_F == md.MDA_F && (md.MDA_ECN_VALUE_IZM != 0 || md.MDA_EN_VALUE_IZM != 0)).Count() == 1)
        //                                    break;
        //                            }
        //                            while (true)
        //                            {
        //                                RefreshI(md, mode_paramTAU);
        //                                if (md.MDA_I == currentI)
        //                                    md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = md.MDA_I;
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                while (true)
        //                                {
        //                                    //изменяем частоту на 100кГц}
        //                                    RefreshI(md, mode_paramTAU);
        //                                    if (md.MDA_I == currentI)
        //                                        md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            }
        //                        }
        //                        break;
        //                    case "UF":
        //                        if (md.MDA_U0 != 0 || md.MDA_ES_VALUE_IZM_MKV != 0) //есть другие измерения на этой частоте
        //                        {  //создаём новую строку только с UF-измерением на новой частоте
        //                            mdAdd = new MEASURING_DATA();
        //                            mdAdd = md.DeepCopy();
        //                            //в копируемой строке обнулим все UF,они перенесутся в новую
        //                            md.MDA_UFCN_VALUE_IZM = 0;
        //                            md.MDA_UFN_VALUE_IZM = 0;
        //                            md.MDA_UFCN_VALUE_IZM_DB = 0;
        //                            md.MDA_UFCN_VALUE_IZM_MKV = 0;
        //                            md.MDA_UF = 0;
        //                            //в новой строке оставим только UF. E=0 по условиям отбора, U0 и САЗ обнулим
        //                            mdAdd.MDA_U0CN_VALUE_IZM = 0;
        //                            mdAdd.MDA_U0N_VALUE_IZM = 0;
        //                            mdAdd.MDA_U0CN_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_U0CN_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_U0 = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_EGS_DB = 0;
        //                            mdAdd.MDA_EGS_MKV = 0;
        //                            mdAdd.MDA_KPS = 0;
        //                            mdAdd.MDA_I = 0;
        //                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                            while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                            methodsEntities.MEASURING_DATA.Add(mdAdd);
        //                            while (true)
        //                            {
        //                                RefreshI(mdAdd, mode_paramTAU);
        //                                if (mdAdd.MDA_I == currentI)
        //                                {
        //                                    mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                    //изменяем частоту на 100кГц до тех пор, пока она не станет единственной
        //                                    while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3);
        //                                }
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_UF != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = mdAdd.MDA_I;
        //                                while (true) //пока модифицированная частота не останется единственной в коллекции
        //                                {
        //                                    mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                                    if (rows.Where(p => p.MDA_F == mdAdd.MDA_F).Count() == 1)
        //                                        break;
        //                                }
        //                                //mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                while (true)
        //                                {
        //                                    RefreshI(mdAdd, mode_paramTAU);
        //                                    if (mdAdd.MDA_I == currentI)
        //                                    {
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                        while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                                    }
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_UF != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            }
        //                        }
        //                        else  //нет измерений других видов вида на этой частоте
        //                        {
        //                            while (true) //пока модифицированная частота не останется единственной в коллекции
        //                            {
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                                if (rows.Where(p => p.MDA_F == md.MDA_F).Count() == 1)
        //                                    break;
        //                            }
        //                            //md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                            while (true)
        //                            {
        //                                RefreshI(md, mode_paramTAU);
        //                                if (md.MDA_I == currentI)
        //                                {
        //                                    while (true) //пока модифицированная частота не останется единственной в коллекции
        //                                    {
        //                                        md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                                        if (rows.Where(p => p.MDA_F == md.MDA_F).Count() == 1)
        //                                            break;
        //                                    }
        //                                    // md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                }
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = md.MDA_I;
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                while (true)
        //                                {
        //                                    //изменяем частоту на 100кГц}
        //                                    RefreshI(md, mode_paramTAU);
        //                                    if (md.MDA_I == currentI)
        //                                    {
        //                                        while (true) //пока модифицированная частота не останется единственной в коллекции
        //                                        {
        //                                            md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                                            if (rows.Where(p => p.MDA_F == md.MDA_F).Count() == 1)
        //                                                break;
        //                                        }
        //                                        //md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                    }
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            }
        //                        }

        //                        break;
        //                    case "U0":
        //                        if (md.MDA_ES_VALUE_IZM_MKV != 0) //есть другие измерения на этой частоте
        //                        {  //создаём новую строку только с U0-измерением на новой частоте
        //                            mdAdd = new MEASURING_DATA();
        //                            mdAdd = md.DeepCopy();
        //                            //в копируемой строке обнулим все U0, они перенесутся в новую. В новой строке САЗ обнулим
        //                            md.MDA_U0CN_VALUE_IZM = 0;
        //                            md.MDA_U0N_VALUE_IZM = 0;
        //                            md.MDA_U0CN_VALUE_IZM_DB = 0;
        //                            md.MDA_U0CN_VALUE_IZM_MKV = 0;
        //                            md.MDA_U0 = 0;
        //                            //в новой строке оставим только САЗ

        //                            mdAdd.MDA_ES_VALUE_IZM = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_DB = 0;
        //                            mdAdd.MDA_ES_VALUE_IZM_MKV = 0;
        //                            mdAdd.MDA_EGS_DB = 0;
        //                            mdAdd.MDA_EGS_MKV = 0;
        //                            mdAdd.MDA_KPS = 0;
        //                            mdAdd.MDA_I = 0;
        //                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                            while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                            methodsEntities.MEASURING_DATA.Add(mdAdd);
        //                            while (true)
        //                            {
        //                                RefreshI(mdAdd, mode_paramTAU);
        //                                if (mdAdd.MDA_I == currentI)
        //                                {
        //                                    mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                    //изменяем частоту на 100кГц до тех пор, пока она не станет единственной
        //                                    while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3);
        //                                }
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_U0 != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = mdAdd.MDA_I;
        //                                mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                while (true)
        //                                {
        //                                    RefreshI(mdAdd, mode_paramTAU);
        //                                    if (mdAdd.MDA_I == currentI)
        //                                    {
        //                                        mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                        while (methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_F == mdAdd.MDA_F).FirstOrDefault() != null)
        //                                            mdAdd.MDA_F = Math.Round(mdAdd.MDA_F + (Functions.GetUnitValue(Units, mdAdd.MDA_F_UNIT_ID) == "кГц" ? 100 : 0.1), 3); //изменяем частоту на 100кГц
        //                                    }
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mdAdd.MDA_MODE_ID && p.MDA_U0 != 0 && p.MDA_I == mdAdd.MDA_I).Count();
        //                            }
        //                        }
        //                        else  //нет измерений других видов вида на этой частоте
        //                        {
        //                            md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                            while (true)
        //                            {
        //                                RefreshI(md, mode_paramTAU);
        //                                if (md.MDA_I == currentI)
        //                                    md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = md.MDA_I;
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                while (true)
        //                                {
        //                                    //изменяем частоту на 100кГц}
        //                                    RefreshI(md, mode_paramTAU);
        //                                    if (md.MDA_I == currentI)
        //                                        md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            }
        //                        }
        //                        break;
        //                    case "SAZ":
        //                        //нет измерений других видов вида на этой частоте
        //                        {
        //                            md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц
        //                            while (true)
        //                            {
        //                                RefreshI(md, mode_paramTAU);
        //                                if (md.MDA_I == currentI)
        //                                    md.MDA_F = Math.Round(md.MDA_F + step, 3); //изменяем частоту на 100кГц}
        //                                else
        //                                {
        //                                    break; //присвоили номер след. лепестка                                           
        //                                }
        //                            }
        //                            //проверим, сколько попало частот в новый лепесток. Если > 2, то продолжим добавление частот до попадания в след интервал
        //                            t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            while (t > 2)
        //                            {
        //                                currentI = md.MDA_I;
        //                                md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                while (true)
        //                                {
        //                                    //изменяем частоту на 100кГц}
        //                                    RefreshI(md, mode_paramTAU);
        //                                    if (md.MDA_I == currentI)
        //                                        md.MDA_F = Math.Round(md.MDA_F + step, 3);
        //                                    else
        //                                    {
        //                                        break; //присвоили номер след. лепестка                                           
        //                                    }
        //                                }
        //                                t = rows.Where(p => p.MDA_I != 0 && p.MDA_I == md.MDA_I).Count();
        //                            }
        //                        }
        //                        break;
        //                }
        //                //подкорректируем частоту и пересчитаем
        //                cicle = true;
        //                break;
        //            }
        //        }
        //    }

        //}
        //частотный интервал для одной строки измерений
        private void RefreshI(MEASURING_DATA md, double mode_paramTAU)
        {

            double f_kGc = Functions.F_kGc(md.MDA_F,
                            Functions.GetUnitValue(Units, (int)md.MDA_F_UNIT_ID));
            // RaisePropertyChanged(() => paramTAU);
            md.MDA_I = (int)Math.Truncate(1 +
                                           mode_paramTAU * f_kGc / 1000000);
        }
        //без перерасчёта Ka, KF, K0
        private void MakeSimpleData(out double value_dB, out double value_mkV, double K, double value)
        {
            value_dB = 0;
            value_mkV = 0;
            //значение в ДБ, с учётом поправочного коэффициента
            value_dB = value + K;
            value_mkV = Math.Round(Functions.DbmkV(value_dB), 3);

        }

        private void RefreshResults()
        {
            RaisePropertyChanged(() => Results);
            RefreshGcResults?.Invoke();
        }

        //перерасчёт всех значений измерений после изменения выбора антенны
        private void ITUpdate(MODE mode, string mdaType)
        {
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (!keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничего делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            if (AntennaE == null && AntennaH == null || Measurings == null || !Measurings.Any())
                //  if (selectedRow == null || Antenna == null || Measurings == null || !Measurings.Any())
                return;
            var temp = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_TYPE == mdaType);
            if (temp == null || !temp.Any())
            {
                if (!keySuspendParent) //поток не был остановлен до вызова ф-ии, поэтому его восстанавливаем
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    keySuspend = false;
                }

            }

            foreach (MEASURING_DATA md in temp)
            {
                RefreshKa(md);
                Calculate_All(md);
                //изменение непосредственно в БД
                //methodsEntities.MEASURING_DATA.Where(p => p.MDA_ID == md.MDA_ID).Update
                //    (u => new MEASURING_DATA
                //    {
                //        MDA_TYPE = mdaType,
                //        MDA_KA = md.MDA_KA,
                //        MDA_ECN_VALUE_IZM_MKV = md.MDA_ECN_VALUE_IZM_MKV,
                //        MDA_EN_VALUE_IZM_MKV = md.MDA_EN_VALUE_IZM_MKV,
                //        MDA_ECN_VALUE_IZM_DB = md.MDA_ECN_VALUE_IZM_DB,
                //        MDA_EN_VALUE_IZM_DB = md.MDA_EN_VALUE_IZM_DB,
                //        MDA_E = md.MDA_E
                //    });
                // methodsEntities.MEASURING_DATA.Where(p => p.MDA_ID == md.MDA_ID).Update
                //   (u => md);
            }
            SaveData(null);
            MeasuringsRefresh();
            ModeResultClear(mode, false);

            RaisePropertyChanged(() => Measurings);
            RaisePropertyChanged(() => selectedRow);
            RaisePropertyChanged(() => buttonCalculateEnabled);
            RefreshGcE?.Invoke();
            RefreshGcCollection?.Invoke();
            if (!keySuspendParent) //поток не был остановлен до вызова ф-ии, поэтому его восстанавливаем
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                keySuspend = false;
            }
        }
        //добавление антенны в новую строку измерений
        //private void IT(MEASURING_DATA md)
        //{
        //    //if (ITUnified && Measurings!= null && Measurings.Any() && Measurings.Count > 1 && Measurings[0].MDA_ANT_ID != null)   //Измерительный тракт, как в первой строке
        //    if (ITUnified && Measurings != null && Measurings.Any() && Measurings[0].MDA_ANT_ID != null)   //Измерительный тракт, как в первой строке           
        //        {
        //        md.MDA_ANT_ID = Measurings[0].MDA_ANT_ID;
        //        md.ANTENNA = Antennas.Where(p => p.ANT_ID == Measurings[0].MDA_ANT_ID).FirstOrDefault();
        //        //Antenna;

        //    }
        //    else //первая строка в режиме или измерительный тракт строк независимый
        //    {
        //        if (Antennas.Any())
        //        {
        //            var ant = Antennas.Where(p => p.ANT_DEFAULT == "да").FirstOrDefault();
        //            if (ant == null)
        //                ant = Antennas.FirstOrDefault();
        //            md.MDA_ANT_ID = ant.ANT_ID;
        //            md.ANTENNA = ant;

        //        }
        //    }
        //    RefreshKa(md);            
        //}

        //значения в строке, зависящие от частоты
        private void After_F(MEASURING_DATA md)
        {
            //частотный интервал
            RefreshI(md, paramTAU);
            //перерасчёт коэффициентов и связанных с ними значений
            RefreshKa(md);
            if (md.MDA_UFCN_VALUE_IZM != 0 || md.MDA_UFN_VALUE_IZM != 0)
                RefreshKF(md);
            if (md.MDA_U0CN_VALUE_IZM != 0 || md.MDA_U0N_VALUE_IZM != 0)
                RefreshK0(md);

            Calculate_All(md);
            RefreshGcE?.Invoke(); //для обновление коэффициентов в gcE и gcCollection           
        }
        //вычисляемые значения для Ecn - ручной ввод
        private void Calculate_Ecn(MEASURING_DATA md, object value)
        {

            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_ECN_VALUE_IZM = Convert.ToDouble(value);
            Calculate_Ecn(md);
        }
        private void Calculate_Ecn(MEASURING_DATA md)
        {
            if (md.MDA_ECN_VALUE_IZM == 0) //нет измерения
                return;
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_KA, md.MDA_ECN_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_ECN_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0)  //широкополосный сигнал                
            {
                md.MDA_ECN_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            }
            else
            {
                if (!isSolid)
                    md.MDA_ECN_VALUE_IZM_MKV = value_mkV;
                else
                {
                    //if (md.MDA_RBW == 0)
                    //    MakeRBW(md);
                    md.MDA_ECN_VALUE_IZM_MKV = Math.Round(value_mkV / Math.Pow(md.MDA_RBW, 1 / 2f), 3);
                }
            }
        }

        //вычисляемые значения для En
        private void Calculate_En(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_EN_VALUE_IZM = Convert.ToDouble(value);
            Calculate_En(md);
        }
        private void Calculate_En(MEASURING_DATA md)
        {
            if (md.MDA_EN_VALUE_IZM == 0) //нет измерения
                return;
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_KA, md.MDA_EN_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_EN_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0) //широкополосный сигнал
                md.MDA_EN_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            else
            {
                if (!isSolid)
                    md.MDA_EN_VALUE_IZM_MKV = value_mkV;
                else
                    md.MDA_EN_VALUE_IZM_MKV = Math.Round(value_mkV / Math.Pow(md.MDA_RBW, 1 / 2f), 3);
            }
        }
        private void Calculate_U0cn(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_U0CN_VALUE_IZM = Convert.ToDouble(value);
            Calculate_U0cn(md);
        }
        private void Calculate_U0cn(MEASURING_DATA md)
        {
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_K0, md.MDA_U0CN_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_U0CN_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0) //широкополосный сигнал
                md.MDA_U0CN_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            else
                md.MDA_U0CN_VALUE_IZM_MKV = value_mkV;
        }
        private void Calculate_U0n(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_U0N_VALUE_IZM = Convert.ToDouble(value);
            Calculate_U0n(md);
        }
        private void Calculate_U0n(MEASURING_DATA md)
        {
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_K0, md.MDA_U0N_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_U0N_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0) //широкополосный сигнал
                md.MDA_U0N_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            else
                md.MDA_U0N_VALUE_IZM_MKV = value_mkV;
        }
        //вычисляемые значения для UFcn
        private void Calculate_UFcn(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_UFCN_VALUE_IZM = Convert.ToDouble(value);

            Calculate_UFcn(md);
        }
        private void Calculate_UFcn(MEASURING_DATA md)
        {
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_KF, md.MDA_UFCN_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_UFCN_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0) //широкополосный сигнал
                md.MDA_UFCN_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            else
                md.MDA_UFCN_VALUE_IZM_MKV = value_mkV;
        }
        private void Calculate_UFn(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_UFN_VALUE_IZM = Convert.ToDouble(value);
            Calculate_UFn(md);
        }
        private void Calculate_UFn(MEASURING_DATA md)
        {
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_K0, md.MDA_UFN_VALUE_IZM);
            if (value_mkV == -1)
            {
                MessageBox.Show("Строка не может быть обработана.");
                value_mkV = 0;
            }
            md.MDA_UFN_VALUE_IZM_DB = value_dB;
            if (md.MDA_RBW == 0)
                MakeRBW(md);
            if (md.MDA_F_BEGIN != 0 && md.MDA_F_END != 0) //широкополосный сигнал
                md.MDA_UFN_VALUE_IZM_MKV = Math.Round(value_mkV * Math.Pow((md.MDA_F_END - md.MDA_F_BEGIN) * 1000 / md.MDA_RBW, 1 / 2f), 3);
            else
                md.MDA_UFN_VALUE_IZM_MKV = value_mkV;
        }
        private void Calculate_Es(MEASURING_DATA md, object value)
        {
            if (md.MDA_F == 0)
            {
                MessageBox.Show("Сначала заполните значение частоты.");
                return;
            }
            md.MDA_ES_VALUE_IZM = Convert.ToDouble(value);
            Calculate_Es(md);
        }
        private void Calculate_Es(MEASURING_DATA md)
        {
            MakeSimpleData(out double value_dB, out double value_mkV, md.MDA_KA, md.MDA_ES_VALUE_IZM);
            md.MDA_ES_VALUE_IZM_DB = value_dB;
            md.MDA_ES_VALUE_IZM_MKV = value_mkV;
            //приведём значение измерений к ширине пропускания 1кГ
            if (selectedMode.MODE_RBW != 0)
            //приведённые значения сейчас нигде не используются, рудимент
            {
                md.MDA_EGS_MKV = Math.Round(value_mkV / Math.Pow(selectedMode.MODE_RBW, 1 / 2f), 3);
                md.MDA_EGS_DB = Math.Round(20 * Math.Log10(md.MDA_EGS_MKV), 3);
            }
            else
            {
                md.MDA_EGS_DB = value_dB;
                md.MDA_EGS_MKV = value_mkV;
            }
            RefreshKps(md);
            //значение от ГШ с учётом погрешности измерительного тракта на ширине 1кГц
            // md.MDA_EGS = md.MDA_EGS_MKV;
        }

        //все вычисляемые значения строки измерений
        private void Calculate_All(MEASURING_DATA md)
        {
            if (md.MDA_ECN_VALUE_IZM != 0)
                Calculate_Ecn(md);
            if (md.MDA_EN_VALUE_IZM != 0)
                Calculate_En(md);
            RefreshE(md);
            if (md.MDA_UFCN_VALUE_IZM != 0)
            {
                RefreshKF(md);
                Calculate_UFcn(md);
            }
            if (md.MDA_UFN_VALUE_IZM != 0)
            {
                RefreshKF(md);
                Calculate_UFn(md);
            }
            RefreshUF(md);
            if (md.MDA_U0CN_VALUE_IZM != 0)
            {
                RefreshK0(md);
                Calculate_U0cn(md);
            }
            if (md.MDA_U0N_VALUE_IZM != 0)
            {
                RefreshK0(md);
                Calculate_U0n(md);
            }
            RefreshU0(md);


            if (md.MDA_ES_VALUE_IZM != 0)
                Calculate_Es(md);
        }

        //обновление параметров выбранного режима и активности кнопки "Расчёт"
        public void CellModesUpdated(object value, string FieldName, MODE row)
        {

            if (arm_id == 0)
            {
                MessageBox.Show("Сначала заполните тип АРМа и АРМ.");
                Modes.Remove(row);
                return;
            }
            MODE mode = row;


            if (mode.MODE_ID == 0) //новая строка
            {
                //Антенна по умолчанию
                int antId = Antennas.Where(p => p.ANT_DEFAULT != null && p.ANT_DEFAULT.Contains("да") && p.ANT_TYPE.Contains("Эл")).FirstOrDefault().ANT_ID;
                mode.MODE_ANT_E_ID = antId;
                mode.MODE_ARM_ID = arm_id;
                //Значения по умолчанию
                mode.MODE_D = 1;
                mode.MODE_L = 0;
                mode.MODE_R = 1;
                mode.MODE_RMAX = 1;//alpha
                mode.MODE_KS = 0.8;
                mode.MODE_FT_UNIT_ID = Functions.GetUnitID(Units, "мГц");
                mode.MODE_RBW_UNIT_ID = Functions.GetUnitID(Units, "кГц");
                mode.MODE_AUTORBW = true;
                mode.MODE_TAU_UNIT_ID = Functions.GetUnitID(Units, "нсек");
                mode.MODE_SVT = "ГШ вблизи СВТ";
                mode.MODE_ANT_GS = false;
                if (FieldName == "MODE_MT_ID")
                {
                    ModeMtUpdated(row);
                }
                methodsEntities.MODE.Add(mode);
                methodsEntities.Entry(mode).State = EntityState.Added;
            }
            else
            {

                var t = methodsEntities.Entry(mode).State;
                if (FieldName == "MODE_IS_SOLID")
                {
                    mode.MODE_RMAX = mode.MODE_IS_SOLID ? Math.Round(Math.Pow(2, 0.5), 4) : 1;
                }

                if (FieldName == "MODE_MT_ID")
                {
                    ModeMtUpdated(mode);
                }
                if (FieldName == "MODE_FT") //поменяли частоту, пересчитаем длит.импульса
                {
                    double ft = Functions.F_kGc(row.MODE_FT,
                        Functions.GetUnitValue(Units, (int)row.MODE_FT_UNIT_ID));
                    if (ft != 0)
                    {
                        string tauUnit = Functions.GetUnitValue(Units, (int)row.MODE_TAU_UNIT_ID);
                        if (tauUnit.Contains("сек"))
                        {
                            double k = tauUnit == "нсек" ? 1000000 :
                                      (tauUnit == "мксек" ? 1000 :
                                      (tauUnit == "мсек" ? 1 : 0.0001));
                            mode.MODE_TAU = Math.Round(k / ft / 2, 6); //в заданной единице
                        }
                    }
                    else
                        mode.MODE_TAU = 0;
                    RaisePropertyChanged(() => gcMeasuringEnabled);
                    RaisePropertyChanged(() => gcMeasuringSAZEnabled);
                }
                if (FieldName == "MODE_TAU") //поменяли длит.импульсв, пересчитаем частоту
                {
                    double tau = Functions.Tau_nsek(row.MODE_TAU,
                    Functions.GetUnitValue(Units, (int)row.MODE_TAU_UNIT_ID));
                    if (tau != 0)
                    {
                        string FtUnit = Functions.GetUnitValue(Units, (int)row.MODE_FT_UNIT_ID);
                        if (FtUnit == "кГц" || FtUnit == "мГц" || FtUnit == "Гц")
                        {
                            double k = FtUnit == "кГц" ? 1000000 :
                                      (FtUnit == "мГц" ? 1000 : 1);
                            mode.MODE_FT = Math.Round(k / tau / 2, 3);
                        }
                    }
                    else
                        mode.MODE_FT = 0;
                }
            }
            var state = methodsEntities.Entry(mode).State;
            //methodsEntities.SaveChanges();
            SaveData(null);      //зависает      
            if (selectedMode == null)//добавлена новая строка            
                RefreshModes(mode.MODE_ID);
            RaisePropertyChanged(() => paramFT);
            RaisePropertyChanged(() => paramTAU);
            //поменялся признак ограничения сигнала, надо сигналы пересчитать
            if (FieldName == "MODE_CONTR_E" || FieldName == "MODE_IS_SOLID")
            {
                //приостановим фон.поток на время перерасчёта сигнала
                keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
                if (!keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничего делать не надо
                {
#pragma warning disable CS0618 // Тип или член устарел
                    dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                    keySuspend = true;
                }

                backgroundWorkerE.RunWorkerAsync();
                return;
            }
            else //синхронное выполнение
            {
                if ((FieldName == "MODE_FT" || FieldName == "MODE_TAU" || FieldName == "MODE_MT_ID") && mode.MEASURING_DATA != null && mode.MEASURING_DATA.Any())
                {
                    RefreshIAsync(mode);
                    return;
                }
                //поменяли выбор антенны, пересчитаем поправочные коэффициенты для строк с соответствующим типом поля
                if (FieldName == "MODE_ANT_E_ID")
                {
                    AntennaE = Antennas.Where(p => p.ANT_ID == (int)value).FirstOrDefault();
                    ITUpdate(mode, "E");
                    return;
                }
                if (FieldName == "MODE_ANT_H_ID")
                {
                    AntennaH = Antennas.Where(p => p.ANT_ID == (int)value).FirstOrDefault();
                    ITUpdate(mode, "H");
                    return;
                }

                if (FieldName == "MODE_D" || FieldName == "MODE_R" || FieldName == "MODE_RBW" || FieldName == "MODE_RBW_UNIT_ID")
                {
                    ModeResultClear(row, false);
                    RefreshGcResults?.Invoke();
                    if (FieldName == "MODE_RBW")
                        RaisePropertyChanged(() => gcMeasuringSAZEnabled);
                }
                if (FieldName == "MODE_ANT_GS" && (bool)!mode.MODE_ANT_GS && mode.MODE_L == 0)
                {
                    MessageBox.Show("Для расчёта затухания САЗ заполните параметр L.");
                    return;
                }

                if (FieldName == "MODE_L" || FieldName == "MODE_SVT" || FieldName == "MODE_ANT_GS") //параметры, влияющие на затухание шума от ГШ
                    RefreshKps(Measurings);

                RaisePropertyChanged(() => buttonCalculateEnabled);
                RaisePropertyChanged(() => gcMeasuringEnabled);
                RaisePropertyChanged(() => gcMeasuringSAZEnabled);
            }
        }

        //перерасчёт введёных строк
        public void CellUpdated(object value, string FieldName, MEASURING_DATA row)
        {
            if (isSP)
            {
                MessageBox.Show("Вероятно Вы забыли отменить признак ШП сигнала. Галочка будет снята. Для ШП сигналов запрещен ручной ввод и редактирование.");
                isSP = false;
                RaisePropertyChanged(() => isSP);
            }

            MEASURING_DATA md = row;
            if (md.MDA_MODE_ID == 0) //новая строка
            {
                // IRemake = false; //для новой строки просто вычисляем I
                if (md.MDA_F == 0)
                {
                    MessageBox.Show("Новая строка с не заполненной частотой будет удалена.");
                    Measurings.Remove(md);
                    return;
                }
                md.MDA_MODE_ID = selectedMode.MODE_ID;
                md.MDA_F_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                    ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID
                    : 0;
                md.MDA_RBW_UNIT_ID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                    ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID
                    : 0;
                //измерительный тракт
                //IT(md);//добавление антенны в новую строку(по умолчанию или, как в первой строке)
                //для обновление UI на лету, до окончания редактирования                    
                if (md.MDA_ECN_BEGIN != 0 && md.MDA_F_END != 0)
                // ШП сигнал,нельзя вводить руками, он вычисляется
                {
                    md.MDA_F = Math.Round((md.MDA_F_END + md.MDA_F_BEGIN) / 2, 3);
                    return;
                }
                //принадлежность частоты к интервалу                                               
                if (selectedMode.MODE_AUTORBW == true)
                //автозаполнение ширины пропускания ИП в зависимости от частоты
                {
                    string val = Functions.GetUnitValue(Units, (int)md.MDA_F_UNIT_ID);
                    double f = (val == "кГц" ? md.MDA_F : (val == "Гц" ? md.MDA_F / 1000 : md.MDA_F * 1000));

                    md.MDA_RBW = MakeRBW(f, selectedMode.MODE_IS_SOLID);
                    md.MDA_RBW_UNIT_ID = Functions.GetUnitID(Units, "кГц");
                }
                //частотный интервал
                RefreshI(md, paramTAU);
                //калибровочный коэффициент антенны
                RefreshKa(md);
                //сохраним новую строку и продолжим обработку                
                methodsEntities.MEASURING_DATA.Add(md);
                SaveData(null);
                selectedRow = md;
                FocusedRow = md;
                RefreshGcCollection?.Invoke();
                Measurings.Add(md);

                return;
            }

            //при изменении входных данных удаляем результаты расчёта  выбранного режима
            if (Results.Where(p => p.RES_MODE_ID == selectedMode.MODE_ID).Any())
                ModeResultClear(selectedMode, false);
            try
            {
                switch (FieldName)
                {
                    //case "MDA_ANT_ID":
                    //    RefreshKa(md);
                    //    ITUpdate(); //перерасчёт измерений в выбраной строке и в остальных для единогоИТ
                    //    break;
                    case "MDA_F":
                        //для обновление UI на лету, до окончания редактирования                    
                        if (md.MDA_ECN_BEGIN != 0 && md.MDA_F_END != 0)
                        // ШП сигнал,нельзя вводить руками, он вычисляется
                        {
                            md.MDA_F = Math.Round((md.MDA_F_END + md.MDA_F_BEGIN) / 2, 3);
                            return;
                        }
                        //принадлежность частоты к интервалу                                               
                        if (selectedMode.MODE_AUTORBW == true)
                        //автозаполнение ширины пропускания ИП в зависимости от частоты
                        {
                            string val = Functions.GetUnitValue(Units, (int)md.MDA_F_UNIT_ID);
                            double f = (val == "кГц" ? md.MDA_F : (val == "Гц" ? md.MDA_F / 1000 : md.MDA_F * 1000));

                            md.MDA_RBW = MakeRBW(f, selectedMode.MODE_IS_SOLID);
                            md.MDA_RBW_UNIT_ID = Functions.GetUnitID(Units, "кГц");
                        }
                        //частотный интервал
                        //if (IRemake) //расчёт I с перераспределением частот по интервалам(если их в лепестке > 2)
                        //    RefreshI(selectedMode);
                        //else
                        RefreshI(md, paramTAU);
                        //перерасчёт значений элементов строки, связанных с частотой
                        After_F(md);
                        break;

                    case "MDA_F_UNIT_ID":
                        //для текущей строки значение частоты уже обновлено. 
                        ((MEASURING_DATA)row).MDA_F_UNIT_ID = Convert.ToInt32(value);
                        if (selectedMode.MODE_AUTORBW == true)
                        //автозаполнение ширины пропускания ИП в зависимости от частоты
                        {
                            string val = Functions.GetUnitValue(Units, (int)md.MDA_F_UNIT_ID);
                            double f = (val == "кГц" ? md.MDA_F : (val == "Гц" ? md.MDA_F / 1000 : md.MDA_F * 1000));

                            md.MDA_RBW = MakeRBW(f, selectedMode.MODE_IS_SOLID);
                            md.MDA_RBW_UNIT_ID = Functions.GetUnitID(Units, "кГц");
                        }
                        break;
                    case "MDA_ECN_VALUE_IZM":
                        Calculate_Ecn(md, value);
                        RefreshE(md);
                        break;
                    case "MDA_EN_VALUE_IZM":
                        Calculate_En(md, value);
                        RefreshE(md);
                        break;
                    case "MDA_UFCN_VALUE_IZM":
                        Calculate_UFcn(md, value);
                        RefreshUF(md);
                        break;
                    case "MDA_UFN_VALUE_IZM":
                        Calculate_UFn(md, value);
                        RefreshUF(md);
                        break;
                    case "MDA_U0CN_VALUE_IZM":
                        Calculate_U0cn(md, value);
                        RefreshU0(md);
                        break;
                    case "MDA_U0N_VALUE_IZM":
                        Calculate_U0n(md, value);
                        RefreshU0(md);
                        break;
                    case "MDA_ES_VALUE_IZM":
                        Calculate_Es(md, value);
                        RefreshKps(md);
                        break;
                }
                //при изменении значений измерений проверим значение интервала
                if (md.MDA_E == 0 && md.MDA_UF == 0 && md.MDA_U0 == 0 && md.MDA_ES_VALUE_IZM_MKV == 0 && md.MDA_I != 0)
                    md.MDA_I = 0;
                else
                    RefreshI(md, paramTAU);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " Ошибка обработки " + FieldName);
            }
            SaveData(null);
            MeasuringsRefresh();
            RaisePropertyChanged(() => Measurings);
            RaisePropertyChanged(() => buttonCalculateEnabled);
            RefreshGcCollection?.Invoke();
            if (selectedRow == null || selectedRow.MDA_ID != md.MDA_ID)
                selectedRow = md;
            if (FocusedRow == null || FocusedRow.MDA_ID != md.MDA_ID)
                FocusedRow = md;
        }
        //калибровочный коэфф. антенны, для всех строк измерения в режиме
        private void RefreshKa(MODE mode)
        {
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (!keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничего делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            if (AntennaE == null && AntennaH == null)
                return;
            foreach (MEASURING_DATA md in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID))
            {
                RefreshKa(md);
            }
            if (!keySuspendParent) //поток не был остановлен до вызова ф-ии, поэтому его восстанавливаем
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                keySuspend = false;
            }
        }
        private void RefreshKa(MEASURING_DATA md)
        {
            if (String.IsNullOrEmpty(md.MDA_TYPE)) //если не заполнено поле типа измерения, присваиваем E
            {
                md.MDA_TYPE = "E";
                return; //антенна не выбрана
            }
            double F = Functions.F_kGc(md.MDA_F, Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID));
            double vDown = 0, vTop = 0;
            ANTENNA_CALIBRATION dcTop, dcDown;
            ANTENNA Antenna = md.MDA_TYPE == "E" ? AntennaE : AntennaH;
            //ANTENNA antenna = Antennas.Where(p => p.ANT_ID == md.MDA_ANT_ID).FirstOrDefault();
            if (Antenna == null)
                return;
            //актуальный Ка, учитывается, если есть данные антенны и заполнено значение частоты                        
            if (Antenna != null && Antenna.ANTENNA_CALIBRATION != null &&
                Antenna.ANTENNA_CALIBRATION.Count != 0 && F != 0)
            {
                if (Antenna.ANT_F_UNIT_ID == null)
                {
                    MessageBox.Show("В справочнике АНТЕННЫ не заполнено поле 'Единица измерения частоты'");
                    return;
                }
                string edIzm = Functions.GetUnitValue(Units, (int)Antenna.ANT_F_UNIT_ID);
                //значение частоты в таблице преобразовываются в кГц
                var temp = Antenna.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) <= F);
                if (temp.Count() > 0)
                    vDown = temp.Min(p => (F - Functions.F_kGc(p.ANT_CAL_F, edIzm)));
                if (temp.Count() > 0 && vDown == 0)
                {
                    dcDown = Antenna.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                    if (dcDown != null)
                        md.MDA_KA = dcDown.ANT_CAL_VALUE;
                }
                else //нет точного совпадения, найдём ближайшее значение с другой стороны
                {
                    var temp1 = Antenna.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) > F);
                    if (temp1.Count() > 0)
                        vTop = temp1.Min(p => (Functions.F_kGc(p.ANT_CAL_F, edIzm) - F));
                    if (temp1.Count() > 0 && vTop == 0)
                    {
                        dcTop = Antenna.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop).FirstOrDefault();
                        if (dcTop != null)
                            md.MDA_KA = dcTop.ANT_CAL_VALUE;
                    }
                    else
                    {
                        //ближайшие значения Ка сверху и снизу для заданной частоты
                        dcDown =
                            Antenna.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                        dcTop =
                            Antenna.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop)
                                .FirstOrDefault();
                        if (dcDown != null && dcTop != null)
                        {
                            //логарифмическая аппроксимация Ка
                            md.MDA_KA =
                                Math.Round(dcDown.ANT_CAL_VALUE + (Math.Log10(F) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))) * (dcTop.ANT_CAL_VALUE - dcDown.ANT_CAL_VALUE) /
                                    (Math.Log10(Functions.F_kGc(dcTop.ANT_CAL_F, edIzm)) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))), 3);
                        }
                        else //логарифмическая аппроксимация невозможно, берём ближайшее значение
                        {
                            if (dcDown != null)
                                md.MDA_KA = dcDown.ANT_CAL_VALUE;
                            if (dcTop != null)
                                md.MDA_KA = dcTop.ANT_CAL_VALUE;
                        }
                    }
                }
            }
            else
                md.MDA_KA = 0;

        }
        //калибровочный коэфф. Эквивалента сети, в зависимости от частоты, для фазы
        private void RefreshKF(MEASURING_DATA md)
        {
            //if (md.MDA_KF != 0)
            //    return; 
            double F = Functions.F_kGc(md.MDA_F, Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID));
            double vDown = 0, vTop = 0;
            ANTENNA_CALIBRATION dcTop, dcDown;
            if (AntennaF == null)
                return;
            if (!AntennaF.ANT_TYPE.Contains("фаза"))
            {
                MessageBox.Show("Выберите антенну, содержащую в названии слово'фаза'");
                return;
            }
            //ANTENNA antenna = Antennas.Where(p => p.ANT_TYPE.Contains("фаза")).FirstOrDefault();
            //актуальный К, учитывается, если есть данные антенны и заполнено значение частоты                        
            if (AntennaF != null && AntennaF.ANTENNA_CALIBRATION != null &&
                AntennaF.ANTENNA_CALIBRATION.Count != 0 && F != 0)
            {
                if (AntennaF.ANT_F_UNIT_ID == null)
                {
                    MessageBox.Show("В справочнике АНТЕННЫ не заполнено поле 'Единица измерения частоты'");
                    return;
                }
                string edIzm = Functions.GetUnitValue(Units, (int)AntennaF.ANT_F_UNIT_ID);
                //значение частоты в таблице преобразовываются в кГц
                var temp = AntennaF.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) <= F);
                if (temp.Count() > 0)
                    vDown = temp.Min(p => (F - Functions.F_kGc(p.ANT_CAL_F, edIzm)));
                if (temp.Count() > 0 && vDown == 0)
                {
                    dcDown = AntennaF.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                    if (dcDown != null)
                        md.MDA_KF = dcDown.ANT_CAL_VALUE;
                }
                else //нет точного совпадения, найдём ближайшее значение с другой стороны
                {
                    var temp1 = AntennaF.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) > F);
                    if (temp1.Count() > 0)
                        vTop = temp1.Min(p => (Functions.F_kGc(p.ANT_CAL_F, edIzm) - F));
                    if (temp1.Count() > 0 && vTop == 0)
                    {
                        dcTop = AntennaF.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop).FirstOrDefault();
                        if (dcTop != null)
                            md.MDA_KF = dcTop.ANT_CAL_VALUE;
                    }
                    else
                    {
                        //ближайшие значения К сверху и снизу для заданной частоты
                        dcDown =
                            AntennaF.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                        dcTop =
                            AntennaF.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop)
                                .FirstOrDefault();
                        if (dcDown != null && dcTop != null)
                        {
                            //логарифмическая аппроксимация К
                            md.MDA_KF =
                                Math.Round(dcDown.ANT_CAL_VALUE + (Math.Log10(F) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))) * (dcTop.ANT_CAL_VALUE - dcDown.ANT_CAL_VALUE) /
                                    (Math.Log10(Functions.F_kGc(dcTop.ANT_CAL_F, edIzm)) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))), 3);
                        }
                        else //логарифмическая аппроксимация невозможно, берём ближайшее значение
                        {
                            if (dcDown != null)
                                md.MDA_KF = dcDown.ANT_CAL_VALUE;
                            if (dcTop != null)
                                md.MDA_KF = dcTop.ANT_CAL_VALUE;
                        }
                    }
                }
            }
            else
                md.MDA_KF = 0;

        }
        //калибровочный коэфф. Эквивалента сети, в зависимости от частоты, для ноль
        private void RefreshK0(MEASURING_DATA md)
        {
            if (md.MDA_K0 != 0)
                return; //антенна не выбрана
            double F = Functions.F_kGc(md.MDA_F, Functions.GetUnitValue(Units, md.MDA_F_UNIT_ID));
            double vDown = 0, vTop = 0;
            ANTENNA_CALIBRATION dcTop, dcDown;

            //ANTENNA antenna = Antennas.Where(p => p.ANT_TYPE.Contains("ноль")).FirstOrDefault();
            if (Antenna0 == null)
                return;
            if (!Antenna0.ANT_TYPE.Contains("ноль"))
            {
                MessageBox.Show("Выберите антенну, содержащую в названии слово'ноль'");
                return;
            }
            //актуальный К, учитывается, если есть данные антенны и заполнено значение частоты                        
            if (Antenna0 != null && Antenna0.ANTENNA_CALIBRATION != null &&
                Antenna0.ANTENNA_CALIBRATION.Count != 0 && F != 0)
            {
                if (Antenna0.ANT_F_UNIT_ID == null)
                {
                    MessageBox.Show("В справочнике АНТЕННЫ не заполнено поле 'Единица измерения частоты'");
                    return;
                }
                string edIzm = Functions.GetUnitValue(Units, (int)Antenna0.ANT_F_UNIT_ID);
                //значение частоты в таблице преобразовываются в кГц
                var temp = Antenna0.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) <= F);
                if (temp.Count() > 0)
                    vDown = temp.Min(p => (F - Functions.F_kGc(p.ANT_CAL_F, edIzm)));
                if (temp.Count() > 0 && vDown == 0)
                {
                    dcDown = Antenna0.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                    if (dcDown != null)
                        md.MDA_K0 = dcDown.ANT_CAL_VALUE;
                }
                else //нет точного совпадения, найдём ближайшее значение с другой стороны
                {
                    var temp1 = Antenna0.ANTENNA_CALIBRATION.Where(d => Functions.F_kGc(d.ANT_CAL_F, edIzm) > F);
                    if (temp1.Count() > 0)
                        vTop = temp1.Min(p => (Functions.F_kGc(p.ANT_CAL_F, edIzm) - F));
                    if (temp1.Count() > 0 && vTop == 0)
                    {
                        dcTop = Antenna0.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop).FirstOrDefault();
                        if (dcTop != null)
                            md.MDA_K0 = dcTop.ANT_CAL_VALUE;
                    }
                    else
                    {
                        //ближайшие значения Ка сверху и снизу для заданной частоты
                        dcDown =
                            Antenna0.ANTENNA_CALIBRATION.Where(d => (F - Functions.F_kGc(d.ANT_CAL_F, edIzm)) == vDown).FirstOrDefault();
                        dcTop =
                            Antenna0.ANTENNA_CALIBRATION.Where(d => (Functions.F_kGc(d.ANT_CAL_F, edIzm) - F) == vTop)
                                .FirstOrDefault();
                        if (dcDown != null && dcTop != null)
                        {
                            //логарифмическая аппроксимация Ка
                            md.MDA_K0 =
                                Math.Round(dcDown.ANT_CAL_VALUE + (Math.Log10(F) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))) * (dcTop.ANT_CAL_VALUE - dcDown.ANT_CAL_VALUE) /
                                    (Math.Log10(Functions.F_kGc(dcTop.ANT_CAL_F, edIzm)) - Math.Log10(Functions.F_kGc(dcDown.ANT_CAL_F, edIzm))), 3);
                        }
                        else //логарифмическая аппроксимация невозможно, берём ближайшее значение
                        {
                            if (dcDown != null)
                                md.MDA_KA = dcDown.ANT_CAL_VALUE;
                            if (dcTop != null)
                                md.MDA_K0 = dcTop.ANT_CAL_VALUE;
                        }
                    }
                }
            }
            else
                md.MDA_K0 = 0;

        }
        //обновление списка организаций и выбор вровь добавленной или первой в списке
        private void RefreshOrganization(int orgId)
        {
            Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
            if (refreshAfterReload) // после перечитывания таблицы после обновления другим пользователем, выбранные данные обновлять не надо
                return;
            if (orgId != 0)
                org_id = orgId;
            else
            {
                if (Organizations.Any())
                    org_id = Organizations[0].ORG_ID;

            }
        }
        //обновление списка типов АРМ для выбранного исследования(работа + счёт)
        private void RefreshArmTypes(int atId)
        {
            ArmTypes = new ObservableCollection<ARM_TYPE>(methodsEntities.ARM_TYPE.Where(p => p.AT_ANL_ID == anl_id));
            if (refreshAfterReload) // после перечитывания таблицы после обновления другим пользователем, выбранные данные обновлять не надо
                return;
            if (atId != 0)
                at_id = atId;
            else
                if (ArmTypes != null && ArmTypes.Count > 0)
                at_id = ArmTypes[0].AT_ID;
            else
                at_id = 0;
        }
        private void RefreshArmTypes() //обновление типов с выбором первого типа в списке
        {
            ArmTypes = new ObservableCollection<ARM_TYPE>(methodsEntities.ARM_TYPE.Where(p => p.AT_ANL_ID == anl_id));
            if (ArmTypes != null && ArmTypes.Count > 0)
                at_id = ArmTypes[0].AT_ID;
            else
                at_id = 0;
        }
        //обновление списка исследований и выбор вновь добавленного
        private void RefreshAnalysis(string invoice)
        {
            analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id));
            if (refreshAfterReload) // после перечитывания таблицы после обновления другим пользователем, выбранные данные обновлять не надо
                return;
            //при выборе заказчика, показываем инф-ю по его первому счёту
            if (analysis.Any())
                if (!String.IsNullOrEmpty(invoice))
                {
                    var t = analysis.Where(p => p.ANL_ORG_ID == org_id && p.ANL_INVOICE == invoice).FirstOrDefault();
                    if (t != null)
                        anl_id = analysis.Where(p => p.ANL_ORG_ID == org_id && p.ANL_INVOICE == invoice).FirstOrDefault().ANL_ID;
                    else
                        anl_id = analysis.Where(p => p.ANL_ORG_ID == org_id).FirstOrDefault().ANL_ID;
                }
                else
                    anl_id = analysis.Where(p => p.ANL_ORG_ID == org_id).FirstOrDefault().ANL_ID;
            else
            {
                //analysis = new ObservableCollection<ANALYSIS>();
                anl_id = 0;
            }
        }

        //обновление списка и выбранного АРМ
        private void RefreshArms(string armNumber)
        {
            bool t = methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id).Any();
            if (at_id != 0 && t)
                Arms = new ObservableCollection<ARM>(methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id).OrderBy(p => p.ARM_NUMBER)); //.OrderBy(p => p.ARM_NUMBER)
            else
                Arms = null;
            //Arms = new ObservableCollection<ARM>();
            if (refreshAfterReload) // после перечитывания таблицы после обновления другим пользователем, выбранные данные обновлять не надо
                return;
            if (Arms != null && Arms.Count > 0)
                if (!String.IsNullOrEmpty(armNumber))
                {
                    var temp = Arms.Where(p => p.ARM_NUMBER == armNumber).FirstOrDefault().ARM_ID;
                    if (arm_id != temp)
                        arm_id = temp;
                }
                else
                    arm_id = Arms[0].ARM_ID;
            else
            {
                arm_id = 0;//следует  arm_one = null;
            }
            var tt = methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == arm_id);
            //count = tt.Count();
            Results = new ObservableCollection<RESULT>(); // 
            foreach (var result in tt)
            {
                Results.Add(result);
            }

            // people.Where(p => animals.Any(a => p.Pet.Contains(a)));
            //filterResults = String.Empty;
            //обновим таблицы
            //RefreshGcResults?.Invoke();
            if (arm_one != null)
            {
                MakeMaxValue();
                //обновим таблицы
                RefreshGcResults?.Invoke();
                ArmEquipments = new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_PARENT_ID == null));
                RaisePropertyChanged(() => armSVT);
                RaisePropertyChanged(() => armTT);
                RaisePropertyChanged(() => cbeTTEnabled);
            }
            //выполняется в arm_id
            //if (refreshAfterConfig && Properties.Settings.Default.modeId != 0 && methodsEntities.MODE.Where(p => p.MODE_ID == Properties.Settings.Default.modeId).Any())
            //    RefreshModes(Properties.Settings.Default.modeId);
            //else
            //    RefreshModes(0);

        }
        private ObservableCollection<ARM> GetArms(int atId)
        {
            if (atId != 0)
                return new ObservableCollection<ARM>(methodsEntities.ARM.Where(p => p.ARM_AT_ID == atId).OrderBy(p => p.ARM_NUMBER));
            else
                return new ObservableCollection<ARM>();
        }
        //обновление списка MODE
        private void RefreshModes(int modeId)
        {
            //через интерфейс обновляется selectedMode после создания нового Modes. Поэтому selectedMode надо восстановить.
            if (arm_id != 0)
                Modes = new ObservableCollection<MODE>(methodsEntities.MODE.Where(p => p.MODE_ARM_ID == arm_id));//.OrderBy(p => p.MODE_TYPE.MT_NAME));
            else
                Modes = new ObservableCollection<MODE>();

            if (refreshAfterReload) // после перечитывания таблицы после обновления другим пользователем, выбранные данные обновлять не надо
            {
                RefreshGcModes?.Invoke();
                return;
            }
            if (Modes != null && Modes.Count > 0)
            {
                if (modeId == 0) //формирование списка после смены АРМа
                {
                    //пройдёмся по строкам и заполним пустые значения зн-ми по умолчанию
                    foreach (MODE mode in Modes)
                    {
                        //значения по умолчанию
                        if (mode.MODE_FT_UNIT_ID == null || mode.MODE_FT_UNIT_ID == 0)
                            mode.MODE_FT_UNIT_ID = Functions.GetUnitID(Units, "мГц");
                        if (mode.MODE_RBW_UNIT_ID == null || mode.MODE_RBW_UNIT_ID == 0)
                            mode.MODE_RBW_UNIT_ID = Functions.GetUnitID(Units, "кГц");
                        if (mode.MODE_TAU_UNIT_ID == null || mode.MODE_TAU_UNIT_ID == 0)
                            mode.MODE_TAU_UNIT_ID = Functions.GetUnitID(Units, "нсек");
                        if (mode.MODE_KS == 0)
                            mode.MODE_KS = 0.8;
                        if (mode.MODE_D == 0)
                            mode.MODE_D = 1;
                        if (mode.MODE_ANT_GS == null)
                            mode.MODE_ANT_GS = true;
                        if (String.IsNullOrEmpty(mode.MODE_SVT))
                            mode.MODE_SVT = "ГШ вблизи СВТ";

                    }
                    selectedMode = Modes.Where(p => p.MODE_ARM_ID == arm_id).FirstOrDefault();
                }
                else
                    selectedMode = Modes.Where(p => p.MODE_ID == modeId).FirstOrDefault();
            }
            focusedMode = selectedMode;
            RefreshGcModes?.Invoke();
        }

        //добавление новой организации-заказчика
        private void AddOrganization(Object o)
        {
            ORGANIZATION organization_new = new ORGANIZATION();
            AddOrganizationWindow addWindow = new AddOrganizationWindow(organization_new);
            addWindow.ShowDialog();
            if (String.IsNullOrEmpty(organization_new.ORG_NAME))
                return;
            else
            {
                if (methodsEntities.ORGANIZATION.Where(p => p.ORG_NAME == organization_new.ORG_NAME).Any())
                {
                    MessageBox.Show("В базе данных уже имеется работа с таким названием.");
                    return;
                }
            }

            methodsEntities.ORGANIZATION.Add(organization_new);
            SaveData(null);
            //обновление списка организаций-заказчиков и выбор вновь добавленного заказчика            
            RefreshOrganization(organization_new.ORG_ID);
        }
        private bool canAddAnalysis(Object o)
        {
            return (org_id != 0);
        }
        //добавление нового исследования(счёта) выбранного заказчика
        private void AddAnalysis(Object o)
        {
            if (org_id == 0)
            {
                MessageBox.Show("Добавьте работу.");
                return;
            }
            ANALYSIS analysis_new = new ANALYSIS()
            {
                ANL_ORG_ID = org_id
            };
            AddAnalisisWindow addWindow = new AddAnalisisWindow(analysis_new);
            addWindow.ShowDialog();
            if (String.IsNullOrEmpty(analysis_new.ANL_INVOICE))
                return;
            else
            {
                if (methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id && p.ANL_NAME == analysis_new.ANL_NAME).Any())
                {
                    MessageBox.Show("В базе данных уже имеется счёт с таким же названием для выбранной работы.");
                    return;
                }
            }
            methodsEntities.ANALYSIS.Add(analysis_new);
            SaveData(null);
            //обновление списка исследований и выбор добавленного счёта
            RefreshAnalysis(analysis_new.ANL_INVOICE);
        }
        //добавление нового типа ARM
        private void CancelArmType(Object o)
        {
            WindowClose();
        }
        private void AddArmType(Object o)
        {
            ARM_TYPE armType_new = new ARM_TYPE();
            if (anl_id == 0)
            {
                MessageBox.Show("Добавьте счёт.");
                return;
            }
            armType_new.AT_ANL_ID = anl_id;

            AddArmTypeWindow addWindow = new AddArmTypeWindow(armType_new);
            addWindow.ShowDialog();
            if (String.IsNullOrEmpty(armType_new.AT_NAME))
                return;
            methodsEntities.ARM_TYPE.Add(armType_new);
            SaveData(null);
            //выбираем добавленный тип АРМ
            at_id = methodsEntities.ARM_TYPE.Where(p => p.AT_NAME == armType_new.AT_NAME).FirstOrDefault().AT_ID;

            RefreshArmTypes(at_id);
        }
        private void RenameArmType(Object o)
        {
            ARM_TYPE at = methodsEntities.ARM_TYPE.Where(p => p.AT_ID == at_id).FirstOrDefault();
            if (at == null)
                return;
            string namePrev = at.AT_NAME;
            AddArmTypeWindow renameWindow = new AddArmTypeWindow(at)
            {
                Title = "Переименование тира АРМ",
            };
            renameWindow.ShowDialog();

            if (String.IsNullOrEmpty(at.AT_NAME))
                return;
            if (methodsEntities.ARM_TYPE.Where(p => p.AT_ANL_ID == at.AT_ANL_ID && p.AT_NAME == at.AT_NAME).Any())
            {
                at.AT_NAME = namePrev;
                MessageBox.Show("Есть такое имя типа ARM");
                return;
            }
            SaveData(null);
            //обновление имени на форме
            RefreshArmTypes(at_id);
        }
        //полное копирование типа ARM
        private void CopyArmType(Object o)
        {
            AllArmsView aav = new AllArmsView(); //строка для копирования с новым названием типа
            ObservableCollection<AllArmsView> col = new ObservableCollection<AllArmsView>(methodsEntities.AllArmsView);
            SelectWindow sw = new SelectWindow(col, aav);
            sw.ShowDialog();
            if (aav == null || aav.AT_ID == 0)
                return; // тип АРМ для копирования не выбран

            ARM_TYPE atNew;
            if (!keySuspend) //функция вызвана с работающим фоновым потоком
            {
                keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            //Копирование
            using (var transaction = methodsEntities.Database.BeginTransaction())
            {
                try
                {
                    atNew = new ARM_TYPE()
                    {
                        AT_ANL_ID = anl_id,
                        AT_NAME = aav.armTypeNew
                    };
                    methodsEntities.ARM_TYPE.Add(atNew);
                    try
                    {
                        methodsEntities.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                        transaction.Rollback();
                        if (keySuspend)
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                            keySuspend = false;
                        }
                        return;
                    }
                    //копирование АРМ в типе
                    ARM armNew;

                    foreach (ARM arm in methodsEntities.ARM.Where(p => p.ARM_AT_ID == aav.AT_ID))
                    {
                        armNew = new ARM()
                        {
                            ARM_AT_ID = atNew.AT_ID,
                            ARM_IK = arm.ARM_IK,
                            ARM_KATEGORIA = arm.ARM_KATEGORIA,
                            ARM_NK = arm.ARM_NK,
                            ARM_NOTE = arm.ARM_NOTE,
                            ARM_NUMBER = arm.ARM_NUMBER,
                            ARM_SVT = arm.ARM_SVT,
                            ARM_TT = arm.ARM_TT
                        };
                        methodsEntities.ARM.Add(armNew);
                        try
                        {
                            methodsEntities.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                            transaction.Rollback();
                            if (keySuspend)
                            {
#pragma warning disable CS0618 // Type or member is obsolete
                                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                                keySuspend = false;
                            }
                            return;
                        }
                        if (arm.ANTENNA_ARM != null && arm.ANTENNA_ARM.Count() != 0)
                        {
                            armNew.ANTENNA_ARM = new Collection<ANTENNA_ARM>();
                            ANTENNA_ARM aaNew;
                            foreach (var aa in arm.ANTENNA_ARM)
                            {
                                aaNew = new ANTENNA_ARM()
                                {
                                    ANTARM_ARM_ID = armNew.ARM_ID,
                                    ANTARM_ANT_ID = aa.ANTARM_ANT_ID
                                };
                                methodsEntities.ANTENNA_ARM.Add(aaNew);
                            }
                        }
                        if (arm.EQUIPMENT != null && arm.EQUIPMENT.Count() != 0)
                        {
                            armNew.EQUIPMENT = new Collection<EQUIPMENT>();
                            EQUIPMENT eqNew;
                            foreach (var eq in arm.EQUIPMENT)
                            {
                                eqNew = eq.DeepCopy();
                                eqNew.EQ_ARM_ID = armNew.ARM_ID;
                                methodsEntities.EQUIPMENT.Add(eqNew);
                            }
                        }
                        //скопируем родителей для тех строк, где они есть после сохраниния изменений
                        if (arm.MEASURING_EQUIPMENT_ARM != null && arm.MEASURING_EQUIPMENT_ARM.Count() != 0)
                        {
                            armNew.MEASURING_EQUIPMENT_ARM = new Collection<MEASURING_EQUIPMENT_ARM>();
                            MEASURING_EQUIPMENT_ARM meaNew;
                            foreach (var mea in arm.MEASURING_EQUIPMENT_ARM)
                            {
                                meaNew = new MEASURING_EQUIPMENT_ARM()
                                {
                                    MEARM_ARM_ID = armNew.ARM_ID,
                                    MEARM_ME_ID = mea.MEARM_ME_ID
                                };
                                methodsEntities.MEASURING_EQUIPMENT_ARM.Add(meaNew);
                            }
                        }
                        if (arm.MEASURING_DEVICE_ARM != null && arm.MEASURING_DEVICE_ARM.Count() != 0)
                        {
                            armNew.MEASURING_DEVICE_ARM = new Collection<MEASURING_DEVICE_ARM>();
                            MEASURING_DEVICE_ARM mdaNew;
                            foreach (var mda in arm.MEASURING_DEVICE_ARM)
                            {
                                mdaNew = new MEASURING_DEVICE_ARM()
                                {
                                    MDARM_ARM_ID = armNew.ARM_ID,
                                    MDARM_MD_ID = mda.MDARM_MD_ID
                                };
                                methodsEntities.MEASURING_DEVICE_ARM.Add(mdaNew);
                            }
                        }
                        if (arm.MODE != null && arm.MODE.Count() != 0)
                        {
                            armNew.MODE = new Collection<MODE>();
                            MODE mNew;
                            foreach (var m in arm.MODE)
                            {
                                mNew = m.DeepCopy();
                                mNew.MODE_ARM_ID = armNew.ARM_ID;
                                methodsEntities.MODE.Add(mNew);
                            }
                        }
                        try
                        {
                            methodsEntities.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                            if (keySuspend)
                            {
#pragma warning disable CS0618 // Type or member is obsolete
                                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                                keySuspend = false;
                            }
                            return;
                        }
                    }
                    //подкорректируем ссылки на родителей в скопированном оборудовании
                    foreach (var temp in methodsEntities.ARM.Where(p => p.ARM_AT_ID == atNew.AT_ID)) //для каждого нового АРМа
                    {
                        foreach (EQUIPMENT eq in temp.EQUIPMENT.Where(p => p.EQ_PARENT_ID != null))//для каждого оборудования в новом АРМе
                        {
                            var parentOld = methodsEntities.EQUIPMENT.Where(p => p.EQ_ID == eq.EQ_PARENT_ID).FirstOrDefault();//родитель-оригинал
                            var parentNew = methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == eq.EQ_ARM_ID &&
                                                                            p.EQ_MODEL == parentOld.EQ_MODEL &&
                                                                            p.EQ_SERIAL == parentOld.EQ_SERIAL).FirstOrDefault(); //строка родителя-оригинала
                            if (parentNew != null)
                                eq.EQ_PARENT_ID = parentNew.EQ_ID;
                        }
                    }
                    try
                    {
                        methodsEntities.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                        transaction.Rollback();
                        if (keySuspend)
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                            keySuspend = false;
                        }
                        return;
                    }

                    RefreshArmTypes(atNew.AT_ID);//обновление коллекции типов АРМ и выбор нового типа
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка копирования " + aav.AT_NAME + ". " + e.Message);
                    transaction.Rollback();
                    return;
                }
                finally
                {
                    if (keySuspend)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                        keySuspend = false;
                    }
                }
            }
        }
        //добавление нового ARM (с новым номером)
        private void AddArm(Object o)
        {
            if (ArmTypes == null || ArmTypes.Count == 0)
            {
                MessageBox.Show(("Заполните тип АРМа."));
                return;
            }

            ARM arm_new = new ARM(); ;
            arm_new.ARM_AT_ID = at_id;
            arm_new.ARM_KATEGORIA = 2;
            string numberPrev = String.Empty;

            if (Arms == null || Arms.Count == 0) //первый АРМ в типе
            {
                //открыть окно для ввода номера первого АРМ
                AddArmWindow addWindow = new AddArmWindow(arm_new);
                try
                {
                    addWindow.ShowDialog();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }

                if (String.IsNullOrEmpty(arm_new.ARM_NUMBER))
                    return;
                //добавим к первому АРМ измерительную аппаратуру и антенну по умолчанию
                arm_new.MEASURING_DEVICE_ARM = new Collection<MEASURING_DEVICE_ARM>();
                MEASURING_DEVICE_ARM newMda;
                foreach (var md in methodsEntities.MEASURING_DEVICE.Where(p => p.MD_DEFAULT == "да"))
                {
                    newMda = new MEASURING_DEVICE_ARM()
                    {
                        MDARM_MD_ID = md.MD_ID
                    };
                    arm_new.MEASURING_DEVICE_ARM.Add(newMda);
                }
                //добавим антенну по умолчанию
                arm_new.ANTENNA_ARM = new Collection<ANTENNA_ARM>();
                ANTENNA_ARM aa;
                if (methodsEntities.ANTENNA.Where(p => p.ANT_DEFAULT == "да").Count() != 0)
                {
                    aa = new ANTENNA_ARM()
                    {
                        ANTARM_ANT_ID = methodsEntities.ANTENNA.Where(p => p.ANT_DEFAULT == "да").FirstOrDefault().ANT_ID
                    };
                    arm_new.ANTENNA_ARM.Add((aa));
                }
                methodsEntities.ARM.Add(arm_new);
                SaveData(null);
                //выбираем добавленный тип АРМ
                RefreshArms(
                methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id && p.ARM_NUMBER == arm_new.ARM_NUMBER)
                    .FirstOrDefault()
                    .ARM_NUMBER);
                return;
            }
            else //копирование АРМ
            {
                numberPrev = arm_one.ARM_NUMBER; //Arms[Arms.Count - 1].ARM_NUMBER;                
                //если номер последнего введённого АРМ не соответствует шаблону, то номер вводим вручную
                if (numberPrev.Length != 10 ||
                    (!Int32.TryParse(numberPrev.Substring(0, 4), out int result1) ||
                     !Int32.TryParse(numberPrev.Substring(5, 5), out int result2) || numberPrev.Substring(4, 1) != "-"))
                {
                    //открыть окно для ввода номера первого АРМ
                    AddArmWindow addWindow = new AddArmWindow(arm_new);
                    try
                    {
                        addWindow.ShowDialog();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                    if (arm_new.ARM_NUMBER == null) //не введено значение
                    {
                        MessageBox.Show("Номер АРМа не был задан.");
                        return;
                    }
                }
                else
                {
                    if (DialogResult.Yes == MessageBox.Show("Сгенерировать номер АРМ?", "", MessageBoxButtons.YesNo))
                    {
                        Int32.TryParse(numberPrev.Substring(5, 5), out result2);
                        int i = 1;
                        string newNumber = (numberPrev.Substring(0, 5) + (result2 + i).ToString("00000"));
                        while (methodsEntities.ARM.Where(p => p.ARM_NUMBER == newNumber).Any())
                        {
                            newNumber = (numberPrev.Substring(0, 5) + (result2 + i).ToString("00000"));
                            i++;
                        }
                        arm_new.ARM_NUMBER = newNumber;
                    }
                    else
                    {
                        //открыть окно для ввода номера первого АРМ
                        AddArmWindow addWindow = new AddArmWindow(arm_new);
                        try
                        {
                            addWindow.ShowDialog();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return;
                        }

                        if (String.IsNullOrEmpty(arm_new.ARM_NUMBER))
                            return;
                    }
                }
                arm_new.ARM_KATEGORIA = Arms[Arms.Count - 1].ARM_KATEGORIA;
                arm_new.ARM_SVT = Arms[Arms.Count - 1].ARM_SVT;
                arm_new.ARM_NK = Arms[Arms.Count - 1].ARM_NK;
                arm_new.ARM_IK = Arms[Arms.Count - 1].ARM_IK;
                arm_new.ARM_TT = Arms[Arms.Count - 1].ARM_TT;
                if (Arms[0].MODE != null)
                {
                    arm_new.MODE = new Collection<MODE>();
                    //копирование режимов
                    foreach (MODE mode in Arms[0].MODE)
                    {
                        MODE newMode = new MODE()
                        {
                            MODE_ITEM = mode.MODE_ITEM,
                            MODE_MT_ID = mode.MODE_MT_ID,
                            MODE_AUTORBW = mode.MODE_AUTORBW,
                            MODE_ITUNIFIED = mode.MODE_ITUNIFIED,
                            MODE_FT = mode.MODE_FT,
                            MODE_TAU = mode.MODE_TAU,
                            MODE_IS_SOLID = mode.MODE_IS_SOLID,
                            MODE_CONTR_E = mode.MODE_CONTR_E,
                            MODE_FT_UNIT_ID = mode.MODE_FT_UNIT_ID,
                            MODE_D = mode.MODE_D,
                            MODE_R = mode.MODE_R,
                            MODE_RMAX = mode.MODE_RMAX,
                            MODE_L = mode.MODE_L,
                            MODE_ANT_GS = mode.MODE_ANT_GS,
                            MODE_RBW = mode.MODE_RBW,
                            MODE_RBW_UNIT_ID = mode.MODE_RBW_UNIT_ID,
                            MODE_SVT = mode.MODE_SVT,
                            MODE_NORMA = mode.MODE_NORMA,
                            MODE_KS = mode.MODE_KS,
                            MODE_EQ_ID = mode.MODE_EQ_ID
                        };
                        arm_new.MODE.Add((newMode));
                    }

                }
                if (Arms[0].EQUIPMENT != null)
                {
                    arm_new.EQUIPMENT = new Collection<EQUIPMENT>();
                    //копирование оборудования верхнего уровня
                    foreach (EQUIPMENT eq in Arms[0].EQUIPMENT)
                    {
                        EQUIPMENT newEq = eq.DeepCopy();
                        newEq.EQ_ARM_ID = arm_new.ARM_ID;
                        arm_new.EQUIPMENT.Add((newEq));
                    }
                }

                if (Arms[0].MEASURING_DEVICE_ARM != null)
                {
                    arm_new.MEASURING_DEVICE_ARM = new Collection<MEASURING_DEVICE_ARM>();
                    //копирование 
                    foreach (MEASURING_DEVICE_ARM mda in Arms[0].MEASURING_DEVICE_ARM)
                    {
                        MEASURING_DEVICE_ARM newMda = new MEASURING_DEVICE_ARM()
                        {
                            MDARM_MD_ID = mda.MDARM_MD_ID
                        };
                        arm_new.MEASURING_DEVICE_ARM.Add((newMda));
                    }
                }
                if (Arms[0].MEASURING_EQUIPMENT_ARM != null)
                {
                    arm_new.MEASURING_EQUIPMENT_ARM = new Collection<MEASURING_EQUIPMENT_ARM>();
                    //копирование 
                    foreach (MEASURING_EQUIPMENT_ARM mea in Arms[0].MEASURING_EQUIPMENT_ARM)
                    {
                        MEASURING_EQUIPMENT_ARM newMea = new MEASURING_EQUIPMENT_ARM()
                        {
                            MEARM_ME_ID = mea.MEARM_ME_ID
                        };
                        arm_new.MEASURING_EQUIPMENT_ARM.Add((newMea));
                    }
                }
                if (Arms[0].ANTENNA_ARM != null)
                {
                    arm_new.ANTENNA_ARM = new Collection<ANTENNA_ARM>();
                    //копирование 
                    foreach (ANTENNA_ARM aa in Arms[0].ANTENNA_ARM)
                    {
                        ANTENNA_ARM newAa = new ANTENNA_ARM()
                        {
                            ANTARM_ANT_ID = aa.ANTARM_ANT_ID
                        };
                        arm_new.ANTENNA_ARM.Add((newAa));
                    }
                }
            }
            methodsEntities.ARM.Add(arm_new);
            SaveData(null);
            //корректировка родительского id в оборудовании нового АРМ
            //выбираем добавленный АРМ
            RefreshArms(
                methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id && p.ARM_NUMBER == arm_new.ARM_NUMBER)
                    .FirstOrDefault()
                    .ARM_NUMBER);
            ARM temp = methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id && p.ARM_NUMBER == arm_new.ARM_NUMBER)
                .FirstOrDefault(); //добавленный АРМ
            foreach (EQUIPMENT eq in temp.EQUIPMENT.Where(p => p.EQ_PARENT_ID != null))//поменяем id родителя
            {
                EQUIPMENT parentEq = methodsEntities.EQUIPMENT.Where(p => p.EQ_ID == eq.EQ_PARENT_ID).FirstOrDefault();
                if (parentEq != null)

                    eq.EQ_PARENT_ID = methodsEntities.EQUIPMENT.Where(p => p.EQ_MODEL == parentEq.EQ_MODEL &&
                                                                       p.EQ_SERIAL == parentEq.EQ_SERIAL &&
                                                                       p.EQ_ARM_ID == temp.ARM_ID)
                    .FirstOrDefault().EQ_ID;
            }
            //если сер.номер копируемого АРМ содержит номер АРМ, обновим его в скорированных строках
            foreach (EQUIPMENT eq in temp.EQUIPMENT)
            {
                if (eq.EQ_SERIAL != null && eq.EQ_SERIAL.Contains(numberPrev))
                    eq.EQ_SERIAL = eq.EQ_SERIAL.Replace(numberPrev, arm_new.ARM_NUMBER);
            }
            SaveData(null);
        }
        //Удаление заказчика и всех его исследований
        private void DeleteOrganization(Object o)
        {
            if (org_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ORG_ID == org_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            if (DialogResult.Yes !=
                MessageBox.Show("Все исследования выбранной работы в БД будут удалены. Вы согласны?",
                    " ", MessageBoxButtons.YesNo))
                return;
            foreach (ANALYSIS anl in methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id))
            {
                methodsEntities.ANALYSIS.Remove(anl);
            }
            methodsEntities.ORGANIZATION.Remove(
                methodsEntities.ORGANIZATION.Where(p => p.ORG_ID == org_id).FirstOrDefault());
            SaveData(null);

            RefreshOrganization(0);

        }
        //Удаление строки ANALYSIS
        private void DeleteAnalysis(Object o)
        {
            if (anl_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ANL_ID == anl_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            if (DialogResult.Yes != MessageBox.Show("Выбранное исследование будет удалено. Вы согласны?",
                " ", MessageBoxButtons.YesNo))
                return;
            methodsEntities.ANALYSIS.Remove(methodsEntities.ANALYSIS.Where(p => p.ANL_ID == anl_id).FirstOrDefault());
            SaveData(null);
            RefreshAnalysis(String.Empty);

        }
        private bool canDeleteArm()
        {
            return (arm_id != 0);
        }
        //Изменение номера АРМ
        private void RenameARM(Object o)
        {
            string numberPrev = arm_one.ARM_NUMBER;
            if (arm_id == 0)
                return;
            AddArmWindow renameWindow = new AddArmWindow(arm_one)
            {
                Title = "Переименование АРМ"
            };
            try
            {
                renameWindow.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            if (methodsEntities.ARM.Where(p => p.ARM_AT_ID == arm_one.ARM_AT_ID && p.ARM_NUMBER == arm_one.ARM_NUMBER).Any()) //есть такое имя в БД
            {
                arm_one.ARM_NUMBER = numberPrev;
                MessageBox.Show("Есть такое имя ARM");
                return;
            }
            //обновление имени на форме
            string ARMNumber_new = arm_one.ARM_NUMBER;
            SaveData(null);
            RefreshArms(ARMNumber_new);
        }
        private void DeleteArm(Object o)
        {
            if (arm_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_ARM_ID == arm_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            if (DialogResult.Yes != MessageBox.Show("Данные выбранного АРМ в БД будут удалены. Вы согласны?",
                " ", MessageBoxButtons.YesNo))
                return;

            methodsEntities.ARM.Remove(
               methodsEntities.ARM.Where(p => p.ARM_ID == arm_id).FirstOrDefault());
            SaveData(null);
            RefreshArms(String.Empty);
        }
        private bool canDeleteArmType()
        {
            return at_id != 0;
        }
        //Удаление строки типа ARM
        private void DeleteArmType(Object o)
        {
            if (at_id == 0)
                return;
            if (methodsEntities.CurrentUserTask.Where(p => p.CUT_AT_ID == at_id && p.CUT_USER_NAME != userName).Any())
            {
                MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                return;
            }
            if (DialogResult.Yes != MessageBox.Show("Данные выбранного типа АРМ в БД будут удалены. Вы согласны?",
                " ", MessageBoxButtons.YesNo))
                return;

            methodsEntities.ARM_TYPE.Remove(
               methodsEntities.ARM_TYPE.Where(p => p.AT_ID == at_id).FirstOrDefault());
            SaveData(null);
            RefreshArmTypes(0);
        }
        private bool canDeleteMode()
        {
            return focusedMode != null;
        }
        //Удаление строки MODE, работает корректно и с отсортированными данными
        private void DeleteMode(MODE focusedRow)
        {
            try
            {
                if (focusedRow == null)
                    return;
                if (methodsEntities.CurrentUserTask.Where(p => p.CUT_MODE_ID == focusedRow.MODE_ID && p.CUT_USER_NAME != userName).Any())
                {
                    MessageBox.Show("Выбранную строку удалить невозможно, т.к. она используется другим пользователем.");
                    return;
                }
                keyCalculate = false;

                backgroundWorkerDeleteAll.RunWorkerAsync();
            }
            catch (Exception e) { MessageBox.Show("Ошибка ударения режима. " + e.Message); }

        }

        private bool canDeleteMeasuringData()
        {
            return (FocusedRow != null);
        }
        private bool canDeleteMeasuringDataMode()
        {
            if (Measurings == null)
                return false;
            return (Measurings.Any());
        }
        //Удаление строки MEASURING_DATA
        private void DeleteMeasuringData(MEASURING_DATA focusedRow)
        {
            if (focusedRow == null)
                return;
            //if (methodsEntities.Entry(focusedRow).State == EntityState.Detached) //попытка удалить новую запись
            //{
            //    Measurings.Remove(focusedRow);

            //    return;
            //}
            //для серверного режима отображения данных
            var delRow = methodsEntities.MEASURING_DATA.Where(p => p.MDA_ID == FocusedRow.MDA_ID).FirstOrDefault();
            if (delRow != null)
                methodsEntities.MEASURING_DATA.Remove(delRow); //удаление из контекста 
            //methodsEntities.MEASURING_DATA.Remove(focusedRow); //удаление из контекста 
            SaveData(null);
            ModeResultClear(selectedMode, false);
            MeasuringsRefresh();
        }
        //Удаление или обнуление всех строк MEASURING_DATA для выбранного режима выбраного вида измерения
        private void DeleteMeasuringDataMode(MEASURING_DATA focusedRow)
        {
            List<MEASURING_DATA> currentMeasurings = Measurings;
            switch (Tag)
            {
                case "E":
                    //currentMeasurings == Measurings, т.е. все строки ПЭМИ
                    //currentMeasurings = new ObservableCollection<MEASURING_DATA>(Measurings.Where(p => p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0));
                    break;
                case "UF":
                    currentMeasurings = Measurings.Where(p => p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFN_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "U0":
                    currentMeasurings = Measurings.Where(p => p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;
                case "Saz":
                    currentMeasurings = Measurings.Where(p => p.MDA_ES_VALUE_IZM != 0).ToList<MEASURING_DATA>();
                    break;

            }
            if (currentMeasurings.Any())
            {
                try
                {
                    backgroundWorkerDelete.RunWorkerAsync(); //асинхронное удаление
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка удаления предыдущих измерений." + e.Message);
                    return;
                }

            }
        }

        //Удаление всех строк строки MEASURING_DATA для выбранного режима
        private void ClearMeasuringDataMode_UF(MEASURING_DATA focusedRow)
        {
            if (Measurings == null || !Measurings.Any())
                return;


            if (focusedRow != null)
            {
                //если есть незавершённый ввод
                if (methodsEntities.Entry(focusedRow).State == EntityState.Detached) //удалить новую запись
                {
                    Measurings.Remove(focusedRow);
                    return;
                }
                methodsEntities.MEASURING_DATA.Remove(focusedRow); //удаление из контекста 
            }
            foreach (var row in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID))
            {
                row.MDA_UF = 0;
                row.MDA_UFCN_VALUE_IZM = 0;
                row.MDA_UFCN_VALUE_IZM_DB = 0;
                row.MDA_UFCN_VALUE_IZM_MKV = 0;
                row.MDA_UFN_VALUE_IZM = 0;
                row.MDA_UFN_VALUE_IZM_DB = 0;
                row.MDA_UFN_VALUE_IZM_MKV = 0;
            }
            //при изменении входных данных удаляем результаты расчёта в режиме
            ModeResultClear(selectedMode, false);
            SaveData(null);
            MeasuringsRefresh();
        }
        private void ClearMeasuringDataMode_U0(MEASURING_DATA focusedRow)
        {
            if (Measurings == null || !Measurings.Any())
                return;


            if (focusedRow != null)
            {
                //если есть незавершённый ввод
                if (methodsEntities.Entry(focusedRow).State == EntityState.Detached) //удалить новую запись
                {
                    Measurings.Remove(focusedRow);
                    return;
                }
                methodsEntities.MEASURING_DATA.Remove(focusedRow); //удаление из контекста 
            }
            foreach (var row in methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID))
            {
                row.MDA_U0 = 0;
                row.MDA_U0CN_VALUE_IZM = 0;
                row.MDA_U0CN_VALUE_IZM_DB = 0;
                row.MDA_U0CN_VALUE_IZM_MKV = 0;
                row.MDA_U0N_VALUE_IZM = 0;
                row.MDA_U0N_VALUE_IZM_DB = 0;
                row.MDA_U0N_VALUE_IZM_MKV = 0;
            }
            //при изменении входных данных удаляем результаты расчёта в режиме
            ModeResultClear(selectedMode, false);
            SaveData(null);
            MeasuringsRefresh();
        }
        public void EditData(Object o)
        {

            //на время сохранения изменений отключаем фоновый поток
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (dbChanged != null && !keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничегог делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            wmu = new WindowMeasuringUpdate();
            wmu.DataContext = this;
            wmuIsEnabled = true;
            IValues = new ObservableCollection<int>(methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == selectedMode.MODE_ID).OrderBy(p => p.MDA_I).Select(p => p.MDA_I).Distinct());
            IValues.OrderBy(p => p);

            EcnValue = 0; EnValue = 0;
            wmu.ShowDialog();
            wmu = null;
            //            if (dbChanged != null && !keySuspendParent) // функция вызывалась с работающим потоком, который был остановлен
            //            {
            //                keySuspend = false;
            //#pragma warning disable CS0618 // Type or member is obsolete
            //                dbChanged.Resume();
            //#pragma warning restore CS0618 // Type or member is obsolete
            //            }
        }
        //перерасчёт сигналов после заданного изменения
        private void Recalculate(Object o)
        {
            if (EcnValue == 0 && EnValue == 0)
            {
                MessageBox.Show("Задайте значения корректировки");
                return;
            }
            //wmuIsEnabled = false;
            //RaisePropertyChanged(() => wmuIsEnabled);
            backgroundWorkerRecalc1.RunWorkerAsync();
        }
        //Сохранение изменений в БД     
        public void SaveData(Object o)
        {
            //на время сохранения изменений отключаем фоновый поток
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (dbChanged != null && !keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничегог делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            using (var transaction = methodsEntities.Database.BeginTransaction())
            {

                try
                {
                    methodsEntities.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                    transaction.Rollback();
                    return;
                }
            }
            if (dbChanged != null && !keySuspendParent) // функция вызывалась с работающим потоком, который был остановлен
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        //Автоформирование RBW
        private double MakeRBW(double f, bool isSolid)
        {
            //f - кГц
            var t = f < 150 ? 0.2 : (f < 30000 ? 9 : ((f < 1000000 || !isSolid) ? 120 : 1000));
            return f < 150 ? 0.2 : (f < 30000 ? 9 : ((f < 1000000 || !isSolid) ? 120 : 1000));
        }

        public void ResultClear()
        {
            if (Modes == null)
                return;
            foreach (var mode in Modes)
            {
                if (selectedMode != null && mode.MODE_ID != selectedMode.MODE_ID)
                    ModeResultClear(mode, false);
            }
            if (selectedMode != null)
                ModeResultClear(selectedMode, false);  //выбранный режим очищается последним, чтобы сформировался правильный Results
            RefreshResults();
        }
        //очистка результатов для режимов диф.спектра, исп-ся для перерасчёта интервалов с контролем кол-ва частот в инт-ле
        public void ResultClearForDS()
        {
            if (Modes == null)
                return;
            foreach (var mode in Modes.Where(p => !p.MODE_IS_SOLID))
            {
                if (selectedMode != null && mode.MODE_ID != selectedMode.MODE_ID)
                    ModeResultClear(mode, false);
            }
            if (selectedMode != null)
                ModeResultClear(selectedMode, false);  //выбранный режим очищается последним, чтобы сформировался правильный Results
            RefreshResults();
        }
        //очистка результатов расчёта режима с сохранением в БД
        public void ModeResultClear(MODE mode, bool isAsinc)

        {
            if (mode == null || mode.RESULT == null || !mode.RESULT.Any())
                return;
            try
            {
                methodsEntities.RESULT.RemoveRange(methodsEntities.RESULT.Where(p => p.RES_MODE_ID == mode.MODE_ID));
                //нельзя брать просто selectedMode.RESULT, т.к. после перезагрузки контекста при удалени больших данных, св-во не обновляется
            }
            catch (Exception ed)
            {
                MessageBox.Show("Ошибка удаления результатов расчёта в режиме.   " + ed.Message);
            }
            SaveData(null);

            if (mode.MODE_ID != selectedMode.MODE_ID)
                return;
            //только для выбранного режима
            Results = new ObservableCollection<RESULT>(
                                   methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == arm_one.ARM_ID));
            filterResults = "RES_MODE_ID = " + mode.MODE_ID.ToString();
            MakeMaxValue();
            //обновим таблицы
            RefreshGcResults?.Invoke();
            RefreshGcResultsScen?.Invoke();
        }
        //обнуление результатов расчёта для выбранного режима,активность кнопки "Расчёт", перекраска результатов других режимов
        //выполняется после изменения параметров режима, влияющих на расчёт
        private void RefreshUI()
        {
            RaisePropertyChanged(() => buttonCalculateEnabled);
            ModeResultClear(selectedMode, false);
        }
        //расчёт

        //тип измерений определяется антенной
        private void Calculate(Object o)
        {
            keyCalculate = false;
            if (!keySuspend) //функция вызвана с работающим фоновым потоком
            {
                keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            //расчёт I и САЗ при отсутствии выполняется в CalculateMode(MODE mode, bool isAsinc)
            //обновление Modes
            RefreshModes(selectedMode.MODE_ID);

            backgroundWorkerCalculate.RunWorkerAsync();
        }
        //расчёт и перерасчёт из окна измерений
        private void CalculateMode(MODE mode)
        {

            if (!keySuspend) //функция вызвана с работающим фоновым потоком
            {
                keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            if (mode.RESULT != null && mode.RESULT.Any())
                ModeResultClear(mode, false);

            backgroundWorkerCalculateMode.RunWorkerAsync();
        }
        private void CalculateMode(MODE mode, bool isAsinc)
        {
            if (!isAsinc)                //выполнение в основном потоке
            {
                keyCalculate = false;    //запрет других действий во время расчёта
                if (!keySuspend) //функция вызвана с работающим фоновым потоком
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Type or member is obsolete
                    keySuspend = true;
                }
            }
            if (!mode.MODE_IS_SOLID && IRemake)
            {
                //проверка допустимого числа частот в интервале
                var iGroups = mode.MEASURING_DATA.GroupBy(p => p.MDA_I)
                        .Select(g => new { i = g.Key, Count = g.Count() });
                foreach (var row in iGroups)
                {
                    if (row.Count > 2) //перерасчёт MDA_I
                    {
                        // Value = "Выполняется перерасчёт I режима - '" + mode.MODE_TYPE.MT_NAME + "'";
                        RefreshIWithContrains(mode); //перерасчёт MDA_I с сохранением рез-в
                        break;
                    }
                }
            }
            RESULT result;
            double E_schuma_stationary = 0;
            double E_schuma_portableDrive = 0;
            double E_schuma_portableCarry = 0;

            double E_schuma_U_portableDrive = 0;
            double E_schuma_U_portableCarry = 0;

            double E_schumaAnt = 0;

            double signal_i, signal_iR1, k_zatuchanija_i;
            double[] signal_i_u;
            double E_schuma_SAZ = 0;

            double NORMA_2 = 0, NORMA_3 = 0;

            //расчёт по электрическому полю
            //все данные измерений режима. Создаётся локально, чтобы не вызывал принудительную чистку памяти
            ObservableCollection<MEASURING_DATA> measurings = new ObservableCollection<MEASURING_DATA>(
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_E != 0));
            if (measurings == null || !measurings.Any()) //отсутствуют данные измерений
                return;

            if (armGS != null && armGS != 0 && AntennasGS.Where(p => p.ANT_ID == armGS && p.ANT_MODEL != "Без ГШ").Any())
            {
                //расчёт с ГШ
                if (!measurings.Where(p => p.MDA_ES_VALUE_IZM_MKV != 0).Any()) //не заполнены значения шума от ГШ 
                {
                    string ValuePrev = Value;
                    Value = "Выполняется генерация данных САЗ для режима - '" + mode.MODE_TYPE.MT_NAME + "'";
                    GenerateSAZ(mode, true); //без обновления таблиц измерений
                    Value = ValuePrev;
                    measurings = new ObservableCollection<MEASURING_DATA>(methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID));
                    if (!isAsinc)                //выполнение в основном потоке
                    {
                        RefreshGcE?.Invoke();
                        RefreshGcCollection?.Invoke();
                    }
                }
            }

            if (mode.MODE_TAU_UNIT_ID == 0 || mode.MODE_FT_UNIT_ID == 0 ||
            mode.MODE_TAU == 0 || mode.MODE_FT == 0 || mode.MODE_D == 0 ||
            mode.MODE_R == 0)
            {
                MessageBox.Show("Не все параметры режима " + mode.MODE_TYPE.MT_NAME +
                                " заполнены. Расчёт для этого режима не может быть выполнен.");
                return;
            }
            //расчёт нормы для заданной(текущей) категории выбранного режима
            mode.MODE_NORMA = Functions.Sigma(mode, arm_one);
            if (mode.MODE_NORMA <= 0)
            {
                MessageBox.Show("Не удалось определить нормированное значение сигмы. Проверьте правильность заполнения параметров типа СВТ");
                if (keySuspend)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    keySuspend = false;
                }
                keyCalculate = true;
                RaisePropertyChanged(() => buttonCalculateEnabled);
                return;
            }
            //нормы для 2-3 категорий выбранного режима
            NORMA_2 = Functions.Sigma_1_2_3(mode, arm_one, 2);
            if (NORMA_2 == -1)
                return;
            NORMA_3 = Functions.Sigma_1_2_3(mode, arm_one, 3);
            if (NORMA_3 == -1)
                return;
            double alpha = mode.MODE_RMAX != 0 ? mode.MODE_RMAX : 1; //коэфф. альфа для расчёта дельты
            //величины, приведённые к кГц и нсек
            double tau = Functions.Tau_nsek(mode.MODE_TAU,
                         Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID));
            double ft = Functions.F_kGc(mode.MODE_FT,
                        Functions.GetUnitValue(Units, (int)mode.MODE_FT_UNIT_ID));
            //полоса пропускания, кГц для SAZ
            double rbw = Functions.F_kGc(mode.MODE_RBW,
                Functions.GetUnitValue(Units, (int)mode.MODE_RBW_UNIT_ID));
            result = new RESULT();
            int[] arR2 = new int[3], arR2_2 = new int[3], arR2_3 = new int[3];
            int R2 = 0, R2_2 = 0, R2_3 = 0; //для САЗ
            double R1 = 0, R1_2 = 0, R1_3 = 0;
            bool existDataMeerAs100DB = measurings.Where(p => p.MDA_ECN_VALUE_IZM_DB >= 100).Any() && mode.MODE_IS_SOLID; //есть измерения сигнал+шум >= 100 дБ для сплошного спектра
            bool keyInclude = true; //включать в расчёт измерения > 100 дБ
            if (existDataMeerAs100DB)
                if (DialogResult.No == MessageBox.Show("Имеются измерения Сигнал+Шум >= 100 дБ.Включить их в обработку?", "", MessageBoxButtons.YesNo))
                    keyInclude = false;
                else
                    keyInclude = true;

            for (int j = 0; j <= 1; j++)
            {
                //все значения i для E или H
                IEnumerable<int> iList;
                IEnumerable<MEASURING_DATA> list, listR1, listSAZ;
                string resType;
                string antType;
                bool isE;

                if (j == 0) //эл.поле
                {
                    resType = "E";
                    isE = true;
                    antType = "Электр";
                }
                else
                {
                    resType = "H";
                    isE = false;
                    antType = "Магнит";
                }
                //если не сортировать, то I выбирается не правльно
                if (keyInclude)
                {
                    iList = measurings.OrderBy(p => p.MDA_I).Where(d => d.MDA_F != 0 && d.MDA_E != 0 && d.MDA_TYPE == resType).Select(p => p.MDA_I).Distinct();
                    //iList = measurings.Where(d => d.MDA_F != 0 && d.MDA_E != 0 && d.MDA_TYPE == resType).OrderBy(a => a.MDA_I).Select(k => k.MDA_I).Distinct();
                }
                else
                    iList = measurings.OrderBy(p => p.MDA_I).Where(d => d.MDA_F != 0 && d.MDA_E != 0 && d.MDA_TYPE == resType && d.MDA_ECN_VALUE_IZM_DB < 100).Select(p => p.MDA_I).Distinct();
                if (iList.Contains(0))//есть строки измерений без интервалов
                {
                    RefreshI(mode);
                }
                //расчёт показателя защищённости для каждого интервала i            
                foreach (int i in iList)
                {
                    if (keyInclude)
                        list = measurings.Where(p => p.MDA_I == i && p.MDA_TYPE == resType); //данные измерений в интервале
                    else
                        list = measurings.Where(p => p.MDA_I == i && p.MDA_TYPE == resType && p.MDA_ECN_VALUE_IZM_DB < 100);

                    if (keyInclude)
                        listR1 = isE ? list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 400000) :
                            list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 30000);
                    else
                        listR1 = isE ? list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 400000 && p.MDA_ECN_VALUE_IZM_DB < 100) :
                            list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 30000 && p.MDA_ECN_VALUE_IZM_DB < 100);

                    if (mode.MODE_IS_SOLID)
                    {
                        //signal_i = Functions.E_c_solid(list.Where(p => p.MDA_E != 0), Units);
                        signal_i = Functions.E_c_solid(list, Units); //надо брать все значения. 0-е не будут учитываться, первые и последние значения в лепестке будут браться правильно
                        signal_i_u = Functions.E_c_solid_u(list.Where(p => p.MDA_UF != 0 || p.MDA_U0 != 0), Units);//строки с наводками
                    }
                    else
                    {
                        signal_i = Functions.E_c_diff(list); //строки ПЭМИ
                        signal_i_u = Functions.E_c_diff_u(list.Where(p => p.MDA_UF != 0 || p.MDA_U0 != 0)); //строки с наводками
                    }
                    if (signal_i == 0 && signal_i_u[0] == 0 && signal_i_u[1] == 0)
                        continue;
                    if (mode.MODE_IS_SOLID)
                        signal_iR1 = Functions.E_c_solid(listR1, Units);
                    else
                        signal_iR1 = Functions.E_c_diff(listR1);

                    k_zatuchanija_i = Functions.K_zatuchanija_i(mode.MODE_D, mode.MODE_R, i, tau);

                    if (k_zatuchanija_i == 0)
                        continue;

                    //Если выбран ГШ, то E_schuma_stationary = E_schuma_SAZ
                    if (armGS != null && armGS != 0 && AntennasGS.Where(p => p.ANT_ID == armGS && p.ANT_MODEL != "Без ГШ").Any())
                    {
                        listSAZ = measurings.Where(p => p.MDA_I == i && p.MDA_ES_VALUE_IZM_MKV != 0);
                        E_schuma_SAZ = Functions.E_schumaSAZ_New(i, tau, listSAZ);
                        if (E_schuma_SAZ != 0)
                        {
                            R2 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_SAZ, mode.MODE_NORMA, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);
                            R2_2 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_SAZ, NORMA_2, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);
                            R2_3 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_SAZ, NORMA_3, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);
                        }
                    }
                    else
                    {
                        //нормированная величина напряжённости шума(знаменатель формулы), используется и для R2   
                        E_schuma_stationary = Functions.E_schuma_Num(i, tau, "Стационарное", armKategoria);
                        E_schuma_portableDrive = Functions.E_schuma_Num(i, tau, "Портативное возимое", armKategoria);
                        E_schuma_portableCarry = Functions.E_schuma_Num(i, tau, "Портативное носимое", armKategoria);
                        //R2 для текущей,2,3 категории
                        arR2 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                            E_schuma_portableCarry, mode.MODE_NORMA, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);

                        arR2_2 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                                E_schuma_portableCarry, NORMA_2, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);

                        arR2_3 = Functions.R2(signal_i * alpha, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                                E_schuma_portableCarry, NORMA_3, i, mode.MODE_D, tau, mode.MODE_IS_SOLID);

                    }
                    E_schuma_U_portableDrive = Functions.E_schuma_Num_U(i, tau, "Портативное возимое");
                    E_schuma_U_portableCarry = Functions.E_schuma_Num_U(i, tau, "Портативное носимое");

                    //расчёт зон R1 
                    //предельная интервальная чувствительность СА по эл. и магн. полям для расчёта R1 (приложение Д)
                    E_schumaAnt = Functions.EH_schumaAnt_i(i, tau, isE); //шаг инт-я = 1кГц
                    //R1 для текущей,1,2,3 категории
                    if (E_schumaAnt != 0)
                    {
                        R1 = Functions.R1(signal_iR1 * alpha, (double)mode.MODE_TYPE.MT_KN, E_schumaAnt, mode.MODE_NORMA, i, mode.MODE_D, tau, mode.MODE_IS_SOLID, isE);
                        // R1_1 = Functions.R1(signal_iR1 * alpha, (double)mode.MODE_TYPE.MT_KN, E_schumaAnt, NORMA_1, i, mode.MODE_D, tau, mode.MODE_IS_SOLID, isE);
                        R1_2 = Functions.R1(signal_iR1 * alpha, (double)mode.MODE_TYPE.MT_KN, E_schumaAnt, NORMA_2, i, mode.MODE_D, tau, mode.MODE_IS_SOLID, isE);
                        R1_3 = Functions.R1(signal_iR1 * alpha, (double)mode.MODE_TYPE.MT_KN, E_schumaAnt, NORMA_3, i, mode.MODE_D, tau, mode.MODE_IS_SOLID, isE);
                    }
                    else
                    {
                        R1 = 0;
                        // R1_1 = 0;
                        R1_2 = 0;
                        R1_3 = 0;
                    }
                    var deltaSAZ = E_schuma_SAZ == 0 ? 0
                            : Math.Round(signal_i * alpha / (double)mode.MODE_TYPE.MT_KN / mode.MODE_KS / E_schuma_SAZ, 6);
                    result = new RESULT()
                    {
                        RES_MODE_ID = mode.MODE_ID,
                        RES_TYPE = resType,
                        RES_NORMA = mode.MODE_NORMA,
                        RES_NORMA_2 = NORMA_2,
                        RES_NORMA_3 = NORMA_3,
                        RES_I_ZATUCHANIJA = k_zatuchanija_i,
                        RES_I = i,
                        RES_SIGNAL = Math.Round(signal_i, 3),
                        RES_DELTA_PORTABLE = E_schuma_stationary != 0 ? Math.Round(signal_i * alpha / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_stationary, 3) : 0,

                        RES_DELTA_PORTABLE_CARRY =
                            E_schuma_portableCarry != 0
                                ? Math.Round(signal_i * alpha / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_portableCarry, 3)
                                : 0,

                        RES_DELTA_PORTABLE_DRIVE =
                            E_schuma_portableDrive != 0
                                ? Math.Round(signal_i * alpha / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_portableDrive, 3)
                                : 0,

                        RES_SAZ = E_schuma_SAZ == 0
                            ? 0
                            : Math.Round(signal_i * alpha / (double)mode.MODE_TYPE.MT_KN / mode.MODE_KS / E_schuma_SAZ, 6),
                        RES_R2_PORTABLE = arR2[0],
                        RES_R2_PORTABLE_DRIVE = arR2[1],
                        RES_R2_PORTABLE_CARRY = arR2[2],
                        RES_R2_PORTABLE_1 = deltaSAZ == 0 ? 0 : Math.Round(mode.MODE_NORMA / deltaSAZ, 2),
                        RES_R2_PORTABLE_DRIVE_1 = deltaSAZ == 0 ? 0 : Math.Round(NORMA_2 / deltaSAZ, 2),
                        RES_R2_PORTABLE_CARRY_1 = deltaSAZ == 0 ? 0 : Math.Round(NORMA_3 / deltaSAZ, 2),

                        RES_R2_PORTABLE_2 = arR2_2[0],
                        RES_R2_PORTABLE_DRIVE_2 = arR2_2[1],
                        RES_R2_PORTABLE_CARRY_2 = arR2_2[2],
                        RES_R2_PORTABLE_3 = arR2_3[0],
                        RES_R2_PORTABLE_DRIVE_3 = arR2_3[1],
                        RES_R2_PORTABLE_CARRY_3 = arR2_3[2],
                        RES_DELTA_PORTABLE_DRIVE_UF = E_schuma_U_portableDrive != 0 ? Math.Round(alpha * signal_i_u[0] / (double)mode.MODE_TYPE.MT_KN / E_schuma_U_portableDrive, 3) : 0,
                        RES_DELTA_PORTABLE_DRIVE_U0 = E_schuma_U_portableDrive != 0 ? Math.Round(alpha * signal_i_u[1] / (double)mode.MODE_TYPE.MT_KN / E_schuma_U_portableDrive, 3) : 0,
                        RES_DELTA_PORTABLE_CARRY_UF = E_schuma_U_portableCarry != 0 ? Math.Round(alpha * signal_i_u[0] / (double)mode.MODE_TYPE.MT_KN / E_schuma_U_portableCarry, 3) : 0,
                        RES_DELTA_PORTABLE_CARRY_U0 = E_schuma_U_portableCarry != 0 ? Math.Round(alpha * signal_i_u[1] / (double)mode.MODE_TYPE.MT_KN / E_schuma_U_portableCarry, 3) : 0,

                        RES_R1_SOSR = 0,
                        RES_R1_SOSR_2 = 0,
                        RES_R1_SOSR_3 = 0,


                    };
                    if (R1 != 0)
                        result.RES_R1_SOSR = Math.Round(R1, 1);
                    if (R1_2 != 0)
                        result.RES_R1_SOSR_2 = Math.Round(R1_2, 1);
                    if (R1_3 != 0)
                        result.RES_R1_SOSR_3 = Math.Round(R1_3, 1);
                    result.RES_DRIVE_KF = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_UF / mode.MODE_NORMA);
                    result.RES_DRIVE_K0 = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_U0 / mode.MODE_NORMA);
                    result.RES_CARRY_KF = Math.Round(result.RES_DELTA_PORTABLE_CARRY_UF / mode.MODE_NORMA);
                    result.RES_CARRY_K0 = Math.Round(result.RES_DELTA_PORTABLE_CARRY_U0 / mode.MODE_NORMA);

                    result.RES_DRIVE_KF_2 = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_UF / NORMA_2);
                    result.RES_DRIVE_KF_3 = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_UF / NORMA_3);
                    result.RES_CARRY_KF_2 = Math.Round(result.RES_DELTA_PORTABLE_CARRY_UF / NORMA_2);
                    result.RES_CARRY_KF_3 = Math.Round(result.RES_DELTA_PORTABLE_CARRY_UF / NORMA_3);

                    result.RES_DRIVE_K0_2 = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_U0 / NORMA_2);
                    result.RES_DRIVE_K0_3 = Math.Round(result.RES_DELTA_PORTABLE_DRIVE_U0 / NORMA_3);
                    result.RES_CARRY_K0_2 = Math.Round(result.RES_DELTA_PORTABLE_CARRY_U0 / NORMA_2);
                    result.RES_CARRY_K0_3 = Math.Round(result.RES_DELTA_PORTABLE_CARRY_U0 / NORMA_3);

                    methodsEntities.RESULT.Add(result);
                }

            }
            mode.MODE_R2 = arR2_2[1];
            SaveData(null);
            if (isAsinc) //асинхронный расчёт всех режимов
                return;
            MakeMaxValue();
            Results = new ObservableCollection<RESULT>(methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == arm_one.ARM_ID));
            //обновим таблицы
            RefreshGcResults?.Invoke();
            RefreshGcResultsScen?.Invoke();
            if (!Results.Any())
            {
                MessageBox.Show("Проверьте справочник антенн. Во всех строках должен быть указан правильный тип: Электрическая или Магнитная");
            }
            RefreshGcE?.Invoke();
            RefreshGcCollection?.Invoke();

            if (keySuspend)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                keySuspend = false;
            }
            keyCalculate = true;
            RaisePropertyChanged(() => buttonCalculateEnabled);
        }

        //генерация САЗ в арм. Выполняется по запросу при смене САЗ или по умолчанию при расчёте
        void GenerateSAZ()
        {
            if (arm_one == null || arm_one.MODE == null)
                return;
            if (!keySuspend) //функция вызвана с работающим фоновым потоком
            {
                keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            ResultClear(); //очистка результатов всех режимов  АРМ            
            methodsEntities.Configuration.AutoDetectChangesEnabled = false;
            backgroundWorkerGenerateSAZ.RunWorkerAsync();
        }
        //генерация САЗ в режиме. Выполняется по запросу при смене ГШ для всех режимов или при расчёте режима только для рассчитываемого режима
        void GenerateSAZ(MODE mode, bool isAsinc)
        {
            if (mode.MEASURING_DATA == null || !mode.MEASURING_DATA.Where(p => p.MDA_I != 0 && (p.MDA_E != 0 || p.MDA_UF != 0 || p.MDA_U0 != 0)).Any()) //нет измерений в режиме
                return;
            var k = AntennasGS.Where(p => p.ANT_ID == armGS && p.ANTENNA_CALIBRATION != null).FirstOrDefault().ANT_ERROR;
            mode.MODE_KS = k != 0 ? k : 0.8;
            var t = methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID && p.MDA_I != 0 && (p.MDA_E != 0 || p.MDA_UF != 0 || p.MDA_U0 != 0)).Select(p => p.MDA_I).Distinct();
            foreach (int i in t)
            {
                GenerateSAZ(i, mode);
            }
        }
        //генерация САЗ в лепестке режима
        void GenerateSAZ(int i, MODE mode)
        {
            var valuesGS = AntennasGS.Where(p => p.ANT_ID == armGS && p.ANTENNA_CALIBRATION != null).FirstOrDefault().ANTENNA_CALIBRATION;
            if (!valuesGS.Any())
            {
                MessageBox.Show("В справочнике ГШ отсутствует таблица значений шумов.");
                return;
            }
            //коэффициент шума

            double tau = Functions.Tau_nsek(mode.MODE_TAU,
                        Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID));
            double f_law, f_top;  //мГц границы лепестка, к котором генерируем шум.
            f_law = i == 1 ? 0.3 : Math.Round((i - 1) * 1000 / tau, 0);
            f_top = i * 1000 / tau < 10000 ? Math.Round(i * 1000 / tau, 0) : 60000;

            double f = f_law;
            double step = 0; //шаг частоты, на котором генерируется шум. Зависит от 
            //int antId = 0;
            //ANTENNA ant = new ANTENNA();
            int fUnitID = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "мГц").FirstOrDefault().UNIT_ID
                        : 0;
            int RBWUnitId = methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц") != null
                        ? methodsEntities.UNIT.Where(p => p.UNIT_VALUE == "кГц").FirstOrDefault().UNIT_ID
                        : 0;
            List<MEASURING_DATA> coll = new List<MEASURING_DATA>();
            while (f < f_top)
            {
                if (!valuesGS.Where(p => p.ANT_CAL_F <= f).Any())
                {
                    MessageBox.Show("В справочнике ГШ отсутствуют данные в таблице шумов.");
                    return;
                }
                double valueF = valuesGS.Where(p => p.ANT_CAL_F <= f).Max<ANTENNA_CALIBRATION>(p => p.ANT_CAL_F); //значение в таблице для частоты <= текущей
                double value = valuesGS.Where(p => p.ANT_CAL_F == valueF).FirstOrDefault().ANT_CAL_VALUE; //значение в таблице для частоты <= текущей
                if (value == 0)
                    continue;
                var md = mode.MEASURING_DATA.Where(p => p.MDA_F == f).FirstOrDefault();
                if (md == null)
                {
                    md = new MEASURING_DATA()
                    {
                        MDA_F = f,
                        MDA_I = i,
                        MDA_MODE_ID = mode.MODE_ID,
                        MDA_F_UNIT_ID = fUnitID,
                        MDA_RBW_UNIT_ID = RBWUnitId,
                        MDA_TYPE = "E"
                    };
                    //if (antId == 0)
                    //{
                    //    IT(md);//антенна + коэф.кал
                    //    antId = (int)md.MDA_ANT_ID;
                    //    ant = md.ANTENNA;
                    //}
                    //else
                    //{
                    //    md.MDA_ANT_ID = antId;
                    //    md.ANTENNA = ant;
                    //    RefreshKa(md);
                    //}
                    RefreshKa(md);
                    //добавление остальных реквизитов в строку измерений
                    if (AutoRBW == true)
                        MakeRBW(md);
                }
                md.MDA_ES_VALUE_IZM = value;
                md.MDA_ES_VALUE_IZM_DB = value;
                md.MDA_ES_VALUE_IZM_MKV = Math.Round(Math.Pow(10, value / 20), 3);
                RefreshKps(md);
                coll.Add(md);
                //если существует измерение на частоте f, то дополняем строку полученным значением, инече генерируем новую строку измерений
                if (f >= 0.3 && f < 3)
                    step = 0.2;
                else
                {
                    if (f >= 3 && f < 300)
                        step = 1;
                    else
                    {
                        if (f >= 300 && f < 1000)
                            step = 5;
                        else
                            step = 20;
                    }
                }
                f += step;
            }
            //Value = "Подготовлено для сохраниения " + coll.Count().ToString() + " строк.";
            //добавление новых строк в контекст
            try
            {
                Update.CopyAndMerge(coll);
            }
            catch (Exception eM)
            {
                MessageBox.Show("Ошибка сохранения сгенерированных данных. " + eM.Message);
            }
        }
        //очистка САЗ для всех режимов текущего АРМ. Выполняется при смене(отмене ГШ)
        public void ClearSAZ()
        {
            if (arm_one == null || arm_one.MODE == null)
                return;
            if (!keySuspend) //функция вызвана с работающим фоновым потоком
            {
                keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            ResultClear(); //очистка результатов всех режимов  АРМ
            methodsEntities.Configuration.AutoDetectChangesEnabled = false;
            backgroundWorkerClearSAZ.RunWorkerAsync();
        }
        public void ClearSAZ(MODE mode)
        {
            if (mode.MEASURING_DATA == null || !mode.MEASURING_DATA.Any()) //нет измерений в режиме
                return;
            bool ChangesEnabledPrev = methodsEntities.Configuration.AutoDetectChangesEnabled;

            methodsEntities.Configuration.AutoDetectChangesEnabled = true;
            //обнуляем измерения САЗ в строках, в которых есть другие измерения
            foreach (var md in mode.MEASURING_DATA.Where(p => p.MDA_ES_VALUE_IZM_MKV != 0 &&
                                                              (p.MDA_ECN_VALUE_IZM != 0 || p.MDA_EN_VALUE_IZM != 0 ||
                                                               p.MDA_UFCN_VALUE_IZM != 0 || p.MDA_UFCN_VALUE_IZM != 0 ||
                                                               p.MDA_U0CN_VALUE_IZM != 0 || p.MDA_U0N_VALUE_IZM != 0)))
            {
                md.MDA_ES_VALUE_IZM = 0;
                md.MDA_ES_VALUE_IZM_DB = 0;
                md.MDA_ES_VALUE_IZM_MKV = 0;
                md.MDA_KPS = 0;

            }
            //удаляем строки измерений для которых есть только измерения САЗ
            var range = mode.MEASURING_DATA.Where(p => p.MDA_ES_VALUE_IZM_MKV != 0 &&
                                                       p.MDA_ECN_VALUE_IZM == 0 && p.MDA_EN_VALUE_IZM == 0 &&
                                                       p.MDA_UFCN_VALUE_IZM == 0 && p.MDA_UFCN_VALUE_IZM == 0 &&
                                                       p.MDA_U0CN_VALUE_IZM == 0 && p.MDA_U0N_VALUE_IZM == 0);
            if (range != null && range.Any()) //есть, что удалять
                methodsEntities.MEASURING_DATA.RemoveRange(range);
            SaveData(null);
            methodsEntities.Configuration.AutoDetectChangesEnabled = ChangesEnabledPrev;
        }
        //контроль правильности наличия меток переменных полей отчётов в БД    
        private void FirstReportLabelValue(String reportName)
        {
            List<string> v = xr.AllControls<XRLabel>().Where(p => p.Tag.ToString() == "Var").Select(p => p.Name.Trim()).ToList<string>(); //настраиваемые поля в отчёте
            List<string> vDB = methodsEntities.REPORT_DATA.Where(p => p.RD_ANL_ID == anl_id && p.RD_REPORT == reportName).Select(p => p.RD_LABEL.Trim()).Distinct().ToList<string>();//настраиваемые поля в БД
            //проверим и очистим БД от неактуальных полей
            if (vDB.Count() > 0)
            {
                using (var transaction = methodsEntities.Database.BeginTransaction())
                {

                    try
                    {
                        foreach (string labelDB in vDB)
                        {
                            if (!v.Where(p => p == labelDB).Any())
                            {
                                //нет для такой метки в отчёте удалим все её строки  из БД
                                foreach (var rowForRemove in methodsEntities.REPORT_DATA.Where(p => p.RD_ANL_ID == anl_id && p.RD_REPORT == reportName && p.RD_LABEL == labelDB))
                                {
                                    methodsEntities.REPORT_DATA.Remove(rowForRemove);
                                }
                            }
                        }
                        methodsEntities.SaveChanges(); //сохраненим изменения
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка очистки неактуальных настраиваемых полей в БД" + ". " + e.Message);
                        transaction.Rollback();
                        return;
                    }
                }
            }
            foreach (var label in xr.AllControls<XRLabel>().Where(p => p.Tag.ToString() == "Var"))
            {

                //для каждого поля отчёта с переменной информацией ищем значение по умолчанию в БД
                REPORT_DATA temp =
                    methodsEntities.REPORT_DATA.Where(
                        p => p.RD_ANL_ID == anl_id && p.RD_LABEL == label.Name && p.RD_DEFAULT == true && p.RD_REPORT == reportName)
                        .FirstOrDefault();
                if (temp == null)
                    temp = methodsEntities.REPORT_DATA.Where(
                       p => p.RD_ANL_ID == anl_id && p.RD_LABEL == label.Name && p.RD_REPORT == reportName).FirstOrDefault();
                if (temp == null && !String.IsNullOrEmpty(label.Text))
                //в БД вообще нет значения для этой метки, добавим из шаблона отчёта
                {
                    methodsEntities.REPORT_DATA.Add(new REPORT_DATA()
                    {
                        RD_ANL_ID = anl_id,
                        RD_DEFAULT = true,
                        RD_DESCRIPTION = "Значение из шаблона",
                        RD_REPORT = reportName,
                        RD_LABEL = label.Name.Trim(),
                        RD_TEXT = label.Text
                    }
                        );
                }
                if (methodsEntities.ChangeTracker.Entries().Where(p => p.State == EntityState.Modified || p.State == EntityState.Added || p.State == EntityState.Deleted).Count() != 0)
                    SaveData(null);//сохраненим изменения
            }
        }
        void PrepareDataForReport()
        {
            SaveData(null);

            //перегрузка контекста, чтобы обновились все изменения в БД(удаление Linq .Delete())
            MethodsEntities methodsEntities_ = new MethodsEntities();
            xr = null;
            int npp = 0;
            MakeMaxValue(); //опеределение макс.R2 и R1 для всех категорий из рассчитанных для всех интервалов и режимов            
            var temp = methodsEntities_.MODE.Where(p => p.MODE_ARM_ID == arm_id && p.RESULT.Count != 0);

            ModesForReport = new ObservableCollection<MODE_NPP>();
            if (temp.Any()) //пронумерованные режимы
            {
                string antType = String.Empty;
                foreach (var mo_ in temp)
                {
                    //в обновлённом контексте все данные актуальны, нужно только отсортировать ручками навигационные свойства
                    //для сортировки данных измерений в свойствах навигации MEASURING_DATA и загрузки MODE_TYPE

                    MODE mo = methodsEntities_.MODE.Where(p => p.MODE_ID == mo_.MODE_ID).FirstOrDefault();
                    methodsEntities_.Entry(mo).Collection(e => e.MEASURING_DATA)
                         .Query()
                         .OrderBy(c => c.MDA_F)
                         .Load();
                    methodsEntities_.Entry(mo).Reference(e => e.MODE_TYPE)
                        .Query()
                        .Load();
                    npp += 1;
                    var t = Antennas.Where(p => p.ANT_ID == mo.MEASURING_DATA.FirstOrDefault().MDA_ANT_ID).FirstOrDefault();
                    if (t != null && t.ANT_TYPE != null)
                        antType = Antennas.Where(p => p.ANT_ID == mo.MEASURING_DATA.FirstOrDefault().MDA_ANT_ID).FirstOrDefault().ANT_TYPE;

                    ModesForReport.Add(new MODE_NPP()
                    {
                        NPP = npp.ToString(),
                        AntennaType = antType.ToLower().Contains("эл") ? "Электрическое" :
                        (antType.ToLower().Contains("маг") ? "Магнитное" :
                        (antType.ToLower().Contains("фаза") ? "Эквивалент сети, фаза" : "Эквивалент сети, ноль")),
                        mode = mo
                    });

                }
            }

            MakeEQ(); //список оборудования

        }
        public void ReportWord(Object o)
        {
            keySuspend = true; //фоновый поток приостановили. Во всех вложеных ф-ях с потоком ничего не делаем
#pragma warning disable CS0618 // Type or member is obsolete
            dbChanged.Suspend();//остановим фоновый поток, т.к. даные измерений в реальном времени другим пользователям не нужны
#pragma warning restore CS0618 // Type or member is obsolete

            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = Properties.Settings.Default.pathTemplateWord,
                RestoreDirectory = false
            };
            ofd.Filter = "Документы WORD (*.docx)|*.docx|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "Выберите файл-шаблон отчёта";

            if (ofd.ShowDialog() == DialogResult.Cancel)
            {

                if (keySuspend)
                {
                    keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                }
                return;
            }
            PrepareDataForReport();
            MakeAllIP();

            string filePath = ofd.FileName;//шаблон

            if (String.IsNullOrEmpty(filePath))
                return;
            //директория выбранного шаблона.
            int index = ofd.FileName.LastIndexOfAny("\\".ToCharArray());
            Properties.Settings.Default.pathTemplateWord = ofd.FileName.Substring(0, index + 1);
            Properties.Settings.Default.Save();

            using (DocX doc = DocX.Create(@"c:\Temp\Test.docx"))
            {

                try
                {
                    doc.ApplyTemplate(filePath, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return;
                }
                foreach (var table in doc.Tables)
                {
                    foreach (var paragraph in table.Paragraphs)
                    {
                        var boommarks = paragraph.GetBookmarks();
                        if (boommarks != null && boommarks.Count() > 0)
                        {
                            foreach (var bm in boommarks)
                            {
                                switch (bm.Name)
                                {
                                    case "Таблица_1":
                                        //функция заполнения таблицы "Оборудование"
                                        Table_1_Make(table);
                                        break;
                                    case "Таблица_2":
                                        //функция заполнения таблицы "Изм.инструменты"
                                        Table_2_Make(table);
                                        break;
                                    case "Таблица_3":
                                        Table_3_Make(table);
                                        break;
                                    case "Таблица_4":
                                        Table_4_Make(table);
                                        break;
                                    case "Таблица_5":
                                        Table_5_Make(table);
                                        break;
                                    case "Таблица_2_pred":
                                        Table_2_pred_Make(table);
                                        break;
                                    case "Таблица_3_pred":
                                        Table_3_pred_Make(table);
                                        break;
                                }
                            }
                        }
                    }

                }
                SaveFileDialog sfd = new SaveFileDialog();
                if (Properties.Settings.Default.pathReportWord != String.Empty)
                    sfd.InitialDirectory = Properties.Settings.Default.pathReportWord;
                sfd.Filter = "Документы WORD (*.docx)|*.docx|All files (*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.DefaultExt = ".docx";
                sfd.AddExtension = true;
                sfd.Title = "Заполните имя файла для сохранения отчёта";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        doc.SaveAs(sfd.FileName);
                        //сохранение
                        index = sfd.FileName.LastIndexOfAny("\\".ToCharArray());
                        Properties.Settings.Default.pathReportWord = sfd.FileName.Substring(0, index + 1);
                        Properties.Settings.Default.Save();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }

                    Process p = new Process();
                    p.StartInfo.FileName = sfd.FileName;
                    p.Start();

                }

            }
            if (keySuspend)
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        void Table_1_Make(Table table)
        {
            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            var size = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.Size;
            if (size != null)
                formatting.Size = (double)size;
            //текущая строка
            Row rowNew;
            //отформатированная строка данных, сформированная из строки заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewData = table.InsertRow(table.Rows[0], true);
            int index = table.Rows.Count() - 1;
            foreach (var cell in rowNewData.Cells)
            {
                cell.FillColor = Color.White;
                foreach (var p in cell.Paragraphs)
                    p.ReplaceText(p.Text, "");
            }

            foreach (var rowEQ in eqForReport)
            {
                rowNew = table.InsertRow(rowNewData, false);
                rowNew.Cells[0].Paragraphs[0].InsertText(rowEQ.NPP + ".", false, formatting);
                rowNew.Cells[1].Paragraphs[0].InsertText(rowEQ.Name, false, formatting);
                rowNew.Cells[2].Paragraphs[0].InsertText(rowEQ.Model, false, formatting);
                rowNew.Cells[3].Paragraphs[0].InsertText(rowEQ.Serial, false, formatting);
                rowNew.Cells[4].Paragraphs[0].InsertText(String.Empty, false, formatting);
            }
            //удаление строки-шаблона
            table.Rows[index].Remove();
        }
        void Table_2_Make(Table table)
        {

            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                //параметры форматирования первой строки заголовка таблицы
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            var size = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.Size;
            if (size != null)
                formatting.Size = (double)size;
            //текущая строка
            Row rowNew;
            //отформатированная строка данных, сформированная из строки заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewData = table.InsertRow(table.Rows[0], true);
            int index = table.Rows.Count() - 1;
            foreach (var cell in rowNewData.Cells)
            {
                cell.FillColor = Color.White;
                foreach (var p in cell.Paragraphs)
                    p.ReplaceText(p.Text, "");
            }
            //отформатированная строка заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewHeader = table.InsertRow(table.Rows[0], true);
            rowNewHeader.MergeCells(0, rowNewHeader.Cells.Count - 1);
            foreach (var p in rowNewHeader.Cells[0].Paragraphs)
                p.ReplaceText(p.Text, "");
            rowNewHeader.Cells[0].FillColor = Color.White;

            rowNew = table.InsertRow(rowNewHeader, true);
            rowNew.Cells[0].Paragraphs[0].InsertText("Измерительное оборудование", false, formatting);
            //rowNew.Cells[0].FillColor = Color.White;

            formatting.Bold = false;

            //измерительное оборудование
            foreach (var rowIP in AllIP)
            {
                rowNew = table.InsertRow(rowNewData, false);
                rowNew.Cells[0].Paragraphs[0].InsertText(rowIP.NPP, false, formatting);
                rowNew.Cells[1].Paragraphs[0].InsertText(rowIP.Type, false, formatting);
                rowNew.Cells[2].Paragraphs[0].InsertText(rowIP.Model, false, formatting);
                rowNew.Cells[3].Paragraphs[0].InsertText(rowIP.WorkNumber, false, formatting);
                rowNew.Cells[4].Paragraphs[0].InsertText(rowIP.DiapasonF, false, formatting);
                rowNew.Cells[5].Paragraphs[0].InsertText(rowIP.Date, false, formatting);
            }
            rowNew = table.InsertRow(rowNewHeader, true);
            rowNew.Cells[0].Paragraphs[0].InsertText("Вспомогательное оборудование", false, formatting);

            var allIPHelperEmptyDate = AllIPHelper.Where(p => p.Date.Contains("begin") || p.Date.Contains("end"));
            int npp = AllIP.Count();
            foreach (var rowIP in allIPHelperEmptyDate)
            {
                rowNew = table.InsertRow(rowNewData, true);
                //старое содержимое сохраняется
                npp++;
                rowNew.Cells[0].Paragraphs[0].InsertText(npp.ToString() + ".", false, formatting);
                rowNew.Cells[1].Paragraphs[0].InsertText(rowIP.Type, false, formatting);
                rowNew.Cells[2].Paragraphs[0].InsertText(rowIP.Model, false, formatting);
                rowNew.Cells[3].Paragraphs[0].InsertText(rowIP.WorkNumber, false, formatting);
                rowNew.Cells[4].Paragraphs[0].InsertText(rowIP.DiapasonF, false, formatting);
                rowNew.Cells[5].Paragraphs[0].InsertText(String.Empty, false, formatting);
            }
            //объединение ячеек в столбце
            table.MergeCellsInColumn(5, table.RowCount - allIPHelperEmptyDate.Count(), table.RowCount - 1);
            table.Rows[table.RowCount - allIPHelperEmptyDate.Count()].Cells[5].Paragraphs[0].InsertText("Поверка и калибровка метрологической службой не требуется", false, formatting);

            var allIPHelperWithDate = AllIPHelper.Where(p => !p.Date.Contains("begin") && !p.Date.Contains("end"));
            foreach (var rowIP in allIPHelperWithDate)
            {
                rowNew = table.InsertRow(rowNewData, true);
                npp++;
                rowNew.Cells[0].Paragraphs[0].InsertText(npp.ToString() + ".", false, formatting);
                rowNew.Cells[1].Paragraphs[0].InsertText(rowIP.Type, false, formatting);
                rowNew.Cells[2].Paragraphs[0].InsertText(rowIP.Model, false, formatting);
                rowNew.Cells[3].Paragraphs[0].InsertText(rowIP.WorkNumber, false, formatting);
                rowNew.Cells[4].Paragraphs[0].InsertText(rowIP.DiapasonF, false, formatting);
                rowNew.Cells[5].Paragraphs[0].InsertText(rowIP.Date, false, formatting);
            }
            //удаление строк-шаблонов            
            table.Rows[index].Remove();
            table.Rows[index].Remove();
        }
        void Table_3_Make(Table table)
        {
            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            var size = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.Size;
            if (size != null)
                formatting.Size = (double)size;
            //текущая строка
            Row rowNew;
            //отформатированная строка данных, сформированная из строки заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewData = table.InsertRow(table.Rows[0], true);
            int index = table.Rows.Count() - 1;
            foreach (var cell in rowNewData.Cells)
            {
                cell.FillColor = Color.White;
                foreach (var p in cell.Paragraphs)
                    p.ReplaceText(p.Text, "");
            }

            foreach (var row in ModesForReport)
            {
                rowNew = table.InsertRow(rowNewData, true);
                rowNew.Cells[0].Paragraphs[0].InsertText(row.NPP + ".", false, formatting);
                rowNew.Cells[1].Paragraphs[0].InsertText(row.mode.MODE_TYPE.MT_NAME, false, formatting);
                rowNew.Cells[2].Paragraphs[0].InsertText(row.mode.MODE_IS_SOLID ? "сплошной" : "дискретный", false, formatting);
                rowNew.Cells[3].Paragraphs[0].InsertText(Math.Round(row.mode.MODE_TAU, 3).ToString(), false, formatting);
                rowNew.Cells[4].Paragraphs[0].InsertText(Math.Round(row.mode.MODE_TAU * 2, 3).ToString(), false, formatting);
                rowNew.Cells[5].Paragraphs[0].InsertText(row.mode.MODE_TYPE.MT_KN.ToString(), false, formatting);
            }
            //удаление строки-шаблона
            table.Rows[index].Remove();
        }
        void Table_4_Make(Table table)
        {
            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            var size = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.Size;
            if (size != null)
                formatting.Size = (double)size;
            int npp = 0;
            //текущая строка
            Row rowNew;
            //отформатированная строка данных, сформированная из строки заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewData = table.InsertRow(table.Rows[0], true);
            rowNewData.Height = 0;
            int index = table.Rows.Count() - 1;
            foreach (var cell in rowNewData.Cells)
            {
                cell.FillColor = Color.White;
                foreach (var p in cell.Paragraphs)

                    p.Remove(false);
                cell.Paragraphs[0].Alignment = Xceed.Words.NET.Alignment.center;
            }
            //отформатированная строка заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewHeader = table.InsertRow(rowNewData, true);
            rowNewHeader.Height = 0;
            rowNewHeader.MergeCells(0, rowNewHeader.Cells.Count - 1);
            rowNewHeader.Cells[0].FillColor = Color.LightGray;
            foreach (var row in ModesForReport)
            {
                rowNew = table.InsertRow(rowNewHeader, true);
                //rowNew.Cells[0].Paragraphs[0].InsertText("Режим № " + row.NPP + ". " + row.mode.MODE_TYPE.MT_NAME, false, formatting);

                foreach (var rowData in row.mode.RESULT.Where(p => p.RES_I != 0).OrderBy(p => p.RES_I))
                {
                    string c1 = rowData.RES_I == 1 ? "0,009" : Math.Round((rowData.RES_I - 1) / rowData.MODE.MODE_TAU * 1000, 3).ToString();

                    string c2 = Math.Round(rowData.RES_I / rowData.MODE.MODE_TAU * 1000, 3).ToString();

                    string c4 = rowData.RES_TYPE.ToUpper().Contains("E") ? "Эл" : "Маг";
                    npp++;
                    rowNew = table.InsertRow(rowNewData, true);

                    // rowNew.Cells[0].Paragraphs[0].InsertText(npp + ".", false, formatting);
                    rowNew.Cells[0].Paragraphs[0].InsertText(rowData.RES_I.ToString(), false, formatting);
                    rowNew.Cells[1].Paragraphs[0].InsertText(c1, false, formatting);
                    rowNew.Cells[2].Paragraphs[0].InsertText(c2, false, formatting);
                    //  rowNew.Cells[4].Paragraphs[0].InsertText(c4, false, formatting);
                    rowNew.Cells[3].Paragraphs[0].InsertText(c4, false, formatting);
                    // rowNew.Cells[6].Paragraphs[0].InsertText(rowData.MDA_ECN_VALUE_IZM_MKV.ToString(), false, formatting);
                    // rowNew.Cells[7].Paragraphs[0].InsertText(rowData.MDA_EN_VALUE_IZM_MKV.ToString(), false, formatting);
                    rowNew.Cells[4].Paragraphs[0].InsertText(rowData.RES_SIGNAL.ToString(), false, formatting);

                }
            }
            //удаление строк-шаблонов
            table.Rows[index].Remove();
            table.Rows[index].Remove();
        }
        void Table_5_Make(Table table)
        {
            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[1].Cells[1].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            var size = table.Rows[1].Cells[1].Paragraphs[0].MagicText[0].formatting.Size;
            if (size != null)
                formatting.Size = (double)size;

            //текущая строка
            Row rowNew;
            //отформатированная строка данных, сформированная из строки заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewData = table.InsertRow(table.Rows[1], true);
            rowNewData.Height = 0;
            int index = table.Rows.Count() - 1;
            foreach (var cell in rowNewData.Cells)
            {
                cell.FillColor = Color.White;
                foreach (var p in cell.Paragraphs)
                    p.Remove(false);
                cell.Paragraphs[0].Alignment = Xceed.Words.NET.Alignment.center;
            }
            //отформатированная строка заголовка, исп-ся в качестве шаблона при добавлении строки
            Row rowNewHeader = table.InsertRow(rowNewData, true);
            rowNewHeader.Height = 0;
            rowNewHeader.MergeCells(0, rowNewHeader.Cells.Count - 1);
            rowNewHeader.Cells[0].FillColor = Color.LightGray;
            foreach (var row in ModesForReport)
            {
                rowNew = table.InsertRow(rowNewHeader, true);
                rowNew.Cells[0].Paragraphs[0].InsertText("Режим № " + row.NPP + ". " + row.mode.MODE_TYPE.MT_NAME, false, formatting);

                foreach (var rowData in row.mode.RESULT.OrderBy(p => p.RES_I))
                {
                    rowNew = table.InsertRow(rowNewData, true);

                    rowNew.Cells[0].Paragraphs[0].InsertText(rowData.RES_I.ToString(), false, formatting);
                    rowNew.Cells[1].Paragraphs[0].InsertText(rowData.RES_DELTA_PORTABLE.ToString(), false, formatting);
                    rowNew.Cells[2].Paragraphs[0].InsertText(rowData.RES_DELTA_PORTABLE_DRIVE.ToString(), false, formatting);
                    rowNew.Cells[3].Paragraphs[0].InsertText(rowData.RES_DELTA_PORTABLE_CARRY.ToString(), false, formatting);
                    rowNew.Cells[4].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_2.ToString(), false, formatting);
                    rowNew.Cells[5].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_DRIVE_2.ToString(), false, formatting);
                    rowNew.Cells[6].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_CARRY_2.ToString(), false, formatting);
                    if (rowData.RES_R1_SOSR_2 != 0)
                        rowNew.Cells[7].Paragraphs[0].InsertText(rowData.RES_R1_SOSR_2.ToString(), false, formatting);
                    rowNew.Cells[8].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_3.ToString(), false, formatting);
                    rowNew.Cells[9].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_DRIVE_3.ToString(), false, formatting);
                    rowNew.Cells[10].Paragraphs[0].InsertText(rowData.RES_R2_PORTABLE_CARRY_3.ToString(), false, formatting);
                    if (rowData.RES_R1_SOSR_3 != 0)
                        rowNew.Cells[11].Paragraphs[0].InsertText(rowData.RES_R1_SOSR_3.ToString(), false, formatting);
                }
            }
            //Строки с R2, r1
            rowNew = table.InsertRow(rowNewData, true);
            rowNew.MergeCells(0, 3);
            rowNew.Cells[0].FillColor = Color.LightGray;
            rowNew.Cells[0].Paragraphs[0].InsertText("Максимальные значения ", false, formatting);
            rowNew.Cells[1].Paragraphs[0].InsertText("R2ст", false, formatting);
            rowNew.Cells[2].Paragraphs[0].InsertText("R2воз", false, formatting);
            rowNew.Cells[3].Paragraphs[0].InsertText("R2нос", false, formatting);
            rowNew.Cells[4].Paragraphs[0].InsertText("r1", false, formatting);
            rowNew.Cells[5].Paragraphs[0].InsertText("R2ст", false, formatting);
            rowNew.Cells[6].Paragraphs[0].InsertText("R2воз", false, formatting);
            rowNew.Cells[7].Paragraphs[0].InsertText("R2нос", false, formatting);
            rowNew.Cells[8].Paragraphs[0].InsertText("r1", false, formatting);
            rowNew = table.InsertRow(rowNewData, true);
            rowNew.MergeCells(0, 3);
            rowNew.Cells[0].FillColor = Color.LightGray;
            rowNew.Cells[0].Paragraphs[0].InsertText("R2 и r1", false, formatting);
            rowNew.Cells[1].Paragraphs[0].InsertText(R2_MAX_P_2.ToString(), false, formatting);
            rowNew.Cells[2].Paragraphs[0].InsertText(R2_MAX_D_2.ToString(), false, formatting);
            rowNew.Cells[3].Paragraphs[0].InsertText(R2_MAX_C_2.ToString(), false, formatting);
            rowNew.Cells[4].Paragraphs[0].InsertText(R1_SOSR_MAX_2.ToString(), false, formatting);
            rowNew.Cells[5].Paragraphs[0].InsertText(R2_MAX_P_3.ToString(), false, formatting);
            rowNew.Cells[6].Paragraphs[0].InsertText(R2_MAX_D_3.ToString(), false, formatting);
            rowNew.Cells[7].Paragraphs[0].InsertText(R2_MAX_C_3.ToString(), false, formatting);
            rowNew.Cells[8].Paragraphs[0].InsertText(R1_SOSR_MAX_3.ToString(), false, formatting);
            table.MergeCellsInColumn(0, table.RowCount - 2, table.RowCount - 1);
            //удаление строк-шаблонов
            table.Rows[index].Remove();
            table.Rows[index].Remove();
        }
        void Table_2_pred_Make(Table table)
        {

            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            XceedW.Formatting formattingBold = new XceedW.Formatting()
            {
                Bold = true,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            if (arm_one != null)
            {
                table.Rows[2].Cells[0].Paragraphs[0].InsertText(arm_one.ARM_NUMBER, false, formattingBold);
            }
            if (R2_MAX_P_2 != 0)
                table.Rows[2].Cells[3].Paragraphs[0].InsertText(R2_MAX_P_2.ToString(), false, formatting);
            if (R2_MAX_P_3 != 0)
                table.Rows[2].Cells[4].Paragraphs[0].InsertText(R2_MAX_P_3.ToString(), false, formatting);
            if (R2_MAX_D_2 != 0)
                table.Rows[3].Cells[3].Paragraphs[0].InsertText(R2_MAX_D_2.ToString(), false, formattingBold);
            if (R2_MAX_D_3 != 0)
                table.Rows[3].Cells[4].Paragraphs[0].InsertText(R2_MAX_D_3.ToString(), false, formattingBold);

            if (R2_MAX_C_2 != 0)
                table.Rows[4].Cells[3].Paragraphs[0].InsertText(R2_MAX_C_2.ToString(), false, formatting);
            if (R2_MAX_C_3 != 0)
                table.Rows[4].Cells[4].Paragraphs[0].InsertText(R2_MAX_C_3.ToString(), false, formatting);

            if (R1_SOSR_MAX_2 != 0)
                table.Rows[5].Cells[3].Paragraphs[0].InsertText(R1_SOSR_MAX_2.ToString(), false, formatting);
            if (R1_SOSR_MAX_3 != 0)
                table.Rows[5].Cells[4].Paragraphs[0].InsertText(R1_SOSR_MAX_3.ToString(), false, formatting);

        }
        void Table_3_pred_Make(Table table)
        {

            XceedW.Formatting formatting = new XceedW.Formatting()
            {
                Bold = false,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            XceedW.Formatting formattingBold = new XceedW.Formatting()
            {
                Bold = true,
                FontFamily = table.Rows[0].Cells[0].Paragraphs[0].MagicText[0].formatting.FontFamily
            };
            if (R2_MAX_2 != 0)
                table.Rows[1].Cells[1].Paragraphs[0].InsertText(R2_MAX_2.ToString(), false, formatting);
            if (R2_MAX_3 != 0)
                table.Rows[1].Cells[2].Paragraphs[0].InsertText(R2_MAX_3.ToString(), false, formatting);
        }
        public void Report(string n)
        {
            if (isWord)
            {
                ReportWord(null);
                return;
            }
            PrepareDataForReport();
            //MakeResume();   //вывод о защищённости ТС в режимах
            MakeReport(n);
            //ReportShow();

        }
        public void MakeReport(string n) //заглушка
        {
            if (n == "ProtPost" || n == "ProtFSTEK")
            {
                MakeAllIP();
                // MakeMeasuringForReport();
            }
            if (n == "ProtFSTEK")
            {
                // MakeModesResultForReport();
            }
            string reportName = "";
            ReportWindow rw = new ReportWindow();
            ReportWindowViewModel rwvm;// = new ReportWindowViewModel(methodsEntities, xr, anl_id);
            switch (n)
            {
                case "PredPost":   //предписание-поставка
                    xr = new XtraReportPred_Post();
                    reportName = xr.GetType().Name;
                    //первоначальное заполнение переменных(настраиваемых) полей отчёта с переменным содержимым
                    FirstReportLabelValue(reportName);
                    rwvm = new ReportWindowViewModel(methodsEntities, xr, anl_id);

                    break;
                case "PredPostFSTEK":   //предписание-ФСТЭК
                    xr = new XtraReportPred_FSTEK();
                    reportName = xr.GetType().Name;
                    //первоначальное заполнение переменных(настраиваемых) полей отчёта с переменным содержимым
                    FirstReportLabelValue(reportName);
                    rwvm = new ReportWindowViewModel(methodsEntities, xr, anl_id);

                    break;
                case "ProtPost":   //протокол-поставка
                    xr = new ReportResult_Post();
                    reportName = xr.GetType().Name;
                    //первоначальное заполнение переменных(настраиваемых) полей отчёта с переменным содержимым
                    FirstReportLabelValue(reportName);
                    rwvm = new ReportWindowViewModel(methodsEntities, xr, anl_id);
                    string tables3 = Modes.Count == 1 ? "таблице 3.1" : "таблицах 3.1 - " + "3." + Modes.Count.ToString();
                    //динамическое содержимое метки
                    ((ReportResult_Post)xr).xrLabelAfterJpg.Text = ((ReportResult_Post)xr).xrLabelAfterJpg.Text.Replace("*", tables3);
                    break;

                case "ProtFSTEK":   //протокол-ФСТЭК
                    xr = new ReportResult_FSTEK();
                    reportName = xr.GetType().Name;
                    //первоначальное заполнение переменных(настраиваемых) полей отчёта с переменным содержимым
                    FirstReportLabelValue(reportName);
                    rwvm = new ReportWindowViewModel(methodsEntities, xr, anl_id);
                    // string tables4 = Modes.Count == 1 ? "таблице 4.1" : "таблицах 4.1 - " + "4." + Modes.Count.ToString();
                    //динамическое содержимое метки
                    // ((ReportResult_FSTEK)xr).xrLabelAfterJpg.Text = ((ReportResult_FSTEK)xr).xrLabelAfterJpg.Text.Replace("*", tables4);
                    //видимость наводок
                    if (ModesForReport.Any())
                        foreach (var row in ModesForReport)
                        {
                            if (row.mode.MEASURING_DATA.Where(p => p.MDA_UF != 0 || p.MDA_U0 != 0).Any())
                            {
                                ((ReportResult_FSTEK)xr).DetailReportNavodki_1.Visible = true;
                                ((ReportResult_FSTEK)xr).DetailReportNavodki_2.Visible = true;
                                break;
                            }
                        }
                    break;
                default:
                    return;
            }
            rwvm.closeWindow += rw.CloseWindow;
            rw.DataContext = rwvm;
            rw.ShowDialog();
            if (rwvm.keyCancel)
                return;
            //продолжение сценария
            BindingSource bs = new BindingSource()
            {
                DataSource = this
            };
            xr.DataSource = bs;
            ReportShow();
        }
        public void ReportShow()
        {
            if (xr == null)
                return;
            try
            {
                //работающий код
                //XtraReportPreviewModel model = new XtraReportPreviewModel(xr);
                //DocumentPreviewWindow window = new DocumentPreviewWindow();
                //window.Model = model;
                //xr.CreateDocument();
                //window.Topmost = true;
                //window.ShowDialog();



                WindowReport window = new WindowReport();
                window.dpc.DocumentSource = xr;
                //    xr.RequestParameters = true;
                xr.CreateDocument();
                window.dpc.Document.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
                window.dpc.Document.PageSettings.LeftMargin = 65; //в сотых долях дюйма                              
                window.dpc.Document.PageSettings.RightMargin = 60;
                window.dpc.Document.PageSettings.TopMargin = 88;
                window.dpc.Document.PageSettings.BottomMargin = 60;
                window.ShowDialog();

            }
            catch (Exception eView)
            {
                {
                    MessageBox.Show("Ошибка вывода файл на экран. " + eView.Message);
                }
            }
            finally { }
        }

        private void MakeMaxValue()
        {
            CellStyleConverter.RES_SAZ_MAX = 0;
            CellStyleConverter.RES_DELTA_PORTABLE_MAX = 0;
            CellStyleConverter.RES_DELTA_PORTABLE_DRIVE_MAX = 0;
            CellStyleConverter.RES_DELTA_PORTABLE_CARRY_MAX = 0;
            //R2_MAX_P_1 = 0;
            //R2_MAX_D_1 = 0;
            //R2_MAX_C_1 = 0;

            R2_MAX_P_2 = 0;
            R2_MAX_D_2 = 0;
            R2_MAX_C_2 = 0;

            R2_MAX_P_3 = 0;
            R2_MAX_D_3 = 0;
            R2_MAX_C_3 = 0;

            // R2_MAX_1 = 0;
            R2_MAX_2 = 0;
            R2_MAX_3 = 0;

            //R1_SOSR_MAX_1 = 0;
            R1_SOSR_MAX_2 = 0;
            R1_SOSR_MAX_3 = 0;

            K_MAX_D_F_2 = 0;
            K_MAX_C_F_2 = 0;
            K_MAX_D_0_2 = 0;
            K_MAX_C_0_2 = 0;

            K_MAX_D_F_3 = 0;
            K_MAX_C_F_3 = 0;
            K_MAX_D_0_3 = 0;
            K_MAX_C_0_3 = 0;

            if (arm_id == 0)
                return;
            foreach (MODE mode in methodsEntities.MODE.Where(p => p.MODE_ARM_ID == arm_id))
            {
                //макс.значения в режиме
                //R2_P_1 = 0;
                R2_P_2 = 0;
                R2_P_3 = 0;
                //R2_C_1 = 0;
                R2_C_2 = 0;
                R2_C_3 = 0;
                // R2_D_1 = 0;
                R2_D_2 = 0;
                R2_D_3 = 0;
                //R1_SOSR_1 = 0;
                R1_SOSR_2 = 0;
                R1_SOSR_3 = 0;
                K_D_F_2 = 0;
                K_C_F_2 = 0;
                K_D_0_2 = 0;
                K_C_0_2 = 0;

                K_D_F_3 = 0;
                K_C_F_3 = 0;
                K_D_0_3 = 0;
                K_C_0_3 = 0;

                if (mode.RESULT != null)
                {
                    foreach (var r in mode.RESULT)
                    {   //максимальные значения в режиме
                        if (CellStyleConverter.RES_SAZ_MAX < r.RES_SAZ)
                            CellStyleConverter.RES_SAZ_MAX = r.RES_SAZ;
                        if (CellStyleConverter.RES_DELTA_PORTABLE_MAX < r.RES_DELTA_PORTABLE)
                            CellStyleConverter.RES_DELTA_PORTABLE_MAX = r.RES_DELTA_PORTABLE;
                        if (CellStyleConverter.RES_DELTA_PORTABLE_DRIVE_MAX < r.RES_DELTA_PORTABLE_DRIVE)
                            CellStyleConverter.RES_DELTA_PORTABLE_DRIVE_MAX = r.RES_DELTA_PORTABLE_DRIVE;
                        if (CellStyleConverter.RES_DELTA_PORTABLE_CARRY_MAX < r.RES_DELTA_PORTABLE_CARRY)
                            CellStyleConverter.RES_DELTA_PORTABLE_CARRY_MAX = r.RES_DELTA_PORTABLE_CARRY;
                        //if (R2_P_1 < r.RES_R2_PORTABLE_1) //максимальные значения в режиме
                        //    R2_P_1 = (double)r.RES_R2_PORTABLE_1;
                        if (R2_P_2 < r.RES_R2_PORTABLE_2)
                            R2_P_2 = (double)r.RES_R2_PORTABLE_2;
                        if (R2_P_3 < r.RES_R2_PORTABLE_3)
                            R2_P_3 = (double)r.RES_R2_PORTABLE_3;
                        //if (R2_C_1 < r.RES_R2_PORTABLE_CARRY_1)
                        //    R2_C_1 = (double)r.RES_R2_PORTABLE_CARRY_1;
                        if (R2_C_2 < r.RES_R2_PORTABLE_CARRY_2)
                            R2_C_2 = (double)r.RES_R2_PORTABLE_CARRY_2;
                        if (R2_C_3 < r.RES_R2_PORTABLE_CARRY_3)
                            R2_C_3 = (double)r.RES_R2_PORTABLE_CARRY_3;
                        //if (R2_D_1 < r.RES_R2_PORTABLE_DRIVE_1)
                        //    R2_D_1 = (double)r.RES_R2_PORTABLE_DRIVE_1;
                        if (R2_D_2 < r.RES_R2_PORTABLE_DRIVE_2)
                            R2_D_2 = (double)r.RES_R2_PORTABLE_DRIVE_2;
                        if (R2_D_3 < r.RES_R2_PORTABLE_DRIVE_3)
                            R2_D_3 = (double)r.RES_R2_PORTABLE_DRIVE_3;
                        if (K_D_F_2 < r.RES_DRIVE_KF_2)
                            K_D_F_2 = r.RES_DRIVE_KF_2;
                        if (K_C_F_2 < r.RES_CARRY_KF_2)
                            K_C_F_2 = r.RES_CARRY_KF_2;
                        if (K_D_0_2 < r.RES_DRIVE_K0_2)
                            K_D_0_2 = r.RES_DRIVE_K0_2;
                        if (K_C_0_2 < r.RES_CARRY_K0_2)
                            K_C_0_2 = r.RES_CARRY_K0_2;
                        if (K_D_F_3 < r.RES_DRIVE_KF_2)
                            K_D_F_3 = r.RES_DRIVE_KF_3;
                        if (K_C_F_3 < r.RES_CARRY_KF_3)
                            K_C_F_3 = r.RES_CARRY_KF_3;
                        if (K_D_0_3 < r.RES_DRIVE_K0_3)
                            K_D_0_3 = r.RES_DRIVE_K0_3;
                        if (K_C_0_3 < r.RES_CARRY_K0_3)
                            K_C_0_3 = r.RES_CARRY_K0_3;

                        //if (R1_SOSR_1 == 0 || r.RES_R1_SOSR_1 != 0 && R1_SOSR_1 < r.RES_R1_SOSR_1)
                        //    R1_SOSR_1 = (double)r.RES_R1_SOSR_1;
                        if (R1_SOSR_2 == 0 || r.RES_R1_SOSR_2 != 0 && R1_SOSR_2 < r.RES_R1_SOSR_2)
                            R1_SOSR_2 = (double)r.RES_R1_SOSR_2;
                        if (R1_SOSR_3 == 0 || r.RES_R1_SOSR_3 != 0 && R1_SOSR_3 < r.RES_R1_SOSR_3)
                            R1_SOSR_3 = (double)r.RES_R1_SOSR_3;
                    }
                }
                //сохраним макс. значения для 2-й кат воз.СР в БД
                mode.MODE_R2 = R2_D_2;
                //максммальные значения в АРМе для различных средств разведки
                // R2_MAX_P_1 = Math.Max(R2_MAX_P_1, R2_P_1);
                R2_MAX_P_2 = Math.Max(R2_MAX_P_2, R2_P_2);
                R2_MAX_P_3 = Math.Max(R2_MAX_P_3, R2_P_3);

                // R2_MAX_C_1 = Math.Max(R2_MAX_C_1, R2_C_1);
                R2_MAX_C_2 = Math.Max(R2_MAX_C_2, R2_C_2);
                R2_MAX_C_3 = Math.Max(R2_MAX_C_3, R2_C_3);

                //R2_MAX_D_1 = Math.Max(R2_MAX_D_1, R2_D_1);
                R2_MAX_D_2 = Math.Max(R2_MAX_D_2, R2_D_2);
                R2_MAX_D_3 = Math.Max(R2_MAX_D_3, R2_D_3);

                //максммальные значения в АРМе для всех средств разведки
                //R2_MAX_1 = Math.Max(R2_MAX_1, Math.Max(Math.Max(R2_P_1, R2_C_1), R2_D_1));
                R2_MAX_2 = Math.Max(R2_MAX_2, Math.Max(Math.Max(R2_P_2, R2_C_2), R2_D_2));
                R2_MAX_3 = Math.Max(R2_MAX_3, Math.Max(Math.Max(R2_P_3, R2_C_3), R2_D_3));

                //R1_SOSR_MAX_1 = Math.Max(R1_SOSR_MAX_1, R1_SOSR_1);
                R1_SOSR_MAX_2 = Math.Max(R1_SOSR_MAX_2, R1_SOSR_2);
                R1_SOSR_MAX_3 = Math.Max(R1_SOSR_MAX_3, R1_SOSR_3);

                K_MAX_D_F_2 = Math.Max(K_MAX_D_F_2, K_D_F_2);
                K_MAX_D_F_3 = Math.Max(K_MAX_D_F_3, K_D_F_3);
                K_MAX_D_0_2 = Math.Max(K_MAX_D_0_2, K_D_0_2);
                K_MAX_D_0_3 = Math.Max(K_MAX_D_0_3, K_D_0_3);

                K_MAX_C_F_2 = Math.Max(K_MAX_C_F_2, K_C_F_2);
                K_MAX_C_F_3 = Math.Max(K_MAX_C_F_3, K_C_F_3);
                K_MAX_C_0_2 = Math.Max(K_MAX_C_0_2, K_C_0_2);
                K_MAX_C_0_3 = Math.Max(K_MAX_C_0_3, K_C_0_3);

            }
            RaisePropertyChanged(() => R2_MAX_D_2);
            //SaveData(null);
            try
            {
                methodsEntities.SaveChanges();
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохранения в БД " + e.Message);
                return;
            }
            if (selectedMode == null)
                return;
            Results = new ObservableCollection<RESULT>(
                                  methodsEntities.RESULT.Where(p => p.MODE.MODE_ARM_ID == arm_one.ARM_ID)); // p.RES_MODE_ID == selectedMode.MODE_ID));

            filterResults = "RES_MODE_ID = " + selectedMode.MODE_ID.ToString();
            //обновим таблицы , вынесено, чтобы можно было вызывать в потоке
            //RefreshGcResults?.Invoke();

        }
        //значение частоты и тау по умолчанию для выбранного режима
        public void ModeMtUpdated(MODE mode) //
        {
            //редактируемая строка в контексте
            if (mode == null)
                return;
            if (mode.MODE_MT_ID != 0 && methodsEntities.MODE_TYPE.Where(p => p.MT_ID == mode.MODE_MT_ID && p.MT_F_DEFAULT != 0).Any())
            {
                var mt = methodsEntities.MODE_TYPE.Where(p => p.MT_ID == mode.MODE_MT_ID && p.MT_F_DEFAULT != 0).FirstOrDefault();
                mode.MODE_FT = (double)mt.MT_F_DEFAULT;
                double ft = Functions.F_kGc(mode.MODE_FT,
                Functions.GetUnitValue(Units, (int)mode.MODE_FT_UNIT_ID));
                string tauUnit = Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID);
                if (tauUnit.Contains("сек"))
                {
                    double k = tauUnit == "нсек" ? 1000000 :
                              (tauUnit == "мксек" ? 1000 :
                              (tauUnit == "мсек" ? 1 : 0.0001));
                    mode.MODE_TAU = Math.Round(k / ft / 2, 6); //в заданной единице                    
                }
            }

        }
        #region Работа со справочниками
        //работа со справочниками
        private void GetNSI(string tableName)
        {

            switch (tableName)
            {
                case "ORGANIZATION":

                    if (ow == null)

                    {
                        ow = new OrganizationWindow();
                        OrganizationViewModel vm = new OrganizationViewModel(methodsEntities);
                        vm.focus += ow.Focus;
                        ow.DataContext = vm;
                    }
                    ow.ShowDialog();
                    ow = null;
                    methodsEntities.ORGANIZATION.Load();
                    //Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
                    RefreshOrganization(0);
                    RaisePropertyChanged(() => Organizations);
                    break;
                case "PERSON":

                    if (pw == null)
                    {
                        pw = new PersonWindow();
                        PersonViewModel vm = new PersonViewModel(methodsEntities);
                        vm.focus += pw.Focus;
                        pw.DataContext = vm;
                    }
                    pw.ShowDialog();
                    pw = null;
                    methodsEntities.PERSON.Load();
                    Persons = new ObservableCollection<PERSON>(methodsEntities.PERSON.OrderBy(p => p.PERSON_FIO));
                    Persons_M = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("М") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                    Persons_I = new ObservableCollection<PERSON>(methodsEntities.PERSON.Where(p => p.PERSON_NOTE.Contains("И") || String.IsNullOrEmpty(p.PERSON_NOTE)));
                    RaisePropertyChanged(() => Persons_M);
                    RaisePropertyChanged(() => Persons_I);
                    RaisePropertyChanged(() => Persons);
                    break;
                case "PRODUCER":
                    if (prw == null)
                    {
                        prw = new ProducerWindow();
                        ProducerViewModel vm = new ProducerViewModel(methodsEntities);
                        vm.focus += prw.Focus;
                        prw.DataContext = vm;
                    }
                    prw.ShowDialog();
                    prw = null;
                    methodsEntities.PRODUCER.Load();
                    Producers = new ObservableCollection<PRODUCER>(methodsEntities.PRODUCER.OrderBy(p => p.PROD_NAME));
                    RaisePropertyChanged(() => Producers);
                    break;
                case "EQUIPMENT_TYPE":
                    if (etw == null)
                    {
                        etw = new EqTypeWindow();
                        EqTypeViewModel vm = new EqTypeViewModel(methodsEntities);
                        vm.focus += etw.Focus;
                        etw.DataContext = vm;
                    }
                    etw.ShowDialog();
                    etw = null;
                    methodsEntities.EQUIPMENT_TYPE.Load();
                    EquipmentTypes = new ObservableCollection<EQUIPMENT_TYPE>(methodsEntities.EQUIPMENT_TYPE.OrderBy(p => p.EQT_NAME));
                    RaisePropertyChanged(() => EquipmentTypes);
                    break;
                case "MODE_TYPE":
                    if (mtw == null)
                    {
                        mtw = new ModeTypeWindow();
                        ModeTypeViewModel vm = new ModeTypeViewModel(methodsEntities);
                        vm.focus += mtw.Focus;
                        mtw.DataContext = vm;
                    }
                    mtw.ShowDialog();
                    mtw = null;
                    methodsEntities.MODE_TYPE.Load();
                    ModeTypes = new ObservableCollection<MODE_TYPE>(methodsEntities.MODE_TYPE.OrderBy(p => p.MT_NAME));
                    RaisePropertyChanged(() => ModeTypes);
                    break;

                case "UNIT":
                    if (uw == null)
                    {
                        uw = new UnitWindow();
                        UnitViewModel vm = new UnitViewModel(methodsEntities);
                        vm.focus += uw.Focus;
                        uw.DataContext = vm;
                    }
                    uw.ShowDialog();
                    uw = null;
                    methodsEntities.UNIT.Load();
                    Units = new ObservableCollection<UNIT>(methodsEntities.UNIT.OrderBy(p => p.UNIT_VALUE));
                    UnitsF = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("Гц")));
                    UnitsTau = new ObservableCollection<UNIT>(Units.Where(p => p.UNIT_VALUE.Contains("сек")));
                    RaisePropertyChanged(() => Units);
                    break;
                case "ANTENNA":
                    if (aw == null)
                    {
                        aw = new AntennaWindow();
                        AntennaViewModel vm = new AntennaViewModel(methodsEntities);
                        aw.newCalibr += vm.NewCalibr;
                        vm.FocusUI += aw.FocusUI;
                        vm.RefreshGcAntennas += aw.RefrashGcAntennas;
                        aw.DataContext = vm;
                    }
                    aw.ShowDialog();
                    aw = null;
                    // methodsEntities.ANTENNA.Load();
                    RefreshAntennas();
                    RaisePropertyChanged(() => Antennas);
                    RaisePropertyChanged(() => AntennasGS);
                    break;

                case "MEASURING_DEVICE":
                    if (mdw == null)
                    {
                        mdw = new MDeviceWindow();
                        MDeviceViewModel vm = new MDeviceViewModel(methodsEntities);
                        // vm.Cancel += mdw.Cancel;
                        vm.focus += mdw.Focus;
                        // vm.RefreshGcMDevice += mdw.RefreshGcMDevice;
                        mdw.DataContext = vm;
                    }
                    mdw.ShowDialog();
                    mdw = null;
                    methodsEntities.MEASURING_DEVICE.Load();
                    Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                    RaisePropertyChanged(() => Devices);
                    break;
                case "MEASURING_DEVICE_TYPE":
                    if (mdw == null)
                    {
                        mdtw = new MDTWindow();
                        MDTViewModel vm = new MDTViewModel(methodsEntities);
                        // vm.Cancel += mdtw.Cancel;
                        vm.focus += mdtw.Focus;
                        mdtw.DataContext = vm;
                    }
                    mdtw.ShowDialog();
                    mdtw = null;
                    methodsEntities.MEASURING_DEVICE_TYPE.Load();
                    Devices = new ObservableCollection<MEASURING_DEVICE>(methodsEntities.MEASURING_DEVICE.Where(p => p.MD_IS_HELPER != "да" || p.MD_IS_HELPER == null).OrderBy(p => p.MEASURING_DEVICE_TYPE.MDT_NAME));
                    RaisePropertyChanged(() => Devices);
                    break;
            }

        }
        private void RefreshAntennas()
        {
            Antennas = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.Where(p => p.ANT_TYPE != "Генератор шума").OrderBy(p => p.ANT_TYPE));
            AntennasE = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.Where(p => p.ANT_TYPE.Contains("Элек")).OrderBy(p => p.ANT_TYPE));
            AntennasH = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.Where(p => p.ANT_TYPE.Contains("Маг")).OrderBy(p => p.ANT_TYPE));
            AntennasGS = new ObservableCollection<ANTENNA>(methodsEntities.ANTENNA.Where(p => p.ANT_TYPE == "Генератор шума").OrderBy(p => p.ANT_TYPE));
            RaisePropertyChanged(() => Antennas);
            RaisePropertyChanged(() => AntennasE);
            RaisePropertyChanged(() => AntennasH);
            RaisePropertyChanged(() => AntennasGS);
        }
        //сценарии работ
        private void ExitEsc(Object o)
        {
            Exit?.Invoke();
        }
        private void Scenario(string number)
        {
            Value = String.Empty;
            AppendMode = false;


            switch (number)
            {
                case "0":   //фильтр работ по временному интервалу счетов
                    DateBegin = new DateTime(DateTime.Now.Year, 1, 1); DateEnd = new DateTime(DateTime.Now.Year, 12, 1);
                    WindowSearch ws = new WindowSearch()
                    {
                        DataContext = this
                    };
                    SearchPrepare();
                    WindowClose += ws.WindowClose;
                    search = true;
                    ws.ShowDialog();
                    search = false;
                    if (anl_id_Search != 0)
                    {
                        RefreshOrganization(methodsEntities.ANALYSIS.Where(p => p.ANL_ID == anl_id_Search).FirstOrDefault().ANL_ORG_ID);
                    }
                    break;
                case "1": //добавление работы, счетов
                    //if (analysis_one == null)
                    //{
                    //    MessageBox.Show("В выбранной работе нет ни одного счёта. Выберите сценарий добавления счетов в работу.");
                    //    return;
                    //}
                    keySuspend = true; //фоновый поток приостановили. Во всех вложеных ф-ях с потоком ничего не делаем
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();//остановим фоновый поток, т.к. даные измерений в реальном времени другим пользователям не нужны
#pragma warning restore CS0618 // Type or member is obsolete
                    WindowOrgAnalysis_1 wo1 = new WindowOrgAnalysis_1();
                    OrgAnalysis_1_ViewModel vm = new OrgAnalysis_1_ViewModel(methodsEntities, userName, org_id, anl_id, analysis_one != null && analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty);
                    wo1.DataContext = vm;
                    vm.closeWindow += wo1.WindowClose;
                    vm.focusName += wo1.focusName;

                    wo1.ShowDialog();
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    //выбор на последнем вводе
                    Organizations = new ObservableCollection<ORGANIZATION>(methodsEntities.ORGANIZATION.OrderBy(p => p.ORG_NAME));
                    org_id = OrgAnalysis_1_ViewModel.orgId;
                    break;
                case "1_1": //добавление счетов
                    keySuspend = true; //фоновый поток приостановили. Во всех вложеных ф-ях с потоком ничего не делаем
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();//остановим фоновый поток, т.к. даные измерений в реальном времени другим пользователям не нужны
#pragma warning restore CS0618 // Type or member is obsolete
                    WindowOrgAnalysis_2 wo2 = new WindowOrgAnalysis_2()
                    {
                        Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME
                    };
                    wo2.buttonPrev.Visibility = System.Windows.Visibility.Hidden;
                    if (analysis_one != null)
                        vm = new OrgAnalysis_1_ViewModel(methodsEntities, userName, org_id, anl_id, analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty);
                    else
                        vm = new OrgAnalysis_1_ViewModel(methodsEntities, userName, org_id, anl_id, String.Empty);
                    vm.analysis = analysis;
                    wo2.DataContext = vm;
                    vm.closeWindow2 += wo2.WindowClose;
                    vm.focusName2 += wo2.focusName;
                    vm.PrepareNewAnalysis();
                    //vm.anl_id = vm.analysis.Any() ? vm.analysis[0].ANL_ID : 0;
                    wo2.ShowDialog();

                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }

                    //выбор на последнем вводе
                    if (org_id == OrgAnalysis_1_ViewModel.orgId) //поработали со счетами первоначально выбранной работы
                        analysis = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ANL_ORG_ID == org_id));
                    else
                        org_id = OrgAnalysis_1_ViewModel.orgId;
                    anl_id = OrgAnalysis_1_ViewModel.anlId;
                    //RefreshGcResults?.Invoke();
                    //RefreshGcE?.Invoke();
                    //RefreshGcCollection?.Invoke();

                    break;
                case "2": //состав и параметры счетов
                    if (anl_id == 0)
                    {
                        MessageBox.Show("В работе нет ни одного счёта. Сначала создайте счёт.");
                        return;
                    }
                    WindowTypeARMMode_1 wt1 = new WindowTypeARMMode_1();
                    try
                    {
                        wt1.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " +
                            analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty + ".";
                    }
                    catch (Exception e) { }
                    wt1.DataContext = this;
                    WindowClose1 += wt1.WindowClose;
                    wt1.ShowDialog();

                    break;
                case "3": //измерения, дифференцированный спектр
                    if (isMultiSelect)
                    {
                        MessageBox.Show("Для открытия окна измерений отключите галочку 'Режим автоизмерений");
                        return;
                    }
                    if (anl_id == 0)
                    {
                        MessageBox.Show("В работе нет ни одного счёта. Сначала создайте счёт.");
                        return;
                    }
                    if (selectedMode == null && focusedMode != null)
                        selectedMode = focusedMode;

                    keySuspend = true; //фоновый поток приостановили. Во всех вложеных ф-ях с потоком ничего не делаем
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();//остановим фоновый поток, т.к. даные измерений в реальном времени другим пользователям не нужны
#pragma warning restore CS0618 // Type or member is obsolete
                    MeasuringType = "Электрическое";
                    if (!isSolid) //дифференцированный спектр
                    {
                        if (wmDiff == null || !wmDiff.IsLoaded)
                        {
                            wmDiff = new WindowMeasuringDiff_1();
                            try
                            {
                                wmDiff.Title = selectedMode.MODE_TYPE != null ? selectedMode.MODE_TYPE.MT_NAME : String.Empty;

                            }
                            catch (Exception e) { MessageBox.Show("Ошибка формирования заголовка окна. Сценарий 3." + e.Message); }

                            wmDiff.DataContext = this;

                            WindowClose1 += wmDiff.WindowClose;
                            wmDiff.CellUpdated += CellUpdated;
                            wmDiff.ResultClear += ResultClear;
                            RefreshGcE += wmDiff.RefreshGcE;
                        }
                        tcSelectedItemChanged(0); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                        System.Windows.Clipboard.Clear();
                        canAuto = true;
                        RaisePropertyChanged(() => AutoRBW);
                        wmDiff.ShowDialog();
                        wmDiff = null;
                    }
                    else //сплошной спектр
                    {
                        //ContraintE = false;
                        if (wmSolid == null)
                        {
                            wmSolid = new WindowMeasuringSolid_1();
                            try
                            {
                                wmSolid.Title = selectedMode.MODE_TYPE != null ? selectedMode.MODE_TYPE.MT_NAME : String.Empty;
                            }
                            catch (Exception e) { MessageBox.Show("Ошибка формирования заголовка окна. Сценарий 4." + e.Message); }
                            wmSolid.DataContext = this;

                            WindowClose1 += wmSolid.WindowClose;
                            wmSolid.CellUpdated += CellUpdated;
                            wmSolid.ResultClear += ResultClear;
                            RefreshGcE += wmSolid.RefreshGcE;
                        }
                        tcSelectedItemChanged(0); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                        System.Windows.Clipboard.Clear();
                        canAuto = true;
                        wmSolid.ShowDialog();
                        wmSolid = null;
                    }

                    RefreshGcResults?.Invoke();
                    RefreshGcE?.Invoke();
                    RefreshGcCollection?.Invoke();
                    keyWindowMeasuring_4 = false; //ключ запрета запуска фонового потока пока открыты окна измерений
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    break;
                case "4": //измерения, сплошной спектр
                    keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();
#pragma warning restore CS0618 // Type or member is obsolete
                    if (wmSolid == null)
                    {
                        wmSolid = new WindowMeasuringSolid_1();
                        try
                        {
                            wmSolid.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                                        ".   АРМ - " + arm_one.ARM_TYPE.AT_NAME + "/" + arm_one.ARM_NUMBER + ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + "   Данные измерений.";

                        }
                        catch (Exception e) { MessageBox.Show("Ошибка формирования заголовка окна. Сценарий 4." + e.Message); }
                        wmSolid.DataContext = this;

                        WindowClose1 += wmSolid.WindowClose;
                        wmSolid.CellUpdated += CellUpdated;
                        wmSolid.ResultClear += ResultClear;
                        RefreshGcE += wmSolid.RefreshGcE;
                    }
                    tcSelectedItemChanged(0); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                    System.Windows.Clipboard.Clear();
                    canAuto = true;
                    wmSolid.ShowDialog();
                    wmSolid = null;
                    RefreshGcE?.Invoke();
                    RefreshGcCollection?.Invoke();
                    tcSelectedItemChanged(0);
                    RaisePropertyChanged(() => Measurings); //
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    break;
                case "5": //анализ
                    keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();//
#pragma warning restore CS0618 // Type or member is obsolete
                    if (!Results.Any())
                    {
                        MessageBox.Show("Отсутствуют данные для анализа. Выполните измерения и расчёт.");
                        return;
                    }
                    WindowAnalyseResults wm5 = new WindowAnalyseResults();
                    try
                    {
                        wm5.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME +
                            ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty;
                    }
                    catch (Exception e) { }
                    var vmA = new AnalyseResultsViewModel(methodsEntities, ArmTypes, at_id);
                    wm5.DataContext = vmA;
                    wm5.refreshAnalyse += vmA.refreshAnalyse;
                    wm5.ShowDialog();
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    break;
                case "6": //отчёты
                    keySuspend = true;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Suspend();//
#pragma warning restore CS0618 // Type or member is obsolete
                    if (!Results.Any())
                    {
                        MessageBox.Show("Отсутствуют данные для формирования отчёта. Выполните измерения и расчёт.");
                        return;
                    }
                    WindowReports wm6 = new WindowReports()
                    {
                        DataContext = this
                    };
                    WindowClose1 += wm6.WindowClose;
                    isWord = true;
                    wm6.ShowDialog();
                    if (keySuspend)
                    {
                        keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                        dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    break;
            }
        }
        private void SearchPrepare()
        {
            if (!cbDate)
            {
                analysisSearch = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.OrderBy(p => p.ANL_ORG_ID).OrderBy(p => p.ANL_INVOICE));
            }
            else
            {
                if (cbDate && (DateBegin != null || DateEnd != null))
                {
                    if (DateBegin > DateEnd)
                    {
                        MessageBox.Show("Дата окончания должна быть >= даты начала.");
                        return;
                    }
                    analysisSearch = new ObservableCollection<ANALYSIS>(methodsEntities.ANALYSIS.Where(p => p.ANL_DATE_BEGIN >= DateBegin && p.ANL_DATE_BEGIN <= DateEnd &&
                                                          p.ANL_DATE_END >= DateBegin && p.ANL_DATE_BEGIN <= DateEnd).OrderBy(p => p.ANL_ORG_ID).OrderBy(p => p.ANL_INVOICE));
                }
            }
        }
        private void SearchOK(Object o)
        {
            WindowClose();
        }
        private void CancelWindow(string number)
        {
            switch (number)
            {
                case "1":
                    WindowClose1?.Invoke();
                    break;
                case "2":
                    WindowClose2?.Invoke();
                    break;
                case "3":
                    WindowClose3?.Invoke();
                    break;
                case "4":
                    WindowClose4?.Invoke();
                    break;
                case "12":
                    WindowClose2?.Invoke();
                    WindowClose1?.Invoke();
                    break;
                case "123":
                    WindowClose3?.Invoke();
                    WindowClose2?.Invoke();
                    WindowClose1?.Invoke();
                    break;
                case "23":
                    WindowClose3?.Invoke();
                    WindowClose2?.Invoke();
                    break;
                case "1234":
                    WindowClose4?.Invoke();
                    WindowClose3?.Invoke();
                    WindowClose2?.Invoke();
                    WindowClose1?.Invoke();
                    break;
                default:
                    WindowClose?.Invoke();
                    break;

            }
        }
        private bool canNextTypeARM()
        { return (at_id != 0); }
        private void NextTypeARM(Object o)
        {
            WindowTypeARMMode_2 wt2 = new WindowTypeARMMode_2()
            {
                Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty + ".    " +
                               "Тип АРМ - " + ArmTypes.Where(p => p.AT_ID == at_id).FirstOrDefault().AT_NAME + "."
            };
            wt2.DataContext = this;
            WindowClose2 += wt2.WindowClose; //
            wt2.ShowDialog();
        }
        private bool canNextMode(Object o)
        {
            return (arm_id != 0);
        }
        private void NextMode(Object o)
        {
            WindowTypeARMMode_3 wt3 = new WindowTypeARMMode_3()
            {
                Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty + ".    " +
                               "Тип АРМ - " + ArmTypes.Where(p => p.AT_ID == at_id).FirstOrDefault().AT_NAME + ".   " + "АРМ -  " + arm_one.ARM_NUMBER
            };
            wt3.DataContext = this;

            wt3.CellModesUpdated += CellModesUpdated;
            RefreshGcModes += wt3.RefreshGcModes;
            wt3.solidChanged += solidChanged;
            wt3.contrEChanged += contrEChanged;
            WindowClose3 += wt3.WindowClose; //
            wt3.ShowDialog();
            RefreshGcModes -= wt3.RefreshGcModes;
            WindowClose3 -= wt3.WindowClose; //
            RefreshGcModes?.Invoke();
            RefreshGcCollection?.Invoke();
            //поработали с режимами, пересчитаем
        }
        private void NextMeasuring_2(Object o)
        {
            switch (o.ToString())
            {
                case "DiffPhase":
                    WindowMeasuringDiff_2_phase wm1 = new WindowMeasuringDiff_2_phase();

                    try
                    {
                        wm1.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                                    ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Данные измерений наводок.Дифференцированный спектр, фаза.";

                    }
                    catch (Exception e) { MessageBox.Show("Ошибка создания формы " + e.Message); }

                    tcSelectedItemChanged(1); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                    WindowClose2 += wm1.WindowClose;
                    wm1.CellUpdated += CellUpdated;
                    wm1.ResultClear += ResultClear;
                    wm1.DataContext = this;
                    RefreshGcUF += wm1.RefreshGc;
                    System.Windows.Clipboard.Clear();
                    UMinEn = 0;
                    UMaxEn = 0;
                    UMinn = 0;
                    UMaxn = 0;
                    Tag = "UF";
                    wm1.ShowDialog();
                    RefreshGcUF -= wm1.RefreshGc;
                    RefreshGcUF?.Invoke();
                    break;
                case "DiffNull":
                    WindowMeasuringDiff_2_null wm2 = new WindowMeasuringDiff_2_null();
                    try
                    {
                        wm2.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                                    ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Данные измерений наводок.Дифференцированный спектр, ноль.";

                    }
                    catch (Exception e) { MessageBox.Show("Ошибка создания формы " + e.Message); }

                    tcSelectedItemChanged(2); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                    WindowClose2 += wm2.WindowClose;
                    wm2.CellUpdated += CellUpdated;
                    wm2.ResultClear += ResultClear;
                    RefreshGcU0 += wm2.RefreshGc;
                    wm2.DataContext = this;
                    System.Windows.Clipboard.Clear();
                    UMinEn = 0;
                    UMaxEn = 0;
                    UMinn = 0;
                    UMaxn = 0;
                    Tag = "U0";
                    wm2.ShowDialog();
                    RefreshGcU0 -= wm2.RefreshGc;
                    RefreshGcU0?.Invoke();
                    break;
                case "SolidPhase":
                    WindowMeasuringSolid_2_phase wm3 = new WindowMeasuringSolid_2_phase();
                    try
                    {
                        wm3.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                                    ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Данные измерений наводок.Сплошной спектр, фаза.";

                    }
                    catch (Exception e) { MessageBox.Show("Ошибка создания формы " + e.Message); }

                    tcSelectedItemChanged(1); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                    WindowClose2 += wm3.WindowClose;
                    wm3.CellUpdated += CellUpdated;
                    wm3.ResultClear += ResultClear;
                    RefreshGcUF += wm3.RefreshGc;
                    wm3.DataContext = this;
                    System.Windows.Clipboard.Clear();
                    UMinEn = 0;
                    UMaxEn = 0;
                    UMinn = 0;
                    UMaxn = 0;
                    Tag = "UF";
                    wm3.ShowDialog();
                    RefreshGcUF -= wm3.RefreshGc;
                    break;
                case "SolidNull":
                    WindowMeasuringSolid_2_null wm4 = new WindowMeasuringSolid_2_null();
                    try
                    {
                        wm4.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                                    ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Данные измерений наводок.Сплошной спектр, ноль.";

                    }
                    catch (Exception e) { MessageBox.Show("Ошибка создания формы " + e.Message); }

                    tcSelectedItemChanged(2); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
                    WindowClose2 += wm4.WindowClose;
                    wm4.CellUpdated += CellUpdated;
                    wm4.ResultClear += ResultClear;
                    RefreshGcU0 += wm4.RefreshGc;
                    wm4.DataContext = this;
                    System.Windows.Clipboard.Clear();
                    UMinEn = 0;
                    UMaxEn = 0;
                    UMinn = 0;
                    UMaxn = 0;
                    Tag = "U0";
                    wm4.ShowDialog();
                    RefreshGcU0 -= wm4.RefreshGc;
                    break;
            }
            tcSelectedItemChanged(0); //Устанавливаем предыдущую вкладку на основной форме, для того, чтобы правильно копировать данные измерений
        }
        private void NextMeasuring_3(Object o)
        {
            WindowMeasuring_3 wm3 = new WindowMeasuring_3();
            try
            {
                wm3.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                            ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Данные измерений САЗ.";

            }
            catch (Exception e) { }
            wm3.DataContext = this;
            WindowClose3 += wm3.WindowClose;
            wm3.CellUpdated += CellUpdated;
            wm3.ResultClear += ResultClear;
            refreshGcEHs += wm3.RefreshGcEHs;
            tcSelectedItemChanged(2); //Устанавливаем вкладку на основной форме, для того, чтобы правильно копировать данные измерений
            Tag = "Saz";
            wm3.ShowDialog();
            refreshGcEHs -= wm3.RefreshGcEHs;
            tcSelectedItemChanged(1); //Устанавливаем предыдущую вкладку на основной форме, для того, чтобы правильно копировать данные измерений
        }
        private bool canCalculate()
        { return (paramD != 0 && paramFT != 0 && paramR != 0 && paramTAU != 0 && Measurings != null && Measurings.Any()) && keyCalculate; }
        private void NextMeasuring_4(Object o)
        {
            WindowMeasuring_4 wm4 = new WindowMeasuring_4();

            try
            {
                wm4.Title = "Работа - " + Organizations.Where(p => p.ORG_ID == org_id).FirstOrDefault().ORG_NAME + ".   Счёт - " + analysis_one.ANL_INVOICE != null ? analysis_one.ANL_INVOICE : String.Empty +
                            ".   Режим - " + selectedMode.MODE_TYPE.MT_NAME + ".   Оценка защищённости.";

            }
            catch (Exception e) { }
            wm4.DataContext = this;
            RefreshGcResultsScen += wm4.RefreshGcResultsScen;
            if (Results == null || !Results.Where(p => p.RES_MODE_ID == selectedMode.MODE_ID).Any())
            {
                //CalculateMode(selectedMode, false); //выполнение в том же потоке
                CalculateMode(selectedMode);// выполнение в другом потоке

            }
            WindowClose4 += wm4.WindowClose;
            RefreshGcCollection += wm4.RefreshGcCollection;
            keyWindowMeasuring_4 = true;//чтобы не обновлялась таблица Режимов после расчёта/перерасчёта - не запускался фоновый поток, пока не закроются окна измерений
            wm4.ShowDialog();
            RefreshGcCollection -= wm4.RefreshGcCollection;
            // keyWindowMeasuring_4 = false;
        }
        private bool canNextMeasuring()
        {
            return (canCalculate()); //Measurings?.Any() ?? false
        }
        private bool canEquipmentData(Object o)
        {
            return (arm_one != null);
        }
        private void EquipmentData(Object o)
        {
            EquipmentWindowNew ew;
            EquipmentViewModelNew evm;
            if (arm_one == null)
            {
                MessageBox.Show("Выберите АРМ.");
                return;
            }
            bool keySuspendParent = keySuspend; //состояние потока слежения на момент вызова ф-ии. True - поток приостановлен
            if (!keySuspendParent) //поток не остановлен, останоавливаем на время выполнения ф-ии,иначе ничегог делать не надо
            {
#pragma warning disable CS0618 // Тип или член устарел
                dbChanged.Suspend();     //приостановим фоновый поток
#pragma warning restore CS0618 // Тип или член устарел
                keySuspend = true;
            }
            try
            {
                ew = new EquipmentWindowNew();
                try
                {
                    evm = new EquipmentViewModelNew(methodsEntities, arm_one);
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                    return;
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
                if (!keySuspendParent) // функция вызывалась с работающим потоком, который был остановлен
                {
                    keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                    dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
                }
                return;
            }
            try
            {
                ew.DataContext = evm;
                ew.Title = "Оборудование АРМ - " + arm_one.ARM_TYPE.AT_NAME + "-" + arm_one.ARM_NUMBER;
                evm.RefreshData += ew.RefreshData;
            }
            catch (Exception e3)
            {
                MessageBox.Show(e3.Message);
                return;
            }
            ew.ShowDialog();
            ArmEquipments = new ObservableCollection<EQUIPMENT>(methodsEntities.EQUIPMENT.Where(p => p.EQ_ARM_ID == arm_id && p.EQ_IN_MODE == true));
            //RaisePropertyChanged(() => ArmEquipments);
            RefreshGcModes?.Invoke();
            if (!keySuspendParent) // функция вызывалась с работающим потоком, который был остановлен
            {
                keySuspend = false;
#pragma warning disable CS0618 // Type or member is obsolete
                dbChanged.Resume();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        private void MdData(Object o)
        {
            if (arm_one == null)
            {
                MessageBox.Show("Выберите АРМ.");
                return;
            }
            MdWindow ew = new MdWindow();
            MdViewModel evm = new MdViewModel(methodsEntities, arm_one);
            ew.saveData += evm.SaveData;
            ew.DataContext = evm;
            evm.getSelectedRow += ew.getSelectedRow;
            ew.Title = "Измерительная аппаратура АРМ - " + arm_one.ARM_TYPE.AT_NAME + "-" + arm_one.ARM_NUMBER;
            evm.RefreshData += ew.RefreshData;
            ew.ShowDialog();
            ew = null;
            evm = null;
        }

        #endregion Добавление новых значений в справочники


        #region Commands

        public ICommand DeleteOrganizationCommand { get { return new RelayCommand<Object>(DeleteOrganization); } }
        public ICommand AddOrganizationCommand { get { return new RelayCommand<Object>(AddOrganization); } }
        public ICommand DeleteArmCommand { get { return new DelegateCommandMy<Object>(DeleteArm, canDeleteArm); } }
        public ICommand RenameARMCommand { get { return new DelegateCommandMy<Object>(RenameARM, canDeleteArm); } }
        public ICommand RenameARMTypeCommand { get { return new DelegateCommandMy<Object>(RenameArmType, canDeleteArmType); } }
        public ICommand DeleteArmTypeCommand { get { return new DelegateCommandMy<Object>(DeleteArmType, canDeleteArmType); } }
        public ICommand CancelArmTypeCommand { get { return new RelayCommand<Object>(CancelArmType); } }
        public ICommand AddArmCommand { get { return new RelayCommand<Object>(AddArm); } }
        public ICommand AddArmTypeCommand { get { return new RelayCommand<Object>(AddArmType); } }
        public ICommand CopyArmTypeCommand { get { return new RelayCommand<Object>(CopyArmType); } }
        public ICommand DeleteAnalysisCommand { get { return new RelayCommand<Object>(DeleteAnalysis); } }
        public ICommand AddAnalysisCommand { get { return new RelayCommand<Object>(AddAnalysis, canAddAnalysis); } }
        public ICommand DeleteModeCommand { get { return new DelegateCommandMy<MODE>(DeleteMode, canDeleteMode); } }
        public ICommand DeleteMeasuringDataCommand { get { return new DelegateCommandMy<MEASURING_DATA>(DeleteMeasuringData, canDeleteMeasuringData); } }
        public ICommand DeleteMeasuringDataModeCommand { get { return new DelegateCommandMy<MEASURING_DATA>(DeleteMeasuringDataMode, canDeleteMeasuringDataMode); } }
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand EditDataCommand { get { return new RelayCommand<Object>(EditData); } }
        public ICommand RecalculateCommand { get { return new RelayCommand<Object>(Recalculate); } }
        public ICommand CalculateCommand { get { return new RelayCommand<Object>(Calculate); } }
        public ICommand ReportCommand { get { return new RelayCommand<string>(Report); } }
        public ICommand ReportWordCommand { get { return new RelayCommand<Object>(ReportWord); } }

        public ICommand EquipmentDataCommand { get { return new RelayCommand<Object>(EquipmentData, canEquipmentData); } }
        public ICommand MdDataCommand { get { return new RelayCommand<Object>(MdData, canEquipmentData); } }
        public ICommand GetNSICommand { get { return new RelayCommand<string>(GetNSI); } }
        public ICommand RandomCommand { get { return new RelayCommand<Object>(RandomData, canRandomData); } }
        public ICommand RandomUFCommand { get { return new RelayCommand<Object>(RandomData, canRandomUF0Data); } }
        public ICommand RandomU0Command { get { return new RelayCommand<Object>(RandomData, canRandomUF0Data); } }

        public ICommand GetDataAfterMeasuringAutoCommand { get { return new RelayCommand<object>(GetDataAfterMeasuringAuto, canGetDataAfterMeasuringAuto); } }

        public ICommand MeasuringsUtiliteCommand { get { return new RelayCommand<Object>(MeasuringsUtilite, canMeasuringsUtilite); } }
        public ICommand ScenarioCommand { get { return new RelayCommand<string>(Scenario); } }
        public ICommand ExitCommand { get { return new RelayCommand<Object>(ExitEsc); } }
        public ICommand SearchOKCommand { get { return new RelayCommand<Object>(SearchOK); } }
        public ICommand CancelWindowCommand { get { return new RelayCommand<string>(CancelWindow); } }
        public ICommand NextTypeARMCommand { get { return new DelegateCommandMy<Object>(NextTypeARM, canNextTypeARM); } }
        public ICommand NextModeCommand { get { return new RelayCommand<Object>(NextMode, canNextMode); } }
        public ICommand PasteCommand { get { return new DelegateCommandMy<Object>(PasteFromExcel, canPasteFromExcel); } }
        public ICommand RefreshIWithContrainsCommand { get { return new DelegateCommandMy<Object>(RefreshIWithContrains, canRefreshIWithContrains); } }

        public ICommand ClearMeasuringDataCommand
        {
            get { return new DelegateCommandMy<MODE>(ClearMeasuringData); }
        }
        public ICommand NextMeasuring_2Command { get { return new DelegateCommandMy<Object>(NextMeasuring_2, canNextMeasuring); } }

        public ICommand NextMeasuring_3Command { get { return new DelegateCommandMy<Object>(NextMeasuring_3, canNextMeasuring); } }
        public ICommand NextMeasuring_4Command { get { return new DelegateCommandMy<Object>(NextMeasuring_4, canNextMeasuring); } }
        public ICommand CalculateModeCommand { get { return new DelegateCommandMy<MODE>(CalculateMode, canCalculate); } }
     

        #endregion

    }

    public class IP
    {
        public string NPP { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string WorkNumber { get; set; }
        public string DiapasonF { get; set; }
        public string Date { get; set; }
    }

    public class EqForReport
    {
        public string NPP { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Note { get; set; }
    }

    
    public class MODE_NPP : MODE
    {
        public string NPP { get; set; }
        public string AntennaType { get; set; }
        public MODE mode { get; set; }

    }


}