﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.AccountModel.ResponseModel;
using TnR_SS.Domain.ApiModels.BuyerModel;
using TnR_SS.Domain.ApiModels.DebtModel;
using TnR_SS.Domain.ApiModels.FishTypeModel;
using TnR_SS.Domain.ApiModels.PurchaseModal;
using TnR_SS.Domain.ApiModels.TransactionDetailModel;
using TnR_SS.Domain.ApiModels.TransactionModel;
using TnR_SS.Domain.ApiModels.UserInforModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        public async Task<List<DebtApiModel>> GetAllDebtTraderAsync(int traderId)
        {
            PondOwner pondOwner = new();
            List<DebtApiModel> list = new();

            var listPurchase = _unitOfWork.Purchases.GetAll(x => x.TraderID == traderId)
                //.Where(x => x.isCompleted == Entities.PurchaseStatus.Completed && x.isPaid == false)
                .Where(x => x.isCompleted == Entities.PurchaseStatus.Completed && x.SentMoney < x.PayForPondOwner)
                .OrderByDescending(x => x.Date).ThenByDescending(x => x.ID);
            var user = await _unitOfWork.UserInfors.FindAsync(traderId);
            foreach (var purchase in listPurchase)
            {
                DebtApiModel model = new();
                pondOwner = await _unitOfWork.PondOwners.FindAsync(purchase.PondOwnerID);
                model.Creditors = pondOwner.Name;
                model.Debtor = user.LastName + " " + user.FirstName;
                //model.DebtMoney = purchase.PayForPondOwner;
                model.DebtMoney = Math.Round(purchase.PayForPondOwner - purchase.SentMoney);
                model.Date = purchase.Date;

                list.Add(model);
            }

            return list.OrderByDescending(x => x.Date).ToList();
        }

        public async Task<List<DebtApiModel>> GetAllDebtWRAsync(int userId, DateTime? date)
        {
            List<DebtApiModel> list = new();
            var roleUser = await _unitOfWork.UserInfors.GetRolesAsync(userId);
            List<TransactionDetail> listTranDe = new List<TransactionDetail>();

            if (roleUser.Contains(RoleName.WeightRecorder))
            {
                listTranDe = _unitOfWork.TransactionDetails.GetAllByWcIDAndDate(userId, date).Where(x => x.IsPaid == false).ToList();
            }
            else if (roleUser.Contains(RoleName.Trader))
            {
                listTranDe = _unitOfWork.TransactionDetails.GetAllByTraderIdAndDate(userId, date).Where(x => x.IsPaid == false).ToList();
            }
            else
            {
                throw new Exception("Người mua không tồn tại !!");
            }
            var user = await _unitOfWork.UserInfors.FindAsync(userId);

            foreach (var td in listTranDe)
            {
                DebtApiModel model = new();

                model.Creditors = user.LastName + " " + user.FirstName;
                var buyer = await _unitOfWork.Buyers.FindAsync(td.BuyerId);
                model.Debtor = buyer != null ? _mapper.Map<Buyer, BuyerApiModel>(buyer).Name : null;
                model.DebtMoney = Math.Round(td.SellPrice);
                model.Date = _mapper.Map<Transaction, TransactionResModel>(await _unitOfWork.Transactions.FindAsync(td.TransId)).Date;

                list.Add(model);
            }

            return list.OrderByDescending(x => x.Date).ToList();
        }

        public async Task<List<DebtApiModel>> GetDebtAsync(int userId, DateTime? date)
        {
            var roleUser = await _unitOfWork.UserInfors.GetRolesAsync(userId);
            if (roleUser.Contains(RoleName.WeightRecorder))
            {
                return await GetAllDebtWRAsync(userId, date);
            }
            else if (roleUser.Contains(RoleName.Trader))
            {
                return await GetAllDebtTraderAsync(userId);
            }
            return null;
        }

        public async Task<List<DebtTraderApiModel>> GetAllDebtTransactionOfTrader(int id)
        {
            UserResModel user = await GetUserByIdAsync(id);
            List<TransactionDetail> transactionDetails = new List<TransactionDetail>();
            List<CloseTransactionDetail> closeTransactionDetails = new List<CloseTransactionDetail>();
            if (user.RoleName == "Trader")
            {
                foreach (Transaction transaction in _unitOfWork.Transactions.GetAll(filter: t => t.TraderId == id && t.WeightRecorderId == null, orderBy: ps => ps.OrderByDescending(p => p.Date)))
                {
                    if (transaction.isCompleted == TransactionStatus.Completed)
                    {
                        closeTransactionDetails.AddRange(_unitOfWork.CloseTransactionDetails.GetAll(td => td.TransactionId == transaction.ID && td.IsPaid == false));
                    }
                    else
                    {
                        transactionDetails.AddRange(_unitOfWork.TransactionDetails.GetAll(td => td.TransId == transaction.ID && td.IsPaid == false));
                    }
                }
            }
            else
            {
                foreach (Transaction transaction in _unitOfWork.Transactions.GetAll(filter: t => t.WeightRecorderId == id, orderBy: ps => ps.OrderByDescending(p => p.Date)))
                {
                    if (transaction.isCompleted == TransactionStatus.Completed)
                    {
                        closeTransactionDetails.AddRange(_unitOfWork.CloseTransactionDetails.GetAll(td => td.TransactionId == transaction.ID && td.IsPaid == false));
                    }
                    else
                    {
                        transactionDetails.AddRange(_unitOfWork.TransactionDetails.GetAll(td => td.TransId == transaction.ID && td.IsPaid == false));
                    }
                }
            }

            var roleUser = await _unitOfWork.UserInfors.GetRolesAsync(id);

            List<DebtTraderApiModel> debtTraderApiModels = new List<DebtTraderApiModel>();
            foreach (TransactionDetail transactionDetail in transactionDetails)
            {
                Buyer buyer = await _unitOfWork.Buyers.FindAsync(transactionDetail.BuyerId);
                FishType fishType = await _unitOfWork.FishTypes.FindAsync(transactionDetail.FishTypeId);
                Transaction transaction = await _unitOfWork.Transactions.FindAsync(transactionDetail.TransId);
                debtTraderApiModels.Add(new DebtTraderApiModel()
                {
                    ID = transactionDetail.ID,
                    Partner = buyer == null ? "" : (roleUser.Contains(RoleName.Trader) ? "Người mua: " + buyer.Name : buyer.Name),
                    FishName = fishType == null ? null : fishType.FishName,
                    Weight = Math.Round(transactionDetail.Weight, 2),
                    Trader = user.FirstName + " " + user.LastName,
                    Amount = Math.Round(transactionDetail.SellPrice * transactionDetail.Weight),
                    Date = transaction.Date,
                    Status = false
                });
            }

            foreach (CloseTransactionDetail transactionDetail in closeTransactionDetails)
            {
                //Buyer buyer = await _unitOfWork.Buyers.FindAsync(transactionDetail.BuyerId);
                //FishType fishType = await _unitOfWork.FishTypes.FindAsync(transactionDetail.FishTypeId);
                Transaction transaction = await _unitOfWork.Transactions.FindAsync(transactionDetail.TransactionId);
                debtTraderApiModels.Add(new DebtTraderApiModel()
                {
                    ID = transactionDetail.ID,
                    //Partner = buyer == null ? "" : (roleUser.Contains(RoleName.Trader) ? "Người mua: " + buyer.Name : buyer.Name),
                    Partner = (roleUser.Contains(RoleName.Trader) ? "Người mua: " + transactionDetail.BuyerName : transactionDetail.BuyerName),
                    //FishName = fishType == null ? null : fishType.FishName,
                    FishName = transactionDetail.FishName,
                    Weight = Math.Round(transactionDetail.Weight, 2),
                    Trader = user.FirstName + " " + user.LastName,
                    Amount = Math.Round(transactionDetail.SellPrice * transactionDetail.Weight),
                    Date = transaction.Date,
                    Status = true
                });
            }


            if (roleUser.Contains(RoleName.Trader))
            {
                var listTran = _unitOfWork.Transactions.GetAll(x => x.TraderId == id && x.WeightRecorderId != null).OrderByDescending(x => x.Date);
                int count = -1;
                foreach (var tran in listTran)
                {
                    var totalWeight = await _unitOfWork.Transactions.GetTotalWeightAsync(tran.ID);
                    var totalMoney = await _unitOfWork.Transactions.GetTotalMoneyAsync(tran.ID) - totalWeight * tran.CommissionUnit;
                    var sentMoney = tran.SentMoney;

                    if (sentMoney < totalMoney)
                    {
                        var wr = await _unitOfWork.UserInfors.FindAsync(tran.WeightRecorderId);
                        debtTraderApiModels.Add(new DebtTraderApiModel()
                        {
                            ID = count--,
                            Partner = "Chủ bến: " + wr.LastName,
                            FishName = "Tiền bán cá",
                            Weight = Math.Round(totalWeight, 2),
                            Trader = "",
                            Amount = Math.Round(totalMoney - sentMoney),
                            Date = tran.Date,
                            Status = true
                        });
                    }
                }
            }

            return debtTraderApiModels.OrderByDescending(d => d.Date).ToList();
        }

        public async Task UpdateDebtTransationDetail(int userId, int tranDeid)
        {
            UserResModel user = await GetUserByIdAsync(userId);
            TransactionDetail transactionDetail = await _unitOfWork.TransactionDetails.FindAsync(tranDeid);
            CloseTransactionDetail closeTransactionDetail = await _unitOfWork.CloseTransactionDetails.FindAsync(tranDeid);
            if (transactionDetail != null)
            {
                if (user.RoleName == "Trader")
                {
                    Transaction transaction = await _unitOfWork.Transactions.FindAsync(transactionDetail.TransId);
                    if (transaction.TraderId == userId)
                    {
                        transactionDetail.IsPaid = true;
                        _unitOfWork.TransactionDetails.Update(transactionDetail);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }
                else
                {
                    Transaction transaction = await _unitOfWork.Transactions.FindAsync(transactionDetail.TransId);
                    if (transaction.WeightRecorderId == userId)
                    {
                        transactionDetail.IsPaid = true;
                        _unitOfWork.TransactionDetails.Update(transactionDetail);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }
            }
            if (closeTransactionDetail != null)
            {
                if (user.RoleName == "Trader")
                {
                    Transaction transaction = await _unitOfWork.Transactions.FindAsync(closeTransactionDetail.TransactionId);
                    if (transaction.TraderId == userId)
                    {
                        closeTransactionDetail.IsPaid = true;
                        _unitOfWork.CloseTransactionDetails.Update(closeTransactionDetail);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }
                else
                {
                    Transaction transaction = await _unitOfWork.Transactions.FindAsync(closeTransactionDetail.TransactionId);
                    if (transaction.WeightRecorderId == userId)
                    {
                        closeTransactionDetail.IsPaid = true;
                        _unitOfWork.CloseTransactionDetails.Update(closeTransactionDetail);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }
            }
        }

        public async Task<List<DebtTraderApiModel>> GetAllDebtPurchaseOfTrader(int id)
        {
            UserResModel trader = await GetUserByIdAsync(id);
            List<DebtTraderApiModel> debtTraderApiModels = new List<DebtTraderApiModel>();
            foreach (Purchase purchase in _unitOfWork.Purchases.GetAll(filter: p => p.TraderID == id && /*p.isPaid == false*/ p.SentMoney < p.PayForPondOwner && p.isCompleted == PurchaseStatus.Completed,
              orderBy: ps => ps.OrderByDescending(p => p.Date)))
            {
                PondOwner pondOwner = await _unitOfWork.PondOwners.FindAsync(purchase.PondOwnerID);
                double amount = await CalculatePayForPondOwnerAsync(purchase.ID, purchase.Commission);
                debtTraderApiModels.Add(new DebtTraderApiModel()
                {
                    ID = purchase.ID,
                    Partner = pondOwner.Name,
                    Trader = trader.FirstName + " " + trader.LastName,
                    Amount = Math.Round(amount - purchase.SentMoney),
                    Date = purchase.Date
                });
            }
            return debtTraderApiModels.OrderByDescending(x => x.Date).ToList();
        }
        public async Task UpdateDebtPurchaseDetail(int userId, int id, int amount)
        {
            Purchase purchase = await _unitOfWork.Purchases.FindAsync(id);
            if (purchase != null)
            {
                if (purchase.TraderID == userId)
                {
                    //purchase.isPaid = true;
                    //purchase.SentMoney = purchase.PayForPondOwner;
                    purchase.SentMoney += amount;
                    _unitOfWork.Purchases.Update(purchase);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
        }

        public async Task<List<GetDebtForWrWithTraderResModel>> GetAllDebtTransactionOfWRWithTraderAsync(int userId)
        {
            var roleUser = await _unitOfWork.UserInfors.GetRolesAsync(userId);
            if (roleUser.Contains(RoleName.WeightRecorder))
            {
                List<GetDebtForWrWithTraderResModel> list = new List<GetDebtForWrWithTraderResModel>();
                var listTran = _unitOfWork.Transactions.GetAll(x => x.WeightRecorderId == userId).OrderByDescending(x => x.Date);
                foreach (var tran in listTran)
                {
                    var totalMoney = await _unitOfWork.Transactions.GetTotalMoneyAsync(tran.ID) - await _unitOfWork.Transactions.GetTotalWeightAsync(tran.ID) * tran.CommissionUnit;
                    var sentMoney = tran.SentMoney;
                    if (sentMoney < totalMoney)
                    {
                        var trader = await _unitOfWork.UserInfors.FindAsync(tran.TraderId);
                        GetDebtForWrWithTraderResModel model = new GetDebtForWrWithTraderResModel()
                        {
                            Id = tran.ID,
                            Date = tran.Date,
                            SentMoney = Math.Round(sentMoney),
                            Amount = totalMoney - sentMoney,
                            Partner = trader != null ? trader.LastName : ""
                        };

                        list.Add(model);
                    }
                }

                return list;
            }
            else
            {
                throw new Exception("Không có thông tin !!!");
            }

        }

        public async Task UpdateDebtTransactionOfWRWithTrader(UpdateDebtWrWithTraderReqModel apiModel, int wrId)
        {
            var tran = await _unitOfWork.Transactions.FindAsync(apiModel.Id);
            if (tran != null && tran.WeightRecorderId == wrId)
            {
                tran.SentMoney += apiModel.Amount;
                _unitOfWork.Transactions.Update(tran);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Không có thông tin !!!");
            }
        }
    }
}
