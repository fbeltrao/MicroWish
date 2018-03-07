using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Configuration
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; } = "Endpoint=sb://frchamabus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=vGX7jPaBQ5IuvGmzYU+1MxphsHSgVUj2T7dbu3NCRCg=";

        public string CreateOrderQueueName { get; set; } = "createorder";

        public string VerifyOrderPaymentQueueName { get; set; } = "verifyorderpayment";

        public string OrderCreatedTopicName { get; set; } = "ordercreated";

        public string OrderPaymentFailedTopicName { get; set; } = "orderpaymentfailed";

        public string OrderFinalizedTopicName { get; set; } = "orderfinalized";

        public string ProductChangedTopicName { get; set; } = "productchanged";

        public string OrderCreationFailedTopicName { get; set; } = "ordercreationfailed";
    }
}
