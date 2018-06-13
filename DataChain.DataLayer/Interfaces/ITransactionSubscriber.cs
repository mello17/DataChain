using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Abstractions.Interfaces
{
    public interface ITransactionRepository
    {
        Task<byte[]> AddTransactionAsync(IEnumerable<Transaction> transaction);
        List<Transaction> GetLastTransactionAsync();
        Task<Transaction> GetTransactionAsync(int id);
        Task<Transaction> GetTransactionAsync(byte[] hash);
        List<Transaction> GetTransactionsByBlockId(int id);
        void Update(List<Transaction> raw_tx, int index);

    }
}
