﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Repositories;

namespace TnR_SS.DataEFCore.Repositories
{
    public class HistorySalaryEmpRepository : RepositoryBase<HistorySalaryEmp>, IHistorySalaryEmpRepository
    {
        public HistorySalaryEmpRepository(TnR_SSContext context) : base(context) { }

        public List<HistorySalaryEmp> GetAllSalaryByEmpId(int empId)
        {
            var rs = _context.HistorySalaryEmps.AsEnumerable().Where(x => x.EmpId == empId)
                .OrderByDescending(x => x.StartDate).ToList();
            return rs;
        }
    }
}
