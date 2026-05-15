using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        HttpStatusCode status;

        switch (ex)
        {
            case KeyNotFoundException:
                status = HttpStatusCode.NotFound;
                break;

            case FileNotFoundException:
                status = HttpStatusCode.NotFound;
                break;

            case UnauthorizedAccessException:
                status = HttpStatusCode.Unauthorized;
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                break;
        }

        var response = new
        {
            success = false,
            message = ex.Message
        };

        var json = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(json);
    }
}