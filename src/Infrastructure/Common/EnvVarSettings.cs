using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Common;

public class EnvVarSettings
{
    public string? RegPath { get; set; }
    public string[] Variables { get; set; } = [];
}