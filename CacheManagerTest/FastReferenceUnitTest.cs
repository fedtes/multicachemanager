using CacheManager;
using System;
using LightInject;
using Xunit;

namespace CacheManagerTest
{
    public class FastReferenceUnitTest
    {

        public class SimpleRepo : Repository
        {
            public override string CacheProvider => CACHES.FASTREFERENCE;
        }

        public class SimpleRepo2 : Repository
        {
            public override string CacheProvider => CACHES.FASTREFERENCE;
        }


        [Fact]
        public void It_should_store_some_infos()
        {
            var x = new CacheManagerService();
            x.AddRepository<SimpleRepo>();
            var repo = x.ServiceContainer.GetInstance<SimpleRepo>();

            Assert.NotNull(repo);
            Assert.Equal(CACHES.FASTREFERENCE, repo.CacheProvider);

            var d = x.GetRepository<SimpleRepo>();
            d.Set("somekey", "somevalue");
            Assert.Equal("somevalue", d.Get("somekey"));

            d.Set("somekey1", "somevalue1");
            Assert.Equal("somevalue1", d.Get("somekey1"));
        }

        [Fact]
        public void It_should_remove_keys()
        {
            var x = new CacheManagerService();
            x.AddRepository<SimpleRepo>();

            var d = x.GetRepository<SimpleRepo>();
            d.Set("somekey", "somevalue");
            Assert.Equal("somevalue", d.Get("somekey"));
            d.Delete("somekey");
            Assert.Null(d.Get("somekey"));
        }

        [Fact]
        public void It_should_handle_multiple_repo()
        {
            var x = new CacheManagerService();
            x.AddRepository<SimpleRepo>();
            x.AddRepository<SimpleRepo2>();

            var d = x.GetRepository<SimpleRepo>();
            d.Set("somekey", "somevalue");
            var d1 = x.GetRepository<SimpleRepo2>();
            d1.Set("somekey1", "somevalue1");

            Assert.Equal("somevalue", d.Get("somekey"));
            Assert.Equal("somevalue1", d1.Get("somekey1"));
        }
    }
}
