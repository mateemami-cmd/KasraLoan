using KasraLoan.Domain.Entities;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanFundRepository
    {
        /// <summary>ردیفِ واحدِ صندوق را برمی‌گرداند (Id = 1).</summary>
        Task<LoanFund?> GetAsync();

        Task SaveChangesAsync();
    }
}
