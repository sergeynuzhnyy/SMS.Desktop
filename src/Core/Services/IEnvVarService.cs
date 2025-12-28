using Contracts.Models;

namespace Core.Services;

public interface IEnvVarService
{
    event EventHandler? Changed;
    
    HashSet<EnvVariable> GetAllVariablesAsync();
    bool SetVariableAsync(EnvVariable variable);
}