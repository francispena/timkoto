﻿namespace Timkoto.UsersApi.Enumerations
{
    public enum Results
    {
        Unknown,
        NewUserCreated,
        EmailAddressExists,
        InvalidRegistrationCode,
        AccountCreationError,
        AccountCreatedInCognito,
        AccountCreationInCognitoFailed,
        AccountConfirmedInCognito,
        AccountConfirmationInCognitoFailed,
        AccountCreationInCognitoError,
        TransactionAdded,
        AddTransactionFailed,
        CodeCreated,
        InvalidUserId,
        AgentsFound,
        NoAgentFound,
        PlayerFound,
        PlayersFound,
        NoPlayerFound,
        AuthenticationFailed,
        AuthenticationSucceeded,
        ChangePasswordSucceeded,
        ChangePasswordFailed,
        GameNotFound,
        NoContestTeamFound,
        ContestTeamFound,
        NoLineUpToUpdate,
        NotEnoughPoints,
        NoContestFound,
        ProcessingTransactionFailed,
        PlayerLineUpCreated,
        InvalidLineUpCount,
        TeamNameMissing,
        InvalidOperatorId,
        InvalidAgentId,
        InvalidContestId,
        AuthenticationError,
        TeamSubmissionNotAccepted,
        TransactionFound,
        AmountNotAccepted,
        NoTeamFound,
        TeamsFound,
        NoTeamPlayersFound,
        TeamPlayersFound,
        NoAgentPointsFound,
        AgentPointsFound,
        PrizePoolNotSet,
        PrizePoolFound,
        EmailSent,
        EmailSendingFailed,
        PlayerNotFound,
        InvalidResetCode,
        UserNameExists,
        NoTransactionFound,
        TimeZoneLookUpError,
        ContestPackageNotAssigned,
        NoContestPackage,
        ExceededSalary,
        UserNameAvailable,
        Unauthorized,
        NoTokenFound,
        UserInfoUpdated,
        UserInfoUpdateFailed,
        InvalidNumberOfPlayersInPosition,
        InvalidActivationCode,
        AccountAlreadyActivated,
        AccountActivationFailed,
        AccountActivationSucceeded,
        AccountNotYetActivated,
        NewUserCreatedActivationRequired,
        AccountIsNotYetActivated
    }
}
