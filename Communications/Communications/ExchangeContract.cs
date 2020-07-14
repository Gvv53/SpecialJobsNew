using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Communications
{
    [DataContract]
    public class ExchangeContract
    {
        [DataMember]
        public bool Added { get; set; } = false;
        
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public List<double> Noise { get; set; } = new List<double>();

        [DataMember]
        public List<double> Signal { get; set; } = new List<double>();

        [DataMember]
        public List<double> Frequencys { get; set; } = new List<double>();

        [DataMember]
        public List<double> OriginalNoise { get; set; } = new List<double>();

        [DataMember]
        public List<double> OriginalSignal { get; set; } = new List<double>();

        [DataMember]
        public string AdditionalInfo { get; set; }

    }
}
