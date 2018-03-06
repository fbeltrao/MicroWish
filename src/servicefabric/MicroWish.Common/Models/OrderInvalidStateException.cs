using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    [Serializable]
    public class OrderInvalidStateException : Exception
    {
        public OrderInvalidStateException()
        {
        }

        public OrderInvalidStateException(string message) : base(message)
        {
        }

        public OrderInvalidStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrderInvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
