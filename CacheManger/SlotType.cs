using System;
using System.Collections.Generic;
using System.Text;

namespace CacheManager
{
    public abstract class BaseSlotType { }

    public abstract class SlotType<T> : BaseSlotType where T : class
    {
        public abstract T ValueType { get; }
        public virtual T DefaultOnMiss { get => null; }
        public virtual TimeSpan Expiration { get => TimeSpan.FromMinutes(5); }
        public virtual bool ShouldExpire { get => true; }
        public virtual Func<CacheItem, CacheItem> OnHit { get => (c) => c; }
    }
}
