using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayTube.Helpers.Controller
{
	public static class PollyController
	{
		public static void RunRetryPolicyFunction(List<Func<Task>> actionList, int retryCount = 4, int everySecond = 4)
		{
			// Handle both exceptions and return values in one policy
			//HttpStatusCode[] httpStatusCodesWorthRetrying = {
			//    HttpStatusCode.RequestTimeout, // 408
			//    HttpStatusCode.InternalServerError, // 500
			//    HttpStatusCode.BadGateway, // 502
			//    HttpStatusCode.ServiceUnavailable, // 503
			//    HttpStatusCode.GatewayTimeout // 504
			//};

			var retryPolicy = Policy.Handle<HttpRequestException>()
				.Or<OperationCanceledException>()
				.Or<Exception>()
				//.OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
				.WaitAndRetryAsync(retryCount, i => TimeSpan.FromSeconds(everySecond));

			foreach (var action in actionList)
				retryPolicy.ExecuteAsync(action);

		}
	}
}