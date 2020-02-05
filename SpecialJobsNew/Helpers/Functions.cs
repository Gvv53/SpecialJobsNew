using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Microsoft.Win32;


namespace SpecialJobs.Helpers
{

    public class Functions
    {
        public static ObservableCollection<TableNose> tableNose1st, tableNose1drive, tableNose1carry;
        public static ObservableCollection<TableNose> tableNose23st, tableNose23drive, tableNose23carry;
        public static ObservableCollection<TableNose> tableCurrent, tableNoseAntE, tableNoseAntH;
        #region Расчётные функции
        //получение ID из UNIT по названию
        public static int GetUnitID(ObservableCollection<UNIT> Units, string value)
        {
            if (Units.Where(p => p.UNIT_VALUE == value).FirstOrDefault() == null)
                return 0;
            else
                return Units.Where(p => p.UNIT_VALUE == value).FirstOrDefault().UNIT_ID;
        }
        //получение названия  из UNIT по ID
        public static string GetUnitValue(ObservableCollection<UNIT> Units, int Id)
        {
            if (Units.Where(p => p.UNIT_ID == Id).FirstOrDefault() == null)
                return "";
            else
                return Units.Where(p => p.UNIT_ID == Id).FirstOrDefault().UNIT_VALUE;
        }

        //конвертация из дБ в мкВ
        public static double DbmkV(double dB)
        {
            if (Double.IsPositiveInfinity(Math.Pow(10, 0.05 * dB)))
            {
                MessageBox.Show("Значение измерения " + dB.ToString() + " выходит за пределы допустимого.");
                return -1;
            }
            else
               return Math.Round(Math.Pow(10, 0.05 * dB), 3);
        }
        //конвертация из мкВ в дБ
        //public static double mkVdb(double mkV)
        //{
        //    return Math.Round(20 * Math.Log10(mkV), 3);
        //}
        //приведение частоты к кГц
        public static double F_kGc(double F, string edIzm)
        {
            return (edIzm == "кГц" ? F : (edIzm == "Гц" ? F / 1000 : F * 1000));
        }
        //приведение частоты к мГц
        public static double F_mGc(double F, string edIzm)
        {
            return (edIzm == "мГц" ? F : (edIzm == "кГц" ? F / 1000 : F * 1000000));
        }
        //приведение времени к нсек
        public static double Tau_nsek(double Tau, string edIzm)
        {
            return edIzm == "нсек" ? Tau : (edIzm == "мксек" ? Tau * 1000 : (edIzm == "мсек" ? Tau * 1000000 : Tau * 1000000000));
        }

