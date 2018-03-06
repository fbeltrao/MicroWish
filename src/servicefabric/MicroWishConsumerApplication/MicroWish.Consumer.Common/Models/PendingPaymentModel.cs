using System.Runtime.Serialization;

namespace MicroWish.Consumer.Models
{
    [DataContract]
    public class PendingPaymentModel
    {
        [DataMember]
        public string CreditCardType { get; set; }

        [DataMember]
        public string CreditCardNumber { get; set; }

        [DataMember]
        public int ExpirationMonth { get; set; }

        [DataMember]
        public int ExpirationYear { get; set; }
    }
}
