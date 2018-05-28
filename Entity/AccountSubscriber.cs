using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;

namespace DataChain.DataProvider
{
    public class AccountSubscriber : IAccountSubscriber
    {
        private DatachainContext db = new DatachainContext();

        public Account GetAccount(string login)
        {
            AccountModel model = null;
            try
            {
                model = db.Accounts.Where(d => d.Login == login).Single();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            
             return Serializer.DeserializeAccount(model);
            
        }
        public Account GetAccount(HexString token)  
        {
            AccountModel model = null;
            try
            {
                model = db.Accounts.Where(d => d.Key == token.ToByteArray()).Single();
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return Serializer.DeserializeAccount(model);

        }

        public IEnumerable<Account> GetAllAccounts()
        {
            var list = new List<Account>();
            foreach(var account in db.Accounts.ToList())
            {
                list.Add(Serializer.DeserializeAccount(account));
            }
            return list;
        }

        public void AddUser(Account user)
        {
            throw new NotImplementedException();
        }

       
    }
}
