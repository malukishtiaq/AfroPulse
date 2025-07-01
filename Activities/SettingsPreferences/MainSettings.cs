using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Activities.SettingsPreferences
{
	public static class MainSettings
	{
		#region Variables Basic

		public static ISharedPreferences SharedData, AutoNext, InAppReview, UgcPrivacy;
		public static readonly string PrefKeyAutoNext = "auto_next_key";

		public static readonly string PrefKeyInAppReview = "In_App_Review";

		public static readonly string LightMode = "light";
		public static readonly string DarkMode = "dark";
		public static readonly string DefaultMode = "default";

		#endregion

		public static void Init()
		{
			try
			{
				SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
				AutoNext = Application.Context.GetSharedPreferences("auto_next_perf", FileCreationMode.Private);
				InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);
				UgcPrivacy = Application.Context.GetSharedPreferences("Ugc_Privacy", FileCreationMode.Private);

				UserDetails.AutoNext = AutoNext.GetBoolean(PrefKeyAutoNext, true);

				UserDetails.PipIsChecked = SharedData.GetBoolean("picture_in_picture_key", false);

				UserDetails.PopupFilterPlaylistValue = SharedData.GetString("popup_filter_playlist_key", "0");

				string getValue = SharedData.GetString("Night_Mode_key", string.Empty);
				ApplyTheme(getValue);

				AppSettings.Lang = SharedData.GetString("Lang_key", AppSettings.Lang);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public static void ApplyTheme(string themePref)
		{
			try
			{
				if (themePref == LightMode)
				{
					AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
					AppSettings.SetTabDarkTheme = TabTheme.Light;
				}
				else if (themePref == DarkMode)
				{
					AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
					AppSettings.SetTabDarkTheme = TabTheme.Dark;
				}
				else if (themePref == DefaultMode)
				{
					AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

					var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;

					if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
					{
						AppSettings.SetTabDarkTheme = TabTheme.Dark;
						AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
					}
					else  // Night mode is not active, we're using the light theme
					{
						AppSettings.SetTabDarkTheme = TabTheme.Light;
						AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
					}
				}
				else
				{
					switch (AppSettings.SetTabDarkTheme)
					{
						case TabTheme.Dark:
							AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
							AppSettings.SetTabDarkTheme = TabTheme.Dark;
							break;
						case TabTheme.Light:
							AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
							AppSettings.SetTabDarkTheme = TabTheme.Light;
							break;
						default:
							{
								var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;

								if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
								{
									AppSettings.SetTabDarkTheme = TabTheme.Dark;
									AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
								}
								else  // Night mode is not active, we're using the light theme
								{
									AppSettings.SetTabDarkTheme = TabTheme.Light;
									AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
								}

								break;
							}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}