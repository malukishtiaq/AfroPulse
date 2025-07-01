using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using PlayTube.Activities.Base;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Library.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Adapters;
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

namespace PlayTube.Activities.Library
{
	public class SubscriptionsFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		private SubscriptionsAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		public AllChannelSubscribedFragment AllChannelSubscribedFragment;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Create your fragment here
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

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

				//Get Data Api
				Task.Factory.StartNew(StartApiService);
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
				MAdapter = new SubscriptionsAdapter(Activity)
				{
					SubscriptionsList = new ObservableCollection<Classes.SubscriptionsClass>()
				};

				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<Classes.SubscriptionsClass>(Activity, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);
				MAdapter.VideoItemClick += MAdapterOnVideoItemClick;

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

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_Subscriptions), toolbarTitle);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void MAdapterOnVideoItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item.VideoData == null) return;

				GlobalContext.StartPlayVideo(item.VideoData);
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
				MAdapter.SubscriptionsList.Clear();
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
				if (!Methods.CheckConnectivity())
					return;

				//Code get last id where LoadMore >>
				var checkList = MAdapter.SubscriptionsList.LastOrDefault(q => q.Type == ItemType.BigVideo);
				if (MainScrollEvent != null && checkList?.VideoData != null && !string.IsNullOrEmpty(checkList.VideoData?.Id) && !MainScrollEvent.IsLoading)
				{
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(checkList.VideoData.Id) });
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void StartApiService()
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDataChannelAsync, () => LoadDataAsync() });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;

				var (apiStatus, respond) = await RequestsAsync.Video.GetSubscriptionsVideosOrChannelAsync("0", offset, "10");
				if (apiStatus != 200 || respond is not GetVideosListDataObject result || result.VideoList == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.VideoList.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);

						foreach (var users in from item in result.VideoList
											  let check = MAdapter.SubscriptionsList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId)
											  where check == null
											  select new Classes.SubscriptionsClass
											  {
												  Id = Convert.ToInt32(item.Id),
												  VideoData = item,
												  Type = ItemType.BigVideo
											  })
						{
							MAdapter.SubscriptionsList.Add(users);
							AdapterHolders.AddAds(MAdapter, users.Type);
						}
					}
					else
					{
						if (MAdapter.SubscriptionsList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

		private async Task LoadDataChannelAsync()
		{
			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Video.GetSubscriptionsVideosOrChannelAsync("1", "0", "10");
				if (apiStatus != 200 || respond is not GetSubscriptionsChannelObject result || result.ChannelsList == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.ChannelsList.Count;
					if (respondList > 0)
					{
						var checkList = MAdapter.SubscriptionsList.FirstOrDefault(q => q.Type == ItemType.Channel);
						if (checkList == null)
						{
							var channel = new Classes.SubscriptionsClass
							{
								Id = 200,
								ChannelList = new List<UserDataObject>(),
								Type = ItemType.Channel
							};

							foreach (var item in from item in result.ChannelsList let check = channel.ChannelList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								channel.ChannelList.Add(item);
							}

							MAdapter.SubscriptionsList.Insert(0, channel);
						}
						else
						{
							foreach (var item in from item in result.ChannelsList let check = checkList.ChannelList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								checkList.ChannelList.Add(item);
							}
						}
					}
				}

				//Activity?.RunOnUiThread(ShowEmptyPage);
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
		}

		private void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.SubscriptionsList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;

					var checkList = MAdapter.SubscriptionsList.FirstOrDefault(q => q.Type == ItemType.BigVideo);
					if (checkList != null)
					{
						var emptyStateChecker = MAdapter.SubscriptionsList.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
						if (emptyStateChecker != null)
						{
							MAdapter.SubscriptionsList.Remove(emptyStateChecker);
						}
					}

					GlobalContext.LibrarySynchronizer.AddToLiked(MAdapter.SubscriptionsList.FirstOrDefault()?.VideoData, MAdapter.SubscriptionsList.Count);

					MAdapter.NotifyDataSetChanged();
				}
				else
				{
					var emptyStateChecker = MAdapter.SubscriptionsList.FirstOrDefault(q => q.Type == ItemType.EmptyPage);
					if (emptyStateChecker == null)
					{
						MAdapter.SubscriptionsList.Add(new Classes.SubscriptionsClass
						{
							Id = 300,
							Type = ItemType.EmptyPage
						});
						MAdapter.NotifyDataSetChanged();

						EmptyStateLayout.Visibility = ViewStates.Gone;
					}
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
				Task.Factory.StartNew(StartApiService);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion
	}
}