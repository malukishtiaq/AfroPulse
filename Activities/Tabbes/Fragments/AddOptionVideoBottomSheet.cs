using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Shorts;
using PlayTube.Activities.Upload;
using PlayTube.Helpers.Utils;
using System;
using Exception = System.Exception;

namespace PlayTube.Activities.Tabbes.Fragments
{
	public class AddOptionVideoBottomSheet : BottomSheetDialogFragment
	{
		#region Variables Basic

		private TabbedMainActivity GlobalContext;

		private LinearLayout LiveLayout, ImportLayout, UploadLayout, ShortLayout;
		private ImageView IconClose;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Create your fragment here
			GlobalContext = (TabbedMainActivity)Activity;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

				View view = localInflater?.Inflate(Resource.Layout.BottomSheetOptionVideoLayout, container, false);
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

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				IconClose = view.FindViewById<ImageView>(Resource.Id.iconClose);
				IconClose.Click += IconCloseOnClick;

				LiveLayout = view.FindViewById<LinearLayout>(Resource.Id.LiveLayout);
				LiveLayout.Click += LiveLayoutOnClick;

				ImportLayout = view.FindViewById<LinearLayout>(Resource.Id.ImportLayout);
				ImportLayout.Click += BtnImportOnClick;

				UploadLayout = view.FindViewById<LinearLayout>(Resource.Id.UploadLayout);
				UploadLayout.Click += UploadLayoutOnClick;

				ShortLayout = view.FindViewById<LinearLayout>(Resource.Id.ShortLayout);
				ShortLayout.Click += ShortLayoutOnClick;

				ImportLayout.Visibility = AppSettings.ShowButtonImport ? ViewStates.Visible : ViewStates.Gone;
				UploadLayout.Visibility = AppSettings.ShowButtonUpload ? ViewStates.Visible : ViewStates.Gone;
				LiveLayout.Visibility = AppSettings.ShowGoLive ? ViewStates.Visible : ViewStates.Gone;
				ShortLayout.Visibility = AppSettings.ShowShorts ? ViewStates.Visible : ViewStates.Gone;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Event

		private void IconCloseOnClick(object sender, EventArgs e)
		{
			Dismiss();
		}

		private void ShortLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				Activity.StartActivityForResult(new Intent(Activity, typeof(CreateShortsActivity)), 3001);
				GlobalContext.VideoDataWithEventsLoader?.OnStop();

				Dismiss();
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}

		//Upload 
		private void UploadLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				Activity.StartActivityForResult(new Intent(Activity, typeof(VideoUploadActivity)), 3000);

				GlobalContext.VideoDataWithEventsLoader?.OnStop();

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//VideoObject Import
		private void BtnImportOnClick(object sender, EventArgs eventArgs)
		{
			try
			{
				Activity.StartActivity(new Intent(Activity, typeof(VideoImportActivity)));

				GlobalContext.VideoDataWithEventsLoader?.OnStop();

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Live 
		private void LiveLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				GlobalContext.GoLiveOnClick();
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion
	}
}