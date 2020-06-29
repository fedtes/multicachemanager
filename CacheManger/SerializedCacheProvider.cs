using System;
using System.Collections.Generic;
using System.Text;

namespace CacheManager
{
    class SerializedCacheProvider : ICacheProvider
    {

        public SerializedCacheProvider(Repository repository)
        {

        }

        public IDataAccessor GetDataAccessor()
        {
            throw new NotImplementedException();
        }
    }
}
