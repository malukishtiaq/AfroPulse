using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Util;
using PlayTube.Activities.ChannelPopular;
using PlayTube.Activities.ChannelPopular.Adapters;
using PlayTube.Activities.Models;
using PlayTube.Activities.Videos;
using PlayTube.Activities.Videos.Adapters;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Tabbes.Adapters
{
	public class TrendingAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, IVideoMenuListener
	{
		public event EventHandler<TrendingAdapterClickEventArgs> ItemClick;
		public event EventHandler<TrendingAdapterClickEventArgs> ItemLongClick;

		public event EventHandler<VideoAdapterClickEventArgs> VideoItemClick;
		public event EventHandler<VideoAdapterClickEventArgs> VideoItemLongClick;

		public event EventHandler<VideoAdapterClickEventArgs> BigVideoItemClick;
		public event EventHandler<VideoAdapterClickEventArgs> BigVideoItemLongClick;

		private readonly Activity ActivityContext;
		private readonly TabbedMainActivity GlobalContext;
		private PopChannelRowAdapter PopularChannelAdapter;
		public ObservableCollection<Classes.TrendingClass> TrendingList = new ObservableCollection<Classes.TrendingClass>();
		private readonly LibrarySynchronizer LibrarySynchronizer;

		public TrendingAdapter(Activity context)
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
							var vh = new TrendingAdapterViewHolder(itemView, OnClick, OnLongClick);
							return vh;
						}
					case (int)ItemType.RowVideo:
						{
							//Setup your layout here >> Video_Row_View
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoRowView, parent, false);
							var vh = new VideoRowAdapterViewHolder(itemView, VideoOnClick, VideoOnLongClick);
							return vh;
						}
					case (int)ItemType.BigVideo:
						{
							//Setup your layout here >> Video_Big_View
							View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_VideoBigView, parent, false);
							var vh = new VideoBigAdapterViewHolder(itemView, BigVideoOnClick, BigVideoOnLongClick);
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
				var item = TrendingList[position];
				if (item != null)
				{
					switch (item.Type)
					{
						case ItemType.Channel:
							{
								if (viewHolder is TrendingAdapterViewHolder holder)
								{
									if (PopularChannelAdapter == null)
									{
										PopularChannelAdapter = new PopChannelRowAdapter(ActivityContext)
										{
											ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>()
										};

										LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext);
										holder.MRecycler.SetLayoutManager(layoutManager);
										holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

										var sizeProvider = new FixedPreloadSizeProvider(10, 10);
										var preLoader = new RecyclerViewPreloader<GetPopularChannelsObject.Channel>(ActivityContext, PopularChannelAdapter, sizeProvider, 10);
										holder.MRecycler.AddOnScrollListener(preLoader);
										holder.MRecycler.SetAdapter(PopularChannelAdapter);
										PopularChannelAdapter.ItemClick += PopularChannelAdapterOnOnItemClick;
										PopularChannelAdapter.SubscribeButtonClick += PopularChannelAdapterOnSubscribeButtonClick;
									}

									if (item.ChannelList.Count > 0)
									{
										if (PopularChannelAdapter.ChannelList.Count == 0)
										{
											PopularChannelAdapter.ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>(item.ChannelList);
											PopularChannelAdapter.NotifyDataSetChanged();
										}
										else if (PopularChannelAdapter.ChannelList != null && PopularChannelAdapter.ChannelList.Count != item.ChannelList.Count)
										{
											PopularChannelAdapter.ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>(item.ChannelList);
											PopularChannelAdapter.NotifyDataSetChanged();
										}
									}

									holder.MainLinear.Visibility = ViewStates.Visible;
									holder.MoreText.Visibility = PopularChannelAdapter.ChannelList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
									holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_PopularChannels);

								}

								break;
							}
						case ItemType.RowVideo:
							{
								if (viewHolder is VideoRowAdapterViewHolder videoRow)
								{
									GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Thumbnail, videoRow.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

									videoRow.TxtDuration.Text = Methods.Time.SplitStringDuration(item.VideoData.Duration);
									videoRow.TxtTitle.Text = Methods.FunString.DecodeString(item.VideoData.Title);

									videoRow.TxtChannelName.Text = AppTools.GetNameFinal(item.VideoData.Owner?.OwnerClass);
									videoRow.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.VideoData.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

									videoRow.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.VideoData.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

									if (!videoRow.MenuView.HasOnClickListeners)
									{
										videoRow.MenuView.Click += (sender, args) =>
										{
											var data = GetItem(videoRow.BindingAdapterPosition);
											VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(data.VideoData, this);
											videoMenuBottomSheets.Show(GlobalContext.SupportFragmentManager, videoMenuBottomSheets.Tag);
										};

										videoRow.TxtChannelName.Click += (sender, args) =>
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
						case ItemType.BigVideo:
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
									holder.EmptyStateIcon.SetImageResource(Resource.Drawable.icon_fire_vector);

									holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
									holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Trending);
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

		private void PopularChannelAdapterOnOnItemClick(object sender, PopChannelRowAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = PopularChannelAdapter.GetItem(e.Position);
				if (item == null) return;

				GlobalContext.ShowUserChannelFragment(item.UserData, item.UserData.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void PopularChannelAdapterOnSubscribeButtonClick(object sender, PopChannelRowAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = PopularChannelAdapter.GetItem(e.Position);
				if (item == null) return;

				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (!string.IsNullOrEmpty(item.UserData?.SubscriberPrice) && item.UserData?.SubscriberPrice != "0")
						{
							if (e.SubscribeButton.Tag?.ToString() == "PaidSubscribe")
							{
								//This channel is paid, You must pay to subscribe
								GlobalContext.OpenDialog(item.UserData, Convert.ToDouble(item.UserData?.SubscriberPrice));
							}
							else
							{
								e.SubscribeButton.Tag = "Subscribe";
								e.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_normal);

								//Color
								e.SubscribeButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(item.UserData?.Id);


								//Send API Request here for UnSubcribe 
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
						else
						{
							if (e.SubscribeButton.Tag?.ToString() == "Subscribe")
							{
								e.SubscribeButton.Tag = "Subscribed";
								e.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_pressed);

								//Color
								e.SubscribeButton.SetTextColor(Color.White);
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribedButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Add The Video to  Subcribed Videos Database
								Events_Insert_SubscriptionsChannel(item.UserData);

								//Send API Request here for Subcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });


								Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							}
							else
							{
								e.SubscribeButton.Tag = "Subscribe";
								e.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);

								// background
								e.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_normal);

								//Color
								e.SubscribeButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
								//icon
								//Drawable icon = Context.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//e.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(item.UserData?.Id);


								//Send API Request here for UnSubcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(item.UserData?.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
						dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Warning), GlobalContext.GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void Events_Insert_SubscriptionsChannel(UserDataObject owner)
		{
			try
			{
				var sqlEntity = new SqLiteDatabase();

				if (owner != null)
					sqlEntity.Insert_One_SubscriptionChannel(owner);

				GlobalContext.LibrarySynchronizer.AddToSubscriptions(owner);
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
				var check = TrendingList.FirstOrDefault(a => a.VideoData?.VideoId == data.VideoId);
				if (check != null)
				{
					var index = TrendingList.IndexOf(check);
					if (index != -1)
					{
						TrendingList.Remove(check);
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

		public override int ItemCount => TrendingList?.Count ?? 0;

		public Classes.TrendingClass GetItem(int position)
		{
			return TrendingList[position];
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
				var item = TrendingList[position];
				if (item != null)
				{
					return item.Type switch
					{
						ItemType.BigVideo => (int)ItemType.BigVideo,
						ItemType.RowVideo => (int)ItemType.RowVideo,
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

		void OnClick(TrendingAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
		void OnLongClick(TrendingAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

		void VideoOnClick(VideoAdapterClickEventArgs args) => VideoItemClick?.Invoke(this, args);
		void VideoOnLongClick(VideoAdapterClickEventArgs args) => VideoItemLongClick?.Invoke(this, args);

		void BigVideoOnClick(VideoAdapterClickEventArgs args) => BigVideoItemClick?.Invoke(this, args);
		void BigVideoOnLongClick(VideoAdapterClickEventArgs args) => BigVideoItemLongClick?.Invoke(this, args);

		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = TrendingList[p0];

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

	public class TrendingAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }
		public LinearLayout MainLinear { get; private set; }
		public TextView TitleText { get; private set; }
		public TextView MoreText { get; private set; }
		public RecyclerView MRecycler { get; private set; }

		#endregion

		public TrendingAdapterViewHolder(View itemView, Action<TrendingAdapterClickEventArgs> clickListener, Action<TrendingAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				MainLinear = MainView.FindViewById<LinearLayout>(Resource.Id.mainLinear);
				TitleText = MainView.FindViewById<TextView>(Resource.Id.textTitle);
				MoreText = MainView.FindViewById<TextView>(Resource.Id.textMore);
				MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.recyler);

				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);

				//Create an Event
				MainLinear.Click += MoreTextOnClick;
				//itemView.Click += (sender, e) => clickListener(new TrendingAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				//itemView.LongClick += (sender, e) => longClickListener(new TrendingAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
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
				if (globalContext?.TrendingFragment != null)
				{
					globalContext.TrendingFragment.AllChannelPopularFragment = new AllChannelPopularFragment();
					globalContext.FragmentBottomNavigator.DisplayFragment(globalContext.TrendingFragment.AllChannelPopularFragment);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}


	public class TrendingAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}