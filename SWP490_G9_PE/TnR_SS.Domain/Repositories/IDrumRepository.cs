﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.DrumModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Repositories
{
    public interface IDrumRepository : IRepositoryBase<Drum>
    {
        List<Drum> GetAllByTraderId(int traderId);
        List<Drum> GetDrumsByPurchaseDetail(PurchaseDetail data);
        List<Drum> GetDrumsByPurchaseDetails(List<PurchaseDetail> purchaseDetails);
        List<Drum> GetDrumsByClosePurchaseDetail(ClosePurchaseDetail data);
    }
}
