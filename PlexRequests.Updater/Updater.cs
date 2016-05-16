using System;
using PlexRequests.Core;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace PlexRequests.Updater
{
	public class Updater
	{
		public void Start(){
			var c = new StatusChecker ();

			try {

				var release = c.GetStatus ();
				if(!release.UpdateAvailable)
				{
					Console.WriteLine ("No Update availble, shutting down");
				}

				using(var client = new WebClient())
				using(var ms = new MemoryStream(client.DownloadData(release.DownloadUri), false))
				using(var gz = new GZipStream(ms, CompressionLevel.Optimal))
				{
					// TODO decompress stream
				}


			} catch (Exception ex) {
				
				Console.WriteLine (ex.Message);
				Console.WriteLine ("Oops... Looks like we cannot update!");
				Console.ReadLine ();
			}
		}

		
		
	}
}

