using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Comments;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.Library.Anjo.Share;
using PlayTube.Library.Anjo.Share.Abstractions;
using PlayTube.PlayTubeClient.Classes.Comment;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Article
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class ShowArticleActivity : BaseActivity, IDialogListCallBack
	{
		#region Variables Basic

		private CoordinatorLayout RootView;

		private ImageView ImageBlog, UserImage, ImageChannel;
		private TextView TxtTitle, TxtInfo, CategoryName, TxtChannelName, TxtChannelViews, TxtDescription, TxtCountComments;

		private LinearLayout LikeButton, UnLikeButton;

		private ImageView LikeIcon, UnlikeIcon;
		private TextView LikeCount, UnlikeCount;

		private WebView TxtHtml;
		private EmojiconEditText EmojIconEditTextView;
		private ImageView EmojIcon;
		private ImageView SendButton;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		public static CommentsAdapter MAdapter;
		private ProgressBar ProgressBarLoader;
		private View Inflated;
		private ViewStub EmptyStateLayout;
		private NestedScrollViewOnScroll MainScrollEvent;

		private ArticleDataObject ArticleData;
		private string ArticleId, DataWebHtml;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this);

				Window?.SetSoftInputMode(SoftInput.AdjustResize);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.ArticlesViewLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				SetRecyclerViewAdapters();

				GetDataArticles();

				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnResume()
		{
			try
			{
				base.OnResume();
				AddOrRemoveEvent(true);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();
				AddOrRemoveEvent(false);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnTrimMemory(TrimMemory level)
		{
			try
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				base.OnTrimMemory(level);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
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

		#region Menu

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;

				case Resource.Id.action_share:
					ShareEvent();
					break;

				case Resource.Id.action_copy:
					CopyLinkEvent();
					break;

			}

			return base.OnOptionsItemSelected(item);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater?.Inflate(Resource.Menu.MenuArticleShare, menu);
			ChangeMenuIconColor(menu, AppTools.IsTabDark() ? Color.White : Color.Black);

			return base.OnCreateOptionsMenu(menu);

		}

		private void ChangeMenuIconColor(IMenu menu, Color color)
		{
			for (int i = 0; i < menu.Size(); i++)
			{
				var drawable = menu.GetItem(i).Icon;
				if (drawable == null) continue;
				drawable.Mutate();
				drawable.SetColorFilter(new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop));
			}
		}

		#endregion

		#region Functions

		private void InitComponent()
		{
			try
			{
				RootView = FindViewById<CoordinatorLayout>(Resource.Id.root);

				ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog);
				CategoryName = FindViewById<TextView>(Resource.Id.textCategory);

				TxtTitle = FindViewById<TextView>(Resource.Id.Title);
				TxtInfo = FindViewById<TextView>(Resource.Id.info);

				ImageChannel = FindViewById<ImageView>(Resource.Id.Image_Channel);
				TxtChannelName = FindViewById<TextView>(Resource.Id.ChannelName);
				TxtChannelViews = FindViewById<TextView>(Resource.Id.ChannelViews);

				LikeButton = FindViewById<LinearLayout>(Resource.Id.LikeButton);
				LikeIcon = FindViewById<ImageView>(Resource.Id.LikeIcon);
				LikeCount = FindViewById<TextView>(Resource.Id.LikeNumber);

				UnLikeButton = FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
				UnlikeIcon = FindViewById<ImageView>(Resource.Id.UnLikeIcon);
				UnlikeCount = FindViewById<TextView>(Resource.Id.UnLikeNumber);

				TxtDescription = FindViewById<TextView>(Resource.Id.description);

				TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView);

				TxtCountComments = FindViewById<TextView>(Resource.Id.countComments);

				MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
				EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
				ProgressBarLoader = FindViewById<ProgressBar>(Resource.Id.sectionProgress);
				ProgressBarLoader.Visibility = ViewStates.Visible;

				UserImage = FindViewById<ImageView>(Resource.Id.userImage);
				EmojIconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				EmojIcon = FindViewById<ImageView>(Resource.Id.emojiIcon);
				SendButton = FindViewById<ImageView>(Resource.Id.sendButton);

				EmojIconActions emojis = new EmojIconActions(this, RootView, EmojIconEditTextView, EmojIcon);
				emojis.ShowEmojIcon();
				emojis.SetIconsIds(Resource.Drawable.ic_action_keyboard, Resource.Drawable.icon_smile_vector);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar()
		{
			try
			{
				Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					toolbar.Title = "";
					toolbar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(toolbar);
					SupportActionBar.SetDisplayShowCustomEnabled(true);
					SupportActionBar.SetDisplayHomeAsUpEnabled(true);
					SupportActionBar.SetHomeButtonEnabled(true);
					SupportActionBar.SetDisplayShowHomeEnabled(true);

					var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
					icon?.SetTint(AppTools.IsTabDark() ? Color.White : Color.Black);
					SupportActionBar.SetHomeAsUpIndicator(icon);
				}
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
				MAdapter = new CommentsAdapter(this)
				{
					CommentList = new ObservableCollection<CommentDataObject>()
				};
				LayoutManager = new LinearLayoutManager(this);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				var sizeProvider = new FixedPreloadSizeProvider(10, 10);
				var preLoader = new RecyclerViewPreloader<CommentDataObject>(this, MAdapter, sizeProvider, 10);
				MRecycler.AddOnScrollListener(preLoader);
				MRecycler.SetAdapter(MAdapter);

				var nestedScrollView = FindViewById<NestedScrollView>(Resource.Id.nested_scroll_view);

				NestedScrollViewOnScroll recyclerViewOnScrollListener = new NestedScrollViewOnScroll();
				MainScrollEvent = recyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
				nestedScrollView.SetOnScrollChangeListener(recyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -= 
				if (addEvent)
				{
					SendButton.Click += SendButton_Click;
					MAdapter.ReplyClick += CommentsAdapterOnReplyClick;
					LikeButton.Click += LikeButtonOnClick;
					UnLikeButton.Click += UnLikeButtonOnClick;
				}
				else
				{
					SendButton.Click -= SendButton_Click;
					MAdapter.ReplyClick -= CommentsAdapterOnReplyClick;
					LikeButton.Click -= LikeButtonOnClick;
					UnLikeButton.Click -= UnLikeButtonOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void BtnMoreOnClick(object sender, EventArgs e)
		{
			try
			{
				var arrayAdapter = new List<string>();
				var dialogList = new MaterialAlertDialogBuilder(this);

				arrayAdapter.Add(GetString(Resource.String.Lbl_Copy));
				arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Event Menu >> Copy Link
		private void CopyLinkEvent()
		{
			try
			{
				var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

				var clipData = ClipData.NewPlainText("text", ArticleData.Url);
				clipboardManager.PrimaryClip = clipData;

				Toast.MakeText(this, GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Event Menu >> Share
		private async void ShareEvent()
		{
			try
			{
				//Share Plugin same as video
				if (!CrossShare.IsSupported) return;

				await CrossShare.Current.Share(new ShareMessage
				{
					Title = ArticleData.Title,
					Text = " ",
					Url = ArticleData.Url
				});
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void CommentsAdapterOnReplyClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					// show dialog :
					ReplyCommentBottomSheet replyFragment = new ReplyCommentBottomSheet();
					Bundle bundle = new Bundle();

					bundle.PutString("Type", "Article");
					bundle.PutString("Object", JsonConvert.SerializeObject(item));
					replyFragment.Arguments = bundle;

					replyFragment.Show(SupportFragmentManager, replyFragment.Tag);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					if (!string.IsNullOrEmpty(EmojIconEditTextView.Text))
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
							var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
							string time2 = unixTimestamp.ToString();
							string message = EmojIconEditTextView.Text;
							var item = MAdapter.CommentList.FirstOrDefault(a => a.PostId == Convert.ToInt32(ArticleId));
							var postId = item?.PostId ?? 0;
							var videoId = item?.VideoId ?? 0;

							CommentDataObject comment = new CommentDataObject
							{
								Text = message,
								TextTime = time,
								UserId = Convert.ToInt32(UserDetails.UserId),
								Id = Convert.ToInt32(time2),
								IsCommentOwner = true,
								VideoId = videoId,
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
							Task.Run(async () =>
							{
								var (respondCode, respond) = await RequestsAsync.Articles.AddArticlesCommentAsync(ArticleId, message);
								if (respondCode.Equals(200))
								{
									if (respond is MessageIdObject messageId)
									{
										var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
										if (dataComment != null)
											dataComment.Id = messageId.Id;
									}
								}
								else Methods.DisplayReportResult(this, respond);
							});

							//Hide keyboard
							EmojIconEditTextView.Text = "";
							EmojIconEditTextView.ClearFocus();
						}
						else
						{
							Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(this, null, "Login");
					dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_comment), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LikeButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						try
						{
							//If User Liked
							if (LikeButton.Tag?.ToString() == "0")
							{
								LikeButton.Tag = "1";
								LikeIcon.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

								UnlikeIcon.SetColorFilter(Color.ParseColor("#8e8e8e"));
								if (!LikeCount.Text.Contains("K") && !LikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeCount.Text);
									x++;
									LikeCount.Text = x.ToString(CultureInfo.InvariantCulture);
								}

								if (UnLikeButton.Tag?.ToString() == "1")
								{
									UnLikeButton.Tag = "0";
									if (!UnlikeCount.Text.Contains("K") && !UnlikeCount.Text.Contains("M"))
									{
										var x = Convert.ToDouble(UnlikeCount.Text);
										if (x > 0)
										{
											x--;
										}
										else
										{
											x = 0;
										}
										UnlikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
									}
								}

								Toast.MakeText(this, GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Like
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(ArticleId, "like") });

							}
							else
							{
								LikeButton.Tag = "0";

								LikeIcon.SetColorFilter(Color.ParseColor("#8e8e8e"));
								UnlikeIcon.SetColorFilter(Color.ParseColor("#8e8e8e"));
								if (!LikeCount.Text.Contains("K") && !LikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeCount.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									LikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
								}

								Toast.MakeText(this, GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Remove UNLike
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.AddLikeDislikeAsync(ArticleId, "like") });
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(this, null, "Login");
						dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Like), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void UnLikeButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (UnLikeButton.Tag?.ToString() == "0")
						{
							UnLikeButton.Tag = "1";
							UnlikeIcon.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							LikeIcon.SetColorFilter(Color.ParseColor("#8e8e8e"));

							if (!UnlikeCount.Text.Contains("K") && !UnlikeCount.Text.Contains("M"))
							{
								var x = Convert.ToDouble(UnlikeCount.Text);
								x++;
								UnlikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							if (LikeButton.Tag?.ToString() == "1")
							{
								LikeButton.Tag = "0";
								if (!LikeCount.Text.Contains("K") && !LikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeCount.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									LikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
								}
							}

							Toast.MakeText(this, GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short)?.Show();

							//Send API Request here for dislike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.AddLikeDislikeAsync(ArticleId, "dislike") });
						}
						else
						{
							UnLikeButton.Tag = "0";

							UnlikeIcon.SetColorFilter(Color.ParseColor("#8e8e8e"));
							var x = Convert.ToDouble(UnlikeCount.Text);
							if (!UnlikeCount.Text.Contains("K") && !UnlikeCount.Text.Contains("M"))
							{
								if (x > 0)
								{
									x--;
								}
								else
								{
									x = 0;
								}
								UnlikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							//Send API Request here for Remove UNLike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.AddLikeDislikeAsync(ArticleId, "dislike") });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(this, null, "Login");
						dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Dislike), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				string text = itemString;
				if (text == GetString(Resource.String.Lbl_Copy))
				{
					CopyLinkEvent();
				}
				else if (text == GetString(Resource.String.Lbl_Share))
				{
					ShareEvent();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region scroll

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

		#region Load Data Api 

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			else
				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
		}

		private async Task LoadDataAsync(string offset = "0")
		{
			if (MainScrollEvent.IsLoading)
				return;

			if (Methods.CheckConnectivity())
			{
				MainScrollEvent.IsLoading = true;
				int countList = MAdapter.CommentList.Count;

				var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesCommentsAsync(ArticleId, "20", offset);
				if (apiStatus != 200 || respond is not GetCommentsObject result || result.ListComments == null)
				{
					MainScrollEvent.IsLoading = false;
					Methods.DisplayReportResult(this, respond);
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
							RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
						}
						else
						{
							RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short)?.Show();
					}
				}

				MainScrollEvent.IsLoading = false;
				RunOnUiThread(ShowEmptyPage);
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

				Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				MainScrollEvent.IsLoading = false;
			}
		}

		private void ShowEmptyPage()
		{
			try
			{
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

					if (Inflated == null)
						Inflated = EmptyStateLayout?.Inflate();

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
				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
		#endregion


		private void GetDataArticles()
		{
			try
			{
				ArticleData = JsonConvert.DeserializeObject<ArticleDataObject>(Intent?.GetStringExtra("ItemArticle") ?? "");
				if (ArticleData != null)
				{
					ArticleId = ArticleData.Id.ToString();

					GlideImageLoader.LoadImage(this, ArticleData.Image, ImageBlog, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
					GlideImageLoader.LoadImage(this, ArticleData.UserData.Avatar, UserImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					GlideImageLoader.LoadImage(this, ArticleData.UserData.Avatar, ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);


					string name = CategoriesController.ListCategories?.FirstOrDefault(a => a.Id == ArticleData.Category)?.Name;
					if (string.IsNullOrEmpty(name))
						name = GetString(Resource.String.Lbl_Unknown);

					CategoryName.Text = name;

					SupportActionBar.Title = Methods.FunString.DecodeString(ArticleData.Title);

					TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
					TxtDescription.Text = Methods.FunString.DecodeString(ArticleData.Description);

					//2M Views | 10K Shares | 500 Comments | 3 Months Ago
					TxtInfo.Text = ArticleData.Views + " " + GetText(Resource.String.Lbl_Views) + " | " + ArticleData.Shared + " " + GetText(Resource.String.Lbl_Shares)
								   + " | " + ArticleData.CommentsCount + " " + GetText(Resource.String.Lbl_Comments) + " | " + ArticleData.TextTime;

					LikeCount.Text = ArticleData.Likes.ToString();
					UnlikeCount.Text = ArticleData.Dislikes.ToString();

					if (ArticleData.Liked == 1)
					{
						LikeIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
					}
					else
					{
						LikeIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#a7a7a7"));
					}

					if (ArticleData.Disliked == 1)
					{
						UnlikeIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
					}
					else
					{
						UnlikeIcon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor("#a7a7a7"));
					}

					TxtChannelName.Text = AppTools.GetNameFinal(ArticleData.UserData);

					if (string.IsNullOrEmpty(ArticleData.UserData.SubscribeCount))
						ArticleData.UserData.SubscribeCount = "0";

					TxtChannelViews.Text = ArticleData.UserData.SubscribeCount + " " + GetText(Resource.String.Lbl_Subscribers);

					TxtCountComments.Text = ArticleData.CommentsCount.ToString();

					string style = AppTools.IsTabDark() ? "<style type='text/css'>body{color: #fff; background-color: #282828; line-height: 1.42857143;}</style>" : "<style type='text/css'>body{color: #444; background-color: #FFFEFE; line-height: 1.42857143;}</style>";
					string imageFullWidthStyle = "<style>img{display: inline;height: auto;max-width: 100%;}</style>";

					// This method is deprecated but need to use for old os devices
#pragma warning disable CS0618 // Type or member is obsolete
					string content = Build.VERSION.SdkInt >= BuildVersionCodes.N ? Html.FromHtml(ArticleData.Text, FromHtmlOptions.ModeCompact)?.ToString() : Html.FromHtml(ArticleData.Text)?.ToString();
#pragma warning restore CS0618 // Type or member is obsolete

					DataWebHtml = "<!DOCTYPE html>";
					DataWebHtml += "<head><title></title>" + style + imageFullWidthStyle + "</head>";
					DataWebHtml += "<body>" + content + "</body>";
					DataWebHtml += "</html>";

					// <meta name='viewport' content='width=device-width, user-scalable=no' />
					TxtHtml.SetWebViewClient(new MyWebViewClient(this));
					TxtHtml.Settings.LoadsImagesAutomatically = true;
					TxtHtml.Settings.JavaScriptEnabled = true;
					TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
					TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
					TxtHtml.Settings.DomStorageEnabled = true;
					TxtHtml.Settings.AllowFileAccess = true;
					TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

					TxtHtml.Settings.UseWideViewPort = true;
					TxtHtml.Settings.LoadWithOverviewMode = true;

					TxtHtml.Settings.SetSupportZoom(false);
					TxtHtml.Settings.BuiltInZoomControls = false;
					TxtHtml.Settings.DisplayZoomControls = false;

					//int fontSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 18, Resources?.DisplayMetrics);
					//TxtHtml.Settings.DefaultFontSize = fontSize;

					TxtHtml.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);

					if (Methods.CheckConnectivity())
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.GetArticleAsync(ArticleId) });
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private class MyWebViewClient : WebViewClient
		{
			private readonly ShowArticleActivity Activity;
			public MyWebViewClient(ShowArticleActivity mActivity)
			{
				Activity = mActivity;
			}

			public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
			{
				new IntentController(Activity).OpenBrowserFromApp(request.Url.ToString());
				view.LoadDataWithBaseURL(null, Activity.DataWebHtml, "text/html", "UTF-8", null);
				return true;
			}
		}

	}
}