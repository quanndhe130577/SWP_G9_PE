﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Repositories
{
    public interface IPurchaseRepository : IRepositoryBase<Purchase>
    {
        double GetTotalWeight(int purchaseId);
    }
}
