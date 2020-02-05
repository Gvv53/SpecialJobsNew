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
    
    public partial class MODE
    {
        public MODE()
        {
            this.MEASURING_FOR_REPORT = new HashSet<MEASURING_FOR_REPORT>();
            this.RESULT = new HashSet<RESULT>();
            this.MEASURING_DATA = new HashSet<MEASURING_DATA>();
        }
    
        public int MODE_ID { get; set; }
        public Nullable<int> MODE_ARM_ID { get; set; }
        public string MODE_ITEM { get; set; }
        public Nullable<int> MODE_MT_ID { get; set; }
        public Nullable<bool> MODE_AUTORBW { get; set; }
        public Nullable<bool> MODE_ITUNIFIED { get; set; }
        public double MODE_FT { get; set; }
        public Nullable<int> MODE_FT_UNIT_ID { get; set; }
        public double MODE_TAU { get; set; }
        public Nullable<int> MODE_TAU_UNIT_ID { get; set; }
        public double MODE_D { get; set; }
        public double MODE_R { get; set; }
        public double MODE_RMAX { get; set; }
        public double MODE_L { get; set; }
        public Nullable<bool> MODE_ANT_GS { get; set; }
        public double MODE_RBW { get; set; }
        public Nullable<int> MODE_RBW_UNIT_ID { get; set; }
        public string MODE_SVT { get; set; }
        public double MODE_NORMA { get; set; }
        public double MODE_KS { get; set; }
        public Nullable<int> MODE_EQ_ID { get; set; }
        public bool MODE_IS_SOLID { get; set; }
        public double MODE_R2 { get; set; }
        public double MODE_R1 { get; set; }
        public bool MODE_CONTR_E { get; set; }
        public Nullable<int> MODE_ANT_E_ID { get; set; }
        public Nullable<int> MODE_ANT_H_ID { get; set; }
    
        public virtual ANTENNA ANTENNA { get; set; }
        public virtual ANTENNA ANTENNA1 { get; set; }
        public virtual ARM ARM { get; set; }
        public virtual ICollection<MEASURING_FOR_REPORT> MEASURING_FOR_REPORT { get; set; }
        public virtual MODE_TYPE MODE_TYPE { get; set; }
        public virtual UNIT UNIT { get; set; }
        public virtual UNIT UNIT1 { get; set; }
        public virtual ICollection<RESULT> RESULT { get; set; }
        public virtual ICollection<MEASURING_DATA> MEASURING_DATA { get; set; }
        public MODE DeepCopy()
        {
            MODE other = new MODE();    //            (MODE)this.MemberwiseClone();
            other.MODE_ANT_GS = ((MODE)this).MODE_ANT_GS;
            other.MODE_AUTORBW = ((MODE)this).MODE_AUTORBW;
            other.MODE_D = ((MODE)this).MODE_D;
            other.MODE_EQ_ID = ((MODE)this).MODE_EQ_ID;
            other.MODE_FT = ((MODE)this).MODE_FT;
            other.MODE_FT_UNIT_ID = ((MODE)this).MODE_FT_UNIT_ID;
            other.MODE_ITEM = ((MODE)this).MODE_ITEM;
            other.MODE_ITUNIFIED = ((MODE)this).MODE_ITUNIFIED;
            other.MODE_KS = ((MODE)this).MODE_KS;
            other.MODE_L = ((MODE)this).MODE_L;
            other.MODE_MT_ID = ((MODE)this).MODE_MT_ID;
            other.MODE_NORMA = ((MODE)this).MODE_NORMA;
            other.MODE_R = ((MODE)this).MODE_R;
            other.MODE_RBW = ((MODE)this).MODE_RBW;
            other.MODE_RBW_UNIT_ID = ((MODE)this).MODE_RBW_UNIT_ID;
            other.MODE_ANT_GS = ((MODE)this).MODE_ANT_GS;
            other.MODE_RMAX = ((MODE)this).MODE_RMAX;
            other.MODE_SVT = ((MODE)this).MODE_SVT;
            other.MODE_TAU = ((MODE)this).MODE_TAU;
            other.MODE_TAU_UNIT_ID = ((MODE)this).MODE_TAU_UNIT_ID;
            other.MODE_R2 = ((MODE)this).MODE_R2;
            other.MODE_R1 = ((MODE)this).MODE_R1;
            other.MODE_ANT_E_ID = ((MODE)this).MODE_ANT_E_ID;
            other.MODE_ANT_H_ID = ((MODE)this).MODE_ANT_H_ID;
            other.MODE_CONTR_E = ((MODE)this).MODE_CONTR_E;
            return other;
        }
    }
}