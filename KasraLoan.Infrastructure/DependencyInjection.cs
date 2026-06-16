using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeScoreService, EmployeeScoreService>();
            services.AddScoped<ILoanEligibilityService, LoanEligibilityService>();
            services.AddScoped<ILoanCalculationService, LoanCalculationService>();
            services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
            services.AddScoped<ILoanRequestService, LoanRequestService>();

            return services;
        }
    }
}