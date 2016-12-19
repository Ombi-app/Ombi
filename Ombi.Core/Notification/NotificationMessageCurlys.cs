#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: NotificationMessageCurlys.cs
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

using System.Collections.Generic;

namespace Ombi.Core.Notification
{
    public class NotificationMessageCurlys
    {
        public NotificationMessageCurlys(string username, string title, string dateTime, string type, string issue)
        {
            Username = username;
            Title = title;
            Date = dateTime;
            Type = type;
            Issue = issue;
        }
        private string Username { get; }
        private string Title { get;  }
        private string Date { get;  }
        private string Type { get; }
        private string Issue { get;  }

        public Dictionary<string, string> Curlys => new Dictionary<string, string>
        {
            {nameof(Username), Username },
            {nameof(Title), Title },
            {nameof(Date), Date },
            {nameof(Type), Type },
            {nameof(Issue), Issue }
        };
    }
}