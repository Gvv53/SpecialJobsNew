using System;
using System.ServiceModel;
using System.Windows;

namespace Communications
{
    public class PIPEService
    {
        public Action<string> IncomingString;

        public Action<byte[]> IncomingBytes;

        public Action<ExchangeContract> IncomingExchangeContract;

        public Action<int, double> IncomingR2;

        public PIPEService(string uri = Defines.URI, string address = Defines.ADDRESS)
        {
            var host = new ServiceHost(typeof(ImplementationService), new Uri(uri));

            NetNamedPipeBinding netNamedPipeBinding = new NetNamedPipeBinding();

            netNamedPipeBinding.MaxBufferSize = Defines.COMMONBUFFERSIZE;
            netNamedPipeBinding.MaxReceivedMessageSize = Defines.COMMONBUFFERSIZE;

            host.AddServiceEndpoint(typeof(IService), netNamedPipeBinding, address);
            host.Open();

            ImplementationService.IncomingString += (data) =>
                IncomingString?.Invoke(data);

            ImplementationService.IncomingBytes += (data) =>
                IncomingBytes?.Invoke(data);

            ImplementationService.IncomingExchangeContract += (data) =>
                IncomingExchangeContract?.Invoke(data);

            ImplementationService.IncomingR2 += (id, r2) =>
                IncomingR2?.Invoke(id, r2);


        }

        public void SendString(string data)
        {
            ImplementationService.Callback?.SendString(data);
        }
        public void SendBytes(byte[] data)
        {
            ImplementationService.Callback?.SendBytes(data);
        }
        public void SendExchangeContract(ExchangeContract data)
        {
            try
            {
                ImplementationService.Callback?.SendExchangeContract(data);
            } 
            catch(Exception ex)
            {
                MessageBox.Show("Посылка не удалась "
                        + Environment.NewLine
                        + ex.ToString());
            }
        }

    }
}
