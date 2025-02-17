﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TnR_SS.API.Common.ImgurAPI;
using TnR_SS.API.Common.Response;
using TnR_SS.API.Common.Token;
using TnR_SS.Domain.ApiModels.AccountModel.RequestModel;
using TnR_SS.Domain.ApiModels.AccountModel.ResponseModel;
using TnR_SS.Domain.Entities;
using TnR_SS.Domain.Supervisor;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TnR_SS.API.Controller
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITnR_SSSupervisor _tnrssSupervisor;

        public AccountController(ITnR_SSSupervisor tnrssSupervisor)
        {
            _tnrssSupervisor = tnrssSupervisor;
        }

        #region Register      
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ResponseModel> Register(RegisterUserReqModel userData)
        {
            if (ModelState.IsValid)
            {
                //check OTP for phoneNumber
                if (!await _tnrssSupervisor.CheckOTPDoneAsync(userData.OTPID, userData.PhoneNumber))
                {
                    return new ResponseBuilder().Error("Truy cập bị từ chối").ResponseModel;
                }

                if (_tnrssSupervisor.CheckUserPhoneExists(userData.PhoneNumber))
                {
                    return new ResponseBuilder().Error("Số điện thoại không tồn tại").ResponseModel;
                }

                bool checkRoleExists = await _tnrssSupervisor.RoleExistsAsync(userData.RoleNormalizedName);
                if (!checkRoleExists)
                {
                    return new ResponseBuilder().Error("Vai trò không tồn tại").ResponseModel;
                }

                string avatarLink = await ImgurAPI.UploadImgurAsync(userData.AvatarBase64);

                var result = await _tnrssSupervisor.CreateUserAsync(userData, avatarLink);
                if (result.Succeeded)
                {
                    return new ResponseBuilder().Success("Đăng ký thành công").ResponseModel;
                }

                var errors = result.Errors.Select(x => x.Description).ToList();
                return new ResponseBuilder().Errors(errors).ResponseModel;
            }

            return new ResponseBuilder().Error("Thông tin không chính xác").ResponseModel;
        }
        #endregion

        #region Login
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ResponseModel> Login([FromBody] LoginReqModel userData)
        {
            if (ModelState.IsValid)
            {
                var user = _tnrssSupervisor.GetUserByPhoneNumber(userData.PhoneNumber);
                if (user is null)
                {
                    return new ResponseBuilder().Error("Không tìm thấy tài khoản").ResponseModel;
                }

                var userResModel = await _tnrssSupervisor.SignInWithPasswordAsync(user, userData.Password);
                if (userResModel != null)
                {
                    var token = TokenManagement.GetTokenUser(user.Id, userResModel.RoleName);
                    LoginResModel rlm = new LoginResModel()
                    {
                        Token = token,
                        User = userResModel
                    };
                    return new ResponseBuilder<LoginResModel>().Success("Đăng nhập thành công").WithData(rlm).ResponseModel;
                }

                return new ResponseBuilder().Error("Số điện thoại hoặc mật khẩu không đúng").ResponseModel;
            }

            return new ResponseBuilder().Error("Đăng nhập thất bại. Hãy kiểm tra lại thông tin tài khoản").ResponseModel;
        }
        #endregion

        #region update user
        [HttpPost("user/update/{id}")]
        //[Route("update")]
        public async Task<ResponseModel> UpdateUserInfor(int id, UpdateUserReqModel userData)
        {
            if (!TokenManagement.CheckUserIdFromToken(HttpContext, id))
            {
                return new ResponseBuilder().Error("Sai thông tin tài khoản").ResponseModel;
            }

            string avatarLink = await ImgurAPI.UploadImgurAsync(userData.AvatarBase64);
            var result = await _tnrssSupervisor.UpdateUserAsync(userData, id, avatarLink);

            if (result.Succeeded)
            {
                var userResModel = await _tnrssSupervisor.GetUserResModelByIdAsync(id);
                return new ResponseBuilder<UserResModel>().Success("Cập nhật thành công").WithData(userResModel).ResponseModel;
            }

            var errors = result.Errors.Select(x => x.Description).ToList();
            return new ResponseBuilder().Errors(errors).ResponseModel;

        }
        #endregion

        #region change password
        [HttpPost("user/change-password/{id}")]
        public async Task<ResponseModel> ChangePassword(int id, [FromBody] ChangePasswordReqModel changePasswordModel)
        {
            if (ModelState.IsValid)
            {
                if (!TokenManagement.CheckUserIdFromToken(HttpContext, id))
                {
                    return new ResponseBuilder().Error("Sai thông tin tài khoản").ResponseModel;
                }

                var userInfor = await _tnrssSupervisor.GetUserByIdAsync(id);
                if (userInfor is null)
                {
                    return new ResponseBuilder().Error("Tài khoản không tồn tại").ResponseModel;
                }

                var result = await _tnrssSupervisor.ChangeUserPasswordAsync(userInfor.UserID, changePasswordModel.CurrentPassword, changePasswordModel.NewPassword);

                if (result.Succeeded)
                {
                    //await _signInManager.RefreshSignInAsync(userInfor);
                    var token = TokenManagement.GetTokenUser(userInfor.UserID, userInfor.RoleName);
                    LoginResModel rlm = new LoginResModel()
                    {
                        Token = token,
                        User = await _tnrssSupervisor.GetUserResModelByIdAsync(id)
                    };

                    return new ResponseBuilder<LoginResModel>().Success("Cập nhật thành công").WithData(rlm).ResponseModel;
                }
                else
                {
                    var errors = result.Errors.Select(x => x.Description).ToList();
                    return new ResponseBuilder().Errors(errors).ResponseModel;
                }
            }

            return new ResponseBuilder().Error("Sai mật khẩu").ResponseModel;
        }
        #endregion

        #region Reset Password
        [HttpPost("user/reset-password")]
        [AllowAnonymous]
        public async Task<ResponseModel> ResetPassword(ResetPasswordReqModel resetData)
        {
            var userInfor = _tnrssSupervisor.GetUserByPhoneNumber(resetData.PhoneNumber);

            if (userInfor is null)
            {
                return new ResponseBuilder().WithCode(HttpStatusCode.NotFound).WithMessage("Số điện thoại không chính xác").ResponseModel;
            }

            if (!await _tnrssSupervisor.CheckOTPRightAsync(resetData.OTPID, resetData.Code, resetData.PhoneNumber))
            {
                return new ResponseBuilder().Error("Sai mã OTP").ResponseModel;
            }

            var result = await _tnrssSupervisor.ResetUserPasswordAsync(userInfor, resetData.ResetToken, resetData.NewPassword);
            if (result.Succeeded)
            {
                return new ResponseBuilder().Success("Tạo mới mật khẩu thành công").ResponseModel;
            }
            else
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                return new ResponseBuilder().Errors(errors).ResponseModel;
            }
        }
        #endregion

        #region change PhoneNumber 
        [HttpPost("user/change-phone-number/{id}")]
        public async Task<ResponseModel> ChangePhoneNumberOTP(int id, ChangePhoneNumberOTPReqModel modelData)
        {
            if (!TokenManagement.CheckUserIdFromToken(HttpContext, id))
            {
                return new ResponseBuilder().Error("Truy cập bị từ chối").ResponseModel;
            }

            if (_tnrssSupervisor.CheckUserPhoneExists(modelData.NewPhoneNumber))
            {
                return new ResponseBuilder().Error("Số điện thoại đã tồn tại").ResponseModel;
            }

            // if (await _tnrssSupervisor.CheckOTPRightAsync(modelData.OTPID, modelData.Code, modelData.NewPhoneNumber))
            if (true)
            {
                var rs = await _tnrssSupervisor.UpdatePhoneNumberAsync(id, modelData.NewPhoneNumber);
                if (rs.Succeeded)
                {
                    return new ResponseBuilder().Success("Thành công").ResponseModel;
                }

                var errors = rs.Errors.Select(x => x.Description).ToList();
                return new ResponseBuilder().Errors(errors).ResponseModel;
            }

            // return new ResponseBuilder().Error("Invalid OTP").ResponseModel;
        }
        #endregion

        #region logout
        [HttpGet("logout")]
        public async Task<ResponseModel> Logout()
        {
            await _tnrssSupervisor.SignOutAsync();
            return new ResponseBuilder().Success("Đăng suất thành công").ResponseModel;
        }
        #endregion

        #region Get User Info 
        [HttpGet("getUserInfo/{id}")]
        public async Task<ResponseModel> GetUserInfo(int id)
        {
            var user = await _tnrssSupervisor.GetUserByIdAsync(id);
            return new ResponseBuilder<UserResModel>().Success("Lấy thông tin tài khoản thành công").WithData(user).ResponseModel;
        }
        #endregion

    }
}
