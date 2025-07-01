using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Androidx.Media3.UI;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using PlayTube.Activities.Models;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.MediaPlayers.Exo;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using String = Java.Lang.String;
using Uri = Android.Net.Uri;

namespace PlayTube.Activities.Shorts
{
	public class ViewShortsVideoFragment : AndroidX.Fragment.App.Fragment, StTools.IXAutoLinkOnClickListener, IVideoMenuListener
	{
		#region Variables Basic

		private ShortsVideoDetailsActivity GlobalContext;
		private static ViewShortsVideoFragment Instance;

		private StReadMoreOption ReadMoreOption;
		private VideoDataObject DataVideos;

		private ImageView IconBack;

		private View MainView;
		private FrameLayout Root;

		private PlayerView PlayerView;

		private LinearLayout LikeLayout, DisLikeLayout;
		private LinearLayout UserLayout, CommentLayout, ViewsLayout, MoreLayout;
		private ImageView ImgLike, ImgDisLike;
		private ImageView UserImageView, ImgComment, ImgViews;
		private TextView TxtLikeCount, TxtDisLikeCount;
		private TextView TxtUsername, TxtCommentCount, TxtViewsCount;
		private AppCompatButton SubscribeButton;

		private SuperTextView TxtDescription;

		private bool MIsVisibleToUser;
		public ExoController ExoController;
		private VideoDownloadAsyncController VideoControllers;

		#endregion

		#region General

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				MainView = inflater.Inflate(Resource.Layout.ShortsVideoSwipeLayout, container, false);

				GlobalContext = ShortsVideoDetailsActivity.GetInstance();

				Instance = this;
				InitComponent(MainView);
				SetPlayer();

				ReadMoreOption = new StReadMoreOption.Builder()
					.TextLength(200, StReadMoreOption.TypeCharacter)
					.MoreLabel(Activity.GetText(Resource.String.Lbl_ReadMore))
					.LessLabel(Activity.GetText(Resource.String.Lbl_ReadLess))
					.MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
					.LessLabelColor(Color.ParseColor(AppSettings.MainColor))
					.LabelUnderLine(true)
					.Build();

				return MainView;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		//public override void OnViewCreated(View view, Bundle savedInstanceState)
		//{
		//    try
		//    {
		//        base.OnViewCreated(view, savedInstanceState);
		//        MainView = view;
		//    }
		//    catch (Exception exception)
		//    {
		//        Methods.DisplayReportResultTrack(exception);
		//    }
		//}

