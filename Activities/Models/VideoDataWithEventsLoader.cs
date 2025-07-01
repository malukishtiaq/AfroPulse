using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Lights;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Androidx.Media3.UI;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.Lifecycle;
using Anjo.Android.YouTubePlayerX.Player;
using Com.Sothree.Slidinguppanel;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using PlayTube.Activities.Comments;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.ShimmerUtils;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.SuperTextLibrary;
using PlayTube.MediaPlayers;
using PlayTube.MediaPlayers.Exo;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Activity = Android.App.Activity;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Switch = Android.Widget.Switch;
using Uri = Android.Net.Uri;

namespace PlayTube.Activities.Models
{
    public enum VideoEnumTypes
    {
        Youtube, Normal, DailyMotion, Vimeo, Ok, Twitch, Facebook, AgeRestricted, GeoBlocked
    }
	 
    public class VideoDataWithEventsLoader : Java.Lang.Object, IYouTubePlayerInitListener, IYouTubePlayerFullScreenListener, View.IOnClickListener, IDialogListCallBack
	{
		private readonly TabbedMainActivity TabbedMainActivity;
		private readonly GlobalPlayerActivity GlobalPlayerActivity;
		private readonly AppCompatActivity Activity;
		private readonly string ActivityName;

		private LinearLayout MainLayout;
		//private CardView PlayerCardView;
		public FrameLayout Mainroot, Root;

		public YouTubePlayerView TubePlayerView;
		public YouTubePlayerEvents YouTubePlayerEvents;
		public IYouTubePlayer YoutubePlayer;
		private string TypeYouTubePlayerFullScreen = "RequestedOrientation";

		public PlayerView PlayerView, PlayerViewFullScreen;
		public ExoController ExoController;

		public ProgressBar ProgressBar;

		private AppBarLayout AppBarLayoutView;
		public CoordinatorLayout CoordinatorLayoutView;

		private ViewStub ShimmerPageLayout;
		private View InflatedShimmer;
		private TemplateShimmerInflater ShimmerInflater;

		private TextView TxtVideoTitle, TxtInfo;
		private ImageView VideoShowDescription;

		private LinearLayout LikeButton, UnLikeButton, EditButton, DonateButton, RentButton, ShareButton, AddToButton;
		private ImageView LikeIcon, UnLikeIcon;
		private TextView LikeNumber, UnLikeNumber;

		private LinearLayout ChannelLayout;
		private ImageView ImageChannel;
		private TextView TxtChannelName, TxtChannelViews;
		private AppCompatButton SubscribeButton;

		private LinearLayout VideoDescriptionLayout;
		private TextView TxtVideoCategory;
		private TextView TxtPublishDate;
		private SuperTextView TxtVideoDescription;
		private TextSanitizer TextSanitizerAutoLink;

		private LinearLayout AutoNextSection;
		private Switch AutoNextSwitch;
		private FrameLayout VideoBottomLayout;

		private RelativeLayout CommentBottomLayout;
		private TextView TxtCountComments;
		private ImageView IconComments;

		public bool ShowRestrictedVideo;

		private bool IsFullScreen;
		public string VideoIdYoutube;
		public VideoDataObject VideoData;

		public LibrarySynchronizer LibrarySynchronizer;

		public VideoEnumTypes VideoType;
		private readonly Color DefaultIconColor = AppTools.IsTabDark() ? Color.ParseColor("#efefef") : Color.ParseColor("#737884");
		private readonly Color SelectIconColor = Color.ParseColor(AppSettings.MainColor);
		private NextToFragment NextToFragment;
		public CommentsFragment CommentsFragment;
		public ThirdPartyPlayersFragment ThirdPartyPlayersFragment;
		public RestrictedVideoFragment RestrictedVideoPlayerFragment;

		private VideoDownloadAsyncController VideoControllers;
		private static VideoDataWithEventsLoader Instance;

