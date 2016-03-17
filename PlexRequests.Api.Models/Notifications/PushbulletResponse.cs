#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PushbulletResponse.cs
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
namespace PlexRequests.Api.Models.Notifications
{
    public class PushbulletResponse
    {
        public bool active { get; set; }
        public string iden { get; set; }
        public double created { get; set; }
        public double modified { get; set; }
        public string type { get; set; }
        public bool dismissed { get; set; }
        public string direction { get; set; }
        public string sender_iden { get; set; }
        public string sender_email { get; set; }
        public string sender_email_normalized { get; set; }
        public string sender_name { get; set; }
        public string receiver_iden { get; set; }
        public string receiver_email { get; set; }
        public string receiver_email_normalized { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
}