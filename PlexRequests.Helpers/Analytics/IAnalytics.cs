#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IAnalytics.cs
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
using System.Threading.Tasks;

namespace PlexRequests.Helpers.Analytics
{
    public interface IAnalytics
    {
        /// <summary>
        /// Tracks the event.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="action">The action.</param>
        /// <param name="label">The label.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="value">The value.</param>
        void TrackEvent(Category category, Action action, string label, string username, string clientId, int? value = null);

        /// <summary>
        /// Tracks the event asynchronous.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="action">The action.</param>
        /// <param name="label">The label.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Task TrackEventAsync(Category category, Action action, string label, string username, string clientId, int? value = null);

        /// <summary>
        /// Tracks the page view.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="action">The action.</param>
        /// <param name="label">The label.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="value">The value.</param>
        void TrackPageview(Category category, Action action, string label, string username, string clientId, int? value = null);

        /// <summary>
        /// Tracks the page view asynchronous.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="action">The action.</param>
        /// <param name="label">The label.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Task TrackPageviewAsync(Category category, Action action, string label, string username, string clientId, int? value = null);

        /// <summary>
        /// Tracks the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="fatal">if set to <c>true</c> [fatal].</param>
        void TrackException(string message, string username, string clientId, bool fatal);

        /// <summary>
        /// Tracks the exception asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="username">The username.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="fatal">if set to <c>true</c> [fatal].</param>
        /// <returns></returns>
        Task TrackExceptionAsync(string message, string username, string clientId, bool fatal);
    }
}