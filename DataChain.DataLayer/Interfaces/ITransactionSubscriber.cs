using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer.Interfaces
{
    public interface ITransactionSubscriber
    {
        Task<byte[]> AddTransactionAsync(IEnumerable<Transaction> transaction);
        Task<Transaction> GetLastTransactionAsync();
        Task<Transaction> GetTransactionAsync(uint id);
       

    }
}
