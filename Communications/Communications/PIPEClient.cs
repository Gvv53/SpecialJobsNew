using System;
using System.ServiceModel;

namespace Communications
{
    public class PIPEClient
    {
        public Action<string> IncomingString;

        public Action<byte[]> IncomingBytes;

        public Action<ExchangeContract> IncomingExchangeContract;

        public IService Service;
        public PIPEClient(string uri = Defines.URI, string address = Defines.ADDRESS)
        {

            EndpointAddress endpointAddress = new EndpointAddress(uri + "/" + address);
            NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding();

            netNamedPipeBinding.MaxBufferSize = Defines.COMMONBUFFERSIZE;
            netNamedPipeBinding.MaxReceivedMessageSize = Defines.COMMONBUFFERSIZE;

            Service = (new DuplexChannelFactory<IService>(
                new InstanceContext(new ImplementationCallback()),
                netNamedPipeBinding,
                endpointAddress)).CreateChannel();

            Service.Connect();

            ImplementationCallback.IncomingString += (data) =>
                IncomingString?.Invoke(data);

            ImplementationCallback.IncomingBytes += (data) =>
                IncomingBytes?.Invoke(data);

            ImplementationCallback.IncomingExchangeContract += (data) =>
            IncomingExchangeContract?.Invoke(data);

            
        }

        public void SendBytes(byte[] data)
        {
            Service?.SendBytes(data);
        }

        public void SendString(string data)
        {
            Service?.SendString(data);
        }

        public void SendExchangeContract(ExchangeContract data)
        {
            Service?.SendExchangeContract(data);
        }
        public void SendR2(int id, double r2)
        {
            Service?.SendR2(id, r2);
        }
    }
}
