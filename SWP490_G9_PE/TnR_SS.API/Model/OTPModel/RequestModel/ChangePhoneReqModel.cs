﻿using System.ComponentModel.DataAnnotations;

namespace TnR_SS.API.Model.OTPModel.RequestModel
{
    public class ChangePhoneReqModel
    {
        [Required]
        [MinLength(10)]
        public string NewPhoneNumber { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [Compare("CurrentPassword", ErrorMessage = "The new password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }
    }
}