//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Base;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Models;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.SettingsPreferences.Security;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Upgrade;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class SettingsActivity : BaseActivity
	{
		#region Variables Basic


		private FrameLayout EditChannalLayout, WithdrawalsLayout, GoProLayout, VerificationLayout, PointsLayout, BlockedUsersLayout, WalletLayout, LinkWithTvCodeLayout;
		private FrameLayout PasswordLayout, TwoFactorLayout, ManageSessionsLayout;
		private FrameLayout ThemeLayout, LanguageLayout, PictureInPictureLayout;

		private Switch SwitchPip;

		private FrameLayout ClearHistoryLayout, PauseWatchHistoryLayout, ClearCacheLayout;
		private Switch SwitchPauseWatchHistory;

		private FrameLayout RateOurAppLayout, InviteFriendsLayout, AboutAppLayout, TermsOfUseLayout, PrivacyPolicyLayout, HelpLayout;
		private FrameLayout DeleteAccountLayout, LogoutLayout;

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
				SetContentView(Resource.Layout.SettingsLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				LoadDataSettings();
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
				EditChannalLayout = FindViewById<FrameLayout>(Resource.Id.layoutEditChannal);
				WithdrawalsLayout = FindViewById<FrameLayout>(Resource.Id.layoutWithdrawals);
				GoProLayout = FindViewById<FrameLayout>(Resource.Id.layoutGoPro);
				VerificationLayout = FindViewById<FrameLayout>(Resource.Id.layoutVerification);
				PointsLayout = FindViewById<FrameLayout>(Resource.Id.layoutPoints);
				BlockedUsersLayout = FindViewById<FrameLayout>(Resource.Id.layoutBlockedUsers);
				WalletLayout = FindViewById<FrameLayout>(Resource.Id.layoutWallet);
				LinkWithTvCodeLayout = FindViewById<FrameLayout>(Resource.Id.layoutLinkWithTvCode);

				PasswordLayout = FindViewById<FrameLayout>(Resource.Id.layoutPassword);
				TwoFactorLayout = FindViewById<FrameLayout>(Resource.Id.layoutTwoFactor);
				ManageSessionsLayout = FindViewById<FrameLayout>(Resource.Id.layoutManageSessions);

				ThemeLayout = FindViewById<FrameLayout>(Resource.Id.layoutTheme);
				LanguageLayout = FindViewById<FrameLayout>(Resource.Id.layoutLanguage);
				PictureInPictureLayout = FindViewById<FrameLayout>(Resource.Id.layoutPictureInPicture);
				SwitchPip = FindViewById<Switch>(Resource.Id.SwitchPIP);

				ClearHistoryLayout = FindViewById<FrameLayout>(Resource.Id.layoutClearHistory);
				PauseWatchHistoryLayout = FindViewById<FrameLayout>(Resource.Id.layoutPauseWatchHistory);
				SwitchPauseWatchHistory = FindViewById<Switch>(Resource.Id.SwitchPauseWatchHistory);
				ClearCacheLayout = FindViewById<FrameLayout>(Resource.Id.layoutClearCache);

				InviteFriendsLayout = FindViewById<FrameLayout>(Resource.Id.layoutInviteFriends);
				RateOurAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutRateOurApp);
				AboutAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutAboutApp);
				TermsOfUseLayout = FindViewById<FrameLayout>(Resource.Id.layoutTermsOfUse);
				PrivacyPolicyLayout = FindViewById<FrameLayout>(Resource.Id.layoutPrivacyPolicy);
				HelpLayout = FindViewById<FrameLayout>(Resource.Id.layoutHelp);

				DeleteAccountLayout = FindViewById<FrameLayout>(Resource.Id.layoutDeleteAccount);
				LogoutLayout = FindViewById<FrameLayout>(Resource.Id.layoutLogout);

				if (!AppSettings.LinkWithTv)
					LinkWithTvCodeLayout.Visibility = ViewStates.Gone;
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
					EditChannalLayout.Click += EditChannalLayoutOnClick;
					WithdrawalsLayout.Click += WithdrawalsLayoutOnClick;
					GoProLayout.Click += GoProLayoutOnClick;
					VerificationLayout.Click += VerificationLayoutOnClick;
					PointsLayout.Click += PointsLayoutOnClick;
					BlockedUsersLayout.Click += BlockedUsersLayoutOnClick;
					WalletLayout.Click += WalletLayoutOnClick;
					LinkWithTvCodeLayout.Click += LinkWithTvCodeLayoutOnClick;
					PasswordLayout.Click += PasswordLayoutOnClick;
					TwoFactorLayout.Click += TwoFactorLayoutOnClick;
					ManageSessionsLayout.Click += ManageSessionsLayoutOnClick;
					ThemeLayout.Click += ThemeLayoutOnClick;
					LanguageLayout.Click += LanguageLayoutOnClick;
					SwitchPip.CheckedChange += SwitchPIPOnCheckedChange;
					ClearHistoryLayout.Click += ClearHistoryLayoutOnClick;
					SwitchPauseWatchHistory.CheckedChange += SwitchPauseWatchHistoryOnCheckedChange;
					ClearCacheLayout.Click += ClearCacheLayoutOnClick;
					RateOurAppLayout.Click += RateOurAppLayoutOnClick;
					InviteFriendsLayout.Click += InviteFriendsLayoutOnClick;
					AboutAppLayout.Click += AboutAppLayoutOnClick;
					TermsOfUseLayout.Click += TermsOfUseLayoutOnClick;
					PrivacyPolicyLayout.Click += PrivacyPolicyLayoutOnClick;
					HelpLayout.Click += HelpLayoutOnClick;
					DeleteAccountLayout.Click += DeleteAccountLayoutOnClick;
					LogoutLayout.Click += LogoutLayoutOnClick;
				}
				else
				{
					// InfoUserContainer.Click -= InfoUserContainerOnClick;
					EditChannalLayout.Click -= EditChannalLayoutOnClick;
					WithdrawalsLayout.Click -= WithdrawalsLayoutOnClick;
					GoProLayout.Click -= GoProLayoutOnClick;
					VerificationLayout.Click -= VerificationLayoutOnClick;
					PointsLayout.Click -= PointsLayoutOnClick;
					BlockedUsersLayout.Click -= BlockedUsersLayoutOnClick;
					WalletLayout.Click -= WalletLayoutOnClick;
					LinkWithTvCodeLayout.Click -= LinkWithTvCodeLayoutOnClick;
					PasswordLayout.Click -= PasswordLayoutOnClick;
					TwoFactorLayout.Click -= TwoFactorLayoutOnClick;
					ManageSessionsLayout.Click -= ManageSessionsLayoutOnClick;
					ThemeLayout.Click -= ThemeLayoutOnClick;
					LanguageLayout.Click -= LanguageLayoutOnClick;
					SwitchPip.CheckedChange -= SwitchPIPOnCheckedChange;
					ClearHistoryLayout.Click -= ClearHistoryLayoutOnClick;
					SwitchPauseWatchHistory.CheckedChange -= SwitchPauseWatchHistoryOnCheckedChange;
					ClearCacheLayout.Click -= ClearCacheLayoutOnClick;
					RateOurAppLayout.Click -= RateOurAppLayoutOnClick;
					InviteFriendsLayout.Click -= InviteFriendsLayoutOnClick;
					AboutAppLayout.Click -= AboutAppLayoutOnClick;
					TermsOfUseLayout.Click -= TermsOfUseLayoutOnClick;
					PrivacyPolicyLayout.Click -= PrivacyPolicyLayoutOnClick;
					HelpLayout.Click -= HelpLayoutOnClick;
					DeleteAccountLayout.Click -= DeleteAccountLayoutOnClick;
					LogoutLayout.Click -= LogoutLayoutOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void EditChannalLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(EditMyChannelActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void WithdrawalsLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(MonetizationActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void GoProLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(GoProActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void VerificationLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(VerificationActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void PointsLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(PointsActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void BlockedUsersLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(BlockedUsersActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void WalletLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(WalletActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LinkWithTvCodeLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(LinkWithTvCodeActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void PasswordLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(PasswordActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TwoFactorLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(TwoFactorAuthActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ManageSessionsLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(ManageSessionsActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LanguageLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				BottomSheetsLanguage bottomSheetsLanguage = new BottomSheetsLanguage();
				bottomSheetsLanguage.Show(SupportFragmentManager, bottomSheetsLanguage.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ThemeLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				BottomSheetsThemes bottomSheetsThemes = new BottomSheetsThemes();
				bottomSheetsThemes.Show(SupportFragmentManager, bottomSheetsThemes.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void SwitchPIPOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				if (e.IsChecked)
				{
					var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
					if (isPro == "0")
					{
						PopupDialogController dialog = new PopupDialogController(this, null, "Upgrade");
						dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_EnableBackgroundWithUpgraded), GetText(Resource.String.Lbl_Ok), GetText(Resource.String.Lbl_Cancel));
						return;
					}

					UserDetails.PipIsChecked = e.IsChecked;

					MainSettings.SharedData?.Edit()?.PutBoolean("picture_in_picture_key", UserDetails.PipIsChecked)?.Commit();

					if (!AppTools.CheckPictureInPictureAllowed(this) && UserDetails.PipIsChecked)
					{
						var intent = new Intent("android.settings.PICTURE_IN_PICTURE_SETTINGS", Android.Net.Uri.Parse("package:" + PackageName));
						StartActivityForResult(intent, 8520);
					}
				}
				else
				{
					UserDetails.PipIsChecked = e.IsChecked;
					MainSettings.SharedData?.Edit()?.PutBoolean("picture_in_picture_key", UserDetails.PipIsChecked)?.Commit();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async void SwitchPauseWatchHistoryOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			try
			{
				if (!UserDetails.IsLogin)
				{
					PopupDialogController dialog = new PopupDialogController(this, null, "Login");
					dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
					return;
				}

				if (Methods.CheckConnectivity())
				{
					var dictionary = new Dictionary<string, string>
					{
						{"settings_type", "pause_history"},
					};

					var (apiResult, respond) = await RequestsAsync.Global.UpdateUserDataGeneralAsync(dictionary);
					if (apiResult == 200)
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);
							var local = ListUtils.MyChannelList?.FirstOrDefault();
							if (local != null)
							{
                                local.PauseHistory = e.IsChecked ? "1" : "0";
                                UserDetails.IsPauseWatchHistory = e.IsChecked;
                                Toast.MakeText(this, UserDetails.IsPauseWatchHistory ? GetText(Resource.String.Lbl_PausedHistory) : GetText(Resource.String.Lbl_ResumeHistory), ToastLength.Long)?.Show();

                                var database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyChannel(local);

                            }
                        }
					}
					else
					{
						Methods.DisplayAndHudErrorResult(this, respond);
					}
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ClearHistoryLayoutOnClick(object sender, EventArgs e)
		{
			try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    return;
                }

                var dialogBuilder = new MaterialAlertDialogBuilder(this);
                dialogBuilder.SetTitle(this.GetText(Resource.String.Lbl_ClearHistory));
                dialogBuilder.SetMessage(this.GetText(Resource.String.Lbl_AreYouSureClearHistory));
                dialogBuilder.SetPositiveButton(this.GetText(Resource.String.Lbl_Yes), (sender, args) =>
                {
                    try
                    {
                        new LibrarySynchronizer(this).RemoveRecentlyWatched();

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.DeleteHistoryVideosAsync() });
                        else
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();

                        var page = TabbedMainActivity.GetInstance()?.LibraryFragment;
                        if (page != null)
                        {
                            page.MAdapterRecently?.VideoList?.Clear();
                            page.MAdapterRecently?.NotifyDataSetChanged();

                            page.RecentlyViewStub.Visibility = ViewStates.Gone;
                        }

                        Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogBuilder.SetNegativeButton(this.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());
                dialogBuilder.Show(); 
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ClearCacheLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				BottomSheetsClearCache bottomSheetsClearCache = new BottomSheetsClearCache();
				bottomSheetsClearCache.Show(SupportFragmentManager, bottomSheetsClearCache.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void InviteFriendsLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				switch ((int)Build.VERSION.SdkInt)
				{
					case < 23:
						{
							var intent = new Intent(this, typeof(InviteFriendsActivity));
							StartActivity(intent);
							break;
						}
					default:
						{
							//Check to see if any permission in our group is available, if one, then all are
							if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) == Permission.Granted)
							{
								var intent = new Intent(this, typeof(InviteFriendsActivity));
								StartActivity(intent);
							}
							else
							{
								new PermissionsController(this).RequestPermission(101);
							}

							break;
						}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void RateOurAppLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StoreReviewApp store = new StoreReviewApp();
				store.OpenStoreReviewPage(this, PackageName);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void AboutAppLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(this, typeof(LocalWebViewActivity));
				intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/terms/about-us");
				intent.PutExtra("Type", GetText(Resource.String.Lbl_about));
				StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void PrivacyPolicyLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(this, typeof(LocalWebViewActivity));
				intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/terms/privacy-policy");
				intent.PutExtra("Type", GetText(Resource.String.Privacy_Policy));
				StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TermsOfUseLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(this, typeof(LocalWebViewActivity));
				intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/terms/terms");
				intent.PutExtra("Type", GetText(Resource.String.Lbl_Termsofuse));
				StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void HelpLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				var intent = new Intent(this, typeof(LocalWebViewActivity));
				intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/contact-us");
				intent.PutExtra("Type", GetText(Resource.String.Lbl_Help));
				StartActivity(intent);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void DeleteAccountLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				StartActivity(new Intent(this, typeof(DeleteAcountActivity)));
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void LogoutLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				BottomSheetsLogOut bottomSheetsLogOut = new BottomSheetsLogOut();
				bottomSheetsLogOut.Show(SupportFragmentManager, bottomSheetsLogOut.Tag);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				if (requestCode == 101 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
				{
					var intent = new Intent(this, typeof(InviteFriendsActivity));
					StartActivity(intent);
				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void LoadDataSettings()
		{
			try
			{
				var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
				if (dataUser != null)
				{
					//GlideImageLoader.LoadImage(this, dataUser.Avatar, ImageUser, ImageStyle.CenterCrop, ImagePlaceholders.DrawableUser);

					//ChannelName.Text = AppTools.GetNameFinal(dataUser);
					////Verified 
					//if (dataUser.Verified == "1")
					//{
					//    IconVerified.Visibility = ViewStates.Visible;
					//    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconVerified, IonIconsFonts.CheckmarkCircle);
					//}
					//else
					//{
					//    IconVerified.Visibility = ViewStates.Gone;
					//}

					//CountSubscribers.Text = dataUser.SubscribeCount + " " + GetText(Resource.String.Lbl_Subscribers);

					//if (!AppSettings.ShowPoints)
					//    CountPoints.Visibility = ViewStates.Invisible;
					//else
					//    CountPoints.Text = dataUser.Points + " " + GetText(Resource.String.Lbl_Points);

					SwitchPauseWatchHistory.Checked = dataUser.PauseHistory == "1";
				}

				//Delete Preference
				//============== Account_Profile ===================

				if (!AppSettings.ShowMonetization)
					WithdrawalsLayout.Visibility = ViewStates.Gone;

				var isPro = dataUser?.IsPro ?? "0";
				if (!AppSettings.ShowGoPro || isPro != "0")
					GoProLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowVerification)
					VerificationLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowPoints)
					PointsLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowBlockedUsers)
					BlockedUsersLayout.Visibility = ViewStates.Gone;

				//============== SecurityAccount ===================

				if (!AppSettings.ShowEditPassword)
					PasswordLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowSettingsTwoFactor)
					TwoFactorLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowSettingsManageSessions)
					ManageSessionsLayout.Visibility = ViewStates.Gone;

				//============== CategoryGeneral ===================

				if ((int)Build.VERSION.SdkInt <= 23 || !AppSettings.EnablePictureToPictureMode)
					PictureInPictureLayout.Visibility = ViewStates.Gone;

				//============== History_Privacy ===================

				if (!AppSettings.ShowClearHistory)
					ClearHistoryLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowClearCache)
					ClearCacheLayout.Visibility = ViewStates.Gone;

				//============== Support ===================

				if (!AppSettings.ShowSettingsRateApp)
					RateOurAppLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowAbout)
					AboutAppLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowHelp)
					HelpLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowTermsOfUse)
					TermsOfUseLayout.Visibility = ViewStates.Gone;

				if (!AppSettings.ShowDeleteAccount)
					DeleteAccountLayout.Visibility = ViewStates.Gone;

				SwitchPip.Checked = UserDetails.PipIsChecked;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

	}
}