using DataChain.DataLayer;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DataChain.Infrastructures
{
    public interface ITransactionValidator
    {
        Task<Transaction> ValidateTransaction(object records, string key);
    }
}
