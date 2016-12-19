#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ExternalSettings.cs
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

using System;
using Newtonsoft.Json;
using Ombi.Helpers;

namespace Ombi.Core.SettingModels
{
    public abstract class ExternalSettings : Settings
    {
        public bool Ssl { get; set; }
        public string SubDir { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        [JsonIgnore]
        public virtual Uri FullUri
        {
            get
            {
                if (!string.IsNullOrEmpty(SubDir))
                {
                    var formattedSubDir = Ip.ReturnUriWithSubDir(Port, Ssl, SubDir);
                    return formattedSubDir;
                }
                var formatted = Ip.ReturnUri(Port, Ssl);
                return formatted;
            }
        }
    }
}