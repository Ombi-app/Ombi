using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ombi
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var loggerFact = context.RequestServices.GetService<ILoggerFactory>();
            var logger = loggerFact.CreateLogger<ErrorHandlingMiddleware>();
            logger.LogError(exception, "Something bad happened, ErrorMiddleware caught this");
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            //if (exception is NotFoundException) code = HttpStatusCode.NotFound;
            if (exception is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;
            if (exception is OperationCanceledException) code = HttpStatusCode.NoContent;
            string result;
            if (exception.InnerException != null)
            {
                result = JsonConvert.SerializeObject(new { error = exception.InnerException.Message });
            }
            else
            {
                result = JsonConvert.SerializeObject(new { error = exception.Message });
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}