using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.ServiceBus
{
    public static class BrokeredMessageFactory
    {
        public static BrokeredMessage CreateJsonMessage(object value)
        {
            var json = JsonConvert.SerializeObject(value);
            return new BrokeredMessage(new MemoryStream(UTF8Encoding.UTF8.GetBytes(json)))
            {
                ContentType = "application/json",
            };
        }
    }
}