		public override void SetMenuVisibility(bool menuVisible)
		{
			try
			{
				base.SetMenuVisibility(menuVisible);
				MIsVisibleToUser = menuVisible;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnResume()
		{
			try
			{
				base.OnResume();
				AddOrRemoveEvent(true);

				if (IsResumed && MIsVisibleToUser)
				{
					//var position = Arguments?.GetInt("position", 0); 
					DataVideos = JsonConvert.DeserializeObject<VideoDataObject>(Arguments?.GetString("DataItem") ?? "");
					if (DataVideos != null)
					{
						ListUtils.VideoShortsViewsList ??= new ObservableCollection<VideoDataObject>();
						ListUtils.VideoShortsViewsList.Add(DataVideos);

						LoadData(DataVideos, true);

						if (Methods.CheckConnectivity())
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosInfoAsJson(DataVideos.VideoId) });
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnPause()
		{
			try
			{
				base.OnPause();
				AddOrRemoveEvent(false);
				StopVideo();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnStop()
		{
			try
			{
				base.OnStop();

				if (MIsVisibleToUser)
					StopVideo();
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
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnDestroyView()
		{
			try
			{
				ReleaseVideo();

				base.OnDestroyView();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnDestroy()
		{
			try
			{
				ReleaseVideo();

				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				base.OnDestroy();
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
				IconBack = view.FindViewById<ImageView>(Resource.Id.back);
				IconBack.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				IconBack.ImageTintList = ColorStateList.ValueOf(Color.White);

				Root = view.FindViewById<FrameLayout>(Resource.Id.root);
				PlayerView = view.FindViewById<PlayerView>(Resource.Id.player_view);

				UserLayout = view.FindViewById<LinearLayout>(Resource.Id.userLayout);
				UserImageView = view.FindViewById<ImageView>(Resource.Id.imageAvatar);
				TxtUsername = view.FindViewById<TextView>(Resource.Id.username);
				SubscribeButton = view.FindViewById<AppCompatButton>(Resource.Id.cont);

				TxtDescription = view.FindViewById<SuperTextView>(Resource.Id.tv_descreption);

				LikeLayout = view.FindViewById<LinearLayout>(Resource.Id.likeLayout);
				ImgLike = view.FindViewById<ImageView>(Resource.Id.img_like);
				TxtLikeCount = view.FindViewById<TextView>(Resource.Id.tv_likeCount);
				LikeLayout.Tag = "0";

				DisLikeLayout = view.FindViewById<LinearLayout>(Resource.Id.DislikeLayout);
				ImgDisLike = view.FindViewById<ImageView>(Resource.Id.img_Dislike);
				TxtDisLikeCount = view.FindViewById<TextView>(Resource.Id.tv_DislikeCount);
				DisLikeLayout.Tag = "0";

				CommentLayout = view.FindViewById<LinearLayout>(Resource.Id.commentLayout);
				ImgComment = view.FindViewById<ImageView>(Resource.Id.img_comment);
				TxtCommentCount = view.FindViewById<TextView>(Resource.Id.tv_comment_count);

				ViewsLayout = view.FindViewById<LinearLayout>(Resource.Id.viewsLayout);
				ImgViews = view.FindViewById<ImageView>(Resource.Id.img_views);
				TxtViewsCount = view.FindViewById<TextView>(Resource.Id.tv_views_count);

				MoreLayout = view.FindViewById<LinearLayout>(Resource.Id.moreLayout);
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
					IconBack.Click += IconBackOnClick;
					UserLayout.Click += UserLayoutOnClick;
					SubscribeButton.Click += SubscribeButtonOnClick;
					LikeLayout.Click += LikeLayoutOnClick;
					DisLikeLayout.Click += DisLikeLayoutOnClick;
					CommentLayout.Click += CommentLayoutOnClick;
					MoreLayout.Click += MoreLayoutOnClick;
				}
				else
				{
					IconBack.Click -= IconBackOnClick;
					UserLayout.Click -= UserLayoutOnClick;
					SubscribeButton.Click -= SubscribeButtonOnClick;
					LikeLayout.Click -= LikeLayoutOnClick;
					DisLikeLayout.Click -= DisLikeLayoutOnClick;
					CommentLayout.Click -= CommentLayoutOnClick;
					MoreLayout.Click -= MoreLayoutOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static ViewShortsVideoFragment GetInstance()
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

		#endregion

		#region Event

		private void IconBackOnClick(object sender, EventArgs e)
		{
			try
			{
				GlobalContext.Finish();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void CommentLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				CommentShortBottomSheet shortBottomSheet = new CommentShortBottomSheet();

				Bundle bundle = new Bundle();
				bundle.PutString("VideoId", DataVideos.Id);
				shortBottomSheet.Arguments = bundle;

				shortBottomSheet.Show(ChildFragmentManager, shortBottomSheet.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MoreLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(DataVideos, this, "short");
				videoMenuBottomSheets.Show(Activity.SupportFragmentManager, videoMenuBottomSheets.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void RemoveVideo(VideoDataObject data)
		{
			try
			{
				var check = ListUtils.VideoShortsList.FirstOrDefault(a => a.VideoData?.VideoId == data.VideoId);
				if (check != null)
				{
					var index = ListUtils.VideoShortsList.IndexOf(check);
					if (index != -1)
					{
						ListUtils.VideoShortsList.Remove(check);
						//NotifyItemRemoved(index);

						GlobalContext.MAdapter.UpdateShortsVideoPager(ListUtils.VideoShortsList.Count, ListUtils.VideoShortsList);
						GlobalContext.Pager.Adapter = GlobalContext.MAdapter;
						GlobalContext.Pager.Adapter.NotifyDataSetChanged();

						Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

						var dataObject = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == data.Id);
						if (dataObject == null)
						{
							ListUtils.GlobalNotInterestedList.Add(data);
						}

					}
					if (Methods.CheckConnectivity())
						PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddDeleteNotInterestedAsync(data.Id, true) });
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void DownloadVideo()
		{
			try
			{
				Methods.Path.Chack_MyFolder();

				switch (ExoController.DownloadIcon.Tag?.ToString())
				{
					case "false":
						{
							ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_action_download_stop);
							ExoController.DownloadIcon.Tag = "true";

							if (DataVideos.VideoLocation.Contains("youtube") || DataVideos.VideoType.Contains("Youtube") || DataVideos.VideoType.Contains("youtu"))
							{
								var urlVideo = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(DataVideos.Title);
								if (string.IsNullOrEmpty(urlVideo))
								{
									VideoControllers = new VideoDownloadAsyncController(urlVideo, DataVideos.Title, Activity);
									if (!VideoControllers.CheckDownloadLinkIfExits())
										VideoControllers.StartDownloadManager(DataVideos.Id, DataVideos);
								}
								else
								{
									Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_Error), Activity.GetText(Resource.String.Lbl_You_can_not_Download_video), Activity.GetText(Resource.String.Lbl_Ok));
								}
							}
							else
							{
								VideoControllers = new VideoDownloadAsyncController(DataVideos.VideoLocation, DataVideos.Title, Activity);
								if (!VideoControllers.CheckDownloadLinkIfExits())
									VideoControllers.StartDownloadManager(DataVideos.Id, DataVideos);
							}

							break;
						}
					case "Downloaded":
						try
						{
							MaterialAlertDialogBuilder builder = new MaterialAlertDialogBuilder(Activity);
							builder.SetTitle(Activity.GetText(Resource.String.Lbl_Delete_video));
							builder.SetMessage(Activity.GetText(Resource.String.Lbl_Do_You_want_to_remove_video));

							builder.SetPositiveButton(Activity.GetText(Resource.String.Lbl_Yes), delegate
							{
								try
								{
									VideoControllers.RemoveDiskFile(DataVideos.Title, DataVideos.Id);
									ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_action_download);
									ExoController.DownloadIcon.Tag = "false";
								}
								catch (Exception exception)
								{
									Methods.DisplayReportResultTrack(exception);
								}
							});

							builder.SetNegativeButton(Activity.GetText(Resource.String.Lbl_No), delegate { });

							builder.Show();
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}

						break;
					default:
						ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_action_download);
						ExoController.DownloadIcon.Tag = "false";
						VideoControllers.StopDownloadManager();
						break;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LikeLayoutOnClick(object sender, EventArgs e)
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
							if (LikeLayout.Tag?.ToString() == "0")
							{
								LikeLayout.Tag = "1";
								ImgLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

								ImgDisLike.SetColorFilter(Color.White);
								if (!TxtLikeCount.Text.Contains("K") && !TxtLikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(TxtLikeCount.Text);
									x++;
									TxtLikeCount.Text = x.ToString(CultureInfo.InvariantCulture);
								}

								if (DisLikeLayout.Tag?.ToString() == "1")
								{
									DisLikeLayout.Tag = "0";
									if (!TxtDisLikeCount.Text.Contains("K") && !TxtDisLikeCount.Text.Contains("M"))
									{
										var x = Convert.ToDouble(TxtDisLikeCount.Text);
										if (x > 0)
										{
											x--;
										}
										else
										{
											x = 0;
										}
										TxtDisLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
									}
								}


								//AddToLiked
								TabbedMainActivity.GetInstance()?.LibrarySynchronizer.AddToLiked(DataVideos);

								Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Like
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(DataVideos.Id, "like") });

							}
							else
							{
								LikeLayout.Tag = "0";

								ImgLike.SetColorFilter(Color.White);
								ImgDisLike.SetColorFilter(Color.White);
								if (!TxtLikeCount.Text.Contains("K") && !TxtLikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(TxtLikeCount.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									TxtLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
								}

								Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Remove UNLike
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(DataVideos.Id, "like") });
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, DataVideos, "Login");
						dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Please_sign_in_Like), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void DisLikeLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (DisLikeLayout.Tag?.ToString() == "0")
						{
							DisLikeLayout.Tag = "1";
							ImgDisLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
							ImgLike.SetColorFilter(Color.White);

							if (!TxtDisLikeCount.Text.Contains("K") && !TxtDisLikeCount.Text.Contains("M"))
							{
								var x = Convert.ToDouble(TxtDisLikeCount.Text);
								x++;
								TxtDisLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							if (LikeLayout.Tag?.ToString() == "1")
							{
								LikeLayout.Tag = "0";
								if (!TxtLikeCount.Text.Contains("K") && !TxtLikeCount.Text.Contains("M"))
								{
									var x = Convert.ToDouble(TxtLikeCount.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									TxtLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
								}
							}

							Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short)?.Show();

							//Send API Request here for dislike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(DataVideos.Id, "dislike") });
						}
						else
						{
							DisLikeLayout.Tag = "0";

							ImgDisLike.SetColorFilter(Color.White);
							var x = Convert.ToDouble(TxtDisLikeCount.Text);
							if (!TxtDisLikeCount.Text.Contains("K") && !TxtDisLikeCount.Text.Contains("M"))
							{
								if (x > 0)
								{
									x--;
								}
								else
								{
									x = 0;
								}
								TxtDisLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							//Send API Request here for Remove UNLike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(DataVideos.Id, "dislike") });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, DataVideos, "Login");
						dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Please_sign_in_Dislike), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
					}
				}
				else
				{
					Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void UserLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				GlobalContext.OpenEvent = "Profile";
				GlobalContext.UserDataObjectOpenEvent = DataVideos.Owner?.OwnerClass;

				GlobalContext.Finish();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SubscribeButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						if (!string.IsNullOrEmpty(DataVideos.Owner?.OwnerClass.SubscriberPrice) && DataVideos.Owner?.OwnerClass.SubscriberPrice != "0")
						{
							if (SubscribeButton.Tag?.ToString() == "PaidSubscribe")
							{
								//This channel is paid, You must pay to subscribe

								var dialog = new MaterialAlertDialogBuilder(Context);
								dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
								dialog.SetMessage(Context.GetText(Resource.String.Lbl_ChannelIsPaid));
								dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
								{
									try
									{
										if (await AppTools.CheckWallet(Convert.ToDouble(DataVideos.Owner?.OwnerClass.SubscriberPrice)))
										{
											if (Methods.CheckConnectivity())
											{
												var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", DataVideos.Owner?.OwnerClass.Id);
												if (apiStatus == 200)
												{
													if (respond is MessageObject result)
													{
														Console.WriteLine(result.Message);

														Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();

														SetSubscribeChannelWithPaid();
													}
												}
												else Methods.DisplayAndHudErrorResult(Activity, respond);
											}
											else
												Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
										}
										else
										{
											var dialogBuilder = new MaterialAlertDialogBuilder(Context);
											dialogBuilder.SetTitle(Context.GetText(Resource.String.Lbl_Wallet));
											dialogBuilder.SetMessage(Context.GetText(Resource.String.Lbl_Error_NoWallet));
											dialogBuilder.SetPositiveButton(Context.GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
											{
												try
												{
													Context.StartActivity(new Intent(Context, typeof(WalletActivity)));
												}
												catch (Exception exception)
												{
													Methods.DisplayReportResultTrack(exception);
												}
											});
											dialogBuilder.SetNegativeButton(Context.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

											dialogBuilder.Show();
										}
									}
									catch (Exception exception)
									{
										Methods.DisplayReportResultTrack(exception);
									}
								});
								dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

								dialog.Show();
							}
							else
							{
								SubscribeButton.Tag = "Subscribe";
								SubscribeButton.Text = GetText(Resource.String.Lbl_Subscribe);
								//Color
								//SubscribeButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								//Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subscribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(DataVideos.Owner?.OwnerClass.Id);

								//Send API Request here for UnSubscribed
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(DataVideos.Owner?.OwnerClass.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
						else
						{
							if (SubscribeButton.Tag?.ToString() == "Subscribe")
							{
								SubscribeButton.Tag = "Subscribed";
								SubscribeButton.Text = GetText(Resource.String.Lbl_Subscribed);

								//Color
								//SubscribeButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								//Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Add The Video to  Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.Insert_One_SubscriptionChannel(DataVideos.Owner?.OwnerClass);

								//Send API Request here for Subcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(DataVideos.Owner?.OwnerClass.Id) });

								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							}
							else
							{
								SubscribeButton.Tag = "Subscribe";
								SubscribeButton.Text = GetText(Resource.String.Lbl_Subscribe);
								//Color
								//SubscribeButton.SetTextColor(Color.ParseColor("#efefef"));
								//icon
								//Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribeButton);
								//icon.Bounds = new Rect(10, 10, 10, 7);
								//SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(DataVideos.Owner?.OwnerClass.Id);

								//Send API Request here for UnSubcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(DataVideos.Owner?.OwnerClass.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
						dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
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

		public async void SetSubscribeChannelWithPaid()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					SubscribeButton.Tag = "Subscribed";
					SubscribeButton.Text = GetText(Resource.String.Lbl_Subscribed);

					//Color
					//SubscribeButton.SetTextColor(Color.ParseColor("#efefef"));
					//icon
					//Drawable icon = Activity.GetDrawable(Resource.Drawable.SubcribedButton);
					//icon.Bounds = new Rect(10, 10, 10, 7);
					//SubscribeButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

					//Add The Video to  Subscribe Videos Database
					var sqlEntity = new SqLiteDatabase();
					sqlEntity.Insert_One_SubscriptionChannel(DataVideos.Owner?.OwnerClass);

					//Send API Request here for Subscribe
					var (apiStatus, respond) = await RequestsAsync.Global.AddSubscribeToChannelAsync(DataVideos.Owner?.OwnerClass.Id, "paid");
					if (apiStatus == 200)
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);
							Activity?.RunOnUiThread(() =>
							{
								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							});
						}
					}
					else Methods.DisplayReportResult(Activity, respond);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void AutoLinkTextClick(StTools.XAutoLinkMode autoLinkMode, string matchedText, Dictionary<string, string> userData)
		{
			try
			{
				var typetext = Methods.FunString.Check_Regex(matchedText.Replace(" ", "").Replace("\n", "").Replace("\n", ""));
				if (typetext == "Email" || autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
				{
					Methods.App.SendEmail(Activity, matchedText.Replace(" ", "").Replace("\n", ""));
				}
				else if (typetext == "Website" || autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
				{
					string url = matchedText.Replace(" ", "").Replace("\n", "");
					if (!matchedText.Contains("http"))
					{
						url = "http://" + matchedText.Replace(" ", "").Replace("\n", "");
					}

					//var intent = new Intent(Activity, typeof(LocalWebViewActivity));
					//intent.PutExtra("URL", url);
					//intent.PutExtra("Type", url);
					//Activity.StartActivity(intent);
					new IntentController(TabbedMainActivity.GetInstance()).OpenBrowserFromApp(url);
				}
				else if (typetext == "Hashtag" || autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
				{
					Bundle bundle = new Bundle();
					bundle.PutString("Key", matchedText.Replace("#", ""));
					VideosByKeyFragment videoViewerFragment = new VideosByKeyFragment
					{
						Arguments = bundle
					};
					TabbedMainActivity.GetInstance()?.FragmentBottomNavigator?.DisplayFragment(videoViewerFragment);
				}
				else if (typetext == "Mention" || autoLinkMode == StTools.XAutoLinkMode.ModeMention)
				{
					Bundle bundle = new Bundle();
					bundle.PutString("Key", matchedText.Replace("@", ""));
					VideosByKeyFragment videoViewerFragment = new VideosByKeyFragment
					{
						Arguments = bundle
					};

					TabbedMainActivity.GetInstance()?.FragmentBottomNavigator?.DisplayFragment(videoViewerFragment);
				}
				else if (typetext == "Number" || autoLinkMode == StTools.XAutoLinkMode.ModePhone)
				{
					Methods.App.SaveContacts(Activity, matchedText.Replace(" ", "").Replace("\n", ""), "", "2");
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Exo Player

		private void SetPlayer()
		{
			try
			{
				ExoController = new ExoController(GlobalContext, "ShortsVideo");
				ExoController?.SetPlayer(PlayerView);
				ExoController?.SetPlayerControl(false);
				ExoController.ExoTopLayout.Visibility = ViewStates.Gone;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void StopVideo()
		{
			try
			{
				ExoController?.StopVideo();

				TabbedMainActivity.GetInstance()?.SetOffWakeLock();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void ReleaseVideo()
		{
			try
			{
				ExoController?.ReleaseVideo();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		private async Task GetVideosInfoAsJson(string videoId)
		{
			try
			{
				var (apiStatus, respond) = await RequestsAsync.Video.GetVideosDetailsAsync(videoId, UserDetails.DeviceId?.Replace("-", "") ?? "1");
				if (apiStatus == 200)
				{
					if (respond is GetVideosDetailsObject result)
					{
						Activity?.RunOnUiThread(() => LoadData(result.DataResult, false));

						if (ListUtils.AdsVideoList.Count > 0)
						{
							if (result.DataResult.VideoAd.VideoAdClass != null)
								ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
						}
						else
						{
							ListUtils.AdsVideoList = new ObservableCollection<VideoAdDataObject>();

							if (result.DataResult.VideoAd.VideoAdClass != null)
								ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
						}
					}
				}
				else Methods.DisplayReportResult(Activity, respond);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void LoadData(VideoDataObject dataObject, bool firstPlay)
		{
			try
			{
				DataVideos = dataObject;

				if (firstPlay)
				{
					// Uri
					Uri uri = Uri.Parse(dataObject.VideoLocation);
					ExoController?.FirstPlayVideo(uri, dataObject, false);

					var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(DataVideos.Title);
					if (!string.IsNullOrEmpty(file))
					{
						ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
						ExoController.DownloadIcon.Tag = "Downloaded";
						new LibrarySynchronizer(Activity).AddToWatchOffline(DataVideos);
					}
				}

				GlideImageLoader.LoadImage(Activity, dataObject.Owner?.OwnerClass.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				TxtUsername.Text = AppTools.GetNameFinal(dataObject.Owner?.OwnerClass);

				TxtLikeCount.Text = dataObject.Likes ?? "0";
				TxtDisLikeCount.Text = dataObject.Dislikes ?? "0";

				if (dataObject.IsLiked == "1") // true
				{
					ImgLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
					ImgDisLike.SetColorFilter(Color.White);
					LikeLayout.Tag = "1";
				}
				else
				{
					ImgDisLike.SetColorFilter(Color.White);
					LikeLayout.Tag = "0";
				}

				if (dataObject.IsDisliked == "1") // true
				{
					ImgLike.SetColorFilter(Color.White);
					ImgDisLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
					DisLikeLayout.Tag = "1";
				}
				else
				{
					ImgDisLike.SetColorFilter(Color.White);
					DisLikeLayout.Tag = "0";
				}

				TxtViewsCount.Text = dataObject.Views;
				TxtCommentCount.Text = dataObject.CommentsCount ?? "0";

				if (string.IsNullOrEmpty(dataObject.Title) || string.IsNullOrWhiteSpace(dataObject.Title))
				{
					TxtDescription.Visibility = ViewStates.Invisible;
				}
				else
				{
					ReadMoreOption.AddReadMoreTo(TxtDescription, new String(dataObject.Title));
				}

				var isOwner = dataObject.IsOwner != null && dataObject.IsOwner.Value;
				SubscribeButton.Visibility = isOwner ? ViewStates.Invisible : ViewStates.Visible;

				if (isOwner)
					return;

				if (!string.IsNullOrEmpty(dataObject.Owner?.OwnerClass.SubscriberPrice) && dataObject.Owner?.OwnerClass.SubscriberPrice != "0")
				{
					if (dataObject.Owner?.OwnerClass.AmISubscribed == "0")
					{
						//This channel is paid, You must pay to subscribe
						SubscribeButton.Tag = "PaidSubscribe";

						var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
						var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
						Console.WriteLine(currency);
						SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + dataObject.Owner?.OwnerClass.SubscriberPrice;
					}
					else
					{
						SubscribeButton.Tag = "Subscribed";
						SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);
					}
				}
				else
				{
					SubscribeButton.Tag = dataObject.Owner?.OwnerClass.AmISubscribed == "1" ? "Subscribed" : "Subscribe";

					switch (SubscribeButton.Tag?.ToString())
					{
						case "Subscribed":
							{
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);
								break;
							}
						case "Subscribe":
							{
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);
								break;
							}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


	}
}