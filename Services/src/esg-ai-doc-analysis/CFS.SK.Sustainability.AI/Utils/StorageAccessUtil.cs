using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFS.SK.Sustainability.AI.Utils
{
    public class StorageAccessUtil
    {
        static public BlobServiceClient GetBlobClientFromConnectionString(string ConnectionString)
        {
            var DefaultEndpointSuffix = "core.windows.net";
            var storageAccountName = ConnectionString.Split(';').FirstOrDefault(x => x.Contains("AccountName")).Split('=')[1];
            var storageAccountUri = new Uri($"https://{storageAccountName}.blob.{DefaultEndpointSuffix}");
            DefaultAzureCredential credential = new(DefaultAzureCredential.DefaultEnvironmentVariableName); // CodeQL [SM05137] Environment variable is set in Docker File
            return new BlobServiceClient(storageAccountUri, credential);
        }

        static public Stream GetReportResultBlob(string FileName, string ConnectionString)
        {
            var storageBlobClient = StorageAccessUtil.GetBlobClientFromConnectionString(ConnectionString);

            //Get StorageBlob in "results" container then return as a Stream
            var containerClient = storageBlobClient.GetBlobContainerClient("results");
            var blobClient = containerClient.GetBlobClient(FileName);

            //Copy the blob to a memory stream
            var memoryStream = new MemoryStream();
            blobClient.DownloadTo(memoryStream);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
