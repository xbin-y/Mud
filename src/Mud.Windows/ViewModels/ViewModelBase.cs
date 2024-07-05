using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mud.Windows.ViewModels;

public class ViewModelBase : ObservableObject
{
    public IServiceProvider ServiceProvider => Container.ServiceProvider;
}
