namespace Task_Management_Project.Exeptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("Bad Request.") { }
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }

    }
}
