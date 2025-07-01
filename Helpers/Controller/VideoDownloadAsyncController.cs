//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.SQLite;
using System;
using System.IO;
using System.Linq;
using Environment = Android.OS.Environment;

namespace PlayTube.Helpers.Controller
{
	public class VideoDownloadAsyncController
	{
		private readonly DownloadManager DownloadManager;
		private readonly DownloadManager.Request Request;
		public static readonly string FilePath = Android.OS.Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;
		private readonly string Filename;
		private long DownloadId;
		private VideoDataObject VideoData;
		private static Activity ActivityContext;

		public VideoDownloadAsyncController(string url, string filename, Activity contextActivity)
		{
			try
			{
				ActivityContext = contextActivity;

				if (!filename.Contains(".mp4") || !filename.Contains(".Mp4") || !filename.Contains(".MP4"))
					Filename = filename + ".mp4";
				else
					Filename = filename;

				DownloadManager = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
				Request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void StartDownloadManager(string title, VideoDataObject video)
		{
			try
			{
				Methods.Path.Chack_MyFolder();

				if (video != null && !string.IsNullOrEmpty(title))
				{
					VideoData = video;

					var sqlEntity = new SqLiteDatabase();
					sqlEntity.Insert_WatchOfflineVideos(video);

					var folder = GetDownloadedFolder();

					Request.SetTitle(title);
					Request.SetAllowedNetworkTypes(DownloadNetwork.Mobile | DownloadNetwork.Wifi);

					Request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, "/" + AppSettings.ApplicationName + "/" + Filename);

					Request.SetNotificationVisibility(DownloadVisibility.Visible);
					Request.SetAllowedOverRoaming(true);
					DownloadId = DownloadManager.Enqueue(Request);

					var onDownloadComplete = new OnDownloadComplete
					{
						Video = video
					};

					ActivityContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));
				}
				else
				{
					Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Download_faileds), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void StopDownloadManager()
		{
			try
			{
				DownloadManager.Remove(DownloadId);
				RemoveDiskFile(Filename, VideoData.Id);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public bool RemoveDiskFile(string filename, string id)
		{
			try
			{
				var directories = GetDownloadedFolder();
				string path = new Java.IO.File(directories, filename + ".mp4").Path;

				if (File.Exists(path))
				{
					var sqlEntity = new SqLiteDatabase();
					sqlEntity.Remove_WatchOfflineVideos(id);
					File.Delete(path);
					return true;
				}

				return false;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return false;
			}
		}

		public bool CheckDownloadLinkIfExits()
		{
			try
			{
				if (File.Exists(FilePath + Filename))
					return true;

				return false;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return false;
			}
		}

		public static string GetDownloadedFolder()
		{
			try
			{
				if (!PermissionsController.CheckPermissionStorage(Application.Context))
					return "";

				string directories;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
				{
					var directories1 = Application.Context.GetExternalFilesDir(""); //storage/emulated/0/Android/data/****
					var pathDefault = directories1.AbsolutePath.Split("/Android/")?.FirstOrDefault();
					directories = pathDefault + "/" + Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;

					//var directories = Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName);
				}
				else
				{
					directories = Methods.Path.GetDirectoryDcim() + "/" + Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;
				}

				if (!Directory.Exists(directories))
					Directory.CreateDirectory(directories);

				return directories;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return "";
			}
		}

		public static string GetDownloadedDiskVideoUri(string filename)
		{
			try
			{
				var directories = GetDownloadedFolder();
				Java.IO.File file = new Java.IO.File(directories, filename + ".mp4");

				//Hbh14ktZ3i4frTd  
				if (file.Exists())
				{
					return file.Path;
				}

				return "";
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return "";
			}
		}

		[BroadcastReceiver(Exported = false)]
		[IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
		public class OnDownloadComplete : BroadcastReceiver
		{
			public VideoDataObject Video;

			public override void OnReceive(Context context, Intent intent)
			{
				try
				{
					if (intent.Action == DownloadManager.ActionDownloadComplete)
					{
						if (context == null)
							return;

						DownloadManager downloadManagerExcuter = (DownloadManager)context.GetSystemService(Context.DownloadService);
						long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
						DownloadManager.Query query = new DownloadManager.Query();
						query.SetFilterById(downloadId);
						ICursor c = downloadManagerExcuter.InvokeQuery(query);
						var sqlEntity = new SqLiteDatabase();

						if (c.MoveToFirst())
						{
							int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
							if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
							{
								string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

								ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
								ActivityManager.GetMyMemoryState(appProcessInfo);
								if (appProcessInfo.Importance == Importance.Foreground || appProcessInfo.Importance == Importance.Background)
								{
									sqlEntity.Update_WatchOfflineVideos(Video.Id, downloadedPath);
									if (ActivityContext is TabbedMainActivity tabbedMain)
									{
										tabbedMain.VideoDataWithEventsLoader.ExoController.DownloadIcon.Tag = "Downloaded";
										tabbedMain.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_check_circle);
										tabbedMain.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetColorFilter(Color.Red);
										tabbedMain.LibrarySynchronizer.AddToWatchOffline(Video);

										// tabbedMain.VideoActionsController.ProgressBarDownload.Visibility = ViewStates.Invisible;
										// tabbedMain.VideoActionsController.BtnIconDownload.Visibility = ViewStates.Visible;
									}
									else if (ActivityContext is GlobalPlayerActivity globalPlayer)
									{
										globalPlayer.VideoDataWithEventsLoader.ExoController.DownloadIcon.Tag = "Downloaded";
										globalPlayer.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_check_circle);
										globalPlayer.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetColorFilter(Color.Red);
										globalPlayer.VideoDataWithEventsLoader.LibrarySynchronizer.AddToWatchOffline(Video);

										// GlobalPlayer.VideoActionsController.ProgressBarDownload.Visibility = ViewStates.Invisible;
										// GlobalPlayer.VideoActionsController.BtnIconDownload.Visibility = ViewStates.Visible;
									}
									else if (ActivityContext is FullScreenVideoActivity fullScreen)
									{
										fullScreen.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetImageResource(Resource.Drawable.ic_check_circle);
										fullScreen.VideoDataWithEventsLoader.ExoController.DownloadIcon.SetColorFilter(Color.Red);
										fullScreen.VideoDataWithEventsLoader.ExoController.DownloadIcon.Tag = "Downloaded";
										//fullScreen tabbedMain.LibrarySynchronizer.OfflineVideoList.Add(Video);
									}
								}
								else
								{
									sqlEntity.Update_WatchOfflineVideos(Video.Id, downloadedPath);
								}
							}
						}
					}
				}
				catch (Exception exception)
				{
					Methods.DisplayReportResultTrack(exception);
				}
			}
		}
	}
}