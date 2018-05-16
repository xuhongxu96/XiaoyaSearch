using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using XiaoyaStore.Helper;

namespace XiaoyaStore.Data.Model
{
    public partial class UrlFrontierItem
    {
        public string Host => UrlHelper.GetHost(Url);
        public UrlFrontierItemKey Key => new UrlFrontierItemKey(PlannedTime, Priority);
    }
}
