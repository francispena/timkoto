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
        public async Task<IActionResult> Post([FromBody] AddTransactionRequest newTransaction, [FromHeader] Guid traceId)
        {
            //if (!ModelState.IsValid )
            //{
                
            //}

            var messages = new List<string>();
            ResponseBase result;

            try
            {
                result = await _transactionService.AddTransaction(newTransaction, traceId, messages);

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                result = ResponseBase.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                //TODO: logging
            }
        }
    }
}
