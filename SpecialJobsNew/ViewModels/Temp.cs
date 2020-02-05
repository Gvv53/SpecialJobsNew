using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialJobs.ViewModels
{
    class Temp
    {
        private void CalculateMode(MODE mode)
        {
            SaveData(null); //сохраним все изменения

            //удалим предыдущий расчёт для выбранного режима
            ModeResultClear(mode);
            RESULT result;
            double E_schuma_stationary;
            double E_schuma_portableDrive;
            double E_schuma_portableCarry;

            double E_schumaSosr;
            double E_schumaRaspr;

            double signal_i, k_zatuchanija_i;

            double E_schuma_SAZ;

            double NORMA_1 = 0, NORMA_2 = 0, NORMA_3 = 0;

            //расчёт по электрическому полю
            //все режимы активного АРМа        
            if (!keySuspend) //функция вызвана с работающим фоновым потоком
                dbChenged.Suspend();
            //данные измерений режима
            ObservableCollection<MEASURING_DATA> measurings =
                new ObservableCollection<MEASURING_DATA>(
                    methodsEntities.MEASURING_DATA.Where(p => p.MDA_MODE_ID == mode.MODE_ID));
            if (!measurings.Any()) //отсутствуют данные измерений
                return;

            if (mode.MODE_TAU_UNIT_ID == 0 || mode.MODE_FT_UNIT_ID == 0 ||
                mode.MODE_TAU == 0 || mode.MODE_FT == 0 || mode.MODE_D == 0 ||
                mode.MODE_R == 0 || mode.MODE_RMAX == 0)
            {
                MessageBox.Show("Не все параметры режима " + mode.MODE_TYPE.MT_NAME +
                                " заполнены. Расчёт для этого режима не может быть выполнен.");
                return;
            }
            //расчёт нормы для заданной категории выбранного режима
            mode.MODE_NORMA = Functions.Sigma(mode, arm_one);
            if (mode.MODE_NORMA == 0)
                return;
            if (mode.MODE_NORMA == -1)
                return;

            //нормы для 1-3 категорий выбранного режима
            NORMA_1 = Functions.Sigma_1_2_3(mode, arm_one, 1);
            if (NORMA_1 == -1)
                return;
            NORMA_2 = Functions.Sigma_1_2_3(mode, arm_one, 2);
            if (NORMA_2 == -1)
                return;
            NORMA_3 = Functions.Sigma_1_2_3(mode, arm_one, 3);
            if (NORMA_3 == -1)
                return;
            //величины, приведённые к кГц и нсек
            double tau = Functions.Tau_nsek(mode.MODE_TAU,
                Functions.GetUnitValue(Units, (int)mode.MODE_TAU_UNIT_ID));
            double ft = Functions.F_kGc(mode.MODE_FT,
                Functions.GetUnitValue(Units, (int)mode.MODE_FT_UNIT_ID));
            double rbw = Functions.F_kGc(mode.MODE_RBW,
                Functions.GetUnitValue(Units, (int)mode.MODE_RBW_UNIT_ID));
            result = new RESULT();
            int[] arR2, arR2_1, arR2_2, arR2_3;
            decimal[] arR1, arR1_1, arR1_2, arR1_3;

            //все значения i для E
            var iListE = measurings.Where(d => d.MDA_F != 0 && d.MDA_E != 0 && d.ANTENNA != null && d.ANTENNA.ANT_TYPE.Contains("Электр")).Select(p => p.MDA_I).Distinct();
            //расчёт показателя защищённости для каждого интервала i            
            foreach (int i in iListE)
            {
                //нормированная величина напряжённости шума(знаменатель формулы), используется и для R2
                E_schuma_stationary = Functions.E_schuma_Num(i, tau, "Стационарное", armKategoria);
                E_schuma_portableDrive = Functions.E_schuma_Num(i, tau, "Портативное возимое", armKategoria);
                E_schuma_portableCarry = Functions.E_schuma_Num(i, tau, "Портативное носимое", armKategoria);

                var list = measurings.Where(p => p.MDA_I == i && p.ANTENNA != null && p.ANTENNA.ANT_TYPE.Contains("Электр")); //данные измерений в интервале
                                                                                                                              //показатель напряжённости опасного сигнала в интервале (числитель формулы)
                if (mode.MODE_IS_SOLID)
                    signal_i = Functions.E_c_solid(list, Units);
                else
                    signal_i = Functions.E_c_diff(list);

                if (signal_i == 0)
                    continue;
                k_zatuchanija_i = Functions.K_zatuchanija_i(mode.MODE_D, mode.MODE_R, i, tau);
                if (k_zatuchanija_i == 0)
                    continue;
                //напряжённость для расчёта R1 по эл полю
                E_schumaSosr = Functions.EH_schumaAnt_Num(i, tau, true, true);
                E_schumaRaspr = Functions.EH_schumaAnt_Num(i, tau, false, true);
                //напряжённость Е от САЗ                
                E_schuma_SAZ = Functions.E_schumaSAZ_New(i, tau, list, true, rbw, (bool)RBW_EH);

                arR2 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, mode.MODE_NORMA, tau, list, true);

                arR2_1 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_1, tau, list, true);

                arR2_2 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_2, tau, list, true);

                arR2_3 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_3, tau, list, true);


                //расчёт зон R1  для E
                var listR1E = list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 300000); //данные измерений в интервале c частотой < 300000кГц 
                arR1 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, mode.MODE_NORMA, tau, listR1E,
                    true, mode.MODE_D);
                arR1_1 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_1, tau, listR1E,
                    true, mode.MODE_D);
                arR1_2 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_2, tau, listR1E,
                    true, mode.MODE_D);
                arR1_3 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_3, tau, listR1E,
                    true, mode.MODE_D);

                result = new RESULT()
                {
                    RES_MODE_ID = mode.MODE_ID,
                    RES_TYPE = "E",
                    RES_NORMA = mode.MODE_NORMA,
                    RES_NORMA_1 = NORMA_1,
                    RES_NORMA_2 = NORMA_2,
                    RES_NORMA_3 = NORMA_3,
                    RES_I_ZATUCHANIJA = k_zatuchanija_i,
                    RES_I = i,
                    RES_SIGNAL = Math.Round(signal_i, 4),
                    RES_DELTA_PORTABLE =
                        E_schuma_stationary != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_stationary, 3)
                            : 0,

                    RES_DELTA_PORTABLE_CARRY =
                        E_schuma_portableCarry != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_portableCarry, 3)
                            : 0,

                    RES_DELTA_PORTABLE_DRIVE =
                        E_schuma_portableDrive != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / k_zatuchanija_i / E_schuma_portableDrive, 3)
                            : 0,

                    RES_SAZ = E_schuma_SAZ == 0
                        ? 0
                        : Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / mode.MODE_KS / E_schuma_SAZ, 4),
                    RES_R2_PORTABLE = arR2[0],
                    RES_R2_PORTABLE_DRIVE = arR2[1],
                    RES_R2_PORTABLE_CARRY = arR2[2],
                    RES_R2_PORTABLE_1 = arR2_1[0],
                    RES_R2_PORTABLE_DRIVE_1 = arR2_1[1],
                    RES_R2_PORTABLE_CARRY_1 = arR2_1[2],
                    RES_R2_PORTABLE_2 = arR2_2[0],
                    RES_R2_PORTABLE_DRIVE_2 = arR2_2[1],
                    RES_R2_PORTABLE_CARRY_2 = arR2_2[2],
                    RES_R2_PORTABLE_3 = arR2_3[0],
                    RES_R2_PORTABLE_DRIVE_3 = arR2_3[1],
                    RES_R2_PORTABLE_CARRY_3 = arR2_3[2],

                    RES_R1_SOSR = (double)arR1[0],
                    RES_R1_RASPR = (double)arR1[1],
                    RES_R1_SOSR_1 = (double)arR1_1[0],
                    RES_R1_RASPR_1 = (double)arR1_1[1],
                    RES_R1_SOSR_2 = (double)arR1_2[0],
                    RES_R1_RASPR_2 = (double)arR1_2[1],
                    RES_R1_SOSR_3 = (double)arR1_3[0],
                    RES_R1_RASPR_3 = (double)arR1_3[1]
                };

                methodsEntities.RESULT.Add(result);
            }
            //расчёт по магнитному полю
            //все значения i для H
            var iListH = measurings.Where(d => d.MDA_F != 0 && d.MDA_E != 0 && d.ANTENNA != null && d.ANTENNA.ANT_TYPE.Contains("Магнит")).Select(p => p.MDA_I).Distinct();

            //расчёт показателя защищённости для каждого интервала i            
            foreach (int i in iListH)
            {
                //нормированная величина напряжённости шума(знаменатель формулы), используется и для R2
                E_schuma_stationary = Functions.E_schuma_Num(i, tau, "Стационарное", armKategoria);
                E_schuma_portableDrive = Functions.E_schuma_Num(i, tau, "Портативное возимое", armKategoria);
                E_schuma_portableCarry = Functions.E_schuma_Num(i, tau, "Портативное носимое", armKategoria);

                var list = measurings.Where(p => p.MDA_I == i && p.ANTENNA != null && p.ANTENNA.ANT_TYPE.Contains("Магнит")); //данные измерений в интервале
                                                                                                                              //показатель напряжённости опасного сигнала в интервале (числитель формулы)
                signal_i = Functions.E_c_diff(list);//данные измерений располагаются в одном месте, отличаются только типом антенны
                if (signal_i == 0)
                    continue;

                //напряжённость для расчёта R1 по эл полю
                E_schumaSosr = Functions.EH_schumaAnt_Num(i, tau, true, false);
                E_schumaRaspr = Functions.EH_schumaAnt_Num(i, tau, false, false);
                //напряжённость Е от САЗ                
                E_schuma_SAZ = Functions.E_schumaSAZ_New(i, tau, list, false, rbw, (bool)RBW_EH);

                arR2 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, mode.MODE_NORMA, tau, list, false);

                arR2_1 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_1, tau, list, false);

                arR2_2 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_2, tau, list, false);

                arR2_3 = Functions.R2(ft, (double)mode.MODE_TYPE.MT_KN, E_schuma_stationary, E_schuma_portableDrive,
                    E_schuma_portableCarry, NORMA_3, tau, list, false);


                //расчёт зон R1  для E
                var listR1H = list.Where(p => Functions.F_kGc(p.MDA_F, Functions.GetUnitValue(Units, p.MDA_F_UNIT_ID)) <= 10000); //данные измерений в интервале c частотой < 10000кГц 
                arR1 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, mode.MODE_NORMA, tau, listR1H,
                    false, mode.MODE_D);
                arR1_1 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_1, tau, list,
                    false, mode.MODE_D);
                arR1_2 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_2, tau, list,
                    false, mode.MODE_D);
                arR1_3 = Functions.R1(ft, (double)mode.MODE_TYPE.MT_KN, E_schumaSosr, E_schumaRaspr, NORMA_3, tau, list,
                    false, mode.MODE_D);

                result = new RESULT()
                {
                    RES_MODE_ID = mode.MODE_ID,
                    RES_TYPE = "H",
                    RES_NORMA = mode.MODE_NORMA,
                    RES_NORMA_1 = NORMA_1,
                    RES_NORMA_2 = NORMA_2,
                    RES_NORMA_3 = NORMA_3,
                    RES_I = i,
                    RES_SIGNAL = Math.Round(signal_i, 4),
                    RES_DELTA_PORTABLE =
                        E_schuma_stationary != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / E_schuma_stationary, 3)
                            : 0,

                    RES_DELTA_PORTABLE_CARRY =
                        E_schuma_portableCarry != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / E_schuma_portableCarry, 3)
                            : 0,

                    RES_DELTA_PORTABLE_DRIVE =
                        E_schuma_portableDrive != 0
                            ? Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / E_schuma_portableDrive, 3)
                            : 0,

                    RES_SAZ = E_schuma_SAZ == 0
                        ? 0
                        : Math.Round(signal_i / (double)mode.MODE_TYPE.MT_KN / mode.MODE_KS / E_schuma_SAZ, 4),
                    RES_R2_PORTABLE = arR2[0],
                    RES_R2_PORTABLE_DRIVE = arR2[1],
                    RES_R2_PORTABLE_CARRY = arR2[2],
                    RES_R2_PORTABLE_1 = arR2_1[0],
                    RES_R2_PORTABLE_DRIVE_1 = arR2_1[1],
                    RES_R2_PORTABLE_CARRY_1 = arR2_1[2],
                    RES_R2_PORTABLE_2 = arR2_2[0],
                    RES_R2_PORTABLE_DRIVE_2 = arR2_2[1],
                    RES_R2_PORTABLE_CARRY_2 = arR2_2[2],
                    RES_R2_PORTABLE_3 = arR2_3[0],
                    RES_R2_PORTABLE_DRIVE_3 = arR2_3[1],
                    RES_R2_PORTABLE_CARRY_3 = arR2_3[2],

                    RES_R1_SOSR = (double)arR1[0],
                    RES_R1_RASPR = (double)arR1[1],
                    RES_R1_SOSR_1 = (double)arR1_1[0],
                    RES_R1_RASPR_1 = (double)arR1_1[1],
                    RES_R1_SOSR_2 = (double)arR1_2[0],
                    RES_R1_RASPR_2 = (double)arR1_2[1],
                    RES_R1_SOSR_3 = (double)arR1_3[0],
                    RES_R1_RASPR_3 = (double)arR1_3[1]
                };

                methodsEntities.RESULT.Add(result);

            }

            methodsEntities.SaveChanges();

            MakeMaxValue();
            Results = new ObservableCollection<RESULT>(
                     methodsEntities.RESULT.Where(p => p.RES_MODE_ID == selectedMode.MODE_ID));
            if (RefreshGcResultsScen != null)
                RefreshGcResultsScen();
            if (!Results.Any())
            {
                MessageBox.Show("Проверьте справочник антенн. Во всех строках должен быть указан правильный тип: Электрическая или Магнитная");
            }
            if (!keySuspend) // функция вызывалась с работающим фоновым потоком
                dbChenged.Resume();
         
        }
    }
}
