using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddTransactionRequest request)
        {
            //if (!ModelState.IsValid )
            //{
                
            //}

            var messages = new List<string>();
            GenericResponse result;

            try
            {
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
