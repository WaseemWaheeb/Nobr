using System;
using System.Collections;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.AzureBlob.Extensions;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.AzureBlob.Services
{
    public class StoreDownloadService : DownloadService
    {

        #region Const
        public const string DOWNLOAD_STORE_TYPE_AZURE = "Azure";
        public const string DOWNLOAD_STORE_TYPE_DATABASE = "Database";
        #endregion

        #region Fields

        private readonly IRepository<Download> _downloadRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly BlobService _blobSettingService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        #endregion

        #region Ctor
        public StoreDownloadService(IRepository<Download> downloadRepository,
            IEventPublisher eventPublisher,  BlobService blobSettingService,
            ISettingService settingService,
            ILogger logger
            ) : base(downloadRepository, eventPublisher)
        {
            _downloadRepository = downloadRepository;
            _eventPublisher = eventPublisher;
            _blobSettingService = blobSettingService;
            _settingService = settingService;
            _logger = logger;
        }

        public string StoreType {
            get
            {
                var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
                return azureBlobSetting.DownloadStoreType;
            }
            set
            {
                 //check whether it's a new value
                if (StoreType != value)
                {
                    var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
                    var oldValue = azureBlobSetting.DownloadStoreType;
                    if (String.IsNullOrEmpty(oldValue))
                        oldValue = DOWNLOAD_STORE_TYPE_DATABASE;
                    azureBlobSetting.DownloadStoreType = value;
                    _settingService.SaveSetting(azureBlobSetting);
                    //update all picture objects
                    var downloads = GetDownloads(0, int.MaxValue);
                    foreach (Download download in downloads)
                    {
                        var downloadBinary = LoadDownloadBinary(download, oldValue);

                        //delete from Azure
                        if (oldValue != DOWNLOAD_STORE_TYPE_DATABASE)
                            Delete(download);
                        download.DownloadBinary = downloadBinary;
                        //just update a download (all required logic is in UpdateDownload method)
                        UpdateDownload(download);
                    }
                    
                }
            }
        }


        private byte[] LoadDownloadBinary(Download download, string oldValue)
        {
            var fileName = GetFileName(download);
            return oldValue == DOWNLOAD_STORE_TYPE_AZURE ? _blobSettingService.GetDownload(fileName) : download.DownloadBinary;
        }

        private IEnumerable GetDownloads(int pageIndex, int pageSize)
        {
            var query = from p in _downloadRepository.Table
                        orderby p.Id descending
                        select p;
            var files = new PagedList<Download>(query, pageIndex, pageSize);
            return files;
        }

        #endregion

        #region Utilities
        public static string GetFileName(Download download)
        {
            return download.DownloadGuid.ToString();
        }

        private void Insert(Download download, IRepository<Download> repo)
        {
            var stDownload = repo.GetByGuid(download.DownloadGuid);
            try
            {
                var fileName = GetFileName(download);
                string url = _blobSettingService.InsertDownload(fileName, download.ContentType,
                                              download.DownloadBinary);

                if (stDownload != null)
                {
                    stDownload.DownloadUrl = url;
                    repo.Update(stDownload); //save changes
                }
            }
            catch (Exception ex)
            {
                if (stDownload != null)
                {
                    stDownload.DownloadBinary = download.DownloadBinary;
                    repo.Update(stDownload); //save changes
                }
                throw ex;
            }
        }

        private  void Update(Download download, IRepository<Download> repo)
        {
            var stDownload = repo.GetByGuid(download.DownloadGuid);
            try
            {
                var fileName = GetFileName(download);
                string url = _blobSettingService.InsertDownload(fileName, download.ContentType, download.DownloadBinary);
                if (stDownload != null)
                {
                    stDownload.DownloadUrl = url;
                    repo.Update(stDownload); //save changes
                }
            }
            catch (Exception ex)
            {
                if (stDownload != null)
                {
                    stDownload.DownloadBinary = download.DownloadBinary;
                    repo.Update(stDownload); //save changes
                }
                throw ex;
            }
        }

        public void Delete(Download download)
        {
            var fileName = GetFileName(download);
            _blobSettingService.DeleteDownload(fileName);
        }

        #endregion

        #region Methods
        public override Download GetDownloadById(int downloadId)
        {
            var download = base.GetDownloadById(downloadId);
            if (StoreType == DOWNLOAD_STORE_TYPE_AZURE)
            {
                return GetDownload(download);
            }
            return download;
        }

        public Download GetDownload(Download download)
        {
            var fileName = GetFileName(download);
            try
            {
                return new Download
                {
                    DownloadBinary = _blobSettingService.GetDownload(fileName),
                    Filename = download.Filename,
                    UseDownloadUrl = false,
                    DownloadGuid = download.DownloadGuid,
                    DownloadUrl = "",
                    ContentType = download.ContentType,
                    Extension = download.Extension
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, null);
            }
            return null;
        }

        public override Download GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty) return null;
            var download = base.GetDownloadByGuid(downloadGuid);
            if (StoreType == DOWNLOAD_STORE_TYPE_AZURE)
            {
                return GetDownload(download);
            }
            return download;
        }

        public override void DeleteDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            if (StoreType == DOWNLOAD_STORE_TYPE_AZURE)
            {
                //ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        Delete(download);
                    }
                    catch (Exception ex)
                    {
                        var logger = EngineContext.Current.Resolve<ILogger>();
                        logger.Error(ex.Message, ex, null);
                    }
                }
            }
            base.DeleteDownload(download);
        }

        public override void InsertDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");
            if (StoreType == DOWNLOAD_STORE_TYPE_AZURE)
            {
                var tmp = new Download
                {
                    ContentType = download.ContentType,
                    DownloadBinary = download.DownloadBinary,
                    DownloadGuid = download.DownloadGuid,
                    DownloadUrl = download.DownloadUrl,
                    Extension = download.Extension,
                    Filename = download.Filename,
                    Id = download.Id,
                    IsNew = download.IsNew,
                    UseDownloadUrl = download.UseDownloadUrl
                };
                download.DownloadBinary = null;

                //в базу
                _downloadRepository.Insert(download);
                _eventPublisher.EntityInserted(download);

                {
                    //в блоб
                    try
                    {
                        Insert(tmp, _downloadRepository);
                    }
                    catch (Exception ex)
                    {
                        var logger = EngineContext.Current.Resolve<ILogger>();
                        logger.Error(ex.Message, ex, null);
                    }
                }
            }
            else
            {
                base.InsertDownload(download);
            }
        }

        public override void UpdateDownload(Download download)
        {
            if (download == null)
                throw new ArgumentNullException("download");

            if (StoreType == DOWNLOAD_STORE_TYPE_AZURE)
            {
                var tmp = new Download
                {
                    ContentType = download.ContentType,
                    DownloadBinary = download.DownloadBinary,
                    DownloadGuid = download.DownloadGuid,
                    DownloadUrl = download.DownloadUrl,
                    Extension = download.Extension,
                    Filename = download.Filename,
                    Id = download.Id,
                    IsNew = download.IsNew,
                    UseDownloadUrl = download.UseDownloadUrl
                };
                download.DownloadBinary = null;
                _downloadRepository.Update(download);
                _eventPublisher.EntityUpdated(download);

                //ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        Update(tmp, _downloadRepository);
                    }
                    catch (Exception ex)
                    {
                        var logger = EngineContext.Current.Resolve<ILogger>();
                        logger.Error(ex.Message, ex, null);
                    }
                }
                //);
            }
            else
            {
                base.UpdateDownload(download);
            }
        }
        #endregion
    }
}