using System.ServiceModel;

namespace Communications
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallback))]
    public interface IService
    {
        [OperationContract(IsOneWay = true)]
        void Connect();

        [OperationContract(IsOneWay = true)]
        void SendString(string data);

        [OperationContract(IsOneWay = true)]
        void SendBytes(byte[] data);

        [OperationContract(IsOneWay = true)]
        void SendExchangeContract(ExchangeContract data);

        [OperationContract(IsOneWay = true)]
        void SendR2(int id, double r2);
    }
}
