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
    
    public partial class ANTENNA_ARM
    {
        public int ANTARM_ID { get; set; }
        public Nullable<int> ANTARM_ARM_ID { get; set; }
        public Nullable<int> ANTARM_ANT_ID { get; set; }
    
        public virtual ANTENNA ANTENNA { get; set; }
        public virtual ARM ARM { get; set; }
    }
}