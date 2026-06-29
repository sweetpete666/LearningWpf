namespace LearningWpf.Helper
{
    public class NotEncryptedException : Exception
    {
        public NotEncryptedException(string? message) : base(message)
        {
        }
    }
}
