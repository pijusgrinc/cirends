namespace CirendsAPI.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UnauthorizedAccessException2 : Exception
    {
        public UnauthorizedAccessException2(string message) : base(message) { }

        public UnauthorizedAccessException2(string message, Exception innerException) : base(message, innerException) { }
    }
}