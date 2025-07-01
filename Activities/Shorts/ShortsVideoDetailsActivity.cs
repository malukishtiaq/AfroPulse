using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using PlayTube.Activities.Shorts.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.MediaPlayers.Exo;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Activities.Shorts
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class ShortsVideoDetailsActivity : AppCompatActivity
	{
		#region Variables Basic

		private readonly string KeySelectedPage = "KEY_SELECTED_PAGE_Shorts";

		public ViewPager2 Pager;

		public ShortsVideoPagerAdapter MAdapter;
		private int SelectedPage, VideosCount;
		private string Type;
		private ObservableCollection<Classes.ShortsVideoClass> DataVideos = new ObservableCollection<Classes.ShortsVideoClass>();
		private static ShortsVideoDetailsActivity Instance;
		public string OpenEvent = "";
		public UserDataObject UserDataObjectOpenEvent;
		public PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this);

				Instance = this;

				if (savedInstanceState != null)
				{
					SelectedPage = savedInstanceState.GetInt(KeySelectedPage);
				}

				// Create your application here
				SetContentView(Resource.Layout.ShortsVideoLayout);

				TabbedMainActivity.GetInstance()?.SetOnWakeLock();

				PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(this);

				if (Intent != null)
				{
					Type = Intent?.GetStringExtra("Type") ?? "";
					SelectedPage = Intent?.GetIntExtra("IndexItem", 0) ?? 0;
					VideosCount = Intent?.GetIntExtra("VideosCount", 0) ?? 0;
					if (Type is "VideoShorts" or "OpenVideoShort")
					{
						foreach (var video in ListUtils.VideoShortsList)
						{
							var check = DataVideos.FirstOrDefault(a => a.Id == video.Id);
							if (check == null)
							{
								DataVideos.Add(new Classes.ShortsVideoClass
								{
									Id = video.Id,
									Type = ItemType.ShortVideos,
									VideoData = video.VideoData
								});
							}

							if (AdsGoogle.NativeAdsPool?.Count > 0 && DataVideos.Count % AppSettings.ShowAdNativeCount == 0)
							{
								DataVideos.Add(new Classes.ShortsVideoClass
								{
									Type = ItemType.AdMob4,
								});
							}
						}

						if (Type == "OpenVideoShort")
						{
							var data = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("DataItem") ?? "");
							if (data != null)
							{
								var checkVideo = DataVideos.FirstOrDefault(a => a.Id == data.Id);
								if (checkVideo != null)
								{
									DataVideos.Move(DataVideos.IndexOf(checkVideo), 0);
								}
								else
								{
									DataVideos.Insert(0, new Classes.ShortsVideoClass()
									{
										Id = data.Id,
										Type = ItemType.ShortVideos,
										VideoData = data
									});
								}
							}
						}
					}
					else if (Type == "UserShorts")
					{
						var list = JsonConvert.DeserializeObject<ObservableCollection<VideoDataObject>>(Intent?.GetStringExtra("DataItem") ?? "");
						if (list?.Count > 0)
						{
							foreach (var data in list)
							{
								var check = DataVideos.FirstOrDefault(a => a.Id == data.Id);
								if (check == null)
								{
									DataVideos.Add(new Classes.ShortsVideoClass
									{
										Id = data.Id,
										Type = ItemType.ShortVideos,
										VideoData = data
									});
								}

								if (AdsGoogle.NativeAdsPool?.Count > 0 && DataVideos.Count % AppSettings.ShowAdNativeCount == 0)
								{
									DataVideos.Add(new Classes.ShortsVideoClass
									{
										Type = ItemType.AdMob4,
									});
								}
							}
						}
					}
					else
						DataVideos = JsonConvert.DeserializeObject<ObservableCollection<Classes.ShortsVideoClass>>(Intent?.GetStringExtra("DataItem") ?? "");
				}

				VideosCount = DataVideos?.Count ?? 0;

				//Get Value And Set Toolbar
				InitComponent();

				if (Type is "OpenVideoShort" or "VideoShorts")
					Task.Factory.StartNew(StartApiService);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();
				var instance = ViewShortsVideoFragment.GetInstance();
				instance?.StopVideo();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnStop()
		{
			try
			{
				base.OnStop();
				var instance = ViewShortsVideoFragment.GetInstance();
				instance?.StopVideo();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			try
			{
				base.OnSaveInstanceState(outState);
				outState.PutInt(KeySelectedPage, Pager.CurrentItem);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			try
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				base.OnTrimMemory(level);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnLowMemory()
		{
			try
			{
				GC.Collect(GC.MaxGeneration);
				base.OnLowMemory();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnDestroy()
		{
			try
			{
				if (OpenEvent == "Profile")
				{
					TabbedMainActivity globalContext = TabbedMainActivity.GetInstance();
					globalContext.ShowUserChannelFragment(UserDataObjectOpenEvent, UserDataObjectOpenEvent.Id);
				}

				TabbedMainActivity.GetInstance()?.SetOffWakeLock();
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

				var instance = ViewShortsVideoFragment.GetInstance();
				instance?.StopVideo();
				PreCachingExoPlayerVideo?.Destroy();

				DestroyBasic();

				base.OnDestroy();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Menu

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
			}
			return base.OnOptionsItemSelected(item);
		}

		#endregion

		#region Functions

		private void InitComponent()
		{
			try
			{
				Pager = FindViewById<ViewPager2>(Resource.Id.viewpager);

				MAdapter = new ShortsVideoPagerAdapter(this, VideosCount, DataVideos);

				//Pager.CurrentItem = MAdapter.ItemCount;
				//Pager.OffscreenPageLimit = 0;

				Pager.Orientation = ViewPager2.OrientationVertical;
				//Pager.SetPageTransformer(new CustomViewPageTransformer(TransformType.Flow));
				Pager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
				Pager.Adapter = MAdapter;
				Pager.Adapter.NotifyDataSetChanged();

				Pager.SetCurrentItem(SelectedPage, false);

				AdsGoogle.AdMobNative ads = new AdsGoogle.AdMobNative();
				ads.BindAdMobNative(this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void DestroyBasic()
		{
			try
			{
				Pager = null!;
				MAdapter = null!;
				Instance = null!;
				SelectedPage = 0;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static ShortsVideoDetailsActivity GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#endregion

		#region Permissions

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				switch (requestCode)
				{

					case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
						var instance = ViewShortsVideoFragment.GetInstance();
						instance?.DownloadVideo();
						break;
					case 100:
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
		{
			private readonly ShortsVideoDetailsActivity Activity;

			public MyOnPageChangeCallback(ShortsVideoDetailsActivity activity)
			{
				try
				{
					Activity = activity;
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}

			private int LastPosition = -1;
			public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
				try
				{
					base.OnPageScrolled(position, positionOffset, positionOffsetPixels);

					if (LastPosition == -1)
					{
						LastPosition = position;
					}
					else
					{
						LastPosition = position;
						var instance = ViewShortsVideoFragment.GetInstance();
						instance?.StopVideo();
					}

					if (position > Activity.DataVideos.Count - 3 && (Activity.Type is "VideoShorts" or "OpenVideoShort"))
						Task.Factory.StartNew(Activity.StartApiService);
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}

		private void StartApiService()
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetAllShortsVideo });
		}

		private bool ApiRun;
		private async Task GetAllShortsVideo()
		{
			try
			{
				if (!Methods.CheckConnectivity() || ApiRun)
					return;

				ApiRun = true;

				var offset = ListUtils.VideoShortsViewsList.Aggregate("", (current, dataObject) => current + ("," + dataObject.Id)) ?? "";
				if (!string.IsNullOrEmpty(offset))
				{
					offset = offset.Remove(0, 1);
				}

				var (apiStatus, respond) = await RequestsAsync.Video.GetShortsAsync(offset);
				if (apiStatus != 200 || respond is not GetVideosListDataObject result || result.VideoList == null)
				{
					Methods.DisplayReportResult(this, respond);
				}
				else
				{
					bool add = false;
					var respondList = result.VideoList?.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);
						foreach (var item in from item in result.VideoList let check = ListUtils.VideoShortsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
						{
							var checkViewed = ListUtils.VideoShortsViewsList.FirstOrDefault(a => a.Id == item.Id);
							if (checkViewed == null)
							{
								add = true;
								ListUtils.VideoShortsList.Add(new Classes.ShortsVideoClass
								{
									Id = item.Id,
									Type = ItemType.ShortVideos,
									VideoData = item
								});

								if (AdsGoogle.NativeAdsPool?.Count > 0 && ListUtils.VideoShortsList.Count % AppSettings.ShowAdNativeCount == 0)
								{
									ListUtils.VideoShortsList.Add(new Classes.ShortsVideoClass
									{
										Type = ItemType.AdMob4,
									});
								}

								RunOnUiThread(() =>
								{
									try
									{
										Glide.With(this).Load(item.Owner?.OwnerClass.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
									}
									catch (Exception e)
									{
										Methods.DisplayReportResultTrack(e);
									}
								});
							}
						}
					}

					RunOnUiThread(() =>
					{
						try
						{
							if (add)
							{
								DataVideos = ListUtils.VideoShortsList;
								MAdapter.UpdateShortsVideoPager(DataVideos.Count, DataVideos);
								//Pager.Adapter.NotifyDataSetChanged();
							}
						}
						catch (Exception e)
						{
							Methods.DisplayReportResultTrack(e);
						}
					});

					ApiRun = false;
				}
			}
			catch (Exception e)
			{
				ApiRun = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

	}
}