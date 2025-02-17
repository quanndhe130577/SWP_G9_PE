﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TnR_SS.API.Common.Response;
using TnR_SS.Domain.ApiModels.PondOwnerModel;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Supervisor;
using System.Text.RegularExpressions;
using TnR_SS.API.Common.Token;

namespace TnR_SS.API.Controllers
{
    [Authorize]
    [Route("api/pondOwner")]
    [ApiController]
    public class PondOwnerController : ControllerBase
    {
        private readonly ITnR_SSSupervisor _tnrssSupervisor;
        private readonly IMapper _mapper;

        public PondOwnerController(ITnR_SSSupervisor tnrssSupervisor, IMapper mapper)
        {
            _tnrssSupervisor = tnrssSupervisor;
            _mapper = mapper;
        }

        /*[HttpGet]
        [Route("getAll")]
        public ResponseModel GetAll()
        {
            var rs = _tnrssSupervisor.PondOwner.GetAll().Select(po => _mapper.Map<PondOwnerResModel>(po)).ToList();
            return new ResponseBuilder<List<PondOwnerResModel>>().Success("Get Info Success").WithData(rs).ResponseModel;
        }*/
        [HttpGet]
        [Route("getall/{traderId}")]
        public ResponseModel GetByTraderId(int traderId)
        {
            var rs = _tnrssSupervisor.GetPondOwnerByTraderId(traderId);
            return new ResponseBuilder<List<PondOwnerApiModel>>().Success("Lấy thông tin chủ ao thành công").WithData(rs).ResponseModel;
        }

        [HttpPost]
        [Route("create")]
        public async Task<ResponseModel> Create(PondOwnerApiModel pondOwner)
        {
            var valid = Valid(pondOwner);
            if (valid.IsValid)
            {
                await _tnrssSupervisor.AddPondOwnerAsync(pondOwner);
                return new ResponseBuilder<List<PondOwnerApiModel>>().Success("Thêm thành công").ResponseModel;
            }
            else
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Error(valid.Message).ResponseModel;
            }
        }

        [HttpPost]
        [Route("delete/{id}")]
        public async Task<ResponseModel> Delete(int id)
        {
            PondOwner pondOwner = await _tnrssSupervisor.GetPondOwner(id);
            if (pondOwner == null)
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Error("Không tìm thấy chủ ao").ResponseModel;
            }
            int count = await _tnrssSupervisor.DeletePondOwner(pondOwner);
            if (count > 0)
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Success("Xoá thành công").ResponseModel;
            }
            else
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Error("Xoá thất bại").ResponseModel;
            }
        }


        [HttpPost]
        [Route("update")]
        public async Task<ResponseModel> Update(PondOwnerApiModel pondOwner)
        {
            PondOwner po = await _tnrssSupervisor.GetPondOwner(pondOwner.ID);
            if (po == null)
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Error("Không tìm thấy chủ ao").ResponseModel;
            }
            var valid = Valid(pondOwner);
            if (valid.IsValid)
            {
                var traderId = TokenManagement.GetUserIdInToken(HttpContext);
                await _tnrssSupervisor.EditPondOwner(pondOwner);
                return new ResponseBuilder<List<PondOwnerApiModel>>().Success("Cập nhật thành công").WithData(_tnrssSupervisor.GetPondOwnerByTraderId(traderId)).ResponseModel;
            }
            else
            {
                return new ResponseBuilder<List<PondOwnerApiModel>>().Error(valid.Message).ResponseModel;
            }
        }

        public static PondOwnerValidModel Valid(PondOwnerApiModel pondOwner)
        {
            Regex phoneNumberRegex = new Regex(@"^(84|0[3|5|7|8|9])+([0-9]{8})$");
            if (pondOwner.Name == null)
            {
                return new PondOwnerValidModel() { IsValid = false, Message = "Tên không được để trống" };
            }
            if (pondOwner.Address == null)
            {
                return new PondOwnerValidModel() { IsValid = false, Message = "Địa chỉ không được để trống" };

            }
            if (pondOwner.PhoneNumber == null)
            {
                return new PondOwnerValidModel() { IsValid = false, Message = "Điện thoại không được để trống" };
            }
            if (!phoneNumberRegex.IsMatch(pondOwner.PhoneNumber))
            {
                return new PondOwnerValidModel() { IsValid = false, Message = "Điện thoại không đúng định dạng" };
            }
            if (pondOwner.TraderID == 0)
            {
                return new PondOwnerValidModel() { IsValid = false, Message = "Người bán cá không được để trống" };
            }
            return new PondOwnerValidModel() { IsValid = true, Message = "Cập nhật thành công" };
        }

    }
}