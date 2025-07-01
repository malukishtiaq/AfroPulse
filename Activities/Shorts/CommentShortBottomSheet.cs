using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using PlayTube.Activities.Comments;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Activities.Default;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Comment;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlayTube.Activities.Shorts
{
	public class CommentShortBottomSheet : BottomSheetDialogFragment
	{
		#region Variables Basic
		 
		private LinearLayout RootView;
		private EmojiconEditText EmojiconEditTextView;
		private ImageView UserPic, Emojiicon;
		private ImageView SendButton;
		private RecyclerView MRecycler;
		private LinearLayoutManager MLayoutManager;
		private CommentsAdapter MAdapter;
		private View Inflated;
		private ViewStub EmptyStateLayout;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private string VideoId;
		private CommentClickListener CommentClickListener;
		private LinearLayout CommentLayout;
		private static CommentShortBottomSheet Instance;
		private ShortsVideoDetailsActivity ContextShortsVideo;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Create your fragment here
			ContextShortsVideo = (ShortsVideoDetailsActivity)Activity;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
                Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.BottomSheetShortCommentsLayout, container, false);
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

				VideoId = Arguments?.GetString("VideoId") ?? "";
				CommentClickListener = new CommentClickListener(Activity, "CommentShort");
				 
                InitComponent(view);
				SetRecyclerViewAdapters();

				StartApiService();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View MainView)
		{
			try
			{
				RootView = MainView.FindViewById<LinearLayout>(Resource.Id.root);
				UserPic = MainView.FindViewById<ImageView>(Resource.Id.user_pic);
				Emojiicon = MainView.FindViewById<ImageView>(Resource.Id.emojiIcon);
				EmojiconEditTextView = MainView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				SendButton = MainView.FindViewById<ImageView>(Resource.Id.sendButton);
				MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.commentRecyler);
				EmptyStateLayout = MainView.FindViewById<ViewStub>(Resource.Id.viewStub);
				CommentLayout = MainView.FindViewById<LinearLayout>(Resource.Id.commentLayout);
			 
				SendButton.Click += SendButton_Click;

				if (UserDetails.IsLogin)
				{
					var avatar = ListUtils.MyChannelList.FirstOrDefault()?.Avatar ?? UserDetails.Avatar;
					GlideImageLoader.LoadImage(Activity, avatar, UserPic, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
				}
				else
				{
					CommentLayout.Visibility = ViewStates.Gone;
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
				MAdapter = new CommentsAdapter(Activity);
				MLayoutManager = new LinearLayoutManager(Activity);
				MRecycler.SetLayoutManager(MLayoutManager);
				MRecycler.SetAdapter(MAdapter);
				MRecycler.NestedScrollingEnabled = true;
				MAdapter.ReplyClick += CommentsAdapter_ReplyClick;
				MAdapter.AvatarClick += CommentsAdapter_AvatarClick;
				MAdapter.ItemLongClick += MAdapterOnItemLongClick;

				RecyclerViewOnScrollListener recyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
				MainScrollEvent = recyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
				MRecycler.AddOnScrollListener(recyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;

				MRecycler.Visibility = ViewStates.Visible;
				EmptyStateLayout.Visibility = ViewStates.Gone;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events
		 
		//Event Open Profile User
		private void CommentsAdapter_AvatarClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					ContextShortsVideo.OpenEvent = "Profile";
					ContextShortsVideo.UserDataObjectOpenEvent = item.CommentUserData;

					Dismiss();

					ContextShortsVideo.Finish();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Add New Comment
		private async void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					if (!string.IsNullOrEmpty(EmojiconEditTextView.Text) && !string.IsNullOrWhiteSpace(EmojiconEditTextView.Text))
					{
						if (Methods.CheckConnectivity())
						{
							if (MAdapter.CommentList.Count == 0)
							{
								EmptyStateLayout.Visibility = ViewStates.Gone;
								MRecycler.Visibility = ViewStates.Visible;
							}

							//Comment Code
							string time = Methods.Time.TimeAgo(DateTime.Now, false);
							int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
							string time2 = unixTimestamp.ToString();

							//remove \n in a string
							string message = Regex.Replace(EmojiconEditTextView.Text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

							var postId = MAdapter.CommentList.FirstOrDefault(a => a.VideoId == Convert.ToInt32(VideoId))?.PostId ?? 0;

							CommentDataObject comment = new CommentDataObject
							{
								Text = message,
								TextTime = time,
								UserId = Convert.ToInt32(UserDetails.UserId),
								Id = Convert.ToInt32(time2),
								IsCommentOwner = true,
								VideoId = Convert.ToInt32(VideoId),
								CommentUserData = new UserDataObject
								{
									Avatar = UserDetails.Avatar,
									Username = UserDetails.Username,
									Name = UserDetails.FullName,
									Cover = UserDetails.Cover
								},
								CommentReplies = new List<ReplyDataObject>(),
								DisLikes = 0,
								IsDislikedComment = 0,
								IsLikedComment = 0,
								Likes = 0,
								Pinned = "",
								PostId = postId,
								RepliesCount = 0,
								Time = unixTimestamp
							};

							MAdapter.CommentList.Add(comment);
							int index = MAdapter.CommentList.IndexOf(comment);
							MAdapter.NotifyItemInserted(index);
							MRecycler.ScrollToPosition(MAdapter.CommentList.IndexOf(MAdapter.CommentList.Last()));

							//Api request
							var (respondCode, respond) = await RequestsAsync.Comments.AddCommentAsync(VideoId, message);
							if (respondCode.Equals(200))
							{
								if (respond is MessageIdObject @object)
								{
									var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
									if (dataComment != null)
										dataComment.Id = @object.Id;
								}
							}
							else Methods.DisplayReportResult(Activity, respond);

							//Hide keyboard
							EmojiconEditTextView.Text = "";
							EmojiconEditTextView.ClearFocus();
						}
						else
						{
							Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
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
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void CommentsAdapter_ReplyClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					ReplyCommentBottomSheet replyFragment = new ReplyCommentBottomSheet();
					Bundle bundle = new Bundle();

					bundle.PutString("Type", "video");
					bundle.PutString("Object", JsonConvert.SerializeObject(item));
					replyFragment.Arguments = bundle;

					replyFragment.Show(ChildFragmentManager, replyFragment.Tag);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void MAdapterOnItemLongClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				if (e.Position <= -1) return;

				var item = MAdapter.GetItem(e.Position);
				if (item == null) return;

				CommentClickListener?.MoreCommentPostClick(item);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion Events

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

					int countList = MAdapter.CommentList.Count;

					var (apiStatus, respond) = await RequestsAsync.Comments.GetVideoCommentsAsync(VideoId, "20", offset);
					if (apiStatus != 200 || respond is not GetCommentsObject result || result.ListComments == null)
					{
						MainScrollEvent.IsLoading = false;
						Methods.DisplayReportResult(Activity, respond);
					}
					else
					{
						var respondList = result.ListComments.Count;
						if (respondList > 0)
						{
							foreach (var item in from item in result.ListComments let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								MAdapter.CommentList.Add(item);

								if (MAdapter.CommentList.Count % AppSettings.ShowAdNativeCount + 2 == 0)
								{
									MAdapter.CommentList.Add(new CommentDataObject
									{
										Id = 222222,
										TypeView = "Ads"
									});
								}
							}

							if (countList > 0)
							{
								Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
							}
							else
							{
								Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
							}
						}
						else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
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
							MRecycler.Visibility = ViewStates.Gone;

							Inflated = EmptyStateLayout?.Inflate();
							EmptyStateInflater x = new EmptyStateInflater();
							x?.InflateLayout(Inflated, EmptyStateInflater.Type.Login);
							if (!x.EmptyStateButton.HasOnClickListeners)
							{
								x.EmptyStateButton.Click += null;
								x.EmptyStateButton.Click += LoginButtonOnClick;
							}

							EmptyStateLayout.Visibility = ViewStates.Visible;
							MainScrollEvent.IsLoading = false; 
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

				if (MAdapter.CommentList.Count > 0)
				{
					MRecycler.Visibility = ViewStates.Visible;
					EmptyStateLayout.Visibility = ViewStates.Gone;
				}
				else
				{
					MRecycler.Visibility = ViewStates.Gone;

					Inflated ??= EmptyStateLayout?.Inflate();

					EmptyStateInflater x = new EmptyStateInflater();
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
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
				StartApiService();
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
				var item = MAdapter.CommentList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id.ToString());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

        #endregion
		 

    }
}