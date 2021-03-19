using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Timkoto.Data.Enumerations
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        
        [EnumMember(Value = "WalletDebit")]
        WalletDebit = 1,

        [EnumMember(Value = "WalletCredit")]
        WalletCredit = 2,

        [EnumMember(Value = "CommissionDebit")]
        CommissionDebit = 3,

        [EnumMember(Value = "CommissionCredit")]
        CommissionCredit = 4,

        [EnumMember(Value = "SalaryDebit")]
        SalaryDebit = 5,

        [EnumMember(Value = "SalaryCredit")]
        SalaryCredit = 6
    }
}
