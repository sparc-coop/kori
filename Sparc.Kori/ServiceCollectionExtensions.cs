using Microsoft.Extensions.DependencyInjection;
using Sparc.Blossom;

namespace Sparc.Kori;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKoriDb(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(PouchDbRepository<>));
        return services;
    }
}
