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
    
    public partial class ARM_TYPE
    {
        public ARM_TYPE()
        {
            this.ARM = new HashSet<ARM>();
        }
    
        public int AT_ID { get; set; }
        public Nullable<int> AT_ANL_ID { get; set; }
        public string AT_NAME { get; set; }
        public string AT_NOTE { get; set; }
    
        public virtual ANALYSIS ANALYSIS { get; set; }
        public virtual ICollection<ARM> ARM { get; set; }
    }
}