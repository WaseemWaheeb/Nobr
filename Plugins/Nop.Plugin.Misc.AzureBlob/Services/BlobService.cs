using System;
using System.IO;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.AzureBlob.Services
{
    public class BlobService
    {
        #region Fields
        private AzureBlobSetting _azureBlobSetting;
        private readonly BlobMethods _blobMethodsPictures;
        private readonly BlobMethods _blobMethodsDownload;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public BlobService(AzureBlobSetting azureBlobSetting, ILocalizationService localizationService)
        {
            _azureBlobSetting = azureBlobSetting;
            _blobMethodsPictures = new BlobMethods();
            _blobMethodsDownload = new BlobMethods();
            _localizationService = localizationService;
            ValidatePictureSettings();
            ValidateDownloadSettings();
        }

        public string ValidatePictureSettings()
        {

            _azureBlobSetting = EngineContext.Current.Resolve<ISettingService>().LoadSetting<AzureBlobSetting>();

            if (!String.IsNullOrEmpty(_azureBlobSetting.Account) && !String.IsNullOrEmpty(_azureBlobSetting.AccountKey) &&
                !String.IsNullOrEmpty(_azureBlobSetting.ContainerForPictures))
            {
                try
                {
                    _blobMethodsPictures.RunAtAppStartup(_azureBlobSetting.Account, _azureBlobSetting.AccountKey,
                        _azureBlobSetting.ContainerForPictures, _azureBlobSetting.UseDevAccount);
                    return null;
                }
                catch (Exception ex)
                {
                    return _azureBlobSetting.ContainerForPictures + " " + ex.Message;
                }
            }
            return _localizationService.GetResource("Admin.Configuration.ValidatePictureSettings");
        }

        public string ValidateDownloadSettings()
        {
            _azureBlobSetting = EngineContext.Current.Resolve<ISettingService>().LoadSetting<AzureBlobSetting>();

            if (!String.IsNullOrEmpty(_azureBlobSetting.Account) && !String.IsNullOrEmpty(_azureBlobSetting.AccountKey) &&
                !String.IsNullOrEmpty(_azureBlobSetting.Container))
            {
                try
                {
                    _blobMethodsDownload.RunAtAppStartup(_azureBlobSetting.Account, _azureBlobSetting.AccountKey,
                        _azureBlobSetting.Container, _azureBlobSetting.UseDevAccount);
                    return null;
                }
                catch (Exception ex)
                {
                    return _azureBlobSetting.Container + " " + ex.Message;
                }
            }
            return _localizationService.GetResource("Admin.Configuration.ValidateDownloadSettings");
        }

        #endregion

        #region Methods

        public string GetPictureUrlByUniqName(string targetBlobName)
        {
            return _blobMethodsPictures.GetUrlByUniqName(targetBlobName);
        }

        public string GetDownloadUrlByUniqName(string targetBlobName)
        {
            return _blobMethodsDownload.GetUrlByUniqName(targetBlobName);
        }
        /// <summary>
        /// Insert picture to blob
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        public string InsertPicture(string fileName, string contentType, byte[] binary)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(binary, 0, binary.Length);
            return _blobMethodsPictures.UploadFromStream(stream, fileName);
        }

        /// <summary>
        /// Insert Download to blob
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="contentType">Content Type</param>
        /// <param name="binary">Binary</param>
        public string InsertDownload(string fileName, string contentType, byte[] binary)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(binary, 0, binary.Length);
            return _blobMethodsDownload.UploadFromStream(stream, fileName);
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
        /// <summary>
        /// Get Picture 
        /// </summary>
        /// <param name="fileName">File Name</param>
        public byte[] GetPicture(string fileName)
        {
            MemoryStream stream = new MemoryStream();
            _blobMethodsPictures.DownloadToStream(fileName, stream);
            return ReadToEnd(stream);
        }
        /// <summary>
        /// Get Download 
        /// </summary>
        /// <param name="fileName">File Name</param>
        public byte[] GetDownload(string fileName)
        {
            MemoryStream stream = new MemoryStream();
            _blobMethodsDownload.DownloadToStream(fileName, stream);
            return ReadToEnd(stream); 
        }

        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="fileName">Blob ID</param>
        public void DeletePicture(string fileName)
        {
            _blobMethodsPictures.DeleteBlob(fileName);
        }

        /// <summary>
        /// Delete file from blob
        /// </summary>
        /// <param name="fileName">Blob ID</param>
        public void DeleteDownload(string fileName)
        {
            _blobMethodsDownload.DeleteBlob(fileName);
        }

        public string GetUrlByUniqName(string uri)
        {
            if (!String.IsNullOrEmpty(uri))
            {
                return _azureBlobSetting.UseCDN ?
                    uri.Replace(".blob.core.windows.net", ".vo.msecnd.net").Replace("https://", "http://").Replace(_azureBlobSetting.Account, _azureBlobSetting.CDN) : uri;
            }
            return String.Empty;
        }

        public string GeneratePictureUrlByUniqName(string fileName)
        {
            return String.Format("https://{0}.blob.core.windows.net/{1}/{2}", _azureBlobSetting.Account,
                _azureBlobSetting.ContainerForPictures, fileName);
        }
        #endregion


    }
}

