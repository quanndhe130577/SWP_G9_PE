﻿using System.ComponentModel.DataAnnotations;

namespace TnR_SS.Domain.ApiModels.AccountModel.RequestModel
{
    public class ResetPasswordReqModel
    {
        [Required]
        public int OTPID { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ResetToken { get; set; }
    }
}
