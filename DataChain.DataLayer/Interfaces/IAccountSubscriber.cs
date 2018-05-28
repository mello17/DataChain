using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
