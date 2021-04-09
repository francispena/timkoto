using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Infrastructure.Interfaces;
using Timkoto.UsersApi.Models;
using Timkoto.UsersApi.Services.Interfaces;

namespace Timkoto.UsersApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        private readonly ILogger _logger;

        private readonly string _className = "TransactionController";

        private readonly IVerifier _verifier;

        public TransactionController(ITransactionService transactionService, ILogger logger, IVerifier verifier)
        {
            _transactionService = transactionService;
            _logger = logger;
            _verifier = verifier;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddTransactionRequest request)
        {
            var member = $"{_className}.Post";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - {JsonConvert.SerializeObject(request)}");
            GenericResponse result;

            try
            {
                var idToken = Request.Headers["x-Api-kEy"];
                messages.AddWithTimeStamp(idToken);
                var tokenEmail = _verifier.GetEmail(idToken);
                if (!string.Equals(tokenEmail, request.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    messages.AddWithTimeStamp($"Transaction failed, email did not match - {tokenEmail} and {request.Email}");

                    var genericResponse =
                        GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AddTransactionFailed);

                    return StatusCode(403, genericResponse);
                }

                result = await _transactionService.AddTransaction(request, true, messages);
                messages.AddWithTimeStamp($"_transactionService.AddTransaction - {JsonConvert.SerializeObject(result)}");

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("balance/{userId}")]
        [HttpGet]
        public async Task<IActionResult> Balance([FromRoute] long userId)
        {
            var member = $"{_className}.Balance";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - userId:{userId}");
            GenericResponse result;

            try
            {
                result = await _transactionService.Balance(userId, messages);
                messages.AddWithTimeStamp($"_transactionService. - {JsonConvert.SerializeObject(result)}");

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }

        [Route("history/{userId}")]
        [HttpGet]
        public async Task<IActionResult> History([FromRoute] long userId)
        {
            var member = $"{_className}.History";
            var messages = new List<string>();
            var logType = LogType.Information;
            messages.AddWithTimeStamp($"{member} request - userId:{userId}");
            GenericResponse result;

            try
            {
                result = await _transactionService.History(userId, messages);
                messages.AddWithTimeStamp($"_transactionService.History - {JsonConvert.SerializeObject(result)}");

                return result.ResponseCode == HttpStatusCode.OK ? Ok(result) : StatusCode(403, result);
            }
            catch (Exception ex)
            {
                logType = LogType.Error;
                messages.AddWithTimeStamp($"{member} exception - {JsonConvert.SerializeObject(ex)}");

                result = GenericResponse.CreateErrorResponse(ex);
                return StatusCode(500, result);
            }
            finally
            {
                _logger.Log(member, messages, logType);
            }
        }
    }
}
