using Contracts.Models;
using Core.Services;
using Infrastructure.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.EnvVarService;

public class EnvVarService : IEnvVarService
{
    public event EventHandler? Changed;
    
    private readonly ICommentService _commentService;
    private readonly IOptionsMonitor<EnvVarSettings> _optionsMonitor;
    private EnvVarSettings EnvVarSettings => _optionsMonitor.CurrentValue; 
    private readonly ILogger<EnvVarService> _logger;
    
    public EnvVarService(
        ICommentService commentService, 
        IOptionsMonitor<EnvVarSettings> optionsMonitor, 
        ILogger<EnvVarService> logger)
    {
        _commentService = commentService;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        
        _optionsMonitor.OnChange(_ => Changed?.Invoke(this, EventArgs.Empty));
    }
    
    public HashSet<EnvVariable> GetAllVariablesAsync()
    {
        _logger.LogDebug($"[{nameof(EnvVarService)}] {nameof(GetAllVariablesAsync)}");
        
        if (EnvVarSettings.Variables.Length == 0)
        {
            return [];
        }

        var allVariables = EnvVarSettings.Variables.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        HashSet<EnvVariable> variables = [];
        foreach (var variable in allVariables)
        {
            var value = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);
            variables.Add(new EnvVariable
            {
                Name = variable,
                Value = value,
                Comment = _commentService.GetComment(variable)
            });
        }
        _logger.LogDebug($"[{nameof(EnvVarService)}] {nameof(GetAllVariablesAsync)} Count: {allVariables.Length}");
        
        return variables;
    }

    public bool SetVariableAsync(EnvVariable variable)
    {
        _logger.LogDebug($"[{nameof(EnvVarService)}] {nameof(SetVariableAsync)}");
        
        if (string.IsNullOrWhiteSpace(variable.Name))
        {
            return false;
        }
        
        Environment.SetEnvironmentVariable(variable.Name, variable.Value, EnvironmentVariableTarget.User);
        _commentService.SetComment(variable.Name, variable.Comment);
        
        return true;
    }
}