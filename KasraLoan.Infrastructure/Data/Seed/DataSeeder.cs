using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using KasraLoan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(KasraLoanDbContext context)
        {
            var loanTypes = new List<LoanType>
        {
        new LoanType
        {
            Name = "Travel Loan",
            Type = LoanTypeEnum.TravelLoan,
            IsActive = true
        },
        new LoanType
        {
            Name = "Qarzolhasaneh Loan",
            Type = LoanTypeEnum.QarzolhasanehLoan,
            IsActive = false
        },
        new LoanType
        {
            Name = "Special Case Loan",
            Type = LoanTypeEnum.SpecialCaseLoan,
            IsActive = true
        },
        new LoanType
        {
            Name = "Marriage Loan",
            Type = LoanTypeEnum.MarriageLoan,
            IsActive = true
        },
        new LoanType
        {
            Name = "Immediate Payment Loan",
            Type = LoanTypeEnum.ImmediatePaymentLoan,
            IsActive = true
        },
        new LoanType
        {
            Name = "Bank Introduction Loan",
            Type = LoanTypeEnum.BankIntroductionLoan,
            IsActive = false
        }
    };

            foreach (var loanType in loanTypes)
            {
                var existingLoanType = await context.LoanTypes
                    .FirstOrDefaultAsync(x => x.Type == loanType.Type);

                if (existingLoanType == null)
                {
                    context.LoanTypes.Add(loanType);
                }
                else
                {
                    existingLoanType.Name = loanType.Name;
                    existingLoanType.IsActive = loanType.IsActive;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}