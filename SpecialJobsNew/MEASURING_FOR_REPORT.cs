//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpecialJobs
{
    using System;
    using System.Collections.Generic;
    
    public partial class MEASURING_FOR_REPORT
    {
        public int MFR_ID { get; set; }
        public int MFR_MODE_ID { get; set; }
        public int MFR_NPP { get; set; }
        public Nullable<double> MFR_F { get; set; }
        public Nullable<int> MFR__I { get; set; }
        public Nullable<double> MFR_ECN { get; set; }
        public Nullable<double> MFR_EN { get; set; }
        public Nullable<double> MFR_E { get; set; }
        public string MFR_R2_2 { get; set; }
        public string MFR_R2_3 { get; set; }
        public Nullable<double> MFR_UGS1 { get; set; }
        public Nullable<double> MFR_UGS2 { get; set; }
        public string MFR_R1_2 { get; set; }
        public string MFR_R1_3 { get; set; }
        public string MFR__R1_2 { get; set; }
        public string MFR__R1_3 { get; set; }
        public Nullable<double> MFR_KZAT { get; set; }
        public string MFR_DELTA { get; set; }
        public Nullable<double> MFR_F_BEGIN { get; set; }
        public Nullable<double> MFR_F_END { get; set; }
    
        public virtual MODE MODE { get; set; }
    }
}
