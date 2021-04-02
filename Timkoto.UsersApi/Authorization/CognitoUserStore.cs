using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Timkoto.Data.Repositories;
using Timkoto.Data.Services.Interfaces;
using Timkoto.UsersApi.Authorization.Interfaces;
using Timkoto.UsersApi.Enumerations;
using Timkoto.UsersApi.Extensions;
using Timkoto.UsersApi.Models;
using ChangePasswordRequest = Timkoto.UsersApi.Models.ChangePasswordRequest;

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
            messages.AddWithTimeStamp("CognitoUserStore.AuthenticateAsync");
            GenericResponse genericResponse;

            try
            {
                var userDb = await _persistService.FindOne<User>(_ => _.Email == email);
                if (userDb == null)
                {
                    return GenericResponse.Create(false, HttpStatusCode.Forbidden,
                        Results.AuthenticationError);
                }
                
                var userPool = new CognitoUserPool(_userPoolId, _clientId, _providerClient);
                var user = new CognitoUser(email, _clientId, userPool, _providerClient);

                var authRequest = new InitiateSrpAuthRequest
                {
                    Password = password
                };

                var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

                messages.AddWithTimeStamp($"authResponse - {JsonConvert.SerializeObject(authResponse.AuthenticationResult)}" );

                if (!string.IsNullOrWhiteSpace(authResponse?.AuthenticationResult?.IdToken))
                {
                    genericResponse =
                        GenericResponse.Create(true, HttpStatusCode.OK, Results.AuthenticationSucceeded);

                    //genericResponse.Jwt = new JWToken
                    //{
                    //    AccessToken = authResponse.AuthenticationResult?.AccessToken,
                    //    IdToken = authResponse.AuthenticationResult?.IdToken,
                    //    RefreshToken = authResponse.AuthenticationResult?.RefreshToken
                    //};

                    genericResponse.Data = new
                    {
                        authResponse.AuthenticationResult?.IdToken,
                        User = userDb
                    };
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
                messages.AddWithTimeStamp($"CognitoUserStore.AuthenticateAsync exception - {JsonConvert.SerializeObject(ex)}");

                genericResponse =
                    GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.AuthenticationError);
                genericResponse.ExceptionMessage = ex.Message;
                genericResponse.ExceptionStackTrace = ex.StackTrace;
            }

            return genericResponse;
        }

        public async Task<GenericResponse> ChangePasswordAsync(ChangePasswordRequest request, List<string> messages)
        {
            var user = await _persistService.FindOne<User>(_ => _.IsActive && _.Email == request.Email && _.PasswordResetCode == request.Code);
            if (user == null)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidResetCode);
            }

            if (DateTime.UtcNow.Subtract(user.UpdateDateTime).TotalMinutes > 600)
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.InvalidResetCode);
            }

            try
            {
                var setPasswordResult = await _providerClient.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
                {
                    UserPoolId = _userPoolId,
                    Username = request.Email,
                    Password = request.Password,
                    Permanent = true
                });

                return setPasswordResult.HttpStatusCode == HttpStatusCode.OK
                    ? GenericResponse.Create(true, HttpStatusCode.OK, Results.ChangePasswordSucceeded)
                    : GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.ChangePasswordFailed);
            }
            catch
            {
                return GenericResponse.Create(false, HttpStatusCode.Forbidden, Results.ChangePasswordFailed);
            }
        }

        public async Task<GenericResponse> RefreshToken(string jwToken, List<string> messages)
        {
            GenericResponse genericResponse = null;

            try
            {
                var jwt = JsonConvert.DeserializeObject<JWToken>(jwToken);

                if (jwt == null || string.IsNullOrWhiteSpace(jwt.IdToken) || string.IsNullOrWhiteSpace(jwt.AccessToken) || string.IsNullOrWhiteSpace(jwt.RefreshToken))
                {
                    throw new NotAuthorizedException("");
                }

                var jwts = jwt.IdToken.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (jwts.Length != 3)
                {
                    throw new NotAuthorizedException("");
                }

                var idTokenPayloadStringBase64String = Convert.FromBase64String($"{jwts[1]}=");
                var decodedString = Encoding.UTF8.GetString(idTokenPayloadStringBase64String);

                var idTokenPayload = JsonConvert.DeserializeObject<IdTokenPayload>(decodedString);
                if (idTokenPayload == null)
                {
                    throw new NotAuthorizedException("");
                }

                var userDb = await _persistService.FindOne<User>(_ => _.Email == idTokenPayload.CognitoUsername);
                if (userDb == null)
                {
                    return GenericResponse.Create(false, HttpStatusCode.Forbidden,
                        Results.AuthenticationError);
                }

                var userPool = new CognitoUserPool(_userPoolId, _clientId, _providerClient);
                var user = new CognitoUser(idTokenPayload.CognitoUsername, _clientId, userPool, _providerClient);

                var refreshRequest = new InitiateRefreshTokenAuthRequest()
                {
                    AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
                };

                var issueTime = DateTimeOffset.FromUnixTimeSeconds(idTokenPayload.iat).DateTime;
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(idTokenPayload.exp).DateTime;

                user.SessionTokens =
                    new CognitoUserSession(jwt.IdToken, jwt.AccessToken, jwt.RefreshToken, issueTime, expirationTime);

                var refreshResponse = await user.StartWithRefreshTokenAuthAsync(refreshRequest);

                if (!string.IsNullOrWhiteSpace(refreshResponse?.AuthenticationResult?.IdToken))
                {
                    genericResponse =
                        GenericResponse.Create(true, HttpStatusCode.OK, Results.AuthenticationSucceeded);

                    genericResponse.Jwt = new JWToken
                    {
                        AccessToken = refreshResponse.AuthenticationResult?.AccessToken,
                        IdToken = refreshResponse.AuthenticationResult?.IdToken,
                        RefreshToken = refreshResponse.AuthenticationResult?.RefreshToken
                    };

                    genericResponse.Data = new
                    {
                        refreshResponse.AuthenticationResult?.IdToken,
                        User = userDb
                    };
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
    }
}


//var adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
//{
//    Username = userName,
//    UserPoolId = _userPoolId,
//    UserAttributes = new List<AttributeType> { new AttributeType { Name = "email_verified", Value = "true" } }
//};

//await _client.AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest);
