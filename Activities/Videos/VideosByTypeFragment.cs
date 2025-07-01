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
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Videos
{
	public class VideosByTypeFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		private VideoBigAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		private string TypeVideo = "";

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
				TypeVideo = Arguments?.GetString("Type") ?? "";

				//Get Value And Set Toolbar
				InitComponent(view);
				InitShimmer(view);
				InitToolbar(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;

				//Get Data Api
				GetVideoByType();

				AdsGoogle.Ad_Interstitial(Activity);
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
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				ShowFacebookAds(view, MRecycler);
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

				RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
				MainScrollEvent = xamarinRecyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
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
				if (toolbar != null)
				{
					GlobalContext.SetToolBar(toolbar, TypeVideo + " " + Context.GetText(Resource.String.Lbl_Videos));
				}
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
				ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.VideoRowTemplate);
				ShimmerInflater.Show();
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
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			try
			{
				ShimmerInflater?.Show();

				//Get Data Api
				MAdapter.VideoList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Scroll

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Code get last id where LoadMore >>
				var item = MAdapter.VideoList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void GetVideoByType()
		{
			try
			{
				if (TypeVideo.Contains("Top"))
				{
					var checkList = GlobalContext.HomeFragment.LatestHomeTab.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.TopVideos);
					if (checkList != null)
					{
						MAdapter.VideoList = new ObservableCollection<VideoDataObject>(checkList.TopVideoList);

						var lastId = checkList.TopVideoList.LastOrDefault()?.Id ?? "0";
						StartApiService(lastId);
					}
				}
				else if (TypeVideo.Contains("Latest"))
				{
					var checkList = GlobalContext.HomeFragment.LatestHomeTab.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LatestVideos);
					if (checkList != null)
					{
						MAdapter.VideoList = new ObservableCollection<VideoDataObject>(checkList.LatestVideoList);

						var lastId = checkList.LatestVideoList.LastOrDefault()?.Id ?? "0";
						StartApiService("0", lastId);
					}
				}
				else if (TypeVideo.Contains("Fav"))
				{
					var checkList = GlobalContext.HomeFragment.LatestHomeTab.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.FavVideos);
					if (checkList != null)
					{
						MAdapter.VideoList = new ObservableCollection<VideoDataObject>(checkList.FavVideoList);

						var lastId = checkList.FavVideoList.LastOrDefault()?.Id ?? "0";
						StartApiService("0", "0", lastId);
					}
				}
				else if (TypeVideo.Contains("Live"))
				{
					var checkList = GlobalContext.HomeFragment.LatestHomeTab.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LiveVideos);
					if (checkList != null)
					{
						MAdapter.VideoList = new ObservableCollection<VideoDataObject>(checkList.LiveVideoList);

						var lastId = checkList.LiveVideoList.LastOrDefault()?.Id ?? "0";
						StartApiService("0", "0", "0", lastId);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void StartApiService(string topOffset = "0", string latestOffset = "0", string favOffset = "0", string liveOffset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(topOffset, latestOffset, favOffset, liveOffset) });
		}

		private async Task LoadDataAsync(string topOffset = "0", string latestOffset = "0", string favOffset = "0", string liveOffset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.VideoList.Count;

				var (apiStatus, respond) = await RequestsAsync.Video.GetVideosAsync("0", topOffset, latestOffset, favOffset, liveOffset, "25");
				if (apiStatus != 200 || respond is not GetVideosObject result || result.DataResult == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					int respondList = 0;
					if (TypeVideo.Contains("Top"))
					{
						result.DataResult.Top = AppTools.ListFilter(new List<VideoDataObject>(result.DataResult.Top));
						respondList = result.DataResult.Top.Count;
					}
					else if (TypeVideo.Contains("Latest"))
					{
						result.DataResult.Latest = AppTools.ListFilter(new List<VideoDataObject>(result.DataResult.Latest));
						respondList = result.DataResult.Latest.Count;
					}
					else if (TypeVideo.Contains("Fav"))
					{
						result.DataResult.Fav = AppTools.ListFilter(new List<VideoDataObject>(result.DataResult.Fav));
						respondList = result.DataResult.Fav.Count;
					}
					else if (TypeVideo.Contains("Live"))
					{
						result.DataResult.Live = AppTools.ListFilter(new List<VideoDataObject>(result.DataResult.Live));
						respondList = result.DataResult.Live.Count;
					}
					if (respondList > 0)
					{
						if (countList > 0)
						{
							if (TypeVideo.Contains("Top"))
							{
								foreach (var item in from item in result.DataResult.Top let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
								{
									MAdapter.VideoList.Add(item);
								}
							}
							else if (TypeVideo.Contains("Latest"))
							{
								foreach (var item in from item in result.DataResult.Latest let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
								{
									MAdapter.VideoList.Add(item);
								}
							}
							else if (TypeVideo.Contains("Fav"))
							{
								foreach (var item in from item in result.DataResult.Fav let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
								{
									MAdapter.VideoList.Add(item);
								}
							}
							else if (TypeVideo.Contains("Live"))
							{
								foreach (var item in from item in result.DataResult.Live let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
								{
									MAdapter.VideoList.Add(item);
								}
							}
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
						else
						{
							if (TypeVideo.Contains("Top"))
							{
								MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.DataResult.Top);
							}
							else if (TypeVideo.Contains("Latest"))
							{
								MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.DataResult.Latest);
							}
							else if (TypeVideo.Contains("Fav"))
							{
								MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.DataResult.Fav);
							}
							else if (TypeVideo.Contains("Live"))
							{
								MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.DataResult.Live);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.VideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
				}

				Activity?.RunOnUiThread(ShowEmptyPage);
			}
			else
			{
				Inflated = EmptyStateLayout?.Inflate();
				EmptyStateInflater x = new EmptyStateInflater();
				x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
				if (!x.EmptyStateButton.HasOnClickListeners)
				{
					x.EmptyStateButton.Click += null;
					x.EmptyStateButton.Click += EmptyStateButtonOnClick;
				}

				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.VideoList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					if (Inflated == null)
						Inflated = EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoVideo);
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
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;
				Methods.DisplayReportResultTrack(e);
			}
		}

		//No Internet Connection 
		private void EmptyStateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				GetVideoByType();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion
	}
}