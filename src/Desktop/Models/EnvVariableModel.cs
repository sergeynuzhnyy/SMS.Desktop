using CommunityToolkit.Mvvm.ComponentModel;

namespace Desktop.Models;

public partial class EnvVariableModel : ObservableObject, IEquatable<EnvVariableModel>
{
    [ObservableProperty] private string _name = null!;
    [ObservableProperty] private string? _value;
    [ObservableProperty] private string? _comment;

    public bool Equals(EnvVariableModel? other)
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
        return Equals((EnvVariableModel)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}