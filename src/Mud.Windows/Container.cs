using System;

namespace Mud.Windows;

public class Container
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
