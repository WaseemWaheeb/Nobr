using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.AzureBlob.Services
{
    public static class AzureConnectionStringFactory
    {
        public static string Create(string storageAccountName, string storageAccountKey, bool isDevAccount)
        {
            if (isDevAccount)
            {
                return string.Format(@"DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1};BlobEndpoint=http://127.0.0.1:10000/{0}/;TableEndpoint=http://127.0.0.1:10002/{0}/;QueueEndpoint=http://127.0.0.1:10001/{0}/",
                    storageAccountName,
                    storageAccountKey);
            }
            else
            {
                return string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", 
                    storageAccountName, 
                    storageAccountKey);
            }
        }
    }
}
