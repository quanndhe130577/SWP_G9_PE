﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TnR_SS.Domain.ApiModels.EmployeeModel
{
    public class EmployeeApiModel
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        //public string LastName { get; set; }
        
        public DateTime DOB { get; set; }

        [RegularExpression(@"(84|0[3|5|7|8|9])+([0-9]{8})\b", ErrorMessage = "Phone Number invalid")]
        [MaxLength(11)]
        [MinLength(10)]
        [Required]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        /*protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            
            return base.IsValid(value, validationContext);
        }*/
    }
}
