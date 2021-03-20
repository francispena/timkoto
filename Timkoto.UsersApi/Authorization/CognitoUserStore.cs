using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization
{
    public class CognitoUserStore : ICognitoUserStore
    {
        private readonly AmazonCognitoIdentityProviderClient _providerClient = new AmazonCognitoIdentityProviderClient(RegionEndpoint.APSoutheast1);

        private readonly IPersistService _persistService;
        private readonly string _userPoolId;
        private readonly string _clientId;

        public CognitoUserStore(IPersistService persistService)
        {
            var configuration = Startup.Configuration;

            _clientId = configuration["CognitoClientId"];
            _userPoolId = configuration["CognitoUserPoolId"];
            _persistService = persistService;
        }

        public async Task<Results> CreateAsync(string email, string password, List<string> messages)
        {
            var lambdaContext = Startup.LambdaContext;

            lambdaContext.Logger.Log("CreateAsync");

            var retVal = Results.Unknown;
            try
            {
                var signUpRequest = new SignUpRequest
                {
                    ClientId = _clientId,
                    Password = password,
                    Username = email,
                    UserAttributes = new List<AttributeType>
                        {new AttributeType {Name = "email", Value = email}}
                };

                messages.Add("start signUpResult");
                var signUpResult = await _providerClient.SignUpAsync(signUpRequest);
                messages.Add($"signUpResult - {JsonConvert.SerializeObject(signUpResult)}");

                if (signUpResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    retVal = Results.AccountCreationInCognitoFailed;
                    return retVal;
                }

                var adminConfirmSignUpRequest = new AdminConfirmSignUpRequest
                {
                    Username = email,
                    UserPoolId = _userPoolId,
                };

                var adminConfirmSignUpResult = await _providerClient.AdminConfirmSignUpAsync(adminConfirmSignUpRequest);
                messages.Add($"adminConfirmSignUpResult - {JsonConvert.SerializeObject(adminConfirmSignUpResult)}");
                if (adminConfirmSignUpResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    await _providerClient.AdminDeleteUserAsync(new AdminDeleteUserRequest
                    {
                        UserPoolId = _userPoolId,
                        Username = email
                    });

                    retVal = Results.AccountConfirmationInCognitoFailed;
                    return retVal;
                }

                retVal = Results.AccountConfirmedInCognito;
            }
            catch (Exception ex)
            {
                retVal = Results.AccountCreationInCognitoError;
                messages.Add($"CreateAsync error message - {ex.Message}");
            }
            finally
            {
                messages.Add($"retVal - {retVal}");
                lambdaContext?.Logger.Log(string.Join("\r\n", messages));
            }

            return retVal;
        }

        public async Task<GenericResponse> AuthenticateAsync(string email, string password, List<string> messages)
        {
            GenericResponse genericResponse;

            try
            {
                var userPool = new CognitoUserPool(_userPoolId, _clientId, _providerClient);
                var user = new CognitoUser(email, _clientId, userPool, _providerClient);

                var authRequest = new InitiateSrpAuthRequest
                {
                    Password = password
                };

                var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(authResponse?.AuthenticationResult?.IdToken))
                {
                    var userDb = await _persistService.FindOne<User>(_ => _.Email == email);
                    if (userDb == null)
                    {
                        genericResponse = GenericResponse.Create(false, HttpStatusCode.Forbidden,
                            Results.AuthenticationError);
                    }
                    else
                    {
                        genericResponse =
                            GenericResponse.Create(true, HttpStatusCode.OK, Results.AuthenticationSucceeded);
                        genericResponse.Data = new
                        {
                            authResponse.AuthenticationResult?.IdToken,
                            User = userDb
                        };
                    }
                }
                else
                {
                    genericResponse =
                        GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AuthenticationFailed);
                }
            }
            catch (NotAuthorizedException)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AuthenticationFailed);
            }
            catch (Exception ex)
            {
                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AuthenticationError);
                genericResponse.ExceptionMessage = ex.Message;
                genericResponse.ExceptionStackTrace = ex.StackTrace;
            }

            return genericResponse;
        }

        public async Task<GenericResponse> ChangePasswordAsync(string userName, string password, List<string> messages)
        {
            var setPasswordResult = await _providerClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = userName,
                Password = password,
                Permanent = true
            });

            var genericResponse = setPasswordResult.HttpStatusCode == HttpStatusCode.OK
                ? GenericResponse.Create(true, HttpStatusCode.OK, Results.ChangePasswordSucceeded)
                : GenericResponse.Create(true, HttpStatusCode.Forbidden, Results.ChangePasswordFailed);

            return genericResponse;
        }
    }
}


//var adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
//{
//    Username = userName,
//    UserPoolId = _userPoolId,
//    UserAttributes = new List<AttributeType> { new AttributeType { Name = "email_verified", Value = "true" } }
//};

//await _client.AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest);
