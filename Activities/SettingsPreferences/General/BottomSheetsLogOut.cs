using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Activities.SettingsPreferences.General
{
	public class BottomSheetsLogOut : BottomSheetDialogFragment
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
				View view = localInflater?.Inflate(Resource.Layout.BottomSheetsLogOut, container, false);
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
				var btnLogOutCancel = view.FindViewById<AppCompatButton>(Resource.Id.btn_log_out_cancel);
				var btnLogOutYes = view.FindViewById<AppCompatButton>(Resource.Id.btn_log_out_yes);

				btnLogOutYes.Click += (sender, args) =>
				{
					try
					{
						TabbedMainActivity.GetInstance()?.VideoDataWithEventsLoader?.OnDestroy();

						ApiRequest.Logout(Activity);

						Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				btnLogOutCancel.Click += (sender, args) =>
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