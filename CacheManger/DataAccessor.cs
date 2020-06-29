using System;
using System.Collections.Generic;
using System.Text;

namespace CacheManager
{
    public interface ICacheKey
    {
        String GetCacheKey();
    }

    public interface IDataAccessor
    {
        T Get<T>(string key) where T : class; 
        T Get<T>(string key, Func<IDataAccessor,T,T> onMiss) where T : class;
        void Set<T>(string key, T value) where T : class;
        void Set<T>(string key, T value, Func<IDataAccessor,T,T> onHit) where T : class;
        void Delete(string key);
        T Get<T>(ICacheKey key) where T : class;
        T Get<T>(ICacheKey key, Func<IDataAccessor, T, T> onMiss) where T : class;
        void Set<T>(ICacheKey key, T value) where T : class;
        void Set<T>(ICacheKey key, T value, Func<IDataAccessor, T, T> onHit) where T : class;
        void Delete(ICacheKey key);
        void Invalidate();
    }

    internal class StandardSlotAccessor<S> : IDataAccessor where S : BaseSlotType
    {
        private BaseSlotType baseSlotType;
        private CacheStore cacheStore;

        public StandardSlotAccessor(BaseSlotType baseSlotType, CacheStore cacheStore)
        {
            this.cacheStore = cacheStore;
            this.baseSlotType = baseSlotType;
        }

        private SlotType<T> GetSlotType<T>() where T: class
        {
            return (SlotType<T>)baseSlotType;
        }

        public void Delete(string key)
        {
            if (cacheStore.store.ContainsKey(key))
            {
                cacheStore.store.Remove(key);
            }
        }

        public T Get<T>(string key) where T : class
        {
            return this.Get<T>(key, (a, d) => d);
        }

        public T Get<T>(string key, Func<IDataAccessor,T, T> onMiss) where T : class
        {
            var c = GetSlotType<T>();
            if (cacheStore.store.ContainsKey(key))
            {
                var i = cacheStore.store[key];

                if (c.ShouldExpire && i.eta < DateTime.Now)
                {
                    return onMiss(this, c.DefaultOnMiss);
                }
                else
                {
                    i.hitCount++;
                    return c.OnHit(i).value;
                }
            }
            else
            {
                return onMiss(this, c.DefaultOnMiss);
            }
        }

        public void Invalidate()
        {
            cacheStore.store.Clear();
        }

        public void Set<T>(string key, T value) where T : class
        {
            var c = GetSlotType<T>();
            if (c.ShouldExpire)
            {
                cacheStore.store.Add(key, new CacheItem() { value = value, eta = DateTime.Now.Add(c.Expiration) });
            } 
            else
            {
                cacheStore.store.Add(key, new CacheItem() { value = value, eta = DateTime.MinValue });
            }
        }

        public void Set<T>(string key, T value, Func<IDataAccessor, T, T> onHit) where T : class
        {
            var c = GetSlotType<T>();
            if (cacheStore.store.ContainsKey(key))
            {
                var v = onHit(this, cacheStore.store[key].value);
                var x = cacheStore.store[key];
                x.value = v;
                x.hitCount = 0;
                if (c.ShouldExpire)
                    x.eta = DateTime.Now.Add(c.Expiration);
                else
                    x.eta = DateTime.MinValue;
            }
            else
            {
                this.Set<T>(key, value);
            }
        }

        public T Get<T>(ICacheKey key) where T : class
        {
            return this.Get<T>(key.GetCacheKey());
        }

        public T Get<T>(ICacheKey key, Func<IDataAccessor, T, T> onMiss) where T : class
        {
            return this.Get<T>(key.GetCacheKey(), onMiss);
        }

        public void Set<T>(ICacheKey key, T value) where T : class
        {
            this.Set(key.GetCacheKey(), value);
        }

        public void Set<T>(ICacheKey key, T value, Func<IDataAccessor, T, T> onHit) where T : class
        {
            this.Set(key.GetCacheKey(), value, onHit);
        }

        public void Delete(ICacheKey key)
        {
            this.Delete(key.GetCacheKey());
        }
    }
}
