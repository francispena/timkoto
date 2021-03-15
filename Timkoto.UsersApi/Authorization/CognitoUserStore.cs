using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timkoto.UsersApi.Authorization.Interfaces;

namespace Timkoto.UsersApi.Authorization
{
    public class CognitoUserStore : ICognitoUserStore
    {
        private readonly AmazonCognitoIdentityProviderClient _client = new AmazonCognitoIdentityProviderClient();
        
        private readonly string _userPoolId;
        private readonly string _clientId;

        public CognitoUserStore()
        {
            var configuration = Startup.Configuration;

            _clientId = configuration["CognitoClientId"];
            _userPoolId = configuration["CognitoUserPoolId"];
        }

        public async Task<bool> CreateAsync(string userName, string password)
        {
            var lambdaContext = Startup.LambdaContext;

            var messages = new List<string>();
            

            var retVal = false;

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
                var signUpResult = await _client.SignUpAsync(signUpRequest);
                messages.Add($"signUpResult - {JsonConvert.SerializeObject(signUpResult)}");

                if (signUpResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                var adminConfirmSignUpRequest = new AdminConfirmSignUpRequest
                {
                    Username = userName,
                    UserPoolId = _userPoolId,
                };

                var adminConfirmSignUpResult = await _client.AdminConfirmSignUpAsync(adminConfirmSignUpRequest);
                messages.Add($"adminConfirmSignUpResult - {JsonConvert.SerializeObject(adminConfirmSignUpResult)}");
                if (adminConfirmSignUpResult.HttpStatusCode != HttpStatusCode.OK)
                {
                    retVal = false;
                }
            }
            catch (System.Exception ex)
            {
                retVal = false;
                messages.Add($"CreateAsync error message - {ex.Message}");
            }
            finally
            {
                lambdaContext.Logger.Log(string.Join("\r\n", messages));
            }

            //var adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
            //{
            //    Username = userName,
            //    UserPoolId = _userPoolId,
            //    UserAttributes = new List<AttributeType> { new AttributeType { Name = "email_verified", Value = "true" } }
            //};

            //await _client.AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest);

            return retVal;
        }

        public async Task<string> AuthenticateAsync(string userName, string password)
        {
            var provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials());
            var userPool = new CognitoUserPool(_userPoolId, _clientId, provider);
            var user = new CognitoUser(userName, _clientId, userPool, provider);
            
            var authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };

            var authResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

            return authResponse?.AuthenticationResult?.IdToken;
        }
    }
}
