using System.Net;
using FluentValidation;
using System.Text.Json;
using KasraLoan.Application.Common.Exceptions;

namespace KasraLoan.API.Middlewares
{
    public class ExceptionMiddleware
    {
        // مهم: بدون این تنظیم، JsonSerializer نام‌ها را PascalCase می‌نویسد
        // ("Message")، در حالی که بقیه‌ی پاسخ‌های API از MVC می‌آیند و camelCase
        // هستند. نتیجه‌اش این بود که فرانت که دنبال `message` می‌گشت هیچ پیامی
        // پیدا نمی‌کرد و همیشه متن عمومیِ «خطایی رخ داد» را نشان می‌داد —
        // یعنی همه‌ی پیام‌های دقیقِ فارسیِ دامنه بی‌اثر بودند.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ForbiddenAccessException ex)
            {
                await WriteAsync(context, HttpStatusCode.Forbidden, new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                await WriteAsync(context, HttpStatusCode.BadRequest, new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                await WriteAsync(context, HttpStatusCode.BadRequest, new
                {
                    // اولین خطای اعتبارسنجی به‌عنوان پیام اصلی می‌آید تا فرانت
                    // چیزی مفیدتر از «Validation failed» برای نمایش داشته باشد.
                    message = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "اطلاعات ارسالی معتبر نیست.",
                    errors = ex.Errors.Select(x => new
                    {
                        propertyName = x.PropertyName,
                        errorMessage = x.ErrorMessage
                    })
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteAsync(context, HttpStatusCode.Unauthorized, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                await WriteAsync(context, HttpStatusCode.NotFound, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // خطای پیش‌بینی‌نشده باید لاگ شود؛ قبلاً بی‌سروصدا بلعیده می‌شد و
                // ۵۰۰ها بدون هیچ ردی در لاگ ناپدید می‌شدند.
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await WriteAsync(context, HttpStatusCode.InternalServerError,
                    new { message = "خطای غیرمنتظره‌ای رخ داد." });
            }
        }

        private static async Task WriteAsync(HttpContext context, HttpStatusCode status, object body)
        {
            // اگر پاسخ شروع شده باشد، دیگر نمی‌شود هدر یا بدنه نوشت.
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json; charset=utf-8";

            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
        }
    }
}
