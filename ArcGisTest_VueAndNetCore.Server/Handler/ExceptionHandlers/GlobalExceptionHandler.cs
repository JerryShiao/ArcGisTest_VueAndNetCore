using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArcGisTest_VueAndNetCore.Server.Handler.ExceptionHandlers
{
    /// <summary>
    /// 全局例外處理器
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        // 直接透過 DI 注入 Logger
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // 1. 在這裡寫 Log！系統不會掛掉，且能完整記錄 Exception 資訊
            _logger.LogError(exception, "系統發生未處理的異常：{Message}", exception.Message);

            // 2. 設定要回傳給前端的錯誤回應 (例如符合 RFC 9457 標準的 ProblemDetails)
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            // 這裡可以根據實際需求，決定要回傳哪些錯誤資訊給前端。
            var errorResponse = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "伺服器內部錯誤",
                Detail = "系統發生預期之外的錯誤，請稍後再試或聯絡系統管理員。"
            };

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            // 3. 回傳 true 代表「我已經處理好這個錯誤了」，
            // 這樣 ASP.NET Core 就知道不需要再往外拋出、更不會讓 Application 崩潰掛掉。
            return true;
        }

    }//class end
}//namespace end
