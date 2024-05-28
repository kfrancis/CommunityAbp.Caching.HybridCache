using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace CommunityAbp.Caching.HybridCache
{
    /// <summary>
    /// The module for using .net 9's new Hybrid Cache  
    /// </summary>
    [DependsOn(typeof(Volo.Abp.Caching.AbpCachingModule))]
    public class AbpHybridCacheModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            var hybridCacheEnabled = configuration.GetValue<bool>("HybridCache:Enabled", false);
            if (hybridCacheEnabled)
            {
                context.Services.AddHybridCache();
                context.Services.Replace(ServiceDescriptor.Singleton<IDistributedCache, AbpHybridCache>());
            }
        }
    }
}
