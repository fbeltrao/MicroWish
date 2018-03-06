using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Configuration
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; } = "Endpoint=sb://frchamabus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=vGX7jPaBQ5IuvGmzYU+1MxphsHSgVUj2T7dbu3NCRCg=";

        public string CreateOrderQueueName { get; set; } = "createorder";

        public string VerifyOrderPaymentQueueName { get; set; } = "verifyorderpayment";

        public string OrderCreatedTopicName { get; set; } = "ordercreated";

        public string OrderPaymentFailedTopicName { get; set; } = "orderpaymentfailed";

        public string OrderFinalizedTopicName { get; set; } = "orderfinalized";
    }

    public class ServiceBusConfigurationInServiceFabric : ServiceBusConfiguration
    {
        public ServiceBusConfigurationInServiceFabric(ServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var serviceBusConfigurationSection = configurationPackage.Settings.Sections.Contains("ServiceBus") ? configurationPackage.Settings.Sections["ServiceBus"] : null;
            this.ConnectionString = serviceBusConfigurationSection?.Parameters["ConnectionString"]?.Value ?? "Endpoint=sb://frchamabus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=vGX7jPaBQ5IuvGmzYU+1MxphsHSgVUj2T7dbu3NCRCg=";
            this.CreateOrderQueueName = serviceBusConfigurationSection?.Parameters["CreateOrderQueueName"]?.Value ?? "createorder";
        }
    }
}
