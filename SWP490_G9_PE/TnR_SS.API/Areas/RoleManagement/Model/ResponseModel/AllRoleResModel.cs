﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TnR_SS.API.Areas.RoleManagement.Model.ResponseModel
{
    public class AllRoleResModel
    {
        [Required]
        public string NormalizedName { get; set; }
        [Required]
        public string DisplayName { get; set; }
    }
}
