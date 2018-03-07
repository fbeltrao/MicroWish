using MicroWish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Contracts
{
    public interface IVendorAPIClient
    {
        Task<VendorModel> Get(Guid vendorId);
    }
}
