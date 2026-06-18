namespace Task_Management_Project.Exeptions
{
    public class ValidationException : Exception
    {
        public object Details { get; set; }

        public ValidationException() : base("Validation failed.") { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, object details) : base(message)
        {
            Details = details;
        }
    }
}
