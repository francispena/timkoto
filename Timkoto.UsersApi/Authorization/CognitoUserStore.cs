using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Models;

namespace Timkoto.UsersApi.Authorization
{
    public class CognitoUserStore : ICognitoUserStore
    {
        private readonly AmazonCognitoIdentityProviderClient _providerClient = new AmazonCognitoIdentityProviderClient(RegionEndpoint.APSoutheast1);

        private readonly string _userPoolId;
        private readonly string _clientId;

        public CognitoUserStore()
        {
            var configuration = Startup.Configuration;

            _clientId = configuration["CognitoClientId"];
            _userPoolId = configuration["CognitoUserPoolId"];
        }

        public async Task<Results> CreateAsync(string userName, string password, List<string> messages)
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
                    Username = userName,
                    UserAttributes = new List<AttributeType>
                        {new AttributeType {Name = "email", Value = userName}}
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
                    Username = userName,
                    UserPoolId = _userPoolId,
                };

                var adminConfirmSignUpResult = await _providerClient.AdminConfirmSignUpAsync(adminConfirmSignUpRequest);
                messages.Add($"adminConfirmSignUpResult - {JsonConvert.SerializeObject(adminConfirmSignUpResult)}");
                if (adminConfirmSignUpResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    await _providerClient.AdminDeleteUserAsync(new AdminDeleteUserRequest
                    {
                        UserPoolId = _userPoolId,
                        Username = userName
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

        public async Task<GenericResponse> AuthenticateAsync(string userName, string password, List<string> messages)
        {
            var userPool = new CognitoUserPool(_userPoolId, _clientId, _providerClient);
            var user = new CognitoUser(userName, _clientId, userPool, _providerClient);

            var authRequest = new InitiateSrpAuthRequest
            {
                Password = password
            };

            var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

            GenericResponse genericResponse;

            if (!string.IsNullOrWhiteSpace(authResponse?.AuthenticationResult?.IdToken))
            {
                genericResponse = GenericResponse.Create(true, HttpStatusCode.OK, Results.AuthenticationSucceeded);
                genericResponse.Data = new
                {
                    IdToken = authResponse?.AuthenticationResult?.IdToken
                };
            }
            else
            {
                genericResponse= GenericResponse.Create(true, HttpStatusCode.Forbidden, Results.AuthenticationFailed);
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
