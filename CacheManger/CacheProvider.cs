using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CacheManager
{
    public interface ICacheProvider
    {
        IDataAccessor GetSlot<T>() where T : BaseSlotType;
        void AddSlot<T>() where T : BaseSlotType;
        void AddSlot(Type slotType);
        void AddSlot(BaseSlotType SlotInstance);
    }

    public class CacheItem
    {
        public dynamic value;
        public DateTime eta;
        public int hitCount;
    }

    internal class CacheStore
    {
        public IDictionary<string, CacheItem> store = new ConcurrentDictionary<string, CacheItem>();
    }

    internal class StandardCacheProvider : ICacheProvider
    {

        private IDictionary<Type, BaseSlotType> slots = new ConcurrentDictionary<Type, BaseSlotType>();

        private IDictionary<BaseSlotType, CacheStore> cache = new ConcurrentDictionary<BaseSlotType, CacheStore>();

        public StandardCacheProvider()
        {
        }

        public void AddSlot<T>() where T : BaseSlotType
        {
            this.AddSlot(typeof(T));
        }

        public void AddSlot(Type slotType)
        {
            if (!slots.ContainsKey(slotType))
            {
                var b = (BaseSlotType)Activator.CreateInstance(slotType);
                slots.Add(slotType, b);
                cache.Add(b, new CacheStore());
            }
        }

        public void AddSlot(BaseSlotType SlotInstance)
        {
            if (!slots.ContainsKey(SlotInstance.GetType()))
            {
                slots.Add(SlotInstance.GetType(), SlotInstance);
                cache.Add(SlotInstance, new CacheStore());
            }
        }

        public IDataAccessor GetSlot<T>() where T : BaseSlotType
        {
            if (slots.ContainsKey(typeof(T)))
                return new StandardSlotAccessor<T>(slots[typeof(T)], cache[slots[typeof(T)]]);
            else
                return null;
        }
    }

}
