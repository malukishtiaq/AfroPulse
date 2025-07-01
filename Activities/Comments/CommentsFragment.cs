using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Activities.Default;
using PlayTube.Activities.Tabbes;
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

namespace PlayTube.Activities.Comments
{
	public class CommentsFragment : Fragment
	{
		#region Variables Basic

		private SwipeRefreshLayout SwipeRefreshLayout;
		private TabbedMainActivity MainContext;
		private View MainView;
		private RelativeLayout RootView;
		private EmojiconEditText EmojiconEditTextView;
		private ImageView UserPic, Emojiicon;
		private CircleButton SendButton;
		private RecyclerView MRecycler;
		private LinearLayoutManager MLayoutManager;
		public CommentsAdapter MAdapter;
		private ProgressBar ProgressBarLoader;
		private View Inflated;
		private ViewStub EmptyStateLayout;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private string VideoId;
		private RelativeLayout CommentButtomLayout;
		private CommentClickListener CommentClickListener;
		private LinearLayout CommentLayout;

		#endregion

		public override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				HasOptionsMenu = true;
				MainContext = TabbedMainActivity.GetInstance();
				CommentClickListener = new CommentClickListener(Activity, "Comment");
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}

		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Activity.Window?.SetSoftInputMode(SoftInput.AdjustResize);

				MainView = inflater?.Inflate(Resource.Layout.CommentsLayout, container, false);

				InitComponent();
				SetRecyclerViewAdapters();

				return MainView;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#region Functions

		private void InitComponent()
		{
			try
			{
				RootView = MainView.FindViewById<RelativeLayout>(Resource.Id.root);
				UserPic = MainView.FindViewById<ImageView>(Resource.Id.user_pic);
				Emojiicon = MainView.FindViewById<ImageView>(Resource.Id.emojiIcon);
				EmojiconEditTextView = MainView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				SendButton = MainView.FindViewById<CircleButton>(Resource.Id.sendButton);
				MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.commentRecyler);
				ProgressBarLoader = MainView.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
				EmptyStateLayout = MainView.FindViewById<ViewStub>(Resource.Id.viewStub);
				CommentButtomLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.commentonButtom);
				CommentLayout = MainView.FindViewById<LinearLayout>(Resource.Id.commentLayout);
				SwipeRefreshLayout = MainView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);

				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
				SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

				SendButton.Click += SendButton_Click;

				if (UserDetails.IsLogin)
				{
					var avatar = ListUtils.MyChannelList.FirstOrDefault()?.Avatar ?? UserDetails.Avatar;
					GlideImageLoader.LoadImage(Activity, avatar, UserPic, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

					ProgressBarLoader.Visibility = ViewStates.Visible;
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

		private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			try
			{
				MAdapter.CommentList.Clear();
				MAdapter.NotifyDataSetChanged();

				MainScrollEvent.IsLoading = false;

				StartApiService(VideoId, "0");
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Event Open Profile User
		private void CommentsAdapter_AvatarClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					MainContext.ShowUserChannelFragment(item.CommentUserData, item.UserId.ToString());
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
							HideKeyboard();

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
					MainContext.ShowReplyCommentFragment(item, "video");
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

		public void StartApiService(string videoId, string offset)
		{
			VideoId = videoId;
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
							if (ProgressBarLoader.Visibility == ViewStates.Visible)
								ProgressBarLoader.Visibility = ViewStates.Gone;

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
							SwipeRefreshLayout.Refreshing = false;
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
				SwipeRefreshLayout.Refreshing = false;

				if (ProgressBarLoader.Visibility == ViewStates.Visible)
					ProgressBarLoader.Visibility = ViewStates.Gone;

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
				SwipeRefreshLayout.Refreshing = false;

				if (ProgressBarLoader.Visibility == ViewStates.Visible)
					ProgressBarLoader.Visibility = ViewStates.Gone;

				Methods.DisplayReportResultTrack(e);
			}
		}

		//No Internet Connection 
		private void EmptyStateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				StartApiService(VideoId, "0");
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
					StartApiService(VideoId, item.Id.ToString());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private void HideKeyboard()
		{
			try
			{
				var inputManager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
				inputManager?.HideSoftInputFromWindow(Activity.CurrentFocus?.WindowToken, HideSoftInputFlags.None);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}
}