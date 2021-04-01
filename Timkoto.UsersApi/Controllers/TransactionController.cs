using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.BaseClasses;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        private readonly IAppConfig _appConfig;
        
        private readonly IVerifier _verifier;

        public TransactionController(ITransactionService transactionService, IAppConfig appConfig, IVerifier verifier)
        {
            _transactionService = transactionService;
            _appConfig = appConfig;
            _verifier = verifier;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddTransactionRequest request)
        {
            
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                if (_appConfig.IsProduction)
                {
                    var httpOnlyAccessToken = Request.Cookies["HttpOnlyAccessToken"];
                    var verified = await _verifier.VerifyAccessToken(request.UserId, httpOnlyAccessToken);
                    if (!verified)
                    {
                        return StatusCode(401, GenericResponse.Create(false, HttpStatusCode.Unauthorized, Results.Unauthorized));
                    }
                }
                
                result = await _transactionService.AddTransaction(request, true, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("balance/{userId}")]
        [HttpGet]
        public async Task<IActionResult> Balance([FromRoute] long userId)
        {
            //if (!ModelState.IsValid )
            //{

            //}

            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _transactionService.Balance(userId, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }

        [Route("history/{userId}")]
        [HttpGet]
        public async Task<IActionResult> History([FromRoute] long userId)
        {
            var messages = new List<string>();
            GenericResponse result;

            try
            {
                result = await _transactionService.History(userId, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
