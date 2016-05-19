using System;

namespace PlexRequests.Updater
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Starting PlexRequests .Net updater");
            var s = new Updater();
            s.Start(args[0]);
		}
	}
}
