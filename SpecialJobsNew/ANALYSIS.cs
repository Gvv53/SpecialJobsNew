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
    
    public partial class ANALYSIS
    {
        public ANALYSIS()
        {
            this.ARM_TYPE = new HashSet<ARM_TYPE>();
            this.REPORT_DATA = new HashSet<REPORT_DATA>();
        }
    
        public int ANL_ID { get; set; }
        public int ANL_ORG_ID { get; set; }
        public string ANL_INVOICE { get; set; }
        public string ANL_REASON { get; set; }
        public Nullable<int> ANL_METHOD_ID { get; set; }
        public Nullable<System.DateTime> ANL_DATE_BEGIN { get; set; }
        public Nullable<System.DateTime> ANL_DATE_END { get; set; }
        public string ANL_NAME { get; set; }
        public Nullable<int> ANL_MANAGER_PERSON_ID { get; set; }
        public Nullable<int> ANL_EXECUTOR_PERSON_ID { get; set; }
        public string ANL_NOTE { get; set; }
    
        public virtual METHOD METHOD { get; set; }
        public virtual ORGANIZATION ORGANIZATION { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual PERSON PERSON1 { get; set; }
        public virtual ICollection<ARM_TYPE> ARM_TYPE { get; set; }
        public virtual ICollection<REPORT_DATA> REPORT_DATA { get; set; }
    }
}
