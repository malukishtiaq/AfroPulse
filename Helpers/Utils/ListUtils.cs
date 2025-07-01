using PlayTube.Helpers.Models;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Messages;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static PlayTube.Helpers.Models.Classes;

namespace PlayTube.Helpers.Utils
{
	public static class ListUtils
	{
		//############# DONT'T MODIFY HERE #############
		//List Items Declaration 
		//*********************************************************
		public static GetSettingsObject.SiteSettingsObject MySettingsList;
		public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();

		public static readonly ObservableCollection<VideoDataObject> FeaturedVideosList = new ObservableCollection<VideoDataObject>();
		public static readonly ObservableCollection<VideoDataObject> GlobalNotInterestedList = new ObservableCollection<VideoDataObject>();
		public static readonly ObservableCollection<VideoDataObject> GlobalViewsVideosList = new ObservableCollection<VideoDataObject>();
		public static readonly ObservableCollection<UserDataObject> MyChannelList = new ObservableCollection<UserDataObject>();
		public static ObservableCollection<GetChatsObject.Data> ChatList = new ObservableCollection<GetChatsObject.Data>();
		public static ObservableCollection<PlayListVideoObject> PlayListVideoObjectList = new ObservableCollection<PlayListVideoObject>();
		public static ObservableCollection<VideoAdDataObject> AdsVideoList = new ObservableCollection<VideoAdDataObject>();
		public static ObservableCollection<Classes.LibraryItem> LibraryList = new ObservableCollection<Classes.LibraryItem>();

		public static ObservableCollection<VideoDataObject> ArrayListPlay { set; get; } = new ObservableCollection<VideoDataObject>();
		public static ObservableCollection<VideoDataObject> LessonList { set; get; } = new ObservableCollection<VideoDataObject>();
		public static readonly ObservableCollection<ShortsVideoClass> VideoShortsList = new ObservableCollection<ShortsVideoClass>();
		public static ObservableCollection<VideoDataObject> VideoShortsViewsList = new ObservableCollection<VideoDataObject>();

		public static void ClearAllList()
		{
			try
			{
				MySettingsList = null;
				DataUserLoginList.Clear();
				FeaturedVideosList.Clear();
				GlobalNotInterestedList.Clear();
				GlobalViewsVideosList.Clear();
				MyChannelList.Clear();
				ChatList.Clear();
				PlayListVideoObjectList.Clear();
				AdsVideoList.Clear();
				ArrayListPlay.Clear();
				LessonList.Clear();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
		{
			try
			{
				items.ToList().ForEach(collection.Add);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
		{
			var list = new List<List<T>>();

			for (int i = 0; i < locations.Count; i += nSize)
			{
				list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
			}

			return list;
		}

		public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
		{
			var enumerable = source as T[] ?? source.ToArray();

			return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
		}

		public static void AddRepeated<T>(this List<T> self, T item, int count)
		{
			var temp = Enumerable.Repeat(item, count);
			self.AddRange(temp);
		}
	}
}