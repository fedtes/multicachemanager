using System;
using System.Runtime.CompilerServices;
using LightInject;

[assembly: InternalsVisibleTo("CacheManagerTest")]

namespace CacheManager
{
    /// <summary>
    /// Available cache types
    /// </summary>
    public static class CACHES
    {
        /// <summary>
        /// Store values by ref. This is fastest but less safe in case of not immutable objects
        /// </summary>
        public const string FASTREFERENCE = "FASTREFERENCE";

        /// <summary>
        /// Store values as serialized values. Slower, the value should be serializable, but safer. NOT IMPLEMENTED YET.
        /// </summary>
        public const string SERIALIZED = "SERIALIZED";
    }

    public interface ICacheKey
    {
        String GetCacheKey();
    }

    public interface IDataAccessor
    {
        dynamic Get(string key);
        dynamic Get(string key, Func<IDataAccessor, dynamic, dynamic> onMiss);
        void Set(string key, dynamic value);
        void Set(string key, dynamic value, Func<IDataAccessor, dynamic, dynamic> onHit);
        void Delete(string key);
        dynamic Get(ICacheKey key);
        dynamic Get(ICacheKey key, Func<IDataAccessor, dynamic, dynamic> onMiss);
        void Set(ICacheKey key, dynamic value);
        void Set(ICacheKey key, dynamic value, Func<IDataAccessor, dynamic, dynamic> onHit);
        void Delete(ICacheKey key);
        void Invalidate();
    }

    public interface ICacheProvider
    {
        IDataAccessor GetDataAccessor();
    }

    public class CacheItem
    {
        public virtual dynamic value { get; set; }
        public virtual DateTime eta { get; set; }
        public virtual int hitCount { get; set; }
    }

    internal class ReadOnlyCacheItem : CacheItem
    {
        public dynamic _value;
        public DateTime _eta;
        public int _hitCount;

        internal ReadOnlyCacheItem(CacheItem cacheItem)
        {
            _value = cacheItem.value;
            _eta = cacheItem.eta;
            _hitCount = cacheItem.hitCount;
        }

        public override dynamic value { get => _value; set => throw new NotSupportedException("You cannot set this value here! To edit use instead the onHit delegate in the Repository configuration."); }
        public override DateTime eta { get => _eta; set => throw new NotSupportedException("You cannot set this value here! To edit use instead the onHit delegate in the Repository configuration."); }
        public override int hitCount { get => _hitCount; set => throw new NotSupportedException("You cannot set this value here! To edit use instead the onHit delegate in the Repository configuration."); }
    }

    public abstract class Repository
    {
        public abstract String CacheProvider { get; }
        public virtual dynamic DefaultOnMiss { get => null; }
        public virtual TimeSpan Expiration { get => TimeSpan.FromMinutes(5); }
        public virtual bool ShouldExpire { get => true; }
        public virtual int MaxHit { get => int.MaxValue; }
        public virtual Func<CacheItem, CacheItem> OnHit { get => (c) => c; }
        public virtual Func<CacheItem, Boolean> IsValid { get => (c) => true; }
    }

    public class CacheManagerService
    {
        internal ServiceContainer ServiceContainer;

        public CacheManagerService()
        {
            ServiceContainer = new ServiceContainer();
            ServiceContainer.Register<Repository, ICacheProvider>( 
                (s, r) => new FastReferenceCacheProvider(r), 
                CACHES.FASTREFERENCE
            );

            ServiceContainer.Register<Repository, ICacheProvider>(
                (s, r) => new SerializedCacheProvider(r),
                CACHES.SERIALIZED
            );
        }

        public IDataAccessor GetRepository<T>() where T : Repository
        {
            var repo = ServiceContainer.GetInstance<T>();
            return ServiceContainer.GetInstance<ICacheProvider>($"{typeof(T).GetHashCode()}").GetDataAccessor();

        }

        public void AddRepository<T>() where T : Repository, new()
        {
            T repo = new T();
            ServiceContainer.RegisterInstance<T>(repo);
            ICacheProvider cacheProvider = ServiceContainer.GetInstance<Repository, ICacheProvider>(repo, repo.CacheProvider);
            ServiceContainer.RegisterInstance<ICacheProvider>(cacheProvider, $"{typeof(T).GetHashCode()}");
        }
        
    }
}
