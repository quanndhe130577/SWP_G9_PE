﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TnR_SS.Domain.Entities
{
    [Table("PondOwner")]
    public class PondOwner : BaseEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }


        [Required]
        public string PhoneNumber { get; set; }


        [Required]
        public int TraderID { get; set; }
        public UserInfor Trader { get; set; }

        public List<Purchase> Purchases { get; set; }
        //public List<FishType> FishTypes { get; set; }
    }
}
