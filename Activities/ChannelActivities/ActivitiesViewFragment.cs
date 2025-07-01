using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Activities;
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

namespace PlayTube.Activities.ChannelActivities
{
	public class ActivitiesViewFragment : Fragment, IDialogListCallBack, IDialogInputCallBack
	{
		#region Variables Basic

		private ImageView ImageAct, ImgSent;
		private TextView TimeUser, ContentAct;
		private LinearLayout LikeActLayout, ShareActLayout, CommentActLayout;
		private ImageView IconLikeAct;
		private TextView CountLikeAct, CountCommentAct;
		private LinearLayoutManager LayoutManager;
		private RecyclerView MRecycler;
		public CommentsAdapter MAdapter;
		private ViewStub EmptyStateLayout;
		private View Inflated;
		private EditText TxtComment;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private RecyclerViewOnScrollListener MainScrollEvent;
		private ActivitiesDataObject ItemActivity;
		private string ActivityId, TypeDialog;
		private CommentDataObject ItemComments;
		private TabbedMainActivity GlobalContext;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			GlobalContext = (TabbedMainActivity)Activity;
			HasOptionsMenu = true;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				View view = inflater?.Inflate(Resource.Layout.ActivitiesViewLayout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);
				ActivityId = Arguments?.GetString("ActivityId") ?? "";

				//Get Value And Set Toolbar
				InitComponent(view);
				InitToolbar(view);
				SetRecyclerViewAdapters();
				AddOrRemoveEvent(true);

				LoadData();
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

		private void InitComponent(View view)
		{
			try
			{
				ImageAct = (ImageView)view.FindViewById(Resource.Id.ImageAct);
				TimeUser = view.FindViewById<TextView>(Resource.Id.TimeUser);
				ContentAct = view.FindViewById<TextView>(Resource.Id.ContentAct);
				LikeActLayout = view.FindViewById<LinearLayout>(Resource.Id.LikeActLayout);
				IconLikeAct = view.FindViewById<ImageView>(Resource.Id.IconLikeAct);
				CountLikeAct = view.FindViewById<TextView>(Resource.Id.CountLikeAct);
				ShareActLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareActLayout);
				CommentActLayout = view.FindViewById<LinearLayout>(Resource.Id.CommentActLayout);
				CountCommentAct = view.FindViewById<TextView>(Resource.Id.CountCommentAct);

				ShareActLayout.Visibility = ViewStates.Invisible;

				EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
				MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recycler_view);
				TxtComment = view.FindViewById<EditText>(Resource.Id.commenttext);
				ImgSent = view.FindViewById<ImageView>(Resource.Id.send);

				SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = false;
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

