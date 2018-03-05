using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Order.Contract.Models
{
    public enum OrderState
    {
        Created,
        Canceled,
        Finalized,
        Delivered
    }
}
