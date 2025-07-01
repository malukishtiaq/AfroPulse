using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Activities.SettingsPreferences.General
{
	public class BottomSheetsThemes : BottomSheetDialogFragment
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
				View view = localInflater?.Inflate(Resource.Layout.BottomSheetsThemes, container, false);
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
				var themeLight = view.FindViewById<LinearLayout>(Resource.Id.theme_light);
				var themeMoon = view.FindViewById<LinearLayout>(Resource.Id.theme_moon_);

				var themeLightCheck = view.FindViewById<ImageView>(Resource.Id.theme_light_check);
				var themeMoonCheck = view.FindViewById<ImageView>(Resource.Id.theme_moon_check);


				string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);
				if (getValue == MainSettings.LightMode)
				{
					themeLightCheck.Visibility = ViewStates.Visible;
				}
				else
				{
					themeLightCheck.Visibility = ViewStates.Gone;
				}

				if (getValue == MainSettings.DarkMode)
				{
					themeMoonCheck.Visibility = ViewStates.Visible;
				}
				else
				{
					themeMoonCheck.Visibility = ViewStates.Gone;
				}

				themeLight.Click += (sender, args) =>
				{
					try
					{

						if (getValue != MainSettings.LightMode)
						{
							//Set Light Mode   
							//NightMode.Summary = this.GetString(Resource.String.Lbl_Light);

							MainSettings.ApplyTheme(MainSettings.LightMode);
							MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

							if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
							{
								GlobalContext.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
								GlobalContext.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
							}

							Intent intent = new Intent(GlobalContext, typeof(TabbedMainActivity));
							intent.AddCategory(Intent.CategoryHome);
							intent.SetAction(Intent.ActionMain);
							intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
							intent.AddFlags(ActivityFlags.NoAnimation);
							GlobalContext.FinishAffinity();
							GlobalContext.OverridePendingTransition(0, 0);
							StartActivity(intent);
						}
						Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				themeMoon.Click += (sender, args) =>
				{
					try
					{
						if (getValue != MainSettings.DarkMode)
						{
							//NightMode.Summary = this.GetString(Resource.String.Lbl_Dark);

							MainSettings.ApplyTheme(MainSettings.DarkMode);
							MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

							if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
							{
								GlobalContext.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
								GlobalContext.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
							}

							Intent intent = new Intent(GlobalContext, typeof(TabbedMainActivity));
							intent.AddCategory(Intent.CategoryHome);
							intent.SetAction(Intent.ActionMain);
							intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
							intent.AddFlags(ActivityFlags.NoAnimation);
							GlobalContext.FinishAffinity();
							GlobalContext.OverridePendingTransition(0, 0);
							StartActivity(intent);
						}
						Dismiss();
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				};

				//themeBattery.Click += (sender, args) =>
				//{
				//	try
				//	{
				//		if (getValue != MainSettings.DefaultMode)
				//		{
				//			//NightMode.Summary = this.GetString(Resource.String.Lbl_SetByBattery);
				//			MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

				//			if ((int)Build.VERSION.SdkInt >= 29)
				//			{
				//				AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

				//				var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
				//				switch (currentNightMode)
				//				{
				//					case UiMode.NightNo:
				//						// Night mode is not active, we're using the light theme
				//						MainSettings.ApplyTheme(MainSettings.LightMode);
				//						break;
				//					case UiMode.NightYes:
				//						// Night mode is active, we're using dark theme
				//						MainSettings.ApplyTheme(MainSettings.DarkMode);
				//						break;
				//				}
				//			}
				//			else
				//			{
				//				AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

				//				var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
				//				switch (currentNightMode)
				//				{
				//					case UiMode.NightNo:
				//						// Night mode is not active, we're using the light theme
				//						MainSettings.ApplyTheme(MainSettings.LightMode);
				//						break;
				//					case UiMode.NightYes:
				//						// Night mode is active, we're using dark theme
				//						MainSettings.ApplyTheme(MainSettings.DarkMode);
				//						break;
				//				}

				//				if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
				//				{
				//					GlobalContext.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
				//					GlobalContext.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
				//				}

				//				Intent intent = new Intent(GlobalContext, typeof(TabbedMainActivity));
				//				intent.AddCategory(Intent.CategoryHome);
				//				intent.SetAction(Intent.ActionMain);
				//				intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
				//				intent.AddFlags(ActivityFlags.NoAnimation);
				//				GlobalContext.FinishAffinity();
				//				GlobalContext.OverridePendingTransition(0, 0);
				//				StartActivity(intent);
				//			}
				//		}
				//		Dismiss();
				//	}
				//	catch (Exception exception)
				//	{
				//		Methods.DisplayReportResultTrack(exception);
				//	}
				//};

			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		#endregion
	}
}