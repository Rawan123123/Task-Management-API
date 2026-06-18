namespace Task_Management_Project.Exeptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("Unauthorized access.") { }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
