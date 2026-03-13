using System.ServiceModel;

namespace com.mymooo.mall.wcf
{
    public class WcfConfig
    {
        public WcfConfig()
        {
            HttpBinding = new BasicHttpBinding();
            HttpBinding.MaxBufferSize = int.MaxValue;
            HttpBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            HttpBinding.MaxReceivedMessageSize = int.MaxValue;
            HttpBinding.AllowCookies = true;
        }

        public BasicHttpBinding HttpBinding { get; }

        public string Url { get; set; } = string.Empty;

        public EndpointAddress GetEndpointAddress(string path)
        {
            return new EndpointAddress($"{Url}{path}");
        }
    }
}
