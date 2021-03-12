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
        
        [EnumMember(Value = "Wallet-Debit")]
        WalletDebit = 1,

        [EnumMember(Value = "Wallet-Credit")]
        WalletCredit = 2,

        [EnumMember(Value = "Commission-Debit")]
        CommissionDebit = 3,

        [EnumMember(Value = "Commission-Credit")]
        CommissionCredit = 4,

        [EnumMember(Value = "Salary-Debit")]
        SalaryDebit = 5,

        [EnumMember(Value = "Salary-Credit")]
        SalaryCredit = 6
    }
}
