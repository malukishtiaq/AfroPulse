using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Library
{
	public class WatchOfflineVideosFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		private VideoBigAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private TabbedMainActivity GlobalContext;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = (TabbedMainActivity)Activity;
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);
				//Get Value And Set Toolbar
				InitComponent(view);
				InitShimmer(view);
				InitToolbar(view);
				SetRecyclerViewAdapters();

				MAdapter.ItemClick += MAdapterOnItemClick;

				//Get Data  
				LoadDataAsync();

				AdsGoogle.Ad_RewardedVideo(Activity);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnLowMemory()
		{
			try
			{
				GC.Collect(GC.MaxGeneration);
				base.OnLowMemory();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
				EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

				SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				ShowFacebookAds(view, MRecycler);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitShimmer(View view)
		{
			try
			{
				ShimmerPageLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
				InflatedShimmer ??= ShimmerPageLayout.Inflate();

				ShimmerInflater = new TemplateShimmerInflater();
				ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.VideoBigTemplate);
				ShimmerInflater.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetRecyclerViewAdapters()
		{
			try
			{
				MAdapter = new VideoBigAdapter(Activity)
				{
					VideoList = new ObservableCollection<VideoDataObject>()
				};
				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<VideoDataObject>(Activity, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_WatchOffline_Videos), toolbarTitle);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void MAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					var sqlEntity = new SqLiteDatabase();
					var dataVideo = sqlEntity.Get_LatestWatchOfflineVideos(item.Id);
					if (dataVideo != null && !string.IsNullOrEmpty(dataVideo.VideoLocation) && (dataVideo.VideoLocation.Contains("file://") || dataVideo.VideoLocation.Contains("content://") || dataVideo.VideoLocation.Contains("storage") || dataVideo.VideoLocation.Contains("/data/user/0/")))
					{
						var filePath = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(dataVideo.Title);
						if (!string.IsNullOrEmpty(filePath))
							item.VideoLocation = filePath;
						else
						{
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_FileNotFoundInDownload), ToastLength.Long)?.Show();
							return;
						}

						GlobalContext.StartPlayVideo(item);
					}
					else
					{
						Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_FileNotFoundInDownload), ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data 

		private void LoadDataAsync()
		{
			try
			{
				var sqlEntity = new SqLiteDatabase();
				var listVideos = sqlEntity.Get_WatchOfflineVideos();
				if (listVideos.Count > 0)
				{
					MAdapter.VideoList = new ObservableCollection<VideoDataObject>(listVideos);
					MAdapter.NotifyDataSetChanged();
				}
				GlobalContext.LibrarySynchronizer.AddToWatchOffline(MAdapter.VideoList.FirstOrDefault(), MAdapter.VideoList.Count);
				ShowEmptyPage();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = false;

				if (MAdapter.VideoList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					Inflated ??= EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoWatchOfflineVideos);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
					}
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				ShimmerInflater?.Hide();
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion
	}
}