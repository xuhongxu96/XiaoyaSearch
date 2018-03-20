using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaStore.Data;

namespace XiaoyaStore.Store
{
    public abstract class BaseStore
    {
        protected DbContextOptions mOptions = null;
        public BaseStore(DbContextOptions options = null)
        {
            mOptions = options;
        }

        protected XiaoyaSearchContext NewContext()
        {
            if (mOptions == null)
            {
                return new XiaoyaSearchContext();
            }
            else
            {
                return new XiaoyaSearchContext(mOptions);
            }
        }
    }
}
