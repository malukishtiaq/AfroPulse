using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Util;
using Androidx.Media3.Datasource;
using Androidx.Media3.Datasource.Cache;
using Androidx.Media3.Exoplayer;
using Androidx.Media3.Exoplayer.Dash;
using Androidx.Media3.Exoplayer.Hls;
using Androidx.Media3.Exoplayer.Rtsp;
using Androidx.Media3.Exoplayer.Smoothstreaming;
using Androidx.Media3.Exoplayer.Source;
using Androidx.Media3.Exoplayer.Trackselection;
using Androidx.Media3.Extractor.TS;
using Androidx.Media3.UI;
using AndroidX.AppCompat.App;
using Java.Net;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Uri = Android.Net.Uri;

namespace PlayTube.MediaPlayers.Exo
{
	public class ExoController
	{
		private readonly AppCompatActivity ActivityContext;

		private IExoPlayer VideoPlayer, FullScreenVideoPlayer;
		private PlayerView PlayerView, FullScreenPlayerView;
		private PlayerControlView ControlView;

		private PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

		private IDataSource.IFactory DataSourceFactory;
		private IDataSource.IFactory HttpDataSourceFactory;

		public ImageView ExoBackButton, DownloadIcon, ShareIcon, BtnPrev, MenuButton, MFullScreenIcon;
		public ImageButton BtnNext;
		public LinearLayout ExoTopLayout, MFullScreenButton, ExoTopAds, ExoEventButton;
		public FrameLayout BtnBackward, BtnForward;
		public TextView BtnSkipIntro;
		public DefaultTimeBar ProgressTimeBar;

		private VideoAdDataObject DataAdsVideo;
		private Timer TimerAds;
		private readonly string TypePage;
		public Uri VideoUrl;

