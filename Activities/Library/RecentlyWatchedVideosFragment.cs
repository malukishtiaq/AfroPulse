using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
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
using PlayTube.SQLite;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Library
{
	public class RecentlyWatchedVideosFragment : RecyclerViewDefaultBaseFragment, IDialogListCallBack
    {
		#region Variables Basic

		private CnVideoAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private TabbedMainActivity GlobalContext;
        private FrameLayout MoreButton;
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

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				MAdapter.ItemClick += MAdapterOnItemClick;

				//Get Data Api
				Task.Factory.StartNew(() => StartApiService());

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

               MoreButton = view.FindViewById<FrameLayout>(Resource.Id.toolbar_more);
               MoreButton.Visibility = ViewStates.Visible;
               MoreButton.Click += MoreButtonOnClick;
				 
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
				MAdapter = new CnVideoAdapter(Activity)
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
				var toolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbar_title);
				GlobalContext.SetToolBar(toolbar, GetText(Resource.String.Lbl_RecentlyWatched), toolbarTitle);
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

        private void MoreButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

                arrayAdapter.Add(GetString(Resource.String.Lbl_PauseWatchHistory));
                arrayAdapter.Add(GetString(Resource.String.Lbl_ClearHistory)); 

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, ChVideoAdapterClickEventArgs e)
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

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_PauseWatchHistory))
                {
                    PauseWatchHistoryEvent();
                }
                else if (text == GetString(Resource.String.Lbl_ClearHistory))
                {
                    ClearHistoryEvent();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ClearHistoryEvent()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialAlertDialogBuilder(Context);
                dialogBuilder.SetTitle(GetText(Resource.String.Lbl_ClearHistory));
                dialogBuilder.SetMessage(GetText(Resource.String.Lbl_AreYouSureClearHistory));
                dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (sender, args) =>
                {
                    try
                    {
                        new LibrarySynchronizer(Activity).RemoveRecentlyWatched();

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.DeleteHistoryVideosAsync() });
                        else
                            Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();

                        MAdapter.VideoList.Clear();
                        MAdapter.NotifyDataSetChanged();

                        MainScrollEvent.IsLoading = false;

                        var page = TabbedMainActivity.GetInstance()?.LibraryFragment;
                        if (page != null)
                        {
                            page.MAdapterRecently?.VideoList?.Clear();
                            page.MAdapterRecently?.NotifyDataSetChanged();

                            page.RecentlyViewStub.Visibility = ViewStates.Gone;
                        }
                        Activity?.RunOnUiThread(ShowEmptyPage);

                        Toast.MakeText(Context, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());
                dialogBuilder.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void PauseWatchHistoryEvent()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var dictionary = new Dictionary<string, string>
                    {
                        {"settings_type", "pause_history"},
                    };

                    var (apiResult, respond) = await RequestsAsync.Global.UpdateUserDataGeneralAsync(dictionary);
                    if (apiResult == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyChannelList?.FirstOrDefault();
                            if (local != null)
                            {
                                if (local.PauseHistory == "1")
                                {
                                    local.PauseHistory = "0";
                                    UserDetails.IsPauseWatchHistory = false;

                                    Toast.MakeText(Context, GetText(Resource.String.Lbl_PausedHistory), ToastLength.Long)?.Show();
                                }
                                else if (local.PauseHistory == "0")
                                {
                                    local.PauseHistory = "1";
                                    UserDetails.IsPauseWatchHistory = true;
                                    Toast.MakeText(Context, GetText(Resource.String.Lbl_ResumeHistory), ToastLength.Long)?.Show();
                                }

                                var database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyChannel(local); 
                            } 
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(Activity, respond);
                    }
                }
                else
                {
                    Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Code get last id where LoadMore >>
				//video "history_id" will be used as offset for load more instead of video "id"
				var item = MAdapter.VideoList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.HistoryId) && !MainScrollEvent.IsLoading)
					StartApiService(item.HistoryId);
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
			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Video.GetHistoryVideosAsync("10", offset);
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
					int countList = MAdapter.VideoList.Count;
					var respondList = result.VideoList.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);
						if (countList > 0)
						{
							foreach (var item in from item in result.VideoList let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
							{
								MAdapter.VideoList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.VideoList.Count - countList); });
						}
						else
						{
							MAdapter.VideoList = new ObservableCollection<VideoDataObject>(result.VideoList);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.VideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
				}

				GlobalContext.LibrarySynchronizer.AddToRecentlyWatched(MAdapter.VideoList.FirstOrDefault(), MAdapter.VideoList.Count);

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
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoRecentlyWatched);
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
	}
}