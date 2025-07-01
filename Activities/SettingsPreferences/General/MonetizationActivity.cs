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
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.TextField;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Globalization;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.General
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MonetizationActivity : BaseActivity
	{
		#region Variables Basic

		private AdView MAdView;
		private TextView CountBalnceText;
		private AppCompatButton BtnWithdraw;
		private TextInputEditText AmountEditText, PayPalEmailEditText;
		private double CountBalnce;
		#endregion

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.MonetizationLayout);
				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				Get_Data_User();
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
				AdsGoogle.LifecycleAdView(MAdView, "Resume");
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

				AdsGoogle.LifecycleAdView(MAdView, "Pause");
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

		protected override void OnDestroy()
		{
			try
			{
				AdsGoogle.LifecycleAdView(MAdView, "Destroy");
				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

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
				BtnWithdraw = FindViewById<AppCompatButton>(Resource.Id.btn_withdraw);
				CountBalnceText = FindViewById<TextView>(Resource.Id.countBalnceText);
				AmountEditText = FindViewById<TextInputEditText>(Resource.Id.Monetization_Edit);
				PayPalEmailEditText = FindViewById<TextInputEditText>(Resource.Id.PayPalEmail_Edit);

				Methods.SetColorEditText(AmountEditText, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(PayPalEmailEditText, AppTools.IsTabDark() ? Color.White : Color.Black);

				MAdView = FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(MAdView, null);
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
					BtnWithdraw.Click += BtnWithdrawOnClick;
				}
				else
				{
					BtnWithdraw.Click -= BtnWithdrawOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void BtnWithdrawOnClick(object sender, EventArgs e)
		{
			try
			{
				if (CountBalnce < Convert.ToDouble(AmountEditText.Text))
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CantRequestMonetization), ToastLength.Long)?.Show();
				}
				else if (string.IsNullOrEmpty(PayPalEmailEditText.Text.Replace(" ", "")) || string.IsNullOrEmpty(AmountEditText.Text.Replace(" ", "")))
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
				}
				else
				{
					if (Methods.CheckConnectivity())
					{
						//Show a progress
						AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

						var (apiStatus, respond) = await RequestsAsync.Global.MonetizationAsync(AmountEditText.Text, PayPalEmailEditText.Text);
						if (apiStatus == 200)
						{
							if (respond is MessageObject result)
							{
								Console.WriteLine(result.Message);
								Toast.MakeText(this, GetText(Resource.String.Lbl_RequestSentMonetization), ToastLength.Long)?.Show();
							}
						}
						else Methods.DisplayReportResult(this, respond);

						AndHUD.Shared.Dismiss();
					}
					else
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception exception)
			{
				AndHUD.Shared.Dismiss();
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private async void Get_Data_User()
		{
			try
			{
				if (ListUtils.MyChannelList?.Count == 0)
					await ApiRequest.GetChannelData(this, UserDetails.UserId);

				var local = ListUtils.MyChannelList?.FirstOrDefault();
				if (local != null)
				{
					CountBalnce = Convert.ToDouble(local.Balance);
					CountBalnceText.Text = "$" + CountBalnce.ToString(CultureInfo.InvariantCulture);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}
}