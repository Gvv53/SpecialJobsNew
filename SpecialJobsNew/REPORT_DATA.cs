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
    
    public partial class REPORT_DATA
    {
        public int RD_ID { get; set; }
        public int RD_ANL_ID { get; set; }
        public string RD_LABEL { get; set; }
        public string RD_REPORT { get; set; }
        public string RD_DESCRIPTION { get; set; }
        public string RD_TEXT { get; set; }
        public Nullable<bool> RD_DEFAULT { get; set; }
    
        public virtual ANALYSIS ANALYSIS { get; set; }
    }
}