using DataChain.Abstractions.Interfaces;

namespace DataChain.Infrastructure
{
    public interface IUnitOfWork
    {

        ITransactionSubscriber Transactions { get; }
        IAccountSubscriber Accounts { get; }
        IBlockSubscriber Blocks { get; }
        IRecordSubscriber Records { get; }
        ITransactionValidator TransactionValidator { get; }
    }
}
