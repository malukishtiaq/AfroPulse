using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.Security
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class TwoFactorAuthActivity : BaseActivity
	{
		#region Variables Basic

		private Switch SwitchPip;
		private string TypeTwoFactor;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				Methods.App.FullScreenApp(this);

				// Create your application here
				SetContentView(Resource.Layout.TwoFactorAuthLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
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

		#endregion

		#region Menu

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

		#endregion

		#region Functions

		private void InitComponent()
		{
			try
			{
				SwitchPip = FindViewById<Switch>(Resource.Id.SwitchPIP);

				AdsGoogle.Ad_AdMobNative(this);

				var twoFactorUSer = ListUtils.MyChannelList?.FirstOrDefault()?.TwoFactor;
				if (twoFactorUSer == "0")
				{
					//Disable
					SwitchPip.Checked = false;
					TypeTwoFactor = "0";
				}
				else
				{
					//Enable
					SwitchPip.Checked = true;
					TypeTwoFactor = "1";
				}
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
				Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
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

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					SwitchPip.CheckedChange += SwitchPIPOnCheckedChange;
				}
				else
				{
					SwitchPip.CheckedChange -= SwitchPIPOnCheckedChange;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void SwitchPIPOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					return;
				}

				TypeTwoFactor = e.IsChecked ? "1" : "0";

				//Show a progress
				AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

				var dictionary = new Dictionary<string, string>
				{
					{"settings_type", "two_factor"},
					{"two_factor", TypeTwoFactor},
				};

				var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataGeneralAsync(dictionary);
				if (apiStatus == 200)
				{
					if (respond is not MessageObject result) return;
					Console.WriteLine(result.Message);

					var local = ListUtils.MyChannelList?.FirstOrDefault();
					if (local != null)
					{
						local.TwoFactor = TypeTwoFactor;

						var sqLiteDatabase = new SqLiteDatabase();
						sqLiteDatabase.InsertOrUpdate_DataMyChannel(local);
					}

					Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short)?.Show();
					AndHUD.Shared.Dismiss();
				}
				else
				{
					Methods.DisplayAndHudErrorResult(this, respond);
				}
			}
			catch (Exception exception)
			{
				AndHUD.Shared.Dismiss();
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

	}
}