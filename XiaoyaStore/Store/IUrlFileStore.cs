﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data.Model;

namespace XiaoyaStore.Store
{
    public interface IUrlFileStore
    {
        UrlFile Save(UrlFile urlFile);
        UrlFile LoadByUrl(string url);
        UrlFile LoadByFilePath(string path);
        UrlFile LoadAnyForIndex();
    }
}
