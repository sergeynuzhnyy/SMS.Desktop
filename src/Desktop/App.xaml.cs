using System.Configuration;
using System.Data;
using System.Windows;
using Desktop.ViewModels;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var host = new HostBuilder()
            .ConfigureHost(e.Args)
            .ConfigureServices((context, collection) =>
            {
                collection.AddTransient<MainWindowViewModel>();
            })
            .Build();

        MainWindow window = new()
        {
            DataContext = host.Services.GetRequiredService<MainWindowViewModel>()
        };
        window.Show();
    }
}