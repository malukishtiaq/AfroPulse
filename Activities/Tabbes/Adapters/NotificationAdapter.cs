using Android.App;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Tabbes.Adapters
{
	public class NotificationAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<NotificationAdapterClickEventArgs> ItemClick;
		public event EventHandler<NotificationAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;
		public ObservableCollection<NotificationDataObject> NotificationList = new ObservableCollection<NotificationDataObject>();

		public NotificationAdapter(Activity context)
		{
			HasStableIds = true;
			ActivityContext = context;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_NotifcationView, parent, false);
				var vh = new NotificationAdapterViewHolder(itemView, OnClick, OnLongClick);
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
				if (viewHolder is NotificationAdapterViewHolder holder)
				{
					var item = NotificationList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
						GlideImageLoader.LoadImage(ActivityContext, item.Video?.Thumbnail, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

						string text;
						if (item.Title.Contains("added"))
						{
							// FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Videocam);
							text = item.Title == "added a new video" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Added) : item.Title;
						}
						else if (item.Title.Contains("unsubscribed"))
						{
							// FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.PersonAdd);
							text = item.Title == "unsubscribed from your channel" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Unsubscribed) : item.Title;
						}
						else if (item.Title.Contains("subscribed"))
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.PersonAdd);
							text = item.Title == "subscribed to your channel" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Subscribed) : item.Title;
						}
						else if (item.Title.Contains("disliked"))
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.ThumbsDown);
							text = item.Title == "disliked your video" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Disliked) : item.Title;
						}
						else if (item.Title.Contains("liked"))
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.ThumbsUp);
							text = item.Title == "liked your video" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Liked) : item.Title;
						}
						else if (item.Title.Contains("commented"))
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Chatboxes);
							text = item.Title == "commented on your video" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Commented) : item.Title;
						}
						else if (item.Title.Contains("purchased"))
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Chatboxes);
							text = item.Title == "purchased on your video" ? ActivityContext.GetText(Resource.String.Lbl_Notif_Purchased) : item.Title;
						}
						else
						{
							//FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Notifications);
							text = item.Title;
						}

						var name = AppTools.GetNameFinal(item.UserData);
						string tempString = name + " " + text;
						try
						{
							SpannableString spanString = new SpannableString(tempString);
							spanString.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, name.Length, 0);
							spanString.SetSpan(new ForegroundColorSpan(Color.ParseColor(AppSettings.MainColor)), 0, name.Length, SpanTypes.ExclusiveExclusive);

							holder.UserNameNotfy.SetText(spanString, TextView.BufferType.Spannable);
						}
						catch
						{
							holder.UserNameNotfy.Text = tempString;
						}

						holder.TextNotfy.Text = item.Time;
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		public NotificationDataObject GetItem(int position)
		{
			return NotificationList[position];
		}

		public override int ItemCount => NotificationList?.Count ?? 0;

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

		void OnClick(NotificationAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
		void OnLongClick(NotificationAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);


		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is NotificationAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ImageUser);
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
				var item = NotificationList[p0];

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

	public class NotificationAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; }

		public ImageView ImageUser { get; private set; }
		public ImageView Image { get; private set; }
		//public TextView IconNotfy { get; private set; }
		public TextView UserNameNotfy { get; private set; }
		public TextView TextNotfy { get; private set; }

		#endregion


		public NotificationAdapterViewHolder(View itemView, Action<NotificationAdapterClickEventArgs> clickListener, Action<NotificationAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
				Image = MainView.FindViewById<ImageView>(Resource.Id.image_view);
				//IconNotfy = MainView.FindViewById<TextView>(Resource.Id.smallIcon);
				UserNameNotfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
				TextNotfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);

				//Create an Event
				itemView.Click += (sender, e) => clickListener(new NotificationAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new NotificationAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class NotificationAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}