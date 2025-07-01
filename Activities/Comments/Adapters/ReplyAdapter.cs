using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
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

namespace PlayTube.Activities.Comments.Adapters
{
	public class ReplyAdapter : RecyclerView.Adapter
	{
		public event EventHandler<ReplyAdapterClickEventArgs> ReplyClick;
		public event EventHandler<ReplyAdapterClickEventArgs> AvatarClick;
		public event EventHandler<ReplyAdapterClickEventArgs> ItemClick;
		public event EventHandler<ReplyAdapterClickEventArgs> ItemLongClick;

		private readonly Activity ActivityContext;
		public ObservableCollection<ReplyDataObject> ReplyList = new ObservableCollection<ReplyDataObject>();

		public ReplyAdapter(Activity context)
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
				//Setup your layout here >> Style_Comment
				View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false);
				var vh = new ReplyAdapterViewHolder(itemView, OnClick, OnAvatarClick, OnReplyClick, OnLongClick);
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
				if (viewHolder is ReplyAdapterViewHolder holder)
				{
					var item = ReplyList[position];
					if (item != null)
					{
						if (item.ReplyUserData != null)
						{
							GlideImageLoader.LoadImage(ActivityContext, item.ReplyUserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

							holder.UsernameTextView.Text = AppTools.GetNameFinal(item.ReplyUserData);
						}

						TextSanitizer chnager = new TextSanitizer(holder.CommentText, ActivityContext);
						chnager.Load(Methods.FunString.DecodeString(item.Text));
						holder.TimeTextView.Text = item.TextTime;

						holder.LikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.ReplyLikes));
						holder.UnLikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.ReplyDislikes));
						//holder.RepliesCount.Text = Methods.FunString.FormatPriceValue(item.RepliesCount);

						if (item.IsLikedReply == 1)
						{
							holder.LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							holder.LikeButton.Tag = "1";
						}
						else
						{
							holder.LikeButton.Tag = "0";
						}

						if (item.IsDislikedReply == 1)
						{
							holder.UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							holder.UnLikeButton.Tag = "1";
						}
						else
						{
							holder.UnLikeButton.Tag = "0";
						}

						if (!holder.UnLikeButton.HasOnClickListeners)
							holder.UnLikeButton.Click += (sender, e) => OnUnLikeButtonClick(holder, new ReplyAdapterClickEventArgs { Position = position, View = holder.MainView });
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void OnLikeButtonClick(ReplyAdapterViewHolder holder, ReplyAdapterClickEventArgs e)
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
								item.IsLikedReply = 0;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyLikes = Convert.ToInt32(x);
								}
							}
							else
							{
								holder.LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
								holder.LikeButton.Tag = "1";
								item.IsLikedReply = 1;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									x++;
									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyLikes = Convert.ToInt32(x);
								}
							}

							if (holder.UnLikeButton.Tag?.ToString() == "1")
							{
								holder.UnLikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.UnLikeButton.Tag = "0";
								item.IsDislikedReply = 0;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyDislikes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentReplyAsync(item.Id.ToString(), true) });

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

		private void OnUnLikeButtonClick(ReplyAdapterViewHolder holder, ReplyAdapterClickEventArgs e)
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
								item.IsDislikedReply = 0;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyDislikes = Convert.ToInt32(x);
								}
							}
							else
							{
								holder.UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

								holder.UnLikeButton.Tag = "1";
								item.IsDislikedReply = 1;

								if (!holder.UnLikeNumber.Text.Contains("K") && !holder.UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.UnLikeNumber.Text);
									x++;
									holder.UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyDislikes = Convert.ToInt32(x);
								}
							}

							if (holder.LikeButton.Tag?.ToString() == "1")
							{
								holder.LikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								holder.LikeButton.Tag = "0";
								item.IsLikedReply = 0;

								if (!holder.LikeNumber.Text.Contains("K") && !holder.LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(holder.LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;

									holder.LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									item.ReplyLikes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentReplyAsync(item.Id.ToString(), false) });

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

		public override int ItemCount => ReplyList?.Count ?? 0;


		public ReplyDataObject GetItem(int position)
		{
			return ReplyList[position];
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

		void OnReplyClick(ReplyAdapterClickEventArgs args) => ReplyClick?.Invoke(this, args);
		void OnAvatarClick(ReplyAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
		void OnClick(ReplyAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(ReplyAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
	}

	public class ReplyAdapterViewHolder : RecyclerView.ViewHolder
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

		public ReplyAdapterViewHolder(View itemView, Action<ReplyAdapterClickEventArgs> clickListener, Action<ReplyAdapterClickEventArgs> clickAvatarListener, Action<ReplyAdapterClickEventArgs> clickReplyListener, Action<ReplyAdapterClickEventArgs> longClickListener) : base(itemView)
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
				ReplyButton.Visibility = ViewStates.Invisible;

				//Event
				Image.Click += (sender, e) => clickAvatarListener(new ReplyAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				ReplyButton.Click += (sender, e) => clickReplyListener(new ReplyAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.Click += (sender, e) => clickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
				itemView.LongClick += (sender, e) => longClickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}

	public class ReplyAdapterClickEventArgs : EventArgs
	{
		public View View { get; set; }
		public int Position { get; set; }
	}
}