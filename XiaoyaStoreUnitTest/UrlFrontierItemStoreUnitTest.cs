using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XiaoyaStore.Config;
using XiaoyaStore.Data;
using XiaoyaStore.Data.MergeOperator;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Store;

namespace XiaoyaStoreUnitTest
{
    [TestClass]
    public class UrlFrontierItemStoreUnitTest
    {
        StoreConfig config = new StoreConfig
        {
            StoreDirectory = "Store",
        };

        string dbName = "UrlFrontierItems";

        [TestMethod]
        public void TestInit()
        {
            StoreTestHelper.DeleteDb(config, dbName);

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.Init(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });
            }

            using (var helper = new StoreTestHelper(config, dbName))
            {
                var cf = new ColumnFamilies
                {
                    { "HostCount", new ColumnFamilyOptions().SetMergeOperator(new CounterOperator()) },
                };
                helper.OpenDb(cf);

                var urls = new List<string>();

                using (var iter = helper.Db.NewIterator())
                {
                    for (iter.SeekToFirst(); iter.Valid(); iter.Next())
                    {
                        var item = ModelSerializer.DeserializeModel<UrlFrontierItem>(iter.Value());
                        urls.Add(item.Url);
                    }
                }
                Assert.IsTrue(urls.Contains("http://baidu.com"));
                Assert.IsTrue(urls.Contains("http://xuhongxu.com"));
            }
        }

        [TestMethod]
        public void TestPushUrls()
        {
            StoreTestHelper.DeleteDb(config, dbName);

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://google.com",
                });
            }


            using (var helper = new StoreTestHelper(config, dbName))
            {
                var cf = new ColumnFamilies
                {
                    { "HostCount", new ColumnFamilyOptions().SetMergeOperator(new CounterOperator()) },
                };
                helper.OpenDb(cf);


                var urls = new List<string>();

                using (var iter = helper.Db.NewIterator())
                {
                    for (iter.SeekToFirst(); iter.Valid(); iter.Next())
                    {
                        var item = ModelSerializer.DeserializeModel<UrlFrontierItem>(iter.Value());
                        urls.Add(item.Url);
                    }
                }
                Assert.IsTrue(urls.Contains("http://baidu.com"));
                Assert.IsTrue(urls.Contains("http://xuhongxu.com"));
                Assert.IsTrue(urls.Contains("http://google.com"));
            }
        }

        [TestMethod]
        public void TestPushBack()
        {
            StoreTestHelper.DeleteDb(config, dbName);

            string poppedUrl;
            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                Assert.IsFalse(urlFrontierItemStore.PushBack("http://baidu.com",
                    TimeSpan.FromDays(1)));

                Assert.IsFalse(urlFrontierItemStore.PushBack("http://a.com",
                    TimeSpan.FromDays(1)));

                poppedUrl = urlFrontierItemStore.PopUrlForCrawl();

                Assert.IsTrue(urlFrontierItemStore.PushBack(poppedUrl,
                    TimeSpan.FromDays(1)));
            }

            using (var helper = new StoreTestHelper(config, dbName))
            {
                var cf = new ColumnFamilies
                {
                    { "HostCount", new ColumnFamilyOptions().SetMergeOperator(new CounterOperator()) },
                };
                helper.OpenDb(cf);

                var urls = new List<string>();

                using (var iter = helper.Db.NewIterator())
                {
                    for (iter.SeekToFirst(); iter.Valid(); iter.Next())
                    {
                        var item = ModelSerializer.DeserializeModel<UrlFrontierItem>(iter.Value());
                        if (item.Url == poppedUrl)
                        {
                            Console.WriteLine("PlannedTime: {0}", item.PlannedTime);
                            Console.WriteLine("Priority: {0}", item.Priority);
                            Assert.IsTrue(item.PlannedTime > DateTime.Now.AddHours(1));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            StoreTestHelper.DeleteDb(config, dbName);

            string poppedUrl;
            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                });

                poppedUrl = urlFrontierItemStore.PopUrlForCrawl();

                urlFrontierItemStore.Remove(poppedUrl);
            }

            using (var helper = new StoreTestHelper(config, dbName))
            {
                var cf = new ColumnFamilies
                {
                    { "HostCount", new ColumnFamilyOptions().SetMergeOperator(new CounterOperator()) },
                };
                helper.OpenDb(cf);

                var urls = new List<string>();

                using (var iter = helper.Db.NewIterator())
                {
                    for (iter.SeekToFirst(); iter.Valid(); iter.Next())
                    {
                        var item = ModelSerializer.DeserializeModel<UrlFrontierItem>(iter.Value());
                        if (item.Url == poppedUrl)
                        {
                            Assert.Fail(item.Url + " should be removed");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestGetHostCount()
        {
            StoreTestHelper.DeleteDb(config, dbName);

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://baidu.com",
                    "http://xuhongxu.com",
                    "http://xuhongxu.com/a",
                    "http://xuhongxu.com/b",
                });
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                Assert.AreEqual(1, urlFrontierItemStore.GetHostCount("baidu.com"));
                Assert.AreEqual(3, urlFrontierItemStore.GetHostCount("xuhongxu.com"));
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.PushUrls(new List<string>
                {
                    "http://baidu.com/a",
                    "http://xuhongxu.com",
                    "http://xuhongxu.com/c",
                    "http://xuhongxu.com/d",
                });
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                Assert.AreEqual(2, urlFrontierItemStore.GetHostCount("baidu.com"));
                Assert.AreEqual(5, urlFrontierItemStore.GetHostCount("xuhongxu.com"));
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                urlFrontierItemStore.Remove("http://xuhongxu.com");
            }

            using (var urlFrontierItemStore = new UrlFrontierItemStore(config))
            {
                Assert.AreEqual(2, urlFrontierItemStore.GetHostCount("baidu.com"));
                Assert.AreEqual(4, urlFrontierItemStore.GetHostCount("xuhongxu.com"));
            }
        }
    }
}
