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
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Auth;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Default
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class VerificationCodeActivity : BaseActivity
	{
		#region Variables Basic

		private EditText TxtNumber1;
		private AppCompatButton BtnVerify;

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
				SetContentView(Resource.Layout.VerificationCodeLayout);

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
				TxtNumber1 = FindViewById<EditText>(Resource.Id.TextNumber1);
				BtnVerify = FindViewById<AppCompatButton>(Resource.Id.verifyButton);
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

		private void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					BtnVerify.Click += BtnVerifyOnClick;
				}
				else
				{
					BtnVerify.Click -= BtnVerifyOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void BtnVerifyOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(TxtNumber1.Text) && !string.IsNullOrWhiteSpace(TxtNumber1.Text))
				{
					if (Methods.CheckConnectivity())
					{
						//Show a progress
						AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

						var (apiStatus, respond) = await RequestsAsync.Auth.TwoFactorAsync(TxtNumber1.Text, UserDetails.DeviceId);
						if (apiStatus == 200)
						{
							if (respond is AuthTwoFactorObject auth)
							{
								SetDataLogin(auth.Data);

								UserDetails.IsLogin = true;

								StartActivity(new Intent(this, typeof(TabbedMainActivity)));
								AndHUD.Shared.Dismiss();
								FinishAffinity();
							}
						}
						else
						{
							if (respond is ErrorObject errorMessage)
							{
								Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorMessage.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
							}
							Methods.DisplayReportResult(this, respond);
						}

						AndHUD.Shared.Dismiss();
					}
					else
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
				else
				{
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				AndHUD.Shared.Dismiss();
			}
		}

		#endregion

		private void SetDataLogin(AuthTwoFactor auth)
		{
			try
			{
				UserDetails.AccessToken = Current.AccessToken = auth.SessionId;
				UserDetails.UserId = InitializePlayTube.UserId = auth.UserId;
				UserDetails.Status = "Active";
				UserDetails.Cookie = auth.Cookie;

				//Insert user data to database
				var user = new DataTables.LoginTb
				{
					UserId = UserDetails.UserId,
					AccessToken = UserDetails.AccessToken,
					Cookie = UserDetails.Cookie,
					Username = UserDetails.Username,
					Password = UserDetails.Password,
					Status = "Active",
					Lang = "",
					DeviceId = UserDetails.DeviceId,
					Email = UserDetails.Email,
				};

				ListUtils.DataUserLoginList = new ObservableCollection<DataTables.LoginTb> { user };
				UserDetails.IsLogin = true;

				var dbDatabase = new SqLiteDatabase();
				dbDatabase.InsertOrUpdateLogin_Credentials(user);

				PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetChannelData(this, UserDetails.UserId) });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}