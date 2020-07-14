using System;

namespace Communications
{
    public class ImplementationCallback : ICallback
    {
        public static Action<string> IncomingString;

        public static Action<byte[]> IncomingBytes;

        public static Action<ExchangeContract> IncomingExchangeContract;

        public void SendString(string data) => ImplementationCallback.IncomingString?.Invoke(data);

        public void SendBytes(byte[] data) => ImplementationCallback.IncomingBytes?.Invoke(data);

        public void SendExchangeContract(ExchangeContract data) => ImplementationCallback.IncomingExchangeContract?.Invoke(data);

    }
}
