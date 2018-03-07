using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    [DataContract]
    public class PaymentModel : ICloneable
    {
        [DataMember]
        public string CreditCardType { get; set; }

        [DataMember]
        public string CreditCardNumber { get; set; }

        [DataMember]
        public int ExpirationMonth { get; set; }

        [DataMember]
        public int ExpirationYear { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public PaymentStatus Status { get; set; }

        [DataMember]
        public DateTime? ProcessingDate { get; set; }

        [DataMember]
        public decimal Value { get; set; }


        public PaymentModel()
        {

        }

        public PaymentModel(PaymentModel other)
        {
            this.CreditCardType = other.CreditCardType;
            this.CreditCardNumber = other.CreditCardNumber;
            this.ExpirationMonth = other.ExpirationMonth;
            this.ExpirationYear = other.ExpirationYear;
            this.Status = other.Status;
            this.ProcessingDate = other.ProcessingDate;
            this.Value = other.Value;
        }

        public PaymentModel Clone()
        {
            return new PaymentModel(this);
        }

        object ICloneable.Clone()
        {
            return new PaymentModel(this);
        }
    }
}
