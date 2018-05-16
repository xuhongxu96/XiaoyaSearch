using System;
using System.Collections.Generic;
using System.Text;
using XiaoyaLogger;

namespace XiaoyaStore.Config
{
    public class StoreConfig
    {
        public string StoreDirectory { get; set; }
        public RuntimeLogger Logger { get; set; }
    }
}
