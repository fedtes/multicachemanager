using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CacheManager
{
    class FastReferenceCacheProvider : ICacheProvider, IDataAccessor
    {
        private ConcurrentDictionary<string, CacheItem> values 
            = new ConcurrentDictionary<string, CacheItem>();

        private readonly Repository repo;

        public FastReferenceCacheProvider(Repository repository)
        {
            repo = repository;
        }

        public IDataAccessor GetDataAccessor()
        {
            return (IDataAccessor)this;
        }

        public void Delete(string key)
        {
            CacheItem v;
            values.TryRemove(key, out v);
        }

        public void Delete(ICacheKey key)
        {
            this.Delete(key.GetCacheKey());
        }

        public dynamic Get(string key)
        {
            return this.Get(key, (k, v) => v);
        }

        public dynamic Get(string key, Func<IDataAccessor, dynamic, dynamic> onMiss)
        {
            CacheItem v;
            if (values.TryGetValue(key, out v))
            {
                if (repo.ShouldExpire)
                {
                    if (v.eta > DateTime.Now)
                    {
                        if (v.hitCount <= repo.MaxHit)
                        {
                            CacheItem cacheItem = new ReadOnlyCacheItem(v);

                            if (repo.IsValid(cacheItem))
                            {
                                var c =  repo.OnHit(cacheItem);
                                return values.AddOrUpdate(key, v, (k, v1) => repo.OnHit(v1)).value;
                            }
                        }
                    }
                }

                this.Delete(key);
                return null;
            }
            else
            {
                return onMiss(this, repo.DefaultOnMiss);
            }
        }

        public dynamic Get(ICacheKey key)
        {
            return this.Get(key.GetCacheKey(), (k,v) => v);
        }

        public dynamic Get(ICacheKey key, Func<IDataAccessor, dynamic, dynamic> onMiss)
        {
            return this.Get(key.GetCacheKey(), onMiss);
        }

        public void Set(string key, dynamic value)
        {
            Func<IDataAccessor, dynamic, dynamic> x = (i, d) => d;
            this.Set(key, value, x);
        }

        public void Set(string key, dynamic value, Func<IDataAccessor, dynamic, dynamic> onHit)
        {
            var c = new CacheItem()
            {
                value = value,
                eta = repo.ShouldExpire ? DateTime.Now.Add(repo.Expiration) : DateTime.MaxValue,
                hitCount = 0
            };

            CacheItem upd(string k, CacheItem found) {
                var x = onHit(this, found.value);
                found.value = x;
                return found;
            };

            values.AddOrUpdate(key, c, upd);
        }


        public void Set(ICacheKey key, dynamic value)
        {
            this.Set(key.GetCacheKey(), value);
        }

        public void Set(ICacheKey key, dynamic value, Func<IDataAccessor, dynamic, dynamic> onHit)
        {
            this.Set(key.GetCacheKey(), value, onHit);
        }


        public void Invalidate()
        {
            throw new NotImplementedException();
        }
    }
}
