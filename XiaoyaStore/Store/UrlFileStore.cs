using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XiaoyaStore.Data;
using XiaoyaStore.Data.Model;
using XiaoyaStore.Helper;

namespace XiaoyaStore.Store
{
    public class UrlFileStore : IUrlFileStore
    {
        protected XiaoyaSearchContext mContext;
        public UrlFileStore(XiaoyaSearchContext context)
        {
            mContext = context;
        }

        public async Task<UrlFile> LoadAnyForIndexAsync()
        {
            var urlFile = mContext.UrlFiles
                .OrderBy(o => o.UpdatedAt)
                .FirstOrDefault(o => o.IsIndexed == false);

            urlFile.IsIndexed = true;
            await mContext.SaveChangesAsync();

            return urlFile;
        }

        public UrlFile LoadByFilePath(string path)
        {
            return mContext.UrlFiles.SingleOrDefault(o => o.FilePath == path);
        }

        public UrlFile LoadByUrl(string url)
        {
            return mContext.UrlFiles.SingleOrDefault(o => o.Url == url);
        }

        public async Task<UrlFile> SaveAsync(UrlFile urlFile)
        {
            using (var dbContextTransaction = await mContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Find if the url exists
                    var oldUrlFile = mContext.UrlFiles.SingleOrDefault(o => o.Url == urlFile.Url);
                    if (oldUrlFile != null)
                    {
                        var updateInterval = DateTime.Now.Subtract(oldUrlFile.UpdatedAt);
                        oldUrlFile.UpdateInterval = (oldUrlFile.UpdateInterval * 3 + updateInterval) / 4;

                        // Exists this url, then judge if two fetched file is same
                        if (oldUrlFile.FileHash == urlFile.FileHash
                            && (oldUrlFile.Content != "" && oldUrlFile.Content == urlFile.Content
                           || FileHelper.FilesAreEqual(oldUrlFile.FilePath, urlFile.FilePath)))
                        {
                            // Same
                            // Delete new file
                            File.Delete(urlFile.FilePath);
                        }
                        else
                        {
                            // Updated
                            oldUrlFile.UpdatedAt = DateTime.Now;

                            // Delete old file
                            File.Delete(oldUrlFile.FilePath);

                            // Update info
                            oldUrlFile.IsIndexed = false;
                            oldUrlFile.FilePath = urlFile.FilePath;
                            oldUrlFile.FileHash = urlFile.FileHash;
                            oldUrlFile.Content = urlFile.Content;
                            oldUrlFile.Charset = urlFile.Charset;
                            oldUrlFile.MimeType = urlFile.MimeType;
                        }

                        urlFile = oldUrlFile;
                    }
                    else
                    {
                        // first see this url, add to database
                        urlFile.IsIndexed = false;
                        urlFile.UpdatedAt = DateTime.Now;
                        urlFile.CreatedAt = DateTime.Now;
                        urlFile.UpdateInterval = TimeSpan.FromDays(3);
                        await mContext.UrlFiles.AddAsync(urlFile);
                    }
                    await mContext.SaveChangesAsync();

                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                }
                return urlFile;
            }
        }

        public async Task<UrlFile> SaveContentAsync(int urlFileId, string content)
        {
            var urlFile = mContext.UrlFiles.SingleOrDefault(o => o.UrlFileId == urlFileId);
            urlFile.Content = content;
            await mContext.SaveChangesAsync();
            return urlFile;
        }
    }
}
