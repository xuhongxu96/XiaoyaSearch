using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data.Model
{
    public class Link
    {
        public int LinkId { get; set; }
        public int UrlFileId { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
    }
}
