using System.Net;
using System.Text.Json;
using Task_Management_Project.Exeptions;
using Task_Management_Project.Models;

namespace Task_Management_Project.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            ErrorResponse errorResponse = new ErrorResponse();

            switch (exception)
            {
                case NotFoundException nf:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse(response.StatusCode, nf.Message);
                    break;


                case BadRequestException br:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse(response.StatusCode, br.Message);
                    break;


                case ValidationException ve:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse(response.StatusCode, ve.Message, ve.Details);
                    break;

                case UnauthorizedException ua:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new ErrorResponse(response.StatusCode, ua.Message);
                    break;


                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    // Extract the innermost exception message for better debugging
                    string GetInnermostException(Exception ex)
                    {
                        while (ex.InnerException != null)
                            ex = ex.InnerException;
                        return ex.Message;
                    }

                    errorResponse = new ErrorResponse(
                        response.StatusCode,
                        "An unexpected error occurred",
                        GetInnermostException(exception) + " | " + exception.StackTrace
                    ); 
                    break;
            }


            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }
}
