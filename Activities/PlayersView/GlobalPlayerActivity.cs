using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;

namespace PlayTube.Activities.PlayersView
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", AutoRemoveFromRecents = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop, SupportsPictureInPicture = true, ResizeableActivity = true)]
	public class GlobalPlayerActivity : BaseActivity
	{
		#region Variables Basic

		private static GlobalPlayerActivity Instance;
		public static bool OnOpenPage;
		private bool OnStopCalled;

		public VideoDataWithEventsLoader VideoDataWithEventsLoader;
		public PictureInPictureParams PictureInPictureParams;

		public readonly string ActionPlay = Application.Context.PackageName + ".action.PLAY_PLAYER";
		public readonly string ActionPause = Application.Context.PackageName + ".action.PAUSE_PLAYER";
		private RemoteActionReceiver MRemoteActionReceiver;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.VideoSliderLayout);

				Instance = this;
				OnOpenPage = true;
				InitBackPressed("GlobalPlayerActivity");

				VideoDataWithEventsLoader = new VideoDataWithEventsLoader(this, "GlobalPlayerActivity");

				var videoData = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("VideoObject") ?? "");
				VideoDataWithEventsLoader.LoadDataVideo(videoData);
				EnterPipMode(true);

				MRemoteActionReceiver = new RemoteActionReceiver();
				MRemoteActionReceiver.SetGlobalPlayerActivity(this);

				IntentFilter intentFilter = new IntentFilter();
				intentFilter.AddAction(ActionPlay);
				intentFilter.AddAction(ActionPause);

				if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
					RegisterReceiver(MRemoteActionReceiver, intentFilter, ReceiverFlags.Exported);
				else
					RegisterReceiver(MRemoteActionReceiver, intentFilter);
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
				VideoDataWithEventsLoader?.AddOrRemoveEvent(true);
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
				VideoDataWithEventsLoader?.AddOrRemoveEvent(false);
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

		protected override void OnStop()
		{
			try
			{
				OnStopCalled = true;
				base.OnStop();
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
				VideoDataWithEventsLoader?.OnDestroy();
				UnregisterReceiver(MRemoteActionReceiver);
				OnOpenPage = false;

				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		protected override void OnNewIntent(Intent intent)
		{
			try
			{
				base.OnNewIntent(intent);

				var videoData = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("VideoObject") ?? "");
				VideoDataWithEventsLoader?.NewLoad(videoData);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Functions

		public static GlobalPlayerActivity GetInstance()
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

		#region Back Pressed

		private bool IsPipModeEnabled = true;
		public void BackPressed()
		{
			try
			{
				if (VideoDataWithEventsLoader.TubePlayerView != null && VideoDataWithEventsLoader.TubePlayerView.FullScreen)
				{
					VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
					return;
				}

				if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) && IsPipModeEnabled)
				{
					switch (VideoDataWithEventsLoader?.VideoType)
					{
						case VideoEnumTypes.Normal:
							EnterPictureInPictureMode(PictureInPictureParams);
							break;
						case VideoEnumTypes.Youtube:
							Finish();
							break;
					}
				}
				else
				{
					Finish();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				Finish();
			}
		}

		public void EnterPipMode(bool nowPlaying)
		{
			try
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
				{
					if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
					{
						Rect sourceRectHint = new Rect();
						VideoDataWithEventsLoader.PlayerView.GetGlobalVisibleRect(sourceRectHint);

						var builder = new PictureInPictureParams.Builder();

						// Check if the setAspectRatio method is available (added in API level 31)
						if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
						{
							Rational aspectRatioRational = new Rational(32, 16);
							builder.SetAspectRatio(aspectRatioRational);
						}

						builder.SetSourceRectHint(sourceRectHint);

						if ((int)Build.VERSION.SdkInt >= 31)
							builder.SetAutoEnterEnabled(true);

						builder.SetActions(BuildPipParams(nowPlaying));

						PictureInPictureParams = builder?.Build();
						SetPictureInPictureParams(PictureInPictureParams);
					}
					else
					{
						EnterPictureInPictureMode(PictureInPictureParams);
					}

					//new Handler(Looper.MainLooper).PostDelayed(CheckPipPermission, 30);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void CheckPipPermission()
		{
			try
			{
				IsPipModeEnabled = IsInPictureInPictureMode;
				if (!IsInPictureInPictureMode)
				{
					BackPressed();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Permissions && Result

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				base.OnActivityResult(requestCode, resultCode, data);

				switch (requestCode)
				{
					case 2000 when resultCode == Result.Ok:
						{
							VideoDataWithEventsLoader?.ExoController?.RestartPlayAfterShrinkScreen();
							break;
						}
					case 2100 when resultCode == Result.Ok:
						{
							VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
							break;
						}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				switch (requestCode)
				{
					case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
						VideoDataWithEventsLoader?.DownloadVideo();
						break;
					case 100:
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region PictureInPictur

		public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
		{
			try
			{
				VideoDataWithEventsLoader.ExoController.HideControls(isInPictureInPictureMode);

				if (isInPictureInPictureMode)
				{
					// Hide the full-screen UI (controls, etc.) while in PiP mode.
					VideoDataWithEventsLoader.CoordinatorLayoutView.Visibility = ViewStates.Gone;

					switch (VideoDataWithEventsLoader.VideoType)
					{
						case VideoEnumTypes.Normal:
							if (VideoDataWithEventsLoader.ExoController.GetControlView() != null)
								VideoDataWithEventsLoader.ExoController.GetControlView().Visibility = ViewStates.Gone;

							VideoDataWithEventsLoader.ExoController.HideControls(isInPictureInPictureMode);

							VideoDataWithEventsLoader.TubePlayerView.Visibility = ViewStates.Gone;

							var layoutParams = VideoDataWithEventsLoader.PlayerView.LayoutParameters;
							layoutParams.Height = ViewGroup.LayoutParams.MatchParent;
							VideoDataWithEventsLoader.PlayerView.LayoutParameters = layoutParams;

							var layoutParamsMainroot = VideoDataWithEventsLoader.Mainroot.LayoutParameters;
							layoutParamsMainroot.Height = ViewGroup.LayoutParams.MatchParent;
							VideoDataWithEventsLoader.Mainroot.LayoutParameters = layoutParamsMainroot;

							break;
						case VideoEnumTypes.Youtube:
							if (isInPictureInPictureMode)
								VideoDataWithEventsLoader.TubePlayerView?.PlayerUiController.ShowUi(false);
							break;
					}
				}
				else
				{
					// Restore the full-screen UI.
					VideoDataWithEventsLoader.CoordinatorLayoutView.Visibility = ViewStates.Visible;
					switch (VideoDataWithEventsLoader.VideoType)
					{
						case VideoEnumTypes.Normal:
							if (VideoDataWithEventsLoader.ExoController.GetControlView() != null)
								VideoDataWithEventsLoader.ExoController.GetControlView().Visibility = ViewStates.Visible;

							VideoDataWithEventsLoader.ExoController.HideControls(isInPictureInPictureMode);

							Resources r = Application.Context.Resources;
							int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 210, r.DisplayMetrics);

							var layoutParams = VideoDataWithEventsLoader.PlayerView.LayoutParameters;
							layoutParams.Height = px;
							VideoDataWithEventsLoader.PlayerView.LayoutParameters = layoutParams;

							var layoutParamsMainroot = VideoDataWithEventsLoader.Mainroot.LayoutParameters;
							layoutParamsMainroot.Height = px;
							VideoDataWithEventsLoader.Mainroot.LayoutParameters = layoutParamsMainroot;

							break;
						case VideoEnumTypes.Youtube:
							VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowUi(true);
							break;
					}

					//when close
					if (OnStopCalled)
					{
						switch (VideoDataWithEventsLoader.VideoType)
						{
							case VideoEnumTypes.Normal:
								VideoDataWithEventsLoader.OnStop();
								break;
							case VideoEnumTypes.Youtube:
								VideoDataWithEventsLoader.YoutubePlayer.Pause();
								VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowUi(true);
								break;
						}

						VideoDataWithEventsLoader.FinishActivityAndTask();
					}
				}

				base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		protected override void OnUserLeaveHint()
		{
			try
			{
				switch (VideoDataWithEventsLoader.VideoType)
				{
					case VideoEnumTypes.Normal:
						if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
						{
							EnterPictureInPictureMode(PictureInPictureParams);
						}
						base.OnUserLeaveHint();
						break;
					case VideoEnumTypes.Youtube:
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private List<RemoteAction> BuildPipParams(bool nowPlaying)
		{
			try
			{
				Icon icon;
				string text, action;
				if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
				{
					if (nowPlaying)
					{
						icon = Icon.CreateWithResource(this, ResourceConstant.Drawable.icon_pause_vector);
						text = "pause";
						action = ActionPause;
					}
					else
					{
						icon = Icon.CreateWithResource(this, ResourceConstant.Drawable.icon_play_vector);
						text = "play";
						action = ActionPlay;
					}

					var intent = new Intent(action);

					PendingIntent broadcast = PendingIntent.GetBroadcast(this, nowPlaying ? 0 : 1, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
					RemoteAction remoteAction = new RemoteAction(icon, text, text, broadcast);

					List<RemoteAction> actions = [remoteAction];

					return actions;
				}

				return null;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#endregion

	}


	[BroadcastReceiver(Exported = true)]
	public class RemoteActionReceiver : BroadcastReceiver
	{
		private GlobalPlayerActivity Activity;
		public void SetGlobalPlayerActivity(GlobalPlayerActivity activity)
		{
			Activity = activity;
		}

		public override void OnReceive(Context context, Intent intent)
		{
			try
			{
				if (intent?.Action == null)
				{
					Console.WriteLine("onReceive intent should not be null and contain an action.");
					return;
				}

				if (intent.Action == Activity.ActionPlay)
				{
					switch (Activity.VideoDataWithEventsLoader.VideoType)
					{
						case VideoEnumTypes.Normal:
							Activity.VideoDataWithEventsLoader.ExoController?.PlayVideo();
							break;
						case VideoEnumTypes.Youtube:
							Activity.VideoDataWithEventsLoader.YoutubePlayer.Play();
							break;
					}
					Activity.EnterPipMode(true);
				}
				else if (intent.Action == Activity.ActionPause)
				{
					switch (Activity.VideoDataWithEventsLoader.VideoType)
					{
						case VideoEnumTypes.Normal:
							Activity.VideoDataWithEventsLoader.OnStop();

							if (Activity.VideoDataWithEventsLoader.ExoController.GetControlView() != null)
								Activity.VideoDataWithEventsLoader.ExoController.GetControlView().Visibility = ViewStates.Gone;
							break;
						case VideoEnumTypes.Youtube:
							Activity.VideoDataWithEventsLoader.YoutubePlayer.Pause();
							break;
					}
					Activity.EnterPipMode(false);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}