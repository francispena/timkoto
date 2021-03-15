namespace Timkoto.UsersApi.Enumerations
{
    public enum AddNewUserResult
    {
        NewUserCreated,

        EmailAddressExists,

        InvalidRegistrationCode,

        AccountCreationError
    }
}
