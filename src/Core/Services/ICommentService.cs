namespace Core.Services;

public interface ICommentService
{
    string? GetComment(string variable);
    bool SetComment(string variable, string? comment);
}