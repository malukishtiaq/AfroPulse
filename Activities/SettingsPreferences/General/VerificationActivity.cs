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
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.TextField;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Linq;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences.General
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class VerificationActivity : BaseActivity
	{
		#region Variables Basic

		private ImageView PassportImage;
		private AppCompatButton BtnAddImage, BtnSubmit;
		private TextInputEditText FirstNameEdit, LastNameEdit, MessagesEdit;
		private string PathImage = "";
		private LinearLayout VerifiedLinear, NotVerifiedLinear;
		private TextView VerifiedIcon, TextTitileVerified;


		#endregion

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.VerificationLayout);
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
				PassportImage = FindViewById<ImageView>(Resource.Id.Image);
				BtnAddImage = FindViewById<AppCompatButton>(Resource.Id.btn_AddPhoto);
				FirstNameEdit = FindViewById<TextInputEditText>(Resource.Id.FirstName_text);
				LastNameEdit = FindViewById<TextInputEditText>(Resource.Id.LastName_text);
				MessagesEdit = FindViewById<TextInputEditText>(Resource.Id.Messages_Edit);
				BtnSubmit = FindViewById<AppCompatButton>(Resource.Id.submitButton);

				TextTitileVerified = FindViewById<TextView>(Resource.Id.textTitileVerified);
				VerifiedIcon = FindViewById<TextView>(Resource.Id.verifiedIcon);
				VerifiedLinear = FindViewById<LinearLayout>(Resource.Id.verified);
				NotVerifiedLinear = FindViewById<LinearLayout>(Resource.Id.notVerified);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, VerifiedIcon, IonIconsFonts.CheckmarkCircle);
				VerifiedIcon.SetTextColor(Color.ParseColor(AppSettings.MainColor));
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
				var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolBar != null)
				{
					toolBar.Title = " ";

					toolBar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(toolBar);
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

		public void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					BtnAddImage.Click += BtnAddImageOnClick;
					BtnSubmit.Click += BtnSubmitOnClick;
				}
				else
				{
					BtnAddImage.Click -= BtnAddImageOnClick;
					BtnSubmit.Click -= BtnSubmitOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void BtnSubmitOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
				else
				{
					if (string.IsNullOrEmpty(FirstNameEdit.Text))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_firstname), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(LastNameEdit.Text))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_lastname), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(MessagesEdit.Text))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_Messages), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(PathImage))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short)?.Show();
					}
					else
					{
						//Show a progress
						AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

						var (apiStatus, respond) = await RequestsAsync.Global.VerificationAsync(FirstNameEdit.Text, LastNameEdit.Text, MessagesEdit.Text, PathImage);
						if (apiStatus == 200)
						{
							if (respond is MessageObject result)
							{
								Console.WriteLine(result.Message);
								AndHUD.Shared.Dismiss();
								Toast.MakeText(this, GetText(Resource.String.Lbl_RequestedVerification), ToastLength.Short)?.Show();
							}
						}
						else Methods.DisplayAndHudErrorResult(this, respond); 
					}
				}
			}
			catch (Exception exception)
			{
				AndHUD.Shared.Dismiss();
				Methods.DisplayReportResultTrack(exception);
			}
		}

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

		private void CloseButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				PathImage = "";
				GlideImageLoader.LoadImage(this, "ImagePlacholder", PassportImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
							Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(PassportImage);
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

				if (requestCode == 108) //Image Picker
				{
					if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
					{
						//Open Image 
						OpenDialogGallery();
					}
					else
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
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

		private async void Get_Data_User()
		{
			try
			{
				if (ListUtils.MyChannelList?.Count == 0)
					await ApiRequest.GetChannelData(this, UserDetails.UserId);

				var local = ListUtils.MyChannelList?.FirstOrDefault();
				if (local != null)
				{
					if (local.Verified == "1")
					{
						VerifiedLinear.Visibility = ViewStates.Visible;
						NotVerifiedLinear.Visibility = ViewStates.Gone;
						TextTitileVerified.Text = GetText(Resource.String.Lbl_WelcomeTo) + " " + AppSettings.ApplicationName;
					}
					else
					{
						VerifiedLinear.Visibility = ViewStates.Gone;
						NotVerifiedLinear.Visibility = ViewStates.Visible;
						TextTitileVerified.Text = GetText(Resource.String.Lbl_Please_select_Image_passport);
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