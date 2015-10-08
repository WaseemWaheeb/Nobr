using Nop.Plugin.Misc.AzureBlob.Domain;

namespace Nop.Plugin.Misc.AzureBlob.Services
{
    public interface IPictureFileService
    {

        /// <summary>
        /// Gets a pictureFile
        /// </summary>
        /// <param name="pictureFileId">pictureFile identifier</param>
        /// <returns>pictureFile</returns>
        PictureFile GetPictureFileById(int pictureFileId);

        /// <summary>
        /// Gets a pictureFile
        /// </summary>
        /// <param name="pictureId">picture identifier</param>
        /// <returns>pictureFile</returns>s
        PictureFile[] GetPictureFilesByPictureId(int pictureId);

        /// <summary>
        /// Clear Cache
        /// </summary>
        /// <param name="pictureId">pictureId</param>
        void ClearCache(int  pictureId);

        /// <summary>
        /// Updates the pictureFile
        /// </summary>
        /// <param name="pictureFile">pictureFile</param>
        void UpdatePictureFile(PictureFile pictureFile);

        /// <summary>
        /// Deletes a picture files
        /// </summary>
        void DeletePictureFiles(int pictureId);

        /// <summary>
        /// Check if image exist
        /// </summary>
        bool IsExist(int pictureId, string fileName);
        /// <summary>
        /// Check if image exist
        /// </summary>
        bool IsExist(string fileName);
        /// <summary>
        /// Get URL
        /// </summary>
        string GetUrlByUniqName(int pictureId, string fileName);
        /// <summary>
        /// Get URL
        /// </summary>
        string GetUrlByUniqName(string fileName);
        /// <summary>
        /// Deletes a picture file
        /// </summary>
        void DeletePictureFile(int id);
    }
}