				TxtComment.Text = "";
				Methods.SetColorEditText(TxtComment, AppTools.IsTabDark() ? Color.White : Color.Black);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					string title = Context.GetString(Resource.String.Lbl_Activities);
					GlobalContext.SetToolBar(toolbar, title);
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
				MAdapter = new CommentsAdapter(Activity)
				{
					CommentList = new ObservableCollection<CommentDataObject>()
				};
				LayoutManager = new LinearLayoutManager(Activity);
				MRecycler.SetLayoutManager(LayoutManager);
				MRecycler.HasFixedSize = true;
				MRecycler.SetItemViewCacheSize(10);
				MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
				MRecycler.NestedScrollingEnabled = false;
				MRecycler.SetAdapter(MAdapter);

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

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					LikeActLayout.Click += LikeActLayoutOnClick;
					ImgSent.Click += ImgSentOnClick;
					MAdapter.ItemLongClick += MAdapterOnItemLongClick;
					MAdapter.ReplyClick += CommentsAdapter_ReplyClick;
					MAdapter.AvatarClick += CommentsAdapter_AvatarClick;
				}
				else
				{
					LikeActLayout.Click -= LikeActLayoutOnClick;
					ImgSent.Click -= ImgSentOnClick;
					MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
					MAdapter.ReplyClick -= CommentsAdapter_ReplyClick;
					MAdapter.AvatarClick -= CommentsAdapter_AvatarClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		//Edit Or Delete Comment 
		private void MAdapterOnItemLongClick(object sender, CommentAdapterClickEventArgs e)
		{
			try
			{
				var position = e.Position;
				if (position > -1)
				{
					ItemComments = MAdapter.GetItem(position);
					if (ItemComments != null)
					{
						TypeDialog = "MoreComment";

						var arrayAdapter = new List<string>();
						var dialogList = new MaterialAlertDialogBuilder(Activity);

						if (ItemComments.IsCommentOwner && UserDetails.IsLogin)
						{
							arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Delete));
							arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Edit));
						}

						arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Copy));

						dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
						dialogList.SetPositiveButton(Activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

						dialogList.Show();
					}
				}
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
					GlobalContext.ShowUserChannelFragment(item.CommentUserData, item.UserId.ToString());
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
					// show dialog :
					ReplyCommentActivitiesBottomSheet replyFragment = new ReplyCommentActivitiesBottomSheet();
					Bundle bundle = new Bundle();

					bundle.PutString("Type", "Activities");
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

		//Add Comment 
		private async void ImgSentOnClick(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					if (!string.IsNullOrEmpty(TxtComment.Text))
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
							string message = TxtComment.Text;
							var postId = MAdapter.CommentList.FirstOrDefault(a => a.ActivityId == Convert.ToInt32(ActivityId))?.PostId ?? 0;

							var comment = new CommentDataObject
							{
								Text = message,
								TextTime = time,
								UserId = Convert.ToInt32(UserDetails.UserId),
								Id = Convert.ToInt32(time2),
								IsCommentOwner = true,
								ActivityId = Convert.ToInt32(ActivityId),
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
							var (respondCode, respond) = await RequestsAsync.Activities.AddCommentActivityAsync(ActivityId, message);
							if (respondCode.Equals(200))
							{
								if (respond is MessageIdObject messageId)
								{
									var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
									if (dataComment != null)
										dataComment.Id = messageId.Id;
								}
							}
							else Methods.DisplayReportResult(Activity, respond);

							//Hide keyboard
							TxtComment.Text = "";
							TxtComment.ClearFocus();
						}
						else
						{
							Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
					dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_comment), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Dislike
		//private void DislikeActLayoutOnClick(object sender, EventArgs e)
		//{
		//    try
		//    {
		//        if (Methods.CheckConnectivity())
		//        {
		//            if (UserDetails.IsLogin)
		//            {

		//                if (ItemActivity == null) return;

		//                ItemActivity.IsDisliked = ItemActivity.IsDisliked switch
		//                {
		//                    "0" => "1",
		//                    "1" => "0",
		//                    _ => "1"
		//                };


		//                switch (ItemActivity.IsDisliked)
		//                {
		//                    case "0":
		//                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
		//                        IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
		//                        DislikeActLayout.Tag = "Dislike";
		//                        break;
		//                    case "1":
		//                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
		//                        IconDislikeAct.SetTextColor(Color.ParseColor("#4caf50"));
		//                        DislikeActLayout.Tag = "Disliked";
		//                        break;
		//                    default:
		//                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
		//                        IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
		//                        DislikeActLayout.Tag = "Dislike";
		//                        break;
		//                }

		//                switch (DislikeActLayout.Tag?.ToString())
		//                {
		//                    case "Disliked":
		//                        {
		//                            if (!ItemActivity.Dislikes.Contains("K") && !ItemActivity.Dislikes.Contains("M"))
		//                            {
		//                                var x = Convert.ToDouble(ItemActivity.Dislikes);
		//                                x++;
		//                                ItemActivity.Dislikes = x.ToString(CultureInfo.CurrentCulture);
		//                            }
		//                            break;
		//                        }
		//                    case "Dislike":
		//                        {
		//                            if (!ItemActivity.Dislikes.Contains("K") && !ItemActivity.Dislikes.Contains("M"))
		//                            {
		//                                var x = Convert.ToDouble(ItemActivity.Dislikes);
		//                                if (x > 0)
		//                                    x--;
		//                                else
		//                                    x = 0;

		//                                ItemActivity.Dislikes = x.ToString(CultureInfo.CurrentCulture);
		//                            }
		//                            break;
		//                        }
		//                }

		//                CountDislikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Dislikes));

		//                if (ItemActivity.IsLiked == "1")
		//                {
		//                    ItemActivity.IsLiked = "0";
		//                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconLikeAct, FontAwesomeIcon.ThumbsUp);
		//                    IconLikeAct.SetTextColor(Color.ParseColor("#444444"));
		//                    LikeActLayout.Tag = "Like";

		//                    if (!ItemActivity.Likes.Contains("K") && !ItemActivity.Likes.Contains("M"))
		//                    {
		//                        var x = Convert.ToDouble(ItemActivity.Likes);
		//                        if (x > 0)
		//                            x--;
		//                        else
		//                            x = 0;

		//                        ItemActivity.Likes = x.ToString(CultureInfo.CurrentCulture);
		//                    }
		//                }

		//                CountLikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Likes));

		//                if (!Methods.CheckConnectivity())
		//                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
		//                else
		//                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.DisLikeActivityAsync(ItemActivity.Id) });
		//            }
		//            else
		//            {
		//                PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
		//                dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Like), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
		//            }
		//        }
		//        else
		//        {
		//            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
		//        }
		//    }
		//    catch (Exception exception)
		//    {
		//        Methods.DisplayReportResultTrack(exception);
		//    }
		//}

		//Like
		private void LikeActLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (ItemActivity == null) return;

						ItemActivity.IsLiked = ItemActivity.IsLiked switch
						{
							"0" => "1",
							"1" => "0",
							_ => "1"
						};

						switch (ItemActivity.IsLiked)
						{
							case "0":
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								LikeActLayout.Tag = "Like";
								break;
							case "1":
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_filled);
								LikeActLayout.Tag = "Liked";
								break;
							default:
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								LikeActLayout.Tag = "Like";
								break;
						}

						switch (LikeActLayout.Tag?.ToString())
						{
							case "Liked":
								{
									if (!ItemActivity.Likes.Contains("K") && !ItemActivity.Likes.Contains("M"))
									{
										var x = Convert.ToDouble(ItemActivity.Likes);
										x++;
										ItemActivity.Likes = x.ToString(CultureInfo.CurrentCulture);
									}
									break;
								}
							case "Like":
								{
									if (!ItemActivity.Likes.Contains("K") && !ItemActivity.Likes.Contains("M"))
									{
										var x = Convert.ToDouble(ItemActivity.Likes);
										if (x > 0)
											x--;
										else
											x = 0;

										ItemActivity.Likes = x.ToString(CultureInfo.CurrentCulture);
									}
									break;
								}
						}

						CountLikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Likes));

						//if (ItemActivity.IsDisliked == "1")
						//{
						//    ItemActivity.IsDisliked = "0";
						//    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//    IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
						//    DislikeActLayout.Tag = "Dislike";

						//    if (!ItemActivity.Dislikes.Contains("K") && !ItemActivity.Dislikes.Contains("M"))
						//    {
						//        var x = Convert.ToDouble(ItemActivity.Dislikes);
						//        if (x > 0)
						//            x--;
						//        else
						//            x = 0;

						//        ItemActivity.Dislikes = x.ToString(CultureInfo.CurrentCulture);
						//    }
						//}

						//CountDislikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Dislikes));

						if (!Methods.CheckConnectivity())
							Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						else
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.LikeActivityAsync(ItemActivity.Id) });
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Dislike), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
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

		#endregion

		#region Scroll Comment

		private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
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

		#region Data Activity

		private void LoadData()
		{
			try
			{
				if (!string.IsNullOrEmpty(Arguments?.GetString("DataItemActivity")))
				{
					ItemActivity = JsonConvert.DeserializeObject<ActivitiesDataObject>(Arguments?.GetString("DataItemActivity"));
					if (ItemActivity != null)
					{
						GlideImageLoader.LoadImage(Activity, ItemActivity.Image, ImageAct, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

						TimeUser.Text = Methods.Time.TimeAgo(Convert.ToInt32(ItemActivity.Time), false);

						ContentAct.Text = Methods.FunString.DecodeString(ItemActivity.Text);


						switch (ItemActivity.IsLiked)
						{
							case "0":
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								LikeActLayout.Tag = "Like";
								break;
							case "1":
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_filled);
								LikeActLayout.Tag = "Liked";
								break;
							default:
								IconLikeAct.SetImageResource(Resource.Drawable.pif_ant_design_heart_outlite);
								LikeActLayout.Tag = "Like";
								break;
						}

						CountLikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Likes));

						//switch (ItemActivity.IsDisliked)
						//{
						//    case "0":
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
						//        DislikeActLayout.Tag = "Dislike";
						//        break;
						//    case "1":
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        IconDislikeAct.SetTextColor(Color.ParseColor("#4caf50"));
						//        DislikeActLayout.Tag = "Disliked";
						//        break;
						//    default:
						//        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconDislikeAct, FontAwesomeIcon.ThumbsDown);
						//        IconDislikeAct.SetTextColor(Color.ParseColor("#444444"));
						//        DislikeActLayout.Tag = "Dislike";
						//        break;
						//}

						//CountDislikeAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.Dislikes));

						CountCommentAct.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(ItemActivity.CommentsCount));

						Task.Factory.StartNew(() => StartApiService());
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Load Data Api Comment 

		private void StartApiService(string offset = "0")
		{
			if (!Methods.CheckConnectivity())
				Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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

				var (apiStatus, respond) = await RequestsAsync.Activities.GetCommentsActivityAsync(ItemActivity.Id, "10", offset);
				if (apiStatus != 200 || respond is not GetCommentsActivityObject result || result.Data == null)
				{
					Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.Data.Count;
					if (respondList > 0)
					{
						foreach (var item in from item in result.Data let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
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
					else
					{
						if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

				Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
			}
			MainScrollEvent.IsLoading = false;
		}

		private void ShowEmptyPage()
		{
			try
			{
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;
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
				MainScrollEvent.IsLoading = false;
				SwipeRefreshLayout.Refreshing = false;
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

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				string text = itemString;
				if (text == Activity.GetText(Resource.String.Lbl_Edit))
				{
					if (Methods.CheckConnectivity())
					{
						TypeDialog = "EditComment";

						var dialogBuilder = new MaterialAlertDialogBuilder(Activity);
						dialogBuilder.SetTitle(Resource.String.Lbl_Edit);

						EditText input = new EditText(Activity);
						input.SetHint(Resource.String.Lbl_AddCommentHere);
						input.Text = ItemComments.Text;
						input.InputType = InputTypes.ClassText;
						LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
						input.LayoutParameters = lp;

						dialogBuilder.SetView(input);

						dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_Save), new MaterialDialogUtils(input, this));
						dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

						dialogBuilder.Show();
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
				else if (text == Activity.GetText(Resource.String.Lbl_Delete))
				{
					if (Methods.CheckConnectivity())
					{
						var data = MAdapter.CommentList.FirstOrDefault(a => a.Id == ItemComments?.Id);
						if (data != null)
						{
							MAdapter.CommentList.Remove(data);

							int index = MAdapter.CommentList.IndexOf(data);
							if (index >= 0)
								MAdapter.NotifyItemRemoved(index);

							//Send api delete 
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.DeleteCommentActivityAsync(ItemActivity.Id) });
						}
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
				else if (text == Activity.GetText(Resource.String.Lbl_Copy))
				{
					CopyLinkEvent(Methods.FunString.DecodeString(ItemComments.Text));
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnInput(IDialogInterface dialog, string input)
		{
			try
			{
				if (input.Length > 0)
				{
					if (Methods.CheckConnectivity())
					{
						var data = MAdapter.CommentList.FirstOrDefault(a => a.Id == ItemComments?.Id);
						if (data != null)
						{
							data.Text = input;

							int index = MAdapter.CommentList.IndexOf(data);
							if (index >= 0)
								MAdapter.NotifyItemChanged(index);

							//Send api delete 
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Activities.EditCommentActivityAsync(ItemActivity.Id, input) });
						}
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Copy Link
		private void CopyLinkEvent(string text)
		{
			try
			{
				var clipboardManager = (ClipboardManager)Activity.GetSystemService(Context.ClipboardService);

				var clipData = ClipData.NewPlainText("text", text);
				clipboardManager.PrimaryClip = clipData;

				Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion
	}
}