﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.FishTypeModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        public List<FishTypeWithPriceResModel> GetAllLastFishTypeByTraderId(int traderId)
        {
            /*var listType = _unitOfWork.FishTypes.GetAllAsync();
            List<FishTypeApiModel> list = new List<FishTypeApiModel>();
            foreach(var type in listType)
            {
                list.Add(_mapper.Map<FishType, FishTypeApiModel>(type));
            }
            return list;*/

            var fishTypes = _unitOfWork.FishTypes.GetAll(x => x.TraderID == traderId)
                .Select(x => _mapper.Map<FishType, FishTypeWithPriceResModel>(x)).ToList();

            foreach (var ft in fishTypes)
            {
                var fishTypePrice = _unitOfWork.FishTypePrices.GetTopDateByFishTypeID(ft.FTID);
                ft.Date = fishTypePrice.Date;
                ft.Price = fishTypePrice.Price;
            }

            return fishTypes;

        }

        public async Task CreateFishTypesAsync(List<FishTypeWithPriceResModel> listType)
        {
            foreach (var obj in listType)
            {
                var fishType = _mapper.Map<FishTypeWithPriceResModel, FishType>(obj);
                await _unitOfWork.FishTypes.CreateAsync(fishType);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateFishTypeAsync(FishTypeWithPriceResModel fishTypeModel)
        {
            var fishType = await _unitOfWork.FishTypes.FindAsync(fishTypeModel.FTID);
            fishType = _mapper.Map<FishTypeWithPriceResModel, FishType>(fishTypeModel, fishType);
            _unitOfWork.FishTypes.Update(fishType);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
