using DataChain.Abstractions.Interfaces;
using DataChain.Infrastructure;
using DataChain.DataProvider;


namespace DataChain.WebApplication.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private TransactionSubscriber txSubscribe;
        private RecordSubscriber recSubscribe;
        private BlockSubscriber blcSubscribe;
        private TransactionValidator validator;
        private AccountSubscriber account;
       

        public ITransactionSubscriber Transactions
        {
            get
            {
                if (txSubscribe == null)
                    txSubscribe = new TransactionSubscriber();
                return (ITransactionSubscriber)txSubscribe;
            }
        }

        public ITransactionValidator TransactionValidator
        {
            get
            {
                if (validator == null)
                {
                    validator = new TransactionValidator((TransactionSubscriber)this.Transactions);
                }
                return validator;
            }
        }


        public IRecordSubscriber Records
        { 
             get
             {
                if (recSubscribe == null)
                    recSubscribe = new RecordSubscriber();
                return (IRecordSubscriber) txSubscribe;
             }
        }

        public IBlockSubscriber Blocks
        {
            get
            {
                if (blcSubscribe == null)
                    blcSubscribe = new BlockSubscriber();
                return (IBlockSubscriber)blcSubscribe;
            }
        }

        public IAccountSubscriber Accounts
        {
            get
            {
                if (account == null)
                    account = new AccountSubscriber();
                return (IAccountSubscriber)account;
            }
        }



    }
}