﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.BuyerModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        public List<BuyerApiModel> GetAllBuyerByWCId(int userId)
        {
            return _unitOfWork.Buyers.GetAll(x => x.SellerId == userId).Select(x => _mapper.Map<Buyer, BuyerApiModel>(x)).ToList();
            /*List<BuyerApiModel> buyers = new();
            foreach (var item in listBuyer)
            {
                buyers.Add(_mapper.Map<Buyer, BuyerApiModel>(item));
            }
            return buyers;*/
        }

        public async Task CreateBuyerAsync(BuyerApiModel model, int wcId)
        {
            var buyer = _mapper.Map<BuyerApiModel, Buyer>(model);
            var check = _unitOfWork.Buyers.GetAll(x => x.PhoneNumber == model.PhoneNumber && x.SellerId == wcId).FirstOrDefault();
            if (check != null)
            {
                throw new Exception("Người mua sử dụng số điện thoại này đã tồn tại !!!");
            }

            buyer.SellerId = wcId;
            await _unitOfWork.Buyers.CreateAsync(buyer);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateBuyerAsync(BuyerApiModel model, int wcId)
        {
            var buyer = await _unitOfWork.Buyers.FindAsync(model.ID);
            if (buyer.SellerId != wcId)
            {
                throw new Exception("Thông tin người mua không tồn tại !!!");
            }

            var check = _unitOfWork.Buyers.GetAll(x => x.PhoneNumber == model.PhoneNumber && x.SellerId == wcId).FirstOrDefault();
            if (check != null && check.ID != buyer.ID)
            {
                throw new Exception("Người mua sử dụng số điện thoại này đã tồn tại !!!");
            }

            buyer = _mapper.Map<BuyerApiModel, Buyer>(model, buyer);
            _unitOfWork.Buyers.Update(buyer);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteBuyerAsync(int buyerId, int wcId)
        {
            try
            {
                var buyer = await _unitOfWork.Buyers.FindAsync(buyerId);
                if (buyer != null && buyer.SellerId == wcId)
                {
                    _unitOfWork.Buyers.DeleteById(buyerId);
                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    throw new Exception("Thông tin người mua không tồn tại !!!");
                }
            }
            catch
            {
                throw new Exception("Thông tin người dùng đang được sử dụng, không thể xóa");
            }

        }

        public async Task<BuyerApiModel> GetDetailBuyerAsync(int buyerId, int wcId)
        {
            var buyerDetail = await _unitOfWork.Buyers.FindAsync(buyerId);
            if (buyerDetail.SellerId != wcId)
            {
                throw new Exception("Thông tin người mua không tồn tại !!!");
            }
            BuyerApiModel buyerApi = _mapper.Map<Buyer, BuyerApiModel>(buyerDetail);
            return buyerApi;

        }

        public List<BuyerApiModel> GetTop5BuyerByNameOrPhone(string input, int wcId)
        {
            if (input == null || input.Trim() == "")
            {
                return _unitOfWork.Buyers.GetAll(x => x.SellerId == wcId)
                .Take(5)
                .Select(x => _mapper.Map<Buyer, BuyerApiModel>(x)).ToList();
            }
            else
            {
                var rs = _unitOfWork.Buyers.GetAll(x => x.SellerId == wcId)
                .Where(x => x.Name.Contains(input, StringComparison.CurrentCultureIgnoreCase) || x.PhoneNumber.Contains(input)).Take(5)
                .Select(x => _mapper.Map<Buyer, BuyerApiModel>(x)).ToList();
                return rs;
            }
        }
    }
}
