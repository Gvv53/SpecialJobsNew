using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.XtraReports.UI;
using SpecialJobs.ViewModels.ForReport;
using SpecialJobs.Views.ForReport;
//using Excel = Microsoft.Office.Interop.Excel;
using SpecialJobs.Converters;
using SpecialJobs.Reports;
using SpecialJobs.Views;
using SpecialJobs.Views.ForScenario;
using SpecialJobs.ViewModels.ForScenario;
using SpecialJobs.Helpers;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Threading;
using System.Configuration;
using System.Data.Entity.SqlServer;


namespace SpecialJobs.ViewModels.ForScenario
{
    class AnalyseResultsViewModel:BaseViewModel
    {
    #region Properties
        MethodsEntities methodsEntities { get; set; }
        private ObservableCollection<DataForAnalyse> _DataForAnalyse;
        public ObservableCollection<DataForAnalyse> DataForAnalyse {
            get
            { return _DataForAnalyse; }
            set
            {
                _DataForAnalyse = value;
                RaisePropertyChanged(() => DataForAnalyse);
            }
        }
        

        public ObservableCollection<ARM_TYPE> ArmTypes { get; set; }
        private int _at_id;
        public int at_id {
            get { return _at_id; }
            set
            {
                _at_id = value;
                RaisePropertyChanged(() => at_id);
                MakeDataForAnalyse();
            }
        }
        public bool kategoria1 { get; set; }
        public bool kategoria2 { get; set; }
        public bool kategoria3 { get; set; }
        public bool srPort { get; set; }
        public bool srDrive { get; set; }
        public bool srCarry { get; set; }
        public bool lsSosr { get; set; }
        public bool lsRaspr { get; set; }