        //категорированные нормы Сигма для СВТ       
        public static double Sigma(MODE mode, ARM arm)
        {

            if (mode.MODE_TYPE == null)
            {
                MessageBox.Show("Выберите режим исследования ТС");
                return -1;
            }
            if (mode.MODE_TYPE.MT_ISVISILABLE && arm.ARM_KATEGORIA >= 2)
                return 0.1;
            double sigma = 0;
            if (arm.ARM_SVT == "Универсальное")
            {
                if (arm.ARM_KATEGORIA == 0)
                {
                    MessageBox.Show("Заполниете поле 'Категория'");
                    return 0;
                }
                if (arm.ARM_KATEGORIA == 1)
                    sigma = 0.2;
                if (arm.ARM_KATEGORIA == 2)
                {
                    switch (mode.MODE_TYPE.MT_CK)
                    {
                        case "Последовательный":
                            sigma = 0.7;
                            break;
                        case "Параллельный":
                            sigma = 1;
                            break;
                        case "УсловноПараллельный":
                            if (mode.MODE_TYPE.MT_M == 1.0)
                                sigma = 1;
                            if (mode.MODE_TYPE.MT_M < 1 && mode.MODE_TYPE.MT_M >= 0.9)
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_M < 0.9 && mode.MODE_TYPE.MT_M >= 0.25)
                                sigma = 0.8;
                            if (mode.MODE_TYPE.MT_M < 0.25 && mode.MODE_TYPE.MT_M >= 0)
                                sigma = 0.7;
                            break;
                    }
                }
                if (arm.ARM_KATEGORIA == 3)
                {
                    switch (mode.MODE_TYPE.MT_CK)
                    {
                        case "Последовательный":
                            sigma = 1.2;
                            break;
                        case "Параллельный":
                            sigma = 4;
                            break;
                        case "Условно-параллельный":
                            if (mode.MODE_TYPE.MT_M <= 1 && mode.MODE_TYPE.MT_M >= 0.95)
                                sigma = 4;
                            if (mode.MODE_TYPE.MT_M < 0.95 && mode.MODE_TYPE.MT_M >= 0.9)
                                sigma = 3;
                            if (mode.MODE_TYPE.MT_M < 0.9 && mode.MODE_TYPE.MT_M >= 0.5)
                                sigma = 1.6;
                            if (mode.MODE_TYPE.MT_M < 0.5 && mode.MODE_TYPE.MT_M >= 0.25)
                                sigma = 1.4;
                            if (mode.MODE_TYPE.MT_M < 0.25 && mode.MODE_TYPE.MT_M >= 0)
                                sigma = 1.2;
                            break;
                    }
                }
                if (mode.MODE_TYPE.MT_NAME.Contains("Вывод на монитор") && arm.ARM_KATEGORIA == 1)
                    return Math.Round(sigma / 16, 3);
                return sigma;
            }
            else
            {

                if (arm.ARM_TT == "Восстановление графических документов")
                {
                    if (mode.MODE_TYPE.MT_CK != "Условно-параллельный")

                        switch (arm.ARM_IK)
                        {
                            case "РЛИ":
                                if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                    sigma = 0.1;
                                else
                                    sigma = 0.04;
                                break;
                            case "СЛИ":
                                sigma = 0.2;
                                break;
                            case "СЛИП":
                                sigma = 0.5;
                                break;
                            case "":
                                MessageBox.Show("Выберите значение 'Тип перехватываемого графического документа'. ");
                                sigma = 0;
                                break;
                        }
                }
                if (arm.ARM_TT == "Чтение текста")
                {

                    if (mode.MODE_TYPE.MT_CK == "Последовательный")
                        sigma = 0.7;
                    if (mode.MODE_TYPE.MT_CK == "Параллельный")
                        sigma = 1;
                    else
                    {
                        if (mode.MODE_TYPE.MT_M == 1)
                            sigma = 1;
                        if (mode.MODE_TYPE.MT_M == 0.95 || mode.MODE_TYPE.MT_M == 0.9)
                            sigma = 0.9;
                        if (mode.MODE_TYPE.MT_M == 0.5 || mode.MODE_TYPE.MT_M == 0.25)
                            sigma = 0.8;
                        if (mode.MODE_TYPE.MT_M == 0)
                            sigma = 0.7;
                    }
                }
                if (arm.ARM_TT == "Восстановление буквенно цифровых сообщений")
                {
                    sigma = 0.2;
                }
                if (arm.ARM_TT == "Чтение чисел")
                {
                    switch (arm.ARM_NK)
                    {
                        case "КГЧ1":
                            sigma = 0.3;
                            break;
                        case "МКГЧД":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 2.6;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M > 0.9)
                                    sigma = 2.6;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 2;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1.1;
                                if (mode.MODE_TYPE.MT_M == 0.25)
                                    sigma = 1;
                                if (mode.MODE_TYPE.MT_M == 0)
                                    sigma = 0.9;
                            }
                            break;
                        case "КО":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 1.3;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M == 1)
                                    sigma = 1.3;
                                if (mode.MODE_TYPE.MT_M == 0.95)
                                    sigma = 1.2;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 1.1;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1;
                                if (mode.MODE_TYPE.MT_M <= 0.25)
                                    sigma = 0.9;
                            }
                            break;
                        case "МКО":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 1.2;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 4;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M > 0.9)
                                    sigma = 4;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 3;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1.6;
                                if (mode.MODE_TYPE.MT_M == 0.25)
                                    sigma = 1.3;
                                if (mode.MODE_TYPE.MT_M == 0)
                                    sigma = 1.2;
                            }
                            break;
                    }
                }
                if (mode.MODE_TYPE.MT_NAME.Contains("Вывод на монитор") && arm.ARM_KATEGORIA == 1)
                    return Math.Round(sigma / 16, 3);
                return Math.Round(sigma, 3);
            }
        }

        //норма для заданной категории
        public static double Sigma_1_2_3(MODE mode, ARM arm, int kategoria)
        {
            if (mode.MODE_TYPE == null)
            {
                MessageBox.Show("Выберите режим исследования ТС");
                return -1;
            }
            if (mode.MODE_TYPE.MT_ISVISILABLE && kategoria >= 2)
                return 0.1;
            double sigma = 0;
            if (arm.ARM_SVT == "Универсальное")
            {
                if (kategoria == 1)
                    sigma = 0.2;
                if (kategoria == 2)
                {
                    switch (mode.MODE_TYPE.MT_CK)
                    {
                        case "Последовательный":
                            sigma = 0.7;
                            break;
                        case "Параллельный":
                            sigma = 1;
                            break;
                        case "УсловноПараллельный":
                            if (mode.MODE_TYPE.MT_M == 1.0)
                                sigma = 1;
                            if (mode.MODE_TYPE.MT_M < 1 && mode.MODE_TYPE.MT_M >= 0.9)
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_M < 0.9 && mode.MODE_TYPE.MT_M >= 0.25)
                                sigma = 0.8;
                            if (mode.MODE_TYPE.MT_M < 0.25 && mode.MODE_TYPE.MT_M >= 0)
                                sigma = 0.7;
                            break;
                    }
                }
                if (kategoria == 3)
                {
                    switch (mode.MODE_TYPE.MT_CK)
                    {
                        case "Последовательный":
                            sigma = 1.2;
                            break;
                        case "Параллельный":
                            sigma = 4;
                            break;
                        case "Условно-параллельный":
                            if (mode.MODE_TYPE.MT_M <= 1 && mode.MODE_TYPE.MT_M >= 0.95)
                                sigma = 4;
                            if (mode.MODE_TYPE.MT_M < 0.95 && mode.MODE_TYPE.MT_M >= 0.9)
                                sigma = 3;
                            if (mode.MODE_TYPE.MT_M < 0.9 && mode.MODE_TYPE.MT_M >= 0.5)
                                sigma = 1.6;
                            if (mode.MODE_TYPE.MT_M < 0.5 && mode.MODE_TYPE.MT_M >= 0.25)
                                sigma = 1.4;
                            if (mode.MODE_TYPE.MT_M < 0.25 && mode.MODE_TYPE.MT_M >= 0)
                                sigma = 1.2;
                            break;
                    }
                }
                if (mode.MODE_TYPE.MT_ISVISILABLE && kategoria == 1)
                    return Math.Round(sigma / 16, 3);
                return sigma;
            }
            else
            {

                if (arm.ARM_TT == "Восстановление графических документов")
                {
                    if (mode.MODE_TYPE.MT_CK != "Условно-параллельный")

                        switch (arm.ARM_IK)
                        {
                            case "РЛИ":
                                if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                    sigma = 0.1;
                                else
                                    sigma = 0.04;
                                break;
                            case "СЛИ":
                                sigma = 0.2;
                                break;
                            case "СЛИП":
                                sigma = 0.5;
                                break;
                            case "":
                                MessageBox.Show("Выберите значение 'Тип перехватываемого графического документа'. ");
                                sigma = 0;
                                break;
                        }
                }
                if (arm.ARM_TT == "Чтение текста")
                {

                    if (mode.MODE_TYPE.MT_CK == "Последовательный")
                        sigma = 0.7;
                    if (mode.MODE_TYPE.MT_CK == "Параллельный")
                        sigma = 1;
                    else
                    {
                        if (mode.MODE_TYPE.MT_M == 1)
                            sigma = 1;
                        if (mode.MODE_TYPE.MT_M == 0.95 || mode.MODE_TYPE.MT_M == 0.9)
                            sigma = 0.9;
                        if (mode.MODE_TYPE.MT_M == 0.5 || mode.MODE_TYPE.MT_M == 0.25)
                            sigma = 0.8;
                        if (mode.MODE_TYPE.MT_M == 0)
                            sigma = 0.7;
                    }
                }
                if (arm.ARM_TT == "Восстановление буквенно цифровых сообщений")
                {
                    sigma = 0.2;
                }
                if (arm.ARM_TT == "Чтение чисел")
                {
                    switch (arm.ARM_NK)
                    {
                        case "КГЧ1":
                            sigma = 0.3;
                            break;
                        case "МКГЧД":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 2.6;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M > 0.9)
                                    sigma = 2.6;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 2;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1.1;
                                if (mode.MODE_TYPE.MT_M == 0.25)
                                    sigma = 1;
                                if (mode.MODE_TYPE.MT_M == 0)
                                    sigma = 0.9;
                            }
                            break;
                        case "КО":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 0.9;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 1.3;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M == 1)
                                    sigma = 1.3;
                                if (mode.MODE_TYPE.MT_M == 0.95)
                                    sigma = 1.2;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 1.1;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1;
                                if (mode.MODE_TYPE.MT_M <= 0.25)
                                    sigma = 0.9;
                            }
                            break;
                        case "МКО":
                            if (mode.MODE_TYPE.MT_CK == "Последовательный")
                                sigma = 1.2;
                            if (mode.MODE_TYPE.MT_CK == "Параллельный")
                                sigma = 4;
                            else
                            {
                                if (mode.MODE_TYPE.MT_M > 0.9)
                                    sigma = 4;
                                if (mode.MODE_TYPE.MT_M == 0.9)
                                    sigma = 3;
                                if (mode.MODE_TYPE.MT_M == 0.5)
                                    sigma = 1.6;
                                if (mode.MODE_TYPE.MT_M == 0.25)
                                    sigma = 1.3;
                                if (mode.MODE_TYPE.MT_M == 0)
                                    sigma = 1.2;
                            }
                            break;
                    }
                }
                if (mode.MODE_TYPE.MT_ISVISILABLE && kategoria == 1)
                    return Math.Round(sigma / 16, 3);
                return Math.Round(sigma, 3);
            }
        }
        // интервальное затухание по новой формуле затухания(приложение Г новой методики)
        //с шагом = 1 кГц
        public static double K_zatuchanija_i_1(double d, double R, int i, double tau)
        {
            if (d == R)
                return 1;
            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = i == 1 ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;
            double step = 1;
            //double step = (f_top - f_law) / 100;
            double f = f_law; //начальная частота в интервале
            double K_zat = 0;  //затухание на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций

            while (f <= f_top)
            {
                K_zat = Functions.K_zatuchanija_G(d, R, f); //затухание на частоте, дБ
                if (f == f_law || f == f_top)
                    summa += Math.Pow(K_zat, 2) / 2;   //сумма квадратов, мкВ для 1-й и последней частоты в интервале
                else
                    summa += Math.Pow(K_zat, 2);       //сумма квадратов, мкВ                   
                f++; //частота очередного шага
            }
            return Math.Round(Math.Pow(summa * step / (f_top - f_law), 0.5), 3); //квадратный корень из интеграла, мкВ
        }
        //ИНтервальное затухание с шагом интегр-я = 1 или 1/100 интервала, в зав-ти от частоты 
        public static double K_zatuchanija_i(double d, double R, int i, double tau)
        {
            if (d == R)
                return 1;
            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = i == 1 ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;

            //if (i == 1 || f_law < 100000)
            //    return K_zatuchanija_i_1(d, R, i, tau);

           //double step = (f_top - f_law) / 100;
            double f = f_law; //начальная частота в интервале
            double K_zat = 0;  //затухание на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            double step = 0;
            while (f <= f_top)
            {
                if (step == 0 && f < 100000)
                    step = 1;
                else
                {
                    if (step <= 1 && (f + step) >= 100000)
                    {
                        step = (f_top - f_law) / 100;
                    }
                }
                K_zat = Functions.K_zatuchanija_G(d, R, f); //затухание на частоте, дБ
                if (f == f_law || f == f_top)
                    summa += Math.Pow(K_zat, 2) / 2 * step;   //сумма квадратов, мкВ для 1-й и последней частоты в интервале
                else
                    summa += Math.Pow(K_zat, 2) * step;       //сумма квадратов, мкВ   
                if (f == f_top)
                    break;
                if ((f + step) > f_top)
                    f = f_top;
                else
                    f += step; //частота очередного шага
            }
            return Math.Round(Math.Pow(summa  / (f_top - f_law), 0.5), 3); //квадратный корень из интеграла, мкВ
        }
        public static double K_zatuchanija_i_prev(double d, double R, int i, double tau)
        {
            if (d == R)
                return 1;
            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = i == 1 ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;

            if (i == 1 || f_law < 100000)
                return K_zatuchanija_i_1(d, R, i, tau);

            double step = (f_top - f_law) / 100;
            double f = f_law; //начальная частота в интервале
            double K_zat = 0;  //затухание на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            int k = 1;
            while (k < 101)
            {               
                K_zat = Functions.K_zatuchanija_G(d, R, f); //затухание на частоте, дБ
                if (k == 1 || k == 101)
                    summa += Math.Pow(K_zat, 2) / 2;   //сумма квадратов, мкВ для 1-й и последней частоты в интервале
                else
                    summa += Math.Pow(K_zat, 2);       //сумма квадратов, мкВ   
                k++;
                if (k == 101)
                    f = f_top;
                else
                    f += step ; //частота очередного шага
            }
            return Math.Round(Math.Pow(summa * step / (f_top - f_law), 0.5), 3); //квадратный корень из интеграла, мкВ
        }
        // интервальное затухание по новой формуле затухания(приложение Г новой методики) Для R1
        public static double K_zatuchanija_i_R1(double d, double R, int i, double tau,bool isE)
        {
            if (d == R)
                return 1;
            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = i == 1 ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;
           //double step = 1;
            
            if (isE && f_top > 400000)
                f_top = 400000;
            if (!isE && f_top > 30000)
                f_top = 30000;
            double step = (f_top - f_law) / 100;
            double f = f_law; //начальная частота в интервале
            double K_zat = 0;  //затухание на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            int k = 1;
            while (k <= 101 )
            {
                K_zat = Functions.K_zatuchanija_G_R1(d, R, f); //затухание на частоте, дБ
                if (k == 1 || k == 101)
                    summa += Math.Pow(K_zat, 2) / 2;   //сумма квадратов, мкВ для 1-й и последней частоты в интервале
                else
                    summa += Math.Pow(K_zat, 2); 
                //сумма квадратов, мкВ                   
                f += step; //частота очередного шага
                if (k == 101)
                    f = f_top;
                else
                Math.Round(f += step); //частота очередного шага
                k++;
            }
            return Math.Round(Math.Pow(summa * step / (f_top - f_law), 0.5), 3); //квадратный корень из интеграла, мкВ
        }

        // коэффициент затухания электромагнитного поля в свободном пространстве
        public static double K_zatuchanija(double d, double R, double f)
        {
            //f - кГц
            if (d == 0 || R == 0 || f == 0)
                return 0;
            double lambda = 300000 / f; 
            if (d <= lambda / 2 / Math.PI) // т.А в ближней зоне
            {
                if (R >= d && R <= lambda / 2 / Math.PI) // т.Б в ближней зоне
                    return Math.Pow(R / d, 3);
                if (R > lambda / 2 / Math.PI && R <= 6 * lambda) // т.Б в промежуточной зоне
                    return (lambda / 2 / Math.PI) * (Math.Pow(R, 2) / Math.Pow(d, 3));
                if (R > 6 * lambda)
                    return 3 * R * Math.Pow(lambda, 2) / Math.PI / Math.Pow(d, 3); // т.Б в дальней зоне
            }
            if (d > lambda / 2 / Math.PI && d <= 6 * lambda) // т.А в промежуточной зоне
            {
                if (R > lambda / 2 / Math.PI && R <= 6 * lambda)   // т.Б в промежуточной зоне
                    return Math.Pow(R / d, 2);
                if (R > 6 * lambda)
                    return 6 * lambda * R / Math.Pow(d, 2);// т.Б в дальней зоне
            }
            if (d > 6 * lambda) // т.А  и Б в дальней зоне
                return R / d;
            return 0;   //не удалось рассчитать
        }
        //коэффициент затухания побочных электромагнитных излучений, приложение Г 
        //(новая методика с учётом доп.затухания, обусловленного ограничениями, накладываемыми на 
        //область существенную для распространения тестового сигнала)
        public static double K_zatuchanija_G(double d, double R, double f) //f кГц
        {            
            if (d == 0 || R == 0 || f == 0)
                return 0;
            if (R == 1 && d == 1)
                return 1;
            double lambda = 300000/ f; //длина волны, м
            double Kcn, Kdop, v, k = 2* Math.PI / lambda; // k-волновое число
            Kcn = Math.Pow(R, 3) / Math.Pow(d, 3) * Math.Pow((Math.Pow(k, 4) * Math.Pow(d, 4) - Math.Pow(k, 2) * Math.Pow(d, 2) + 1) /
                                               (Math.Pow(k, 4) * Math.Pow(R, 4) - Math.Pow(k, 2) * Math.Pow(R, 2) + 1), 0.5);
            v = Math.Pow(d * (R - d) * 300000 / R / f, 0.5); //радиус 1-й зоны Френеля,м
            Kdop = v <= 0.3 ? 1 : v / 0.3;
            return Math.Round(Kcn * Kdop, 3);
        }
        //коэффициент затухания для расчёта R1, приложение Г (новая методика с учётом доп.затухания, обусловленного ограничениями, наклвдываемыми на 
        //область существенную для распространения тестового сигнала)
        public static double K_zatuchanija_G_R1(double d, double R, double f) //f кГц
        {
            if (d == 0 || R == 0 || f == 0)
                return 0;
            if (R == 1 && d == 1)
                return 1;
            double lambda = 300000 / f; //длина волны, м
            double Kcn, k = 2 * Math.PI / lambda; // k-волновое число
            Kcn = Math.Pow(R, 3) / Math.Pow(d, 3) * Math.Pow((Math.Pow(k, 4) * Math.Pow(d, 4) - Math.Pow(k, 2) * Math.Pow(d, 2) + 1) /
                                               (Math.Pow(k, 4) * Math.Pow(R, 4) - Math.Pow(k, 2) * Math.Pow(R, 2) + 1), 0.5);
            return Math.Round(Kcn, 3);
        }            

        //Нормированный шум (Интергал в знаменателе для расчёта зоны R1)
        //предельная чувствительность антенн определяется по Приложение Д
        public static double EH_schumaAnt_i(int i, double tau, bool isE)
        {
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = i == 1 ? 9 : (i - 1) * 1000000 / tau; //кГц
            if (isE && f_law >= 400000 || !isE && f_law >= 30000)
                return 0;
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;

            double step = 1;
            //double step = (f_top - f_law) / 100;
            if (isE && f_top > 400000)
                f_top = 400000;
            if (!isE && f_top > 30000)
                f_top = 30000;
           
            double f = f_law; //начальная частота в интервале
            double EH_sn = 0;  //спектральная плотность напряжённости шума на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            
            while (f <= f_top)
            {
                EH_sn = Functions.EH_sn(f, i,isE); //нормированное значение шума на частоте, дБ
                if (f == f_law || f == f_top)
                    summa += Math.Pow(Math.Pow(10, EH_sn / 20), 2) / 2;   //сумма квадратов, мкВ
                else
                    summa += Math.Pow(Math.Pow(10, EH_sn / 20), 2);   //сумма квадратов, мкВ
               
                Math.Round(f += step ); //частота очередного шага

            }
            return Math.Round(Math.Pow(summa * step, 0.5), 3); //квадратный корень из интеграла, мкВ
        }        
        //предельная чувствительность случайной антенны от частоты, приложение Д
        public static double EH_sn(double f, int i, bool isE)
        {
            if (isE)
            {
                if (tableNoseAntE == null || tableNoseAntE.Any())
                {
                    tableNoseAntE = new ObservableCollection<TableNose>
                    {
                        new TableNose() { fa = 9, fb = 50, A = 74, B = 45 },
                        new TableNose() { fa = 50, fb = 3000, A = 45, B = 11 },
                        new TableNose() { fa = 3000, fb = 10000, A = 11, B = 9 },
                        new TableNose() { fa = 10000, fb = 40000, A = 9, B = 7 },
                        new TableNose() { fa = 40000, fb = 400000, A = 7, B = 3 }
                    };
                }
                tableCurrent = tableNoseAntE;
            }
            else
            { 
                if (tableNoseAntH == null || tableNoseAntH.Any())
                {
                    tableNoseAntH = new ObservableCollection<TableNose>
                    {
                        new TableNose() { fa = 9, fb = 50, A = 108, B = 88 },
                        new TableNose() { fa = 50, fb = 3000, A = 88, B = 48 },
                        new TableNose() { fa = 3000, fb = 10000, A = 48, B = 40 },
                        new TableNose() { fa = 10000, fb = 30000, A = 40, B = 38 }
                    };
                }
                tableCurrent = tableNoseAntH;
            }
            var tableStr = tableCurrent.Where(p => p.fa <= f && p.fb > f).FirstOrDefault();
            if (tableStr != null)
            {
                var res = Math.Round((tableStr.B * Math.Log(f / tableStr.fa) + tableStr.A * Math.Log(tableStr.fb / f)) / Math.Log(tableStr.fb / tableStr.fa), 3);
                return res; //дБ
            }
            else
                return 0;

        }

        //Спектральная плотность напряжённости шума от частоты, новая методика (прил.В ФСТЭК)
        public static double E_sn(double f, string im, int kategoria)
        {
            tableCurrent = new ObservableCollection<TableNose>();
            switch (kategoria)
            {
                case 1:
                    switch (im)
                    {
                        case "Стационарное":
                        if (tableNose1st == null || tableNose1st.Any())
                        {
                                tableNose1st = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 40, B = 2 },
                                    new TableNose() { fa = 50, fb = 200, A = 2, B = -12 },
                                    new TableNose() { fa = 200, fb = 3000, A = -12, B = -21 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -21, B = -28 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -28, B = -32 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -32, B = -30 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -30, B = -30 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -30, B = -30 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -30, B = -30 }
                                };
                            }
                            tableCurrent = tableNose1st;
                        break;
                        case "Портативное возимое":
                            if (tableNose1drive == null || tableNose1drive.Any())
                            {
                                tableNose1drive = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 40, B = 2 },
                                    new TableNose() { fa = 50, fb = 200, A = 2, B = -12 },
                                    new TableNose() { fa = 200, fb = 3000, A = -12, B = -21 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -21, B = -26 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -26, B = -26 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -26, B = -25 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -26, B = -24 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -24, B = -23 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -23, B = -24 }
                                };
                            }
                            tableCurrent = tableNose1drive;
                            break;
                        case "Портативное носимое":
                            if (tableNose1carry == null || tableNose1carry.Any())
                            {
                                tableNose1carry = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 40, B = 2 },
                                    new TableNose() { fa = 50, fb = 200, A = 2, B = -12 },
                                    new TableNose() { fa = 200, fb = 3000, A = -12, B = -21 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -21, B = -21 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -21, B = -20 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -20, B = -18 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -18, B = -17 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -17, B = -16 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -16, B = -17 }
                                };
                            }
                            tableCurrent = tableNose1carry;
                            break;                     
                    }
                    break;
                default: //2,3 категория
                    switch (im)
                    {
                        case "Стационарное":
                            if (tableNose23st == null || tableNose23st.Any())
                            {
                                tableNose23st = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 45, B = 7 },
                                    new TableNose() { fa = 50, fb = 200, A = 7, B = -6 },
                                    new TableNose() { fa = 200, fb = 3000, A = -6, B = -15 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -15, B = -23 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -23, B = -28 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -28, B = -28 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -28, B = -30 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -30, B = -30 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -30, B = -30 }
                                };
                            }
                            tableCurrent = tableNose23st;
                            break;
                        case "Портативное возимое":
                            if (tableNose23drive == null || tableNose23drive.Any())
                            {
                                tableNose23drive = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 45, B = 7 },
                                    new TableNose() { fa = 50, fb = 200, A = 7, B = -6 },
                                    new TableNose() { fa = 200, fb = 3000, A = -6, B = -15 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -15, B = -21 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -21, B = -23 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -23, B = -24 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -24, B = -24 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -24, B = -23 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -23, B = -24 }
                                };
                            }
                            tableCurrent = tableNose23drive;
                            break;
                        case "Портативное носимое":
                            if (tableNose23carry == null || tableNose23carry.Any())
                            {
                                tableNose23carry = new ObservableCollection<TableNose>
                                {
                                    new TableNose() { fa = 9, fb = 50, A = 45, B = 7 },
                                    new TableNose() { fa = 50, fb = 200, A = 7, B = -6 },
                                    new TableNose() { fa = 200, fb = 3000, A = -6, B = -15 },
                                    new TableNose() { fa = 3000, fb = 30000, A = -15, B = -18 },
                                    new TableNose() { fa = 30000, fb = 100000, A = -18, B = -19 },
                                    new TableNose() { fa = 100000, fb = 300000, A = -19, B = -18 },
                                    new TableNose() { fa = 300000, fb = 1000000, A = -18, B = -17 },
                                    new TableNose() { fa = 1000000, fb = 3000000, A = -17, B = -16 },
                                    new TableNose() { fa = 3000000, fb = 10000000, A = -16, B = -17 }
                                };
                            }
                            tableCurrent = tableNose23carry;
                            break;
                    }
                    break;
            }
            var tableStr = tableCurrent.Where(p => p.fa <= f && p.fb > f).FirstOrDefault();
            if (tableStr != null)
            {
                var res = Math.Round((tableStr.B * Math.Log(f / tableStr.fa) + tableStr.A * Math.Log(tableStr.fb / f)) / Math.Log(tableStr.fb / tableStr.fa), 3);
                return res; //дБ
            }
            else
                return 0;
        }
      
        //сигнал для дифференцированного спектра
        public static double E_c_diff(IEnumerable<MEASURING_DATA> list)
        {
            double summa = 0,summaMax = 0; int angleMax = 0;
            var angles = list.Select(p => p.MDA_ANGLE).Distinct(); //углы поворотного стола
            foreach (var angle in angles)
            {
                summa = 0; angleMax = 0;
                foreach (MEASURING_DATA md in list.Where (p=>p.MDA_ANGLE == angle))
                {
                    summa += Math.Pow(md.MDA_E, 2); //сума квадратов сигнала для угла angle
                }
                if (summa > summaMax)
                {
                    summaMax = summa;
                    angleMax = angle;
                }
            }
            return Math.Round(Math.Pow(summaMax, 0.5),3);
        }
        //сигнал наводки для дифференцированного спектра
        public static double[] E_c_diff_u(IEnumerable<MEASURING_DATA> list)
        {
            double summa_f = 0, summa_0 = 0, summaMax_f = 0, summaMax_0 = 0; int angleMax_f = 0, angleMax_0 = 0;
            var angles = list.Select(p => p.MDA_ANGLE).Distinct(); //углы поворотного стола           
            foreach (var angle in angles)
            {
                summa_f = summa_0 = 0; angleMax_f = angleMax_0 = 0;
                foreach (MEASURING_DATA md in list.Where(p => p.MDA_ANGLE == angle))
                {
                    summa_f += Math.Pow(md.MDA_UF, 2); //сума квадратов сигнала для угла angle
                    summa_0 += Math.Pow(md.MDA_U0, 2);
                }
                if (summa_f > summaMax_f)
                {
                    summaMax_f = summa_f;
                    angleMax_f = angle;
                }
                if (summa_0 > summaMax_0)
                {
                    summaMax_0 = summa_0;
                    angleMax_0 = angle;
                }

            }
            return new double[2] {Math.Round(Math.Pow(summaMax_f, 0.5), 3), Math.Round(Math.Pow(summaMax_0, 0.5), 3) };
        }

        //Сигнал для сплошного спектра
        public static double E_c_solid(IEnumerable<MEASURING_DATA> list, ObservableCollection<UNIT> units)
        {
            double summa = 0, summaMax = 0; int angleMax = 0;
            var angles = list.Select(p => p.MDA_ANGLE).Distinct(); //углы поворотного стола
            foreach (var angle in angles)
            {
                summa = 0; angleMax = 0;
                int count = list.Where(p => p.MDA_ANGLE == angle).Count();
                int k = 0;
                foreach (MEASURING_DATA md in list.Where(p => p.MDA_ANGLE == angle))
                {
                    k++;
                    double fkG = F_kGc(md.MDA_F, GetUnitValue(units, md.MDA_F_UNIT_ID));
                    //определение ширины полосы пропускания ИП
                    double weigth = fkG >= 9 && fkG < 150 ? 0.2 :
                                 (fkG >= 150 && fkG < 30000 ? 9 :
                                 (fkG >= 30000 && fkG < 1000000 ? 120 : 1000));
                    if (k == 1 || k == count)
                        summa += Math.Pow(md.MDA_E, 2) * weigth/2;
                    else
                        summa += Math.Pow(md.MDA_E, 2)* weigth; //сума квадратов сигнала для угла angle
                    if (summa > summaMax)
                    {
                        summaMax = summa;
                        angleMax = angle;
                    }
                }
                
            }
            return Math.Round(Math.Pow(summaMax, 0.5), 3);        
        }
        //Сигнал наводки для сплошного спектра
        public static double[] E_c_solid_u(IEnumerable<MEASURING_DATA> list, ObservableCollection<UNIT> units)
        {
            double summa_f = 0, summa_0 = 0, summaMax_f = 0, summaMax_0 = 0; int angleMax_f = 0, angleMax_0 = 0;
            var angles = list.Select(p => p.MDA_ANGLE).Distinct(); //углы поворотного стола
            foreach (var angle in angles)
            {
                summa_f = 0; summa_0 = 0; summaMax_f = 0; summaMax_0 = 0;
                angleMax_f = 0; angleMax_0 = 0;
                int count = list.Where(p => p.MDA_ANGLE == angle).Count();
                int k = 0;
                foreach (MEASURING_DATA md in list.Where(p => p.MDA_ANGLE == angle))
                {
                    k++;
                    double fkG = F_kGc(md.MDA_F, GetUnitValue(units, md.MDA_F_UNIT_ID));
                    //определение ширины полосы пропускания ИП
                    double weigth = fkG >= 9 && fkG < 150 ? 0.2 :
                                 (fkG >= 150 && fkG < 30000 ? 9 :
                                 (fkG >= 30000 && fkG < 1000000 ? 120 : 1000));
                    if (k == 1 || k == count)
                    {
                        summa_f += Math.Pow(md.MDA_UF, 2) * weigth / 2;
                        summa_0 += Math.Pow(md.MDA_U0, 2) * weigth / 2;
                    }
                    else
                    {
                        summa_f += Math.Pow(md.MDA_UF, 2) * weigth; //сума квадратов сигнала для угла angle
                        summa_0 += Math.Pow(md.MDA_U0, 2) * weigth; //сума квадратов сигнала для угла angle
                    }
                    if (summa_f > summaMax_f)
                    {
                        summaMax_f = summa_f;
                        angleMax_f = angle;
                    }
                    if (summa_0 > summaMax_0)
                    {
                        summaMax_0 = summa_0;
                        angleMax_0 = angle;
                    }

                }

            }
            return new double[] { Math.Round(Math.Pow(summaMax_f, 0.5), 3), Math.Round(Math.Pow(summaMax_0, 0.5), 3) };
        }
        //Шум c САЗ - Знаменатель в формуле оценки защищённости информации, обрабатываемой СВТ, для i-го интервала частот
        //используется при определении размеров  зоны2
        //новый вариант расчёта САЗ
        //Шум, измеренный на j-й частоте ИП, с частотой пропускания RBW,пересчитается на полосe пропускания ИП,c которой мерился сигнал
        //если ширина полос различная
        //Суммируется столько значений квадратов шума, сколько было измерений.
        //включать шумы на других частотах не имеет смысла, т.к. они не глушат оцениваемый сигнал
        public static double E_schumaSAZ_New(int i, double tau, IEnumerable<MEASURING_DATA> dmList)
        {
            double summ = 0;
            double deltaF = Math.Round(i * 1000000 / tau, 0) - Math.Round(i == 1 ? 10 : (i - 1) * 1000000 / tau, 0);
            foreach (MEASURING_DATA dm in dmList)
            {
                if (dm.MDA_ES_VALUE_IZM_MKV == 0)
                    continue;
                dm.MDA_KPS = dm.MDA_KPS == 0 ? 1 : dm.MDA_KPS;                //сумма квадратов измеренного шума от САЗ, в пересчёте на ширину полосы пропускания ИП, применяемого при измерении информационного сигнала
                //   summ += Math.Pow((isE ? dm.MDA_EGS_MKV : dm.MDA_HGS) *
                //summ += Math.Pow(dm.MDA_EGS_MKV * Math.Pow((RBW_EH ? dm.MDA_RBW : deltaF), 1 / 2f) / dm.MDA_KPS, 2);//сумма квадратов измеренного шума от САЗ                
                summ += Math.Pow(dm.MDA_ES_VALUE_IZM_MKV / dm.MDA_KPS, 2);//сумма квадратов измеренного шума от САЗ                
            }
            return Math.Round(Math.Pow(summ, 1 / 2f), 3);
        }
        //Шум - Знаменатель(кв.корень из интеграла) в формуле расчёта требуемых значений затухания(Прил.Е новой методики)        
        //Используется метод трапеций численного интегрирования
        public static double E_schuma_Num_U(int i, double tau, string im)
        {
            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = (i == 1) ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;
            double step = 1;
            //double step = (f_top - f_law) / 100;
            double f = f_law; //начальная частота в интервале
            double E_sn = 0;  //спектральная плотность напряжённости шума на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            if (f_law > 400000)
                return Math.Round(Math.Pow(summa * step, 0.5), 3);
            while (f <= f_top && f<=400000)
            {
                E_sn = Functions.E_sn_U(f, im);  //Спектральная плотность напряжённости шума от частоты, новая методика (прил.E ФСТЭК), дБ
                if (E_sn == 0)
                    break;
                if (f == f_law || f == f_top)
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2) / 2;   //сумма квадратов, мкВ
                else
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2);   //сумма квадратов, мкВ

                //if (k == 101)
                //    f = f_top;
                //else
                Math.Round(f += step); //частота очередного шага
                //k++;
            }
            return Math.Round(Math.Pow(summa * step, 0.5), 3); //квадратный корень из интеграла, мкВ            
        }
        public static double E_sn_U(double f, string im)
        {
            tableCurrent = new ObservableCollection<TableNose>();
            switch (im)
            {
               case "Портативное возимое":
               if (tableNose1drive == null || tableNose1drive.Any())
               {
                        tableNose1drive = new ObservableCollection<TableNose>
                        {
                            new TableNose() { fa = 9, fb = 400, A = -16.5, B = -20.5 },
                            new TableNose() { fa = 400, fb = 4000, A = -20.5, B = -25 },
                            new TableNose() { fa = 4000, fb = 30000, A = -25, B = -28 },
                            new TableNose() { fa = 30000, fb = 60000, A = -28, B = -29 },
                            new TableNose() { fa = 60000, fb = 400000, A = -29, B = -34 }
                        };
                    }
               tableCurrent = tableNose1drive;
               break;
               case "Портативное носимое":
               if (tableNose1carry == null || tableNose1carry.Any())
               {
                        tableNose1carry = new ObservableCollection<TableNose>
                        {
                            new TableNose() { fa = 9, fb = 400, A = -14.5, B = -18.5 },
                            new TableNose() { fa = 400, fb = 4000, A = -18.5, B = -23 },
                            new TableNose() { fa = 4000, fb = 30000, A = -23, B = -26 },
                            new TableNose() { fa = 30000, fb = 60000, A = -26, B = -28 },
                            new TableNose() { fa = 60000, fb = 400000, A = -28, B = -34 }
                        };
                    }
               tableCurrent = tableNose1carry;
               break;
            }
            var tableStr = tableCurrent.Where(p => p.fa <= f && p.fb > f).FirstOrDefault();
            if (tableStr != null)
            {
                var res = Math.Round((tableStr.B * Math.Log(f / tableStr.fa) + tableStr.A * Math.Log(tableStr.fb / f)) / Math.Log(tableStr.fb / tableStr.fa), 3);
                return res; //дБ
            }
            else
                return 0;
        }

        //Шум - Знаменатель(кв.корень из интеграла) в формуле расчёта R2
        //п.4 ФСТЭК
        //Используется метод трапеций численного интегрирования
        public static double E_schuma_Num_Prev(int i, double tau, string im, int kategoria)
        {
            //System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\temp\шум.txt",true);
            //file.WriteLine("==================================================");
            //file.WriteLine("i =" + i.ToString() + ";   tau=" + tau.ToString() + ";   " + im  );

            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = (i == 1 )? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;

            if (i == 1 || f_law < 100000)
                return E_schuma_Num_1(i, tau, im, kategoria);

            double step = (f_top - f_law)/100;
            //double step = 1;
            double f = f_law ; //начальная частота в интервале
            double E_sn = 0;  //спектральная плотность напряжённости шума на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            int k = 1;
            //file.WriteLine("Шаг интегрирования = " + step.ToString());
            //file.WriteLine("----------------");
            //file.WriteLine("Шаг ит-ии  Частота   Шум на частоте,дБ     Сумма квадратов шума,мкВ");
            while (k <= 101)
            {
                E_sn = Functions.E_sn(f, im, kategoria);  //Спектральная плотность напряжённости шума от частоты, новая методика (прил.В ФСТЭК), дБ
                //if (f == f_law || f == f_top)
                if (k == 1 || k == 101)
                     summa += Math.Pow(Math.Pow(10,E_sn/20),2)/2 ;   //сумма квадратов, мкВ
                else
                     summa += Math.Pow(Math.Pow(10,E_sn/20), 2);   //сумма квадратов, мкВ
                //file.WriteLine(k.ToString() + "                   " + Math.Round(f,0) + "                   " + Math.Round(E_sn,6) + "                     " + Math.Round(summa,6));

                if (k == 101)
                    f = f_top;
                else
                    Math.Round(f += step); //частота очередного шага
                k++;
            }
            
            //file.WriteLine("Шум в интервале - " + Math.Round(Math.Pow(summa * step, 0.5), 4));
            //file.WriteLine("==================================================");
            //file.Close();
            return Math.Round(Math.Pow(summa * step, 0.5), 3); //квадратный корень из интеграла, мкВ            
        }
        public static double E_schuma_Num(int i, double tau, string im, int kategoria)
        {
            //System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\temp\шум.txt",true);
            //file.WriteLine("==================================================");
            //file.WriteLine("i =" + i.ToString() + ";   tau=" + tau.ToString() + ";   " + im  );

            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = (i == 1) ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;


            double step = 0; // (f_top - f_law) / 100;
            //double step = 1;
            double f = f_law; //начальная частота в интервале
            double E_sn = 0;  //спектральная плотность напряжённости шума на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            //int k = 1;
            //file.WriteLine("Шаг интегрирования = " + step.ToString());
            //file.WriteLine("----------------");
            //file.WriteLine("Шаг ит-ии  Частота   Шум на частоте,дБ     Сумма квадратов шума,мкВ");
            while (f <= f_top)
            {
                if (step == 0 && f < 100000)
                    step = 1;
                else
                {
                    if (step <= 1 && (f + step) >= 100000)
                    {
                        step = (f_top - f_law) / 100;
                    }
                }
                E_sn = Functions.E_sn(f, im, kategoria);  //Спектральная плотность напряжённости шума от частоты, новая методика (прил.В ФСТЭК), дБ
                if (f == f_law || f == f_top)
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2) / 2 * step;   //сумма квадратов, мкВ
                else
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2) * step;   //сумма квадратов, мкВ
                //file.WriteLine(k.ToString() + "                   " + Math.Round(f,0) + "                   " + Math.Round(E_sn,6) + "                     " + Math.Round(summa,6));
                if (f == f_top)
                    break;
                if (f+step > f_top)
                    f = f_top;
                else
                    Math.Round(f += step); //частота очередного шага
            }

            //file.WriteLine("Шум в интервале - " + Math.Round(Math.Pow(summa * step, 0.5), 4));
            //file.WriteLine("==================================================");
            //file.Close();
            return Math.Round(Math.Pow(summa , 0.5), 3); //квадратный корень из интеграла, мкВ            
        }
        //шум с шагом интегрирования 1 кГц
        public static double E_schuma_Num_1(int i, double tau, string im, int kategoria)
        {

            //tau - нсек
            //определяем границы интервала частот
            double f_law, f_top;
            f_law = (i == 1) ? 9 : (i - 1) * 1000000 / tau; //кГц
            f_top = i * 1000000 / tau < 10000000 ? Math.Round(i * 1000000 / tau, 1) : 100000000;
            int step = 1;
            double f = f_law; //начальная частота в интервале
            double E_sn = 0;  //спектральная плотность напряжённости шума на частоте
            double summa = 0; //значение интеграла, вычисленного численным методом трапеций
            while (f <= f_top)
            {
                E_sn = Functions.E_sn(f, im, kategoria);  //Спектральная плотность напряжённости шума от частоты, новая методика (прил.В ФСТЭК), дБ
                if (f == f_law || f == f_top)
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2) / 2;   //сумма квадратов, мкВ
                else
                    summa += Math.Pow(Math.Pow(10, E_sn / 20), 2);   //сумма квадратов, мкВ
                Math.Round(f += step); //частота очередного шага

            }
            return Math.Round(Math.Pow(summa * step, 0.5), 3); //квадратный корень из интеграла, мкВ            
        }

        //расчёт зон R2 для 3-х видов средств разведки
        public static int[] R2(double signal_i,double Kn, double E_schuma_stationary, double E_schuma_portableDrive,
            double E_schuma_portableCarry, double Norma, int i,double d, double tau,bool isSolid )
        {
            //частота в кГц, время в нсек
            if (E_schuma_stationary == 0 || E_schuma_portableDrive == 0 ||
                E_schuma_portableCarry == 0 || Norma == 0)
                return new int[3];

            int[] R2 = new int[3];
            double delta = 0,k_zatuchanija_i = 1; //числитель для очередной итерации
            int R = 0;
            double a = (isSolid ? Math.Pow(2, 0.5) : 1);
            //для каждой строки перерассчитываем 
            while (R2[0] == 0 || R2[1] == 0 || R2[2] == 0)
            {
                //if (R < 10)
                    R++;
                //else
                    //R += 5;
                if (R >= 1000)
                {
                    if (R2[0] == 0)
                        R2[0] = 1000;
                    if (R2[1] == 0)
                        R2[1] = 1000;
                    if (R2[2] == 0)
                        R2[2] = 1000;
                    return R2;
                }
                k_zatuchanija_i = Functions.K_zatuchanija_i(d, R, i, tau); //затухание в интервале
                //delta = E_schuma_stationary != 0
                //               ? a * Math.Round(signal_i / Kn / k_zatuchanija_i, 3)
                //               : 0;
                delta = a * Math.Round(signal_i / Kn / k_zatuchanija_i, 3);
               

                R2[0] = (R2[0] == 0 &&( delta / E_schuma_stationary) <= Norma) ? R : R2[0];
                R2[1] = (R2[1] == 0 && (delta / E_schuma_portableDrive) <= Norma) ? R : R2[1];
                R2[2] = (R2[2] == 0 && (delta / E_schuma_portableCarry) <= Norma) ? R : R2[2];
            }
            R2[0] = (R2[0] > 1000) ? 1000 : R2[0];
            R2[1] = (R2[1] > 1000) ? 1000 : R2[1];
            R2[2] = (R2[2] > 1000 )? 1000 : R2[2];
            return R2;
        }


        //расчёт зон R2 для САЗ
        public static int R2(double signal_i, double Kn, double E_schuma_SAZ, double Norma, int i, double d, double tau, bool isSolid)
        {
            //частота в кГц, время в нсек
            if (E_schuma_SAZ == 0)
                return 0;

            int R2 = 0;
            double delta = 0, k_zatuchanija_i = 1; //числитель для очередной итерации
            int R = 0;
            double a = (isSolid ? Math.Pow(2, 0.5) : 1);
            //для каждой строки перерассчитываем 
            while (R2 == 0)
            {
                //if (R < 10)
                R++;
                //else
                //R += 5;
                if (R >= 1000)
                {
                    if (R2 == 0)
                        R2 = 1000;
                    return R2;
                }
                k_zatuchanija_i = Functions.K_zatuchanija_i(d, R, i, tau); //затухание в интервале
                //delta = E_schuma_stationary != 0
                //               ? a * Math.Round(signal_i / Kn / k_zatuchanija_i, 3)
                //               : 0;
                delta = a * Math.Round(signal_i / Kn / k_zatuchanija_i, 3);


                R2 = (R2 == 0 && (delta / E_schuma_SAZ) <= Norma) ? R : R2;
            }
            R2 = (R2 > 1000) ? 1000 : R2;
            return R2;
        }

        //расчёт зон R1 от наводок электрического или магнитного поля
        public static double R1(double signal_i, double Kn, double E_schumaAnt,
             double Norma, int i, double d, double tau, bool isSolid,bool isE)
        {
            double R1 = 0;
            double delta = 0, k_zatuchanija_i = 0; //числитель для очередной итерации
            double R = 0.5;
            double a = (isSolid ? Math.Pow(2, 0.5) : 1);
            //для каждой строки перерассчитываем 
            while (R1 == 0)
            {                               
                k_zatuchanija_i = Functions.K_zatuchanija_i_R1(d, R, i, tau,isE); //затухание в интервале
                delta = E_schumaAnt != 0
                               ? a * Math.Round(signal_i / Kn / k_zatuchanija_i, 3)
                               : 0;
                R1 = (R1 == 0 && (delta / E_schumaAnt) <= Norma) ? R : R1;
                //R = Math.Round(R < 1 ? R + 0.1: R + 1, 1);
                R += 0.1;

            }

            return R1;
        }                

        
     #endregion Расчётные функции
     #region Файловый ввод-вывод

        public static string[] Read()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Data\CSV"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Data\CSV");
            ofd.InitialDirectory = Environment.CurrentDirectory + @"\Data\CSV";
            ofd.DefaultExt = ".csv";
            if (ofd.ShowDialog() == false)
                return new string[0];
            try
            {
                return File.ReadAllLines(ofd.FileName);
            }
            catch (Exception e)
            {
                return new string[0];
            }

        }

        public static Stream GetStreamForRead()
        {
             OpenFileDialog ofd = new OpenFileDialog();            
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Data"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Data");
            ofd.InitialDirectory = Environment.CurrentDirectory + @"\Data";
            ofd.DefaultExt = ".bin";
            if (ofd.ShowDialog() == false)
                return null;
            try
            {
                return File.OpenRead(ofd.FileName);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static FileStream GetFileStreamForRead()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Data"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Data");
            ofd.InitialDirectory = Environment.CurrentDirectory + @"\Data";
            ofd.DefaultExt = ".bin";
            if (ofd.ShowDialog() == false)
                return null;
            try
            {
                return File.OpenRead(ofd.FileName);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static Stream GetStreamForWrite()
        {
            //Stream TestFileStream;
            SaveFileDialog sfd = new SaveFileDialog()
            { DefaultExt = ".bin" };
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Data"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Data");
            sfd.InitialDirectory = Environment.CurrentDirectory + @"\Data";
            sfd.FileName = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString();
            if (sfd.ShowDialog() == true)
                return File.Create(sfd.FileName);
            else
                return null;
        }

        public static byte[] Serialization(Object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            byte[] arBytes = stream.ToArray();
            return arBytes;
        }

        public static Object Deserialization(byte[] arBytes)
        {
             Stream str = new MemoryStream(arBytes);
             BinaryFormatter deserializer = new BinaryFormatter();
             return (Object)deserializer.Deserialize(str);//получили экземпляр класса, дополненный результатами расчёта
            
            //BinaryFormatter formatter = new BinaryFormatter();
            //MemoryStream stream = new MemoryStream();
            //formatter.Serialize(stream, obj);
            //byte[] arBytes = stream.ToArray();
            //return arBytes;
        }
        #endregion Файловый ввод-вывод
    }
    public class TableNose
    {
        public double  fa { get; set; }
        public double fb { get; set; }
        public double A { get; set; }
        public double B { get; set; }

    }
}
