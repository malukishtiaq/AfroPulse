using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
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
using System.Threading.Tasks;
using static Google.Android.Material.BottomSheet.BottomSheetBehavior;

namespace PlayTube.Activities.ChannelActivities
{
	public class ReplyCommentActivitiesBottomSheet : BottomSheetDialogFragment
	{
		#region Variables Basic

		private LinearLayout RootView;
		private View Inflated;
		private ViewStub EmptyStateLayout;
		private EmojiconEditText EmojiconEditTextView;
		private ImageView Emojiicon;
		private CircleButton SendButton;
		private RecyclerView MRecycler;
		private LinearLayoutManager LayoutManager;
		private ReplyAdapter MAdapter;
		private ImageView Image;
		private SuperTextView CommentText;
		private TextView TimeTextView;
		private TextView UsernameTextView;
		private ImageView LikeiconView;
		private ImageView UnLikeiconView;
		private TextView ReplyiconView;
		private TextView LikeNumber;
		private TextView UnLikeNumber;
		private TextView RepliesCount;
		private LinearLayout LikeButton;
		private LinearLayout UnLikeButton;
		private LinearLayout ReplyButton;
		private readonly BottomSheetCallback MBottomSheetBehaviorCallback = new MyBottomSheetCallBack();
		private CommentDataObject Comment = new CommentDataObject();
		private TabbedMainActivity GlobalContext;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private ActivitiesViewFragment ActivitiesView;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = TabbedMainActivity.GetInstance();
		}

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
				Comment = JsonConvert.DeserializeObject<CommentDataObject>(Arguments?.GetString("Object") ?? "");

				ActivitiesView = GlobalContext.UserChannelFragment != null ? GlobalContext.UserChannelFragment.ActivitiesFragment.ActivitiesViewFragment : GlobalContext.MyChannelFragment.ActivitiesFragment.ActivitiesViewFragment;

				InitComponent(view);
				SetRecyclerViewAdapters();

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

