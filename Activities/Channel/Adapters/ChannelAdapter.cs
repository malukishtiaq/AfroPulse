using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Channel.Adapters
{
	public class ChannelAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<ChannelAdapterClickEventArgs> ItemClick;
		public event EventHandler<ChannelAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;

		public readonly ObservableCollection<UserDataObject> ChannelList = new ObservableCollection<UserDataObject>();

		public ChannelAdapter(Activity context)
		{
			HasStableIds = true;
			ActivityContext = context;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> ChannelSubscribed_View
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ChannelSubscribedView, parent, false);
				var vh = new ChannelAdapterViewHolder(itemView, OnClick, OnLongClick);
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
				if (viewHolder is ChannelAdapterViewHolder holder)
				{
					var item = ChannelList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.ImgChannel, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);

						holder.TxtNamechannal.Text = Methods.FunString.SubStringCutOf(AppTools.GetNameFinal(item), 16);

						//Verified 
						if (item.Verified == "1")
						{
							holder.IconVerified.Visibility = ViewStates.Visible;
							FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconVerified, IonIconsFonts.CheckmarkCircle);
						}
						else
						{
							holder.IconVerified.Visibility = ViewStates.Gone;
						}

						if (item.SubscribeCount != null)
						{
							item.SubscribeCount = item.SubscribeCount.Replace(",", "");
							holder.ChannelViews.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.SubscribeCount)) + " " + ActivityContext.GetText(Resource.String.Lbl_Subscribers);
						}
						else
							holder.ChannelViews.Text = "0 " + ActivityContext.GetText(Resource.String.Lbl_Subscribers);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		public override int ItemCount => ChannelList?.Count ?? 0;

		public UserDataObject GetItem(int position)
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
		void OnClick(ChannelAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(ChannelAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is ChannelAdapterViewHolder viewHolder)
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

				if (item.Avatar != "")
				{
					d.Add(item.Avatar);
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

	public class ChannelAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		public ImageView ImgChannel { get; private set; }
		public TextView TxtNamechannal { get; private set; }
		public TextView IconVerified { get; private set; }
		public TextView ChannelViews { get; private set; }

		#endregion

		public ChannelAdapterViewHolder(View itemView, Action<ChannelAdapterClickEventArgs> clickListener, Action<ChannelAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				ImgChannel = MainView.FindViewById<ImageView>(Resource.Id.Channel_Image);
				TxtNamechannal = MainView.FindViewById<TextView>(Resource.Id.ChannelName);
				IconVerified = MainView.FindViewById<TextView>(Resource.Id.IconVerified);
				ChannelViews = MainView.FindViewById<TextView>(Resource.Id.ChannelViews);

				//Event
				itemView.Click += (sender, e) => clickListener(new ChannelAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new ChannelAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class ChannelAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}