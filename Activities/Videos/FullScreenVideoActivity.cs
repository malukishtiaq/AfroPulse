using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Androidx.Media3.UI;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Utils;
using PlayTube.MediaPlayers.Exo;
using System;

namespace PlayTube.Activities.Videos
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
	public class FullScreenVideoActivity : BaseActivity
	{
		#region Variables Basic

		private ExoController ExoController;
		private PlayerView PlayerViewFullScreen;
		public VideoDataWithEventsLoader VideoDataWithEventsLoader;
		public static FullScreenVideoActivity Instance;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				//Set Full screen 
				Methods.App.FullScreenApp(this, true);

				var type = Intent?.GetStringExtra("type") ?? "";
				if (type == "RequestedOrientation")
				{
					//ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
					//ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
					//
				}
				RequestedOrientation = ScreenOrientation.Landscape;

				SetContentView(Resource.Layout.FullScreenDialogLayout);

				Instance = this;

				//Get Value And Set Toolbar
				InitComponent();
				InitBackPressed("FullScreenVideoActivity");
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
				ExoController?.PlayVideo();
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
				ExoController?.StopVideo();
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
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		protected override void OnDestroy()
		{
			try
			{
				Instance = null;
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

		private void InitComponent()
		{
			try
			{
				PlayerViewFullScreen = FindViewById<PlayerView>(Resource.Id.player_view2);

				//===================== Exo Player ======================== 

				VideoDataWithEventsLoader = VideoDataWithEventsLoader.GetInstance();
				ExoController = VideoDataWithEventsLoader.ExoController;

				ExoController?.SetFullScreenPlayerView(PlayerViewFullScreen);
				ExoController?.PlayFullScreen();
				ExoController?.SetPlayerControl(true, true);

				//var videoUrl = Intent?.GetStringExtra("videoUrl") ?? "";
				//var videoPosition = Intent?.GetStringExtra("videoPosition") ?? "0";

				//ExoController = new ExoController(this);
				//ExoController.SetPlayer(PlayerViewFullScreen);
				//ExoController.SetPlayerControl(true, true);

				//ExoController?.SetFullScreenPlayerView(PlayerViewFullScreen);

				//Uri
				//Android.Net.Uri uri = Android.Net.Uri.Parse(videoUrl);
				//ExoController?.FirstPlayVideo(uri, int.Parse(videoPosition));
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		public void BackPressed()
		{
			try
			{
				VideoDataWithEventsLoader?.InitFullscreenDialog("", "Close");
				Finish();
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
				if (newConfig.Orientation == Orientation.Landscape)
				{
				}
				else if (newConfig.Orientation == Orientation.Portrait)
				{
					VideoDataWithEventsLoader?.InitFullscreenDialog("", "Close");
				}
				base.OnConfigurationChanged(newConfig);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

	}
}