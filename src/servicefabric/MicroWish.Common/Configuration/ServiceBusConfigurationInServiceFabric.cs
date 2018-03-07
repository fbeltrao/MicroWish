using System.Fabric;

namespace MicroWish.Configuration
{
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
