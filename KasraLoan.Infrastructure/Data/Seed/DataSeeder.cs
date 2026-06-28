using KasraLoan.Domain.Entities;
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
            if (!await context.LoanTypes.AnyAsync())
            {
                context.LoanTypes.AddRange(
                    new LoanType
                    {
                        Name = "Travel Loan",
                        IsActive = true
                    },
                    new LoanType
                    {
                        Name = "Qarzolhasaneh Loan",
                        IsActive = false
                    },
                    new LoanType
                    {
                        Name = "Special Case Loan",
                        IsActive = true
                    },
                    new LoanType
                    {
                        Name = "Marriage Loan",
                        IsActive = true
                    },
                    new LoanType
                    {
                        Name = "Immediate Payment Loan",
                        IsActive = true
                    },
                    new LoanType
                    {
                        Name = "Bank Introduction Loan",
                        IsActive = false
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}