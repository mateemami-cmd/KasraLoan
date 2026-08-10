using KasraLoan.Application.Common.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace KasraLoan.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var stopwatch = Stopwatch.StartNew();

        // درخواست‌های حساس (رمز عبور، اطلاعات کارت بانکی و ...) نباید payload‌شان
        // در لاگ نوشته شود؛ لاگ‌ها روی دیسک ذخیره می‌شوند و ممکن است جایی منتشر شوند.
        if (request is ISensitiveRequest)
        {
            _logger.LogInformation("START Request: {RequestName} | Payload: [redacted]", requestName);
        }
        else
        {
            _logger.LogInformation("START Request: {RequestName} | Payload: {@Request}", requestName, request);
        }

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation("END Request: {RequestName} completed in {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "ERROR Request: {RequestName} failed after {ElapsedMilliseconds} ms", requestName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}