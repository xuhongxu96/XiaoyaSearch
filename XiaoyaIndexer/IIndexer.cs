﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XiaoyaIndexer
{
    public interface IIndexer
    {
        Task CreateIndexAsync();
        void StopIndex();
        bool IsWaiting { get; }
    }
}
