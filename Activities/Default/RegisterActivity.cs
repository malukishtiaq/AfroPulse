using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
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
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.Default
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class RegisterActivity : SocialLoginBaseActivity
	{
		#region Variables Basic

		private LinearLayout UsernameLayout, EmailLayout, PasswordLayout, ConfirmPasswordLayout, PhoneNumLayout;
		private EditText TxtUsername, TxtEmail, TxtPassword, TxtConfirmPassword, TxtPhoneNum;
		private CheckBox ChkTermsOfUse;
		private TextView TxtTermsOfService;
		private AppCompatButton BtnSignUp;
		private ImageView BackIcon, UsernameIcon, EmailIcon, PasswordIcon, ConfirmPasswordIcon, PhoneNumIcon, ImageShowPass, ImageShowConPass;
		private string GenderStatus = "male";
		private RadioButton RadioMale, RadioFemale;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				// Create your application here
				SetContentView(Resource.Layout.RegisterLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitSocialLogins();
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

		protected override void OnDestroy()
		{
			try
			{
				DestroyBasic();
				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Functions

		private void InitComponent()
		{
			try
			{
				BackIcon = FindViewById<ImageView>(Resource.Id.backArrow);
				BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				BackIcon.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);

				UsernameLayout = FindViewById<LinearLayout>(Resource.Id.UsernameLayout);
				TxtUsername = FindViewById<EditText>(Resource.Id.etUsername);
				UsernameIcon = FindViewById<ImageView>(Resource.Id.imageUsername);

				EmailLayout = FindViewById<LinearLayout>(Resource.Id.EmailLayout);
				TxtEmail = FindViewById<EditText>(Resource.Id.etEmail);
				EmailIcon = FindViewById<ImageView>(Resource.Id.imageEmail);

				PasswordLayout = FindViewById<LinearLayout>(Resource.Id.PasswordLayout);
				TxtPassword = FindViewById<EditText>(Resource.Id.etPassword);
				PasswordIcon = FindViewById<ImageView>(Resource.Id.imagePass);

				ConfirmPasswordLayout = FindViewById<LinearLayout>(Resource.Id.ConfirmPasswordLayout);
				TxtConfirmPassword = FindViewById<EditText>(Resource.Id.etConfirmPassword);
				ConfirmPasswordIcon = FindViewById<ImageView>(Resource.Id.imageConfPass);

				PhoneNumLayout = FindViewById<LinearLayout>(Resource.Id.PhoneNumLayout);
				TxtPhoneNum = FindViewById<EditText>(Resource.Id.etPhoneNum);
				PhoneNumIcon = FindViewById<ImageView>(Resource.Id.imagePhoneNum);

				ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
				ImageShowPass.Tag = "hide";

				ImageShowConPass = FindViewById<ImageView>(Resource.Id.imageShowConfPass);
				ImageShowConPass.Tag = "hide";

				RadioMale = FindViewById<RadioButton>(Resource.Id.radioMale);
				RadioFemale = FindViewById<RadioButton>(Resource.Id.radioFemale);

				ChkTermsOfUse = FindViewById<CheckBox>(Resource.Id.terms_of_use);
				TxtTermsOfService = FindViewById<TextView>(Resource.Id.terms_of_service);

				BtnSignUp = FindViewById<AppCompatButton>(Resource.Id.bntSignUp);

				ToggleVisibility(false);

				Methods.SetColorEditText(TxtUsername, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtEmail, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtPassword, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtConfirmPassword, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtPhoneNum, AppTools.IsTabDark() ? Color.White : Color.Black);

				if (!AppSettings.EnablePhoneNumber)
					PhoneNumLayout.Visibility = ViewStates.Gone;
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
					BackIcon.Click += BackIconOnClick;
					TxtUsername.FocusChange += TxtUsernameOnFocusChange;
					TxtEmail.FocusChange += TxtEmailOnFocusChange;
					TxtPassword.FocusChange += TxtPasswordOnFocusChange;
					TxtConfirmPassword.FocusChange += TxtConfirmPasswordOnFocusChange;
					TxtPhoneNum.FocusChange += TxtPhoneNumOnFocusChange;
					BtnSignUp.Click += BtnSignUpOnClick;
					ImageShowPass.Click += ImageShowPassOnClick;
					ImageShowConPass.Click += ImageShowConPassOnClick;
					TxtTermsOfService.Click += TxtTermsOfServiceOnClick;
					RadioMale.CheckedChange += RadioMaleOnCheckedChange;
					RadioFemale.CheckedChange += RadioFemaleOnCheckedChange;
				}
				else
				{
					BackIcon.Click -= BackIconOnClick;
					TxtUsername.FocusChange -= TxtUsernameOnFocusChange;
					TxtEmail.FocusChange -= TxtEmailOnFocusChange;
					TxtPassword.FocusChange -= TxtPasswordOnFocusChange;
					TxtConfirmPassword.FocusChange -= TxtConfirmPasswordOnFocusChange;
					TxtPhoneNum.FocusChange -= TxtPhoneNumOnFocusChange;
					BtnSignUp.Click -= BtnSignUpOnClick;
					ImageShowPass.Touch -= ImageShowPassOnClick;
					ImageShowConPass.Touch -= ImageShowConPassOnClick;
					TxtTermsOfService.Click -= TxtTermsOfServiceOnClick;
					RadioMale.CheckedChange -= RadioMaleOnCheckedChange;
					RadioFemale.CheckedChange -= RadioFemaleOnCheckedChange;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void DestroyBasic()
		{
			try
			{
				TxtUsername = null;
				TxtEmail = null;
				TxtPassword = null;
				TxtConfirmPassword = null;
				ChkTermsOfUse = null;
				TxtTermsOfService = null;
				BtnSignUp = null;
				GenderStatus = "male";
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void RadioFemaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				bool isChecked = RadioFemale.Checked;
				if (isChecked)
				{
					RadioMale.Checked = false;
					RadioFemale.Checked = true;
					GenderStatus = "female";
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void RadioMaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				bool isChecked = RadioMale.Checked;
				if (isChecked)
				{
					RadioMale.Checked = true;
					RadioFemale.Checked = false;
					GenderStatus = "male";
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void BackIconOnClick(object sender, EventArgs e)
		{
			try
			{
				Finish();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtUsernameOnFocusChange(object sender, View.FocusChangeEventArgs e)
		{
			try
			{
				InitEditTextsIconsColor();
				SetHighLight(e.HasFocus, UsernameLayout, UsernameIcon);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtEmailOnFocusChange(object sender, View.FocusChangeEventArgs e)
		{
			try
			{
				InitEditTextsIconsColor();
				SetHighLight(e.HasFocus, EmailLayout, EmailIcon);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtPasswordOnFocusChange(object sender, View.FocusChangeEventArgs e)
		{
			try
			{
				InitEditTextsIconsColor();
				SetHighLight(e.HasFocus, PasswordLayout, PasswordIcon);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtConfirmPasswordOnFocusChange(object sender, View.FocusChangeEventArgs e)
		{
			try
			{
				InitEditTextsIconsColor();
				SetHighLight(e.HasFocus, ConfirmPasswordLayout, ConfirmPasswordIcon);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtPhoneNumOnFocusChange(object sender, View.FocusChangeEventArgs e)
		{
			try
			{
				InitEditTextsIconsColor();
				SetHighLight(e.HasFocus, PhoneNumLayout, PhoneNumIcon);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		//Show Con Password 
		private void ImageShowConPassOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ImageShowConPass.Tag?.ToString() == "hide")
				{
					ImageShowConPass.SetImageResource(Resource.Drawable.icon_eyes_vector);
					ImageShowConPass.Tag = "show";
					TxtConfirmPassword.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
					TxtConfirmPassword.SetSelection(TxtConfirmPassword.Text.Length);
				}
				else
				{
					ImageShowConPass.SetImageResource(Resource.Drawable.ic_eye_hide);
					ImageShowConPass.Tag = "hide";
					TxtConfirmPassword.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
					TxtConfirmPassword.SetSelection(TxtConfirmPassword.Text.Length);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Show Password 
		private void ImageShowPassOnClick(object sender, EventArgs e)
		{
			try
			{
				if (ImageShowPass.Tag?.ToString() == "hide")
				{
					ImageShowPass.SetImageResource(Resource.Drawable.icon_eyes_vector);
					ImageShowPass.Tag = "show";
					TxtPassword.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
					TxtPassword.SetSelection(TxtPassword.Text.Length);
				}
				else
				{
					ImageShowPass.SetImageResource(Resource.Drawable.ic_eye_hide);
					ImageShowPass.Tag = "hide";
					TxtPassword.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
					TxtPassword.SetSelection(TxtPassword.Text.Length);
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//start login 
		private async void BtnSignUpOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!ChkTermsOfUse.Checked)
				{
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
					return;
				}

				if (!Methods.CheckConnectivity())
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
					return;
				}

				if (string.IsNullOrEmpty(TxtUsername.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")) || string.IsNullOrEmpty(GenderStatus) || string.IsNullOrEmpty(TxtPassword.Text) || string.IsNullOrEmpty(TxtConfirmPassword.Text))
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
					return;
				}

				var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
				if (!check)
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
					return;
				}

				if (TxtPassword.Text != TxtConfirmPassword.Text)
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Your_password_dont_match), GetText(Resource.String.Lbl_Ok));
					return;
				}

				if (AppSettings.EnablePhoneNumber && !Methods.FunString.IsPhoneNumber(TxtPhoneNum.Text))
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_PhoneNumberIsWrong), GetText(Resource.String.Lbl_Ok));
					return;
				}

				HideKeyboard();

				ToggleVisibility(true);

				var (apiStatus, respond) = await RequestsAsync.Auth.RegistrationAsync(TxtEmail.Text.Replace(" ", ""), TxtUsername.Text.Replace(" ", ""), TxtPassword.Text, TxtConfirmPassword.Text, GenderStatus, TxtPhoneNum.Text, UserDetails.DeviceId);
				switch (apiStatus)
				{
					case 200 when respond is RegisterObject result:

						SetDataLogin(result, TxtPassword.Text);

						UserDetails.IsLogin = true;

						StartActivity(new Intent(this, typeof(TabbedMainActivity)));

						ToggleVisibility(false);
						FinishAffinity();
						break;
					case 200:
						{
							if (respond is MessageObject message)
							{
								ToggleVisibility(false);
								Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), message.Message.Contains("We have sent you an email") ? GetString(Resource.String.Lbl_VerifyRegistration) : message.Message, GetText(Resource.String.Lbl_Ok));
							}

							break;
						}
					case 400:
						{
							if (respond is ErrorObject error)
							{
								ToggleVisibility(false);
								Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
							}

							break;
						}
					default:
						ToggleVisibility(false);
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
						break;
				}

			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Open Terms Of Service
		private void TxtTermsOfServiceOnClick(object sender, EventArgs e)
		{
			try
			{
				var url = InitializePlayTube.WebsiteUrl + "/terms/terms";
				new IntentController(this).OpenBrowserFromApp(url);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private void InitEditTextsIconsColor()
		{
			try
			{
				if (TxtUsername.Text != "")
					UsernameIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

				if (TxtEmail.Text != "")
					EmailIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

				if (TxtPassword.Text != "")
					PasswordIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

				if (TxtConfirmPassword.Text != "")
					ConfirmPasswordIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

				if (TxtPhoneNum.Text != "")
					PhoneNumIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void SetHighLight(bool state, LinearLayout layout, ImageView icon)
		{
			try
			{
				if (state)
				{
					layout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
					icon.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
				}
				else
				{
					layout.SetBackgroundResource(Resource.Drawable.new_login_status);
					icon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
		protected override void ToggleVisibility(bool isLoginProgress)
		{
			try
			{
				if (isLoginProgress)
					AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
				else
					AndHUD.Shared.Dismiss();

				BtnSignUp.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SetDataLogin(RegisterObject auth, string password)
		{
			try
			{
				UserDetails.Username = TxtEmail.Text;
				UserDetails.FullName = TxtEmail.Text;
				UserDetails.Password = password;
				UserDetails.AccessToken = Current.AccessToken = auth.User.S;
				UserDetails.UserId = InitializePlayTube.UserId = auth.User.UserId;
				UserDetails.Status = "Active";
				UserDetails.Cookie = auth.User.Cookie;
				UserDetails.Email = TxtEmail.Text;

				//Insert user data to database
				var user = new DataTables.LoginTb
				{
					UserId = UserDetails.UserId,
					AccessToken = UserDetails.AccessToken,
					Cookie = UserDetails.Cookie,
					Username = TxtEmail.Text,
					Password = password,
					Status = "Active",
					Lang = "",
					DeviceId = UserDetails.DeviceId,
				};

				ListUtils.DataUserLoginList.Clear();
				ListUtils.DataUserLoginList.Add(user);

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