using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Services
{
    public interface ILoanRuleEngine
    {
        LoanRuleEngine IsEligible(LoanRuleEngine context);
    }
}