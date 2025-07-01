using Android;
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
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Com.Google.Android.Gms.Ads.Admanager;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Linq;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Payment
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class PaymentLocalActivity : BaseActivity
	{
		#region Variables Basic

		private TextView CardNumber, CardCode, CardCountry, CardName;
		private ImageView Image;
		private CircleButton ImageClose;
		private AppCompatButton BtnAddImage, BtnApply;
		private string Price, PathImage = "";
		private AdManagerAdView AdManagerAdView;


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
				SetContentView(Resource.Layout.PaymentLocalLayout);

				Price = Intent?.GetStringExtra("Price");

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
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
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
				CardNumber = (TextView)FindViewById(Resource.Id.card_number);
				CardCode = (TextView)FindViewById(Resource.Id.card_code);
				CardCountry = (TextView)FindViewById(Resource.Id.card_country);
				CardName = (TextView)FindViewById(Resource.Id.card_name);
				Image = (ImageView)FindViewById(Resource.Id.Image);

				ImageClose = (CircleButton)FindViewById(Resource.Id.ImageCircle);
				BtnAddImage = (AppCompatButton)FindViewById(Resource.Id.btn_AddPhoto);
				BtnApply = (AppCompatButton)FindViewById(Resource.Id.ApplyButton);

				var bankDescription = ListUtils.MySettingsList?.BankDescription;
				bankDescription = bankDescription?.Replace("&lt;", "<").Replace("&gt;", ">");
				var splitText = bankDescription?.Split(new[] { "<p>", "</p>" }, StringSplitOptions.None);
				Console.WriteLine(splitText);

				if (splitText != null)
				{
					CardNumber.Text = splitText[1];
					CardName.Text = splitText[3];
					CardCode.Text = splitText[5];
					CardCountry.Text = splitText[7];
				}

				AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
				AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
					toolbar.Title = GetString(Resource.String.Lbl_BankTransfer);
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
					ImageClose.Click += ImageCloseOnClick;
					BtnAddImage.Click += BtnAddImageOnClick;
					BtnApply.Click += BtnApplyOnClick;
				}
				else
				{
					ImageClose.Click -= ImageCloseOnClick;
					BtnAddImage.Click -= BtnAddImageOnClick;
					BtnApply.Click -= BtnApplyOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void BtnAddImageOnClick(object sender, EventArgs e)
		{
			try
			{
				OpenDialogGallery();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async void BtnApplyOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (string.IsNullOrEmpty(PathImage) || string.IsNullOrWhiteSpace(PathImage))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorPleaseSelectImage), ToastLength.Long)?.Show();
						return;
					}

					//Show a progress
					AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

					var (apiStatus, respond) = await RequestsAsync.Payment.UploadBankRecipeAsync(Price, "wallet", PathImage, "");
					if (apiStatus == 200)
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_YourWasReceiptSuccessfullyUploaded), ToastLength.Long)?.Show();

						AndHUD.Shared.Dismiss();
						Finish();
					}
					else
					{
						Methods.DisplayAndHudErrorResult(this, respond);
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				AndHUD.Shared.Dismiss();
			}
		}

		private void ImageCloseOnClick(object sender, EventArgs e)
		{
			try
			{
				PathImage = "";
				GlideImageLoader.LoadImage(this, "Grey_Offline", Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Permissions && Result

		//Result
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				base.OnActivityResult(requestCode, resultCode, data);

				if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
				{
					var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
					if (listPath?.List?.Count > 0)
					{
						var filepath = listPath.List.FirstOrDefault();
						if (!string.IsNullOrEmpty(filepath))
						{
							//Do something with your Uri
							PathImage = filepath;
							GlideImageLoader.LoadImage(this, filepath, Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Permissions
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				if (requestCode == 108)
				{
					if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
					{
						OpenDialogGallery();
					}
					else
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Dialog Gallery

		public void OpenDialogGallery(bool allowVideo = false, bool allowMultiple = false)
		{
			try
			{
				OptionPixImage optionPixImage = OptionPixImage.GetOptionPixImage(allowVideo, allowMultiple);

				// Check if we're running on Android 5.0 or higher
				if ((int)Build.VERSION.SdkInt < 23)
				{
					Intent intent = new Intent(this, typeof(PixImagePickerActivity));
					intent.PutExtra("OptionPixImage", JsonConvert.SerializeObject(optionPixImage));
					StartActivityForResult(intent, PixImagePickerActivity.RequestCode);
				}
				else
				{
					if (PermissionsController.CheckPermissionStorage(this, "file") && ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
					{
						Intent intent = new Intent(this, typeof(PixImagePickerActivity));
						intent.PutExtra("OptionPixImage", JsonConvert.SerializeObject(optionPixImage));
						StartActivityForResult(intent, PixImagePickerActivity.RequestCode);
					}
					else
					{
						new PermissionsController(this).RequestPermission(108, "file");
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

	}
}