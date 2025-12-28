using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts.Models;
using Core.Services;
using Desktop.Models;
using Microsoft.Extensions.Logging;

namespace Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableValidator
{
    private static readonly SemaphoreSlim _lockUpdateVariables = new(1, 1);
    private static readonly SemaphoreSlim _lockSetVariables = new(1, 1);
    
    private readonly IEnvVarService _envVarService;
    private readonly ILogger<MainWindowViewModel> _logger;
    
    [ObservableProperty] private bool _appliedChanges = false;
    
    private ImmutableDictionary<string, EnvVariable> _originalVariables 
        = ImmutableDictionary<string, EnvVariable>.Empty;
    private ObservableCollection<EnvVariableModel> _variables = [];
    public ReadOnlyObservableCollection<EnvVariableModel> Variables { get; }
    

    public MainWindowViewModel(IEnvVarService envVarService, ILogger<MainWindowViewModel> logger)
    {
        _envVarService = envVarService;
        _logger = logger;
        
        Variables = new ReadOnlyObservableCollection<EnvVariableModel>(_variables);
        
        UpdateEnvVariables();
        _envVarService.Changed += EnvVarServiceOnChanged;
    }

    private async void UpdateEnvVariables()
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await _lockUpdateVariables.WaitAsync();
            try
            {
                AppliedChanges = true;

                _variables.Clear();
                _originalVariables = _envVarService.GetAllVariablesAsync()
                    .ToImmutableDictionary(variable => variable.Name);
                foreach (var variable in _originalVariables)
                {
                    EnvVariableModel variableModel = new()
                    {
                        Name = variable.Value.Name,
                        Value = variable.Value.Value,
                        Comment = variable.Value.Comment
                    };
                    variableModel.PropertyChanged += VariableModelOnPropertyChanged;
                    _variables.Add(variableModel);
                }

                AppliedChanges = false;
            }
            finally
            {
                _lockUpdateVariables.Release();
            }
        });
    }

    private async void VariableModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnvVariableModel.Name))
        {
            return;
        }

        await _lockSetVariables.WaitAsync();
        try
        {
            var originalVariables = _originalVariables.ToImmutableDictionary();
            var changedVariables = _variables
                .Where(variable => originalVariables.TryGetValue(variable.Name, out var originalVariable)
                                   && (variable.Value != originalVariable.Value
                                       || variable.Comment != originalVariable.Comment))
                .ToArray();

            AppliedChanges = true;
            await Task.Run(() =>
            {
                foreach (var changedVariable in changedVariables)
                {
                    if (_envVarService.SetVariableAsync(new EnvVariable
                        {
                            Name = changedVariable.Name,
                            Value = changedVariable.Value,
                            Comment = changedVariable.Comment
                        }))
                    {
                        var changedValue = originalVariables[changedVariable.Name].Value != changedVariable.Value
                            ? $"'Value: {originalVariables[changedVariable.Name].Value} => {changedVariable.Value}'"
                            : null;
                        var changedComment = originalVariables[changedVariable.Name].Comment != changedVariable.Comment
                            ? $"'Comment: {originalVariables[changedVariable.Name].Comment} => {changedVariable.Comment}'"
                            : null;
                        
                        _logger.LogInformation("Variable {name} was changed {changes}", 
                            changedVariable.Name, 
                            string.Join(", ", changedValue, changedComment));
                    }
                }
            });
            AppliedChanges = false;

            UpdateEnvVariables();
        }
        finally
        {
            _lockSetVariables.Release();
        }
    }

    private void EnvVarServiceOnChanged(object? sender, EventArgs e)
    {
        UpdateEnvVariables();
    }

    [RelayCommand]
    private void Close()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void Minimize()
    {
        var window = Application.Current.MainWindow;
        if (window != null)
        {
            window.WindowState = WindowState.Minimized;
        }
    }
}