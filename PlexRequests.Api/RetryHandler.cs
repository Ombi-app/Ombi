using System;
using Polly.Retry;
using Polly;

namespace PlexRequests.Api
{
	public static class RetryHandler
	{
		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] TimeSpan, Action action)
		{
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(TimeSpan, (exception, timeSpan) => action());
			
			return policy;
		}

		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] TimeSpan)
		{
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(TimeSpan);

			return policy;
		}

		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] TimeSpan, Action<Exception, TimeSpan> action)
		{
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(TimeSpan, (exception, timeSpan) => action(exception, timeSpan));

			return policy;
		}
	}
}

