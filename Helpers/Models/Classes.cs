//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using PlayTube.Helpers.Fonts;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using System.Collections.Generic;

namespace PlayTube.Helpers.Models
{
	public class Classes
	{
		public class Category
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public int Image { get; set; }
			public string Color { get; set; }
			public List<Category> SubList { get; set; }
		}


		public class TrendingClass
		{
			public int Id { get; set; }
			public ItemType Type { get; set; }
			public List<GetPopularChannelsObject.Channel> ChannelList { get; set; }
			public VideoDataObject VideoData { get; set; }
		}

		public class ShortsVideoClass
		{
			public string Id { get; set; }
			public ItemType Type { get; set; }
			public VideoDataObject VideoData { get; set; }
		}

		public class MainVideoClass
		{
			public int Id { get; set; }
			public ItemType Type { get; set; }
			public List<VideoDataObject> TopVideoList { get; set; }
			public List<VideoDataObject> LatestVideoList { get; set; }
			public List<VideoDataObject> FavVideoList { get; set; }
			public List<VideoDataObject> StockVideoList { get; set; }
			public List<VideoDataObject> ShortsVideoList { get; set; }
			public VideoDataObject VideoData { get; set; }
			public List<VideoDataObject> LiveVideoList { get; set; }
			public List<VideoDataObject> SliderVideoList { get; set; }
		}

		public class SubscriptionsClass
		{
			public int Id { get; set; }
			public ItemType Type { get; set; }
			public List<UserDataObject> ChannelList { get; set; }
			public VideoDataObject VideoData { get; set; }
		}

		public class LibraryItem
		{
			public string SectionId { get; set; }
			public string SectionText { get; set; }
			public int VideoCount { get; set; }
			public string BackgroundImage { get; set; }
			public int Icon { get; set; }
			public string BackgroundColor { get; set; }
		}

		public class ItemOptionObject
		{
			public string Id { get; set; }
			public string Text { get; set; }
			public int Icon { get; set; }
			public FontsIconFrameWork TypeIcon { get; set; }
			public string Color { get; set; }
		}
	}

	public enum ItemType
	{
		RowVideo = 100, BigVideo = 500, Channel = 200, EmptyPage = 300, TopVideos = 101, LatestVideos = 102, FavVideos = 103, OtherVideos = 104, LiveVideos = 105, StockVideos = 106, SliderHome = 107, ShortVideos = 108
		, AdMob1 = 601, AdMob2 = 602, AdMob3 = 603, AdMob4 = 604,
	}


	public enum ShowAds
	{
		AllUsers,
		UnProfessional,
	}


	public enum CardPlayerView
	{
		Radius,
		Square,
	}

	public enum TabTheme
	{
		Dark,
		Light,
		SameAsPhone,
	}

	public enum PlayerTheme
	{
		Theme1,
		Theme2,
	}

	public enum ShareSystem
	{
		WebsiteUrl,
		ApplicationShortUrl,
	}

}