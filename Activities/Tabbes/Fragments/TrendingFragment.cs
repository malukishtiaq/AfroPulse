using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using PlayTube.Activities.Article;
using PlayTube.Activities.ChannelPopular;
using PlayTube.Activities.Movies;
using PlayTube.Activities.Tabbes.Adapters;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Activities.Tabbes.Fragments
{
	public class TrendingFragment : Fragment
	{
		#region Variables Basic

		private ImageView AppLogo;
		public ImageView ProfileButton, NotificationButton, SearchButton, ChatButton;

		private LinearLayout HeaderTrendingLayout;
		private TrendingAdapter MAdapter;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private CardView ChannelCard, MoviesCard, ArticleCard;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;
		private NestedScrollViewOnScroll MainScrollEvent;
		private TabbedMainActivity GlobalContext;

		public AllChannelPopularFragment AllChannelPopularFragment;
		private MoviesFragment MoviesFragment;
		private ArticlesFragment ArticlesFragment;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = (TabbedMainActivity)Activity;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.TTrendingLayout, container, false);
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
				SetRecyclerViewAdapters(view);

				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
				ChannelCard.Click += ChannelCardOnClick;
				MoviesCard.Click += MoviesCardOnClick;
				ArticleCard.Click += ArticleCardOnClick;

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
				AppLogo = (ImageView)view.FindViewById(Resource.Id.app_logo);
				AppLogo.SetImageResource(AppTools.IsTabDark() ? Resource.Drawable.logolight : Resource.Drawable.logo);

				ChatButton = (ImageView)view.FindViewById(Resource.Id.chatButton);
				ChatButton.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);
				ChatButton.Click += GlobalContext.ChatButtonOnClick;

				if (!UserDetails.IsLogin)
					ChatButton.Visibility = ViewStates.Gone;

				SearchButton = (ImageView)view.FindViewById(Resource.Id.searchButton);
				SearchButton.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);
				SearchButton.Click += GlobalContext.SearchButtonOnClick;

				NotificationButton = (ImageView)view.FindViewById(Resource.Id.notificationButton);
				NotificationButton.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);
				NotificationButton.Click += GlobalContext.NotificationButtonOnClick;

				ProfileButton = (ImageView)view.FindViewById(Resource.Id.profileButton);
				ProfileButton.Click += GlobalContext.ProfileButtonOnClick;
				GlideImageLoader.LoadImage(Activity, UserDetails.Avatar, ProfileButton, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				if (!UserDetails.IsLogin)
				{
					NotificationButton.Visibility = ViewStates.Gone;
					ProfileButton.Visibility = ViewStates.Gone;
				}

				HeaderTrendingLayout = (LinearLayout)view.FindViewById(Resource.Id.HeaderTrendingLayout);
				ChannelCard = (CardView)view.FindViewById(Resource.Id.ChannelCard);
				MoviesCard = (CardView)view.FindViewById(Resource.Id.MoviesCard);
				ArticleCard = (CardView)view.FindViewById(Resource.Id.ArticleCard);

				MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
				EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

				SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				if (!UserDetails.IsLogin)
				{
					HeaderTrendingLayout.Visibility = ViewStates.Gone;
				}

				if (!AppSettings.ShowChannelPopular)
					ChannelCard.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowArticle)
					ArticleCard.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowMovies)
					MoviesCard.Visibility = ViewStates.Gone;
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

		private void SetRecyclerViewAdapters(View view)
		{
			try
			{
				MAdapter = new TrendingAdapter(Activity)
				{
					TrendingList = new ObservableCollection<Classes.TrendingClass>()
				};
				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<Classes.TrendingClass>(Activity, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);
				MAdapter.VideoItemClick += MAdapterOnVideoItemClick;
				MAdapter.BigVideoItemClick += MAdapterOnVideoItemClick;

				var nestedScrollView = view.FindViewById<NestedScrollView>(Resource.Id.nested_scroll_view);

				NestedScrollViewOnScroll recyclerViewOnScrollListener = new NestedScrollViewOnScroll();
				MainScrollEvent = recyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				nestedScrollView.SetOnScrollChangeListener(recyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

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

		#endregion

		#region Events

		private void ArticleCardOnClick(object sender, EventArgs e)
		{
			try
			{
				ArticlesFragment = new ArticlesFragment();
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(ArticlesFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MoviesCardOnClick(object sender, EventArgs e)
		{
			try
			{
				MoviesFragment = new MoviesFragment();
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(MoviesFragment);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ChannelCardOnClick(object sender, EventArgs e)
		{
			try
			{
				AllChannelPopularFragment = new AllChannelPopularFragment();
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(AllChannelPopularFragment);
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
				MAdapter.TrendingList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;
				Task.Factory.StartNew(StartApiService);
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
				var checkList = MAdapter.TrendingList.LastOrDefault(q => q.Type == ItemType.RowVideo || q.Type == ItemType.BigVideo);
				if (MainScrollEvent != null && checkList?.VideoData != null && !string.IsNullOrEmpty(checkList.VideoData?.Id) && !MainScrollEvent.IsLoading)
				{
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(checkList.VideoData?.Id) });
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
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync() });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;

				var (apiStatus, respond) = await RequestsAsync.Video.GetTrendingVideosAsync(offset, "10");
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
											  let check = MAdapter.TrendingList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId)
											  where check == null
											  select new Classes.TrendingClass
											  {
												  Id = Convert.ToInt32(item.Id),
												  VideoData = item,
												  Type = ItemType.BigVideo
											  })
						{
							MAdapter.TrendingList.Add(users);
							AdapterHolders.AddAds(MAdapter, ItemType.BigVideo);
						}
					}
					else
					{
						if (MAdapter.TrendingList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

				if (MAdapter.TrendingList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;

					var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.BigVideo || q.Type == ItemType.RowVideo);
					if (checkList != null)
					{
						var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
						if (emptyStateChecker != null)
						{
							MAdapter.TrendingList.Remove(emptyStateChecker);
						}
					}

					MAdapter.NotifyDataSetChanged();
				}
				else
				{
					var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.EmptyPage);
					if (emptyStateChecker == null)
					{
						MAdapter.TrendingList.Add(new Classes.TrendingClass
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