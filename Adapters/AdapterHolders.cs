using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using PlayTube.Activities.Library.Adapters;
using PlayTube.Activities.Tabbes.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Adapters
{
	public class AdapterHolders
	{
		public class EmptyStateViewHolder : RecyclerView.ViewHolder
		{
			#region Variables Basic

			public View MainView { get; private set; }
			public AppCompatButton EmptyStateButton { get; private set; }
			public ImageView EmptyStateIcon { get; private set; }
			public TextView DescriptionText { get; private set; }
			public TextView TitleText { get; private set; }

			#endregion

			public EmptyStateViewHolder(View itemView) : base(itemView)
			{
				try
				{
					MainView = itemView;

					EmptyStateIcon = (ImageView)MainView.FindViewById(Resource.Id.emtyicon);
					TitleText = (TextView)MainView.FindViewById(Resource.Id.headText);
					DescriptionText = (TextView)MainView.FindViewById(Resource.Id.seconderyText);
					EmptyStateButton = (AppCompatButton)MainView.FindViewById(Resource.Id.button);
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}
		public class AdMobAdapterViewHolder : RecyclerView.ViewHolder
		{
			public View MainView { get; private set; }
			public TemplateView MianAlert { get; private set; }

			public AdMobAdapterViewHolder(View itemView, Activity activity) : base(itemView)
			{
				try
				{
					MainView = itemView;

					MianAlert = MainView.FindViewById<TemplateView>(Resource.Id.my_template);
					MianAlert.Visibility = ViewStates.Gone;

					AdsGoogle.Ad_AdMobNative(activity, MianAlert);
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}

		public class AdMob3AdapterViewHolder : RecyclerView.ViewHolder
		{
			public View MainView { get; private set; }
			public AdManagerAdView AdManagerAdView { get; private set; }

			public AdMob3AdapterViewHolder(View itemView) : base(itemView)
			{
				try
				{
					MainView = itemView;

					AdManagerAdView = MainView.FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
					AdManagerAdView.Visibility = ViewStates.Gone;
					AdsGoogle.InitAdManagerAdView(AdManagerAdView);
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}

		private static ItemType LastAdsType = ItemType.AdMob3;
		public static void AddAds(TrendingAdapter mAdapter, ItemType typeView, bool add = false)
		{
			try
			{
				if (AppSettings.ShowAdMobNative)
				{
					if (mAdapter.TrendingList.Count > 0)
					{
						if (typeView == ItemType.BigVideo)
						{
							if (mAdapter.TrendingList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								switch (LastAdsType)
								{
									case ItemType.AdMob1:
										mAdapter.TrendingList.Add(new Classes.TrendingClass
										{
											Type = ItemType.AdMob1,
										});
										LastAdsType = ItemType.AdMob3;
										break;
									case ItemType.AdMob3:
										mAdapter.TrendingList.Add(new Classes.TrendingClass
										{
											Type = ItemType.AdMob3,
										});
										LastAdsType = ItemType.AdMob1;
										break;
								}
							}
						}
						else if (typeView == ItemType.RowVideo)
						{
							if (mAdapter.TrendingList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								if (add)
								{
									mAdapter.TrendingList.Add(new Classes.TrendingClass
									{
										Type = ItemType.AdMob3,
									});
								}
								else
								{
									mAdapter.TrendingList.Add(new Classes.TrendingClass
									{
										Type = ItemType.AdMob2,
									});
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public static void AddAds(SubscriptionsAdapter mAdapter, ItemType typeView, bool add = false)
		{
			try
			{
				if (AppSettings.ShowAdMobNative)
				{
					if (mAdapter.SubscriptionsList.Count > 0)
					{
						if (typeView == ItemType.BigVideo)
						{
							if (mAdapter.SubscriptionsList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								switch (LastAdsType)
								{
									case ItemType.AdMob1:
										mAdapter.SubscriptionsList.Add(new Classes.SubscriptionsClass
										{
											Type = ItemType.AdMob1,
										});
										LastAdsType = ItemType.AdMob3;
										break;
									case ItemType.AdMob3:
										mAdapter.SubscriptionsList.Add(new Classes.SubscriptionsClass
										{
											Type = ItemType.AdMob3,
										});
										LastAdsType = ItemType.AdMob1;
										break;
								}
							}
						}
						else if (typeView == ItemType.RowVideo)
						{
							if (mAdapter.SubscriptionsList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								if (add)
								{
									mAdapter.SubscriptionsList.Add(new Classes.SubscriptionsClass
									{
										Type = ItemType.AdMob3,
									});
								}
								else
								{
									mAdapter.SubscriptionsList.Add(new Classes.SubscriptionsClass
									{
										Type = ItemType.AdMob2,
									});
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public static void AddAds(MainVideoAdapter mAdapter, ItemType typeView, bool add = false)
		{
			try
			{
				if (AppSettings.ShowAdMobNative)
				{
					if (mAdapter.MainVideoList.Count > 0)
					{
						if (typeView == ItemType.BigVideo)
						{
							if (mAdapter.MainVideoList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								switch (LastAdsType)
								{
									case ItemType.AdMob1:
										mAdapter.MainVideoList.Add(new Classes.MainVideoClass()
										{
											Type = ItemType.AdMob1,
										});
										LastAdsType = ItemType.AdMob3;
										break;
									case ItemType.AdMob3:
										mAdapter.MainVideoList.Add(new Classes.MainVideoClass
										{
											Type = ItemType.AdMob3,
										});
										LastAdsType = ItemType.AdMob1;
										break;
								}
							}
						}
						else if (typeView == ItemType.RowVideo)
						{
							if (mAdapter.MainVideoList.Count % AppSettings.ShowAdNativeCount == 0 || add)
							{
								if (add)
								{
									mAdapter.MainVideoList.Add(new Classes.MainVideoClass
									{
										Type = ItemType.AdMob3,
									});
								}
								else
								{
									mAdapter.MainVideoList.Add(new Classes.MainVideoClass
									{
										Type = ItemType.AdMob2,
									});
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}