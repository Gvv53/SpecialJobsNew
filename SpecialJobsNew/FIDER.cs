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
    
    public partial class FIDER
    {
        public FIDER()
        {
            this.FIDER_CALIBRATION = new HashSet<FIDER_CALIBRATION>();
        }
    
        public int F_ID { get; set; }
        public string F_TYPE { get; set; }
        public string F_WORKNUMBER { get; set; }
        public string F_SERTIFICAT { get; set; }
        public double F_ERROR { get; set; }
        public string F_OWNER { get; set; }
        public string F_CONDITIONS { get; set; }
        public Nullable<int> F_PERSON_ID { get; set; }
        public Nullable<int> F_EXECUTOR_ID { get; set; }
        public Nullable<System.DateTime> F_DATE { get; set; }
        public Nullable<int> F_F_UNIT_ID { get; set; }
        public Nullable<int> F_VALUE_UNIT_ID { get; set; }
        public string F_COMMENT { get; set; }
        public string F_MODEL { get; set; }
        public string F_F_INTERVAL { get; set; }
        public string F_DEFAULT { get; set; }
    
        public virtual ICollection<FIDER_CALIBRATION> FIDER_CALIBRATION { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual PERSON PERSON1 { get; set; }
        public virtual UNIT UNIT { get; set; }
        public virtual UNIT UNIT1 { get; set; }
    }
}
