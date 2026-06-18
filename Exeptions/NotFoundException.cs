namespace Task_Management_Project.Exeptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource not found.") { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    }
}
