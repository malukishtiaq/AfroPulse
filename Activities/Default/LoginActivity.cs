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
using PlayTube.Helpers.Utils;
using System;

namespace PlayTube.Activities.Default
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class LoginActivity : SocialLoginBaseActivity
	{
		#region Variables Basic

		private EditText TxtUsername, TxtPassword;
		private TextView TxtForgotPassword;
		private AppCompatButton BtnLogin;
		private LinearLayout UsernameLayout, PasswordLayout, SignLayout;
		private ImageView BackIcon, UsernameIcon, PasswordIcon, ImageShowPass;

		#endregion

		#region General

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				// Create your application here
				SetContentView(Resource.Layout.LoginLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitSocialLogins();

				if (AppSettings.EnableSmartLockForPasswords)
					BuildClients();
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
				BackIcon = FindViewById<ImageView>(Resource.Id.backArrow);
				BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				BackIcon.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);

				UsernameLayout = FindViewById<LinearLayout>(Resource.Id.UsernameLayout);
				TxtUsername = FindViewById<EditText>(Resource.Id.etUsername);
				UsernameIcon = FindViewById<ImageView>(Resource.Id.imageUsername);

				PasswordLayout = FindViewById<LinearLayout>(Resource.Id.PasswordLayout);
				TxtPassword = FindViewById<EditText>(Resource.Id.etPassword);
				PasswordIcon = FindViewById<ImageView>(Resource.Id.imagePass);

				BtnLogin = FindViewById<AppCompatButton>(Resource.Id.bntLogin);
				TxtForgotPassword = FindViewById<TextView>(Resource.Id.textForgotPassword);
				ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
				ImageShowPass.Tag = "hide";

				SignLayout = FindViewById<LinearLayout>(Resource.Id.SignLayout);

				Methods.SetColorEditText(TxtUsername, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtPassword, AppTools.IsTabDark() ? Color.White : Color.Black);
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
					TxtPassword.FocusChange += TxtPasswordOnFocusChange;
					BtnLogin.Click += BtnLoginOnClick;
					TxtForgotPassword.Click += TxtForgotPasswordOnClick;
					ImageShowPass.Click += ImageShowPassOnClick;
					SignLayout.Click += SignLayoutOnClick;
				}
				else
				{
					BackIcon.Click -= BackIconOnClick;
					TxtUsername.FocusChange -= TxtUsernameOnFocusChange;
					TxtPassword.FocusChange -= TxtPasswordOnFocusChange;
					BtnLogin.Click -= BtnLoginOnClick;
					TxtForgotPassword.Click -= TxtForgotPasswordOnClick;
					ImageShowPass.Click -= ImageShowPassOnClick;
					SignLayout.Click -= SignLayoutOnClick;
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
				TxtPassword = null;
				TxtForgotPassword = null;
				BtnLogin = null;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

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

		//Forgot Password
		private void TxtForgotPasswordOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(ForgetPasswordActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//start login 
		private async void BtnLoginOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
					return;
				}

				if (string.IsNullOrEmpty(TxtUsername.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtPassword.Text))
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
					return;
				}

				HideKeyboard();

				ToggleVisibility(true);

				await AuthApi(TxtUsername.Text.Replace(" ", ""), TxtPassword.Text);
			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
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

		private void SignLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(RegisterActivity)));
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

				if (TxtPassword.Text != "")
					PasswordIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
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

				BtnLogin.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}