using System;
using System.Drawing;
using System.IO;
using ImageResizer;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.AzureBlob.Services
{
    public class StorePictureService : PictureService
    {

        #region Const
        private static readonly object s_lock2 = new object();
        public const string PICTURE_STORE_TYPE_AZURE = "Azure";
        public const string PICTURE_STORE_TYPE_DATABASE = "Database";
        public const string PICTURE_STORE_TYPE_FILESYSTEM = "FileSystem";

        #endregion

        #region Fileds
        private readonly BlobService _storage;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IWebHelper _webHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService2;
        private readonly IPictureFileService _pictureFileService;
        #endregion

        #region Ctor

        public StorePictureService(IRepository<Picture> pictureRepository,
           IRepository<ProductPicture> productPictureRepository,
           ISettingService settingService,
           IWebHelper webHelper,
           ILogger logger,
           IDbContext dbContext,
           IEventPublisher eventPublisher,
           MediaSettings mediaSettings,
           IPictureFileService pictureFileService,
           BlobService blobSettingService)
            : base(pictureRepository,
             productPictureRepository,
            settingService, webHelper,
           logger, dbContext, eventPublisher,
            mediaSettings)

        {
            _webHelper = webHelper;
            _settingService2 = settingService;
            _pictureRepository = pictureRepository;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _logger = logger;
            _storage = blobSettingService;
            _pictureFileService = pictureFileService;
        }
        #endregion

        #region Utilities

        public string GetFileName(int id, string mimeType)
        {
            var lastPart = GetFileExtensionFromMimeType(mimeType);
            return string.Format("{0}_0.{1}", id.ToString("000000000"), lastPart);
        }
        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        public override byte[] LoadPictureBinary(Picture picture)
        {
            return LoadPictureBinary(picture, StoreInDb);
        }
        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        protected override byte[] LoadPictureBinary(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var result = fromDb
                ? picture.PictureBinary
                : StoreInProvider? LoadPictureFromStorage(picture.Id, picture.MimeType) : LoadPictureFromFile(picture.Id, picture.MimeType);
            return result;
        }

        private byte[] LoadPictureFromStorage(int id, string mimeType)
        {
            try
            {
                var fileName = GetFileName(id, mimeType);
                return _storage.GetPicture(fileName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            return null;
        }

        #endregion

        #region Base methods
        public void SavePictureInFileSystem(int pictureId, byte[] pictureBinary, string mimeType)
        {
            var fileName = GetFileName(pictureId, mimeType);
            File.WriteAllBytes(GetPictureLocalPath(fileName), pictureBinary);
        }

        public void DeletePictureFromFileSystem(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var fileName = GetFileName(picture.Id, picture.MimeType);
            string filePath = GetPictureLocalPath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public Picture InsertPictureBase(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            var picture = new Picture
            {
                PictureBinary = pictureBinary,
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew,
            };
            _pictureRepository.Insert(picture);

            //event notification
            _eventPublisher.EntityInserted(picture);

            return picture;

        }


        public Picture UpdatePictureBase(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);



            var picture = GetPictureById(pictureId);
            if (picture == null)
                return null;

            //delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
                DeletePictureThumbs(picture);

            picture.PictureBinary = pictureBinary;
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;

            _pictureRepository.Update(picture);

            //event notification
            _eventPublisher.EntityUpdated(picture);

            return picture;

        }

        public override Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)
        {
            if (StoreInProvider)
            {
                /*try
                {*/
                    var picture = InsertPictureBase(null, mimeType, seoFilename, altAttribute, titleAttribute, isNew, validateBinary);
                    var fileName = GetFileName(picture.Id, mimeType);
                    if (validateBinary)
                        pictureBinary = ValidatePicture(pictureBinary, mimeType);
                    string url = _storage.InsertPicture(fileName, mimeType, pictureBinary);
                    _pictureFileService.ClearCache(picture.Id);
                    return picture;
                /*}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                }*/
            }
            return base.InsertPicture(pictureBinary, mimeType, seoFilename, altAttribute, titleAttribute, isNew, validateBinary);
        }

        public override Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null,
            bool isNew = true, bool validateBinary = true)

        {
            if (StoreInProvider)
            {
                /*try
                {*/
                    var fileName = GetFileName(pictureId, mimeType);
                    if (pictureBinary != null)
                    {
                        var url = _storage.InsertPicture(fileName, mimeType, pictureBinary);
                        _pictureFileService.DeletePictureFile(pictureId);
                        _pictureFileService.ClearCache(pictureId);
                    }
                    return UpdatePictureBase(pictureId, null, mimeType, seoFilename, altAttribute, titleAttribute, isNew, false);
                
                /*}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                }*/
            }
            return base.UpdatePicture(pictureId, pictureBinary, mimeType, seoFilename, altAttribute, titleAttribute, isNew, validateBinary);
        }

        public override void DeletePicture(Picture picture)
        {
            if (StoreInProvider)
            {
                /*try
                {*/
                    var fileName = GetFileName(picture.Id, picture.MimeType);
                    _storage.DeletePicture(fileName);
                    _pictureFileService.DeletePictureFiles(picture.Id);
                    base.DeletePicture(picture);
                /*}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                }*/
            }
            else
            {
                base.DeletePicture(picture);
            }
        }
        #endregion

        #region URL

        /// <summary>
        /// Gets the default picture URL
        /// </summary>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Picture URL</returns>
        public override string GetDefaultPictureUrl(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null)
        {
            if (StoreInProvider)
            {
                try
                {
                    string defaultImageFileName;

                    switch (defaultPictureType)
                    {
                        case PictureType.Entity:
                            defaultImageFileName = _settingService2.GetSettingByKey("Media.DefaultImageName", "default-image.gif");
                            break;
                        case PictureType.Avatar:
                            defaultImageFileName = _settingService2.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
                            break;
                        default:
                            defaultImageFileName = _settingService2.GetSettingByKey("Media.DefaultImageName", "default-image.gif");
                            break;
                    }
                    var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
                    if (targetSize == 0 || azureBlobSetting.AlwaysShowMainImage)
                    {
                        return GetThumbUrl(defaultImageFileName, storeLocation);
                    }
                    string fileExtension = Path.GetExtension(defaultImageFileName);
                    string thumbFileName = string.Format("{0}_{1}{2}",
                        Path.GetFileNameWithoutExtension(defaultImageFileName),
                        targetSize,
                        fileExtension);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName);
                    if (!_pictureFileService.IsExist(thumbFilePath))
                    {
                        var pictureBinary = _storage.GetPicture(defaultImageFileName);
                        using (var stream = new MemoryStream(pictureBinary))
                        {
                            Bitmap b = null;
                            try
                            {
                                //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                b = new Bitmap(stream);
                            }
                            catch (ArgumentException exc)
                            {
                                _logger.Error(string.Format("Error generating picture thumb. filename={0}", thumbFilePath), exc);
                            }

                            var newSize = CalculateDimensions(b.Size, targetSize);

                            var destStream = new MemoryStream();
                            ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                            {
                                Width = newSize.Width,
                                Height = newSize.Height,
                                Scale = ScaleMode.Both,
                                Quality = _mediaSettings.DefaultImageQuality
                            });
                            var destBinary = destStream.ToArray();
                            var thumbUrl = _storage.InsertPicture(thumbFileName, GetMimeFromExtension(fileExtension), destBinary);
                            b.Dispose();
                        }
                    }
                    var url = GetThumbUrl(thumbFileName, storeLocation);
                    return url;
                }
                catch (Exception ex)
                {
                    //_logger.Error(ex.Message, ex, null);
                    //_logger.Information("GetDefaultPictureUrl " + targetSize);
                    return "";
                }
            }
            return base.GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
        }


        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="pictureId">picture id</param>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbUrl(int pictureId, string thumbFileName, string storeLocation = null)
        {
            if (StoreInProvider)
            {
                /*try
                {*/
                var uri = _pictureFileService.GetUrlByUniqName(pictureId, thumbFileName);

                return _storage.GetUrlByUniqName(uri);
                /*}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                }*/
            }
            return base.GetThumbUrl(thumbFileName, storeLocation);
        }
        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="pictureId">picture id</param>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            if (StoreInProvider)
            {
                /*try
                {*/
                var uri = _pictureFileService.GetUrlByUniqName(thumbFileName);

                return _storage.GetUrlByUniqName(uri);
                /*}
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                }*/
            }
            return base.GetThumbUrl(thumbFileName, storeLocation);
        }

        public override string GetPictureUrl(Picture picture,
               int targetSize = 0,
               bool showDefaultPicture = true,
               string storeLocation = null,
               PictureType defaultPictureType = PictureType.Entity)
        {
            if (StoreInProvider)
            {
                try
                {
                
                    string url = string.Empty;
                    byte[] pictureBinary = null;
                    var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
                    if (picture != null && azureBlobSetting.AlwaysShowMainImage)
                    {
                        var fileName = GetFileName(picture.Id, picture.MimeType);
                        return GetThumbUrl(fileName, storeLocation);
                    }
                    if (picture == null || !_pictureFileService.IsExist(picture.Id, GetFileName(picture.Id, picture.MimeType)))
                    {
                        if (showDefaultPicture)
                        {
                            url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                        }
                        //if(picture!=null) _logger.Information("GetPictureUrl " + targetSize + " " + picture.Id + " " + picture.SeoFilename);
                        return url;
                    }

                    string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
                    string thumbFileName;
                    if (picture.IsNew)
                    {
                        pictureBinary = LoadPictureBinary(picture);
                        DeletePictureThumbs(picture);

                        //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                        UpdatePicture(picture.Id,
                            pictureBinary, picture.MimeType, picture.SeoFilename, picture.AltAttribute, picture.TitleAttribute,
                            false, false);
                    }
                    lock (s_lock2)
                    {
                        string seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure
                        if (targetSize == 0)
                        {
                            thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                                string.Format("{0}_{1}.{2}", picture.Id.ToString("000000000"), seoFileName, lastPart) :
                                string.Format("{0}.{1}", picture.Id.ToString("000000000"), lastPart);
                            if (!_pictureFileService.IsExist(picture.Id, thumbFileName))
                            {
                                if (pictureBinary == null || pictureBinary.Length == 0)
                                    pictureBinary = LoadPictureBinary(picture);
                                var thumbUrl = _storage.InsertPicture(thumbFileName, picture.MimeType, pictureBinary);
                                _pictureFileService.ClearCache(picture.Id);
                            }
                        }
                        else
                        {
                            thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                                string.Format("{0}_{1}_{2}.{3}", picture.Id.ToString("000000000"), seoFileName, targetSize, lastPart) :
                                string.Format("{0}_{1}.{2}", picture.Id.ToString("000000000"), targetSize, lastPart);

                            if (!_pictureFileService.IsExist(picture.Id, thumbFileName))
                            {
                                if (pictureBinary == null || pictureBinary.Length == 0)
                                    pictureBinary = LoadPictureBinary(picture);
                                using (var stream = new MemoryStream(pictureBinary))
                                {
                                    Bitmap b = null;
                                    try
                                    {
                                        //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                        b = new Bitmap(stream);
                                    }
                                    catch (ArgumentException exc)
                                    {
                                        _logger.Error(string.Format("Error generating picture thumb. ID={0}", picture.Id), exc);
                                    }
                                    if (b == null)
                                    {
                                        //bitmap could not be loaded for some reasons
                                        return url;
                                    }

                                    var newSize = CalculateDimensions(b.Size, targetSize);

                                    var destStream = new MemoryStream();
                                    ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                                    {
                                        Width = newSize.Width,
                                        Height = newSize.Height,
                                        Scale = ScaleMode.Both,
                                        Quality = _mediaSettings.DefaultImageQuality
                                    });
                                    var destBinary = destStream.ToArray();
                                    var thumbFileUrl = _storage.InsertPicture(thumbFileName, picture.MimeType, destBinary);
                                    _pictureFileService.ClearCache(picture.Id);
                                    b.Dispose();
                                }
                            }
                        }
                    }
                    url = GetThumbUrl(picture.Id, thumbFileName, storeLocation);
                    return url;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex, null);
                    string url = string.Empty;
                    //if (showDefaultPicture)
                    //{
                    //    url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                    //}
                    //if (picture != null) _logger.Information("GetPictureUrl " + targetSize + " " + picture.Id + " " + picture.SeoFilename);
                    return url;
                }
            }
            return base.GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }
        
        #endregion

        #region Properties

        public virtual bool StoreInProvider
        {
            get
            {
                var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
                return azureBlobSetting.PictureStoreType != PICTURE_STORE_TYPE_DATABASE &&
                       azureBlobSetting.PictureStoreType != PICTURE_STORE_TYPE_FILESYSTEM;
            }
        }

        public virtual string StoreType
        {
            get
            {
                var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
                return azureBlobSetting.PictureStoreType;
            }
            set
            {
                //check whether it's a new value
                if (StoreType != value)
                {
                    var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
                    var oldValue = azureBlobSetting.PictureStoreType;
                    if (String.IsNullOrEmpty(oldValue))
                        oldValue = StoreInDb ? PICTURE_STORE_TYPE_DATABASE : PICTURE_STORE_TYPE_FILESYSTEM;
                    azureBlobSetting.PictureStoreType = value;
                    _settingService2.SaveSetting(azureBlobSetting);

                    _settingService2.SetSetting("Media.Images.StoreInDB", value == PICTURE_STORE_TYPE_DATABASE);

                    //update all picture objects
                    var pictures = GetPictures(0, int.MaxValue);
                    foreach (var picture in pictures)
                    {
                        var pictureBinary = LoadPictureBinary(picture, oldValue);

                        //delete from file system
                        if (oldValue != PICTURE_STORE_TYPE_DATABASE)
                            DeletePictureOnFileSystem(picture);

                        if (oldValue != PICTURE_STORE_TYPE_DATABASE && oldValue != PICTURE_STORE_TYPE_FILESYSTEM)
                            DeletePictureOnStorage(picture);

                        //just update a picture (all required logic is in UpdatePicture method)
                        UpdatePicture(picture.Id,
                            pictureBinary, picture.MimeType, picture.SeoFilename, picture.AltAttribute, picture.TitleAttribute, true, false);
                        //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown when "moving" pictures
                    }

                    if (value != PICTURE_STORE_TYPE_DATABASE && value != PICTURE_STORE_TYPE_FILESYSTEM)
                    {
                        CopyDefaultFiles();
                    }
                }

            }
        }

        private byte[] LoadPictureBinary(Picture picture, string pictureStoreType)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            var result = pictureStoreType == PICTURE_STORE_TYPE_DATABASE
                ? picture.PictureBinary
                : pictureStoreType != PICTURE_STORE_TYPE_DATABASE &&
                   pictureStoreType != PICTURE_STORE_TYPE_FILESYSTEM ? 
                   LoadPictureFromStorage(picture.Id, picture.MimeType) : LoadPictureFromFile(picture.Id, picture.MimeType);
            return result;
        }

        private void DeletePictureOnStorage(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");
            var pictures = _pictureFileService.GetPictureFilesByPictureId(picture.Id);
            foreach (var pictureFile in pictures)
            {
                _pictureFileService.DeletePictureFile(pictureFile.Id);
                _storage.DeletePicture(pictureFile.FileName);
            }

        }

        /// <summary>
        /// Gets or sets a value indicating whether the images should be stored in data base.
        /// </summary>
        public override bool StoreInDb
        {
            get
            {
                return _settingService2.GetSettingByKey("Media.Images.StoreInDB", true);
            }
            set
            {
                //check whether it's a new value
                if (StoreInDb != value)
                {
                    //save the new setting value
                    _settingService2.SetSetting("Media.Images.StoreInDB", value);

                    StoreType = value ? PICTURE_STORE_TYPE_DATABASE : PICTURE_STORE_TYPE_FILESYSTEM;
                }
            }
        }
        public void CopyDefaultFiles()
        {
            var azureBlobSetting = _settingService2.LoadSetting<AzureBlobSetting>();
            azureBlobSetting.DefaultImageName = CopyDefaultImage("Media.DefaultImageName", "default-image.gif");
            azureBlobSetting.DefaultAvatarImageName = CopyDefaultImage("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
            _settingService2.SaveSetting(azureBlobSetting);
        }

        private string CopyDefaultImage(string imageKey, string imageDefaultValue)
        {
            string defaultImageFileName;
            defaultImageFileName = _settingService2.GetSettingByKey(imageKey, imageDefaultValue);

            string filePath = GetPictureLocalPath(defaultImageFileName);

            if (!File.Exists(filePath))
            {
                return "" ;
            }
            var ext = Path.GetExtension(filePath);
            var pictureBinary = File.ReadAllBytes(filePath);
            var mimeType = GetMimeFromExtension(ext);

            string url = _storage.InsertPicture(imageDefaultValue, mimeType, pictureBinary);
            return url;
           
        }

        public string GetUrlByUniqName(int id, string fileName)
        {
            var uri = _pictureFileService.GetUrlByUniqName(id, fileName);

            return _storage.GetUrlByUniqName(uri);
        }

        protected virtual string GetMimeFromExtension(string fileExt)
        {
            fileExt = fileExt.TrimStart(".".ToCharArray()).ToLower().Trim();
            switch (fileExt)
            {
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                default:
                    return "image/jpeg";
            }
        }
        #endregion
    }

}
