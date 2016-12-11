#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Analytics.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using NLog;

using HttpUtility = Nancy.Helpers.HttpUtility;

namespace PlexRequests.Helpers.Analytics
{
    public class Analytics : IAnalytics
    {
        private const string AnalyticsUri = "http://www.google-analytics.com/collect";
        private const string RequestMethod = "POST";
        private const string TrackingId = "UA-77083919-2";

        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void TrackEvent(Category category, Action action, string label, string username, string clientId, int? value = null)
        {
			try
			{

			var cat = category.ToString();
				var act = action.ToString();
				Track(HitType.@event, username, cat, act, label, clientId, value);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }

        public async void TrackEventAsync(Category category, Action action, string label, string username, string clientId, int? value = null)
        {
			try
			{
				var cat = category.ToString();
				var act = action.ToString();
				await TrackAsync(HitType.@event, username, cat, act, clientId, label, value);

			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }

        public void TrackPageview(Category category, Action action, string label, string username, string clientId, int? value = null)
        {
			try
			{
				var cat = category.ToString();
				var act = action.ToString();
				Track(HitType.@pageview, username, cat, act, clientId, label, value);

			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }
        public async Task TrackPageviewAsync(Category category, Action action, string label, string username, string clientId, int? value = null)
        {
			try
			{
				var cat = category.ToString();
				var act = action.ToString();
				await TrackAsync(HitType.@pageview, username, cat, act, clientId, label, value);

			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }

        public void TrackException(string message, string username, string clientId, bool fatal)
        {
			try
			{

			var fatalInt = fatal ? 1 : 0;
				Track(HitType.exception, message, fatalInt, username, clientId);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }

        public async Task TrackExceptionAsync(string message, string username, string clientId, bool fatal)
        {
			try
			{

			var fatalInt = fatal ? 1 : 0;
				await TrackAsync(HitType.exception, message, fatalInt, username, clientId);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
        }

        private void Track(HitType type, string username, string category, string action, string clientId, string label, int? value = null)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));

            var postData = BuildRequestData(type, username, category, action, clientId, label, value, null, null);

            var postDataString = postData
                .Aggregate("", (data, next) => string.Format($"{data}&{next.Key}={HttpUtility.UrlEncode(next.Value)}"))
                .TrimEnd('&');

            SendRequest(postDataString);
        }

        private async Task TrackAsync(HitType type, string username, string category, string action, string clientId, string label, int? value = null)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrEmpty(action)) throw new ArgumentNullException(nameof(action));

            var postData = BuildRequestData(type, username, category, action, clientId, label, value, null, null);

            var postDataString = postData
                .Aggregate("", (data, next) => string.Format($"{data}&{next.Key}={HttpUtility.UrlEncode(next.Value)}"))
                .TrimEnd('&');

            await SendRequestAsync(postDataString);
        }
        private async Task TrackAsync(HitType type, string message, int fatal, string username, string clientId)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

            var postData = BuildRequestData(type, username, null, null, null, clientId, null, message, fatal);

            var postDataString = postData
                .Aggregate("", (data, next) => string.Format($"{data}&{next.Key}={HttpUtility.UrlEncode(next.Value)}"))
                .TrimEnd('&');

            await SendRequestAsync(postDataString);
        }

        private void Track(HitType type, string message, int fatal, string username, string clientId)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            var postData = BuildRequestData(type, username, null, null, null, clientId, null, message, fatal);

            var postDataString = postData
                .Aggregate("", (data, next) => string.Format($"{data}&{next.Key}={HttpUtility.UrlEncode(next.Value)}"))
                .TrimEnd('&');

            SendRequest(postDataString);
        }

        private void SendRequest(string postDataString)
        {
            var request = (HttpWebRequest)WebRequest.Create(AnalyticsUri);
            request.Method = RequestMethod;
            // set the Content-Length header to the correct value
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataString);
#if !DEBUG
            // write the request body to the request
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postDataString);
            }

            try
            {
                var webResponse = (HttpWebResponse)request.GetResponse();
                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpException((int)webResponse.StatusCode, "Google Analytics tracking did not return OK 200");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Analytics tracking failed");
            }
#endif
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task SendRequestAsync(string postDataString)
        {
            var request = (HttpWebRequest)WebRequest.Create(AnalyticsUri);
            request.Method = RequestMethod;
            // set the Content-Length header to the correct value
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataString);

#if !DEBUG
            // write the request body to the request
            using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                await writer.WriteAsync(postDataString);
            }

            try
            {
                var webResponse = (HttpWebResponse)await request.GetResponseAsync();
                if (webResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpException((int)webResponse.StatusCode, "Google Analytics tracking did not return OK 200");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Analytics tracking failed");
            }
#endif
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private Dictionary<string, string> BuildRequestData(HitType type, string username, string category, string action, string clientId, string label,  int? value, string exceptionDescription, int? fatal)
        {
            var postData = new Dictionary<string, string>
                           {
                               { "v", "1" },
                               { "tid", TrackingId },
                               { "t", type.ToString() }
                           };

            if (!string.IsNullOrEmpty(username))
            {
                postData.Add("uid", username);
            }

            postData.Add("cid", !string.IsNullOrEmpty(clientId)
                ? clientId
                : Guid.NewGuid().ToString());

            if (!string.IsNullOrEmpty(label))
            {
                postData.Add("el", label);
            }
            if (value.HasValue)
            {
                postData.Add("ev", value.ToString());
            }
            if (!string.IsNullOrEmpty(category))
            {
                postData.Add("ec", category);
            }
            if (!string.IsNullOrEmpty(action))
            {
                postData.Add("ea", action);
            }
            if (!string.IsNullOrEmpty(exceptionDescription))
            {
                postData.Add("exd", exceptionDescription);
            }
            if (fatal.HasValue)
            {
                postData.Add("exf", fatal.ToString());
            }
            return postData;
        }
    }
}