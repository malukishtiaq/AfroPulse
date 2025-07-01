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
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Payment.Utils;
using System;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.General
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class WalletActivity : PaymentBaseActivity
	{
		#region Variables Basic

		private ImageView Avatar;
		private TextView TxtProfileName, TxtUsername;

		private TextView TxtMyBalance;
		public TextInputEditText TxtAmount;
		private AppCompatButton BtnReplenish;
		private static WalletActivity Instance;

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
				SetContentView(Resource.Layout.WalletLayout);

				Instance = this;

				//Get Value And Set Toolbar
				InitBuy();
				InitComponent();
				InitToolbar();

				AdsGoogle.Ad_AdMobNative(this);
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
				Get_Data_User();
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

		protected override void OnDestroy()
		{
			try
			{
				if (AppSettings.ShowRazorPay) InitRazorPay?.StopRazorPay();
				if (AppSettings.ShowPayStack) PayStackPayment?.StopPayStack();
				if (AppSettings.ShowPaySera) PaySeraPayment?.StopPaySera();
				if (AppSettings.ShowSecurionPay) SecurionPayPayment?.StopSecurionPay();
				if (AppSettings.ShowIyziPay) IyziPayPayment?.StopIyziPay();

				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Menu

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item.ItemId == Android.Resource.Id.Home)
			{
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
				Avatar = FindViewById<ImageView>(Resource.Id.avatar);
				TxtProfileName = FindViewById<TextView>(Resource.Id.name);
				TxtUsername = FindViewById<TextView>(Resource.Id.tv_subname);

				TxtMyBalance = FindViewById<TextView>(Resource.Id.myBalance);

				TxtAmount = FindViewById<TextInputEditText>(Resource.Id.AmountEditText);
				BtnReplenish = FindViewById<AppCompatButton>(Resource.Id.ReplenishButton);

				Methods.SetColorEditText(TxtAmount, AppTools.IsTabDark() ? Color.White : Color.Black);

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
					BtnReplenish.Click += BtnReplenishOnClick;
				}
				else
				{
					BtnReplenish.Click -= BtnReplenishOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static WalletActivity GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#endregion

		#region Events

		private void BtnReplenishOnClick(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterAmount), ToastLength.Long)?.Show();
					return;
				}

				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
					return;
				}

				Bundle bundle = new Bundle();

				Price = TxtAmount.Text;

				bundle.PutString("Price", Price);
				bundle.PutString("Type", Type);

				PaymentXBottomSheetDialog bottomSheetDialog = new PaymentXBottomSheetDialog()
				{
					Arguments = bundle
				};
				bottomSheetDialog.Show(SupportFragmentManager, bottomSheetDialog.Tag);
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
				await ApiRequest.GetChannelData(this, UserDetails.UserId);

				var local = ListUtils.MyChannelList?.FirstOrDefault();
				if (local != null)
				{
					GlideImageLoader.LoadImage(this, local.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					TxtProfileName.Text = AppTools.GetNameFinal(local);
					TxtUsername.Text = "@" + local.Username;

					var success = double.TryParse(local.Wallet, out var number);
					if (success)
					{
						TxtMyBalance.Text = number.ToString("F");
					}
					else
					{
						TxtMyBalance.Text = local.Wallet;
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}