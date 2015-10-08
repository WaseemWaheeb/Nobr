using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.AzureBlob.Installation;

namespace Nop.Plugin.Misc.AzureBlob.Data
{
    public class PictureFileObjectContext : DbContext, IDbContext
    {
        public PictureFileObjectContext(string nameOrConnectionString) : base(nameOrConnectionString) { }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PictureFileMap());
            base.OnModelCreating(modelBuilder);
        }

        public string CreateDatabaseInstallationScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public void Install()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<PictureFileObjectContext>(null);
            var installation = EngineContext.Current.Resolve<AzureBlobInstallationService>();
            installation.InstallData();
        }

        public void Uninstall()
        {
            Database.ExecuteSqlCommand("DROP TABLE [ITP_PictureFile]");
            SaveChanges();
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public System.Collections.Generic.IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Detach(object entity)
        {
            throw new NotImplementedException();
        }

        public bool ProxyCreationEnabled
        {
            get;
            set;
        }

        public bool AutoDetectChangesEnabled
        {
            get;
            set;
        }
    }
}
