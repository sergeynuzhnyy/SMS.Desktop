using Core.Services;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace Infrastructure.EnvVarService;

public class CommentService : ICommentService
{
    private readonly string _regPath;
    private readonly ILogger<CommentService> _logger;

    public CommentService(IOptionsMonitor<EnvVarSettings> settings, ILogger<CommentService> logger)
    {
        _regPath = settings.CurrentValue.RegPath ?? @"Software\EnvComments";
        _logger = logger;
    }
    
    public string? GetComment(string variable)
    {
        _logger.LogDebug($"[{nameof(CommentService)}.{nameof(GetComment)}] {variable}");
        
        if (string.IsNullOrWhiteSpace(variable))
        {
            return null;
        }
        
        using var key = Registry.CurrentUser.OpenSubKey(_regPath);
        return key?.GetValue(variable)?.ToString();
    }

    public bool SetComment(string variable, string? comment)
    {
        _logger.LogDebug($"[{nameof(CommentService)}.{nameof(SetComment)}] {variable}: {comment}");
        
        using var key = Registry.CurrentUser.CreateSubKey(_regPath);
        if (string.IsNullOrWhiteSpace(comment))
        {
            key.DeleteValue(variable);
        }
        else
        {
            key.SetValue(variable, comment);
        }

        return true;
    }
}