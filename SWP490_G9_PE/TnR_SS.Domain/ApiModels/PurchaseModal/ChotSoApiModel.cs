﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TnR_SS.Domain.ApiModels.PurchaseModal
{
    public class ChotSoApiModel
    {
        public int ID { get; set; }
        public double CommissionPercent { get; set; }

        [Required]
        public bool IsPaid { get; set; } = false;
    }
}
