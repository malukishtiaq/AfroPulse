using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Util;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Channel.Adapters;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
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

namespace PlayTube.Activities.Library.Adapters
{
	public class SubscriptionsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, IVideoMenuListener
	{
		public event EventHandler<SubscriptionsAdapterClickEventArgs> ItemClick;
		public event EventHandler<SubscriptionsAdapterClickEventArgs> ItemLongClick;

		public event EventHandler<VideoAdapterClickEventArgs> VideoItemClick;
		public event EventHandler<VideoAdapterClickEventArgs> VideoItemLongClick;

		private readonly Activity ActivityContext;
		private readonly TabbedMainActivity GlobalContext;
		private AllChannelSubscribedAdapter ChannelAdapter;
		public ObservableCollection<Classes.SubscriptionsClass> SubscriptionsList = new ObservableCollection<Classes.SubscriptionsClass>();
		private readonly LibrarySynchronizer LibrarySynchronizer;

		public SubscriptionsAdapter(Activity context)
		{
			try
			{
				ActivityContext = context;
				GlobalContext = TabbedMainActivity.GetInstance();
				LibrarySynchronizer = new LibrarySynchronizer(context);
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
					case (int)ItemType.Channel:
						{
							//Setup your layout here >> TemplateRecyclerViewLayout
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
							var vh = new SubscriptionsAdapterViewHolder(itemView, OnClick, OnLongClick);
							return vh;
						}
					case (int)ItemType.BigVideo:
						{
							//Setup your layout here >> Video_Big_View
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoBigView, parent, false);
							var vh = new VideoBigAdapterViewHolder(itemView, VideoOnClick, VideoOnLongClick);
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
				var item = SubscriptionsList[position];
				if (item != null)
				{
					switch (item.Type)
					{
						case ItemType.Channel:
							{
								if (viewHolder is SubscriptionsAdapterViewHolder holder)
								{
									if (ChannelAdapter == null)
									{
										ChannelAdapter = new AllChannelSubscribedAdapter(ActivityContext)
										{
											ChannelList = new ObservableCollection<UserDataObject>()
										};

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.HasFixedSize = true;
										holder.MRecycler.SetItemViewCacheSize(10);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<UserDataObject>(ActivityContext, ChannelAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(ChannelAdapter);

										ChannelAdapter.ItemClick += ChannelAdapterOnOnItemClick;
									}

									if (item.ChannelList.Count > 0)
									{
										ChannelAdapter.ChannelList = new ObservableCollection<UserDataObject>(item.ChannelList.Take(4));
										ChannelAdapter.NotifyDataSetChanged();
									}

									holder.MainLinear.Visibility = ViewStates.Visible;
									holder.MoreText.Visibility = ViewStates.Visible;
									holder.TitleText.Text = ActivityContext.GetString(Resource.String.Lbl_All_Channal);
									holder.TitleIcon.SetImageResource(Resource.Drawable.pif_video_list);
								}

								break;
							}
						case ItemType.BigVideo:
							{
								if (viewHolder is VideoBigAdapterViewHolder holder)
								{
									GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Thumbnail, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
									GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Owner?.OwnerClass?.Avatar, holder.ChannelImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

									holder.TxtDuration.Text = Methods.Time.SplitStringDuration(item.VideoData.Duration);
									holder.TxtTitle.Text = Methods.FunString.DecodeString(item.VideoData.Title);

									if (!string.IsNullOrEmpty(item.VideoData.PausedTime) && !UserDetails.IsPauseWatchHistory)
									{
										int time = Convert.ToInt32(item.VideoData.PausedTime);
										if (time > 0)
										{
											holder.Progress.Visibility = ViewStates.Visible;
											holder.Progress.SetProgress(time, false);
										}
										else
										{
											holder.Progress.Visibility = ViewStates.Gone;
										}
									}
									else
									{
										holder.Progress.Visibility = ViewStates.Gone;
									}

									holder.TxtChannelName.Text = AppTools.GetNameFinal(item.VideoData.Owner?.OwnerClass);

									var view = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.VideoData.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

									holder.TxtViewsCount.Text = " | " + view + " | " + item.VideoData.TimeAgo;

									holder.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.VideoData.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

									if (!holder.MenuView.HasOnClickListeners)
									{
										holder.MenuView.Click += (sender, args) =>
										{
											var data = GetItem(holder.BindingAdapterPosition);
											VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(data.VideoData, this);
											videoMenuBottomSheets.Show(GlobalContext.SupportFragmentManager, videoMenuBottomSheets.Tag);
										};

										holder.InfoContainer.Click += (sender, args) =>
										{
											var data = GetItem(holder.BindingAdapterPosition);
											GlobalContext?.ShowUserChannelFragment(data.VideoData.Owner?.OwnerClass, data.VideoData.Owner?.OwnerClass.Id);
										};
									}

									//Set Badge on videos
									AppTools.ShowGlobalBadgeSystem(holder.VideoType, item.VideoData);
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

		public void RemoveVideo(VideoDataObject data)
		{
			try
			{
				var check = SubscriptionsList.FirstOrDefault(a => a.VideoData?.VideoId == data.VideoId);
				if (check != null)
				{
					var index = SubscriptionsList.IndexOf(check);
					if (index != -1)
					{
						SubscriptionsList.Remove(check);
						NotifyItemRemoved(index);
						NotifyItemRangeChanged(index, ItemCount);

						Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

						var dataObject = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == data.Id);
						if (dataObject == null)
						{
							ListUtils.GlobalNotInterestedList.Add(data);
						}

					}
					if (Methods.CheckConnectivity())
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddDeleteNotInterestedAsync(data.Id, true) });
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void ChannelAdapterOnOnItemClick(object sender, AllChannelAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = ChannelAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.ShowUserChannelFragment(item, item.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override int ItemCount => SubscriptionsList?.Count ?? 0;
		public Classes.SubscriptionsClass GetItem(int position)
		{
			return SubscriptionsList[position];
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
				var item = SubscriptionsList[position];
				if (item != null)
				{
					return item.Type switch
					{
						ItemType.BigVideo => (int)ItemType.BigVideo,
						ItemType.Channel => (int)ItemType.Channel,
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
				return 0;
			}
		}

		void OnClick(SubscriptionsAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
		void OnLongClick(SubscriptionsAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

		void VideoOnClick(VideoAdapterClickEventArgs args) => VideoItemClick?.Invoke(this, args);
		void VideoOnLongClick(VideoAdapterClickEventArgs args) => VideoItemLongClick?.Invoke(this, args);


		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = SubscriptionsList[p0];

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

	public class SubscriptionsAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }
		public LinearLayout MainLinear { get; private set; }
		public ImageView TitleIcon { get; private set; }
		public TextView TitleText { get; private set; }
		public TextView MoreText { get; private set; }
		public RecyclerView MRecycler { get; private set; }

		#endregion

		public SubscriptionsAdapterViewHolder(View itemView, Action<SubscriptionsAdapterClickEventArgs> clickListener, Action<SubscriptionsAdapterClickEventArgs> longClickListener) : base(itemView)
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
				MainLinear.Click += MoreTextOnClick;
				//itemView.Click += (sender, e) => clickListener(new SubscriptionsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				//itemView.LongClick += (sender, e) => longClickListener(new SubscriptionsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MoreTextOnClick(object sender, EventArgs e)
		{
			try
			{
				var globalContext = TabbedMainActivity.GetInstance();
				if (globalContext?.LibraryFragment?.SubscriptionsFragment != null)
				{
					globalContext.LibraryFragment.SubscriptionsFragment.AllChannelSubscribedFragment = new AllChannelSubscribedFragment();
					globalContext.FragmentBottomNavigator.DisplayFragment(globalContext.LibraryFragment.SubscriptionsFragment.AllChannelSubscribedFragment);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class SubscriptionsAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}