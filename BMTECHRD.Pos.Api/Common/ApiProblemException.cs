namespace BMTECHRD.Pos.Api.Common;

public sealed class ApiProblemException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public string? ErrorCode { get; }

    public ApiProblemException(int statusCode, string title, string detail, string? errorCode = null)
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        ErrorCode = errorCode;
    }
}
