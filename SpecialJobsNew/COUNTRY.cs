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
    
    public partial class COUNTRY
    {
        public COUNTRY()
        {
            this.EQUIPMENT = new HashSet<EQUIPMENT>();
        }
    
        public int COUNTRY_ID { get; set; }
        public string COUNTRY_NAME { get; set; }
    
        public virtual ICollection<EQUIPMENT> EQUIPMENT { get; set; }
    }
}
