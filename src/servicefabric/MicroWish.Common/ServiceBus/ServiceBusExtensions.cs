using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace MicroWish.ServiceBus
{
    public static class ServiceBusExtensions
    {

        public static TEntity GetJsonBody<TEntity>(this BrokeredMessage msg) where TEntity : class
        {
            var data = msg.GetBody<Stream>();
            byte[] buff = new byte[data.Length];
            data.Read(buff, 0, (int)data.Length);

            return JsonConvert.DeserializeObject<TEntity>(Encoding.UTF8.GetString(buff));

        }

    }
}
