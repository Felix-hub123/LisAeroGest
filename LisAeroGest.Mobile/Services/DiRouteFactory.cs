using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile.Services
{
    public class DiRouteFactory<TPage> : RouteFactory
        where TPage : Element
    {
        public override Element GetOrCreate()
        {
            var services = IPlatformApplication.Current?.Services;

            if (services == null)
            {
                throw new InvalidOperationException(
                    "Não foi possível aceder aos serviços da aplicação.");
            }

            return services.GetRequiredService<TPage>();
        }

        public override Element GetOrCreate(IServiceProvider services)
        {
            return services.GetRequiredService<TPage>();
        }
    }
}