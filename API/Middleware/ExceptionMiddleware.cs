using System.Text.Json;
using BackendJX3D.Core.Base;

namespace BackendJX3D.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionMiddleware(RequestDelegate middleware)
        {
            next = middleware;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BaseException.ErrorException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                string response = JsonSerializer.Serialize(new
                {
                    success = false,
                    statusCode  = ex.StatusCode,
                    error = ex.ErrorDetail
                });

                await context.Response.WriteAsync(response);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                string response = JsonSerializer.Serialize(new
                {
                    success = false,
                    statusCode = ex.HResult,
                    error = new
                    {
                        errorCode = "internal_server_error",
                        errorMessage = "Đã xảy ra lỗi không xác định.",
                        detail = ex.Message
                    }
                });

                await context.Response.WriteAsync(response);
            }
        }
    }
}
