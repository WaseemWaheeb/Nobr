using Nop.Core;

namespace Nop.Plugin.Misc.AzureBlob.Domain
{
    public class PictureFile :  BaseEntity 
    {
        /// <summary>
        /// Gets or sets the PictureId
        /// </summary>
        public virtual int PictureId { get; set; }
        /// <summary>
        /// Gets or sets the File Name
        /// </summary>
        public virtual string FileName { get; set; }
        /// <summary>
        /// Gets or sets the  PictureURL
        /// </summary>
        public virtual string PictureURL { get; set; }
    }
}
