using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.Playlist.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Activities.Channel.Tab
{
	public class ChPlayListFragment : Fragment
	{
		#region Variables Basic

		public PlayListsAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		public RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		public ViewStub EmptyStateLayout, ShimmerPageLayout;
		public View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
		private string IdChannel = "";

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
				View view = inflater?.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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
				IdChannel = Arguments?.GetString("ChannelId");

				//Get Value And Set Toolbar
				InitComponent(view);
				InitShimmer(view);
				SetRecyclerViewAdapters();

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;
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
				ShimmerInflater?.Show();

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
				if (item != null && !string.IsNullOrEmpty(item.ListId) && !MainScrollEvent.IsLoading)
					StartApiService(item.ListId);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data Api 

		public void StartApiService(string offset = "0")
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

				var (apiStatus, respond) = await RequestsAsync.Playlist.GetMyPlaylistAsync(IdChannel, offset, "25");
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
						foreach (var item in from item in result.AllPlaylist let check = MAdapter.PlayListsList.FirstOrDefault(a => a.ListId == item.ListId) where check == null select item)
						{
							if (IdChannel == UserDetails.UserId)
							{
								MAdapter.PlayListsList.Add(item);
							}
							else
							{
								if (item.Privacy == 1)
									MAdapter.PlayListsList.Add(item);
							}
						}

						if (countList > 0)
						{
							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.PlayListsList.Count - countList); });
						}
						else
						{
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

		public void ShowEmptyPage()
		{
			try
			{
				ShimmerInflater?.Hide();
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;

				if (GlobalContext.UserChannelFragment != null && GlobalContext.UserChannelFragment.SubscribeChannelButton?.Tag?.ToString() == "PaidSubscribe" && IdChannel != UserDetails.UserId)
				{
					Inflated ??= EmptyStateLayout?.Inflate();
					GlobalContext.UserChannelFragment.SetEmptyPageSubscribeChannelWithPaid(EmptyStateLayout, Inflated);
				}
				else
				{
					if (MAdapter.PlayListsList.Count > 0)
					{
						MRecycler.Visibility = ViewStates.Visible;
						EmptyStateLayout.Visibility = ViewStates.Gone;
					}
					else
					{
						MRecycler.Visibility = ViewStates.Gone;

						Inflated ??= EmptyStateLayout?.Inflate();

						EmptyStateInflater x = new EmptyStateInflater();
						x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoPlayLists);
						if (!x.EmptyStateButton.HasOnClickListeners)
						{
							x.EmptyStateButton.Click += null;
						}
						EmptyStateLayout.Visibility = ViewStates.Visible;
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