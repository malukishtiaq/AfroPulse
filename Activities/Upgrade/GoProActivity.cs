using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseActivity = PlayTube.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Upgrade
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
	public class GoProActivity : BaseActivity
	{
		#region Variables Basic

		private TextView HeadText, PriceProMember, PriceFreeMember;
		private AppCompatButton BtnUpgrade;

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
				SetContentView(Resource.Layout.GoProLayout);

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
				HeadText = FindViewById<TextView>(Resource.Id.headText);
				PriceFreeMember = FindViewById<TextView>(Resource.Id.priceFreeMember);
				PriceProMember = FindViewById<TextView>(Resource.Id.priceProMember);

				HeadText.Text = GetText(Resource.String.Lbl_Title_Pro1) + " " + AppSettings.ApplicationName + " " + GetText(Resource.String.Lbl_Title_Pro2);

				BtnUpgrade = FindViewById<AppCompatButton>(Resource.Id.btn_upgrade);

				var idCurrency = ListUtils.MySettingsList?.PaypalCurrency ?? "USD";
				var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
				Console.WriteLine(currency);

				PriceProMember.Text = currencyIcon + ListUtils.MySettingsList?.ProPkgPrice;
				PriceFreeMember.Text = currencyIcon + "0";
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
					BtnUpgrade.Click += UpgradeButtonOnClick;
				}
				else
				{
					BtnUpgrade.Click -= UpgradeButtonOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void UpgradeButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				var dialog = new MaterialAlertDialogBuilder(this);
				dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
				dialog.SetMessage(GetText(Resource.String.Lbl_Go_Pro));
				dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
				{
					try
					{
						if (await AppTools.CheckWallet(AppSettings.AmountGoPro))
						{
							if (Methods.CheckConnectivity())
							{
								var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("pro");
								if (apiStatus == 200)
								{
									if (respond is MessageObject result)
									{
										Console.WriteLine(result.Message);

										PollyController.RunRetryPolicyFunction(new List<Func<Task>> { SetProApi });
									}
								}
								else Methods.DisplayAndHudErrorResult(this, respond);
							}
							else
								Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
						}
						else
						{
							var dialogBuilder = new MaterialAlertDialogBuilder(this);
							dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
							dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
							dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
							{
								try
								{
									var intent = new Intent(this, typeof(WalletActivity));
									intent.PutExtra("Type", "GoPro");
									StartActivity(intent);
								}
								catch (Exception exception)
								{
									Methods.DisplayReportResultTrack(exception);
								}
							});
							dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

							dialogBuilder.Show();
						}
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				});
				dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

				dialog.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private async Task SetProApi()
		{
			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Global.UpgradeAsync();
				if (apiStatus == 200)
				{
					var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
					if (dataUser != null)
					{
						dataUser.IsPro = "1";

						var sqlEntity = new SqLiteDatabase();
						sqlEntity.InsertOrUpdate_DataMyChannel(dataUser);
					}

					Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
					Finish();
				}
				else Methods.DisplayReportResult(this, respond);
			}
			else
			{
				Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
			}
		}
	}
}