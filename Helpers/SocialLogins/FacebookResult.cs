﻿using Newtonsoft.Json;

namespace PlayTube.Helpers.SocialLogins
{
	public class FacebookResult
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("email")]
		public string Email { get; set; }
	}
}