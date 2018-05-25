using DataChain.DataLayer.Interfaces;

namespace DataChain.Infrastructures
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
