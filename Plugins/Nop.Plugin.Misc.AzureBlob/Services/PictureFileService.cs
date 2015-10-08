using System;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Misc.AzureBlob.Domain;
using Nop.Services.Events;
using System.Linq;
using Nop.Core.Domain.Media;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.AzureBlob.Services
{


    public class PictureFileService : IPictureFileService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture file ID
        /// </remarks>
        private const string PICTUREFILE_BY_PICTUREFILEID_KEY = "Nop.picturefile.allbypicturefileid-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : file name
        /// </remarks>
        private const string PICTUREFILE_BY_FILENAME_KEY = "Nop.picturefile.allbyfilename-{0}";
        
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture ID
        /// </remarks>
        private const string PICTUREFILE_BY_PICTUREID_KEY = "Nop.picturefile.allbypictureid-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : picture file ID
        /// {1} : picture file name
        /// </remarks>
        private const string PICTUREFILE_BY_PICTUREID_FILENAME_KEY = "Nop.picturefile.allbypictureid-{0}-filename-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PICTUREFILE_PATTERN_KEY = "Nop.picturefile.";

        #endregion

        #region Fields
        private readonly IRepository<PictureFile> _pictureFileRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private BlobService _blobSettingService;
        private ISettingService _settingService;
        #endregion

        #region Ctor
        public PictureFileService(ICacheManager cacheManager, IRepository<PictureFile> pictureFileRepository, IEventPublisher eventPublisher, BlobService blobSettingService, ISettingService settingService)
        {
            _pictureFileRepository = pictureFileRepository;
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
            _blobSettingService = blobSettingService;
            _settingService = settingService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Deletes a picture files
        /// </summary>
        public virtual void DeletePictureFile(int id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            var query = from pfr in _pictureFileRepository.Table
                        where pfr.Id == id
                        select pfr;
            var pf = query.FirstOrDefault();
            if (pf != null)
            {
                _pictureFileRepository.Delete(pf);
                //event notification
                _eventPublisher.EntityDeleted(pf);
                //cache
                _cacheManager.RemoveByPattern(String.Format(PICTUREFILE_BY_PICTUREID_KEY, pf.PictureId));
            }
        }

        /// <summary>
        /// Deletes a picture files
        /// </summary>
        public virtual void DeletePictureFiles(int pictureId)
        {
            if (pictureId == null)
                throw new ArgumentNullException("pictureId");
            var query = from pfr in _pictureFileRepository.Table
                        where pfr.PictureId == pictureId
                        select pfr;
            foreach (var pf in query.ToList())
            {
                _pictureFileRepository.Delete(pf);
                //event notification
                _eventPublisher.EntityDeleted(pf);

            }
            //cache
            _cacheManager.RemoveByPattern(String.Format(PICTUREFILE_BY_PICTUREID_KEY, pictureId));
        }

        public bool IsExist(int pictureId, string fileName)
        {
            return !String.IsNullOrEmpty(GetUrlByUniqName(pictureId, fileName));
        }

        public string GetUrlByUniqName(int pictureId, string fileName)
        {
            //cache
            string key = string.Format(PICTUREFILE_BY_PICTUREID_FILENAME_KEY, pictureId, fileName);
            return _cacheManager.Get(key, () =>
            {
                var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
                return azureBlobSetting.CheckIfImageExist ? _blobSettingService.GetPictureUrlByUniqName(fileName) : _blobSettingService.GeneratePictureUrlByUniqName(fileName);
            });
        }

        public bool IsExist(string fileName)
        {
            return !String.IsNullOrEmpty(GetUrlByUniqName( fileName));
        }

        public string GetUrlByUniqName(string fileName)
        {
            //cache
            string key = string.Format(PICTUREFILE_BY_FILENAME_KEY, fileName);
            return _cacheManager.Get(key, () =>
            {
                var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
                return azureBlobSetting.CheckIfImageExist ? _blobSettingService.GetPictureUrlByUniqName(fileName) : _blobSettingService.GeneratePictureUrlByUniqName(fileName);
            });
        }
        /// <summary>
        /// Gets a pictureFile
        /// </summary>
        /// <param name="pictureFileId">pictureFile identifier</param>
        /// <returns>pictureFile</returns>
        public virtual PictureFile GetPictureFileById(int pictureFileId)
        {
            if (pictureFileId == 0)
                return null;
            //cache
            string key = string.Format(PICTUREFILE_BY_PICTUREFILEID_KEY, pictureFileId);
            return _cacheManager.Get(key, () =>
            {
                var pictureFile = _pictureFileRepository.GetById(pictureFileId);
                return pictureFile;
            });
        }

        /// <summary>
        /// Gets a pictureFiles
        /// </summary>
        /// <param name="pictureId">picture identifier</param>
        /// <returns>pictureFile</returns>
        public virtual PictureFile[] GetPictureFilesByPictureId(int pictureId)
        {
            if (pictureId == 0)
                return null;
            var queryPF = from pfr in _pictureFileRepository.Table
                          where pfr.PictureId == pictureId
                          select pfr; ;
            return queryPF.ToArray();
        }

        /// <summary>
        /// Inserts a pictureFile
        /// </summary>
        /// <param name="pictureId">picture Id</param>
        public virtual void ClearCache(int pictureId)
        {

            //cache
            _cacheManager.RemoveByPattern(String.Format(PICTUREFILE_BY_PICTUREID_KEY, pictureId));

        }

        /// <summary>
        /// Updates the pictureFile
        /// </summary>
        /// <param name="pictureFile">pictureFile</param>
        public virtual void UpdatePictureFile(PictureFile pictureFile)
        {
            if (pictureFile == null)
                throw new ArgumentNullException("pictureFile");
            _pictureFileRepository.Update(pictureFile);

            //cache
            _cacheManager.RemoveByPattern(String.Format(PICTUREFILE_BY_PICTUREID_KEY, pictureFile.PictureId));

            //event notification
            _eventPublisher.EntityUpdated(pictureFile);
        }

        #endregion
    }
}
