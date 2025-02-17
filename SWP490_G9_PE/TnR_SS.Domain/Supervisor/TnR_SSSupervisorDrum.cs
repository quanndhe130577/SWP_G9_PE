﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.DrumModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        private List<DrumApiModel> GetListDrumByPurchaseDetail(PurchaseDetail purchaseDetail)
        {
            var listDrum = _unitOfWork.Drums.GetDrumsByPurchaseDetail(purchaseDetail);
            List<DrumApiModel> list = new List<DrumApiModel>();
            foreach (var item in listDrum)
            {
                list.Add(_mapper.Map<Drum, DrumApiModel>(item));
            }

            return list;
        }

        private List<DrumApiModel> GetListDrumByClosePurchaseDetail(ClosePurchaseDetail closePurchaseDetail)
        {
            var listDrum = _unitOfWork.Drums.GetDrumsByClosePurchaseDetail(closePurchaseDetail);
            List<DrumApiModel> list = new List<DrumApiModel>();
            foreach (var item in listDrum)
            {
                list.Add(_mapper.Map<Drum, DrumApiModel>(item));
            }

            return list;
        }

        public List<DrumApiModel> GetAllDrumByTruckId(int truckId)
        {
            return _unitOfWork.Drums.GetAll(x => x.TruckID == truckId).Select(x => _mapper.Map<Drum, DrumApiModel>(x)).ToList();
        }

        public async Task<int> CreateDrumAsync(DrumApiModel drumModel, int traderId)
        {
            var truck = _unitOfWork.Trucks.GetAll(x => x.TraderID == traderId).Select(x => x.ID);
            var drum = _mapper.Map<DrumApiModel, Drum>(drumModel);
            if (truck.Contains(drum.TruckID))
            {
                await _unitOfWork.Drums.CreateAsync(drum);
                await _unitOfWork.SaveChangeAsync();
                return drum.ID;
            }
            else
            {
                throw new Exception("Thông tin lồ không chính xác");
            }
        }

        public List<DrumApiModel> GetAllDrumByTraderId(int traderId)
        {
            return _unitOfWork.Drums.GetAllByTraderId(traderId).Select(x => _mapper.Map<Drum, DrumApiModel>(x)).ToList();
        }

        public async Task UpdateDrumAsync(DrumApiModel drum, int userId)
        {
            var drumEdit = await _unitOfWork.Drums.FindAsync(drum.ID);
            drumEdit = _mapper.Map<DrumApiModel, Drum>(drum, drumEdit);
            var listTruckId = _unitOfWork.Trucks.GetAll(x => x.TraderID == userId).Select(x => x.ID);
            if (listTruckId.Contains(drumEdit.TruckID))
            {
                var drumCheck = _unitOfWork.Drums.GetAllByTraderId(userId).Where(x => x.Number == drum.Number).FirstOrDefault();
                if (drumCheck != null && drumCheck.ID != drum.ID)
                {
                    throw new Exception("Xe tải được chọn đã có lồ với tên " + drum.Number);
                }

                _unitOfWork.Drums.Update(drumEdit);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Thông tin lồ không chính xác");
            }
        }

        public async Task DeleteDrumAsync(int drumId, int userId)
        {
            var drum = await _unitOfWork.Drums.FindAsync(drumId);
            var truck = _unitOfWork.Trucks.GetAll(x => x.TraderID == userId).Select(x => x.ID);
            var lk = _unitOfWork.LK_PurchaseDetail_Drums.GetAll(x => x.DrumID == drumId);
            if (lk != null && lk.Count() != 0)
            {
                throw new Exception("Thông tin lồ đang được sử dụng, không thể xóa !!!");
            }

            if (truck.Contains(drum.TruckID))
            {
                _unitOfWork.Drums.Delete(drum);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Thông tin lồ không chính xác");
            }
        }

        public DrumApiModel GetDetailDrum(int userId, int drumId)
        {
            var listEmp = GetAllDrumByTraderId(userId);
            var drumDetail = listEmp.Where(x => x.ID == drumId);
            foreach (var obj in listEmp)
            {
                if (obj.ID == drumId)
                {
                    return obj;
                }
                else
                {
                    throw new Exception("Thông tin lồ không chính xác");
                }
            }
            return null;
        }
    }
}
