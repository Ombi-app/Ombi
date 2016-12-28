#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: EmailBasicTemplate.cs
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
using System.IO;
using System.Text;
using System.Windows.Forms;
using NLog;

namespace Ombi.Core.Notification.Templates
{
    public class EmailBasicTemplate : IEmailBasicTemplate
    { 
        public string TemplateLocation => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty, "Notification", "Templates", "BasicRequestTemplate.html");
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string SubjectKey = "{@SUBJECT}";
        private const string BodyKey = "{@BODY}";
        private const string ImgSrc = "{@IMGSRC}";
        private const string DateKey = "{@DATENOW}";
        
        public string LoadTemplate(string subject, string body, string imgSrc)
        {
            try
            {
                var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
                sb.Replace(SubjectKey, subject);
                sb.Replace(BodyKey, body);
                sb.Replace(ImgSrc, imgSrc);
                sb.Replace(DateKey, DateTime.Now.ToString("f"));

                return sb.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return string.Empty;
            }
        }
    }
}