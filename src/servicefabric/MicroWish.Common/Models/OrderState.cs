﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{    
    public enum OrderState
    {
        Created,
        Canceled,
        Finalized,
        Delivered,
        Failed
    }
}
