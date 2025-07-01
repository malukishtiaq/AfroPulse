using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.Playlist.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Library
{
	public class PlayListsVideosFragment : RecyclerViewDefaultBaseFragment
	{
		#region Variables Basic

		public PlayListsAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout;
		private View Inflated;
		private RecyclerViewOnScrollListener MainScrollEvent;
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
				InitToolbar(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;

				//Get Data Api
				GetPlayListsVideos();

				AdsGoogle.Ad_RewardedInterstitial(Activity);
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
				MAdapter = new PlayListsAdapter(Activity)
				{
					PlayListsList = new ObservableCollection<PlayListVideoObject>()
				};
				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<PlayListVideoObject>(Activity, MAdapter, sizeProvider, 10);
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
					string title = Context.GetString(Resource.String.Lbl_PlayLists_Videos);
					GlobalContext.SetToolBar(toolbar, title);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events


		private void MAdapterOnItemClick(object sender, PlayListsAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				Bundle bundle = new Bundle();
				bundle.PutString("ItemPlayList", JsonConvert.SerializeObject(item));
				bundle.PutString("Name_PlayList", item.Name);
				SubPlayListsVideosFragment fragment = new SubPlayListsVideosFragment { Arguments = bundle };
				GlobalContext.FragmentBottomNavigator.DisplayFragment(fragment);
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
				//Get Data Api
				MAdapter.PlayListsList.Clear();
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
				var item = MAdapter.PlayListsList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id.ToString());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		private void GetPlayListsVideos()
		{
			try
			{
				if (ListUtils.PlayListVideoObjectList.Count > 0)
				{
					MAdapter.PlayListsList = ListUtils.PlayListVideoObjectList;
					MAdapter.NotifyDataSetChanged();
				}

				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

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
				int countList = MAdapter.PlayListsList.Count;

				var (apiStatus, respond) = await RequestsAsync.Playlist.GetMyPlaylistAsync(UserDetails.UserId, offset, "25");
				if (apiStatus != 200 || respond is not GetPlaylistObject result || result.AllPlaylist == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.AllPlaylist.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.AllPlaylist let check = MAdapter.PlayListsList.FirstOrDefault(a => a.ListId == item.ListId) where check == null select item)
							{
								MAdapter.PlayListsList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.PlayListsList.Count - countList); });
						}
						else
						{
							MAdapter.PlayListsList = new ObservableCollection<PlayListVideoObject>(result.AllPlaylist);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.PlayListsList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlayList), ToastLength.Short)?.Show();
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
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				if (MAdapter.PlayListsList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;

					ListUtils.PlayListVideoObjectList = MAdapter.PlayListsList;

					var dataPlaylist = ListUtils.PlayListVideoObjectList.FirstOrDefault();
					if (dataPlaylist != null)
					{
						var item = ListUtils.LibraryList.FirstOrDefault(a => a.SectionId == "5");
						if (item == null) return;
						item.VideoCount = item.VideoCount++;

						item.BackgroundImage = dataPlaylist.Thumbnail;

						var sqlEntity = new SqLiteDatabase();
						sqlEntity.InsertLibraryItem(item);
					}

				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					if (Inflated == null)
						Inflated = EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoPlayLists);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
					}
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
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
	}
}