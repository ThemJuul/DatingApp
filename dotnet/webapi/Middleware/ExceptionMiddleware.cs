using System.Net;
using System.Text.Json;
using WebApi.Errors;

namespace WebApi.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, ex.Message);

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			var respnse = environment.IsDevelopment()
				? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace)
				: new ApiException(context.Response.StatusCode, ex.Message, "Internal server error.");

			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			var json = JsonSerializer.Serialize(respnse, options);

			await context.Response.WriteAsync(json);
		}
    }
}