        #endregion Properties
        public void refreshAnalyse()
        {
            RaisePropertyChanged(() => kategoria1);
            RaisePropertyChanged(() => kategoria2);
            RaisePropertyChanged(() => kategoria3);
            RaisePropertyChanged(() => srPort);
            RaisePropertyChanged(() => srDrive);
            RaisePropertyChanged(() => srCarry);
            RaisePropertyChanged(() => lsSosr);
            RaisePropertyChanged(() => lsRaspr);
        }
    public AnalyseResultsViewModel(MethodsEntities methodsEntities, ObservableCollection<ARM_TYPE> ArmTypes, int at_id)
        {
            this.methodsEntities = methodsEntities;
            this.ArmTypes = ArmTypes;
            RaisePropertyChanged(() => ArmTypes);
            DataForAnalyse = new ObservableCollection<ForScenario.DataForAnalyse>();
            this.at_id = at_id;
            srPort = srDrive = srCarry = true;
            lsSosr = true;
            kategoria2 = true;
            kategoria1 = false;
            kategoria3 = false;
        }
        //результаты расчёта для всех АРМов выбранного типа
        void MakeDataForAnalyse()
        {
           
            DataForAnalyse = new ObservableCollection<ForScenario.DataForAnalyse>();
         //   DataForAnalyseGrid = new ObservableCollection<ForScenario.DataForAnalyse_Grid>();
            if (methodsEntities.ARM.Where(p=>p.ARM_AT_ID == at_id).Any())
            {
                foreach(ARM arm in methodsEntities.ARM.Where(p => p.ARM_AT_ID == at_id))
                {
                    double? R2p_1arm = 0, R2p_2arm = 0, R2p_3arm = 0, R2d_1arm = 0, R2d_2arm = 0, R2d_3arm = 0, R2c_1arm = 0, R2c_2arm = 0, R2c_3arm = 0,
                                   R1s_1arm = 0, R1s_2arm = 0, R1s_3arm = 0, R1r_1arm = 0, R1r_2arm = 0, R1r_3arm = 0;                   
                    if ((bool)arm.MODE?.Any())
                        foreach(MODE mode in arm.MODE )
                        {
                            double? R2p_1mode = 0, R2p_2mode = 0, R2p_3mode = 0, R2d_1mode = 0, R2d_2mode = 0, R2d_3mode = 0, R2c_1mode = 0, R2c_2mode = 0, R2c_3mode = 0,
                                   R1s_1mode = 0, R1s_2mode = 0, R1s_3mode = 0;
                            int R2ip_1mode = 0, R2ip_2mode = 0, R2ip_3mode = 0, R2id_1mode = 0, R2id_2mode = 0, R2id_3mode = 0, R2ic_1mode = 0, R2ic_2mode = 0, R2ic_3mode = 0,
                                   R1is_1mode = 0, R1is_2mode = 0, R1is_3mode = 0;
                            if (!(bool)mode.RESULT?.Any()) //режим не рассчитан
                                continue;
                            var intervals = methodsEntities.RESULT.Where(p => p.RES_MODE_ID == mode.MODE_ID);
                            //максимальные значения в режиме по интервалам
                            foreach (var i in intervals)
                            {
                                if (R2p_1mode < i.RES_R2_PORTABLE_1)
                                {
                                    R2p_1mode = i.RES_R2_PORTABLE_1;
                                    R2ip_1mode = i.RES_I;
                                }
                                if (R2p_2mode < i.RES_R2_PORTABLE_2)
                                {
                                    R2p_2mode = i.RES_R2_PORTABLE_2;
                                    R2ip_2mode = i.RES_I;
                                }
                                if (R2p_3mode < i.RES_R2_PORTABLE_3)
                                {
                                    R2p_3mode = i.RES_R2_PORTABLE_3;
                                    R2ip_3mode = i.RES_I;
                                }

                                if (R2d_1mode < i.RES_R2_PORTABLE_DRIVE_1)
                                {
                                    R2d_1mode = i.RES_R2_PORTABLE_DRIVE_1;
                                    R2id_1mode = i.RES_I;
                                }
                                if (R2d_2mode < i.RES_R2_PORTABLE_DRIVE_2)
                                {
                                    R2d_2mode = i.RES_R2_PORTABLE_DRIVE_2;
                                    R2id_2mode = i.RES_I;
                                }
                                if (R2d_3mode < i.RES_R2_PORTABLE_DRIVE_3)
                                {
                                    R2d_3mode = i.RES_R2_PORTABLE_DRIVE_3;
                                    R2id_3mode = i.RES_I;
                                }

                                if (R2c_1mode < i.RES_R2_PORTABLE_CARRY_1)
                                {
                                    R2c_1mode = i.RES_R2_PORTABLE_CARRY_1;
                                    R2ic_1mode = i.RES_I;
                                }
                                if (R2c_2mode < i.RES_R2_PORTABLE_CARRY_2)
                                {
                                    R2c_2mode = i.RES_R2_PORTABLE_CARRY_2;
                                    R2ic_2mode = i.RES_I;
                                }
                                if (R2c_3mode < i.RES_R2_PORTABLE_CARRY_3)
                                {
                                    R2c_3mode = i.RES_R2_PORTABLE_CARRY_3;
                                    R2ic_3mode = i.RES_I;
                                }

                                   
                                if (R1s_1mode < i.RES_R1_SOSR_1)
                                {
                                    R1s_1mode = i.RES_R1_SOSR_1;
                                    R1is_1mode = i.RES_I;
                                }
                                if (R1s_2mode < i.RES_R1_SOSR_2)
                                {
                                    R1s_2mode = i.RES_R1_SOSR_2;
                                    R1is_2mode = i.RES_I;
                                }
                                if (R1s_3mode < i.RES_R1_SOSR_3)
                                {
                                    R1s_3mode = i.RES_R1_SOSR_3;
                                    R1is_3mode = i.RES_I;
                                }
                                                          
                            }
                            DataForAnalyse.Add(new DataForAnalyse
                            {
                                ID = mode.MODE_ID,
                                parentID = arm.ARM_ID,
                                Name = mode.MODE_TYPE.MT_NAME,
                                R2portable_1 = R2p_1mode,
                                R2portable_2 = R2p_2mode,
                                R2portable_3 = R2p_3mode,
                                Int_R2portable_1 = R2ip_1mode,
                                Int_R2portable_2 = R2ip_2mode,
                                Int_R2portable_3 = R2ip_3mode,

                                R2drive_1 = R2d_1mode,
                                R2drive_2 = R2d_2mode,
                                R2drive_3 = R2d_3mode,
                                Int_R2drive_1 = R2id_1mode,
                                Int_R2drive_2 = R2id_2mode,
                                Int_R2drive_3 = R2id_3mode,

                                R2carry_1 = R2c_1mode,
                                R2carry_2 = R2c_2mode,
                                R2carry_3 = R2c_3mode,
                                Int_R2carry_1 = R2ic_1mode,
                                Int_R2carry_2 = R2ic_2mode,
                                Int_R2carry_3 = R2ic_3mode,
                                
                                R1sosr_1 = R1s_1mode,
                                Int_R1sosr_1 = R1is_1mode,
                                R1sosr_2 = R1s_2mode,
                                Int_R1sosr_2 = R1is_2mode,
                                R1sosr_3 = R1s_3mode,
                                Int_R1sosr_3 = R1is_3mode
                            });
                            //максимальные значения в АРМ
                            R2p_1arm = R2p_1arm < R2p_1mode ? R2p_1mode : R2p_1arm;
                            R2d_1arm = R2d_1arm < R2d_1mode ? R2d_1mode : R2d_1arm;
                            R2c_1arm = R2c_1arm < R2c_1mode ? R2c_1mode : R2c_1arm;

                            R2p_2arm = R2p_2arm < R2p_2mode ? R2p_2mode : R2p_2arm;
                            R2d_2arm = R2d_2arm < R2d_2mode ? R2d_2mode : R2d_2arm;
                            R2c_2arm = R2c_2arm < R2c_2mode ? R2c_2mode : R2c_2arm;

                            R2p_3arm = R2p_3arm < R2p_3mode ? R2p_3mode : R2p_3arm;
                            R2d_3arm = R2d_3arm < R2d_3mode ? R2d_3mode : R2d_3arm;
                            R2c_3arm = R2c_3arm < R2c_3mode ? R2c_3mode : R2c_3arm;

                            R1s_1arm = R1s_1arm < R1s_1mode ? R1s_1mode : R1s_1arm;
                            R1s_2arm = R1s_2arm < R1s_2mode ? R1s_2mode : R1s_2arm;
                            R1s_3arm = R1s_3arm < R1s_3mode ? R1s_3mode : R1s_3arm;

                        }
                    DataForAnalyse.Add(new DataForAnalyse
                    {
                        ID = arm.ARM_ID,
                        Name = arm.ARM_NUMBER,
                        R2portable_1 = R2p_1arm != 0 ? R2p_1arm : null,
                        R2portable_2 = R2p_2arm != 0 ? R2p_2arm : null,
                        R2portable_3 = R2p_3arm != 0 ? R2p_3arm : null,

                        R2drive_1 = R2d_1arm != 0 ? R2d_1arm : null,
                        R2drive_2 = R2d_2arm != 0 ? R2d_2arm : null,
                        R2drive_3 = R2d_3arm != 0 ? R2d_3arm : null,

                        R2carry_1 = R2c_1arm != 0 ? R2c_1arm : null,
                        R2carry_2 = R2c_2arm != 0 ? R2c_2arm : null,
                        R2carry_3 = R2c_3arm != 0 ? R2c_3arm : null,

                        R1sosr_1 = R1s_1arm != 0 ? R1s_1arm : null,
                        R1sosr_2 = R1s_2arm != 0 ? R1s_2arm : null,
                        R1sosr_3 = R1s_3arm != 0 ? R1s_3arm : null,
                    });
                    CellStyleConverter.R2portable_1_MAX = Math.Max(CellStyleConverter.R2portable_1_MAX,(double)R2p_1arm);
                    CellStyleConverter.R2portable_2_MAX = Math.Max(CellStyleConverter.R2portable_2_MAX, (double)R2p_2arm);
                    CellStyleConverter.R2portable_3_MAX = Math.Max(CellStyleConverter.R2portable_3_MAX, (double)R2p_3arm);

                    CellStyleConverter.R2drive_1_MAX = Math.Max(CellStyleConverter.R2drive_1_MAX, (double)R2d_1arm);
                    CellStyleConverter.R2drive_2_MAX = Math.Max(CellStyleConverter.R2drive_2_MAX, (double)R2d_2arm);
                    CellStyleConverter.R2drive_3_MAX = Math.Max(CellStyleConverter.R2drive_3_MAX, (double)R2d_3arm);

                    CellStyleConverter.R2carry_1_MAX = Math.Max(CellStyleConverter.R2carry_1_MAX, (double)R2c_1arm);
                    CellStyleConverter.R2carry_2_MAX = Math.Max(CellStyleConverter.R2carry_2_MAX, (double)R2c_2arm);
                    CellStyleConverter.R2carry_3_MAX = Math.Max(CellStyleConverter.R2carry_3_MAX, (double)R2c_3arm);

                    CellStyleConverter.R1sosr_1_MAX = Math.Max(CellStyleConverter.R1sosr_1_MAX, (double)R1s_1arm);
                    CellStyleConverter.R1sosr_2_MAX = Math.Max(CellStyleConverter.R1sosr_2_MAX, (double)R1s_2arm);
                    CellStyleConverter.R1sosr_3_MAX = Math.Max(CellStyleConverter.R1sosr_3_MAX, (double)R1s_3arm);


                }
            }
        }
    }
   
