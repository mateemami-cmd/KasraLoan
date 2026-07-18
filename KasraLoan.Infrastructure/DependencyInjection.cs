using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.LoanRules.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Infrastructure.Services;

namespace KasraLoan.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<ILoanTypeRepository, LoanTypeRepository>();

            services.AddScoped<IEmployeeScoreRepository, EmployeeScoreRepository>();

            services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            services.AddScoped<IEmployeeService, EmployeeService>();

            services.AddScoped<IEmployeeScoreService, EmployeeScoreService>();

            services.AddScoped<ILoanEligibilityService, LoanEligibilityService>();

            services.AddScoped<ILoanCalculationService, LoanCalculationService>();

            services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();

            services.AddScoped<ILoanRequestService, LoanRequestService>();

            services.AddScoped<ILoanRuleEngine, LoanRuleEngine>();

            services.AddScoped<ILoanRule, TravelLoanRule>();

            services.AddScoped<ILoanRule, MarriageLoanRule>();

            services.AddScoped<ILoanRule, SpecialCaseLoanRule>();

            services.AddScoped<ILoanRule, ImmediatePaymentLoanRule>();

            services.AddScoped<ILoanRule, QarzolhasanehLoanRule>();

            services.AddScoped<ILoanRule, BankIntroductionLoanRule>();

            //services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<ILoanInstallmentService, LoanInstallmentService>();

            services.AddScoped<IAuthRepository, AuthRepository>();

            services.AddScoped<IJwtService, JwtService>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
    }
}