using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Auth;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;

namespace PlayTube.Activities.Default
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class ForgetPasswordActivity : AppCompatActivity
	{
		#region Variables Basic

		private LinearLayout EmailLayout;
		private ImageView BackIcon, EmailIcon;
		private AppCompatButton BtnSend;
		private EditText TxtEmail;

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
				SetContentView(Resource.Layout.ForgetPasswordLayout);

				//Get Value And Set Toolbar
				InitComponent();
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

		#region Functions

		private void InitComponent()
		{
			try
			{
				BackIcon = FindViewById<ImageView>(Resource.Id.backArrow);
				BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
				BackIcon.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);

				EmailLayout = FindViewById<LinearLayout>(Resource.Id.EmailLayout);
				TxtEmail = FindViewById<EditText>(Resource.Id.etEmail);
				EmailIcon = FindViewById<ImageView>(Resource.Id.imageEmail);

				BtnSend = FindViewById<AppCompatButton>(Resource.Id.SendButton);

				Methods.SetColorEditText(TxtEmail, AppTools.IsTabDark() ? Color.White : Color.Black);
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
					TxtEmail.FocusChange += TxtEmailOnFocusChange;
					BtnSend.Click += BtnSendOnClick;
				}
				else
				{
					BackIcon.Click -= BackIconOnClick;
					TxtEmail.FocusChange -= TxtEmailOnFocusChange;
					BtnSend.Click -= BtnSendOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

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

		private async void BtnSendOnClick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (!string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")))
				{
					if (Methods.CheckConnectivity())
					{
						var check = Methods.FunString.IsEmailValid(TxtEmail.Text);
						if (!check)
						{
							Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
						}
						else
						{
							HideKeyboard();

							ToggleVisibility(true);

							var (apiStatus, respond) = await RequestsAsync.Auth.ForgetPasswordAsync(TxtEmail.Text.Replace(" ", ""));
							if (apiStatus == 200)
							{
								if (respond is ResetPasswordObject result)
								{
									ToggleVisibility(false);
									Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationCorrect), GetText(Resource.String.Lbl_PasswordSent), GetText(Resource.String.Lbl_Ok));
								}
							}
							else if (apiStatus == 400)
							{
								if (respond is ErrorObject error)
								{
									ToggleVisibility(false);
									Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
								}
							}
							else
							{
								ToggleVisibility(false);
								Methods.DisplayReportResult(this, respond);
							}
						}
					}
					else
					{
						ToggleVisibility(false);
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
					}
				}
			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), exception.ToString(), GetText(Resource.String.Lbl_Ok));
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		private void InitEditTextsIconsColor()
		{
			try
			{
				if (TxtEmail.Text != "")
					EmailIcon.ImageTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(this, AppTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
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
		private void HideKeyboard()
		{
			try
			{
				var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
				inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ToggleVisibility(bool isLoginProgress)
		{
			try
			{
				if (isLoginProgress)
					AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
				else
					AndHUD.Shared.Dismiss();

				BtnSend.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

	}
}