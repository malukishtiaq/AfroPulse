using Android.OS;
using Android.Runtime;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using Newtonsoft.Json;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Collections.ObjectModel;
using static PlayTube.Helpers.Models.Classes;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace PlayTube.Activities.Shorts.Adapters
{
	public class ShortsVideoPagerAdapter : FragmentStateAdapter
	{
		private int CountVideo;
		private ObservableCollection<ShortsVideoClass> DataVideos;

		public ShortsVideoPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public ShortsVideoPagerAdapter(Fragment fragment) : base(fragment)
		{
		}

		public ShortsVideoPagerAdapter(FragmentActivity fragmentActivity) : base(fragmentActivity)
		{
		}

		public ShortsVideoPagerAdapter(FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
		{
		}

		public ShortsVideoPagerAdapter(FragmentActivity fragmentActivity, int size, ObservableCollection<ShortsVideoClass> dataVideos) : base(fragmentActivity)
		{
			try
			{
				CountVideo = size;
				DataVideos = dataVideos;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void UpdateShortsVideoPager(int size, ObservableCollection<ShortsVideoClass> dataVideos)
		{
			try
			{
				CountVideo = size;
				DataVideos = dataVideos;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => CountVideo;

		public override Fragment CreateFragment(int position)
		{
			try
			{
				Bundle bundle = new Bundle();
				bundle.PutInt("position", position);

				var dataItem = DataVideos[position];
				if (dataItem.Type == ItemType.ShortVideos)
				{
					if (dataItem.VideoData != null)
					{
						bundle.PutString("DataItem", JsonConvert.SerializeObject(dataItem.VideoData));
						ViewShortsVideoFragment viewShortsVideoFragment = new ViewShortsVideoFragment { Arguments = bundle };
						return viewShortsVideoFragment;
					}
				}
				else
				{
					AdsFragment adsFragment = new AdsFragment();
					return adsFragment;
				}

				return null;
			}
			catch (Exception a)
			{
				Methods.DisplayReportResultTrack(a);
				return null;
			}
		}

		public override bool ContainsItem(long itemId)
		{
			try
			{
				return base.ContainsItem(itemId);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}

		public override long GetItemId(int position)
		{
			try
			{
				return position;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return 0;
			}
		}
	}
}