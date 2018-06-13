using DataChain.Abstractions.Interfaces;
using DataChain.Infrastructure;
using DataChain.DataProvider;


namespace DataChain.WebApplication.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionRepository txSubscribe;
        private RecordRepository recSubscribe;
        private BlockRepository blcSubscribe;
        private TransactionValidator validator;
        private AccountRepository account;
       

        public ITransactionRepository Transactions
        {
            get
            {
                if (txSubscribe == null)
                    txSubscribe = new TransactionRepository();
                return (ITransactionRepository)txSubscribe;
            }
        }

        public ITransactionValidator TransactionValidator
        {
            get
            {
                if (validator == null)
                {
                    validator = new TransactionValidator();
                }
                return validator;
            }
        }


        public IRecordRepository Records
        { 
             get
             {
                if (recSubscribe == null)
                    recSubscribe = new RecordRepository();
                return (IRecordRepository) txSubscribe;
             }
        }

        public IBlockRepository Blocks
        {
            get
            {
                if (blcSubscribe == null)
                    blcSubscribe = new BlockRepository();
                return (IBlockRepository)blcSubscribe;
            }
        }

        public IAccountRepository Accounts
        {
            get
            {
                if (account == null)
                    account = new AccountRepository();
                return (IAccountRepository)account;
            }
        }



    }
}