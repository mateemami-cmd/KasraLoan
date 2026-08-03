using FluentValidation;
using KasraLoan.API.Authorization;
using KasraLoan.API.Middlewares;
using KasraLoan.Application.Behaviors;
using KasraLoan.Infrastructure;
using KasraLoan.Infrastructure.Data;
using KasraLoan.Infrastructure.Data.Seed;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;

namespace KasraLoan.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File(
                    "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                builder.Services.AddControllers();

                builder.Services.AddValidatorsFromAssembly(Assembly.Load("KasraLoan.Application"));

                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(Assembly.Load("KasraLoan.Application"));
                });

                builder.Services.AddTransient(
                    typeof(IPipelineBehavior<,>),
                    typeof(ValidationBehavior<,>));

                builder.Services.AddTransient(
                    typeof(IPipelineBehavior<,>),
                    typeof(LoggingBehavior<,>));

                builder.Services.AddInfrastructure();

                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition(
                        "Bearer",
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                            Description = "Enter JWT like: Bearer {your token}"
                        });

                    options.AddSecurityRequirement(
                        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                        {
                            {
                                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                                {
                                    Reference =
                                        new Microsoft.OpenApi.Models.OpenApiReference
                                        {
                                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                        }
                                },
                                Array.Empty<string>()
                            }
                        });
                });

                builder.Services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        var jwt = builder.Configuration.GetSection("JwtSettings");

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,

                                ValidIssuer = jwt["Issuer"],
                                ValidAudience = jwt["Audience"],
                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(jwt["Key"]!))
                            };
                    });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        LoanPolicies.AdminOnly,
                        policy => policy.RequireRole("Admin"));

                    options.AddPolicy(
                        LoanPolicies.EmployeeOnly,
                        policy => policy.RequireRole("Employee"));

                    options.AddPolicy(
                        LoanPolicies.AdminOrEmployee,
                        policy => policy.RequireRole("Admin", "Employee"));
                });

                builder.Services.AddDbContext<KasraLoanDbContext>(options =>
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                var app = builder.Build();

                app.UseSerilogRequestLogging();

                app.UseMiddleware<ExceptionMiddleware>();

                app.UseCors("AllowAll");

                // سرو کردن فایل‌های آپلودشده (مثل عکس پروفایل) از wwwroot.
                app.UseStaticFiles();

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
                    var context =
                        scope.ServiceProvider.GetRequiredService<KasraLoanDbContext>();

                    await DataSeeder.SeedAsync(context);
                }

                Log.Information("KasraLoan API Started Successfully");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}