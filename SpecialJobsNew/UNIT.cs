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
    
    public partial class UNIT
    {
        public UNIT()
        {
            this.ANTENNA = new HashSet<ANTENNA>();
            this.FIDER = new HashSet<FIDER>();
            this.FIDER1 = new HashSet<FIDER>();
            this.MODE = new HashSet<MODE>();
            this.MODE1 = new HashSet<MODE>();
            this.MEASURING_DATA = new HashSet<MEASURING_DATA>();
            this.MEASURING_DATA1 = new HashSet<MEASURING_DATA>();
        }
    
        public int UNIT_ID { get; set; }
        public string UNIT_VALUE { get; set; }
        public string UNIT_NOTE { get; set; }
    
        public virtual ICollection<ANTENNA> ANTENNA { get; set; }
        public virtual ICollection<FIDER> FIDER { get; set; }
        public virtual ICollection<FIDER> FIDER1 { get; set; }
        public virtual ICollection<MODE> MODE { get; set; }
        public virtual ICollection<MODE> MODE1 { get; set; }
        public virtual ICollection<MEASURING_DATA> MEASURING_DATA { get; set; }
        public virtual ICollection<MEASURING_DATA> MEASURING_DATA1 { get; set; }
    }
}