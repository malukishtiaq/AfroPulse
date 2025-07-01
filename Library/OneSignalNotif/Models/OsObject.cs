using Newtonsoft.Json;
using PlayTube.PlayTubeClient.Classes.Global;

namespace PlayTube.Library.OneSignalNotif.Models
{
	public class OsObject
	{
		public class OsNotificationObject
		{
			[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
			public string UserId { get; set; }

			[JsonProperty("notify_type", NullValueHandling = NullValueHandling.Ignore)]
			public string Type { get; set; }

			[JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
			public VideoDataObject Video { get; set; }

			[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
			public string Url { get; set; }

		}
	}
}