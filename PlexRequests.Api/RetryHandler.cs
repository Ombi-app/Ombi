#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RetryHandler.cs
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

using Polly;
using Polly.Retry;

namespace PlexRequests.Api
{
    public static class RetryHandler
    {
        private static readonly TimeSpan[] DefaultTime = { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) };

        public static T Execute<T>(Func<T> action, TimeSpan[] timeSpan)
        {
            var policy = RetryAndWaitPolicy(timeSpan);

            return policy.Execute(action);
        }

        public static T Execute<T>(Func<T> func, TimeSpan[] timeSpan, Action<Exception, TimeSpan> action)
        {
            if (timeSpan == null)
            {
                timeSpan = DefaultTime;
            }
            var policy = RetryAndWaitPolicy(timeSpan, action);

            return policy.Execute(func);
        }

        public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan, Action action)
        {
            if (timeSpan == null)
            {
                timeSpan = DefaultTime;
            }
            var policy = Policy.Handle<Exception>().WaitAndRetry(timeSpan, (e, ts) => action());

            return policy;
        }

        public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan)
        {
            if (timeSpan == null)
            {
                timeSpan = DefaultTime;
            }
            var policy = Policy.Handle<Exception>().WaitAndRetry(timeSpan);

            return policy;
        }

        public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan, Action<Exception, TimeSpan> action)
        {
            if (timeSpan == null)
            {
                timeSpan = DefaultTime;
            }
            var policy = Policy.Handle<Exception>().WaitAndRetry(timeSpan, action);

            return policy;
        }
    }
}