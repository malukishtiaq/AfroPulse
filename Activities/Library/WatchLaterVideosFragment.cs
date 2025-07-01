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
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Library
{
	public class WatchLaterVideosFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		public WatchLaterVideoRowAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private TabbedMainActivity GlobalContext;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private static WatchLaterVideosFragment Instance;
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

				Instance = this;

				//Get Value And Set Toolbar 
				InitComponent(view);
				InitShimmer(view);
				InitToolbar(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;

				//Get Data  
				Task.Factory.StartNew(() => StartApiService());
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

		public override void OnDestroy()
		{
			try
			{
				Instance = null;
				base.OnDestroy();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
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

				ShowGoogleAds(view, MRecycler);
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
				MAdapter = new WatchLaterVideoRowAdapter(Activity)
				{
					VideoList = new ObservableCollection<DataWatchLaterVideos>()
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
				var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_WatchLater_Videos), toolbarTitle);
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

		private void MAdapterOnItemClick(object sender, WatchLaterVideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item.Videos?.VideoAdClass);
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

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.VideoList.Count;

				var (apiStatus, respond) = await RequestsAsync.Video.WatchLaterVideosAsync("10", offset);
				if (apiStatus != 200 || respond is not WatchLaterVideosListObject result || result.Data == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					result.Data.RemoveAll(a => a.Videos?.VideoAdClass == null);

					var respondList = result.Data.Count;
					if (respondList > 0)
					{
						//var list = AppTools.ListFilter((from data in result.Data where data.Videos?.VideoAdClass != null select data.Videos?.VideoAdClass).ToList());

						foreach (var item in from item in result.Data let check = MAdapter.VideoList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
						{
							if (item.Videos?.VideoAdClass == null)
								continue;

							item.Videos = new VideoUnion()
							{
								VideoAdClass = AppTools.VideoFilter(item.Videos?.VideoAdClass)
							};

							MAdapter.VideoList.Add(item);
						}

						if (countList > 0)
						{
							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.VideoList.Count - countList); });
						}
						else
						{
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.VideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
				}

				GlobalContext.LibrarySynchronizer.SetWatchLater(MAdapter.VideoList.FirstOrDefault()?.Videos?.VideoAdClass, MAdapter.VideoList.Count);

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

		public void ShowEmptyPage()
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
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoWatchLater);
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
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		public static WatchLaterVideosFragment GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
	}
}