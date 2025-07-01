using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Util;
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;
using PlayTube.Activities.Live.Page;
using PlayTube.Activities.Shorts;
using PlayTube.Activities.Stock;
using PlayTube.Activities.Videos;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Tabbes.Adapters
{
	public class MainVideoAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, IVideoMenuListener
	{
		public event EventHandler<MainVideoAdapterClickEventArgs> ItemClick;
		public event EventHandler<MainVideoAdapterClickEventArgs> ItemLongClick;

		public event EventHandler<VideoAdapterClickEventArgs> OtherVideosItemClick;
		public event EventHandler<VideoAdapterClickEventArgs> OtherVideosItemLongClick;

		private readonly Activity ActivityContext;
		private readonly TabbedMainActivity GlobalContext;
		private VideoHorizontalAdapter TopVideosAdapter, LatestVideosAdapter, FavVideosAdapter, LiveVideosAdapter, StockVideosAdapter;
		private VideoHorizontalAdapterShorts ShortsVideosAdapter;

		public ObservableCollection<Classes.MainVideoClass> MainVideoList = new ObservableCollection<Classes.MainVideoClass>();

		public MainVideoAdapter(Activity context)
		{
			try
			{
				HasStableIds = true;
				ActivityContext = context;
				GlobalContext = TabbedMainActivity.GetInstance();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				switch (viewType)
				{
					case (int)ItemType.SliderHome:
						{
							//Setup your layout here >> SliderHomePageLayout
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.SliderHomePageLayout, parent, false);
							var vh = new SliderHomePageViewHolder(itemView, OtherVideosOnClick, OtherVideosOnLongClick);
							return vh;
						}
					case (int)ItemType.LiveVideos:
					case (int)ItemType.TopVideos:
					case (int)ItemType.LatestVideos:
					case (int)ItemType.FavVideos:
					case (int)ItemType.StockVideos:
					case (int)ItemType.ShortVideos:
						{
							//Setup your layout here >> TemplateRecyclerViewLayout 
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
							var vh = new MainVideoAdapterViewHolder(itemView, OnClick, OnLongClick);
							return vh;
						}
					case (int)ItemType.OtherVideos:
						{
							//Setup your layout here >> Video_Big_View
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoBigView, parent, false);
							var vh = new VideoBigAdapterViewHolder(itemView, OtherVideosOnClick, OtherVideosOnLongClick);
							return vh;
						}
					case (int)ItemType.AdMob1:
						{
							var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob, parent, false);
							var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, ActivityContext);
							return vh;
						}
					case (int)ItemType.AdMob2:
						{
							var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob2, parent, false);
							var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, ActivityContext);
							return vh;
						}
					case (int)ItemType.AdMob3:
						{
							var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob3, parent, false);
							var vh = new AdapterHolders.AdMob3AdapterViewHolder(itemView);
							return vh;
						}
					case (int)ItemType.EmptyPage:
						{
							//Setup your layout here >> EmptyStateLayout
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmptyStateLayout, parent, false);
							var vh = new AdapterHolders.EmptyStateViewHolder(itemView);
							return vh;
						}
					default:
						return null;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return null;
			}
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
		{
			try
			{
				var item = MainVideoList[position];
				if (item != null)
				{
					switch (item.Type)
					{
						case ItemType.SliderHome:
							{
								if (viewHolder is SliderHomePageViewHolder holder)
								{
									holder.ViewPagerView.Adapter = new ImageCoursalViewPager(ActivityContext, ListUtils.FeaturedVideosList);

									//holder.ViewPagerView.CurrentItem = 0;
									holder.ViewPagerView.OffscreenPageLimit = holder.ViewPagerView.Adapter.Count;

									//holder.ViewPagerView.SetCurrentItem(0, false);
									holder.ViewPagerView.Adapter.NotifyDataSetChanged();

									holder.CircleIndicatorView.SetViewPager(holder.ViewPagerView);
								}
								break;
							}
						case ItemType.LiveVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (LiveVideosAdapter == null)
									{
										LiveVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.LiveVideo) { VideoList = new ObservableCollection<VideoDataObject>() };
										LiveVideosAdapter.ItemClick += LiveVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, LiveVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(LiveVideosAdapter);
									}

									if (item.LiveVideoList.Count > 0)
									{
										LiveVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.LiveVideoList);
										ActivityContext?.RunOnUiThread(() => { LiveVideosAdapter.NotifyDataSetChanged(); });  
									}

									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += LiveVideosMainLinearOnClick;

									holder.MoreText.Visibility = LiveVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Live);
									holder.TitleIcon.SetImageResource(Resource.Drawable.icon_live_vector);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
								}
								break;
							}
						case ItemType.TopVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (TopVideosAdapter == null)
									{
										TopVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.TopVideo) { VideoList = new ObservableCollection<VideoDataObject>() };
										TopVideosAdapter.ItemClick += TopVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, TopVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(TopVideosAdapter);
									}

									if (item.TopVideoList.Count > 0)
									{
										TopVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.TopVideoList);
										ActivityContext?.RunOnUiThread(() => { TopVideosAdapter.NotifyDataSetChanged(); }); 
									}
									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += TopVideosMainLinearOnClick;

									holder.MoreText.Visibility = TopVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Top_videos);
									holder.TitleIcon.SetImageResource(Resource.Drawable.pif_top_videos);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#F8AB17"));

								}

								break;
							}
						case ItemType.LatestVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (LatestVideosAdapter == null)
									{
										LatestVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.LatestVideo) { VideoList = new ObservableCollection<VideoDataObject>() };
										LatestVideosAdapter.ItemClick += LatestVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, LatestVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(LatestVideosAdapter);
									}

									if (item.LatestVideoList.Count > 0)
									{
										LatestVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.LatestVideoList);
										ActivityContext?.RunOnUiThread(() => { LatestVideosAdapter.NotifyDataSetChanged(); }); 
									}

									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += LatestVideosMainLinearOnClick;

									holder.MoreText.Visibility = LatestVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Latest_videos);
									holder.TitleIcon.SetImageResource(Resource.Drawable.ic_video_camera_vector);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#c1134e"));
								}

								break;
							}
						case ItemType.FavVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (FavVideosAdapter == null)
									{
										FavVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.FavVideo) { VideoList = new ObservableCollection<VideoDataObject>() };
										FavVideosAdapter.ItemClick += FavVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, FavVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(FavVideosAdapter);
									}

									if (item.FavVideoList.Count > 0)
									{
										FavVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.FavVideoList);
										ActivityContext?.RunOnUiThread(() => { FavVideosAdapter.NotifyDataSetChanged(); }); 
									}

									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += FavVideosMainLinearOnClick;

									holder.MoreText.Visibility = FavVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Fav_videos);
									holder.TitleIcon.SetImageResource(Resource.Drawable.ic_star_vector);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#FDD835"));
								}

								break;
							}
						case ItemType.StockVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (StockVideosAdapter == null)
									{
										StockVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.StockVideo) { VideoList = new ObservableCollection<VideoDataObject>() };
										StockVideosAdapter.ItemClick += StockVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, StockVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(StockVideosAdapter);
									}

									if (item.StockVideoList.Count > 0)
									{
										StockVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.StockVideoList);
										ActivityContext?.RunOnUiThread(() => { StockVideosAdapter.NotifyDataSetChanged(); }); 
									}

									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += StockVideosMainLinearOnClick;

									holder.MoreText.Visibility = StockVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_StockVideo);
									holder.TitleIcon.SetImageResource(Resource.Drawable.pif_stock_videos);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#53B175"));
								}

								break;
							}
						case ItemType.ShortVideos:
							{
								if (viewHolder is MainVideoAdapterViewHolder holder)
								{
									if (ShortsVideosAdapter == null)
									{
										ShortsVideosAdapter = new VideoHorizontalAdapterShorts(ActivityContext) { VideoList = new ObservableCollection<VideoDataObject>() };
										ShortsVideosAdapter.ItemClick += ShortVideosAdapterOnItemClick;

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<VideoDataObject>(ActivityContext, ShortsVideosAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(ShortsVideosAdapter);
									}

									if (item.ShortsVideoList.Count > 0)
									{
										ShortsVideosAdapter.VideoList = new ObservableCollection<VideoDataObject>(item.ShortsVideoList);
										ActivityContext?.RunOnUiThread(() => { ShortsVideosAdapter.NotifyDataSetChanged(); });  
									}

									holder.MainLinear.Visibility = ViewStates.Visible;

									if (!holder.MainLinear.HasOnClickListeners)
										holder.MainLinear.Click += ShortsVideoMainLinearOnClick;

									holder.MoreText.Visibility = ShortsVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Shorts);
									holder.TitleIcon.SetImageResource(Resource.Drawable.icon_shorts_vector);
									holder.TitleIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#f40406"));
								}

								break;
							}
						case ItemType.OtherVideos:
							{
								if (viewHolder is VideoBigAdapterViewHolder videoRow)
								{
									GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Thumbnail, videoRow.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
									GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Owner?.OwnerClass?.Avatar, videoRow.ChannelImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

									videoRow.TxtDuration.Text = Methods.Time.SplitStringDuration(item.VideoData.Duration);
									videoRow.TxtTitle.Text = Methods.FunString.DecodeString(item.VideoData.Title);

									if (!string.IsNullOrEmpty(item.VideoData.PausedTime))
									{
										int time = Convert.ToInt32(item.VideoData.PausedTime);
										if (time > 0)
										{
											videoRow.Progress.Visibility = ViewStates.Visible;
											videoRow.Progress.SetProgress(time, false);
										}
										else
										{
											videoRow.Progress.Visibility = ViewStates.Gone;
										}
									}
									else
									{
										videoRow.Progress.Visibility = ViewStates.Gone;
									}

									videoRow.TxtChannelName.Text = AppTools.GetNameFinal(item.VideoData.Owner?.OwnerClass);

									var view = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.VideoData.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);
									videoRow.TxtViewsCount.Text = " | " + view + " | " + item.VideoData.TimeAgo;

									videoRow.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.VideoData.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

									if (!videoRow.MenuView.HasOnClickListeners)
									{
										videoRow.MenuView.Click += (sender, args) =>
										{
											var data = GetItem(videoRow.BindingAdapterPosition);
											VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(data.VideoData, this);
											videoMenuBottomSheets.Show(GlobalContext.SupportFragmentManager, videoMenuBottomSheets.Tag);
										};

										videoRow.InfoContainer.Click += (sender, args) =>
										{
											var data = GetItem(videoRow.BindingAdapterPosition);
											TabbedMainActivity.GetInstance()?.ShowUserChannelFragment(data.VideoData.Owner?.OwnerClass, data.VideoData.Owner?.OwnerClass.Id);
										};
									}

									//Set Badge on videos
									AppTools.ShowGlobalBadgeSystem(videoRow.VideoType, item.VideoData);
								}
								break;
							}
						case ItemType.EmptyPage:
							{
								if (viewHolder is AdapterHolders.EmptyStateViewHolder holder)
								{
									//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
									holder.EmptyStateIcon.SetImageResource(Resource.Drawable.ic_no_video_vector);
									holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
									holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_No_videos_found_for_now);
									holder.EmptyStateButton.Visibility = ViewStates.Gone;
								}
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

		private void LiveVideosMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				Bundle bundle = new Bundle();

				bundle.PutString("Type", "Live");

				VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
				{
					Arguments = bundle
				};
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
				GlobalContext?.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void StockVideosMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				StockVideosFragment stockVideosFragment = new StockVideosFragment();
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(stockVideosFragment);
				GlobalContext?.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ShortsVideoMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(GlobalContext, typeof(ShortsVideoDetailsActivity));
				intent.PutExtra("Type", "VideoShorts");
				//intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoShortsList.ToList()));
				GlobalContext.StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void FavVideosMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				Bundle bundle = new Bundle();

				bundle.PutString("Type", "Fav");

				VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
				{
					Arguments = bundle
				};
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
				GlobalContext?.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LatestVideosMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				Bundle bundle = new Bundle();

				bundle.PutString("Type", "Latest");

				VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
				{
					Arguments = bundle
				};
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
				GlobalContext?.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TopVideosMainLinearOnClick(object sender, EventArgs e)
		{
			try
			{
				Bundle bundle = new Bundle();

				bundle.PutString("Type", "Top");

				VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
				{
					Arguments = bundle
				};
				GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
				GlobalContext?.VideoDataWithEventsLoader?.OnStop();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Open Video from Fav
		private void FavVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = FavVideosAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Open Video from Latest
		private void LatestVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = LatestVideosAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void StockVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = StockVideosAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void LiveVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				var item = LiveVideosAdapter.VideoList[e.Position];
				if (item?.LiveTime != null && item.LiveTime > 0)
				{
					//Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
					Intent intent = new Intent(ActivityContext, typeof(LiveStreamingActivity));
					//Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
					intent.PutExtra(LiveConstants.KeyClientRole, IO.Agora.Rtc2.Constants.ClientRoleAudience);
					intent.PutExtra("VideoId", item.Id);
					intent.PutExtra("StreamName", item.StreamName);
					intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(item));
					ActivityContext.StartActivity(intent);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Open Video from Top
		private void TopVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = TopVideosAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.StartPlayVideo(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ShortVideosAdapterOnItemClick(object sender, VideoHorizontalAdapterShortsClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = ShortsVideosAdapter.GetItem(e.Position);
				if (item == null) return;

				var intent = new Intent(GlobalContext, typeof(ShortsVideoDetailsActivity));
				intent.PutExtra("Type", "VideoShorts");
				intent.PutExtra("IndexItem", e.Position);
				//intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoShortsList.ToList()));
				GlobalContext.StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void RemoveVideo(VideoDataObject data)
		{
			try
			{
				var check = MainVideoList.FirstOrDefault(a => a.VideoData?.VideoId == data.VideoId);
				if (check != null)
				{
					var index = MainVideoList.IndexOf(check);
					if (index != -1)
					{
						MainVideoList.Remove(check);
						NotifyItemRemoved(index);
						NotifyItemRangeChanged(index, ItemCount);

						Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

						var dataObject = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == data.Id);
						if (dataObject == null)
						{
							ListUtils.GlobalNotInterestedList.Add(data);
						}

					}
				}
				if (Methods.CheckConnectivity())
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddDeleteNotInterestedAsync(data.Id, true) });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override int ItemCount => MainVideoList?.Count ?? 0;

		public Classes.MainVideoClass GetItem(int position)
		{
			return MainVideoList[position];
		}

		public void Clear()
		{
			try
			{
				ShortsVideosAdapter?.VideoList.Clear();
				TopVideosAdapter?.VideoList.Clear();
				LatestVideosAdapter?.VideoList.Clear();
				FavVideosAdapter?.VideoList.Clear();
				LiveVideosAdapter?.VideoList.Clear();
				StockVideosAdapter?.VideoList.Clear();

				ShortsVideosAdapter?.NotifyDataSetChanged();
				TopVideosAdapter?.NotifyDataSetChanged();
				LatestVideosAdapter?.NotifyDataSetChanged();
				FavVideosAdapter?.NotifyDataSetChanged();
				LiveVideosAdapter?.NotifyDataSetChanged();
				StockVideosAdapter?.NotifyDataSetChanged();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override long GetItemId(int position)
		{
			try
			{
				return position;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return 0;
			}
		}

		public override int GetItemViewType(int position)
		{
			try
			{
				var item = MainVideoList[position];
				if (item != null)
				{
					return item.Type switch
					{
						ItemType.SliderHome => (int)ItemType.SliderHome,
						ItemType.TopVideos => (int)ItemType.TopVideos,
						ItemType.LatestVideos => (int)ItemType.LatestVideos,
						ItemType.LiveVideos => (int)ItemType.LiveVideos,
						ItemType.FavVideos => (int)ItemType.FavVideos,
						ItemType.OtherVideos => (int)ItemType.OtherVideos,
						ItemType.StockVideos => (int)ItemType.StockVideos,
						ItemType.ShortVideos => (int)ItemType.ShortVideos,
						ItemType.AdMob1 => (int)ItemType.AdMob1,
						ItemType.AdMob2 => (int)ItemType.AdMob2,
						ItemType.AdMob3 => (int)ItemType.AdMob3,
						ItemType.EmptyPage => (int)ItemType.EmptyPage,
						_ => (int)ItemType.EmptyPage
					};
				}

				return (int)ItemType.EmptyPage;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return (int)ItemType.EmptyPage;
			}
		}

		void OnClick(MainVideoAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
		void OnLongClick(MainVideoAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

		void OtherVideosOnClick(VideoAdapterClickEventArgs args) => OtherVideosItemClick?.Invoke(this, args);
		void OtherVideosOnLongClick(VideoAdapterClickEventArgs args) => OtherVideosItemLongClick?.Invoke(this, args);

		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = MainVideoList[p0];

				if (item?.VideoData == null)
					return Collections.SingletonList(p0);

				if (!string.IsNullOrEmpty(item.VideoData?.Thumbnail))
				{
					d.Add(item.VideoData.Thumbnail);
					return d;
				}

				return d;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return Collections.SingletonList(p0);
			}
		}

		public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
		{
			return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
		}

	}

	public class MainVideoAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }
		public LinearLayout MainLinear { get; private set; }
		public ImageView TitleIcon { get; private set; }
		public TextView TitleText { get; private set; }
		public TextView MoreText { get; private set; }
		public RecyclerView MRecycler { get; private set; }

		#endregion

		public MainVideoAdapterViewHolder(View itemView, Action<MainVideoAdapterClickEventArgs> clickListener, Action<MainVideoAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				MainLinear = MainView.FindViewById<LinearLayout>(Resource.Id.mainLinear);
				TitleIcon = MainView.FindViewById<ImageView>(Resource.Id.iconTitle);
				TitleText = MainView.FindViewById<TextView>(Resource.Id.textTitle);
				MoreText = MainView.FindViewById<TextView>(Resource.Id.textMore);
				MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.recyler);

				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);

				//Create an Event
				//itemView.Click += (sender, e) => clickListener(new MainVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				//itemView.LongClick += (sender, e) => longClickListener(new MainVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}


	public class SliderHomePageViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; set; }
		public ViewPager ViewPagerView { get; private set; }
		public CircleIndicator CircleIndicatorView { get; private set; }


		#endregion

		public SliderHomePageViewHolder(View itemView, Action<VideoAdapterClickEventArgs> clickListener, Action<VideoAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				//Get values
				ViewPagerView = itemView.FindViewById<ViewPager>(Resource.Id.viewpager2);
				CircleIndicatorView = itemView.FindViewById<CircleIndicator>(Resource.Id.indicator);

				//Create an Event
				//itemView.Click += (sender, e) => clickListener(new VideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, VideoStyle = VideoAdapterClickEventArgs.VideoType.BigVideo });
				//itemView.LongClick += (sender, e) => longClickListener(new VideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class MainVideoAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}