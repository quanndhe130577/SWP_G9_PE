﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TnR_SS.API.Common.Response;
using TnR_SS.API.Common.Token;
using TnR_SS.Domain.ApiModels.AdvanceSalaryModel;
using TnR_SS.Domain.ApiModels.EmployeeModel;
using TnR_SS.Domain.Supervisor;

namespace TnR_SS.API.Controllers
{
    [Route("api/advanceSalary")]
    [ApiController]
    public class AdvanceSalaryController : ControllerBase
    {
        private readonly ITnR_SSSupervisor _tnrssSupervisor;

        public AdvanceSalaryController(ITnR_SSSupervisor tnrssSupervisor)
        {
            _tnrssSupervisor = tnrssSupervisor;
        }

        [HttpGet("getall/{empId}")]
        public ResponseModel GetAll(int empId)
        {
            var list = _tnrssSupervisor.GetAllAdvanceSalary(empId);
            return new ResponseBuilder<List<AdvanceSalaryApiModel>>().Success("Lấy thông tin ứng trước lương").WithData(list).ResponseModel;
        }

        [HttpPost("create")]
        public async Task<ResponseModel> CreateAdvanceSalaryAsync(AdvanceSalaryApiModel employee)
        {
            await _tnrssSupervisor.CreateAdvanceSalary(employee);
            await _tnrssSupervisor.UpsertHistorySalary(employee.Date, employee.EmpId);
            return new ResponseBuilder().Success("Tạo ứng trước thành công").ResponseModel;
        }

        [HttpPost("update")]
        public async Task<ResponseModel> UpdateAdvanceSalaryAsync(AdvanceSalaryApiModel employee)
        {
            await _tnrssSupervisor.UpdateAdvanceSalary(employee);
            await _tnrssSupervisor.UpsertHistorySalary(employee.Date, employee.EmpId);
            return new ResponseBuilder().Success("Cập nhật thông ứng trước lương").ResponseModel;
        }

        [HttpPost("delete/{empId}")]
        public async Task<ResponseModel> DeleteAdvanceSalaryAsync(int empId)
        {
            var ed = await _tnrssSupervisor.GetAdvanceSalary(empId);
            await _tnrssSupervisor.DeleteAdvanceSalary(ed);
            await _tnrssSupervisor.UpsertHistorySalary(ed.Date, ed.EmpId);
            return new ResponseBuilder().Success("Xóa lương ứng trước thành công").ResponseModel;
        }
    }
}
