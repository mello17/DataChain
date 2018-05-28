using DataChain.Abstractions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DataChain.Infrastructure
{
    public interface ITransactionValidator
    {
        Task<Transaction> ValidateTransaction(object records, string key);
    }
}
