﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TnR_SS.Domain.Entities
{
    public class Purchase : BaseEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /*[Required]
        public double TotalAmount { get; set; }

        [Required]
        public double TotalWeight { get; set; }*/

        [Required]
        public double PayForPondOwner { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public double Commission { get; set; } // tiền trả cho môi giới

        /*[Required]
        public bool isPaid { get; set; } = false;*/

        public double SentMoney { get; set; } // tiền đã trả trước cho chủ ao
        public PurchaseStatus isCompleted { get; set; } = PurchaseStatus.Pending; // đã chốt sổ hay chưa

        public int? PondOwnerID { get; set; }

        public PondOwner PondOwner { get; set; }

        [Required]
        public int TraderID { get; set; }
        public UserInfor UserInfor { get; set; }

        public List<PurchaseDetail> PurchaseDetails { get; set; } = new List<PurchaseDetail>();
        public List<ClosePurchaseDetail> ClosePurchaseDetails { get; set; } = new List<ClosePurchaseDetail>();
        public List<FishType> FishTypes { get; set; }
    }

    public enum PurchaseStatus
    {
        Pending,
        Completed,
        Remain
    }
}
