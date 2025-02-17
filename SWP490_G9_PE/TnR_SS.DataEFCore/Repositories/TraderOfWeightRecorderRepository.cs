﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Repositories;

namespace TnR_SS.DataEFCore.Repositories
{
    public class TraderOfWeightRecorderRepository : RepositoryBase<TraderOfWeightRecorder>, ITraderOfWeightRecorderRepository
    {
        public TraderOfWeightRecorderRepository(TnR_SSContext context) : base(context) { }
    }
}
