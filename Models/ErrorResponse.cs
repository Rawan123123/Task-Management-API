namespace Task_Management_Project.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }

        public ErrorResponse() { }

        public ErrorResponse(int statusCode, string message, object details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}
