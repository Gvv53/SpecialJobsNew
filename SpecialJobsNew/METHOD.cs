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
    
    public partial class METHOD
    {
        public METHOD()
        {
            this.ANALYSIS = new HashSet<ANALYSIS>();
        }
    
        public int METHOD_ID { get; set; }
        public string METHOD_NAME { get; set; }
        public string METHOD_ABBREV { get; set; }
        public string METHOD_NOTE { get; set; }
    
        public virtual ICollection<ANALYSIS> ANALYSIS { get; set; }
    }
}