    class DataForAnalyse
    {
        public int ID { get; set; }
        public int parentID { get; set; }
        public string Name { get; set; }
        //
        public int Int_R2portable_1 { get; set; } //интервал с макс.значением
        public double? R2portable_1 { get; set; }
        public int Int_R2drive_1 { get; set; } //интервал с макс.значением
        public double? R2drive_1 { get; set; }
        public int Int_R2carry_1 { get; set; } //интервал с макс.значением
        public double? R2carry_1 { get; set; }
        public int Int_R1sosr_1 { get; set; } //интервал с макс.значением
        public double? R1sosr_1 { get; set; }
        public int Int_R2portable_2 { get; set; } //интервал с макс.значением
        public double? R2portable_2 { get; set; }

        public int Int_R2drive_2 { get; set; } //интервал с макс.значением
        public double? R2drive_2 { get; set; }
        public int Int_R2carry_2 { get; set; } //интервал с макс.значением
        public double? R2carry_2 { get; set; }
        public int Int_R1sosr_2 { get; set; } //интервал с макс.значением
        public double? R1sosr_2 { get; set; }

        public int Int_R2portable_3 { get; set; } //интервал с макс.значением
        public double? R2portable_3 { get; set; }

        public int Int_R2drive_3 { get; set; } //интервал с макс.значением
        public double? R2drive_3 { get; set; }


        public int Int_R2carry_3 { get; set; } //интервал с макс.значением
        public double? R2carry_3 { get; set; }

        public int Int_R1sosr_3 { get; set; } //интервал с макс.значением
        public double? R1sosr_3 { get; set; }

        
    }

}
