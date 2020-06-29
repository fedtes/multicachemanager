using System;
using LightInject;

namespace CacheManager
{
    public class CacheManagerService : ICacheProvider
    {

        private ServiceContainer ServiceContainer;

        private void RootComposition()
        {
            
        }
        public CacheManagerService()
        {
            ServiceContainer = new ServiceContainer();
            ServiceContainer.RegisterSingleton<ICacheProvider, StandardCacheProvider>();
            RootComposition();
        }

        public CacheManagerService(Type cacheprovider)
        {
            ServiceContainer = new ServiceContainer();
            ServiceContainer.RegisterSingleton(typeof(ICacheProvider), cacheprovider);
            RootComposition();
        }

        public IDataAccessor GetSlot<T>() where T : BaseSlotType
        {
            return ServiceContainer.GetInstance<ICacheProvider>().GetSlot<T>();
        }

        public void AddSlot<T>() where T : BaseSlotType
        {
            ServiceContainer.GetInstance<ICacheProvider>().AddSlot<T>();
        }

        public void AddSlot(Type slotType)
        {
            ServiceContainer.GetInstance<ICacheProvider>().AddSlot(slotType);
        }

        public void AddSlot(BaseSlotType SlotInstance)
        {
            ServiceContainer.GetInstance<ICacheProvider>().AddSlot(SlotInstance);
        }



    }
}
