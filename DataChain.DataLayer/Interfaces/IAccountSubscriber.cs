using System;
using System.Collections.Generic;

namespace DataChain.Abstractions.Interfaces
{
    public interface IAccountSubscriber
    {
        Account GetAccount(string login);
        Account GetAccount(HexString token);
        IEnumerable<Account> GetAllAccounts();
        void AddUser(Account user);



    }
}
