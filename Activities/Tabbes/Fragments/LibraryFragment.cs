using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Library;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.Playlist.Adapters;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Videos;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidX.SwipeRefreshLayout.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Tabbes.Fragments
{
	public class LibraryFragment : Fragment
	{
		#region Variables Basic

		private TabbedMainActivity GlobalContext;

		public VideoHorizontalSmallAdapter MAdapterRecently;
		public ViewStub RecentlyViewStub;
		private View RecentlyInflated;
		private FrameLayout IconSettings;
		private LinearLayout ViewProfile, SubscriptionLayout, WatchLaterLayout, WatchOfflineLayout, LikedVideosLayout, PaidVideosLayout, SharedVideosLayout, NewPlaylistLayout;
		private TextView UserName;
		private ImageView UserPic;
		private TextView SubscriptionsCount, WatchLaterCount, LikedVideosCount, PaidVideosCount, SharedVideosCount;
		public TextView WatchOfflineCount;

        private SwipeRefreshLayout SwipeRefreshLayout;


        public PlayListsRowAdapter MAdapter;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private NestedScrollViewOnScroll MainScrollEvent;


		public SubscriptionsFragment SubscriptionsFragment;
		public WatchLaterVideosFragment WatchLaterVideosFragment;
		private RecentlyWatchedVideosFragment RecentlyWatchedVideosFragment;
		private WatchOfflineVideosFragment WatchOfflineVideosFragment;
		private LikedVideosFragment LikedVideosFragment;
		private SharedVideosFragment SharedVideosFragment;
		private PaidVideosFragment PaidVideosFragment;

		private PopupFilterList PopupFilterList;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				GlobalContext = (TabbedMainActivity)Activity;
				HasOptionsMenu = true;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.TLibraryLayout, container, false);
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
				//InitToolbar(view);
				SetRecyclerViewAdapters(view);
				GetLibrarySectionViews();

				PopupFilterList = new PopupFilterList(view, Activity, MAdapter);

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;

				Task.Factory.StartNew(() => StartApiService());
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
				ViewProfile = view.FindViewById<LinearLayout>(Resource.Id.view_profile);
				UserPic = view.FindViewById<ImageView>(Resource.Id.user_pic);
				UserName = view.FindViewById<TextView>(Resource.Id.user_name);
				ViewProfile.Click += View_profile_Click;

				IconSettings = view.FindViewById<FrameLayout>(Resource.Id.mySettings_icon);
				IconSettings.Click += IconSettingsOnClick;

				RecentlyViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubRecently);

				SubscriptionLayout = view.FindViewById<LinearLayout>(Resource.Id.subscriptionLayout);
				SubscriptionsCount = view.FindViewById<TextView>(Resource.Id.SubscriptionsCount);
				SubscriptionLayout.Click += SubscriptionLayoutOnClick;

				WatchLaterLayout = view.FindViewById<LinearLayout>(Resource.Id.WatchLaterLayout);
				WatchLaterCount = view.FindViewById<TextView>(Resource.Id.WatchLaterCount);
				WatchLaterLayout.Click += WatchLaterLayoutOnClick;

				WatchOfflineLayout = view.FindViewById<LinearLayout>(Resource.Id.WatchOfflineLayout);
				WatchOfflineCount = view.FindViewById<TextView>(Resource.Id.WatchOfflineCount);
				WatchOfflineLayout.Click += WatchOfflineLayoutOnClick;

				LikedVideosLayout = view.FindViewById<LinearLayout>(Resource.Id.LikedVideosLayout);
				LikedVideosCount = view.FindViewById<TextView>(Resource.Id.LikedVideosCount);
				LikedVideosLayout.Click += LikedVideosLayoutOnClick;

				PaidVideosLayout = view.FindViewById<LinearLayout>(Resource.Id.PaidVideosLayout);
				PaidVideosCount = view.FindViewById<TextView>(Resource.Id.PaidVideosCount);
				PaidVideosLayout.Click += PaidVideosLayoutOnClick;

				SharedVideosLayout = view.FindViewById<LinearLayout>(Resource.Id.SharedVideosLayout);
				SharedVideosCount = view.FindViewById<TextView>(Resource.Id.SharedVideosCount);
				SharedVideosLayout.Click += SharedVideosLayoutOnClick;

				NewPlaylistLayout = view.FindViewById<LinearLayout>(Resource.Id.NewPlaylistLayout);
				NewPlaylistLayout.Click += NewPlaylistLayoutOnClick;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
				 
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetRecyclerViewAdapters(View view)
		{
			try
			{
				MAdapter = new PlayListsRowAdapter(Activity)
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

				var nestedScrollView = view.FindViewById<NestedScrollView>(Resource.Id.nested_scroll_view);

				NestedScrollViewOnScroll recyclerViewOnScrollListener = new NestedScrollViewOnScroll();
				MainScrollEvent = recyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				nestedScrollView.SetOnScrollChangeListener(recyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
				 
				//=================

				MAdapterRecently = new VideoHorizontalSmallAdapter(Activity)
				{
					VideoList = new ObservableCollection<VideoDataObject>()
				};
				MAdapterRecently.ItemClick += MAdapterRecentlyOnItemClick;
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
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_Library), null, false);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

        #endregion

        #region Events

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                //Get Data Api
                MAdapterRecently.VideoList.Clear();
                MAdapterRecently.NotifyDataSetChanged();
				 
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

        private void IconSettingsOnClick(object sender, EventArgs e)
		{
			try
			{
				Context.StartActivity(new Intent(Context, typeof(SettingsActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void NewPlaylistLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				Context.StartActivity(new Intent(Context, typeof(CreateNewPlaylistActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void View_profile_Click(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					GlobalContext.MyChannelFragment = new MyChannelFragment();
					GlobalContext?.FragmentBottomNavigator.DisplayFragment(GlobalContext?.MyChannelFragment);
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
					dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterRecentlyOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapterRecently.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
		{
			try
			{
				var item = MAdapter.PlayListsList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPlaylistAsync(item.Id.ToString()) });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MAdapterOnItemClick(object sender, PlayListsRowAdapterClickEventArgs e)
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

		private void WatchLaterLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				WatchLaterVideosFragment = new WatchLaterVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(WatchLaterVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void WatchOfflineLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				WatchOfflineVideosFragment = new WatchOfflineVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(WatchOfflineVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LikedVideosLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				LikedVideosFragment = new LikedVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(LikedVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void PaidVideosLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				PaidVideosFragment = new PaidVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(PaidVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SharedVideosLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				SharedVideosFragment = new SharedVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(SharedVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SubscriptionLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				SubscriptionsFragment = new SubscriptionsFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(SubscriptionsFragment);
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
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadRecentlyAsync, () => LoadPlaylistAsync(offset) });
		}

		private async Task LoadRecentlyAsync()
		{
			if (!AppSettings.AllowRecentlyWatched)
				return;

			if (Methods.CheckConnectivity())
			{
				int countList = MAdapterRecently.VideoList.Count;

				var (apiStatus, respond) = await RequestsAsync.Video.GetHistoryVideosAsync("10", "0");
				if (apiStatus != 200 || respond is not GetVideosListDataObject result || result.VideoList == null)
				{
					if (respond is ErrorObject errorMessage)
					{
						if (errorMessage.errors.ErrorText != "Bad Request, Invalid or missing parameter")
							Methods.DisplayReportResult(Activity, respond);
					}
					else
						Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					result.VideoList = AppTools.ListFilter(result.VideoList);

					var respondList = result.VideoList.Count;
					if (respondList > 0)
					{
						if (countList > 0)
						{
							foreach (var item in from item in result.VideoList let check = MAdapterRecently.VideoList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								MAdapterRecently.VideoList.Add(item);
							}

							Activity?.RunOnUiThread(() =>
							{
								MAdapterRecently.NotifyItemRangeInserted(countList, MAdapterRecently.VideoList.Count - countList);
							});
						}
						else
						{
							MAdapterRecently.VideoList = new ObservableCollection<VideoDataObject>(result.VideoList);

							Activity?.RunOnUiThread(() =>
							{
								RecentlyInflated ??= RecentlyViewStub.Inflate();

								TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
								recyclerInflater.InflateLayout<UserDataObject>(Activity, RecentlyInflated, MAdapterRecently, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_RecentlyWatched), Resource.Drawable.pif_recent_history, true);
								if (!recyclerInflater.MainLinear.HasOnClickListeners)
								{
									recyclerInflater.MainLinear.Click += null;
									recyclerInflater.MainLinear.Click += RecentlyMoreOnClick;
								}
							});
						}
					}

					Activity?.RunOnUiThread(() =>
					{
						try
						{
                            SwipeRefreshLayout.Refreshing = false;

                            if (MAdapterRecently.VideoList.Count > 0)
                                RecentlyViewStub.Visibility = ViewStates.Visible;

                        }
                        catch (Exception exception)
                        {
                            SwipeRefreshLayout.Refreshing = false;
                            Methods.DisplayReportResultTrack(exception);
                        }
					});
				}
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		private void RecentlyMoreOnClick(object sender, EventArgs e)
		{
			try
			{
				RecentlyWatchedVideosFragment = new RecentlyWatchedVideosFragment();
				GlobalContext.FragmentBottomNavigator.DisplayFragment(RecentlyWatchedVideosFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async Task LoadPlaylistAsync(string offset = "0")
		{
			if (!AppSettings.AllowPlayLists || MainScrollEvent.IsLoading)
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

					Activity?.RunOnUiThread(() =>
					{
						try
						{
                            SwipeRefreshLayout.Refreshing = false;
							if (MAdapter.PlayListsList.Count > 0)
							{
								PopupFilterList.TopLayout.Visibility = ViewStates.Visible;
								MRecycler.Visibility = ViewStates.Visible;

								PopupFilterList.CheckType(UserDetails.PopupFilterPlaylistValue);

								ListUtils.PlayListVideoObjectList = MAdapter.PlayListsList;
							}
							else
							{
								PopupFilterList.TopLayout.Visibility = ViewStates.Gone;
								MRecycler.Visibility = ViewStates.Gone;
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
                            SwipeRefreshLayout.Refreshing = false;
                        }
					});
				}
				//Activity?.RunOnUiThread(ShowEmptyPage);
			}
			else
			{
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		#endregion

		private async void GetLibrarySectionViews()
		{
			try
			{
				if (ListUtils.MyChannelList.Count == 0)
					await ApiRequest.GetChannelData(Activity, UserDetails.UserId);

				var data = ListUtils.MyChannelList.FirstOrDefault();
				if (data != null)
				{
					GlideImageLoader.LoadImage(Activity, data.Avatar, UserPic, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					UserName.Text = AppTools.GetNameFinal(data);
				}

				GetVideoCount();
				//SubscriptionLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.AllowWatchLater)
					WatchLaterLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.AllowOfflineDownload)
					WatchOfflineLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.AllowPlayLists)
				{
					NewPlaylistLayout.Visibility = ViewStates.Gone;
					MRecycler.Visibility = ViewStates.Gone;
					PopupFilterList.TopLayout.Visibility = ViewStates.Gone;
				}

				if (!AppSettings.AllowLiked)
					LikedVideosLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.AllowShared)
					SharedVideosLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.AllowPaid)
					PaidVideosLayout.Visibility = ViewStates.Gone;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void GetVideoCount()
		{
			try
			{
				var sqlEntity = new SqLiteDatabase();
				var check = sqlEntity.Get_LibraryItem();
				if (check?.Count > 0)
				{
					foreach (var item in check)
					{
						switch (item.SectionId)
						{
							//Subscriptions
							case "1":
								SubscriptionsCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_Subscriptions) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
							//WatchLater
							case "2":
								WatchLaterCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_WatchLater) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
							//RecentlyWatched
							case "3":
								break;
							//WatchOffline
							case "4":
								WatchOfflineCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_WatchOffline) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
							//PlayLists
							case "5":
								break;
							//Liked
							case "6":
								LikedVideosCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_Liked) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
							//Shared
							case "7":
								SharedVideosCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_Shared) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
							//Paid
							case "8":
								PaidVideosCount.Text = item.VideoCount == 0 ? GetText(Resource.String.Lbl_Library_Des_Paid) : item.VideoCount + " " + GetText(Resource.String.Lbl_Videos);
								break;
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