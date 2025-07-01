using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.PlayTubeClient.Classes.Comment;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Comments.Adapters
{
	public class CommentsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
	{
		public event EventHandler<CommentAdapterClickEventArgs> ReplyClick;
		public event EventHandler<CommentAdapterClickEventArgs> AvatarClick;
		public event EventHandler<CommentAdapterClickEventArgs> ItemClick;
		public event EventHandler<CommentAdapterClickEventArgs> ItemLongClick;

		private readonly Activity ActivityContext;
		public ObservableCollection<CommentDataObject> CommentList = new ObservableCollection<CommentDataObject>();

		public CommentsAdapter(Activity context)
		{
			try
			{
				ActivityContext = context;
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
				//Ads
				if (viewType == 222222)
				{
					var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob5, parent, false);
					var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, ActivityContext);
					return vh;
				}
				else
				{
					//Setup your layout here >> Style_Comment
					View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false);
					var vh = new CommentAdapterViewHolder(itemView, OnClick, OnAvatarClick, OnReplyClick, OnLongClick);
					return vh;
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
				var item = CommentList[position];
				if (item?.TypeView != "Ads")
				{
					if (viewHolder is CommentAdapterViewHolder holder)
					{
						if (item.CommentUserData != null)
						{
							GlideImageLoader.LoadImage(ActivityContext, item.CommentUserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

							holder.UsernameTextView.Text = AppTools.GetNameFinal(item.CommentUserData);
						}

						TextSanitizer chnager = new TextSanitizer(holder.CommentText, ActivityContext);
						chnager.Load(Methods.FunString.DecodeString(item.Text));
						holder.TimeTextView.Text = item.TextTime;

						holder.LikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Likes));
						holder.UnLikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.DisLikes));
						holder.RepliesCount.Text = Methods.FunString.FormatPriceValue(item.RepliesCount) + " " + ActivityContext.GetText(Resource.String.Lbl_Replies);

						if (item.IsLikedComment == 1)
						{
							holder.LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							holder.LikeButton.Tag = "1";
						}
						else
						{
							holder.LikeButton.Tag = "0";
						}

						if (item.IsDislikedComment == 1)
						{
							holder.UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							holder.UnLikeButton.Tag = "1";
						}
						else
						{
							holder.UnLikeButton.Tag = "0";
						}

						if (!holder.LikeButton.HasOnClickListeners)
							holder.LikeButton.Click += (sender, e) => OnLikeButtonClick(holder, new CommentAdapterClickEventArgs { Position = position, View = holder.MainView });

						if (!holder.UnLikeButton.HasOnClickListeners)
							holder.UnLikeButton.Click += (sender, e) => OnUnLikeButtonClick(holder, new CommentAdapterClickEventArgs { Position = position, View = holder.MainView });
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void OnLikeButtonClick(CommentAdapterViewHolder holder, CommentAdapterClickEventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						var item = GetItem(e.Position);
						if (item != null)
						{
							if (holder.LikeButton.Tag?.ToString() == "1")
							{
								holder.LikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.LikeButton.Tag = "0";
								item.IsLikedComment = 0;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.Likes = Convert.ToInt32(x);
								}
							}
							else
							{
								holder.LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
								holder.LikeButton.Tag = "1";
								item.IsLikedComment = 1;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									x++;
									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.Likes = Convert.ToInt32(x);
								}
							}

							if (holder.UnLikeButton.Tag?.ToString() == "1")
							{
								holder.UnLikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.UnLikeButton.Tag = "0";
								item.IsDislikedComment = 0;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.DisLikes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentAsync(item.Id.ToString(), true) });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
						dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_Like), ActivityContext.GetText(Resource.String.Lbl_Yes),
							ActivityContext.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void OnUnLikeButtonClick(CommentAdapterViewHolder holder, CommentAdapterClickEventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						var item = GetItem(e.Position);
						if (item != null)
						{
							if (holder.UnLikeButton.Tag?.ToString() == "1")
							{
								holder.UnLikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.UnLikeButton.Tag = "0";
								item.IsDislikedComment = 0;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.DisLikes = Convert.ToInt32(x);
								}
							}
							else
							{
								holder.UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

								holder.UnLikeButton.Tag = "1";
								item.IsDislikedComment = 1;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									x++;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.DisLikes = Convert.ToInt32(x);
								}
							}

							if (holder.LikeButton.Tag?.ToString() == "1")
							{
								holder.LikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.LikeButton.Tag = "0";
								item.IsLikedComment = 0;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;

									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.Likes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentAsync(item.Id.ToString(), false) });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(ActivityContext, null, "Login");
						dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning),
							ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_Dislike),
							ActivityContext.GetText(Resource.String.Lbl_Yes),
							ActivityContext.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override int ItemCount => CommentList?.Count ?? 0;

		public CommentDataObject GetItem(int position)
		{
			return CommentList[position];
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
				var item = GetItem(position);
				if (item != null)
				{
					if (item.TypeView == "Ads")
					{
						return 222222;
					}
					else
					{
						return 1111;
					}
				}
				return 1111;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return 1111;
			}
		}
		void OnReplyClick(CommentAdapterClickEventArgs args) => ReplyClick?.Invoke(this, args);
		void OnAvatarClick(CommentAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
		void OnClick(CommentAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(CommentAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

		public IList GetPreloadItems(int p0)
		{
			try
			{
				var d = new List<string>();
				var item = CommentList[p0];

				if (item == null)
					return Collections.SingletonList(p0);

				if (item.CommentUserData.Avatar != "")
				{
					d.Add(item.CommentUserData.Avatar);
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

	public class CommentAdapterViewHolder : RecyclerView.ViewHolder
	{
		#region Variables Basic

		public View MainView { get; private set; }
		public ImageView Image { get; private set; }
		public SuperTextView CommentText { get; private set; }

		public TextView TimeTextView { get; private set; }
		public TextView UsernameTextView { get; private set; }

		public ImageView LikeiconView { get; private set; }
		public ImageView UnLikeiconView { get; private set; }
		public ImageView ReplyiconView { get; private set; }
		public TextView LikeNumber { get; private set; }
		public TextView UnLikeNumber { get; private set; }
		public TextView RepliesCount { get; private set; }
		public LinearLayout LikeButton { get; private set; }
		public FrameLayout UnLikeButton { get; private set; }
		public LinearLayout ReplyButton { get; private set; }
		#endregion

		public CommentAdapterViewHolder(View itemView, Action<CommentAdapterClickEventArgs> clickListener, Action<CommentAdapterClickEventArgs> clickAvatarListener, Action<CommentAdapterClickEventArgs> clickReplyListener, Action<CommentAdapterClickEventArgs> longClickListener) : base(itemView)
		{
			try
			{
				MainView = itemView;

				Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
				CommentText = MainView.FindViewById<SuperTextView>(Resource.Id.active);

				UsernameTextView = MainView.FindViewById<TextView>(Resource.Id.username);
				TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);

				LikeiconView = MainView.FindViewById<ImageView>(Resource.Id.Likeicon);
				UnLikeiconView = MainView.FindViewById<ImageView>(Resource.Id.UnLikeicon);
				ReplyiconView = MainView.FindViewById<ImageView>(Resource.Id.ReplyIcon);

				LikeNumber = MainView.FindViewById<TextView>(Resource.Id.LikeNumber);
				UnLikeNumber = MainView.FindViewById<TextView>(Resource.Id.UnLikeNumber);
				RepliesCount = MainView.FindViewById<TextView>(Resource.Id.RepliesCount);

				LikeButton = MainView.FindViewById<LinearLayout>(Resource.Id.LikeButton);
				UnLikeButton = MainView.FindViewById<FrameLayout>(Resource.Id.UnLikeButton);
				ReplyButton = MainView.FindViewById<LinearLayout>(Resource.Id.ReplyButton);

				//Event
				Image.Click += (sender, e) => clickAvatarListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				ReplyButton.Click += (sender, e) => clickReplyListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.Click += (sender, e) => clickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new CommentAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}

	public class CommentAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}