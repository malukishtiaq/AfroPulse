//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using PlayTube.PlayTubeClient.Classes.Global;
using SQLite;

namespace PlayTube.SQLite
{
	public class DataTables
	{
		[Table("LoginTb")]
		public class LoginTb
		{
			[PrimaryKey, AutoIncrement]
			public int AutoId { get; set; }

			public string UserId { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public string Cookie { get; set; }
			public string Email { get; set; }
			public string Status { get; set; }
			public string Lang { get; set; }
			public string DeviceId { get; set; }
			public string AccessToken { get; set; }
		}

		[Table("ChannelTb")]
		public class ChannelTb : UserDataObject
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdChannel { get; set; }

			public new string FavCategory { get; set; }
		}

		[Table("MySettingsTb")]
		public class MySettingsTb : GetSettingsObject.SiteSettingsObject
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdMySettings { get; set; }

			public new string AffiliateType { get; set; }
			public new string CurrencyArray { get; set; }
			public new string CurrencySymbolArray { get; set; }
			public new string Continents { get; set; }
			public new string MoviesCategories { get; set; }
			public new string SubCategories { get; set; }
			public new string Categories { get; set; }
		}

		[Table("WatchOfflineVideosTb")]
		public class WatchOfflineVideosTb : VideoDataObject
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdaWatchOffline { get; set; }

			public new string Owner { get; set; }
			public new string PlaylistData { get; set; }

			////Extended
			//public string VideoName { get; set; }
			//public string VideoSavedPath { get; set; } 
		}

		[Table("SubscriptionsChannelTb")]
		public class SubscriptionsChannelTb : UserDataObject
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdSubscriptions { get; set; }

			public new string FavCategory { get; set; }
		}

		[Table("LibraryItemTb")]
		public class LibraryItemTb
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdLibrary { get; set; }

			public string SectionId { get; set; }
			public string SectionText { get; set; }
			public int VideoCount { get; set; }
			public string BackgroundImage { get; set; }
			public int Icon { get; set; }
			public string BackgroundColor { get; set; }
		}

		[Table("SharedVideosTb")]
		public class SharedVideosTb : VideoDataObject
		{
			[PrimaryKey, AutoIncrement]
			public int AutoIdSharedVideos { get; set; }

			public new string Owner { get; set; }
			public new string PlaylistData { get; set; }
		}

		[Table("LastChatTb")]
		public class LastChatTb
		{
			[PrimaryKey, AutoIncrement] public int AutoIdLastChat { get; set; }

			public string Id { get; set; }
			public string UserOne { get; set; }
			public string UserTwo { get; set; }
			public string Time { get; set; }
			public string TextTime { get; set; }
			public string UserDataJson { get; set; }
			public string GetCountSeen { get; set; }
			public string GetLastMessageJson { get; set; }
		}

		[Table("MessageTb")]
		public class MessageTb
		{
			[PrimaryKey, AutoIncrement] public int AutoIdMessage { get; set; }

			public string Id { get; set; }
			public string FromId { get; set; }
			public string ToId { get; set; }
			public string Text { get; set; }
			public string Seen { get; set; }
			public string Time { get; set; }
			public string FromDeleted { get; set; }
			public string ToDeleted { get; set; }
			public string TextTime { get; set; }
			public string Position { get; set; }
		}
	}
}