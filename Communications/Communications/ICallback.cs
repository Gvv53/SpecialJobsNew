using System.ServiceModel;

namespace Communications
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void SendString(string data);

        [OperationContract(IsOneWay = true)]
        void SendBytes(byte[] data);

        [OperationContract(IsOneWay = true)]
        void SendExchangeContract(ExchangeContract data);
    }
}
