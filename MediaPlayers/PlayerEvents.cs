using Android.App;
using Android.Views;
using Android.Widget;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Text;
using Androidx.Media3.UI;
using PlayTube.Activities.Models;
using PlayTube.Activities.Shorts;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Linq;
using Android.Content.Res;
using Android.Util;
using Androidx.Media3.Datasource;
using Java.Lang;
using Exception = System.Exception;
using Object = Java.Lang.Object;  

namespace PlayTube.MediaPlayers
{
	public class PlayerEvents : Object, IPlayer.IListener, PlayerView.IControllerVisibilityListener, PlayerControlView.IProgressUpdateListener
	{
		private readonly Activity ActContext;
		private readonly ProgressBar LoadingProgressBar;
		private readonly ImageButton VideoPlayButton;
		private readonly VideoDataWithEventsLoader VideoPlayerController;
		private readonly string TypePage;

		public PlayerEvents(Activity act, PlayerControlView controlView, string typePage)
		{
			try
			{
				ActContext = act;
				TypePage = typePage;
				VideoPlayerController = VideoDataWithEventsLoader.GetInstance();

				if (controlView != null)
				{
					VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play_pause);
					LoadingProgressBar = act.FindViewById<ProgressBar>(Resource.Id.progress_bar);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnAudioAttributesChanged(AudioAttributes audioAttributes)
		{

		}

		public void OnAudioSessionIdChanged(int audioSessionId)
		{

		}

		public void OnAvailableCommandsChanged(IPlayer.Commands availableCommands)
		{

		}

		public void OnCues(CueGroup cueGroup)
		{

		}

		public void OnDeviceInfoChanged(DeviceInfo deviceInfo)
		{

		}

		public void OnDeviceVolumeChanged(int volume, bool muted)
		{

		}

		public void OnEvents(IPlayer player, IPlayer.Events events)
		{

		}

		public void OnIsLoadingChanged(bool isLoading)
		{

		}

		public void OnIsPlayingChanged(bool isPlaying)
		{

		}

		public void OnLoadingChanged(bool p0)
		{

		}

		public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
		{

		}

		public void OnMediaItemTransition(MediaItem mediaItem, int reason)
		{

		}

		public void OnMediaMetadataChanged(MediaMetadata mediaMetadata)
		{

		}

		public void OnMetadata(Metadata metadata)
		{

		}

		public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
		{

		}

		public void OnPlaybackParametersChanged(PlaybackParameters p0)
		{

		}

		public void OnPlaybackStateChanged(int playbackState)
		{

		}

		public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
		{

		}

		public void OnPlayerError(PlaybackException error)
		{
            try
            {
                Throwable cause = error?.Cause;
                if (cause is IHttpDataSource.HttpDataSourceException httpError)
                {
                    // An HTTP error occurred.
                    //There is an error loading this video.
                    Console.WriteLine("Error code " + error.ErrorCode);

                    Toast.MakeText(ActContext, ActContext.GetText(Resource.String.Lbl_ErrorLoadVideo) + ", " + ActContext.GetText(Resource.String.Lbl_Error_Code) + " " + error.ErrorCode, ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

		public void OnPlayerErrorChanged(PlaybackException error)
		{

		}

		public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
		{
			try
			{
				if (playbackState == IPlayer.StateEnded || playbackState == IPlayer.StateIdle && !playWhenReady)
				{
					TabbedMainActivity.GetInstance().SetOffWakeLock();
					VideoPlayerController?.ExoController.ToggleExoPlayerKeepScreenOnFeature(false);

					if (playbackState == IPlayer.StateEnded)
					{
						if (TypePage == "ShortsVideo")
						{
							ViewShortsVideoFragment.GetInstance()?.ExoController?.Repeat();
						}
						else
						{
							OnVideoEnded();
						}
					}
				}
				else
				{ // STATE_IDLE, STATE_ENDED
				  // This prevents the screen from getting dim/lock
					TabbedMainActivity.GetInstance().SetOnWakeLock();
					VideoPlayerController?.ExoController.ToggleExoPlayerKeepScreenOnFeature(true);
				}

				if (VideoPlayButton == null || LoadingProgressBar == null)
					return;

				switch (playbackState)
				{
					case IPlayer.StateEnded:
						{
							if (VideoPlayerController.ShowRestrictedVideo)
							{
								var videoData = VideoPlayerController.VideoData;
								if (!string.IsNullOrEmpty(videoData.SellVideo) && videoData.SellVideo != "0")
									VideoPlayerController.ShowRestrictedVideoFragment(null, ActContext, "purchaseVideo");
								else if (!string.IsNullOrEmpty(videoData.RentPrice) && videoData.RentPrice != "0" && AppSettings.RentVideosSystem)
									VideoPlayerController.ShowRestrictedVideoFragment(null, ActContext, "RentVideo");

								VideoPlayerController.ShowRestrictedVideo = false;
							}
							else
							{
								switch (playWhenReady)
								{
									case false:
										VideoPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
										break;
									default:
										VideoPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
										break;
								}
								VideoPlayButton.Visibility = ViewStates.Visible;
							}

							break;
						}
					case IPlayer.StateReady:
						{
							//Allen On Next Update

							//if (VideoController.Player.VideoFormat.Height > 650)
							//    VideoController.Player.VideoFormat.Height = (int)(VideoController.Player.VideoFormat.Height / 1.8);

							//float videoRatio = (float)VideoController.Player.VideoFormat.Width / VideoController.Player.VideoFormat.Height ;

							//var display = ActContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay;
							//var size = new Point();
							//display.GetSize(size);

							//float displayRatio = (float)size.X / size.Y;

							//if (videoRatio > displayRatio)
							//{
							//    VideoPlayerController.MainVideoFrameLayout.LayoutParameters.Height = (int)Math.Round(VideoPlayerController.MainVideoFrameLayout.MeasuredWidth / videoRatio);
							//    VideoPlayerController.MainVideoFrameLayout.RequestLayout();
							//}
							//else
							//{
							//    LinearLayout.LayoutParams Params = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 220);
							//    Params.Gravity = GravityFlags.Center;
							//    VideoPlayerController.MainVideoFrameLayout.LayoutParameters = Params;
							//}


							switch (playWhenReady)
							{
								case false:
									VideoPlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
									break;
								default:
									VideoPlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
									break;
							}
							VideoPlayButton.Visibility = ViewStates.Visible;
							LoadingProgressBar.Visibility = ViewStates.Invisible;


							bool canNext = ListUtils.ArrayListPlay.Count > 0;
							VideoPlayerController.ExoController.BtnNext.Enabled = canNext;
							VideoPlayerController.ExoController.BtnNext.Alpha = canNext ? 1f : 0.3f;

							break;
						}
					case IPlayer.StateBuffering:
						VideoPlayButton.Visibility = ViewStates.Invisible;
						break;
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
		{

		}

		public void OnPositionDiscontinuity(int p0)
		{
			try
			{
				// Load metadata for mediaId and update the UI. 
				var tag = VideoPlayerController?.PlayerView?.Player?.CurrentMediaItem?.MediaId ?? "";
				if (tag == "normal")
				{
					//Methods.DisplayReportResult(ActContext, "OnTimelineChanged >> normal");
					VideoPlayerController.ExoController.ExoTopLayout.Visibility = ViewStates.Visible;
					VideoPlayerController.ExoController.ExoEventButton.Visibility = ViewStates.Visible;
					VideoPlayerController.ExoController.BtnSkipIntro.Visibility = ViewStates.Gone;
					VideoPlayerController.ExoController.ExoTopAds.Visibility = ViewStates.Gone;

					VideoPlayerController.ExoController.ProgressTimeBar.Visibility = ViewStates.Visible;
				}
				else if (tag == "Ads")
				{
					VideoPlayerController.ExoController.ProgressTimeBar.Visibility = ViewStates.Gone;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnRenderedFirstFrame()
		{

		}

		public void OnRepeatModeChanged(int p0)
		{

		}

		public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
		{

		}

		public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
		{

		}

		public void OnShuffleModeEnabledChanged(bool p0)
		{

		}

		public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
		{

		}

		public void OnSurfaceSizeChanged(int width, int height)
		{
			
		}
        
        public void OnTimelineChanged(Timeline timeline, int reason)
		{
			try
			{
				var tag = VideoPlayerController?.PlayerView?.Player?.CurrentMediaItem?.MediaId ?? "";
				if (reason == 0 || tag == "normal")
				{
					//Methods.DisplayReportResult(ActContext, "OnTimelineChanged >> normal");
					VideoPlayerController.ExoController.ExoTopLayout.Visibility = ViewStates.Visible;
					VideoPlayerController.ExoController.ExoEventButton.Visibility = ViewStates.Visible;
					VideoPlayerController.ExoController.BtnSkipIntro.Visibility = ViewStates.Gone;
					VideoPlayerController.ExoController.ExoTopAds.Visibility = ViewStates.Gone;

					VideoPlayerController.ExoController.ProgressTimeBar.Visibility = ViewStates.Visible;
				}
				else if (tag == "Ads")
				{
					VideoPlayerController.ExoController.ProgressTimeBar.Visibility = ViewStates.Gone;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnTrackSelectionParametersChanged(TrackSelectionParameters parameters)
		{

		}

		public void OnTracksChanged(Tracks tracks)
		{

		} 

        public void OnVideoSizeChanged(VideoSize videoSize)
		{
            try
            {
                if (TypePage == "ShortsVideo")
                    return;

                int videoWidth = videoSize.Width;
                int videoHeight = videoSize.Height;
                float pixelWidthHeightRatio = videoSize.PixelWidthHeightRatio;
                int unappliedRotationDegrees = videoSize.UnappliedRotationDegrees;
				 
                bool isBottomVideoViewShowing = TabbedMainActivity.GetInstance().IsBottomVideoViewShowing;
                Resources r = Application.Context.Resources;
                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 210, r.DisplayMetrics);

                if (videoSize.Height > px)
                {
                    if (isBottomVideoViewShowing == false && VideoPlayerController.VideoType == VideoEnumTypes.Normal)
                    {
                        var height = (int)(((float)videoHeight / videoWidth * px) * 1.5);
					 
                        var layoutParamsMainroot = VideoPlayerController.Mainroot.LayoutParameters;
                        layoutParamsMainroot.Height = height;
                        VideoPlayerController.Mainroot.LayoutParameters = layoutParamsMainroot;
                        VideoPlayerController.Mainroot.SetMinimumHeight(height);

                        //var layoutParamsRoot = VideoPlayerController.Root.LayoutParameters;
                        //layoutParamsRoot.Height = height;
                        //VideoPlayerController.Root.LayoutParameters = layoutParamsRoot;
                        //VideoPlayerController.Root.SetMinimumHeight(height); 
                    }
                    else
                    {
						var layoutParamsMainroot = VideoPlayerController.Mainroot.LayoutParameters;
						layoutParamsMainroot.Height = px;
						VideoPlayerController.Mainroot.LayoutParameters = layoutParamsMainroot;
						VideoPlayerController.Mainroot.SetMinimumHeight(px);
					}
                }
                else
                {
					//var layoutParamsMainroot = VideoPlayerController.Mainroot.LayoutParameters;
					//layoutParamsMainroot.Height = px;
					//VideoPlayerController.Mainroot.LayoutParameters = layoutParamsMainroot;
					//VideoPlayerController.Mainroot.SetMinimumHeight(px);
				} 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

		public void OnVolumeChanged(float volume)
		{

		}

		public void OnVideoEnded()
		{
			try
			{
				if (TypePage == "ShortsVideo")
					return;

				if (ListUtils.ArrayListPlay.Count > 0 && UserDetails.AutoNext)
				{
					var data = ListUtils.ArrayListPlay.FirstOrDefault();
					if (data != null)
					{
						ListUtils.LessonList.Add(data);
						TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void OnVisibilityChanged(int visibility)
		{

		}

		public void OnProgressUpdate(long position, long bufferedPosition)
		{

		}
	}
}