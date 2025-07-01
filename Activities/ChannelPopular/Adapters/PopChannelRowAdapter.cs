using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Video;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace PlayTube.Activities.ChannelPopular.Adapters
{
	public class PopChannelRowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<PopChannelRowAdapterClickEventArgs> SubscribeButtonClick;
		public event EventHandler<PopChannelRowAdapterClickEventArgs> ItemClick;
		public event EventHandler<PopChannelRowAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;

		public ObservableCollection<GetPopularChannelsObject.Channel> ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>();

		public PopChannelRowAdapter(Activity context)
		{
			HasStableIds = true;
			ActivityContext = context;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Style_PopularChannels_view
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PopularChannelsView, parent, false);
				var vh = new PopChannelRowAdapterViewHolder(itemView, SubscribeButtonOnClick, OnClick, OnLongClick);
				return vh;
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
				if (viewHolder is PopChannelRowAdapterViewHolder holder)
				{
					var item = ChannelList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.ImgChannel, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);

						holder.TxtChannelName.Text = Methods.FunString.SubStringCutOf(AppTools.GetNameFinal(item.UserData), 16);
						holder.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.UserData.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

						holder.TxtSubscribers.Text = item.SubscribersCount + " " + ActivityContext.GetText(Resource.String.Lbl_Subscribers);

						// 
						var isOwner = item.UserData.Id == UserDetails.UserId;
						holder.SubscribeButton.Visibility = isOwner ? ViewStates.Invisible : ViewStates.Visible;

						if (isOwner)
							return;

						item.UserData.AmISubscribed ??= item.UserData.AmISubscribed;

						if (!string.IsNullOrEmpty(item.UserData.SubscriberPrice) && item.UserData.SubscriberPrice != "0")
						{
							if (item.UserData.AmISubscribed == "0")
							{
								//This channel is paid, You must pay to subscribe
								holder.SubscribeButton.Tag = "PaidSubscribe";

								//Color
								//holder.SubscribeButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								//Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//holder.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
								var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
								Console.WriteLine(currency);
								holder.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + item.UserData.SubscriberPrice;
							}
							else
							{
								holder.SubscribeButton.Tag = "Subscribed";

								holder.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);


								//Color
								holder.SubscribeButton.SetTextColor(Color.White);

								// back ground
								holder.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_pressed);


								//icon
								//Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//holder.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
							}
						}
						else
						{
							holder.SubscribeButton.Tag = item.UserData.AmISubscribed == "1" ? "Subscribed" : "Subscribe";

							switch (holder.SubscribeButton.Tag?.ToString())
							{
								case "Subscribed":
									{
										holder.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

										//Color
										holder.SubscribeButton.SetTextColor(Color.White);

										// back ground
										holder.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_pressed);

										//icon
										//Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
										//icon.Bounds = new Rect(10, 10, 10, 7);
										//holder.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
										break;
									}
								case "Subscribe":
									{
										holder.SubscribeButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);

										//Color
										holder.SubscribeButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));

										// back ground
										holder.SubscribeButton.SetBackgroundResource(Resource.Drawable.round_button_normal);
										//icon
										//Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
										//icon.Bounds = new Rect(10, 10, 10, 7);
										//holder.SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
										break;
									}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override int ItemCount => ChannelList?.Count ?? 0;


		public GetPopularChannelsObject.Channel GetItem(int position)
		{
			return ChannelList[position];
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
				return position;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return 0;
			}
		}
		void SubscribeButtonOnClick(PopChannelRowAdapterClickEventArgs args) => SubscribeButtonClick?.Invoke(this, args);
		void OnClick(PopChannelRowAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(PopChannelRowAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is PopChannelRowAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ImgChannel);
				}
				base.OnViewRecycled(holder);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = ChannelList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.UserData?.Avatar != "")
				{
					d.Add(item.UserData?.Avatar);
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
			return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
				.Apply(new RequestOptions().CenterCrop());
		}
	}

	public class PopChannelRowAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		public ImageView ImgChannel { get; private set; }
		public TextView TxtChannelName { get; private set; }
		public TextView TxtSubscribers { get; private set; }
		public AppCompatButton SubscribeButton { get; private set; }

		#endregion

		public PopChannelRowAdapterViewHolder(View itemView, Action<PopChannelRowAdapterClickEventArgs> subscribeButtonClickListener, Action<PopChannelRowAdapterClickEventArgs> clickListener, Action<PopChannelRowAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				ImgChannel = MainView.FindViewById<ImageView>(Resource.Id.image);
				TxtChannelName = MainView.FindViewById<TextView>(Resource.Id.name);
				TxtSubscribers = MainView.FindViewById<TextView>(Resource.Id.subscribers);
				SubscribeButton = (AppCompatButton)MainView.FindViewById(Resource.Id.cont);

				SubscribeButton.Visibility = ViewStates.Gone;

				//Event
				SubscribeButton.Click += (sender, e) => subscribeButtonClickListener(new PopChannelRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, SubscribeButton = SubscribeButton });
				itemView.Click += (sender, e) => clickListener(new PopChannelRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, SubscribeButton = SubscribeButton });
				itemView.LongClick += (sender, e) => longClickListener(new PopChannelRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, SubscribeButton = SubscribeButton });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class PopChannelRowAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
		public AppCompatButton SubscribeButton { get; set; }
	}
}