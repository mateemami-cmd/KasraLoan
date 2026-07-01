using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Infrastructure.Data;
using KasraLoan.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using KasraLoan.Infrastructure;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.LoanRules.Implementations;
using KasraLoan.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KasraLoan.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddInfrastructure();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter JWT like: Bearer {your token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                   {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
                   }
                });
            });

            //builder.Services.AddScoped<LoanRuleEngine>();
            //builder.Services.AddScoped<ILoanRule, TravelLoanRule>();
            //builder.Services.AddScoped<ILoanRule, MarriageLoanRule>();
            //builder.Services.AddScoped<ILoanRule, SpecialCaseLoanRule>();
            //builder.Services.AddScoped<ILoanRule, ImmediatePaymentLoanRule>();
            //builder.Services.AddScoped<ILoanRule, QarzolhasanehLoanRule>();
            //builder.Services.AddScoped<ILoanRule, BankIntroductionLoanRule>();
            //builder.Services.AddScoped<ILoanTypeRepository, LoanTypeRepository>();
            //builder.Services.AddScoped<IEmployeeScoreRepository, EmployeeScoreRepository>();
            //builder.Services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = builder.Configuration.GetSection("JwtSettings");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(jwt["Key"]))
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddDbContext<KasraLoanDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<KasraLoanDbContext>();

                await DataSeeder.SeedAsync(context);
            }

            app.Run();
        }
    }
}