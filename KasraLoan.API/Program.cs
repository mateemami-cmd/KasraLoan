using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Infrastructure.Data;
using KasraLoan.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using KasraLoan.Infrastructure;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.LoanRules.Implementations;

namespace KasraLoan.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddInfrastructure();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<LoanRuleEngine>();
            builder.Services.AddScoped<ILoanRule, TravelLoanRule>();
            builder.Services.AddScoped<ILoanRule, MarriageLoanRule>();
            builder.Services.AddScoped<ILoanRule, SpecialCaseLoanRule>();
            builder.Services.AddScoped<ILoanTypeRepository, LoanTypeRepository>();
            builder.Services.AddScoped<IEmployeeScoreRepository, EmployeeScoreRepository>();

            builder.Services.AddDbContext<KasraLoanDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}