		public ExoController(AppCompatActivity context, string typePage = "normal")
		{
			try
			{
				ActivityContext = context;
				TypePage = typePage;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetPlayer(PlayerView playerView, bool useController = true)
		{
			try
			{
				PlayerView = playerView;

				PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(ActivityContext);
				DefaultTrackSelector trackSelector = new DefaultTrackSelector(ActivityContext);
				ControlView = PlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

				VideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(trackSelector)?.Build();
				var playerListener = new PlayerEvents(ActivityContext, ControlView, TypePage);
				VideoPlayer?.AddListener(playerListener);

				PlayerView.UseController = useController;
				PlayerView.Player = VideoPlayer;

				PlayerView.SetShutterBackgroundColor(Color.Transparent); 
            }
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void SetPlayerControl(bool showFullScreen = true, bool isFullScreen = false)
		{
			try
			{
				if (ControlView != null)
				{
					//Check All Views  
					ExoTopLayout = ControlView.FindViewById<LinearLayout>(Resource.Id.topLayout);
					ExoBackButton = ControlView.FindViewById<ImageView>(Resource.Id.BackIcon);
					DownloadIcon = ControlView.FindViewById<ImageView>(Resource.Id.Download_icon);
					ShareIcon = ControlView.FindViewById<ImageView>(Resource.Id.share_icon);
					MenuButton = ControlView.FindViewById<ImageView>(Resource.Id.exo_more_icon);

					ExoTopAds = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_top_ads);
					ExoEventButton = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_event_buttons);
					BtnSkipIntro = ControlView.FindViewById<TextView>(Resource.Id.exo_skipIntro);

					BtnBackward = ControlView.FindViewById<FrameLayout>(Resource.Id.backward);
					BtnForward = ControlView.FindViewById<FrameLayout>(Resource.Id.forward);

					BtnPrev = ControlView.FindViewById<ImageView>(Resource.Id.image_prev);
					BtnNext = ControlView.FindViewById<ImageButton>(Resource.Id.image_next);

					ProgressTimeBar = ControlView.FindViewById<DefaultTimeBar>(Resource.Id.exo_progress);

					BtnSkipIntro.Visibility = ViewStates.Gone;
					ExoTopAds.Visibility = ViewStates.Gone;

					MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
					MFullScreenButton = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_fullscreen_button);

					if (!showFullScreen)
					{
						MFullScreenIcon.Visibility = ViewStates.Gone;
						MFullScreenButton.Visibility = ViewStates.Gone;
					}
					else
					{
						MFullScreenIcon.Visibility = ViewStates.Visible;
						MFullScreenButton.Visibility = ViewStates.Visible;

						var instance = VideoDataWithEventsLoader.GetInstance();
						if (instance != null)
						{
							if (isFullScreen)
							{
								DownloadIcon.Visibility = ViewStates.Gone;
								ExoBackButton.Click += (sender, args) => { FullScreenVideoActivity.Instance?.BackPressed(); };
							}
							else
							{
								ExoBackButton.Click += instance.ExoBackButtonOnClick;
							}

							DownloadIcon.Click += instance.DownloadIconOnClick;
							ShareIcon.Click += instance.ShareIconOnClick;
						}

						MenuButton.Click += MenuButtonOnClick;
						BtnSkipIntro.Click += BtnSkipIntroOnClick;
						ExoTopAds.Click += ExoTopAdsOnClick;

						BtnBackward.Click += BtnBackwardOnClick;
						BtnForward.Click += BtnForwardOnClick;

						BtnPrev.Click += BtnPrevOnClick;
						BtnNext.Click += BtnNextOnClick;

						MFullScreenButton.Click += MFullScreenButtonOnClick;

						if (isFullScreen)
						{
							MFullScreenButton.Tag = "true";
							MFullScreenIcon.SetImageResource(Resource.Drawable.icon_fullscreen_close);
						}
						else
						{
							MFullScreenButton.Tag = "false";
							MFullScreenIcon.SetImageResource(Resource.Drawable.icon_fullscreen_open);
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
		{
			try
			{
				var extension = uri?.Path?.Split('.').LastOrDefault();
				var mime = MimeTypeMap.GetMimeType(extension);
				var mediaItem = new MediaItem.Builder()?.SetUri(uri)?.SetMediaId(tag)?.SetMimeType(mime)?.Build();

				IMediaSource src;
				if (!string.IsNullOrEmpty(uri.Path) && (uri.Path.Contains("file://") || uri.Path.Contains("content://") || uri.Path.Contains("storage") || uri.Path.Contains("/data/user/0/")))
				{
					DataSourceFactory = new FileDataSource.Factory();
					DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, DataSourceFactory);
					src = new ProgressiveMediaSource.Factory(upstreamFactory).CreateMediaSource(mediaItem);
				}
				else
				{
					DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, GetHttpDataSourceFactory());
					DataSourceFactory = BuildReadOnlyCacheDataSource(upstreamFactory, PreCachingExoPlayerVideo.GetCache());

					int contentType = Util.InferContentTypeForUriAndMimeType(uri, extension);
					switch (contentType)
					{
						case C.ContentTypeSs:
							src = new SsMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
							break;
						case C.ContentTypeDash:
							src = new DashMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
							break;
						case C.ContentTypeRtsp:
							src = new RtspMediaSource.Factory().CreateMediaSource(mediaItem);
							break;
						case C.ContentTypeHls:
							DefaultHlsExtractorFactory defaultHlsExtractorFactory = new DefaultHlsExtractorFactory(DefaultTsPayloadReaderFactory.FlagAllowNonIdrKeyframes, true);
							src = new HlsMediaSource.Factory(DataSourceFactory).SetExtractorFactory(defaultHlsExtractorFactory)?.CreateMediaSource(mediaItem);
							break;
						default:
							src = new ProgressiveMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
							break;
					}
				}

				return src;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		private IDataSource.IFactory GetHttpDataSourceFactory()
		{
			if (HttpDataSourceFactory == null)
			{
				CookieManager cookieManager = new CookieManager();
				cookieManager.SetCookiePolicy(ICookiePolicy.AcceptOriginalServer);
				CookieHandler.Default = cookieManager;
				HttpDataSourceFactory = new DefaultHttpDataSource.Factory();
			}

			return HttpDataSourceFactory;
		}

		private CacheDataSource.Factory BuildReadOnlyCacheDataSource(IDataSource.IFactory upstreamFactory, ICache cache)
		{
			return new CacheDataSource.Factory()?.SetCache(cache)?.SetUpstreamDataSourceFactory(upstreamFactory)?.SetCacheWriteDataSinkFactory(null)?.SetFlags(CacheDataSource.FlagIgnoreCacheOnError);
		}

		public void FirstPlayVideo(Uri uri, VideoDataObject videoData, bool showAds = true)
		{
			try
			{
				VideoUrl = uri;

				bool canPrev = ListUtils.LessonList.Count > 0;
				BtnPrev.Enabled = canPrev;
				BtnPrev.Alpha = canPrev ? 1f : 0.3f;

				bool canNext = ListUtils.ArrayListPlay.Count > 0;
				BtnNext.Enabled = canNext;
				BtnNext.Alpha = canNext ? 1f : 0.3f;

				bool vidMonit = /*ListUtils.MySettingsList?.UsrVMon == "on" &&*/ videoData.Monetization == "1" && videoData.Owner?.OwnerClass?.VideoMon == "1";

				var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
				if (!AppSettings.AllowOfflineDownload || AppSettings.AllowDownloadProUser && isPro == "0")
					DownloadIcon.Visibility = ViewStates.Gone;

				var videoSource = GetMediaSourceFromUrl(uri, "normal");

				if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
				{
					PreCachingExoPlayerVideo.CacheVideosFiles(uri);
					videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
					if (videoSource?.MediaItem != null)
						videoSource.MediaItem.MediaId = "normal";
				}

				if (showAds)
					RunVideoWithAds(videoSource, vidMonit);
				else
				{
					VideoPlayer.SetMediaSource(videoSource);
					VideoPlayer.Prepare();
					VideoPlayer.PlayWhenReady = true;
					VideoPlayer.SeekTo(0);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void FirstPlayVideo(Uri uri, long videoDuration)
		{
			try
			{
				VideoUrl = uri;
				var videoSource = GetMediaSourceFromUrl(uri, "normal");

				if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
				{
					PreCachingExoPlayerVideo.CacheVideosFiles(uri);
					videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
				}

				VideoPlayer.SetMediaSource(videoSource);
				VideoPlayer.Prepare();
				VideoPlayer.PlayWhenReady = true;
				VideoPlayer.SeekTo(videoDuration);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void RunVideoWithAds(IMediaSource videoSource, bool showAds)
		{
			try
			{
				bool runAds = false;
				var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
				if (isPro == "0" && ListUtils.AdsVideoList.Count > 0 && Methods.CheckConnectivity() && showAds)
				{
					Random rand = new Random();

					var playPos = rand.Next(ListUtils.AdsVideoList.Count - 1 + 1);
					DataAdsVideo = ListUtils.AdsVideoList[playPos];

					var urlAds = "";
					if (!string.IsNullOrEmpty(DataAdsVideo?.Media))
					{
						urlAds = DataAdsVideo.Media;
					}
					else if (!string.IsNullOrEmpty(DataAdsVideo?.AdMedia))
					{
						urlAds = DataAdsVideo.AdMedia;
					}

					var type = Methods.AttachmentFiles.Check_FileExtension(urlAds);
					if (type == "Video" && DataAdsVideo != null)
					{
						//AppSettings.ShowButtonSkip = DataAdsVideo
						var adVideoSource = GetMediaSourceFromUrl(Uri.Parse(urlAds), "Ads");
						if (adVideoSource != null)
						{
							ListUtils.AdsVideoList.Remove(DataAdsVideo);

							// Plays the first video, then the second video.
							//var concatenatedSource = new ConcatenatingMediaSource(adVideoSource, videoSource);

							VideoPlayer.SetMediaSources(new List<IMediaSource>() { adVideoSource, videoSource });

							ExoTopLayout.Visibility = ViewStates.Gone;
							ExoEventButton.Visibility = ViewStates.Invisible;
							BtnSkipIntro.Visibility = ViewStates.Visible;
							ExoTopAds.Visibility = ViewStates.Visible;

							if (DataAdsVideo.SkipSeconds?.ToString() != "0")
							{
								BtnSkipIntro.Text = DataAdsVideo.SkipSeconds?.ToString();
								// CountShow = DataAdsVideo.SkipSeconds.Value;
								CountShow = AppSettings.ShowButtonSkip;
							}
							else
							{
								BtnSkipIntro.Text = AppSettings.ShowButtonSkip.ToString();
								CountShow = AppSettings.ShowButtonSkip;
							}
							BtnSkipIntro.Enabled = false;
							runAds = true;
						}
						else
						{
							VideoPlayer.SetMediaSource(videoSource);

							ExoTopLayout.Visibility = ViewStates.Visible;
							ExoEventButton.Visibility = ViewStates.Visible;
							BtnSkipIntro.Visibility = ViewStates.Gone;
							ExoTopAds.Visibility = ViewStates.Gone;
						}
					}
					else
					{
						VideoPlayer.SetMediaSource(videoSource);

						ExoTopLayout.Visibility = ViewStates.Visible;
						ExoEventButton.Visibility = ViewStates.Visible;
						BtnSkipIntro.Visibility = ViewStates.Gone;
						ExoTopAds.Visibility = ViewStates.Gone;
					}
				}
				else
				{
					VideoPlayer.SetMediaSource(videoSource);

					ExoTopLayout.Visibility = ViewStates.Visible;
					ExoEventButton.Visibility = ViewStates.Visible;
					BtnSkipIntro.Visibility = ViewStates.Gone;
					ExoTopAds.Visibility = ViewStates.Gone;
				}

				VideoPlayer.Prepare();
				VideoPlayer.PlayWhenReady = true;
				VideoPlayer.SeekTo(0);

				if (runAds)
					RunTimer();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void RunTimer()
		{
			try
			{
				TimerAds = new Timer { Interval = 1000 };
				TimerAds.Elapsed += TimerAdsOnElapsed;
				TimerAds.Start();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private long CountShow = AppSettings.ShowButtonSkip;
		private void TimerAdsOnElapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				ActivityContext?.RunOnUiThread(() =>
				{
					try
					{
						switch (CountShow)
						{
							case 0:
								SetTextSkipIntro();

								BtnSkipIntro.Enabled = true;

								if (TimerAds != null)
								{
									TimerAds.Enabled = false;
									TimerAds.Stop();
								}

								TimerAds = null!;
								break;
							case > 0:
								BtnSkipIntro.Text = CountShow.ToString();
								CountShow--;
								break;
							default:
								SetTextSkipIntro();
								BtnSkipIntro.Enabled = true;

								if (TimerAds != null)
								{
									TimerAds.Enabled = false;
									TimerAds.Stop();
								}

								TimerAds = null!;
								break;
						}
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				});
			}
			catch (Exception ex)
			{
				Methods.DisplayReportResultTrack(ex);
			}
		}

		private void SetTextSkipIntro()
		{
			try
			{
				Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");

				BtnSkipIntro.Gravity = GravityFlags.CenterHorizontal;
				BtnSkipIntro.SetTypeface(font, TypefaceStyle.Normal);
				var woTextDecorator = new TextDecorator
				{
					Content = ActivityContext.GetText(Resource.String.Lbl_SkipAds) + " " + IonIconsFonts.IosArrowForward,
					DecoratedContent = new SpannableString(ActivityContext.GetText(Resource.String.Lbl_SkipAds) + " " + IonIconsFonts.IosArrowForward)
				};
				woTextDecorator.SetTextColor(IonIconsFonts.ArrowForward, "#ffffff");
				woTextDecorator.Build(BtnSkipIntro, woTextDecorator.DecoratedContent);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void PlayVideo()
		{
			try
			{
				if (PlayerView?.Player != null && PlayerView.Player.PlaybackState == IPlayer.StateReady && !PlayerView.Player.PlayWhenReady)
					PlayerView.Player.PlayWhenReady = true;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void StopVideo()
		{
			try
			{
				if (PlayerView?.Player != null && PlayerView.Player.PlayWhenReady)
					PlayerView.Player.PlayWhenReady = false;

				//if (FullScreenPlayerView?.Player != null && FullScreenPlayerView.Player.PlayWhenReady)
				//    FullScreenPlayerView.Player.PlayWhenReady = false;
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
				StopVideo();
				PlayerView?.Player?.Stop();

				if (VideoPlayer != null)
				{
					VideoPlayer.Release();
					VideoPlayer = null;
				}

				FullScreenPlayerView?.Player?.Stop();
				if (FullScreenVideoPlayer != null)
				{
					FullScreenVideoPlayer.Release();
					FullScreenVideoPlayer = null;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public PlayerControlView GetControlView()
		{
			return ControlView;
		}

		public PlayerView GetPlayerView()
		{
			return PlayerView;
		}

		public IExoPlayer GetExoPlayer()
		{
			return VideoPlayer;
		}

		public void ChangePlaybackSpeed(PlaybackParameters playbackParameters)
		{
			try
			{
				if (PlayerView.Player != null)
				{
					PlayerView.Player.PlaybackParameters = playbackParameters;
				}

				if (FullScreenPlayerView.Player != null)
				{
					FullScreenPlayerView.Player.PlaybackParameters = playbackParameters;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#region Event Control View


		private void BtnNextOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ListUtils.ArrayListPlay.Count > 0)
				{
					var data = ListUtils.ArrayListPlay.FirstOrDefault();
					if (data != null)
					{
						ListUtils.LessonList.Add(data);
						TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void BtnPrevOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ListUtils.LessonList.Count > 0)
				{
					var data = ListUtils.LessonList.LastOrDefault();
					if (data != null)
					{
						TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
						ListUtils.LessonList.Remove(data);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void BtnForwardOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ForwardPressed)
				{
					PressedHandler.RemoveCallbacks(() => { ForwardPressed = false; });
					ForwardPressed = false;

					//Add event
					var fTime = 10000; // 10 Sec
					if (PlayerView?.Player != null)
					{
						var eTime = PlayerView.Player.Duration;
						var sTime = PlayerView.Player.CurrentPosition;
						if ((sTime + fTime) <= eTime)
						{
							sTime += fTime;
							PlayerView.Player.SeekTo(sTime);

							if (!PlayerView.Player.PlayWhenReady)
								PlayerView.Player.PlayWhenReady = true;
						}
						else
						{
							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_ErrorForward), ToastLength.Short)?.Show();
						}
					}
				}
				else
				{
					ForwardPressed = true;
					PressedHandler.PostDelayed(() => { ForwardPressed = false; }, 2000L);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
		private bool BackwardPressed, ForwardPressed;
		private readonly Handler PressedHandler = new Handler(Looper.MainLooper);
		private void BtnBackwardOnClick(object sender, EventArgs e)
		{
			try
			{
				if (BackwardPressed)
				{
					PressedHandler.RemoveCallbacks(() => { BackwardPressed = false; });
					BackwardPressed = false;

					//Add event
					var bTime = 10000; // 10 Sec
					if (PlayerView.Player != null)
					{
						var sTime = PlayerView.Player.CurrentPosition;

						if ((sTime - bTime) > 0)
						{
							sTime -= bTime;
							PlayerView.Player.SeekTo(sTime);

							if (!PlayerView.Player.PlayWhenReady)
								PlayerView.Player.PlayWhenReady = true;
						}
						else
						{
							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_ErrorBackward), ToastLength.Short)?.Show();
						}
					}
				}
				else
				{
					BackwardPressed = true;
					PressedHandler.PostDelayed(() => { BackwardPressed = false; }, 2000L);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ExoTopAdsOnClick(object sender, EventArgs e)
		{
			try
			{
				if (DataAdsVideo != null)
				{
					string url = DataAdsVideo.Url;
					new IntentController(ActivityContext).OpenBrowserFromApp(url);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var activity = (AppCompatActivity)ActivityContext;
				var dialogFragment = new MoreMenuVideoDialogFragment();
				dialogFragment.Show(activity.SupportFragmentManager, dialogFragment.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void BtnSkipIntroOnClick(object sender, EventArgs e)
		{
			try
			{
				if (PlayerView?.Player != null)
				{
					PlayerView.Player.Next();

					ExoTopLayout.Visibility = ViewStates.Visible;
					ExoEventButton.Visibility = ViewStates.Visible;
					BtnSkipIntro.Visibility = ViewStates.Gone;
					ExoTopAds.Visibility = ViewStates.Gone;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion

		public void HideControls(bool isInPictureInPictureMode)
		{
			try
			{
                ControlView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;

				ExoTopLayout.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				ExoBackButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				DownloadIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				MFullScreenIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				MFullScreenButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				ShareIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
				MenuButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void ToggleExoPlayerKeepScreenOnFeature(bool keepScreenOn)
		{
			try
			{
				if (PlayerView != null)
				{
					PlayerView.KeepScreenOn = keepScreenOn;
				}

				if (FullScreenPlayerView != null)
				{
					FullScreenPlayerView.KeepScreenOn = keepScreenOn;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void Repeat()
		{
			try
			{
				if (VideoPlayer != null)
				{
					VideoPlayer.SeekTo(0, 0);
					VideoPlayer.PlayWhenReady = true;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#region FullScreen

		private void MFullScreenButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (MFullScreenButton?.Tag?.ToString() == "false")
				{
					VideoDataWithEventsLoader.GetInstance()?.UpdateMainRootDefaultLandscapeSize();
				}
				else if (MFullScreenButton?.Tag?.ToString() == "true")
				{
					VideoDataWithEventsLoader.GetInstance()?.UpdateMainRootDefaultSize();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void SetFullScreenPlayerView(PlayerView playerView)
		{
			try
			{
				FullScreenPlayerView = playerView;

				ControlView = FullScreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

				FullScreenVideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(new DefaultTrackSelector(ActivityContext))?.Build();
				var playerListener = new PlayerEvents(ActivityContext, ControlView, TypePage);
				FullScreenVideoPlayer?.AddListener(playerListener);

				FullScreenPlayerView.UseController = true;
				FullScreenPlayerView.Player = VideoPlayer;
				FullScreenPlayerView.Player.PlayWhenReady = true;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void PlayFullScreen()
		{
			try
			{
				if (FullScreenPlayerView != null)
				{
					FullScreenPlayerView.Player = VideoPlayer;
					if (FullScreenPlayerView.Player != null) FullScreenPlayerView.Player.PlayWhenReady = true;

					MFullScreenButton.Tag = "true";
					MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.icon_fullscreen_close));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void RestartPlayAfterShrinkScreen()
		{
			try
			{
				PlayerView.Player = null!;
				if (FullScreenPlayerView != null)
				{
					PlayerView.Player = FullScreenPlayerView.Player;
					PlayerView.Player.PlayWhenReady = true;
					PlayerView.RequestFocus();
					PlayerView.Visibility = ViewStates.Visible;

					MFullScreenButton.Tag = "false";
					MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.icon_fullscreen_open));

					FullScreenPlayerView.Player = null;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

	}
}