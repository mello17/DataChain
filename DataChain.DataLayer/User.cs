using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;

namespace DataChain.DataLayer
{
    public class Account
    {
        
        public string Login { get;  set; }
        public HexString Password { get;  set; }
        public HexString Key { get; set; }
        public UserRole Role { get; set; }
        public IReadOnlyList<Block> CreatedBlocks { get; set; }

        public Account GetUser(string login)
        {
            return new Account();
        }

        public IEnumerable<Account> GetUsers()
        {
            return new List<Account>();
        }

        public void AddUser(Account user)
        {
            new Account()
            {
                Login = user.Login,
                Password = user.Password,
                CreatedBlocks = new List<Block>(),
            };
        }
    }
}
