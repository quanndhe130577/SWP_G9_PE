﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.BasketModel.ResponseModel;
using TnR_SS.Domain.ApiModels.DrumModel;
using TnR_SS.Domain.ApiModels.FishTypeModel;
using TnR_SS.Domain.ApiModels.LK_PurchaseDetail_DrumModel;
using TnR_SS.Domain.ApiModels.PurchaseDetailModel;
using TnR_SS.Domain.ApiModels.TruckModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        #region private method
        private async Task CreateLK(List<LK_PurchaseDetail_DrumApiModel> listDrum, int purchaseDetailId)
        {
            foreach (var item in listDrum)
            {
                var lk = _mapper.Map<LK_PurchaseDetail_DrumApiModel, LK_PurchaseDeatil_Drum>(item);
                lk.PurchaseDetailID = purchaseDetailId;
                await _unitOfWork.LK_PurchaseDetail_Drums.CreateAsync(lk);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        private async Task CreateLK(List<int> listDrumId, int purchaseDetailId)
        {
            if (listDrumId.Count == 0 || listDrumId == null)
            {
                throw new Exception("Chưa chọn Lồ để chứa cá !!!");
            }

            foreach (var item in listDrumId)
            {
                LK_PurchaseDeatil_Drum lk = new LK_PurchaseDeatil_Drum()
                {
                    DrumID = item,
                    PurchaseDetailID = purchaseDetailId,
                };
                /*var lk = _mapper.Map<LK_PurchaseDetail_DrumApiModel, LK_PurchaseDeatil_Drum>(item);
                lk.PurchaseDetailID = purchaseDetailId;*/
                await _unitOfWork.LK_PurchaseDetail_Drums.CreateAsync(lk);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        private async Task<double> GetPurchaseDetailPriceAsync(int purchaseDetailId)
        {
            var purchaseDetail = await _unitOfWork.PurchaseDetails.FindAsync(purchaseDetailId);
            var fishType = await _unitOfWork.FishTypes.FindAsync(purchaseDetail.FishTypeID);
            var basket = await _unitOfWork.Baskets.FindAsync(purchaseDetail.BasketId);
            //var listDrum = _unitOfWork.LK_PurchaseDeatil_Drums.GetAll(x => x.PurchaseDetailID == purchaseDetailId);
            double totalFishWeight = purchaseDetail.Weight - basket.Weight;
            return totalFishWeight > 0 ? fishType.Price * totalFishWeight : 0;
            //return 0;
        }

        private async Task<double> GetPurchaseDetailPriceAsync(PurchaseDetail purchaseDetail)
        {
            var fishType = await _unitOfWork.FishTypes.FindAsync(purchaseDetail.FishTypeID);
            var basket = await _unitOfWork.Baskets.FindAsync(purchaseDetail.BasketId);
            //var listDrum = _unitOfWork.LK_PurchaseDeatil_Drums.GetAll(x => x.PurchaseDetailID == purchaseDetailId);
            double totalFishWeight = purchaseDetail.Weight - (basket != null ? basket.Weight : 0);
            return totalFishWeight > 0 ? fishType.Price * totalFishWeight : 0;
            //return 0;
        }

        private double GetPurchaseDetailPrice(double fishTypePrice, double basketWeight, double totalWeight)
        {
            double totalFishWeight = totalWeight - basketWeight;
            return totalFishWeight > 0 ? fishTypePrice * totalFishWeight : 0;
        }

        #endregion

        /* private double GetPurchaseDetailWeight(int purchaseDetailId)
         {
             //return _unitOfWork.LK_PurchaseDeatil_Drums.GetAll(x => x.PurchaseDetailID == purchaseDetailId).Sum(x => x.Weight);
             return _unitOfWork.PurchaseDetails.FindAsync(purchaseDetailId).Result.Weight;
         }*/

        public async Task<int> CreatePurchaseDetailAsync(PurchaseDetailReqModel data, int traderId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var purchase = await _unitOfWork.Purchases.FindAsync(data.PurchaseId);
                        if (purchase == null || purchase.TraderID != traderId)
                        {
                            throw new Exception("Đơn mua không tồn tại !!!");
                        }

                        if (purchase.isCompleted.Equals(PurchaseStatus.Completed))
                        {
                            throw new Exception("Đơn mua đã được chốt sổ !!!");
                        }

                        var fishType = await _unitOfWork.FishTypes.FindAsync(data.FishTypeID);
                        if (fishType == null || fishType.PurchaseID != data.PurchaseId/*|| fishType.PondOwnerID != purchase.PondOwnerID*/)
                        {
                            throw new Exception("Giá cá không tồn tại !!!");
                        }

                        var basket = await _unitOfWork.Baskets.FindAsync(data.BasketId);
                        if (basket == null)
                        {
                            throw new Exception("Loại rổ không tồn tại !!!");
                        }

                        if (basket.Weight >= data.Weight)
                        {
                            throw new Exception("Cân nặng mã cân phải lớn hơn cân nặng của rổ !!!");
                        }

                        var purchaseDetail = _mapper.Map<PurchaseDetailReqModel, PurchaseDetail>(data);
                        //double totalFishWeight = data.ListDrum.Sum(x => x.Weight) - basket.Weight;
                        //purchaseDetail.BuyPrice = fishType.Price * totalFishWeight;
                        //purchaseDetail.Weight = data.Weight;
                        await _unitOfWork.PurchaseDetails.CreateAsync(purchaseDetail);
                        await _unitOfWork.SaveChangeAsync();
                        //create lk
                        await CreateLK(data.ListDrumId, purchaseDetail.ID);
                        await transaction.CommitAsync();
                        return purchaseDetail.ID;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                        //throw new Exception("Đã có lỗi xay ra, hãy thử lại sau");
                    }
                }
            });

        }

        public async Task<List<PurchaseDetailResModel>> GetAllPurchaseDetailAsync(int purchaseId)
        {
            var purchase = await _unitOfWork.Purchases.FindAsync(purchaseId);
            var lackOfData = false;
            if (purchase.isCompleted == PurchaseStatus.Completed)
            {
                var listClosePD = _unitOfWork.ClosePurchaseDetails.GetAllByPurchase(purchase);
                if (listClosePD.Count == 0)
                {
                    lackOfData = true;
                    purchase.isCompleted = PurchaseStatus.Pending;
                    _unitOfWork.Purchases.Update(purchase);
                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    List<PurchaseDetailResModel> list = new List<PurchaseDetailResModel>();
                    foreach (var item in listClosePD)
                    {
                        PurchaseDetailResModel data = _mapper.Map<ClosePurchaseDetail, PurchaseDetailResModel>(item);
                        data.ListDrum = GetListDrumByClosePurchaseDetail(item);
                        if (data.ListDrum.Count > 0)
                        {
                            data.Truck = _mapper.Map<Truck, TruckApiModel>(await _unitOfWork.Trucks.FindAsync(data.ListDrum.FirstOrDefault().TruckId));
                        }

                        list.Add(data);
                    }
                    return list;
                }
            }

            if (purchase.isCompleted == PurchaseStatus.Pending || purchase.isCompleted == PurchaseStatus.Remain || lackOfData)
            {
                var listPurchaseDetail = _unitOfWork.PurchaseDetails.GetAll(x => x.PurchaseId == purchaseId).OrderByDescending(x => x.ID);
                List<PurchaseDetailResModel> list = new List<PurchaseDetailResModel>();
                foreach (var item in listPurchaseDetail)
                {
                    PurchaseDetailResModel data = _mapper.Map<PurchaseDetail, PurchaseDetailResModel>(item);
                    data.Basket = _mapper.Map<Basket, BasketApiModel>(await _unitOfWork.Baskets.FindAsync(item.BasketId));
                    if (data.Basket == null)
                    {
                        data.Basket = new BasketApiModel()
                        {
                            Type = "Loại"
                        };
                    }

                    data.FishType = _mapper.Map<FishType, FishTypeApiModel>(await _unitOfWork.FishTypes.FindAsync(item.FishTypeID));
                    data.Price = GetPurchaseDetailPrice(data.FishType.Price, data.Basket != null ? data.Basket.Weight : 0, data.Weight);
                    data.ListDrum = GetListDrumByPurchaseDetail(item);
                    if (data.ListDrum.Count > 0)
                    {
                        data.Truck = _mapper.Map<Truck, TruckApiModel>(await _unitOfWork.Trucks.FindAsync(data.ListDrum.FirstOrDefault().TruckId));
                    }

                    list.Add(data);
                }
                return list;
            }

            return null;

        }

        public async Task UpdatePurchaseDetailAsync(PurchaseDetailReqModel data)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var purchaseDetail = await _unitOfWork.PurchaseDetails.FindAsync(data.Id);
                        if (purchaseDetail is null)
                        {
                            throw new Exception("Không tìm thấy đơn mua !!!");
                        }

                        var purchase = await _unitOfWork.Purchases.FindAsync(purchaseDetail.PurchaseId);
                        if (purchase.isCompleted.Equals(PurchaseStatus.Completed))
                        {
                            throw new Exception("Đơn mua đã được chốt, không thế thay đổi !!!");
                        }

                        purchaseDetail = _mapper.Map<PurchaseDetailReqModel, PurchaseDetail>(data, purchaseDetail);

                        // delete current LK
                        _unitOfWork.LK_PurchaseDetail_Drums.DeleteMany(x => x.PurchaseDetailID == data.Id);
                        await _unitOfWork.SaveChangeAsync();

                        // create new LK
                        await CreateLK(data.ListDrumId, data.Id);
                        await _unitOfWork.SaveChangeAsync();

                        await UpdatePayForPondOwnerAsync(purchaseDetail.PurchaseId);

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        //throw new Exception("Đã có lỗi xay ra, hãy thử lại sau");
                        throw;
                    }
                }
            });
        }

        public async Task DeletePurchaseDetailAsync(int traderId, int purchaseDetailId)
        {
            var purchaseDetail = await _unitOfWork.PurchaseDetails.FindAsync(purchaseDetailId);
            if (purchaseDetail == null)
            {
                throw new Exception("Mã cân mua không tồn tại !!!");
            }

            var purchase = await _unitOfWork.Purchases.FindAsync(purchaseDetail.PurchaseId);
            if (purchase.TraderID == traderId)
            {
                var strategy = _unitOfWork.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    using (var transaction = _unitOfWork.BeginTransaction())
                    {
                        try
                        {
                            _unitOfWork.LK_PurchaseDetail_Drums.RemoveLKByPurchaseDetailId(purchaseDetailId);
                            await _unitOfWork.SaveChangeAsync();
                            // remove close purchase detail
                            //await _unitOfWork.ClosePurchaseDetails.DeleteByPurchaseIdAsync(purchaseDetailId);
                            _unitOfWork.PurchaseDetails.DeleteById(purchaseDetailId);
                            await _unitOfWork.SaveChangeAsync();

                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });
            }
            else
            {
                throw new Exception("Mã cân mua không hợp lệ !!!");
            }
        }
    }
}
