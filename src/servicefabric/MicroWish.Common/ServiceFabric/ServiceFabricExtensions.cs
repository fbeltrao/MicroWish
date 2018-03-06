using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.ServiceFabric
{
    public static class ServiceFabricExtensions
    {
        /// <summary>
        /// Gets the partition key which serves the specified word.
        /// Note that the sample only accepts Int64 partition scheme. 
        /// </summary>
        /// <param name="word">The word that needs to be mapped to a service partition key.</param>
        /// <returns>A long representing the partition key.</returns>
        public static long GetPartitionKey(this Guid guid)
        {
            var first = guid.ToString().ToUpperInvariant().First();
            var offset = first - '0';
            if (offset <= 9)
            {
                return offset;
            }

            return first - 'A' + 10;
        }
    }
}
