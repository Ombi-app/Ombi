#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StartupOptions.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using CommandLine;

namespace Ombi.UI.Start
{
    public class StartupOptions
    {
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        [Option('b',"base", Required = false, HelpText = "Provide a base url for Ombi")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        [Option('p', "port", Required = false, HelpText = "Provide a port for Ombi to run on. You can also change this in the settings page in the UI", Default = 3579)]
        public int Port { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StartupOptions"/> is updated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if updated; otherwise, <c>false</c>.
        /// </value>
        [Option('u', "updated", Required = false, HelpText = "This should only be used by the internal application")]
        public UpdateValue Updated { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StartupOptions"/> is updated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if updated; otherwise, <c>false</c>.
        /// </value>
        [Option('l', "listenerprefix", Required = false, HelpText = "To change the prefix for the listener")]
        public string ListenerPrefix { get; set; }

    }
}