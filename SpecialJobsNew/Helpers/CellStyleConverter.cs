using System.Linq;
using DevExpress.Xpf.Grid;
using System.Windows.Data;
using System.Windows.Media;


namespace SpecialJobs.Converters
{
    public class CellStyleConverter : IMultiValueConverter// MarkupExtension, IValueConverter
    {

        #region IValueConverter Members

        public static double RES_SAZ_MAX;
        public static double RES_DELTA_PORTABLE_MAX;
        public static double RES_DELTA_PORTABLE_DRIVE_MAX;
        public static double RES_DELTA_PORTABLE_CARRY_MAX;

        //для дерева анализа

        public static double R2portable_1_MAX;
        public static double R2portable_2_MAX;
        public static double R2portable_3_MAX;

        public static double R2drive_1_MAX;
        public static double R2drive_2_MAX;
        public static double R2drive_3_MAX;

        public static double R2carry_1_MAX;
        public static double R2carry_2_MAX;
        public static double R2carry_3_MAX;

        public static double R1sosr_1_MAX;
        public static double R1sosr_2_MAX;
        public static double R1sosr_3_MAX;

        public object Convert(object[] value, System.Type targetType,
                    object parameter, System.Globalization.CultureInfo culture)
        {                        
            if (value[1] == null || (double)value[1] == 0 )
                return Brushes.White;
            string columnName = ((ColumnBase)value[2]).FieldName;

            if (columnName == "RES_R2_PORTABLE" || columnName == "RES_R2_PORTABLE_DRIVE" || 
                columnName == "RES_R2_PORTABLE_CARRY")
            {
                if ((double)value[1] > 40)
                    return Brushes.Red;
                if ((double)value[1] == 40)
                    return Brushes.Pink;
                return Brushes.White;
            }
            // else
            switch (columnName)
            {
                //для дерева анализа
                case "R2portable_1":
                    if ((double)value[1] == R2portable_1_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2portable_2":
                    if ((double)value[1] == R2portable_2_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2portable_3":
                    if ((double)value[1] == R2portable_3_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2drive_1":
                    if ((double)value[1] == R2drive_1_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2drive_2":
                    if ((double)value[1] == R2drive_2_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2drive_3":
                    if ((double)value[1] == R2drive_3_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2carry_1":
                    if ((double)value[1] == R2carry_1_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2carry_2":
                    if ((double)value[1] == R2carry_2_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R2carry_3":
                    if ((double)value[1] == R2carry_3_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R1sosr_1":
                    if ((double)value[1] == R1sosr_1_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R1sosr_2":
                    if ((double)value[1] == R1sosr_2_MAX)
                        return Brushes.Red;
                    return Brushes.White;
                case "R1sosr_3":
                    if ((double)value[1] == R1sosr_3_MAX)
                        return Brushes.Red;
                    return Brushes.White;


            }
            //switch (columnName)
            //{
            //    case "RES_SAZ":
            //        if ((double)value[1] == RES_SAZ_MAX && norma < (double)value[1])
            //            return Brushes.Red;
            //        if( norma < (double)value[1])
            //            return Brushes.Pink;
            //        return Brushes.White;
            //    case "RES_DELTA_PORTABLE" :
            //        if ((double)value[1] == RES_DELTA_PORTABLE_MAX && norma < (double)value[1])
            //            return Brushes.Red;
            //        if( norma < (double)value[1])
            //            return Brushes.Pink;
            //        return Brushes.White;
            //    case "RES_DELTA_PORTABLE_DRIVE":
            //        if ((double)value[1] == RES_DELTA_PORTABLE_DRIVE_MAX && norma < (double)value[1])
            //            return Brushes.Red;
            //        if( norma < (double)value[1])
            //            return Brushes.Pink;
            //        return Brushes.White;
            //    case "RES_DELTA_PORTABLE_CARRY":
            //        if ((double)value[1] == RES_DELTA_PORTABLE_CARRY_MAX && norma < (double)value[1])
            //            return Brushes.Red;
            //        if( norma < (double)value[1])
            //            return Brushes.Pink;
            //        return Brushes.White;
            //        //для дерева анализа
            //        case "R2portable_1":
            //            if ((double)value[1] == R2portable_1_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2portable_2":
            //            if ((double)value[1] == R2portable_2_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2portable_3":
            //            if ((double)value[1] == R2portable_3_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2drive_1":
            //            if ((double)value[1] == R2drive_1_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2drive_2":
            //            if ((double)value[1] == R2drive_2_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2drive_3":
            //            if ((double)value[1] == R2drive_3_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2carry_1":
            //            if ((double)value[1] == R2carry_1_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2carry_2":
            //            if ((double)value[1] == R2carry_2_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R2carry_3":
            //            if ((double)value[1] == R2carry_3_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R1sosr_1":
            //            if ((double)value[1] == R1sosr_1_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R1sosr_2":
            //            if ((double)value[1] == R1sosr_2_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;
            //        case "R1sosr_3":
            //            if ((double)value[1] == R1sosr_3_MAX)
            //                return Brushes.Red;
            //            return Brushes.White;


            //    }
            return Brushes.White;
        }

        public object[] ConvertBack(object value, System.Type[] targetType,
                    object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //public override object ProvideValue(System.IServiceProvider serviceProvider)
        //{
        //    return this;
        //}
    }
}
