using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LsmUpdater
{
	public class BlobClient
	{
		public CloudStorageAccount Account { get; protected set; }
		public CloudBlobClient Client { get; protected set; }
		public CloudBlobContainer Container { get; protected set; }

		// Most methods throw if problems

		public BlobClient( string connectionName )
		{
			// throws if problems...
			Account = CloudStorageAccount.Parse( connectionName );
			Client = Account.CreateCloudBlobClient();
		}

		public BlobClient( string connectionName, string containerName )
			: this( connectionName )
		{
			Container = GetContainer( containerName );
		}

		public CloudBlobContainer GetContainer( string name )
		{
			return GetContainerAsync( name ).Result;
		}

		public async Task<CloudBlobContainer> GetContainerAsync( string name )
		{
			var container = Client.GetContainerReference( name );
			await container.CreateAsync();

			// Set the permissions so the blobs are public. 
			BlobContainerPermissions permissions = new BlobContainerPermissions
			{
				PublicAccess = BlobContainerPublicAccessType.Blob
			};

			await container.SetPermissionsAsync( permissions );

			return container;
		}

		public List<IListBlobItem> GetContainerList( CloudBlobContainer container )
		{
			return GetContainerListAsync( container ).Result;
		}

			public async Task<List<IListBlobItem>> GetContainerListAsync( CloudBlobContainer container )
		{
			List<IListBlobItem> items = new List<IListBlobItem>();

			BlobContinuationToken blobContinuationToken = null;
			do
			{
				var results = await container.ListBlobsSegmentedAsync( null, blobContinuationToken );

				// Get the value of the continuation token returned by the listing call.
				blobContinuationToken = results.ContinuationToken;
				foreach( IListBlobItem item in results.Results )
				{
					items.Add( item );
				}
			} while( blobContinuationToken != null ); // Loop while the continuation token is not null.

			return items;
		}

		public async void UploadBlob( CloudBlobContainer cloudBlobContainer, string sourceFile, string localFileName )
		{
			// Get a reference to the blob address, then upload the file to the blob.
			// Use the value of localFileName for the blob name.
			CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference( localFileName );
			await cloudBlockBlob.UploadFromFileAsync( sourceFile );
		}

		public async void DownloadBlob( CloudBlockBlob cloudBlockBlob, string sourceFile, string destinationFile )
		{
			// Download the blob to a local file, using the reference created earlier. 
			// Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
			destinationFile = sourceFile.Replace( ".txt", "_DOWNLOADED.txt" );
			Console.WriteLine( "Downloading blob to {0}", destinationFile );
			Console.WriteLine();
			await cloudBlockBlob.DownloadToFileAsync( destinationFile, FileMode.Create );

		}

	}
}
