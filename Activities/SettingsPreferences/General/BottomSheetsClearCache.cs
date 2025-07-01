using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Utils;
using System;
using System.IO;
using System.Threading.Tasks;


namespace PlayTube.Activities.SettingsPreferences.General
{
	public class BottomSheetsClearCache : BottomSheetDialogFragment
	{
		#region Variables Basic

		private TabbedMainActivity GlobalContext;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);


			GlobalContext = TabbedMainActivity.GetInstance();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
				View view = localInflater?.Inflate(Resource.Layout.BottomSheetsClearCache, container, false);
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
				InitComponent(view);
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


		public override void OnDismiss(IDialogInterface dialog)
		{
			base.OnDismiss(dialog);
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				var btnClearCacheCancel = view.FindViewById<AppCompatButton>(Resource.Id.btn_clear_cache_cancel);
				var btnClearCacheDelete = view.FindViewById<AppCompatButton>(Resource.Id.btn_clear_cache_delete);

				btnClearCacheDelete.Click += (sender, args) =>
				{
					try
					{
						new LibrarySynchronizer(GlobalContext).AddToWatchOffline(null);

						var page = TabbedMainActivity.GetInstance()?.LibraryFragment;
						if (page != null)
						{
							page.WatchOfflineCount.Text = GetText(Resource.String.Lbl_NoVideos);
						}

						Toast.MakeText(GlobalContext, GetText(Resource.String.Lbl_FilesAreNowDeleted), ToastLength.Long)?.Show();

						Task.Factory.StartNew(() =>
						{
							try
							{
								var dirPath = GlobalContext.CacheDir;
								dirPath.Delete();

								string path = Methods.Path.FolderDcimMyApp;
								if (Directory.Exists(path))
								{
									Directory.Delete(path, true);
								}

								Methods.Path.Chack_MyFolder();
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						});
						Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				btnClearCacheCancel.Click += (sender, args) =>
				{
					try
					{
						Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		#endregion
	}
}