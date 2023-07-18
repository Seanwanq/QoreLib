using System;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Services;

namespace QoreLib;

public class ServiceLocator
{
    private static readonly Lazy<ServiceLocator> _instance = new (() => new ServiceLocator());
    public static ServiceLocator Instance => _instance.Value;

    public IDatabaseService DatabaseService { get; set; }
    public ServiceProvider ServiceProvider { get; set; }
    public WindowNotificationManager NotificationManager { get; set; }
    
    private ServiceLocator(){}
}