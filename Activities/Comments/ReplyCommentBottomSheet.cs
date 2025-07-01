using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using PlayTube.Activities.Article;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Activities.Default;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.PlayTubeClient.Classes.Comment;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayTube.Activities.Comments
{
	public class ReplyCommentBottomSheet : BottomSheetDialogFragment, IDialogInterfaceOnShowListener
	{
		#region Variables Basic

		private LinearLayout RootView;
		private View Inflated;
		private ViewStub EmptyStateLayout;
		private EmojiconEditText EmojiconEditTextView;
		private ImageView UserPic, Emojiicon;
		private CircleButton SendButton;
		private RecyclerView ReplyRecyclerView;
		private LinearLayoutManager MLayoutManager;
		public ReplyAdapter ReplyAdapter;
		private ImageView Image;
		private SuperTextView CommentText;
		private TextView TimeTextView;
		private TextView UsernameTextView;
		private ImageView LikeiconView;
		private ImageView UnLikeiconView;
		private ImageView ReplyiconView;
		private TextView LikeNumber;
		private TextView UnLikeNumber;
		private TextView RepliesCount;
		private LinearLayout LikeButton;
		private FrameLayout UnLikeButton;
		private LinearLayout RreplyLayout;

		private CommentDataObject Comment = new CommentDataObject();
		private TabbedMainActivity ActivityContext;
		private string Type;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private CommentClickListener CommentClickListener;
		private static ReplyCommentBottomSheet Instance;
		#endregion

		#region General

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

				View view = localInflater?.Inflate(Resource.Layout.BottomSheetReplyLayout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null!;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);
				Instance = this;

				Dialog.SetOnShowListener(this);

				Type = Arguments?.GetString("Type");

				Comment = JsonConvert.DeserializeObject<CommentDataObject>(Arguments?.GetString("Object") ?? "");

				if (Type == "video") ActivityContext = TabbedMainActivity.GetInstance();

				InitComponent(view);
				SetRecyclerViewAdapters();

				CommentClickListener = new CommentClickListener(ActivityContext, "Reply");
				SendButton.Click += SendButton_Click;
				LikeButton.Click += OnLikeButtonClick;
				UnLikeButton.Click += OnUnLikeButtonClick;

				LoadReplies();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
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

		private void InitComponent(View contentView)
		{
			try
			{
				Image = contentView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
				CommentText = contentView.FindViewById<SuperTextView>(Resource.Id.active);
				UsernameTextView = contentView.FindViewById<TextView>(Resource.Id.username);
				TimeTextView = contentView.FindViewById<TextView>(Resource.Id.time);
				LikeiconView = contentView.FindViewById<ImageView>(Resource.Id.Likeicon);
				UnLikeiconView = contentView.FindViewById<ImageView>(Resource.Id.UnLikeicon);
				ReplyiconView = contentView.FindViewById<ImageView>(Resource.Id.ReplyIcon);
				LikeNumber = contentView.FindViewById<TextView>(Resource.Id.LikeNumber);
				UnLikeNumber = contentView.FindViewById<TextView>(Resource.Id.UnLikeNumber);
				RepliesCount = contentView.FindViewById<TextView>(Resource.Id.RepliesCount);
				LikeButton = contentView.FindViewById<LinearLayout>(Resource.Id.LikeButton);
				UnLikeButton = contentView.FindViewById<FrameLayout>(Resource.Id.UnLikeButton);
				RreplyLayout = contentView.FindViewById<LinearLayout>(Resource.Id.replyButton);

				RootView = contentView.FindViewById<LinearLayout>(Resource.Id.root);
				EmojiconEditTextView = contentView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				UserPic = contentView.FindViewById<ImageView>(Resource.Id.user_pic);
				Emojiicon = contentView.FindViewById<ImageView>(Resource.Id.emojiIcon);
				SendButton = contentView.FindViewById<CircleButton>(Resource.Id.sendButton);
				ReplyRecyclerView = contentView.FindViewById<RecyclerView>(Resource.Id.recyler);
				EmptyStateLayout = contentView.FindViewById<ViewStub>(Resource.Id.viewStub);

				if (UserDetails.IsLogin)
				{
					var avatar = ListUtils.MyChannelList.FirstOrDefault()?.Avatar ?? UserDetails.Avatar;
					GlideImageLoader.LoadImage(Activity, avatar, UserPic, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
				}
				else
				{
					RreplyLayout.Visibility = ViewStates.Gone;
				}

				var emojisIcon = new EmojIconActions(Context, RootView, EmojiconEditTextView, Emojiicon);
				emojisIcon.ShowEmojIcon();
				emojisIcon.SetIconsIds(Resource.Drawable.ic_action_keyboard, Resource.Drawable.icon_smile_vector);
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
				ReplyAdapter = new ReplyAdapter(Activity);
				MLayoutManager = new LinearLayoutManager(Activity);
				ReplyRecyclerView.SetLayoutManager(MLayoutManager);
				ReplyRecyclerView.SetAdapter(ReplyAdapter);
				ReplyAdapter.AvatarClick += ReplyAdapterOnAvatarClick;
				ReplyAdapter.ReplyClick += ReplyAdapterOnReplyClick;
				ReplyAdapter.ItemLongClick += ReplyAdapterOnItemLongClick;

				RecyclerViewOnScrollListener recyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
				MainScrollEvent = recyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
				ReplyRecyclerView.AddOnScrollListener(recyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static ReplyCommentBottomSheet GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}
		private void ReplyAdapterOnItemLongClick(object sender, ReplyAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = ReplyAdapter.GetItem(e.Position);
				if (item == null) return;

				CommentClickListener.MoreReplyPostClick(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Get Replies

		private void LoadReplies()
		{
			try
			{
				if (Comment == null) return;

				GlideImageLoader.LoadImage(ActivityContext, Comment.CommentUserData.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				TextSanitizer chnager = new TextSanitizer(CommentText, ActivityContext);
				chnager.Load(Methods.FunString.DecodeString(Comment.Text));
				TimeTextView.Text = Comment.TextTime;

				UsernameTextView.Text = AppTools.GetNameFinal(Comment.CommentUserData);

				LikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(Comment.Likes));
				UnLikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(Comment.DisLikes));
				RepliesCount.Text = Methods.FunString.FormatPriceValue(Comment.RepliesCount);

				if (Comment.IsLikedComment == 1)
				{
					LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
					LikeButton.Tag = "1";
				}
				else
				{
					LikeButton.Tag = "0";
				}

				if (Comment.IsDislikedComment == 1)
				{
					UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
					UnLikeButton.Tag = "1";
				}
				else
				{
					UnLikeButton.Tag = "0";
				}

				if (Comment.CommentReplies?.Count > 0)
				{
					ReplyAdapter.ReplyList = new ObservableCollection<ReplyDataObject>(Comment.CommentReplies);
					ReplyAdapter.NotifyDataSetChanged();
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}

				string offset = ReplyAdapter.ReplyList.LastOrDefault()?.Id.ToString() ?? "0";
				StartApiService(offset);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Load Data Api

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (Methods.CheckConnectivity())
			{
				if (UserDetails.IsLogin)
				{
					if (MainScrollEvent.IsLoading)
						return;

					MainScrollEvent.IsLoading = true;

					int countList = ReplyAdapter.ReplyList.Count;

					var (apiStatus, respond) = await RequestsAsync.Comments.GetRepliesAsync(Comment.Id.ToString(), "20", offset);
					if (apiStatus != 200 || respond is not GetRepliesObject result || result.Data == null)
					{
						MainScrollEvent.IsLoading = false;
						Methods.DisplayReportResult(Activity, respond);
					}
					else
					{
						var respondList = result.Data.Count;
						if (respondList > 0)
						{
							if (countList > 0)
							{
								foreach (var item in from item in result.Data let check = ReplyAdapter.ReplyList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
								{
									ReplyAdapter.ReplyList.Insert(0, item);
								}

								Activity?.RunOnUiThread(() => { ReplyAdapter.NotifyItemRangeInserted(countList, ReplyAdapter.ReplyList.Count - countList); });
							}
							else
							{
								ReplyAdapter.ReplyList = new ObservableCollection<ReplyDataObject>(result.Data);
								Activity?.RunOnUiThread(() => { ReplyAdapter.NotifyDataSetChanged(); });
							}
						}
						else if (ReplyAdapter.ReplyList.Count > 10 && !ReplyRecyclerView.CanScrollVertically(1))
						{
							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short)?.Show();
						}
					}

					Activity?.RunOnUiThread(ShowEmptyPage);
				}
				else
				{
					Activity?.RunOnUiThread(() =>
					{
						try
						{
							ReplyRecyclerView.Visibility = ViewStates.Gone;

							Inflated = EmptyStateLayout?.Inflate();
							EmptyStateInflater x = new EmptyStateInflater();
							x?.InflateLayout(Inflated, EmptyStateInflater.Type.Login);
							if (!x.EmptyStateButton.HasOnClickListeners)
							{
								x.EmptyStateButton.Click += null;
								x.EmptyStateButton.Click += LoginButtonOnClick;
							}

							EmptyStateLayout.Visibility = ViewStates.Visible;
						}
						catch (Exception e)
						{
							Methods.DisplayReportResultTrack(e);
						}
					});
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

				Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void LoginButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				Activity.StartActivity(new Intent(Activity, typeof(LoginActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ShowEmptyPage()
		{
			try
			{
				MainScrollEvent.IsLoading = false;

				if (ReplyAdapter.ReplyList.Count > 0)
				{
					ReplyRecyclerView.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					ReplyRecyclerView.Visibility = ViewStates.Gone;

					if (Inflated == null)
						Inflated = EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoReplies);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
					}
					EmptyStateLayout.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				MainScrollEvent.IsLoading = false;

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

		#region Events

		private void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(EmojiconEditTextView.Text) && !string.IsNullOrWhiteSpace(EmojiconEditTextView.Text))
				{
					if (UserDetails.IsLogin)
					{
						if (Methods.CheckConnectivity())
						{
							//Comment Code 
							string time = Methods.Time.TimeAgo(DateTime.Now, false);
							EmojiconEditTextView.ClearFocus();

							var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
							string time2 = unixTimestamp.ToString();

							//remove \n in a string
							string message = Regex.Replace(EmojiconEditTextView.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

							ReplyDataObject comment = new ReplyDataObject
							{
								Text = message,
								TextTime = time,
								UserId = Convert.ToInt32(UserDetails.UserId),
								Id = Convert.ToInt32(time2),
								IsReplyOwner = true,
								PostId = Convert.ToInt32(Comment.PostId),
								ReplyUserData = new UserDataObject
								{
									Avatar = UserDetails.Avatar,
									Username = UserDetails.Username,
									Name = UserDetails.FullName,
								}
							};

							EmptyStateLayout.Visibility = ViewStates.Gone;
							ReplyRecyclerView.Visibility = ViewStates.Visible;
							ReplyAdapter.ReplyList.Add(comment);
							ReplyAdapter.NotifyItemInserted(ReplyAdapter.ReplyList.Count - 1);
							ReplyRecyclerView.ScrollToPosition(ReplyAdapter.ReplyList.Count - 1);
							var x = Convert.ToInt32(Comment.RepliesCount);
							RepliesCount.Text = Methods.FunString.FormatPriceValue(++x);

							switch (Type)
							{
								case "video":
									{
										var dataComments = ActivityContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);
										if (dataComments != null)
										{
											dataComments.RepliesCount++;

											if (dataComments.CommentReplies?.Count > 0)
											{
												dataComments.CommentReplies.Add(comment);
											}
											else
											{
												dataComments.CommentReplies = new List<ReplyDataObject> { comment };
											}

											int index = ActivityContext.VideoDataWithEventsLoader.CommentsFragment.MAdapter.CommentList.IndexOf(dataComments);
											ActivityContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter.NotifyItemChanged(index);
										}

										//Api request  
										Task.Run(async () =>
										{
											var (respondCode, respond) = await RequestsAsync.Comments.ReplyVideoCommentsAsync(Comment.VideoId.ToString(), Comment.Id.ToString(), message);
											if (respondCode.Equals(200))
											{
												if (respond is AddReplyObject result)
												{
													var dataComment = ReplyAdapter.ReplyList.FirstOrDefault(a => a.Id == int.Parse(time2));
													if (dataComment != null)
														dataComment.Id = result.ReplyId;
												}
											}
											else Methods.DisplayReportResult(Activity, respond);
										});
										break;
									}
								case "Article":
									{
										var dataComments = ShowArticleActivity.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);
										if (dataComments != null)
										{
											dataComments.RepliesCount++;

											if (dataComments.CommentReplies?.Count > 0)
											{
												dataComments.CommentReplies.Add(comment);
											}
											else
											{
												dataComments.CommentReplies = new List<ReplyDataObject> { comment };
											}

											int index = ShowArticleActivity.MAdapter.CommentList.IndexOf(dataComments);
											ShowArticleActivity.MAdapter.NotifyItemChanged(index);
										}

										//Api request  
										Task.Run(async () =>
										{
											var (respondCode, respond) = await RequestsAsync.Articles.ReplyArticlesCommentsAsync(Comment.PostId.ToString(), Comment.Id.ToString(), message);
											if (respondCode.Equals(200))
											{
												if (respond is AddReplyObject result)
												{
													var dataComment = ReplyAdapter.ReplyList.FirstOrDefault(a => a.Id == int.Parse(time2));
													if (dataComment != null)
														dataComment.Id = result.ReplyId;
												}
											}
											else Methods.DisplayReportResult(Activity, respond);
										});
										break;
									}
							}

							//Hide keyboard
							EmojiconEditTextView.Text = "";
							EmojiconEditTextView.ClearFocus();
						}
						else
						{
							Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning),
							Activity.GetText(Resource.String.Lbl_Please_sign_in_comment),
							Activity.GetText(Resource.String.Lbl_Yes),
							Activity.GetText(Resource.String.Lbl_No));
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void OnUnLikeButtonClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						dynamic dataComments = Type == "video" ? ActivityContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id) : ShowArticleActivity.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);

						if (dataComments != null)
						{
							if (UnLikeButton.Tag?.ToString() == "1")
							{
								UnLikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								UnLikeButton.Tag = "0";
								dataComments.IsDislikedComment = 0;

								if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.DisLikes = Convert.ToInt32(x);
								}
							}
							else
							{
								UnLikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

								UnLikeButton.Tag = "1";
								dataComments.IsDislikedComment = 1;

								if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(UnLikeNumber.Text);
									x++;
									UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.DisLikes = Convert.ToInt32(x);
								}
							}

							if (LikeButton.Tag?.ToString() == "1")
							{
								LikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								LikeButton.Tag = "0";
								dataComments.IsLikedComment = 0;

								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;

									LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.Likes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentAsync(dataComments.Id.ToString(), false) });

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

		private void OnLikeButtonClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						dynamic dataComments = Type == "video" ? ActivityContext?.VideoDataWithEventsLoader?.CommentsFragment?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id) : ShowArticleActivity.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);

						if (dataComments != null)
						{
							if (LikeButton.Tag?.ToString() == "1")
							{
								LikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								LikeButton.Tag = "0";
								dataComments.IsLikedComment = 0;

								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(LikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.Likes = Convert.ToInt32(x);
								}
							}
							else
							{
								LikeiconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
								LikeButton.Tag = "1";
								dataComments.IsLikedComment = 1;

								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(LikeNumber.Text);
									x++;
									LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.Likes = Convert.ToInt32(x);
								}
							}

							if (UnLikeButton.Tag?.ToString() == "1")
							{
								UnLikeiconView.SetColorFilter(Color.ParseColor("#777777"));

								UnLikeButton.Tag = "0";
								dataComments.IsDislikedComment = 0;

								if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
								{
									double x = Convert.ToDouble(UnLikeNumber.Text);
									if (x > 0)
										x--;
									else
										x = 0;
									UnLikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
									dataComments.DisLikes = Convert.ToInt32(x);
								}
							}
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentAsync(dataComments.Id.ToString(), true) });

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

		private void ReplyAdapterOnAvatarClick(object sender, ReplyAdapterClickEventArgs e)
		{
			try
			{
				var item = ReplyAdapter.GetItem(e.Position);
				if (item != null)
					ActivityContext.ShowUserChannelFragment(item.ReplyUserData, item.UserId.ToString());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ReplyAdapterOnReplyClick(object sender, ReplyAdapterClickEventArgs e)
		{
			try
			{
				var item = ReplyAdapter.GetItem(e.Position);
				if (item != null)
				{
					EmojiconEditTextView.Text = "";
					EmojiconEditTextView.Text = "@" + item.ReplyUserData.Username + " ";

					int pos = EmojiconEditTextView.Text.Length;
					EmojiconEditTextView.SetSelection(pos);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Scroll

		private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Code get last id where LoadMore >>
				var item = ReplyAdapter.ReplyList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id.ToString());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		public void OnShow(IDialogInterface dialog)
		{
			try
			{
				var d = dialog as BottomSheetDialog;
				var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
				var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
				var layoutParams = bottomSheet.LayoutParameters;

				if (layoutParams != null)
					layoutParams.Height = Resources.DisplayMetrics.HeightPixels;
				bottomSheet.LayoutParameters = layoutParams;
				bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}
}