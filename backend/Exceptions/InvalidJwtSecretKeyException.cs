namespace backend
{
    public class InvalidJwtSecretKeyException : Exception
    {
        public InvalidJwtSecretKeyException(string message) : base(message) { }
    }
}