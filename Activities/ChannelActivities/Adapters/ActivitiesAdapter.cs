using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Activities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace PlayTube.Activities.ChannelActivities.Adapters
{
	public class ActivitiesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<ActivitiesAdapterClickEventArgs> ItemClick;
		public event EventHandler<ActivitiesAdapterClickEventArgs> LikeItemClick;
		public event EventHandler<ActivitiesAdapterClickEventArgs> ShareItemClick;
		public event EventHandler<ActivitiesAdapterClickEventArgs> ItemLongClick;
		private readonly Activity ActivityContext;
		public ObservableCollection<ActivitiesDataObject> ActivitiesList = new ObservableCollection<ActivitiesDataObject>();

		public ActivitiesAdapter(Activity context)
		{
			HasStableIds = true;
			ActivityContext = context;
		}

		// Create new views (invoked by the layout manager)
		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				//Setup your layout here >> Video_Row_View
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ActivitiesView, parent, false);
				var vh = new ActivitiesAdapterViewHolder(itemView, OnClick, OnLongClick, LikeOnClick, ShareOnClick);
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
				if (viewHolder is ActivitiesAdapterViewHolder holder)
				{
					var item = ActivitiesList[position];
					if (item != null)
					{
						GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.ImageAct, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

						holder.TimeUser.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);

						holder.ContentAct.Text = Methods.FunString.DecodeString(item.Text);

						switch (item.IsLiked)
						{
							case "0":
								holder.IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								holder.LikeActLayout.Tag = "Like";
								break;
							case "1":
								holder.IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_filled);
								holder.LikeActLayout.Tag = "Liked";
								break;
							default:
								holder.IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								holder.LikeActLayout.Tag = "Like";
								break;
						}

						holder.CountLikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Likes));

						//switch (item.IsDisliked)
						//{
						//    case "0":
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, holder.IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        holder.IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
						//        holder.DislikeActLayout.Tag = "Dislike";
						//        break;
						//    case "1":
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, holder.IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        holder.IconDislikeAct.SetTextColor(Color.ParseColor("#4caf50"));
						//        holder.DislikeActLayout.Tag = "Disliked";
						//        break;
						//    default:
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, holder.IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        holder.IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
						//        holder.DislikeActLayout.Tag = "Dislike";
						//        break;
						//}

						//holder.CountDislikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Dislikes));

						holder.CountCommentAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.CommentsCount));
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override int ItemCount => ActivitiesList?.Count ?? 0;

		public ActivitiesDataObject GetItem(int position)
		{
			return ActivitiesList[position];
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

		void LikeOnClick(ActivitiesAdapterClickEventArgs args) => LikeItemClick?.Invoke(this, args);
		void ShareOnClick(ActivitiesAdapterClickEventArgs args) => ShareItemClick?.Invoke(this, args);
		void OnClick(ActivitiesAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(ActivitiesAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			try
			{
				if (ActivityContext?.IsDestroyed != false)
					return;

				if (holder is ActivitiesAdapterViewHolder viewHolder)
				{
					Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ImageAct);
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
				var item = ActivitiesList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.Image != "")
				{
					d.Add(item.Image);
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

	public class ActivitiesAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }

		public ImageView ImageAct { get; private set; }
		public TextView TimeUser { get; private set; }
		public TextView ContentAct { get; private set; }
		public LinearLayout LikeActLayout { get; private set; }
		public ImageView IconLikeAct { get; private set; }
		public TextView CountLikeAct { get; private set; }
		public LinearLayout ShareActLayout { get; private set; }
		public LinearLayout CommentActLayout { get; private set; }
		public TextView CountCommentAct { get; private set; }

		#endregion

		public ActivitiesAdapterViewHolder(View itemView, Action<ActivitiesAdapterClickEventArgs> clickListener, Action<ActivitiesAdapterClickEventArgs> longClickListener,
			Action<ActivitiesAdapterClickEventArgs> likeClickListener, Action<ActivitiesAdapterClickEventArgs> shareClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;


				ImageAct = (ImageView)MainView.FindViewById(Resource.Id.ImageAct);

				TimeUser = MainView.FindViewById<TextView>(Resource.Id.TimeUser);
				ContentAct = MainView.FindViewById<TextView>(Resource.Id.ContentAct);
				LikeActLayout = MainView.FindViewById<LinearLayout>(Resource.Id.LikeActLayout);
				IconLikeAct = MainView.FindViewById<ImageView>(Resource.Id.IconLikeAct);
				CountLikeAct = MainView.FindViewById<TextView>(Resource.Id.CountLikeAct);
				ShareActLayout = MainView.FindViewById<LinearLayout>(Resource.Id.ShareActLayout);
				CommentActLayout = MainView.FindViewById<LinearLayout>(Resource.Id.CommentActLayout);
				CountCommentAct = MainView.FindViewById<TextView>(Resource.Id.CountCommentAct);

				//Create an Event
				itemView.Click += (sender, e) => clickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				//itemView.LongClick += (sender, e) => longClickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });

				LikeActLayout.Click += (sender, e) => likeClickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				ShareActLayout.Click += (sender, e) => shareClickListener(new ActivitiesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}

	public class ActivitiesAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}