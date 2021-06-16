﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.ApiModels.EmployeeModel;
using TnR_SS.Domain.Entities;

namespace TnR_SS.Domain.Supervisor
{
    public partial class TnR_SSSupervisor
    {
        public List<EmployeeApiModel> GetAllEmployeeByTraderId(int traderId)
        {
            var listEmp = _unitOfWork.Employees.GetAllEmployeeByTraderId(traderId);
            List<EmployeeApiModel> list = new();
            foreach (var type in listEmp)
            {
                list.Add(_mapper.Map<Employee, EmployeeApiModel>(type));
            }
            return list;
        }

        public async Task CreateEmployeesAsync(EmployeeApiModel employee, int traderId)
        {
            var obj = _mapper.Map<EmployeeApiModel, Employee>(employee);
            obj.TraderId = traderId;
            await _unitOfWork.Employees.CreateAsync(obj);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateEmployeeAsync(EmployeeApiModel employee, int traderId)
        {
            var empEdit = await _unitOfWork.Employees.FindAsync(employee.ID);
            empEdit = _mapper.Map<EmployeeApiModel, Employee>(employee, empEdit);
            if (empEdit.TraderId == traderId)
            {
                _unitOfWork.Employees.Update(empEdit);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Update fail");
            }
        }

        public async Task DeleteEmployeeAsync(int empId, int traderId)
        {
            var empEdit = await _unitOfWork.Employees.FindAsync(empId);
            if (empEdit.TraderId == traderId)
            {
                _unitOfWork.Employees.Delete(empEdit);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Update fail");
            }
        }

        public EmployeeApiModel GetDetailEmployee(int traderId, int empId)
        {
            var listEmp = _unitOfWork.Employees.GetAllEmployeeByTraderId(traderId);
            foreach (var obj in listEmp)
            {
                if (obj.ID == empId)
                {
                    return _mapper.Map<Employee, EmployeeApiModel>(obj);
                }
            }
            return null;
        }
    }
}