namespace Sublime.Sign.Common.Exceptions;

public class InvalidSignatureException : Exception
{
    public InvalidSignatureException(string message) : base(message)
    {
    }

    public InvalidSignatureException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvalidSignatureException() : base("Signature mismatch. Ensure the correct private key has been used for signing on both services.") { }
}