using DataChain.Abstractions.Interfaces;

namespace DataChain.Infrastructure
{
    public interface IUnitOfWork
    {

        ITransactionRepository Transactions { get; }
        IAccountRepository Accounts { get; }
        IBlockRepository Blocks { get; }
        IRecordRepository Records { get; }
        ITransactionValidator TransactionValidator { get; }
    }
}
