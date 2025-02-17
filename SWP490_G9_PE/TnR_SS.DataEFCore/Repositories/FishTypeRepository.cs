﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Repositories;

namespace TnR_SS.DataEFCore.Repositories
{
    public class FishTypeRepository : RepositoryBase<FishType>, IFishTypeRepository
    {
        public FishTypeRepository(TnR_SSContext context) : base(context) { }
        public List<FishType> GetAllLastByTraderIdAndPondOwnerId(int traderId)
        {
            var rs = _context.Purchases.Where(x => x.isCompleted == PurchaseStatus.Completed && x.TraderID == traderId)
                .Join(
                    _context.FishTypes,
                    p => p.ID,
                    ft => ft.PurchaseID,
                    (p, ft) => ft
                ).AsEnumerable().OrderByDescending(x => x.Date).ThenByDescending(x => x.ID).GroupBy(x => x.FishName)
                .Select(x => x.First()).ToList();

            /*var rs = _context.FishTypes.AsEnumerable().Where(x => x.TraderID == traderId)
                .OrderByDescending(x => x.Date).ThenByDescending(x => x.ID).GroupBy(x => x.FishName)
                .Select(x => x.First()).ToList();*/
            return rs;
        }

        public List<FishType> GetAllByTraderId(int traderId)
        {
            var rs = _context.FishTypes.AsEnumerable().Where(x => x.TraderID == traderId && (x.PurchaseID != null || x.Date.Date >= DateTime.Now.AddDays(7)))
                .OrderByDescending(x => x.Date).ToList();
            return rs;
        }

        public async Task RemoveFishTypeByPurchaseId(int purchaseId)
        {
            var rs = _context.FishTypes.Where(x => x.PurchaseID == purchaseId);
            foreach (var item in rs)
            {
                var tranDe = _context.TransactionDetails.Where(x => x.FishTypeId == item.ID).FirstOrDefault();
                // nếu đã được bán
                if (tranDe != null)
                {
                    item.PurchaseID = null;
                    _context.FishTypes.UpdateRange(rs);
                }
                else
                {
                    _context.FishTypes.Remove(item);
                }
            }

            await _context.SaveChangesAsync();
        }

        public double GetTotalWeightOfFishType(int fishTypeId)
        {
            return (from p in _context.PurchaseDetails.Where(x => x.FishTypeID == fishTypeId)
                    from bk in _context.Baskets.Where(x => x.ID == p.BasketId).DefaultIfEmpty()
                    select new
                    {
                        purchaseDetail = p,
                        basket = bk
                    }).Sum(x => x.basket != null ? x.purchaseDetail.Weight - x.basket.Weight : x.purchaseDetail.Weight);

            /* return _context.PurchaseDetails.Where(x => x.FishTypeID == fishTypeId)
                 .Join(
                     _context.Baskets.DefaultIfEmpty(),
                     pd => pd.BasketId,
                     bk => bk.ID,
                     (pd, bk) => new
                     {
                         realWeight = bk != null ? pd.Weight - bk.Weight : pd.Weight
                     }
                 )
                 .Sum(x => x.realWeight);*/
        }

        public double GetSellWeightOfFishType(int fishTypeId)
        {
            return _context.TransactionDetails.Where(x => x.FishTypeId == fishTypeId).Sum(x => x.Weight);
        }

        public List<FishType> GetAllFishTypeByPurchaseIds(List<int> listPurchaseId)
        {
            return _context.FishTypes.AsEnumerable().Join(
                    listPurchaseId,
                    ft => ft.PurchaseID,
                    p => p,
                    (ft, p) => ft
                ).ToList();
        }

        public bool CheckFishTypeOfPurchaseInUse(int purchaseId)
        {
            var rs = _context.Transactions.Where(x => x.isCompleted == TransactionStatus.Pending).Join(
                    _context.TransactionDetails,
                    t => t.ID,
                    td => td.TransId,
                    (t, td) => new
                    {
                        fishTypeId = td.FishTypeId
                    }).Distinct().Join(
                        _context.FishTypes,
                        fti => fti.fishTypeId,
                        ft => ft.ID,
                        (fti, ft) => ft.PurchaseID
                    ).Where(x => x == purchaseId);

            if (rs == null || rs.Count() == 0)
            {
                // chưa được dùng
                return false;
            }

            return true;
        }

        public List<FishType> GetAllFishTypeForTransaction(int? traderId, int userId, DateTime date)
        {
            var userRole = _context.UserRoles.Where(x => x.UserId == userId).Join(
                    _context.RoleUsers,
                    ur => ur.RoleId,
                    ru => ru.Id,
                    (ur, ru) => ru.NormalizedName);

            List<int> listPurchaseId = new();
            if (userRole.Contains(RoleName.Trader))
            {
                listPurchaseId = _context.Purchases.Where(x => x.TraderID == userId && x.Date.Date == date.Date && x.isCompleted != PurchaseStatus.Pending).Select(x => x.ID).ToList();
            }
            else if (userRole.Contains(RoleName.WeightRecorder) && traderId != null)
            {
                listPurchaseId = _context.Purchases.Where(x => x.TraderID == traderId && x.Date.Date == date.Date && x.isCompleted != PurchaseStatus.Pending).Select(x => x.ID).ToList();
            }

            return GetAllInUseByPurchaseIds(listPurchaseId);
        }
        private List<FishType> GetAllInUseByPurchaseIds(List<int> listPurchaseId)
        {
            return _context.FishTypes.Join(
                   _context.PurchaseDetails,
                   ft => ft.ID,
                   pd => pd.FishTypeID,
                   (ft, pd) => new
                   {
                       fishType = ft,
                       purchaseDetail = pd
                   }).AsEnumerable().Join(
                        listPurchaseId,
                        lk => lk.fishType.PurchaseID,
                        pId => pId,
                        (lk, pId) => lk.fishType
                    ).Distinct().ToList();
        }

        public async Task ClearDataAsync()
        {
            var listFishIdInPur = _context.PurchaseDetails.Select(x => x.FishTypeID).Distinct();
            var listFishIdInTran = _context.TransactionDetails.Select(x => x.FishTypeId).Distinct();
            var listRemove = _context.FishTypes.Where(x => !listFishIdInPur.Contains(x.ID) && !listFishIdInTran.Contains(x.ID) && x.Date.Date != DateTime.Now.Date);
            _context.FishTypes.RemoveRange(listRemove);
            await _context.SaveChangesAsync();
        }

        public List<FishType> GetListFishTypeRemainByDay(DateTime date, int traderId)
        {
            return _context.Purchases.Where(x => x.TraderID == traderId && x.Date.Date == date.Date && x.isCompleted == PurchaseStatus.Remain)
                .Join(
                    _context.FishTypes,
                    p => p.ID,
                    ft => ft.PurchaseID,
                    (p, ft) => ft
                ).Distinct().ToList();
        }
    }
}

