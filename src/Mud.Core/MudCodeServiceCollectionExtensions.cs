using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mud.Core;

public static class MudCodeServiceCollectionExtensions
{
    public static IServiceCollection AddMudCode(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient();
        services.TryAddSingleton<IStockManager, StockManager>();
        return services;
    }
}
