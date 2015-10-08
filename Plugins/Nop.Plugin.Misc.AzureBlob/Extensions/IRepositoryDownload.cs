using System;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Media;

namespace Nop.Plugin.Misc.AzureBlob.Extensions
{
    public static class IRepositoryDownloadExtension
    {
        public static Download GetByGuid(this IRepository<Download> stdownloadRepository, Guid guid)
        {
            var query = from o in stdownloadRepository.Table
                        where o.DownloadGuid == guid
                        select o;
            return query.FirstOrDefault();
        }
    }
}
