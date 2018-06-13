using System;
using System.Collections.Generic;
using DataChain.Abstractions;

namespace DataChain.Abstractions.Interfaces
{
    public interface IAccountRepository
    {
        Account GetAccount(string login);
        Account GetAccount(HexString token);
        IEnumerable<Account> GetAllAccounts();
        void AddUser(Account user);



    }
}
