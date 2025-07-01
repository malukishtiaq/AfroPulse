//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.Security
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class PasswordActivity : BaseActivity
	{
		#region Variables Basic

		private TextInputEditText TxtCurrentPassword, TxtNewPassword, TxtRepeatPassword;
		private AppCompatButton SaveButton;

		#endregion

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.ChangePasswordLayout);

				InitComponent();
				InitToolbar();
				AdsGoogle.Ad_AdMobNative(this);

			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void InitComponent()
		{
			try
			{
				//Get values
				TxtCurrentPassword = FindViewById<TextInputEditText>(Resource.Id.CurrentPassword_Edit);
				TxtNewPassword = FindViewById<TextInputEditText>(Resource.Id.NewPassword_Edit);
				TxtRepeatPassword = FindViewById<TextInputEditText>(Resource.Id.RepeatPassword_Edit);

				Methods.SetColorEditText(TxtCurrentPassword, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtNewPassword, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtRepeatPassword, AppTools.IsTabDark() ? Color.White : Color.Black);

				SaveButton = FindViewById<AppCompatButton>(Resource.Id.SaveButton);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar()
		{
			try
			{
				var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					toolbar.Title = " ";

					toolbar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(toolbar);
					SupportActionBar.SetDisplayShowCustomEnabled(true);
					SupportActionBar.SetDisplayHomeAsUpEnabled(true);
					SupportActionBar.SetHomeButtonEnabled(true);
					SupportActionBar.SetDisplayShowHomeEnabled(true);

					var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
					icon?.SetTint(AppTools.IsTabDark() ? Color.White : Color.Black);
					SupportActionBar.SetHomeAsUpIndicator(icon);
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private async void SaveButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(TxtCurrentPassword.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtNewPassword.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtRepeatPassword.Text.Replace(" ", "")))
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
					return;
				}

				if (TxtNewPassword.Text != TxtRepeatPassword.Text)
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long)?.Show();
				}
				else
				{
					if (Methods.CheckConnectivity())
					{
						//Show a progress
						AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

						var (apiStatus, respond) = await RequestsAsync.Auth.ChangePasswordAsync(TxtNewPassword.Text, TxtRepeatPassword.Text, TxtCurrentPassword.Text);
						if (apiStatus == 200)
						{
							if (respond is MessageObject result)
							{
								Toast.MakeText(this, result.Message, ToastLength.Short)?.Show();
								AndHUD.Shared.Dismiss();
							}
						}
						else
						{
							Methods.DisplayAndHudErrorResult(this, respond);
						}
					}
					else
					{
						AndHUD.Shared.Dismiss();
						Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
			}
			catch (Exception exception)
			{
				AndHUD.Shared.Dismiss();
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
			}
			return base.OnOptionsItemSelected(item);
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

		public void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					SaveButton.Click += SaveButtonOnClick;
				}
				else
				{
					SaveButton.Click -= SaveButtonOnClick;
				}
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
				AddOrRemoveEvent(true);
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
				AddOrRemoveEvent(false);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}