﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TnR_SS.API.Common.Response;
using TnR_SS.API.Common.Token;
using TnR_SS.Domain.ApiModels.PurchaseModal;
using TnR_SS.Domain.Supervisor;

namespace TnR_SS.API.Controllers
{
    [Route("api/purchase")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {

        private readonly ITnR_SSSupervisor _tnrssSupervisor;

        public PurchaseController(ITnR_SSSupervisor tnrssSupervisor)
        {
            _tnrssSupervisor = tnrssSupervisor;
        }

        #region create purchase
        [HttpPost("create")]
        //[Route("update")]
        public async Task<ResponseModel> CreatePurchase(PurchaseReqModel purchaseData)
        {
            if (!TokenManagement.CheckUserIdFromToken(HttpContext, purchaseData.TraderID))
            {
                return new ResponseBuilder().Error("Access denied").ResponseModel;
            }

            var newData = await _tnrssSupervisor.CreatePurchaseAndFishTypePriceAsync(purchaseData);

            return new ResponseBuilder<object>().Success("Create purchase success").WithData(new { purchaseData = newData }).ResponseModel;
        }
        #endregion
    }
}

