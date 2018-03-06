using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroWish.ServiceFabric
{
    /// <summary>
    /// Source code found here: https://github.com/ianrufus/BlogPosts/blob/master/ServiceBusTrigger/TriggerService/ServiceListeners/ServiceBusQueueListener.cs
    /// </summary>
    public class ServiceBusTopicListener : ICommunicationListener
    {
        private string _topicName;
        private readonly string _subscriptionName;
        private string _connectionString;
        private SubscriptionClient _client;
        private Func<BrokeredMessage, Task> _callback;

        public ServiceBusTopicListener(Func<BrokeredMessage, Task> callback, string connectionString, string topicName, string subscriptionName)
        {
            // Set variables
            _callback = callback;
            _connectionString = connectionString;
            _topicName = topicName;
            this._subscriptionName = subscriptionName;
        }

        public void Abort()
        {
            // Close down
            Stop();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            // Close down
            Stop();
            return Task.FromResult(true);
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var ns = NamespaceManager.CreateFromConnectionString(_connectionString);
            if (! await ns.SubscriptionExistsAsync(_topicName, _subscriptionName))
                await ns.CreateSubscriptionAsync(_topicName, _subscriptionName);

            var factory = MessagingFactory.CreateFromConnectionString(_connectionString);
            _client = factory.CreateSubscriptionClient(_topicName, _subscriptionName);            
            _client.OnMessageAsync(async (message) => await _callback(message));

            // Return the uri - in this case, that's just our connection string
            return _connectionString;
        }

        private void Stop()
        {
            if (_client != null && !_client.IsClosed)
            {
                _client.Close();
                _client = null;
            }
        }
    }
}