		public override void OnStart()
		{
			try
			{
				base.OnStart();
				var dialog = Dialog;
				//Make dialog full screen with transparent background
				if (dialog != null)
				{
					var width = ViewGroup.LayoutParams.MatchParent;
					var height = ViewGroup.LayoutParams.MatchParent;
					dialog.Window?.SetLayout(width, height);
					dialog.Window?.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
				}
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
				ReplyiconView = contentView.FindViewById<TextView>(Resource.Id.ReplyIcon);
				LikeNumber = contentView.FindViewById<TextView>(Resource.Id.LikeNumber);
				UnLikeNumber = contentView.FindViewById<TextView>(Resource.Id.UnLikeNumber);
				RepliesCount = contentView.FindViewById<TextView>(Resource.Id.RepliesCount);
				LikeButton = contentView.FindViewById<LinearLayout>(Resource.Id.LikeButton);
				UnLikeButton = contentView.FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
				ReplyButton = contentView.FindViewById<LinearLayout>(Resource.Id.ReplyButton);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ReplyiconView, IonIconsFonts.Undo);

				RootView = contentView.FindViewById<LinearLayout>(Resource.Id.root);
				EmojiconEditTextView = contentView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				Emojiicon = contentView.FindViewById<ImageView>(Resource.Id.emojiIcon);
				SendButton = contentView.FindViewById<CircleButton>(Resource.Id.sendButton);
				MRecycler = contentView.FindViewById<RecyclerView>(Resource.Id.recyler);
				EmptyStateLayout = contentView.FindViewById<ViewStub>(Resource.Id.viewStub);

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
				MAdapter = new ReplyAdapter(Activity)
				{
					ReplyList = new ObservableCollection<ReplyDataObject>()
				};
				LayoutManager = new LinearLayoutManager(Activity);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				MRecycler.NestedScrollingEnabled = false;
				MRecycler.SetAdapter(MAdapter);

				MAdapter.AvatarClick += ReplyAdapterOnAvatarClick;
				MAdapter.ReplyClick += ReplyAdapterOnReplyClick;

				RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
				MainScrollEvent = xamarinRecyclerViewOnScrollListener;
				MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
				MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
				MainScrollEvent.IsLoading = false;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		#endregion

		#region Get Replies

		private void LoadReplies()
		{
			try
			{
				if (Comment == null) return;

				GlideImageLoader.LoadImage(Activity, Comment.CommentUserData.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				TextSanitizer chnager = new TextSanitizer(CommentText, Activity, GlobalContext);
				chnager.Load(Methods.FunString.DecodeString(Comment.Text));
				TimeTextView.Text = Comment.TextTime;

				UsernameTextView.Text = AppTools.GetNameFinal(Comment.CommentUserData);

				LikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(Comment.Likes));
				UnLikeNumber.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(Comment.Likes));
				RepliesCount.Text = Comment.RepliesCount.ToString();

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

				MAdapter.ReplyList = new ObservableCollection<ReplyDataObject>(Comment.CommentReplies);
				if (MAdapter.ReplyList.Count > 0)
					MAdapter.NotifyDataSetChanged();

				Task.Factory.StartNew(() => StartApiService());
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Scroll

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
		{
			try
			{
				//Code get last id where LoadMore >>
				var item = MAdapter.ReplyList.LastOrDefault();
				if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
					StartApiService(item.Id.ToString());
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
				if (!string.IsNullOrEmpty(EmojiconEditTextView.Text))
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
							string message = EmojiconEditTextView.Text;

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
							MRecycler.Visibility = ViewStates.Visible;
							MAdapter.ReplyList.Add(comment);
							MAdapter.NotifyItemInserted(MAdapter.ReplyList.Count - 1);
							MRecycler.ScrollToPosition(MAdapter.ReplyList.Count - 1);
							var x = Convert.ToInt32(Comment.RepliesCount);
							RepliesCount.Text = Methods.FunString.FormatPriceValue(++x);

							var dataComments = ActivitiesView?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);
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

								int index = ActivitiesView.MAdapter.CommentList.IndexOf(dataComments);
								if (index > -1)
									ActivitiesView.MAdapter.NotifyItemChanged(index);
							}

							//Api request  
							Task.Run(async () =>
							{
								var (respondCode, respond) = await RequestsAsync.Activities.AddReplyCommentsAsync(Comment.Id.ToString(), Comment.Id.ToString(), message);
								if (respondCode.Equals(200))
								{
									if (respond is AddReplyObject result)
									{
										var dataComment = MAdapter.ReplyList.FirstOrDefault(a => a.Id == int.Parse(time2));
										if (dataComment != null)
											dataComment.Id = result.ReplyId;
									}
								}
								else Methods.DisplayReportResult(Activity, respond);
							});

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
						dynamic dataComments = ActivitiesView?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);

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

							int index = ActivitiesView.MAdapter.CommentList.IndexOf(dataComments);
							ActivitiesView?.MAdapter.NotifyItemChanged(index);

							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentReplyAsync(dataComments.Id.ToString(), false) });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning),
							Activity.GetText(Resource.String.Lbl_Please_sign_in_Dislike),
							Activity.GetText(Resource.String.Lbl_Yes),
							Activity.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
						var dataComments = ActivitiesView?.MAdapter?.CommentList?.FirstOrDefault(a => a.Id == Comment.Id);
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

							int index = ActivitiesView.MAdapter.CommentList.IndexOf(dataComments);
							ActivitiesView.MAdapter.NotifyItemChanged(index);

							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comments.AddLikeOrDislikeCommentReplyAsync(dataComments.Id.ToString(), true) });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning),
							Activity.GetText(Resource.String.Lbl_Please_sign_in_Like),
							Activity.GetText(Resource.String.Lbl_Yes),
							Activity.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
				var item = MAdapter.GetItem(e.Position);
				if (item != null)
				{
					GlobalContext.ShowUserChannelFragment(item.ReplyUserData, item.UserId.ToString());
				}
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
				var item = MAdapter.GetItem(e.Position);
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

		#region Load Data Api Reply 

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
				int countList = MAdapter.ReplyList.Count;

				var (apiStatus, respond) = await RequestsAsync.Activities.GetReplyActivityAsync(Comment.Id.ToString(), "10", offset);
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
							foreach (var item in from item in result.Data let check = MAdapter.ReplyList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
							{
								MAdapter.ReplyList.Add(item);
							}

							Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ReplyList.Count - countList); });
						}
						else
						{
							MAdapter.ReplyList = new ObservableCollection<ReplyDataObject>(result.Data);
							Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
						}
					}
					else
					{
						if (MAdapter.ReplyList.Count > 10 && !MRecycler.CanScrollVertically(1))
							Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short)?.Show();
					}
				}

				Activity?.RunOnUiThread(ShowEmptyPage);
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

				Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void ShowEmptyPage()
		{
			try
			{
				MainScrollEvent.IsLoading = false;
				if (MAdapter.ReplyList?.Count > 0)
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
					x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoReplies);
					if (!x.EmptyStateButton.HasOnClickListeners)
					{
						x.EmptyStateButton.Click += null;
						//x.EmptyStateButton.Click += EmptyStateButton_Click;
					}
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

		#region BottomSheetCallBack

		private class MyBottomSheetCallBack : BottomSheetCallback
		{
			public override void OnSlide(View bottomSheet, float slideOffset)
			{
				try
				{
					//Sliding
					if (slideOffset == StateHidden) Dispose();
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public override void OnStateChanged(View bottomSheet, int newState)
			{
				//State changed
			}
		}

		#endregion

	}
}