using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using Anjo.Android.YouTubePlayerX.Player;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Utils;
using PlayTube.MediaPlayers;
using System;

namespace PlayTube.Activities.Videos
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class YouTubePlayerFullScreenActivity : AppCompatActivity, IYouTubePlayerInitListener, IYouTubePlayerFullScreenListener
	{
		public static YouTubePlayerFullScreenActivity Instance;

		private YouTubePlayerView YouTubePlayerView;
		private IYouTubePlayer YouTubePlayer;
		private YouTubePlayerEvents PlayerEvents;

		private string VideoIdYoutube;
		private int CurrentSecond;

		private VideoDataWithEventsLoader VideoPlayerController;

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this, true);

				var type = Intent?.GetStringExtra("type") ?? "";
				if (type == "RequestedOrientation")
				{
					//ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
					//ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
					RequestedOrientation = ScreenOrientation.Landscape;
				}

				SetContentView(Resource.Layout.FullScreenYouTubePlayerLayout);

				Instance = this;
				VideoIdYoutube = Intent?.GetStringExtra("VideoIdYoutube") ?? "";
				CurrentSecond = Intent?.GetIntExtra("CurrentSecond", 0) ?? 0;

				InitYouTubePlayerView();
				InitBackPressed();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
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

		protected override void OnDestroy()
		{
			try
			{
				Instance = null;
				YouTubePlayerView?.Release();

				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Menu

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Intent intent = new Intent();
					SetResult(Result.Ok, intent);
					Finish();
					return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		public void BackPressed()
		{
			try
			{
				Intent intent = new Intent();
				SetResult(Result.Ok, intent);
				Finish();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		private void InitYouTubePlayerView()
		{
			try
			{
				VideoPlayerController = VideoDataWithEventsLoader.GetInstance();

				YouTubePlayerView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);

				// The player will automatically release itself when the activity is destroyed.
				// The player will automatically pause when the activity is paused
				// If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
				Lifecycle.AddObserver(YouTubePlayerView);

				YouTubePlayerView.PlayerUiController.ShowMenuButton(false);
				YouTubePlayerView.Initialize(this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void InitBackPressed()
		{
			try
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
				{
					OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "YouTubePlayerFullScreenActivity"));
				}
				else
				{
					OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "YouTubePlayerFullScreenActivity", true));
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnInitSuccess(IYouTubePlayer youTubePlayer)
		{
			try
			{
				YouTubePlayer = youTubePlayer;
				PlayerEvents = new YouTubePlayerEvents(VideoPlayerController, YouTubePlayer, VideoIdYoutube, "FullScreen", CurrentSecond);
				YouTubePlayer.AddListener(PlayerEvents);
				YouTubePlayerView.AddFullScreenListener(this);
				YouTubePlayerView.EnterFullScreen();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			try
			{
				YouTubePlayerView.PlayerUiController.Menu.Dismiss();
				if (newConfig.Orientation == Orientation.Landscape)
				{
				}
				else if (newConfig.Orientation == Orientation.Portrait)
				{
					OnYouTubePlayerExitFullScreen();
				}
				base.OnConfigurationChanged(newConfig);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnYouTubePlayerEnterFullScreen()
		{

		}

		public void OnYouTubePlayerExitFullScreen()
		{
			try
			{
				if (VideoPlayerController != null)
				{
					VideoPlayerController.YouTubePlayerEvents.CurrentSecond = PlayerEvents.CurrentSecond;
					VideoPlayerController.YouTubePlayerEvents.IsPlaying = PlayerEvents.IsPlaying;
				}

				Intent intent = new Intent();
				SetResult(Result.Ok, intent);
				Finish();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}