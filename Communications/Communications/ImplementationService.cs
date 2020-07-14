using System;
using System.ServiceModel;

namespace Communications
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ImplementationService : IService
    {
        public void Connect()
        {
            Callback = OperationContext.Current.GetCallbackChannel<ICallback>();
        }
        public static ICallback Callback { get; set; }

        public static Action<string> IncomingString;

        public static Action<byte[]> IncomingBytes;

        public static Action<ExchangeContract> IncomingExchangeContract;

        public static Action<int, double> IncomingR2;

        public void SendString(string data) => ImplementationService.IncomingString?.Invoke(data);

        public void SendBytes(byte[] data) => ImplementationService.IncomingBytes?.Invoke(data);

        public void SendExchangeContract(ExchangeContract data) => ImplementationService.IncomingExchangeContract?.Invoke(data);

        public void SendR2(int id, double r2) => ImplementationService.IncomingR2?.Invoke(id, r2);
    }
}
