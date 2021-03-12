using Timkoto.Data.Enumerations;

namespace Timkoto.UsersApi.Models
{
    public class AddTransactionRequest
    {
        public virtual long OperatorId { get; set; }

        public virtual long UserId { get; set; }

        public virtual UserType UserType { get; set; }

        public virtual TransactionType TransactionType { get; set; }

        public virtual decimal Amount { get; set; }
    }
}
