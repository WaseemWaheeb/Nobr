using System.Data.Entity.ModelConfiguration;
using Nop.Plugin.Misc.AzureBlob.Domain;

namespace Nop.Plugin.Misc.AzureBlob.Data
{
    public class PictureFileMap : EntityTypeConfiguration<PictureFile>
    {
        public PictureFileMap()
        {
            this.ToTable("ITP_PictureFile");
            this.HasKey(m => m.Id);
            this.Property(m => m.PictureId).IsRequired();
            this.Property(m => m.FileName).IsRequired().HasMaxLength(300);
            this.Property(m => m.PictureURL).IsRequired();
        }
    }
}
