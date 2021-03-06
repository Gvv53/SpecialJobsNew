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
    
    public partial class ANTENNA
    {
        public ANTENNA()
        {
            this.ANTENNA_ARM = new HashSet<ANTENNA_ARM>();
            this.ANTENNA_CALIBRATION = new HashSet<ANTENNA_CALIBRATION>();
            this.ARM = new HashSet<ARM>();
            this.MODE = new HashSet<MODE>();
            this.MODE1 = new HashSet<MODE>();
        }
    
        public int ANT_ID { get; set; }
        public string ANT_TYPE { get; set; }
        public string ANT_MODEL { get; set; }
        public string ANT_WORKNUMBER { get; set; }
        public string ANT_SERTIFICAT { get; set; }
        public string ANT_F_INTERVAL { get; set; }
        public double ANT_ERROR { get; set; }
        public string ANT_OWNER { get; set; }
        public string ANT_CONDITIONS { get; set; }
        public Nullable<int> ANT_PERSON_ID { get; set; }
        public Nullable<int> ANT_EXECUTOR_ID { get; set; }
        public Nullable<System.DateTime> ANT_DATE { get; set; }
        public Nullable<int> ANT_F_UNIT_ID { get; set; }
        public string ANT_COMMENT { get; set; }
        public string ANT_DEFAULT { get; set; }
        public string ANT_NAME { get; set; }
    
        public virtual ICollection<ANTENNA_ARM> ANTENNA_ARM { get; set; }
        public virtual ICollection<ANTENNA_CALIBRATION> ANTENNA_CALIBRATION { get; set; }
        public virtual PERSON PERSON { get; set; }
        public virtual PERSON PERSON1 { get; set; }
        public virtual UNIT UNIT { get; set; }
        public virtual ICollection<ARM> ARM { get; set; }
        public virtual ICollection<MODE> MODE { get; set; }
        public virtual ICollection<MODE> MODE1 { get; set; }
    }
}
