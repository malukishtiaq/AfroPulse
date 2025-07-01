using Newtonsoft.Json;

namespace PlayTube.Helpers.Models
{
	public class OneSignalObject
	{
		public class NotificationInfoObject
		{
			[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
			public int Id { get; set; }

			[JsonProperty("notifier_id", NullValueHandling = NullValueHandling.Ignore)]
			public int NotifierId { get; set; }

			[JsonProperty("recipient_id", NullValueHandling = NullValueHandling.Ignore)]
			public int RecipientId { get; set; }

			[JsonProperty("video_id", NullValueHandling = NullValueHandling.Ignore)]
			public int VideoId { get; set; }

			[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
			public string Type { get; set; }

			[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
			public string Text { get; set; }

			[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
			public string Url { get; set; }

			[JsonProperty("seen", NullValueHandling = NullValueHandling.Ignore)]
			public string Seen { get; set; }

			[JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
			public string Time { get; set; }

			[JsonProperty("sent_push", NullValueHandling = NullValueHandling.Ignore)]
			public int SentPush { get; set; }

			[JsonProperty("type_text", NullValueHandling = NullValueHandling.Ignore)]
			public string TypeText { get; set; }
		}

	}
}