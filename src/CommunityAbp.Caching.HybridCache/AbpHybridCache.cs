using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Volo.Abp.Caching;

namespace CommunityAbp.Caching.HybridCache
{
    /// <summary>
    /// The module for using .net 9's new Hybrid Cache
    /// </summary>
    public class AbpHybridCache : Microsoft.Extensions.Caching.Hybrid.HybridCache, ICacheSupportsMultipleItems, IDistributedCache
    {
        private readonly IDistributedCache? _backendCache;
        private readonly DistributedCacheEntryOptions _defaultDistributedCacheExpiration;
        private readonly TimeSpan _defaultExpiration;
        private readonly HybridCacheEntryFlags _defaultFlags;

        // note this already includes hardFlags
        private readonly TimeSpan _defaultLocalCacheExpiration;

        private readonly CacheFeatures _features;
        private readonly HybridCacheEntryFlags _hardFlags;
        private readonly IMemoryCache _localCache;
        private readonly ILogger _logger;
        private readonly HybridCacheOptions _options;
        private readonly IHybridCacheSerializerFactory[] _serializerFactories;
        private readonly IServiceProvider _services; // we can't resolve per-type serializers until we see each T
                                                     // used to avoid constant type-testing

        public AbpHybridCache(IOptions<HybridCacheOptions> options, IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _localCache = services.GetRequiredService<IMemoryCache>();
            _options = options.Value;
            _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(AbpHybridCache)) ?? NullLogger.Instance;

            _backendCache = services.GetService<IDistributedCache>(); // note optional

            // ignore L2 if it is really just the same L1, wrapped
            // (note not just an "is" test; if someone has a custom subclass, who knows what it does?)
            if (_backendCache is not null
                && _backendCache.GetType() == typeof(MemoryDistributedCache)
                && _localCache.GetType() == typeof(MemoryCache))
            {
                _backendCache = null;
            }

            // perform type-tests on the backend once only
            _features |= _backendCache switch
            {
                IBufferDistributedCache => CacheFeatures.BackendCache | CacheFeatures.BackendBuffers,
                not null => CacheFeatures.BackendCache,
                _ => CacheFeatures.None
            };

            // When resolving serializers via the factory API, we will want the *last* instance,
            // i.e. "last added wins"; we can optimize by reversing the array ahead of time, and
            // taking the first match
            var factories = services.GetServices<IHybridCacheSerializerFactory>().ToArray();
            Array.Reverse(factories);
            _serializerFactories = factories;

            //MaximumPayloadBytes = checked((int)_options.MaximumPayloadBytes); // for now hard-limit to 2GiB

            var defaultEntryOptions = _options.DefaultEntryOptions;

            if (_backendCache is null)
            {
                _hardFlags |= HybridCacheEntryFlags.DisableDistributedCache;
            }
            _defaultFlags = (defaultEntryOptions?.Flags ?? HybridCacheEntryFlags.None) | _hardFlags;
            _defaultExpiration = defaultEntryOptions?.Expiration ?? TimeSpan.FromMinutes(5);
            _defaultLocalCacheExpiration = defaultEntryOptions?.LocalCacheExpiration ?? TimeSpan.FromMinutes(1);
            _defaultDistributedCacheExpiration = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultExpiration };
        }

        // *always* present (for example, because no L2)
        [Flags]
        internal enum CacheFeatures
        {
            None = 0,
            BackendCache = 1 << 0,
            BackendBuffers = 1 << 1,
        }

        internal IDistributedCache? BackendCache => _backendCache;

        internal IMemoryCache LocalCache => _localCache;

        internal HybridCacheOptions Options => _options;

        public byte[]? Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public byte[]?[] GetMany(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]?[]> GetManyAsync(IEnumerable<string> keys, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void RefreshMany(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        public Task RefreshManyAsync(IEnumerable<string> keys, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RemoveKeyAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void RemoveMany(IEnumerable<string> keys)
        {
            throw new NotImplementedException();
        }

        public Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RemoveTagAsync(string tag, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void SetMany(IEnumerable<KeyValuePair<string, byte[]>> items, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public Task SetManyAsync(IEnumerable<KeyValuePair<string, byte[]>> items, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        // used to restrict features in test suite
        internal void DebugRemoveFeatures(CacheFeatures features) => Unsafe.AsRef(in _features) &= ~features;

        internal CacheFeatures GetFeatures() => _features;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HybridCacheEntryFlags GetEffectiveFlags(HybridCacheEntryOptions? options)
            => (options?.Flags | _hardFlags) ?? _defaultFlags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CacheFeatures GetFeatures(CacheFeatures mask) => _features & mask;
    }
}