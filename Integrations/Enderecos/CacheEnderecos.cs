using Microsoft.Extensions.Caching.Memory; namespace BD_TRAMPO.Integrations.Enderecos; public sealed class CacheEnderecos():MemoryCache(new MemoryCacheOptions {SizeLimit=500});
