using System;

namespace PlexRequests.Updater
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Starting PlexRequests .Net updater");
            var s = new Updater();
		    if (args.Length >= 2)
		    {
		        s.Start(args[0], args[1]);
		    }
		    else
		    {
		        s.Start(args[0], string.Empty);
		    }
		}
	}
}
