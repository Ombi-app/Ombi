using System;
using System.Linq;
using System.Windows.Forms;
using Ombi.Common.Processes;

namespace Ombi.Updater
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    var i = new InstallService();
		    var context = ParseArgs(args);
		    i.Start(context);
		    //Console.WriteLine ("Starting Ombi updater");
		    //         var s = new Updater();
		    //   if (args.Length >= 2)
		    //   {
		    //       s.Start(args[0], args[1]);
		    //   }
		    //   else
		    //   {
		    //       s.Start(args[0], string.Empty);
		    //   }
		}

	    private static UpdateStartupContext ParseArgs(string[] args)
	    {

            var proc = new ProcessProvider();
            var ombiProc = proc.FindProcessByName("Ombi").FirstOrDefault().Id;
            return new UpdateStartupContext
	        {
	            DownloadPath = args[0],
                ProcessId = ombiProc,
                StartupArgs = args.Length > 1 ? args[1] : string.Empty
	        };
	    }
	}
}
