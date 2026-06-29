namespace Company.Shared.Bootstrapping
{
    public class NotEncryptedException : Exception
    {
        public NotEncryptedException(string? message) : base(message)
        {
        }
    }
}
