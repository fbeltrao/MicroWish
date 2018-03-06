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
    public class ServiceBusQueueListener : ICommunicationListener
    {
        private string _queueName;
        private string _connectionString;
        private QueueClient _client;
        private Func<BrokeredMessage, Task> _callback;

        public ServiceBusQueueListener(Func<BrokeredMessage, Task> callback, string connectionString, string queueName)
        {
            // Set variables
            _callback = callback;
            _connectionString = connectionString;
            _queueName = queueName;
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

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _client = QueueClient.CreateFromConnectionString(_connectionString, _queueName);
            _client.OnMessageAsync(async (message) => await _callback(message));

            // Return the uri - in this case, that's just our connection string
            return Task.FromResult(_connectionString);
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
