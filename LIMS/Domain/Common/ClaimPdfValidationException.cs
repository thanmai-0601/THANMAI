namespace Domain.Common;

public class ClaimPdfValidationException : Exception
{
    public ClaimPdfValidationException(string message) : base(message)
    {
    }
}
