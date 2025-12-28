namespace Contracts.Models;

public class EnvVariable : IEquatable<EnvVariable>
{
    public string Name { get; init; } = null!;
    public string? Value { get; set; }
    public string? Comment { get; set; }


    public bool Equals(EnvVariable? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EnvVariable)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}