using System;
using Polly.Retry;
using Polly;
using System.Threading.Tasks;

namespace PlexRequests.Api
{
	public static class RetryHandler
	{

		private static TimeSpan[] DefaultTime = new TimeSpan[] { 
			TimeSpan.FromSeconds (2),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(10)};

		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan, Action action)
		{
			if(timeSpan == null)
			{
				timeSpan = DefaultTime;
			}
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(timeSpan, (e, ts) => action());
			
			return policy;
		}

		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan)
		{
			if(timeSpan == null)
			{
				timeSpan = DefaultTime;
			}
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(timeSpan);

			return policy;
		}

		public static RetryPolicy RetryAndWaitPolicy(TimeSpan[] timeSpan, Action<Exception, TimeSpan> action)
		{
			if(timeSpan == null)
			{
				timeSpan = DefaultTime;
			}
			var policy = Policy.Handle<Exception> ()
				.WaitAndRetry(timeSpan, (exception, ts) => action(exception, ts));

			return policy;
		}
			
		public static T Execute<T>(Func<T> action, TimeSpan[] timeSpan)
		{
			var policy = RetryAndWaitPolicy (timeSpan);

			return policy.Execute (action);
		}

		public static T Execute<T>(Func<T> func, TimeSpan[] timeSpan, Action<Exception, TimeSpan> action)
		{
			if(timeSpan == null)
			{
				timeSpan = DefaultTime;
			}
			var policy = RetryAndWaitPolicy (timeSpan, action);

			return policy.Execute (func);
		}
	}
}

