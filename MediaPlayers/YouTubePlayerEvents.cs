using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Lifecycle;
using Anjo.Android.YouTubePlayerX.Player;
using PlayTube.Activities.Models;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;
using System.Linq;

namespace PlayTube.MediaPlayers
{
	public class YouTubePlayerEvents : IYouTubePlayerListener
	{
		private readonly VideoDataWithEventsLoader VideoPlayerController;
		private readonly IYouTubePlayer PlayerView;
		private readonly string VideoIdYoutube;
		public bool IsPlaying;
		public int CurrentSecond, Duration;
		public readonly string Type;

		public YouTubePlayerEvents(VideoDataWithEventsLoader VideoDataWithEventsLoader, IYouTubePlayer playerView, string videoId, string type = "Normal", int currentSecond = 0)
		{
			VideoPlayerController = VideoDataWithEventsLoader;
			PlayerView = playerView;
			VideoIdYoutube = videoId;
			Type = type;
			CurrentSecond = currentSecond;
		}

		public void OnReady()
		{
			try
			{
				if (Type == "FullScreen")
				{
					if (YouTubePlayerFullScreenActivity.Instance.Lifecycle.CurrentState == Lifecycle.State.Resumed)
						PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
					else
						PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnStateChange(int state)
		{
			try
			{
				if (state == PlayerConstants.PlayerState.Ended)
				{
					if (Type != "FullScreen")
						OnVideoEnded();

					IsPlaying = false;
				}
				else if (state == PlayerConstants.PlayerState.Paused)
				{
					IsPlaying = false;
				}
				else if (state == PlayerConstants.PlayerState.Playing)
				{
					IsPlaying = true;
				}

				var mainActivity = TabbedMainActivity.GetInstance();
				if (mainActivity?.VideoDataWithEventsLoader != null)
				{
					mainActivity.VideoDataWithEventsLoader.ProgressBar.Visibility = ViewStates.Invisible;
				}

				var globalPlayerActivity = GlobalPlayerActivity.GetInstance();
				if (globalPlayerActivity?.VideoDataWithEventsLoader != null)
				{
					globalPlayerActivity.VideoDataWithEventsLoader.ProgressBar.Visibility = ViewStates.Invisible;
				}

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnPlaybackQualityChange(string playbackQuality)
		{

		}

		public void OnPlaybackRateChange(string playbackRate)
		{

		}

		public void OnError(int error)
		{
			try
			{
				IsPlaying = false;
				//Toast.MakeText(Application.Context, "Sorry, this video cannot be played, Error code: " + error, ToastLength.Short)?.Show();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnApiChange()
		{

		}

		public void OnCurrentSecond(int second)
		{
			CurrentSecond = second;
		}

		public void OnVideoDuration(int duration)
		{
			Duration = duration;
		}

		public void OnVideoLoadedFraction(int loadedFraction)
		{

		}

		public void OnVideoId(string videoId)
		{

		}

		public void OnVideoEnded()
		{
			try
			{
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

		public void BtnForwardOnClick(string page)
		{
			try
			{
				if (page == "TabbedMainActivity")
				{
					var mainActivity = TabbedMainActivity.GetInstance();
					if (mainActivity == null)
						return;

					if (ForwardPressed)
					{
						PressedHandler.RemoveCallbacks(() => { ForwardPressed = false; });
						ForwardPressed = false;

						//Add event
						var fTime = 10; // 10 Sec
						if (mainActivity.VideoDataWithEventsLoader?.YoutubePlayer != null)
						{
							int eTime = (int)Duration;
							int sTime = (int)CurrentSecond;
							if (sTime + fTime <= eTime)
							{
								sTime += fTime;
								mainActivity.VideoDataWithEventsLoader?.YoutubePlayer.SeekTo(sTime);

								if (mainActivity.VideoDataWithEventsLoader?.YouTubePlayerEvents != null && !mainActivity.VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
									mainActivity.VideoDataWithEventsLoader?.YoutubePlayer.Play();
							}
							else
							{
								Toast.MakeText(mainActivity, mainActivity.GetText(Resource.String.Lbl_ErrorForward), ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						ForwardPressed = true;
						PressedHandler.PostDelayed(() => { ForwardPressed = false; }, 2000L);
					}
				}
				else if (page == "GlobalPlayerActivity")
				{
					var globalPlayerActivity = GlobalPlayerActivity.GetInstance();
					if (globalPlayerActivity == null)
						return;

					if (ForwardPressed)
					{
						PressedHandler.RemoveCallbacks(() => { ForwardPressed = false; });
						ForwardPressed = false;

						//Add event
						var fTime = 10; // 10 Sec
						if (globalPlayerActivity.VideoDataWithEventsLoader?.YoutubePlayer != null)
						{
							int eTime = (int)Duration;
							int sTime = (int)CurrentSecond;
							if (sTime + fTime <= eTime)
							{
								sTime += fTime;
								globalPlayerActivity.VideoDataWithEventsLoader.YoutubePlayer.SeekTo(sTime);

								if (!globalPlayerActivity.VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
									globalPlayerActivity.VideoDataWithEventsLoader.YoutubePlayer.Play();
							}
							else
							{
								Toast.MakeText(globalPlayerActivity, globalPlayerActivity.GetText(Resource.String.Lbl_ErrorForward), ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						ForwardPressed = true;
						PressedHandler.PostDelayed(() => { ForwardPressed = false; }, 2000L);
					}
				}

			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private bool BackwardPressed, ForwardPressed;
		private readonly Handler PressedHandler = new Handler(Looper.MainLooper);

		public void BtnBackwardOnClick(string page)
		{
			try
			{
				if (page == "TabbedMainActivity")
				{
					var mainActivity = TabbedMainActivity.GetInstance();
					if (mainActivity == null)
						return;

					if (BackwardPressed)
					{
						PressedHandler.RemoveCallbacks(() => { BackwardPressed = false; });
						BackwardPressed = false;

						//Add event
						var bTime = 10; // 10 Sec
						if (mainActivity.VideoDataWithEventsLoader.YoutubePlayer != null)
						{
							var sTime = CurrentSecond;

							if (sTime - bTime > 0)
							{
								sTime -= bTime;
								mainActivity.VideoDataWithEventsLoader.YoutubePlayer.SeekTo(sTime);

								if (!mainActivity.VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
									mainActivity.VideoDataWithEventsLoader.YoutubePlayer.Play();
							}
							else
							{
								Toast.MakeText(mainActivity, mainActivity.GetText(Resource.String.Lbl_ErrorBackward), ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						BackwardPressed = true;
						PressedHandler.PostDelayed(() => { BackwardPressed = false; }, 2000L);
					}
				}
				else if (page == "GlobalPlayerActivity")
				{
					var globalPlayerActivity = GlobalPlayerActivity.GetInstance();
					if (globalPlayerActivity == null)
						return;

					if (BackwardPressed)
					{
						PressedHandler.RemoveCallbacks(() => { BackwardPressed = false; });
						BackwardPressed = false;

						//Add event
						var bTime = 10; // 10 Sec
						if (globalPlayerActivity.VideoDataWithEventsLoader.YoutubePlayer != null)
						{
							var sTime = CurrentSecond;

							if (sTime - bTime > 0)
							{
								sTime -= bTime;
								globalPlayerActivity.VideoDataWithEventsLoader.YoutubePlayer.SeekTo(sTime);

								if (!globalPlayerActivity.VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
									globalPlayerActivity.VideoDataWithEventsLoader.YoutubePlayer.Play();
							}
							else
							{
								Toast.MakeText(globalPlayerActivity, globalPlayerActivity.GetText(Resource.String.Lbl_ErrorBackward), ToastLength.Short)?.Show();
							}
						}
					}
					else
					{
						BackwardPressed = true;
						PressedHandler.PostDelayed(() => { BackwardPressed = false; }, 2000L);
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void BtnPreviousOnClick()
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


		public void BtnNextOnClick()
		{
			try
			{
				OnVideoEnded();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}