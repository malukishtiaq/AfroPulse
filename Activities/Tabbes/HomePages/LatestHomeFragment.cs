using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using PlayTube.Activities.Tabbes.Adapters;
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

namespace PlayTube.Activities.Tabbes.HomePages
{
	public class LatestHomeFragment : AndroidX.Fragment.App.Fragment
	{
		#region Variables Basic

		private TabbedMainActivity GlobalContext;

		private SwipeRefreshLayout SwipeRefreshLayout;
		public RecyclerView MRecycler;
		private ViewStub EmptyStateLayout, ShimmerPageLayout;
		private View Inflated, InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;

		public MainVideoAdapter MAdapter;
		private LinearLayoutManager LayoutManager;

		private RecyclerViewOnScrollListener MainScrollEvent;
		private readonly List<VideoDataObject> OtherVideosList = new List<VideoDataObject>();
		private bool MIsVisibleToUser;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				GlobalContext = (TabbedMainActivity)Activity;
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
				View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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
				InitComponent(view);
				InitShimmer(view);
				SetRecyclerViewAdapters();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void SetMenuVisibility(bool menuVisible)
		{
			try
			{
				base.SetMenuVisibility(menuVisible);
				MIsVisibleToUser = menuVisible;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnResume()
		{
			try
			{
				base.OnResume();

				if (IsResumed && MIsVisibleToUser)
				{
					Task.Factory.StartNew(() => StartApiService());
				}
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

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#282828") : Color.ParseColor("#f7f7f7"));
				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

				MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recyler);

				EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

				if (AdsGoogle.NativeAdsPool?.Count <= 3)
				{
					AdsGoogle.AdMobNative ads = new AdsGoogle.AdMobNative();
					ads.BindAdMobNative(Activity);
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
				MAdapter = new MainVideoAdapter(Activity)
				{
					MainVideoList = new ObservableCollection<Classes.MainVideoClass>()
				};
				LayoutManager = new LinearLayoutManager(Context);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<Classes.MainVideoClass>(Activity, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);
				MAdapter.OtherVideosItemClick += OnVideoItemClick;

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

		#region Event

		private string IdLastTop = "0";
		private string IdLastLatest = "0";
		private string IdLastFav = "0";
		private string IdLastLive = "0";

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
		{
			try
			{
				if (MainScrollEvent.IsLoading)
					return;

				string idFeatured = ListUtils.FeaturedVideosList.LastOrDefault()?.Id ?? "0";

				string idTop = "0";
				string idLatest = "0";
				string idFav = "0";
				string idLive = "0";
				string idOther = "0";

				var checkTopList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.TopVideos);
				if (checkTopList != null)
					idTop = checkTopList.TopVideoList.LastOrDefault()?.Id ?? "0";

				var checkLatestList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LatestVideos);
				if (checkLatestList != null)
					idLatest = checkLatestList.LatestVideoList.LastOrDefault()?.Id ?? "0";

				var checkFavList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.FavVideos);
				if (checkFavList != null)
					idFav = checkFavList.FavVideoList.LastOrDefault()?.Id ?? "0";

				var checkLiveList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LiveVideos);
				if (checkLiveList != null)
					idLive = checkLiveList.LiveVideoList.LastOrDefault()?.Id ?? "0";

				var checkOtherList = MAdapter.MainVideoList.LastOrDefault(q => q.Type == ItemType.OtherVideos);
				if (checkOtherList != null)
					idOther = checkOtherList?.VideoData?.Id ?? "0";

				if (Methods.CheckConnectivity())
				{
					if (IdLastTop == idTop && IdLastLatest == idLatest && IdLastFav == idFav && IdLastLive == idLive)
					{
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosByCategoryAsync(idOther) });
					}
					else
					{
						IdLastTop = idTop;
						IdLastLatest = idLatest;
						IdLastFav = idFav;
						IdLastLive = idLive;

						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadVideos(idFeatured, idTop, idLatest, idFav, idLive) });
					}
				}
				else
				{
					SwipeRefreshLayout.Refreshing = false;
					Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
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
				MainScrollEvent.IsLoading = false;

				ShimmerInflater?.Show();

				ListUtils.FeaturedVideosList.Clear();

				MAdapter.MainVideoList.Clear();
				MAdapter.Clear();

				MAdapter.NotifyDataSetChanged();

				EmptyStateLayout.Visibility = ViewStates.Gone;

				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Open Video from Other
		private void OnVideoItemClick(object sender, VideoAdapterClickEventArgs args)
		{
			try
			{
				if (args.Position <= -1) return;

				var item = MAdapter.GetItem(args.Position);
				if (item.VideoData == null) return;

				GlobalContext.StartPlayVideo(item.VideoData);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Load Data 

		private void AddShortVideos()
		{
			try
			{
				if (!AppSettings.ShowShorts)
					return;

				var respondList = ListUtils.VideoShortsList?.Count;
				if (respondList > 0)
				{
					var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.ShortVideos);
					if (checkList == null)
					{
						var shorts = new Classes.MainVideoClass
						{
							Id = 101,
							ShortsVideoList = new List<VideoDataObject>(),
							Type = ItemType.ShortVideos
						};

						foreach (var item in ListUtils.VideoShortsList)
						{
							if (item.Type != ItemType.ShortVideos)
								continue;

							var checkViewed = shorts.ShortsVideoList.FirstOrDefault(a => a.Id == item.VideoData?.Id);
							if (checkViewed == null)
							{
								shorts.ShortsVideoList.Add(item.VideoData);
							}
						}

						var checkTop = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.TopVideos);
						if (checkTop != null)
						{
							var index = MAdapter.MainVideoList.IndexOf(checkTop);
							MAdapter.MainVideoList.Insert(2, shorts);
							Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemInserted(2); });
						}
						else
						{
							MAdapter.MainVideoList.Insert(1, shorts);
							Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemInserted(1); });
						}
					}
					else
					{
						foreach (var item in from item in ListUtils.VideoShortsList let check = checkList.ShortsVideoList.FirstOrDefault(a => a.Id == item.VideoData?.Id) where check == null select item)
						{
							checkList.ShortsVideoList.Add(item.VideoData);
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void StartApiService(string featuredOffset = "0", string topOffset = "0", string latestOffset = "0", string favOffset = "0", string liveOffset = "0")
		{
			if (Methods.CheckConnectivity())
			{
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetNotInterestedVideos, () => LoadVideos(featuredOffset, topOffset, latestOffset, favOffset, liveOffset), GetStockVideosAsync });
			}
			else
			{
				SwipeRefreshLayout.Refreshing = false;
				Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
		}

		private async Task LoadVideos(string featuredOffset = "0", string topOffset = "0", string latestOffset = "0", string favOffset = "0", string liveOffset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;

				var (apiStatus, respond) = await RequestsAsync.Video.GetVideosAsync(featuredOffset, topOffset, latestOffset, favOffset, liveOffset, "12");
				if (apiStatus == 200)
				{
					await Task.Factory.StartNew(() =>
					{
						try
						{
							if (respond is GetVideosObject result)
							{
								if (result.DataResult.Featured?.Count > 0)
								{
									result.DataResult.Featured = AppTools.ListFilter(result.DataResult.Featured);

									foreach (var item in from item in result.DataResult.Featured let check = ListUtils.FeaturedVideosList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
									{
										if (item.IsShort == "1")
											continue;

										if (ListUtils.FeaturedVideosList.Count <= 5)
											ListUtils.FeaturedVideosList.Add(item);
										else
										{
											var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
											if (c == null)
												OtherVideosList.Add(item);
										}
									}
								}

								//Top
								var respondList = result.DataResult.Top?.Count;
								if (respondList > 0)
								{
									result.DataResult.Top = AppTools.ListFilter(result.DataResult.Top);

									var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.TopVideos);
									if (checkList == null)
									{
										var topVideos = new Classes.MainVideoClass
										{
											Id = 101,
											TopVideoList = new List<VideoDataObject>(),
											Type = ItemType.TopVideos
										};

										foreach (var item in from item in result.DataResult.Top let check = topVideos.TopVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (topVideos.TopVideoList.Count <= AppSettings.CountVideosTop)
												topVideos.TopVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}

										MAdapter.MainVideoList.Add(topVideos);
									}
									else
									{
										foreach (var item in from item in result.DataResult.Top let check = checkList.TopVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (checkList.TopVideoList.Count <= AppSettings.CountVideosTop)
												checkList.TopVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}
									}
								}

								//Latest
								var respondLatestList = result.DataResult.Latest?.Count;
								if (respondLatestList > 0)
								{
									result.DataResult.Latest = AppTools.ListFilter(result.DataResult.Latest);

									var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LatestVideos);
									if (checkList == null)
									{
										var latestVideos = new Classes.MainVideoClass
										{
											Id = 102,
											LatestVideoList = new List<VideoDataObject>(),
											Type = ItemType.LatestVideos
										};

										foreach (var item in from item in result.DataResult.Latest let check = latestVideos.LatestVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (latestVideos.LatestVideoList.Count <= AppSettings.CountVideosLatest)
												latestVideos.LatestVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}

										MAdapter.MainVideoList.Add(latestVideos);
									}
									else
									{
										foreach (var item in from item in result.DataResult.Latest let check = checkList.LatestVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (checkList.LatestVideoList.Count <= AppSettings.CountVideosLatest)
												checkList.LatestVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}
									}
								}

								//Fav
								var respondFavList = result.DataResult.Fav?.Count;
								if (respondFavList > 0)
								{
									result.DataResult.Fav = AppTools.ListFilter(result.DataResult.Fav);

									var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.FavVideos);
									if (checkList == null)
									{
										var favVideos = new Classes.MainVideoClass
										{
											Id = 103,
											FavVideoList = new List<VideoDataObject>(),
											Type = ItemType.FavVideos
										};

										foreach (var item in from item in result.DataResult.Fav let check = favVideos.FavVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (favVideos.FavVideoList.Count <= AppSettings.CountVideosFav)
												favVideos.FavVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}

										MAdapter.MainVideoList.Add(favVideos);
									}
									else
									{
										foreach (var item in from item in result.DataResult.Fav let check = checkList.FavVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (checkList.FavVideoList.Count <= AppSettings.CountVideosFav)
												checkList.FavVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}
									}
								}

								//Live
								var respondLiveList = result.DataResult.Live?.Count;
								if (respondLiveList > 0 && AppSettings.ShowGoLive)
								{
									//result.DataResult.Live = AppTools.ListFilter(result.DataResult.Live);

									var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LiveVideos);
									if (checkList == null)
									{
										var liveVideos = new Classes.MainVideoClass
										{
											Id = 105,
											LiveVideoList = new List<VideoDataObject>(),
											Type = ItemType.LiveVideos
										};

										foreach (var item in from item in result.DataResult.Live let check = liveVideos.LiveVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (liveVideos.LiveVideoList.Count <= AppSettings.CountVideosLive)
												liveVideos.LiveVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}

										MAdapter.MainVideoList.Add(liveVideos);
									}
									else
									{
										foreach (var item in from item in result.DataResult.Live let check = checkList.LiveVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
										{
											if (item.IsShort == "1")
												continue;

											if (checkList.LiveVideoList.Count <= AppSettings.CountVideosLive)
												checkList.LiveVideoList.Add(item);
											else
											{
												var c = OtherVideosList.FirstOrDefault(a => a.VideoId == item.VideoId);
												if (c == null)
													OtherVideosList.Add(item);
											}
										}
									}
								}

								//Other
								var respondOtherList = OtherVideosList?.Count;
								if (respondOtherList > 0)
								{
									foreach (var item in from item in OtherVideosList let check = MAdapter.MainVideoList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId) where check == null select item)
									{
										if (item.IsShort == "1")
											continue;

										MAdapter.MainVideoList.Add(new Classes.MainVideoClass
										{
											Id = Convert.ToInt32(item.Id),
											VideoData = item,
											Type = ItemType.OtherVideos
										});

										//AdapterHolders.AddAds(MAdapter, ItemType.BigVideo);
									}
								}
								else
								{
									if (OtherVideosList.Count > 10 && !MRecycler.CanScrollVertically(1))
										Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
								}

								if (MAdapter.MainVideoList.Count > 0)
									Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

								Activity?.RunOnUiThread(ShowEmptyPage);
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
							Activity?.RunOnUiThread(ShowEmptyPage);
						}
					});
				}
				else
				{
					Activity?.RunOnUiThread(ShowEmptyPage);
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(Activity, respond);
				}
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

				Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				Activity?.RunOnUiThread(ShowEmptyPage);
			}
		}

		private async Task GetStockVideosAsync()
		{
			if (!AppSettings.ShowStockVideo || !UserDetails.IsLogin)
				return;

			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Video.GetStockVideosAsync();
				if (apiStatus != 200 || respond is not GetVideosListObject result || result.VideoList == null)
				{
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.VideoList.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);

						var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.StockVideos);
						if (checkList == null)
						{
							var stockVideos = new Classes.MainVideoClass
							{
								Id = 106,
								StockVideoList = new List<VideoDataObject>(),
								Type = ItemType.StockVideos
							};

							foreach (var item in from item in result.VideoList let check = stockVideos.StockVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
							{
								if (stockVideos.StockVideoList.Count <= AppSettings.CountVideosStock)
									stockVideos.StockVideoList.Add(item);
								else
									break;
							}

							var checkTop = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LatestVideos);
							if (checkTop != null)
							{
								var index = MAdapter.MainVideoList.IndexOf(checkTop);
								MAdapter.MainVideoList.Insert(index + 1, stockVideos); 
								Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemInserted(index + 1); });
							}
							else
							{
								MAdapter.MainVideoList.Insert(1, stockVideos);
								Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemInserted(1); });
							}
						}
						else
						{
							foreach (var item in from item in result.VideoList let check = checkList.StockVideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
							{
								if (checkList.StockVideoList.Count <= AppSettings.CountVideosStock)
									checkList.StockVideoList.Add(item);
								else
									break;
							}
						}
					}
				}
			}
		}

		private string IdLastCategory = "";
		private async Task GetVideosByCategoryAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.MainVideoList.Count;

				string idCategory = "";
				var local = ListUtils.MyChannelList?.FirstOrDefault();

				Random random = new Random();
				if (local?.FavCategory?.Count > 0)
				{
					var index = random.Next(0, local.FavCategory.Count);
					idCategory = CategoriesController.ListCategories.FirstOrDefault(c => c.Id == local?.FavCategory[index])?.Id ?? "";
				}
				else
				{
					if (CategoriesController.ListCategories.Count > 0)
					{
						var index = random.Next(0, CategoriesController.ListCategories.Count);
						idCategory = CategoriesController.ListCategories[index]?.Id ?? "";
					}
					else
					{
						return;
					}
				}

				if (IdLastCategory != idCategory)
				{
					IdLastCategory = idCategory;
					offset = "0";
				}

				var (apiStatus, respond) = await RequestsAsync.Video.GetVideosByCategoryAsync(idCategory, "", "10", offset);
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

						foreach (var item in from item in result.VideoList let check = MAdapter.MainVideoList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId && a.Type == ItemType.OtherVideos) where check == null select item)
						{
							if (item.IsShort == "1")
								continue;

							MAdapter.MainVideoList.Add(new Classes.MainVideoClass
							{
								Id = Convert.ToInt32(item.Id),
								VideoData = item,
								Type = ItemType.OtherVideos
							});
							//AdapterHolders.AddAds(MAdapter, ItemType.BigVideo);
						}

						if (countList > 0)
						{
							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MainVideoList.Count - countList); });
						}
						else
						{
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.MainVideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
					Activity?.RunOnUiThread(ShowEmptyPage);
				}
			}
			else
			{
				MainScrollEvent.IsLoading = false;
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

				//Add Featured Videos
				if (ListUtils.FeaturedVideosList.Count > 0)
				{
					var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.SliderHome);
					if (checkList == null)
					{
						var stockVideos = new Classes.MainVideoClass
						{
							Id = 107,
							Type = ItemType.SliderHome
						};

						MAdapter.MainVideoList.Insert(0, stockVideos);
					}
					AddShortVideos();
				}

				if (MAdapter.MainVideoList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;

					var checkList = MAdapter.MainVideoList.FirstOrDefault(q => q.Type is ItemType.LiveVideos or ItemType.TopVideos or ItemType.LatestVideos or ItemType.FavVideos or ItemType.LiveVideos or ItemType.StockVideos or ItemType.ShortVideos or ItemType.OtherVideos);
					if (checkList != null)
					{
						var emptyStateChecker = MAdapter.MainVideoList.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
						if (emptyStateChecker != null)
						{
							MAdapter.MainVideoList.Remove(emptyStateChecker);
						}
					}
				}
				else
				{
					var emptyStateChecker = MAdapter.MainVideoList.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
					if (emptyStateChecker == null)
					{
						MAdapter.MainVideoList.Add(new Classes.MainVideoClass
						{
							Id = 300,
							Type = ItemType.EmptyPage
						});
						MAdapter.NotifyDataSetChanged();
					}

					EmptyStateLayout.Visibility = ViewStates.Gone;
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