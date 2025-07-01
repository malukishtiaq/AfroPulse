using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Util.Concurrent;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Credentials;
using Com.Facebook;
using Com.Facebook.Login;
using Java.Util.Concurrent;
using Newtonsoft.Json;
using Org.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.SocialLogins;
using PlayTube.Helpers.Utils;
using PlayTube.Library.OneSignalNotif;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Auth;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.GoogleAndroid.Libraries.Identity.GoogleId;
using AccessToken = Com.Facebook.AccessToken;
using Credential = AndroidX.Credentials.Credential;
using Exception = System.Exception;
using GetCredentialRequest = AndroidX.Credentials.GetCredentialRequest;
using GetCredentialResponse = AndroidX.Credentials.GetCredentialResponse;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace PlayTube.Activities.Default
{
	[Activity]
	public abstract class SocialLoginBaseActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, ICredentialManagerCallback
	{
		#region Variables Basic

		private LinearLayout BntLoginWowonder, BntLoginGoogle, BntLoginFacebook;
		private TextView ContinueWithText;
		private ICallbackManager MFbCallManager;
		private FbMyProfileTracker ProfileTracker;
		public static ICredentialManager CredentialManager;
		public static SocialLoginBaseActivity Instance;

		#endregion

		#region General 

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);

				InitializePlayTube.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnSecurityProtocolType3072On, new MyReportModeApp());

				//Set Full screen 
				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
				Window?.SetSoftInputMode(SoftInput.AdjustResize);

				Methods.App.FullScreenApp(this);

				Instance = this;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
				{
					if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
					{
						if (string.IsNullOrEmpty(UserDetails.DeviceId))
							OneSignalNotification.Instance.RegisterNotificationDevice(this);
					}
					else
					{
						RequestPermissions(new[]
						{
							Manifest.Permission.PostNotifications
						}, 16248);
					}
				}
				else
				{
					if (string.IsNullOrEmpty(UserDetails.DeviceId))
						OneSignalNotification.Instance.RegisterNotificationDevice(this);
				}

				if (Methods.CheckConnectivity())
					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Functions

		public void InitSocialLogins()
		{
			try
			{
				//#Facebook
				if (AppSettings.ShowFacebookLogin)
				{
					//LoginButton loginButton = new LoginButton(this);
					ProfileTracker = new FbMyProfileTracker();
					ProfileTracker.StartTracking();

					BntLoginFacebook = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
					BntLoginFacebook.Visibility = ViewStates.Visible;
					BntLoginFacebook.Click += BntLoginFacebookOnClick;

					ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
					//loginButton.SetPermissions(new string[]
					//{
					//    "email",
					//    "public_profile"
					//});

					MFbCallManager = ICallbackManager.Factory.Create();
					LoginManager.Instance.RegisterCallback(MFbCallManager, this);

					//FB accessToken
					var accessToken = AccessToken.CurrentAccessToken;
					var isLoggedIn = accessToken != null && !accessToken.IsExpired;
					if (isLoggedIn && Profile.CurrentProfile != null)
					{
						LoginManager.Instance.LogOut();
					}

					string hashId = Methods.App.GetKeyHashesConfigured(this);
					Console.WriteLine(hashId);
				}
				else
				{
					BntLoginFacebook = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
					BntLoginFacebook.Visibility = ViewStates.Gone;
				}

				//#Google
				if (AppSettings.ShowGoogleLogin)
				{
					BntLoginGoogle = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
					BntLoginGoogle.Click += GoogleSignInButtonOnClick;
				}
				else
				{
					BntLoginGoogle = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
					BntLoginGoogle.Visibility = ViewStates.Gone;
				}

				//#WoWonder 
				if (AppSettings.ShowWoWonderLogin)
				{
					BntLoginWowonder = FindViewById<LinearLayout>(Resource.Id.bntLoginWowonder);
					var txtWowonder = FindViewById<TextView>(Resource.Id.txtWowonder);
					BntLoginWowonder.Click += BntLoginWowonderOnClick;

					txtWowonder.Text = GetString(Resource.String.Lbl_LoginWith) + " " + AppSettings.AppNameWoWonder;
					BntLoginWowonder.Visibility = ViewStates.Visible;
				}
				else
				{
					BntLoginWowonder = FindViewById<LinearLayout>(Resource.Id.bntLoginWowonder);
					BntLoginWowonder.Visibility = ViewStates.Gone;
				}

				ContinueWithText = FindViewById<TextView>(Resource.Id.ContinueWithText);
				if (!AppSettings.ShowFacebookLogin && AppSettings.ShowGoogleLogin && AppSettings.ShowWoWonderLogin)
					ContinueWithText.Visibility = ViewStates.Gone;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		//Event Click login using PlayTube
		protected void BntLoginWowonderOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(WoWonderLoginActivity)));
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}

		private void BntLoginFacebookOnClick(object sender, EventArgs e)
		{
			try
			{
				LoginManager.Instance.LogInWithReadPermissions(this, new List<string>
				{
					"email",
					"public_profile"
				});
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Abstract members

		protected abstract void ToggleVisibility(bool isLoginProgress);

		#endregion

		#region Social Logins

		private string FbAccessToken, GAccessToken, GServerCode;

		#region Facebook

		public void OnCancel()
		{
			try
			{
				ToggleVisibility(false);

				//SetResult(Result.Canceled);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnError(FacebookException error)
		{
			try
			{

				ToggleVisibility(false);

				// Handle exception
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

				//SetResult(Result.Canceled);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnSuccess(Java.Lang.Object result)
		{
			try
			{
				//var loginResult = result as LoginResult;
				//var id = AccessToken.CurrentAccessToken.UserId;

				ToggleVisibility(false);

				//SetResult(Result.Ok);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public async void OnCompleted(JSONObject json, GraphResponse response)
		{
			try
			{
				ToggleVisibility(true);

				var accessToken = AccessToken.CurrentAccessToken;
				if (accessToken != null)
				{
					FbAccessToken = accessToken.Token;

					//Login Api 
					var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
					if (apiStatus == 200)
					{
						if (respond is LoginObject auth)
						{
							if (auth.data != null)
							{
								if (!string.IsNullOrEmpty(json?.ToString()))
								{
									var data = json.ToString();
									var result = JsonConvert.DeserializeObject<FacebookResult>(data);
								}

								SetDataLogin(auth, "", "");

								UserDetails.IsLogin = true;

								StartActivity(new Intent(this, typeof(TabbedMainActivity)));

								ToggleVisibility(false);
								FinishAffinity();
							}
						}
					}
					else if (apiStatus == 400)
					{
						if (respond is ErrorObject error)
						{
							ToggleVisibility(false);
							Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
								error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
						}
					}
					else
					{
						ToggleVisibility(false);
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
							respond.ToString(), GetText(Resource.String.Lbl_Ok));
					}
				}
				else
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
				}
			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
		{
			try
			{
				if (e.MProfile != null)
					try
					{
						//var FbFirstName = e.MProfile.FirstName;
						//var FbLastName = e.MProfile.LastName;
						//var FbName = e.MProfile.Name;
						//var FbProfileId = e.MProfile.Id;

						var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
						var parameters = new Bundle();
						parameters.PutString("fields", "id,name,link,age_range,email");
						request.Parameters = parameters;
						request.ExecuteAndWait();
					}
					catch (Java.Lang.Exception ex)
					{
						Methods.DisplayReportResultTrack(ex);
					}
				//else
				//    Toast.MakeText(this, GetString(Resource.String.Lbl_Null_Data_User), ToastLength.Short)?.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
		#endregion

		//======================================================

		#region Google

		//Event Click login using google
		private void GoogleSignInButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				GetGoogleIdOption googleIdOption = new GetGoogleIdOption.Builder()
					.SetFilterByAuthorizedAccounts(false)
					.SetServerClientId(AppSettings.ClientId)
					//.SetAutoSelectEnabled(false) 
					.Build();

				GetCredentialRequest request = new GetCredentialRequest.Builder()
					.AddCredentialOption(googleIdOption)
					.Build();

				CancellationSignal cancellationSignal = new CancellationSignal();
				CredentialManager ??= ICredentialManager.Create(this);
				IExecutor executor = ContextCompat.GetMainExecutor(this);

				CredentialManager.GetCredentialAsync(this, request, cancellationSignal, executor, this);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async void SetContentGoogle(string gAccessToken)
		{
			try
			{
				//Successful log in hooray!!
				if (!string.IsNullOrEmpty(gAccessToken))
				{
					ToggleVisibility(true);

					var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
					if (apiStatus == 200)
					{
						if (respond is LoginObject auth)
						{
							if (auth.data != null)
							{
								SetDataLogin(auth, "", "");

								UserDetails.IsLogin = true;

								StartActivity(new Intent(this, typeof(TabbedMainActivity)));

								ToggleVisibility(false);
								FinishAffinity();
							}
						}
					}
					else if (apiStatus == 400)
					{
						if (respond is ErrorObject error)
						{
							ToggleVisibility(false);
							Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
						}
						else if (respond is ErrorsGoogleObject errorsGoogle)
						{
							ToggleVisibility(false);
							Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorsGoogle.Errors.ErrorText.Message, GetText(Resource.String.Lbl_Ok));
						}
					}
					else
					{
						ToggleVisibility(false);
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
					}
				}
			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void OnError(Object result)
		{
			try
			{
				Toast.MakeText(this, result?.ToString(), ToastLength.Short)?.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public async void OnResult(Object result)
		{
			try
			{
				if (result is GetCredentialResponse response)
				{
					Credential credential = response.Credential;
					if (credential is CustomCredential customCredential)
					{
						if (customCredential.Type == GoogleIdTokenCredential.TypeGoogleIdTokenCredential)
						{
							GoogleIdTokenCredential googleIdTokenCredential = GoogleIdTokenCredential.CreateFrom(credential.Data);

							if (googleIdTokenCredential != null)
							{
								string email = googleIdTokenCredential.Id;
								string firstName = googleIdTokenCredential.GivenName;
								string lastName = googleIdTokenCredential.FamilyName;
								string token = googleIdTokenCredential.IdToken;
								SetContentGoogle(token);
							}
						}
					}
					else if (credential is PasswordCredential passwordCredential)
					{
						HideKeyboard();

						ToggleVisibility(true);
						await AuthApi(passwordCredential.Id, passwordCredential.Password);
					}
				}
				else if (result is CreatePublicKeyCredentialResponse credentialResponse)
				{

				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		//======================================================

		#region WoWonder

		public async void LoginWoWonder(string woWonderAccessToken)
		{
			try
			{
				ToggleVisibility(true);

				if (!string.IsNullOrEmpty(woWonderAccessToken))
				{
					//Login Api 
					(int apiStatus, var respond) = await RequestsAsync.Auth.SocialLoginAsync(woWonderAccessToken, "wowonder", UserDetails.DeviceId);
					if (apiStatus == 200)
					{
						if (respond is LoginObject auth)
						{
							SetDataLogin(auth, "", "");
							ToggleVisibility(false);

							UserDetails.IsLogin = true;

							StartActivity(new Intent(this, typeof(TabbedMainActivity)));
							FinishAffinity();
						}
					}
					else if (apiStatus == 400)
					{
						if (respond is ErrorObject error)
						{
							Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
						}

						ToggleVisibility(false);
					}
					else if (apiStatus == 404)
					{
						ToggleVisibility(false);
						Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
					}
				}
				else
				{
					ToggleVisibility(false);
					Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
				}
			}
			catch (Exception exception)
			{
				ToggleVisibility(false);
				Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#endregion

		#region Permissions && Result 

		//Result
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				// Logins Facebook
				MFbCallManager?.OnActivityResult(requestCode, (int)resultCode, data);
				base.OnActivityResult(requestCode, resultCode, data);
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

				if (requestCode == 16248 && (grantResults.Length > 0 && grantResults[0] == Permission.Granted))
				{
					if (string.IsNullOrEmpty(UserDetails.DeviceId))
						OneSignalNotification.Instance.RegisterNotificationDevice(this);
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Cross App Authentication

		public void BuildClients()
		{
			try
			{
				GetPasswordOption getPasswordOption = new GetPasswordOption();

				GetCredentialRequest getCredRequest = new GetCredentialRequest.Builder()
					.AddCredentialOption(getPasswordOption)
				.Build();

				CredentialManager ??= ICredentialManager.Create(this);

				CredentialManager.GetCredentialAsync(this, getCredRequest, new CancellationSignal(), new HandlerExecutor(Looper.MainLooper), this);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		protected void HideKeyboard()
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

		public async Task AuthApi(string username, string password)
		{
			try
			{
				var (apiStatus, respond) = await RequestsAsync.Auth.RequestLoginAsync(username, password, UserDetails.DeviceId);
				switch (apiStatus)
				{
					case 200 when respond is LoginObject result:

						SetDataLogin(result, username, password);

						UserDetails.IsLogin = true;

						StartActivity(new Intent(this, typeof(TabbedMainActivity)));

						ToggleVisibility(false);
						FinishAffinity();
						break;
					case 200:
						{
							if (respond is AuthMessageObject messageObject)
							{
								UserDetails.Username = username;
								//UserDetails.FullName = MEditTextUsername.Text;
								UserDetails.Password = password;
								UserDetails.UserId = messageObject.UserId;
								UserDetails.Status = "Pending";
								UserDetails.Email = username;

								//Insert user data to database
								var user = new DataTables.LoginTb
								{
									UserId = UserDetails.UserId,
									AccessToken = "",
									Cookie = "",
									Username = username,
									Password = password,
									Status = "Pending",
									Lang = "",
									DeviceId = UserDetails.DeviceId,
								};
								ListUtils.DataUserLoginList.Add(user);

								//var dbDatabase = new SqLiteDatabase();
								// dbDatabase.InsertOrUpdateLogin_Credentials(user);

								StartActivity(new Intent(this, typeof(VerificationCodeActivity)));
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

		private void SetDataLogin(LoginObject auth, string username, string password)
		{
			try
			{
				UserDetails.Username = username;
				UserDetails.FullName = username;
				UserDetails.Password = password;
				UserDetails.AccessToken = Current.AccessToken = auth.data.SessionId;
				UserDetails.UserId = InitializePlayTube.UserId = auth.data.UserId;
				UserDetails.Status = "Active";
				UserDetails.Cookie = auth.data.Cookie;
				UserDetails.Email = username;

				//Insert user data to database
				var user = new DataTables.LoginTb
				{
					UserId = UserDetails.UserId,
					AccessToken = UserDetails.AccessToken,
					Cookie = UserDetails.Cookie,
					Username = username,
					Password = password,
					Status = "Active",
					Lang = "",
					DeviceId = UserDetails.DeviceId,
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