		public VideoDataWithEventsLoader(AppCompatActivity activityContext, string namePage)
		{
			try
			{
				Instance = this;

				Activity = activityContext;
				ActivityName = namePage;
				TabbedMainActivity = TabbedMainActivity.GetInstance();

				switch (namePage)
				{
					case "GlobalPlayerActivity":
						GlobalPlayerActivity = GlobalPlayerActivity.GetInstance();
						break;
				}

				Init();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region Functions

		private void Init()
		{
			try
			{
				SetVideoPlayerAndFragment();
				InitComponent();
				InitShimmer();

				var ftvideo = Activity.SupportFragmentManager.BeginTransaction();
				ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
				//ftvideo.AddToBackStack(null);
				ftvideo.Add(Resource.Id.videoButtomLayout, CommentsFragment, "Comments").Hide(CommentsFragment)?.Commit();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitComponent()
		{
			try
			{
				MainLayout = Activity.FindViewById<LinearLayout>(Resource.Id.slidepanelchildtwo_topviewtwo);
				//PlayerCardView = Activity.FindViewById<CardView>(Resource.Id.cardview2);

				Mainroot = Activity.FindViewById<FrameLayout>(Resource.Id.Mainroot);
				Root = Activity.FindViewById<FrameLayout>(Resource.Id.root);

				ProgressBar = Activity.FindViewById<ProgressBar>(Resource.Id.progress_bar);

				CoordinatorLayoutView = Activity.FindViewById<CoordinatorLayout>(Resource.Id.parent);
				AppBarLayoutView = Activity.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);

				TxtVideoTitle = Activity.FindViewById<TextView>(Resource.Id.video_Titile);
				VideoShowDescription = Activity.FindViewById<ImageView>(Resource.Id.video_ShowDiscription);
				TxtInfo = Activity.FindViewById<TextView>(Resource.Id.info);

				LikeButton = Activity.FindViewById<LinearLayout>(Resource.Id.LikeButton);
				LikeIcon = Activity.FindViewById<ImageView>(Resource.Id.Likeicon);
				LikeNumber = Activity.FindViewById<TextView>(Resource.Id.LikeNumber);

				UnLikeButton = Activity.FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
				UnLikeIcon = Activity.FindViewById<ImageView>(Resource.Id.UnLikeicon);
				UnLikeNumber = Activity.FindViewById<TextView>(Resource.Id.UnLikeNumber);

				EditButton = Activity.FindViewById<LinearLayout>(Resource.Id.editButton);
				DonateButton = Activity.FindViewById<LinearLayout>(Resource.Id.DonateButton);
				RentButton = Activity.FindViewById<LinearLayout>(Resource.Id.RentButton);
				ShareButton = Activity.FindViewById<LinearLayout>(Resource.Id.ShareButton);
				AddToButton = Activity.FindViewById<LinearLayout>(Resource.Id.AddToButton);

				ChannelLayout = Activity.FindViewById<LinearLayout>(Resource.Id.ChannelLayout);
				ImageChannel = Activity.FindViewById<ImageView>(Resource.Id.Image_Channel);
				TxtChannelName = Activity.FindViewById<TextView>(Resource.Id.ChannelName);
				TxtChannelViews = Activity.FindViewById<TextView>(Resource.Id.Channelviews);
				SubscribeButton = Activity.FindViewById<AppCompatButton>(Resource.Id.SubcribeButton);

				VideoDescriptionLayout = Activity.FindViewById<LinearLayout>(Resource.Id.videoDescriptionLayout);
				TxtVideoCategory = Activity.FindViewById<TextView>(Resource.Id.videoCategorytextview);
				TxtPublishDate = Activity.FindViewById<TextView>(Resource.Id.videoDate);
				TxtVideoDescription = Activity.FindViewById<SuperTextView>(Resource.Id.videoDescriptionTextview);


				CommentBottomLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.CommentButtomLayout);
				TxtCountComments = Activity.FindViewById<TextView>(Resource.Id.countComments);
				IconComments = Activity.FindViewById<ImageView>(Resource.Id.iconComments);

				VideoBottomLayout = Activity.FindViewById<FrameLayout>(Resource.Id.videoButtomLayout);

				AutoNextSection = Activity.FindViewById<LinearLayout>(Resource.Id.AutoNextSection);
				AutoNextSwitch = Activity.FindViewById<Switch>(Resource.Id.AutoNextswitch);
				AutoNextSwitch.Checked = UserDetails.AutoNext;

				LibrarySynchronizer = new LibrarySynchronizer(Activity);
				TextSanitizerAutoLink = new TextSanitizer(TxtVideoDescription, Activity);

				if (AppSettings.CardPlayerView == CardPlayerView.Radius)
				{
					//PlayerCardView.Radius = TypedValue.ApplyDimension(ComplexUnitType.Dip, AppSettings.CardPlayerViewRadius, Activity.Resources.DisplayMetrics);
				}
				else
				{
					//PlayerCardView.Radius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 0f, Activity.Resources.DisplayMetrics);

					//=========
					//TypedValue typedValuePrimary = new TypedValue();
					//var theme = Activity.Theme;
					//theme?.ResolveAttribute(Resource.Attribute.colorSecondaryExtra, typedValuePrimary, true);
					//var colorPrimary = new Color(typedValuePrimary.Data);

					//string hex1 = "#" + Integer.ToHexString(colorPrimary).Remove(0, 2);

					//MainLayout.SetBackgroundColor(Color.ParseColor(hex1));
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitShimmer()
		{
			try
			{
				ShimmerPageLayout = Activity.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
				InflatedShimmer ??= ShimmerPageLayout?.Inflate();

				ShimmerInflater = new TemplateShimmerInflater();
				ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.VideoRowTemplate);
				ShimmerInflater.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					LikeButton.Click += LikeButtonOnClick;
					UnLikeButton.Click += UnLikeButtonOnClick;
					ShareButton.Click += ShareButtonOnClick;
					AddToButton.Click += AddToButtonOnClick;
					SubscribeButton.Click += SubscribeButtonOnClick;
					TxtChannelName.Click += ImageChannelOnClick;
					ImageChannel.Click += ImageChannelOnClick;
					TxtVideoCategory.Click += TxtVideoCategoryOnClick;
					VideoShowDescription.Click += VideoShowDescriptionOnClick;
					EditButton.Click += EditButtonOnClick;
					RentButton.Click += RentButtonOnClick;
					DonateButton.Click += DonateButtonOnClick;
					CommentBottomLayout.Click += CommentBottomLayoutOnClick;
					AutoNextSwitch.CheckedChange += AutoNextSwitchOnCheckedChange;
				}
				else
				{
					LikeButton.Click -= LikeButtonOnClick;
					UnLikeButton.Click -= UnLikeButtonOnClick;
					ShareButton.Click -= ShareButtonOnClick;
					AddToButton.Click -= AddToButtonOnClick;
					SubscribeButton.Click -= SubscribeButtonOnClick;
					TxtChannelName.Click -= ImageChannelOnClick;
					ImageChannel.Click -= ImageChannelOnClick;
					TxtVideoCategory.Click -= TxtVideoCategoryOnClick;
					VideoShowDescription.Click -= VideoShowDescriptionOnClick;
					EditButton.Click -= EditButtonOnClick;
					RentButton.Click -= RentButtonOnClick;
					DonateButton.Click -= DonateButtonOnClick;
					CommentBottomLayout.Click -= CommentBottomLayoutOnClick;
					AutoNextSwitch.CheckedChange -= AutoNextSwitchOnCheckedChange;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public readonly List<Fragment> VideoFrameLayoutFragments = new List<Fragment>();
		private void SetVideoPlayerAndFragment()
		{
			try
			{
				CommentsFragment = new CommentsFragment();
				NextToFragment = new NextToFragment();

				var ftvideo = Activity.SupportFragmentManager.BeginTransaction();
				ftvideo.Add(Resource.Id.videoButtomLayout, NextToFragment, "NextTo")?.Commit();

				PlayerView = Activity.FindViewById<PlayerView>(Resource.Id.player_view);
				ExoController = new ExoController(Activity);

				ExoController?.SetPlayer(PlayerView);
				ExoController?.SetPlayerControl();

				TubePlayerView = Activity.FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
				if (TubePlayerView != null)
				{
					TubePlayerView.Initialize(this);

					TubePlayerView.Visibility = ViewStates.Gone;

					// The player will automatically release itself when the activity is destroyed.
					// The player will automatically pause when the activity is paused
					// If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
					Activity.Lifecycle.AddObserver(TubePlayerView);

					if (TubePlayerView.PlayerUiController != null)
					{
						TubePlayerView.PlayerUiController.ShowMenuButton(false);

						TubePlayerView.PlayerUiController.ShowCustomActionLeft1(true);
						TubePlayerView.PlayerUiController.SetCustomActionLeft1(ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_exo_icon_previous), this);

						TubePlayerView.PlayerUiController.ShowCustomActionRight1(true);
						TubePlayerView.PlayerUiController.SetCustomActionRight1(ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_exo_icon_next), this);

						TubePlayerView.PlayerUiController.ShowCustomActionLeft2(true);
						TubePlayerView.PlayerUiController.SetCustomActionLeft2(ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_exo_icon_rewind), this);

						TubePlayerView.PlayerUiController.ShowCustomActionRight2(true);
						TubePlayerView.PlayerUiController.SetCustomActionRight2(ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_exo_icon_fastforward), this);
					}

					//TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
				}

				if (ThirdPartyPlayersFragment == null)
				{
					var ft1 = Activity.SupportFragmentManager.BeginTransaction();
					ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment();
					ft1.Add(Resource.Id.root, ThirdPartyPlayersFragment, ThirdPartyPlayersFragment.Tag)?.Commit();

					if (!VideoFrameLayoutFragments.Contains(ThirdPartyPlayersFragment))
						VideoFrameLayoutFragments.Add(ThirdPartyPlayersFragment);
				}

				if (RestrictedVideoPlayerFragment == null)
				{
					var ft2 = Activity.SupportFragmentManager.BeginTransaction();
					RestrictedVideoPlayerFragment = new RestrictedVideoFragment();
					ft2.Add(Resource.Id.root, RestrictedVideoPlayerFragment, RestrictedVideoPlayerFragment.Tag)?.Commit();

					if (!VideoFrameLayoutFragments.Contains(RestrictedVideoPlayerFragment))
						VideoFrameLayoutFragments.Add(RestrictedVideoPlayerFragment);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		public void ShareIconOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.ShareVideo(VideoData);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void DownloadIconOnClick(object sender, EventArgs e)
		{
			try
			{
				if (PermissionsController.CheckPermissionStorage(Activity))
				{
					DownloadVideo();
				}
				else
				{
					new PermissionsController(Activity).RequestPermission(100);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void ExoBackButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ActivityName == "TabbedMainActivity")
				{
					TabbedMainActivity?.BackIcon_Click();
				}
				else if (ActivityName == "GlobalPlayerActivity")
				{
					switch (VideoType)
					{
						case VideoEnumTypes.Normal:
							if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
							{
								GlobalPlayerActivity.EnterPictureInPictureMode(GlobalPlayerActivity.PictureInPictureParams);
							}
							break;
						case VideoEnumTypes.Youtube:
							FinishActivityAndTask();
							break;
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void AutoNextSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				UserDetails.AutoNext = AutoNextSwitch.Checked;
				MainSettings.AutoNext?.Edit()?.PutBoolean(MainSettings.PrefKeyAutoNext, UserDetails.AutoNext)?.Commit();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void CommentBottomLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (AutoNextSection.Tag?.ToString() == "Comments")
				{
					HideCommentsAndShowNextTo();
				}
				else if (AutoNextSection.Tag?.ToString() == "NextTo")
				{
					HideNextToAndShowComments();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void DonateButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(VideoData.Owner?.OwnerClass?.DonationPaypalEmail))
				{
					var url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=" + VideoData.Owner?.OwnerClass?.DonationPaypalEmail + "&lc=US&item_name=Donation+to+" + VideoData.Owner?.OwnerClass?.Name + "&no_note=0&cn=&currency_code=USD&bn=PP-DonationsBF:btn_donateCC_LG.gif:NonHosted";
					new IntentController(Activity).OpenBrowserFromApp(url);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void RentButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var dialog = new MaterialAlertDialogBuilder(Activity);
				dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
				dialog.SetMessage(Activity.GetText(Resource.String.Lbl_RentVideo));
				dialog.SetPositiveButton(Activity.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
				{
					try
					{
						if (await AppTools.CheckWallet(Convert.ToDouble(VideoData.RentPrice)))
						{
							if (Methods.CheckConnectivity())
							{
								var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("rent", VideoData.Id);
								if (apiStatus == 200)
								{
									if (respond is MessageObject result)
									{
										Console.WriteLine(result.Message);

										Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
										if (VideoData != null)
										{
											VideoData.RentPrice = "0";
											LoadDataVideo(VideoData);
										}
									}
								}
								else Methods.DisplayAndHudErrorResult(Activity, respond);
							}
							else
								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
						}
						else
						{
							var dialogBuilder = new MaterialAlertDialogBuilder(Activity);
							dialogBuilder.SetTitle(Activity.GetText(Resource.String.Lbl_Wallet));
							dialogBuilder.SetMessage(Activity.GetText(Resource.String.Lbl_Error_NoWallet));
							dialogBuilder.SetPositiveButton(Activity.GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
							{
								try
								{
									var intent = new Intent(Activity, typeof(WalletActivity));
									intent.PutExtra("Type", "Rent");
									Activity.StartActivity(intent);
								}
								catch (Exception exception)
								{
									Methods.DisplayReportResultTrack(exception);
								}
							});
							dialogBuilder.SetNegativeButton(Activity.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

							dialogBuilder.Show();
						}
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				});
				dialog.SetNegativeButton(Activity.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

				dialog.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void EditButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ActivityName == "GlobalPlayerActivity")
				{
					Intent intent = new Intent();
					intent.PutExtra("Open", "EditVideo");
					intent.PutExtra("ItemDataVideo", JsonConvert.SerializeObject(VideoData));
					Activity.SetResult(Result.Ok, intent);
				}
				else if (ActivityName == "TabbedMainActivity")
				{
					Bundle bundle = new Bundle();
					bundle.PutString("ItemDataVideo", JsonConvert.SerializeObject(VideoData));

					var editVideoFragment = new EditVideoFragment
					{
						Arguments = bundle
					};

					TabbedMainActivity.FragmentBottomNavigator.DisplayFragment(editVideoFragment);
				}

				FinishActivityAndTask();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void VideoShowDescriptionOnClick(object sender, EventArgs e)
		{
			try
			{
				if (VideoDescriptionLayout.Tag?.ToString() == "Open")
				{
					VideoDescriptionLayout.Visibility = ViewStates.Gone;
					VideoDescriptionLayout.Tag = "closed";
					TxtVideoTitle.Text = Methods.FunString.DecodeString(VideoData.Title);
					VideoDescriptionLayout.Animate().Alpha(1).SetDuration(400);
					TxtChannelName.Animate().Alpha(1).SetDuration(300);
					TxtChannelViews.Animate().Alpha(1).SetDuration(300);
					TxtVideoTitle.SetMaxLines(1);

					ViewGroup parent = (ViewGroup)TxtVideoDescription.Parent;
					ViewGroup.LayoutParams par = parent.LayoutParameters;
					par.Height = 200;
					VideoDescriptionLayout.LayoutParameters = par;
				}
				else
				{
					ViewGroup parent = (ViewGroup)TxtVideoDescription.Parent;
					ViewGroup.LayoutParams par = parent.LayoutParameters;
					par.Height = ViewGroup.LayoutParams.WrapContent;
					VideoDescriptionLayout.LayoutParameters = par;

					VideoDescriptionLayout.Visibility = ViewStates.Visible;
					VideoDescriptionLayout.Tag = "Open";
					TxtVideoTitle.Text = Methods.FunString.DecodeString(VideoData.Title);
					VideoDescriptionLayout.Animate().Alpha(1).SetDuration(500);
					TxtChannelName.Animate().Alpha(1).SetDuration(300);
					TxtChannelViews.Animate().Alpha(1).SetDuration(300);
					TxtVideoTitle.SetMaxLines(4);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtVideoCategoryOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ActivityName == "GlobalPlayerActivity")
				{
					Intent intent = new Intent();
					intent.PutExtra("Open", "VideosByCategory");
					intent.PutExtra("CatId", VideoData.CategoryId);
					intent.PutExtra("CatName", VideoData.CategoryName);
					Activity.SetResult(Result.Ok, intent);
				}
				else if (ActivityName == "TabbedMainActivity")
				{
					Bundle bundle = new Bundle();
					bundle.PutString("CatId", VideoData.CategoryId);
					bundle.PutString("CatName", VideoData.CategoryName);

					var videoViewerFragment = new VideosByCategoryFragment
					{
						Arguments = bundle
					};

					TabbedMainActivity.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
				}

				FinishActivityAndTask();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ImageChannelOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ActivityName == "GlobalPlayerActivity")
				{
					Intent intent = new Intent();
					intent.PutExtra("Open", "UserProfile");
					intent.PutExtra("UserObject", JsonConvert.SerializeObject(VideoData.Owner?.OwnerClass));
					Activity.SetResult(Result.Ok, intent);
				}
				else if (ActivityName == "TabbedMainActivity")
				{
					TabbedMainActivity.ShowUserChannelFragment(VideoData.Owner?.OwnerClass, VideoData.Owner?.OwnerClass?.Id);
				}

				FinishActivityAndTask();
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
						if (!string.IsNullOrEmpty(VideoData.Owner?.OwnerClass?.SubscriberPrice) && VideoData.Owner?.OwnerClass?.SubscriberPrice != "0")
						{
							if (SubscribeButton.Tag?.ToString() == "PaidSubscribe")
							{
								//This channel is paid, You must pay to subscribe
								var dialog = new MaterialAlertDialogBuilder(Activity);
								dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
								dialog.SetMessage(Activity.GetText(Resource.String.Lbl_ChannelIsPaid));
								dialog.SetPositiveButton(Activity.GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
								{
									try
									{
										if (await AppTools.CheckWallet(Convert.ToDouble(VideoData.Owner?.OwnerClass?.SubscriberPrice)))
										{
											if (Methods.CheckConnectivity())
											{
												var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", VideoData.Owner?.OwnerClass.Id);
												if (apiStatus == 200)
												{
													if (respond is MessageObject result)
													{
														Console.WriteLine(result.Message);

														Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
														SetSubscribeChannelWithPaid();
													}
												}
												else Methods.DisplayAndHudErrorResult(Activity, respond);
											}
											else
												Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
										}
										else
										{
											var dialogBuilder = new MaterialAlertDialogBuilder(Activity);
											dialogBuilder.SetTitle(Activity.GetText(Resource.String.Lbl_Wallet));
											dialogBuilder.SetMessage(Activity.GetText(Resource.String.Lbl_Error_NoWallet));
											dialogBuilder.SetPositiveButton(Activity.GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
											{
												try
												{
													Activity.StartActivity(new Intent(Activity, typeof(WalletActivity)));
												}
												catch (Exception exception)
												{
													Methods.DisplayReportResultTrack(exception);
												}
											});
											dialogBuilder.SetNegativeButton(Activity.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

											dialogBuilder.Show();
										}
									}
									catch (Exception exception)
									{
										Methods.DisplayReportResultTrack(exception);
									}
								});
								dialog.SetNegativeButton(Activity.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

								dialog.Show();
							}
							else
							{
								SubscribeButton.Tag = "Subscribe";
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);

								//Remove The Video to Subscribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(VideoData.Owner?.OwnerClass?.Id);

								//Send API Request here for UnSubscribed
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoData.Owner?.OwnerClass?.Id) });

								// Toast.MakeText(activity, activity.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
						else
						{
							if (SubscribeButton.Tag?.ToString() == "Subscribe")
							{
								SubscribeButton.Tag = "Subscribed";
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);

								//Add The Video to  Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.Insert_One_SubscriptionChannel(VideoData.Owner?.OwnerClass);
								LibrarySynchronizer.AddToSubscriptions(VideoData.Owner?.OwnerClass);

								//Send API Request here for Subcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoData.Owner?.OwnerClass?.Id) });

								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
							}
							else
							{
								SubscribeButton.Tag = "Subscribe";
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);

								//Remove The Video to Subcribed Videos Database
								var sqlEntity = new SqLiteDatabase();
								sqlEntity.RemoveSubscriptionsChannel(VideoData.Owner?.OwnerClass?.Id);

								//Send API Request here for UnSubcribe
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoData.Owner?.OwnerClass?.Id) });

								// Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Subcribed), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
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

		private void AddToButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var dialogList = new MaterialAlertDialogBuilder(Activity);

				var arrayAdapter = new List<string>
				{
					Activity.GetString(Resource.String.Lbl_Addto_playlist),

					VideoData.IsWatchLater != null && VideoData.IsWatchLater.Value ? Activity.GetString(Resource.String.Lbl_RemoveFromWatchLater) : Activity.GetString(Resource.String.Lbl_Addto_watchlater)
				};

				dialogList.SetTitle(Activity.GetString(Resource.String.Lbl_Add_To));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(Activity.GetString(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ShareButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer?.ShareVideo(VideoData);
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
							UnLikeIcon.SetColorFilter(SelectIconColor);
							LikeIcon.SetColorFilter(DefaultIconColor);

							if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
							{
								var x = Convert.ToDouble(UnLikeNumber.Text);
								x++;
								UnLikeNumber.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							if (LikeButton.Tag?.ToString() == "1")
							{
								LikeButton.Tag = "0";
								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeNumber.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									LikeNumber.Text = x.ToString(CultureInfo.CurrentCulture);
								}
							}

							Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short)?.Show();

							//Send API Request here for dislike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoData.Id, "dislike") });
						}
						else
						{
							UnLikeButton.Tag = "0";

							UnLikeIcon.SetColorFilter(DefaultIconColor);
							var x = Convert.ToDouble(UnLikeNumber.Text);
							if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
							{
								if (x > 0)
								{
									x--;
								}
								else
								{
									x = 0;
								}
								UnLikeNumber.Text = x.ToString(CultureInfo.CurrentCulture);
							}

							//Send API Request here for Remove UNLike
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoData.Id, "dislike") });

						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "Login");
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
								LikeIcon.SetColorFilter(SelectIconColor);


								UnLikeIcon.SetColorFilter(DefaultIconColor);
								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeNumber.Text);
									x++;
									LikeNumber.Text = x.ToString(CultureInfo.InvariantCulture);
								}

								if (UnLikeButton.Tag?.ToString() == "1")
								{
									UnLikeButton.Tag = "0";
									if (!UnLikeNumber.Text.Contains("K") && !UnLikeNumber.Text.Contains("M"))
									{
										var x = Convert.ToDouble(UnLikeNumber.Text);
										if (x > 0)
										{
											x--;
										}
										else
										{
											x = 0;
										}
										UnLikeNumber.Text = x.ToString(CultureInfo.CurrentCulture);
									}
								}


								//AddToLiked
								LibrarySynchronizer.AddToLiked(VideoData);

								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Like
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoData.Id, "like") });
							}
							else
							{
								LikeButton.Tag = "0";

								LikeIcon.SetColorFilter(DefaultIconColor);
								UnLikeIcon.SetColorFilter(DefaultIconColor);
								if (!LikeNumber.Text.Contains("K") && !LikeNumber.Text.Contains("M"))
								{
									var x = Convert.ToDouble(LikeNumber.Text);
									if (x > 0)
									{
										x--;
									}
									else
									{
										x = 0;
									}

									LikeNumber.Text = x.ToString(CultureInfo.CurrentCulture);
								}

								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short)?.Show();

								//Send API Request here for Remove UNLike
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoData.Id, "like") });
							}
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
						}
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Like), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
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

		#region Load Data Video 

		public async void LoadDataVideo(VideoDataObject videoData)
		{
			try
			{
				ResetAllButton();
				VideoData = videoData;
				if (videoData == null)
					return;

				ShimmerInflater?.Show();

				SetDataVideo(videoData);

				if (Methods.CheckConnectivity())
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosById(videoData.VideoId), () => ApiRequest.AdsVideosApi(Activity), ApiRequest.GetNotInterestedVideos });

				if (ActivityName == "GlobalPlayerActivity")
					await Task.Delay(2000);

				StartPlayVideo(videoData);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetDataVideo(VideoDataObject videoData)
		{
			try
			{
				SetVideoType(videoData);

				TxtVideoTitle.Text = Methods.FunString.DecodeString(videoData.Title);

				if (TabbedMainActivity != null)
				{
					TabbedMainActivity.VideoChannelText.Text = videoData.Owner?.OwnerClass?.Username;
					TabbedMainActivity.VideoTitleText.Text = Methods.FunString.DecodeString(videoData.Title);
					TabbedMainActivity.VideoChannelText.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoData.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
				}

				//Verified 
				TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoData.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
				TxtChannelName.Text = AppTools.GetNameFinal(videoData.Owner?.OwnerClass);
				GlideImageLoader.LoadImage(Activity, videoData.Owner?.OwnerClass?.Avatar, ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

				if (videoData.Owner != null && string.IsNullOrEmpty(videoData.Owner?.OwnerClass?.SubscribeCount))
					videoData.Owner.Value.OwnerClass.SubscribeCount = "0";

				TxtChannelViews.Text = videoData.Owner?.OwnerClass?.SubscribeCount + " " + Activity.GetText(Resource.String.Lbl_Subscribers);

				//2M Views | 10K Shares | 500 Comments | 3 Months Ago
				TxtInfo.Text = CategoriesController.GetCategoryName(videoData) + " | " + videoData.Views + " " + Activity.GetText(Resource.String.Lbl_Views) + " | " + videoData.TimeAgo;

				if (videoData.IsLiked == "1") // true
				{
					LikeIcon.SetColorFilter(SelectIconColor);
					UnLikeIcon.SetColorFilter(DefaultIconColor);
					LikeButton.Tag = "1";
				}
				else
				{
					LikeIcon.SetColorFilter(DefaultIconColor);
					LikeButton.Tag = "0";
				}

				if (videoData.IsDisliked == "1") // true
				{
					LikeIcon.SetColorFilter(DefaultIconColor);
					UnLikeIcon.SetColorFilter(SelectIconColor);
					UnLikeButton.Tag = "1";
				}
				else
				{
					UnLikeIcon.SetColorFilter(DefaultIconColor);
					UnLikeButton.Tag = "0";
				}

				var isOwner = videoData.IsOwner != null && videoData.IsOwner.Value;
				SubscribeButton.Visibility = isOwner ? ViewStates.Invisible : ViewStates.Visible;
				if (!isOwner)
				{
					if (!string.IsNullOrEmpty(videoData.Owner?.OwnerClass?.SubscriberPrice) && videoData.Owner?.OwnerClass?.SubscriberPrice != "0")
					{
						if (videoData.Owner?.OwnerClass?.AmISubscribed == "0")
						{
							//This channel is paid, You must pay to subscribe
							SubscribeButton.Tag = "PaidSubscribe";

							var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
							var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
							Console.WriteLine(currency);
							SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + videoData.Owner?.OwnerClass?.SubscriberPrice;
						}
						else
						{
							SubscribeButton.Tag = "Subscribed";
							SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);
						}
					}
					else
					{
						SubscribeButton.Tag = videoData.Owner?.OwnerClass?.AmISubscribed == "1" ? "Subscribed" : "Subscribe";

						switch (SubscribeButton.Tag?.ToString())
						{
							case "Subscribed":
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);
								break;
							case "Subscribe":
								SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);
								break;
						}
					}
				}

				//Edit
				EditButton.Visibility = isOwner ? ViewStates.Visible : ViewStates.Gone;

				//Rent
				RentButton.Visibility = !string.IsNullOrEmpty(videoData.RentPrice) && videoData.RentPrice != "0" && AppSettings.RentVideosSystem ? ViewStates.Visible : ViewStates.Gone;

				//Donate
				DonateButton.Visibility = !string.IsNullOrEmpty(videoData.Owner?.OwnerClass?.DonationPaypalEmail) && AppSettings.DonateVideosSystem ? ViewStates.Visible : ViewStates.Gone;

				LikeNumber.Text = videoData.Likes ?? "0";
				UnLikeNumber.Text = videoData.Dislikes ?? "0";
				TxtCountComments.Text = videoData.CommentsCount ?? "";

				if (!string.IsNullOrEmpty(videoData.TimeDate))
				{
					TxtPublishDate.Text = Activity.GetText(Resource.String.Lbl_Published_on) + " " + Methods.Time.ConvertToSpanishFormatIfNeeded(videoData.TimeDate);
				}
				else
				{
					DateTime dateTime = Methods.Time.UnixTimeStampToDateTime(int.Parse(videoData.Time));
					TxtPublishDate.Text = Activity.GetText(Resource.String.Lbl_Published_on) + " " + dateTime.ToLongDateString() + " " + dateTime.ToShortTimeString();
				}

				TxtVideoCategory.Text = CategoriesController.GetCategoryName(videoData);

				TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoData.Description));

				SetDownloadIcon();

				VideoDescriptionLayout.Visibility = ViewStates.Gone;
				VideoDescriptionLayout.Tag = "closed";
				//TxtVideoTitle.Text = Methods.FunString.DecodeString(VideoData.Title);
				VideoDescriptionLayout.Animate().Alpha(1).SetDuration(400);
				TxtChannelName.Animate().Alpha(1).SetDuration(300);
				TxtChannelViews.Animate().Alpha(1).SetDuration(300);
				//TxtVideoTitle.SetMaxLines(1);

				ViewGroup parent = (ViewGroup)TxtVideoDescription.Parent;
				ViewGroup.LayoutParams par = parent.LayoutParameters;
				par.Height = 200;
				VideoDescriptionLayout.LayoutParameters = par;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetDownloadIcon()
		{
			try
			{
				var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(VideoData.Title);
				if (!string.IsNullOrEmpty(file))
				{
					ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
					ExoController.DownloadIcon.Tag = "Downloaded";
					LibrarySynchronizer.AddToWatchOffline(VideoData);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		private void ResetAllButton()
		{
			try
			{
				LikeButton.Tag = "0";
				LikeIcon.SetColorFilter(DefaultIconColor);
				LikeNumber.Text = "0";

				UnLikeButton.Tag = "0";
				UnLikeIcon.SetColorFilter(DefaultIconColor);
				UnLikeNumber.Text = "0";

				SubscribeButton.Tag = "Subscribe";
				SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe);

				VideoDescriptionLayout.Tag = "closed";

				//Clear all data 
				if (CommentsFragment?.MAdapter != null)
				{
					CommentsFragment.MAdapter.CommentList?.Clear();
					CommentsFragment.MAdapter.NotifyDataSetChanged();
				}

				if (NextToFragment?.MAdapter != null)
				{
					NextToFragment.MAdapter.VideoList?.Clear();
					NextToFragment.MAdapter.NotifyDataSetChanged();
				}

				HideCommentsAndShowNextTo();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetVideoType(VideoDataObject data)
		{
			try
			{
				VideoType = VideoEnumTypes.Normal;
				var myDetails = ListUtils.MyChannelList?.FirstOrDefault();
				var age = string.IsNullOrWhiteSpace(myDetails?.Age) ? 0 : Convert.ToInt32(myDetails.Age);
				var isBelow18 = age > 0 && age < 18;

				if (data.VideoLocation.Contains("Youtube") || data.VideoLocation.Contains("youtu") || data.VideoType == "VideoObject/youtube")
					VideoType = VideoEnumTypes.Youtube;
				else if (!string.IsNullOrEmpty(data.Vimeo))
					VideoType = VideoEnumTypes.Vimeo;
				else if (!string.IsNullOrEmpty(data.Daily))
					VideoType = VideoEnumTypes.DailyMotion;
				else if (!string.IsNullOrEmpty(data.Ok))
					VideoType = VideoEnumTypes.Ok;
				else if (!string.IsNullOrEmpty(data.Twitch))
					VideoType = VideoEnumTypes.Twitch;
				else if (!string.IsNullOrEmpty(data.Facebook))
					VideoType = VideoEnumTypes.Facebook;
				else if (data.IsOwner != null && data.AgeRestriction == "2" && data.IsOwner.Value == false && isBelow18)
					VideoType = VideoEnumTypes.AgeRestricted;
				else if (!string.IsNullOrEmpty(data.GeoBlocking) && data.IsOwner == false)
					VideoType = VideoEnumTypes.GeoBlocked;

				data.VideoType = VideoType.ToString();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async Task GetVideosById(string videoId)
		{
			try
			{
				var (apiStatus, respond) = await RequestsAsync.Video.GetVideosDetailsAsync(videoId, UserDetails.UserId + "_" + UserDetails.DeviceId?.Replace("-", "") ?? "1");
				if (apiStatus == 200)
				{
					if (respond is GetVideosDetailsObject result)
					{
						VideoData = AppTools.VideoFilter(result.DataResult);
						SetDataVideo(VideoData);
						var updater = ListUtils.GlobalViewsVideosList.FirstOrDefault(a => a.VideoId == videoId);
						if (updater == null)
						{
							ListUtils.GlobalViewsVideosList.Add(result.DataResult);
						}

						result.DataResult.SuggestedVideos = AppTools.ListFilter(result.DataResult.SuggestedVideos);
						result.DataResult.SuggestedVideos.RemoveAll(a => a.IsShort == "1");

						if (IsPlaylist)
						{
							ListUtils.ArrayListPlay = new ObservableCollection<VideoDataObject>(SubPlaylistVideoList);
							NextToFragment?.LoadDataAsync(new ObservableCollection<VideoDataObject>(SubPlaylistVideoList));
						}
						else
						{
							ListUtils.ArrayListPlay = new ObservableCollection<VideoDataObject>(result.DataResult.SuggestedVideos);
							NextToFragment?.LoadDataAsync(new ObservableCollection<VideoDataObject>(result.DataResult.SuggestedVideos));
						}

						CommentsFragment?.StartApiService(VideoData.Id, "0");

						ListUtils.AdsVideoList ??= new ObservableCollection<VideoAdDataObject>();
						if (result.DataResult.VideoAd.VideoAdClass != null)
						{
							var ads = ListUtils.AdsVideoList.FirstOrDefault(a => a.Id == result.DataResult.VideoAd.VideoAdClass.Id);
							if (ads == null)
							{
								ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
							}
						}
					}

					Activity?.RunOnUiThread(() => { ShimmerInflater?.Hide(); });
				}
				else
				{
					Activity?.RunOnUiThread(() => { ShimmerInflater?.Hide(); });
					Methods.DisplayReportResult(Activity, respond);
				}
			}
			catch (Exception e)
			{
				Activity?.RunOnUiThread(() => { ShimmerInflater?.Hide(); });
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Fun Video

		public void StartPlayVideo(string url, long position)
		{
			try
			{
				GlobalVideosRelease("exo");
				CustomNavigationController.BringFragmentToTop(null, Activity.SupportFragmentManager, VideoFrameLayoutFragments);

				// Uri
				var uri = Uri.Parse(url);
				ExoController?.FirstPlayVideo(uri, position);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void StartPlayVideo(VideoDataObject videoObject)
		{
			try
			{
				RestrictedVideoPlayerFragment?.HideRestrictedInfo(true);
				//UpdateMainRootDefaultSize();

				var userNotVideoOwner = videoObject.IsOwner != null && !videoObject.IsOwner.Value;
				 
                switch (VideoType)
				{
					case VideoEnumTypes.AgeRestricted:
						GlobalVideosRelease("All");
						CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, Activity.SupportFragmentManager, VideoFrameLayoutFragments);
						RestrictedVideoPlayerFragment.LoadRestriction("AgeRestriction", videoObject.Thumbnail, videoObject);
						break;
					case VideoEnumTypes.GeoBlocked:
						GlobalVideosRelease("All");
						CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, Activity.SupportFragmentManager, VideoFrameLayoutFragments);
						RestrictedVideoPlayerFragment.LoadRestriction("GeoRestriction", videoObject.Thumbnail, videoObject);
						break;
					case VideoEnumTypes.Youtube:
						VideoIdYoutube = videoObject.VideoLocation.Split(new[] { "v=" }, StringSplitOptions.None).LastOrDefault();
						GlobalVideosRelease("Youtube");
						CustomNavigationController.BringFragmentToTop(null, Activity.SupportFragmentManager, VideoFrameLayoutFragments);

						if (Activity.Lifecycle.CurrentState == Lifecycle.State.Resumed)
							YoutubePlayer?.LoadVideo(VideoIdYoutube, 0);
						else
							YoutubePlayer?.CueVideo(VideoIdYoutube, 0);

						break;
					case VideoEnumTypes.Facebook:
					case VideoEnumTypes.Twitch:
					case VideoEnumTypes.DailyMotion:
					case VideoEnumTypes.Ok:
					case VideoEnumTypes.Vimeo:
						GlobalVideosRelease("All");
						CustomNavigationController.BringFragmentToTop(ThirdPartyPlayersFragment, Activity.SupportFragmentManager, VideoFrameLayoutFragments);
						ThirdPartyPlayersFragment.SetVideoIframe(videoObject);
						break;
					case VideoEnumTypes.Normal:
					default:
						GlobalVideosRelease("exo");
						CustomNavigationController.BringFragmentToTop(null, Activity.SupportFragmentManager, VideoFrameLayoutFragments);

						// Uri
						Uri uri;
						//Rent Or Sell
						if (userNotVideoOwner && (!string.IsNullOrEmpty(videoObject.SellVideo) && videoObject.SellVideo != "0"
												   || !string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem
												   || !string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscriberPrice) && videoObject.Owner?.OwnerClass?.SubscriberPrice != "0" & videoObject.Owner?.OwnerClass?.AmISubscribed == "0"))
						{
							if (!string.IsNullOrEmpty(videoObject.Demo) && videoObject.IsPurchased == "0")
							{
								if (!videoObject.Demo.Contains(InitializePlayTube.WebsiteUrl))
									videoObject.Demo = InitializePlayTube.WebsiteUrl + "/" + videoObject.Demo;

								uri = Uri.Parse(videoObject.Demo);
								ShowRestrictedVideo = true;
							}
							else if (videoObject.IsPurchased != "0")
							{
								uri = Uri.Parse(videoObject.VideoLocation);
							}
							else
							{
								if (!string.IsNullOrEmpty(videoObject.SellVideo) && videoObject.SellVideo != "0")
									ShowRestrictedVideoFragment(RestrictedVideoPlayerFragment, Activity, "purchaseVideo");
								else if (!string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem)
									ShowRestrictedVideoFragment(RestrictedVideoPlayerFragment, Activity, "RentVideo");
								else if (!string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscriberPrice) && videoObject.Owner?.OwnerClass?.SubscriberPrice != "0" & videoObject.Owner?.OwnerClass?.AmISubscribed == "0")
									ShowRestrictedVideoFragment(RestrictedVideoPlayerFragment, Activity, "Subscriber");
								return;
							}
						}
						else
						{
							uri = Uri.Parse(videoObject.VideoLocation);
						}

						ExoController?.FirstPlayVideo(uri, videoObject);
						break;
				}
                
				LibrarySynchronizer.AddToRecentlyWatched(videoObject);

				if (TabbedMainActivity != null)
				{
					TabbedMainActivity.SetOnWakeLock();

					if (TabbedMainActivity.LibraryFragment != null)
					{
						var check = TabbedMainActivity.LibraryFragment.MAdapterRecently?.VideoList.FirstOrDefault(a => a.Id == videoObject?.Id);
						if (check == null)
						{
							TabbedMainActivity.LibraryFragment.MAdapterRecently?.VideoList?.Insert(0, videoObject);
							TabbedMainActivity.LibraryFragment.MAdapterRecently?.NotifyDataSetChanged();
						}
						else
						{
							TabbedMainActivity.LibraryFragment.MAdapterRecently?.VideoList?.Move(TabbedMainActivity.LibraryFragment.MAdapterRecently.VideoList.IndexOf(check), 0);
							TabbedMainActivity.LibraryFragment.MAdapterRecently?.NotifyDataSetChanged();
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		 
		public void ShowRestrictedVideoFragment(RestrictedVideoFragment restrictedVideoPlayerFragment, Activity activity, string type)
		{
			try
			{
				AndroidX.Fragment.App.FragmentTransaction ft = null;
				switch (activity)
				{
					case GlobalPlayerActivity act:
						ft = act.SupportFragmentManager.BeginTransaction();
						break;
					case TabbedMainActivity act2:
						ft = act2.SupportFragmentManager.BeginTransaction();
						break;
				}

				PlayerView.Visibility = ViewStates.Gone;
				ExoController.ReleaseVideo();

				if (restrictedVideoPlayerFragment == null)
				{
					restrictedVideoPlayerFragment = new RestrictedVideoFragment();
					restrictedVideoPlayerFragment.UpdateRestrictionData(type, VideoData.Thumbnail, VideoData);
				}

				if (restrictedVideoPlayerFragment.IsAdded)
				{
					ft?.Show(restrictedVideoPlayerFragment)?.Commit();
					restrictedVideoPlayerFragment.LoadRestriction(type, VideoData.Thumbnail, VideoData);
				}
				else
				{
					ft?.Add(Resource.Id.root, restrictedVideoPlayerFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture))?.Commit();
				}

				if (!VideoFrameLayoutFragments.Contains(restrictedVideoPlayerFragment))
					VideoFrameLayoutFragments.Add(restrictedVideoPlayerFragment);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void GlobalVideosRelease(string type)
		{
			try
			{
				switch (type)
				{
					case "exo":
						{
							YoutubePlayer?.Pause();

							if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Gone;

							PlayerView.Visibility = ViewStates.Visible;

							ThirdPartyPlayersFragment.WebViewPlayer = null;
							break;
						}
					case "Youtube":
						{
							PlayerView.Visibility = ViewStates.Gone;
							ExoController?.StopVideo();

							if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Visible;

							ThirdPartyPlayersFragment.WebViewPlayer = null;

							break;
						}
					case "Other":
						{
							if (ThirdPartyPlayersFragment.IsPlay)
							{
								ThirdPartyPlayersFragment.WebViewPlayer.Destroy();

								var ft1 = Activity.SupportFragmentManager.BeginTransaction();
								ft1.Remove(ThirdPartyPlayersFragment);
								ThirdPartyPlayersFragment.IsPlay = false;
								ThirdPartyPlayersFragment.OnDestroy();
								ThirdPartyPlayersFragment = null!;
							}

							if (ThirdPartyPlayersFragment == null)
							{
								var ft1 = Activity.SupportFragmentManager.BeginTransaction();
								ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment();
								ft1.Add(Resource.Id.root, ThirdPartyPlayersFragment, ThirdPartyPlayersFragment.Tag)?.Commit();

								if (!VideoFrameLayoutFragments.Contains(ThirdPartyPlayersFragment))
									VideoFrameLayoutFragments.Add(ThirdPartyPlayersFragment);
							}

							break;
						}
					case "All":
						{
							YoutubePlayer?.Pause();

							if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Gone;

							PlayerView.Visibility = ViewStates.Gone;
							ExoController?.StopVideo();

							if (ThirdPartyPlayersFragment.IsPlay)
							{
								ThirdPartyPlayersFragment.WebViewPlayer.Destroy();

								var ft1 = Activity.SupportFragmentManager.BeginTransaction();
								ft1.Remove(ThirdPartyPlayersFragment);
								ThirdPartyPlayersFragment.IsPlay = false;
								ThirdPartyPlayersFragment.OnDestroy();
								ThirdPartyPlayersFragment = null!;
							}

							if (ThirdPartyPlayersFragment == null)
							{
								var ft1 = Activity.SupportFragmentManager.BeginTransaction();
								ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment();
								ft1.Add(Resource.Id.root, ThirdPartyPlayersFragment, ThirdPartyPlayersFragment.Tag)?.Commit();

								if (!VideoFrameLayoutFragments.Contains(ThirdPartyPlayersFragment))
									VideoFrameLayoutFragments.Add(ThirdPartyPlayersFragment);
							}

							break;
						}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnDestroy()
		{
			try
			{
				ExoController?.ReleaseVideo();
				TubePlayerView?.Release();

				GlobalVideosRelease("All");
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnStop()
		{
			try
			{
				if (!IsFullScreen)
					ExoController?.StopVideo();

				YoutubePlayer?.Pause();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void NewLoad(VideoDataObject videoObject)
		{
			try
			{
				InitComponent();

				//Called onNewIntent
				ExoController.StopVideo();

				YoutubePlayer?.Pause();

				LoadDataVideo(videoObject);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Full Screen

		//close full screen video with Default "Portrait" Size
		public void UpdateMainRootDefaultSize()
		{
			try
			{
				switch (VideoType)
				{
					case VideoEnumTypes.Normal:
						if (IsFullScreen)
							InitFullscreenDialog("", "Close");
						break;
					case VideoEnumTypes.Youtube:
						if (TubePlayerView != null && TubePlayerView.FullScreen)
							TubePlayerView?.ExitFullScreen();
						break;

				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//open full screen video with Landscape Size
		public void UpdateMainRootDefaultLandscapeSize()
		{
			try
			{
				switch (VideoType)
				{
					case VideoEnumTypes.Normal:
						if (!IsFullScreen)
							InitFullscreenDialog("RequestedOrientation", "Open");
						break;
					case VideoEnumTypes.Youtube:
						if (TubePlayerView != null && !TubePlayerView.FullScreen)
						{
							TypeYouTubePlayerFullScreen = "RequestedOrientation";
							OnYouTubePlayerEnterFullScreen();
						}
						break;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Full Screen
		public void InitFullscreenDialog(string type, string action)
		{
			try
			{
				if (action != "Open")
				{
					Intent intent = new Intent();
					FullScreenVideoActivity.Instance?.SetResult(Result.Ok, intent);
					FullScreenVideoActivity.Instance?.Finish();
					IsFullScreen = false;
				}
				else
				{
					Intent intent = new Intent(Activity, typeof(FullScreenVideoActivity));
					intent.PutExtra("type", type);
					intent.PutExtra("type", type);
					intent.PutExtra("videoUrl", ExoController.VideoUrl?.ToString());
					intent.PutExtra("videoPosition", PlayerView?.Player?.CurrentPosition.ToString());
					Activity.StartActivityForResult(intent, 2000);
					IsFullScreen = true;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion

		#region YouTubePlayer

		public void OnInitSuccess(IYouTubePlayer youTubePlayer)
		{
			try
			{
				if (YoutubePlayer == null)
				{
					YoutubePlayer = youTubePlayer;
					YouTubePlayerEvents = new YouTubePlayerEvents(this, youTubePlayer, VideoIdYoutube);
					YoutubePlayer.AddListener(YouTubePlayerEvents);
					TubePlayerView.AddFullScreenListener(this);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnYouTubePlayerEnterFullScreen()
		{
			try
			{
				Intent intent = new Intent(Activity, typeof(YouTubePlayerFullScreenActivity));
				intent.PutExtra("type", TypeYouTubePlayerFullScreen);
				intent.PutExtra("VideoIdYoutube", VideoIdYoutube);
				intent.PutExtra("CurrentSecond", YouTubePlayerEvents.CurrentSecond);
				Activity.StartActivityForResult(intent, 2100);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnYouTubePlayerExitFullScreen()
		{
			try
			{
				TypeYouTubePlayerFullScreen = "RequestedOrientation";

				if (YouTubePlayerEvents.IsPlaying)
					YoutubePlayer.LoadVideo(VideoIdYoutube, YouTubePlayerEvents.CurrentSecond);

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnClick(View v)
		{
			try
			{
				if (v.Id == Anjo.Android.YouTubePlayerX.Resource.Id.custom_action_left_button)
				{
					YouTubePlayerEvents?.BtnPreviousOnClick();
				}
				else if (v.Id == Anjo.Android.YouTubePlayerX.Resource.Id.custom_action_right_button)
				{
					YouTubePlayerEvents?.BtnNextOnClick();
				}
				else if (v.Id == Anjo.Android.YouTubePlayerX.Resource.Id.custom_action_left_button2)
				{
					YouTubePlayerEvents?.BtnBackwardOnClick(ActivityName);
				}
				else if (v.Id == Anjo.Android.YouTubePlayerX.Resource.Id.custom_action_right_button2)
				{
					YouTubePlayerEvents?.BtnForwardOnClick(ActivityName);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				if (itemString == Activity.GetString(Resource.String.Lbl_Addto_playlist))
				{
					OnMenuAddPlaylistClick();
				}
				else if (itemString == Activity.GetString(Resource.String.Lbl_Addto_watchlater) || itemString == Activity.GetString(Resource.String.Lbl_RemoveFromWatchLater))
				{
					OnMenuAddWatchLaterClick();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Event Menu >> WatchLater
		private async void OnMenuAddWatchLaterClick()
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					//Send API Request here for WatchLater
					if (Methods.CheckConnectivity())
					{
						var (apiStatus, respond) = await RequestsAsync.Video.AddToWatchLaterVideosAsync(VideoData.Id);
						if (apiStatus == 200)
						{
							if (respond is MessageCodeObject result)
							{
								if (result.SuccessType.Contains("Removed"))
								{
									LibrarySynchronizer.RemovedFromWatchLater(VideoData);
									Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_RemovedFromWatchLater), ToastLength.Short)?.Show();
								}
								else if (result.SuccessType.Contains("Added"))
								{
									LibrarySynchronizer.AddToWatchLater(VideoData);
									Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_AddedToWatchLater), ToastLength.Short)?.Show();
								}
							}
						}
						else Methods.DisplayReportResult(Activity, respond);
					}
					else
					{
						Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					}
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "Login");
					dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_WatchLater), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Event Menu >> Playlist
		private void OnMenuAddPlaylistClick()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (UserDetails.IsLogin)
					{
						PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "PlayList");
						dialog.ShowPlayListDialog();
					}
					else
					{
						PopupDialogController dialog = new PopupDialogController(Activity, VideoData, "Login");
						dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_playlist), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
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

		#region Fragment Manager

		public void HideCommentsAndShowNextTo()
		{
			try
			{
				//TxtUpNext.Text = Activity.GetString(Resource.String.Lbl_NextTo);
				AutoNextSection.Tag = "NextTo";
				AutoNextSection.Visibility = ViewStates.Visible;
				IconComments.SetImageResource(Resource.Drawable.pif_video_screen_down);

				var ftvideo = Activity.SupportFragmentManager.BeginTransaction();
				//ftvideo.AddToBackStack(null);
				ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
				ftvideo.Hide(CommentsFragment).Show(NextToFragment)?.Commit();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void HideNextToAndShowComments()
		{
			try
			{
				//TxtUpNext.Text = Activity.GetString(Resource.String.Lbl_Comments);
				AutoNextSection.Tag = "Comments";
				AutoNextSection.Visibility = ViewStates.Gone;
				IconComments.SetImageResource(Resource.Drawable.icon_close_vector);

				var ftvideo = Activity.SupportFragmentManager.BeginTransaction();

				if (!CommentsFragment.IsAdded)
				{
					ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
					//ftvideo.AddToBackStack(null);
					ftvideo.Add(Resource.Id.videoButtomLayout, CommentsFragment, null)?.Commit();
				}
				else
				{
					ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
					ftvideo.Hide(NextToFragment).Show(CommentsFragment)?.Commit();
				}

				CommentsFragment?.StartApiService(VideoData.Id, "0");
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		public async void SetSubscribeChannelWithPaid()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{ 
					//Send API Request here for Subscribed
					var (apiStatus, respond) = await RequestsAsync.Global.AddSubscribeToChannelAsync(VideoData.Owner?.OwnerClass?.Id, "paid");
					if (apiStatus == 200)
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);
							Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();

							SubscribeButton.Tag = "Subscribed";
							SubscribeButton.Text = Activity.GetText(Resource.String.Lbl_Subscribed);

							//Add The Video to  Subscribed Videos Database
							var sqlEntity = new SqLiteDatabase();
							sqlEntity.Insert_One_SubscriptionChannel(VideoData.Owner?.OwnerClass);
							LibrarySynchronizer.AddToSubscriptions(VideoData.Owner?.OwnerClass);

						}
					}
					else Methods.DisplayAndHudErrorResult(Activity, respond);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void FinishActivityAndTask()
		{
			try
			{
				switch (VideoType)
				{
					case VideoEnumTypes.Normal:
						ExoController?.ReleaseVideo();
						break;
					case VideoEnumTypes.Youtube:
						YoutubePlayer?.Stop();
						break;
				}

				if (ActivityName == "GlobalPlayerActivity")
				{
					//MoveTaskToBack(true);
					Activity.FinishAndRemoveTask();
					Activity.Finish();
				}
				else if (ActivityName == "TabbedMainActivity")
				{
					if (TabbedMainActivity.SlidingUpPanel != null && (TabbedMainActivity.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || TabbedMainActivity.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
					{
						TabbedMainActivity.ShowVideoButtomStyle();
						TabbedMainActivity.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static VideoDataWithEventsLoader GetInstance()
		{
			return Instance;
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

							if (VideoData.VideoLocation.Contains("youtube") || VideoData.VideoType.Contains("Youtube") || VideoData.VideoType.Contains("youtu"))
							{
								var urlVideo = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(VideoData.Title);
								if (string.IsNullOrEmpty(urlVideo))
								{
									VideoControllers = new VideoDownloadAsyncController(urlVideo, VideoData.Title, Activity);
									if (!VideoControllers.CheckDownloadLinkIfExits())
										VideoControllers.StartDownloadManager(VideoData.Id, VideoData);
								}
								else
								{
									Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_Error), Activity.GetText(Resource.String.Lbl_You_can_not_Download_video), Activity.GetText(Resource.String.Lbl_Ok));
								}
							}
							else
							{
								VideoControllers = new VideoDownloadAsyncController(VideoData.VideoLocation, VideoData.Title, Activity);
								if (!VideoControllers.CheckDownloadLinkIfExits())
									VideoControllers.StartDownloadManager(VideoData.Id, VideoData);
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
									VideoControllers.RemoveDiskFile(VideoData.Title, VideoData.Id);
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

		public static bool IsPlaylist;
		private static ObservableCollection<VideoDataObject> SubPlaylistVideoList = new ObservableCollection<VideoDataObject>();
		private int IndexSubPlaylistVideo;

		public void SetAsPlaylist(ObservableCollection<DataVideoPlaylistObject> subVideoList, VideoDataObject videoObject, int index = 0)
		{
			try
			{
				var check = subVideoList.FirstOrDefault(a => a.Video?.Id == videoObject.Id);
				if (check != null)
				{
					index = subVideoList.IndexOf(check);
				}

				IndexSubPlaylistVideo = index;

				SubPlaylistVideoList = new ObservableCollection<VideoDataObject>();
				foreach (var data in subVideoList)
				{
					if (data.Video?.Id == videoObject.Id)
						continue;

					SubPlaylistVideoList.Add(data.Video);
				}

				IsPlaylist = true;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}