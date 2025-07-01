//###############################################################
// Author >> Elin Doughouz
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
// Doc https://doughouzlight.com/?onepage-docs=playtube-andorid
//=========================================================

using PlayTube.Helpers.Models;
using System.Collections.Generic;
///AfroPulse
///Starion@55
namespace PlayTube
{
	internal static class AppSettings
	{
		/// <summary>
		/// Deep Links To App Content
		/// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
		/// <string name="ApplicationUrlWeb">demo.playtubescript.com</string>
		/// </summary>
		public static readonly string TripleDesAppServiceProvider = "wlfSXGES7kB9X9kThx4eEhM9snuPNrx4QvWLcQ+HkUgVuk1XZbDzcFFpxjhpX1gD4UuvRfIQY/s+wUfDR0UqAAOnt2kviehGxIn4WyDOLa0MMf9yN+7HKHbWdNoMHRFEE1f3BJutfu2F+UP3fDlgiHiapDIktM9quxprom4zJzB2Kk3J0+PoBQ43yARq1bCayL7Gk7EAGZEfT+J+EU3ioOqj5ZF+bC5POrphELlMQ31ARMUy1a21lv85eU1juanjZExJSADDaEVsFqltutmpYf+ltufmFXUnXuFibiRQ1ZpF4ZUvhW78FJ87JERQvA2wrZvTDARjeCYmD0yF/Cqjo5O0OWc4yMDcfPSDmPmgRw156tWbXq9FnGTUSEFGR55FoVitqQuBkJAvgjt0ox9Dp1sMxDdf1jKxBPP8xLjMi27iUk0Ntqje90Xj12pvWQ1LauN2bsG5y/IgM3822kGK3uW0fkn1rIZUYK7RA6iGQu2o8CZ0DReiQENZrcu8g8EqMc4x8aE9xaetFW0z9JZIep5dkLxhucqFMX9QeG1yH6dkG80z1qr50BgnHIenzlCCYUzicDcs9DfeOF0vqV1zkMjqgrCe1ui7o5PyemmXYhHyQz1g8S0I95oGEz8cwiiA/NbZ2d7iuNEezJDoXt+LSJ0RfT3K/169psCGLCVQP0EMIrX3nKjlyFNoxlQTPLBvUPUmQW9j173RIUcw0qq1xQDktc5P7whfYXQ21g7ao/7iLH+O1O3Ebz1ucCifJ/2oY/R3N4Cf66x/anlwisH3MLIqvyOGrjevQd+vrME0ySd0U2uopc13TetFMPXgrAlwSZ1lXg6IiDUPtEqukS98TrAdBNIBaCOf7Y32mQ8rfsNRY59UwGdbbYrUofhwCu3s8Crxp4o7kYjdXsMxjHIeMz7PDg0px3aEZIZ/Ypn7BXkpZmGWpFCNOKENEHkUpiftn19rs9yVAA0JffR8mQBK0fXrMvAZGdoAS2EDID57F/xsQBe1g92jRlAcO+i5wXI4nMTqisiKMcCFdwVlsSd/qbxCW7YDlvq/M8FBQz8dtHvCojS9G05OvCDGI2fVGP+x09emvsY3wWPP+BZPH3cMaF6UAktFjL2fSbH5JIS3s+iAVv9/+TIJombCx+dVV1qzxpxZIbC4D2YQLLBdCE/JKc/jtKqPBG+f2zMrflI9+Bh23e9ScFjjXdK6qfsLVOxkBeeqrD0qSgTLqegGRM2gKtfkDTS0tHuYCraS3Q16CYE0eHphv3Pfap82SpyiBfY4cim2r7t1fZ9vHnHg4J9fwGRgM5to2GGx+rCsVhrCyTAnYc0TXpAKPND8ADYZqVn93xR5KUWPL6rvAu7FJps0AZ9U1BzWgtJbx4Ll7hI6pAefLVWnFOAWTiG/elISnU9F+fd5Lq0pGyMT/ERdCN6oYgJpCjN3SJVTBWzd7H4fvAWbnkF5HblV1r2f3IjKJ+3zTlafIMWw1Pnqx8QqanaSsdp0gmISX/1EPB8uGW5mUxWxltweF4ZJ7ggS8EgBPF96opbZHs7rqOZHKGHMDpPIB/sboWX/AgD8oW/IEpsaXtA420jLP0nMpl4V/OAP5AanPMZvMe++q7nM8tnjpISPIOLvpAa/vnq5J5YabHFyDpWwPYcSbng9/unk06XPDJvAbAawLcfjsUvDSCTLvaa4rZIdRwr73iFLuwCCpkykIXtS5QACLaedWHy2kyhZLEJlIvyBZjwpwee8qcOB6Jr+LhZbcX6oOhiRhhKRui1ctnf5Il/3Mj3MFb3lXvyQyU89L83iQffVRC0grTOs+vqZQRgYfN7V0xiv/9m+jIqSAws=";

		//Main Settings >>>>> 
		//********************************************************* 
		public static readonly string ApplicationName = "Afro Pulse";
		public static readonly string DatabaseName = "AfroPulse";
		public static string Version = "3.9";

		//Main Colors >>
		//*********************************************************
		public static readonly string MainColor = "#ca0e07";

		public static readonly PlayerTheme PlayerTheme = PlayerTheme.Theme2;

		//Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
		//*********************************************************
		public static bool FlowDirectionRightToLeft = false;
		public static string Lang = ""; //Default language ar

		//Set Language User on site from phone 
		public static readonly bool SetLangUser = false;

		public static readonly Dictionary<string, string> LanguageList = new Dictionary<string, string>()
		{
			{"en", "English"},
			{"ar", "Arabic"},
		};

		//Error Report Mode
		//*********************************************************
		public static readonly bool SetApisReportMode = true;

		//Notification Settings >>
		//*********************************************************
		public static readonly bool ShowNotification = false;
		public static string OneSignalAppId = "e06a3585-d0ac-44ef-b2df-0c24abc23682";

		//Tv Settings >>
		//*********************************************************
		public static readonly bool LinkWithTv = true;

		//AdMob >> Please add the code ads in the Here and analytic.xml 
		//*********************************************************
		public static readonly ShowAds ShowAds = ShowAds.AllUsers;

		public static readonly bool RewardedAdvertisingSystem = true;

		//Three times after entering the ad is displayed
		public static readonly int ShowAdInterstitialCount = 3;
		public static readonly int ShowAdRewardedVideoCount = 3;
		public static readonly int ShowAdNativeCount = 4;
		public static readonly int ShowAdAppOpenCount = 2;

		public static readonly bool ShowAdMobBanner = false;
		public static readonly bool ShowAdMobInterstitial = false;
		public static readonly bool ShowAdMobRewardVideo = false;
		public static readonly bool ShowAdMobNative = false;
		public static readonly bool ShowAdMobAppOpen = false;
		public static readonly bool ShowAdMobRewardedInterstitial = false;

		public static readonly string AdInterstitialKey = "ca-app-pub-5135691635931982/6168068662";
		public static readonly string AdRewardVideoKey = "ca-app-pub-5135691635931982/4663415300";
		public static readonly string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2619721801";
		public static readonly string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/4967593321";
		public static readonly string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/1850136085";

		//FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
		//*********************************************************
		public static readonly bool ShowFbBannerAds = false;
		public static readonly bool ShowFbInterstitialAds = false;
		public static readonly bool ShowFbRewardVideoAds = false;
		public static readonly bool ShowFbNativeAds = false;

		//YOUR_PLACEMENT_ID
		public static readonly string AdsFbBannerKey = "250485588986218_554026418632132";
		public static readonly string AdsFbInterstitialKey = "250485588986218_554026125298828";
		public static readonly string AdsFbRewardVideoKey = "250485588986218_554072818627492";
		public static readonly string AdsFbNativeKey = "250485588986218_554706301897477";

		//Ads AppLovin >> Please add the code ad in the Here 
		//*********************************************************  
		public static readonly bool ShowAppLovinBannerAds = false;
		public static readonly bool ShowAppLovinInterstitialAds = false;
		public static readonly bool ShowAppLovinRewardAds = false;

		public static readonly string AdsAppLovinBannerId = "f9ebf067458aa1df";
		public static readonly string AdsAppLovinInterstitialId = "bd6fa0d996c6fceb";
		public static readonly string AdsAppLovinRewardedId = "d3269ba46c446f63";
		//********************************************************* 

		//Social Logins >>
		//If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
		//Facebook >> ../values/analytic.xml  
		//Google >> ../Properties/AndroidManifest.xml .. line 27
		//*********************************************************
		public static readonly bool EnableSmartLockForPasswords = false;

		public static readonly bool ShowFacebookLogin = false;
		public static readonly bool ShowGoogleLogin = false;
		public static readonly bool ShowWoWonderLogin = false;

		public static readonly string AppNameWoWonder = "WoWonder";
		public static readonly string WoWonderDomainUri = "https://demo.wowonder.com";
		public static readonly string WoWonderAppKey = "35bf23159ca898e246e5e84069f4deba1b81ee97-60b93b3942f269c7a29a1760199642ec-46595136";

		public static readonly string ClientId = "404363570731-j48d139m31tgaq2tj0gamg8ah430botj.apps.googleusercontent.com";

		//First Page
		//*********************************************************
		public static readonly bool ShowSkipButton = true;

		public static readonly bool ShowRegisterButton = true;
		public static readonly bool EnablePhoneNumber = false;

		//Set Theme Full Screen App
		//*********************************************************
		public static readonly bool EnableFullScreenApp = false;
		public static readonly bool EnablePictureToPictureMode = true; //>> Not Working >> Next update 

		//Data Channal Users >> About
		//*********************************************************
		public static readonly bool ShowEmailAccount = true;
		public static readonly bool ShowActivities = true;

		//Tab >> 
		//*********************************************************
		public static readonly bool ShowArticle = true;
		public static readonly bool ShowMovies = true;
		public static readonly bool ShowShorts = true;
		public static readonly bool ShowChannelPopular = true;

		//how in search 
		public static readonly List<string> LastKeyWordList = new List<string>() { "Music", "Party", "Nature", "Snow", "Entertainment", "Holidays", "Comedy", "Politics", "Suspense" };

		//Offline Watched Videos >>  
		//*********************************************************
		public static readonly bool AllowOfflineDownload = true;
		public static readonly bool AllowDownloadProUser = true;
		public static readonly bool AllowWatchLater = true;
		public static readonly bool AllowRecentlyWatched = true;
		public static readonly bool AllowPlayLists = true;
		public static readonly bool AllowLiked = true;
		public static readonly bool AllowShared = true;
		public static readonly bool AllowPaid = true;

		//Import && Upload Videos >>  
		//*********************************************************
		public static bool ShowButtonImport { get; set; } = true;
		public static bool ShowButtonUpload { get; set; } = true;

		//Last_Messages Page >>
		///********************************************************* 
		public static readonly bool RunSoundControl = true;
		public static readonly int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
		public static readonly int MessageRequestSpeed = 3000; // 3 Seconds

		public static readonly int ShowButtonSkip = 10; // 6 Seconds 

		//Set Theme App >> Color - Tab
		//*********************************************************
		public static TabTheme SetTabDarkTheme = TabTheme.SameAsPhone;

		public static readonly bool SetYoutubeTypeBadgeIcon = true;
		public static readonly bool SetVimeoTypeBadgeIcon = false;
		public static readonly bool SetDailyMotionTypeBadgeIcon = false;
		public static readonly bool SetTwichTypeBadgeIcon = false;
		public static readonly bool SetOkTypeBadgeIcon = false;
		public static readonly bool SetFacebookTypeBadgeIcon = false;

		//Bypass Web Errors 
		///*********************************************************
		public static readonly bool TurnTrustFailureOnWebException = true;
		public static readonly bool TurnSecurityProtocolType3072On = true;


		public static readonly int AvatarSize = 60;
		public static readonly int ImageSize = 400;

		//Home Page 
		//*********************************************************
		public static readonly bool ShowStockVideo = true;

		public static readonly int CountVideosTop = 10;
		public static readonly int CountVideosLatest = 10;
		public static readonly int CountVideosFav = 10;
		public static readonly int CountVideosLive = 13;
		public static readonly int CountVideosStock = 10;

		/// <summary>
		/// if Radius you can select how much Radius in the parameter #CardPlayerViewRadius
		/// </summary>
		public static readonly CardPlayerView CardPlayerView = CardPlayerView.Square;
		public static readonly float CardPlayerViewRadius = 10F;

		public static readonly bool ShowGoLive = true;
		public static readonly string AppIdAgoraLive = "9471c47b589c4a35abf3f7338ef18629";

		public static readonly ShareSystem ShareSystem = ShareSystem.WebsiteUrl;

		//Settings 
		//*********************************************************
		public static readonly bool ShowEditPassword = true;
		public static readonly bool ShowMonetization = true; //(Withdrawals)
		public static readonly bool ShowVerification = true;
		public static readonly bool ShowBlockedUsers = true;
		public static readonly bool ShowPoints = true;
		public static readonly bool ShowSettingsTwoFactor = true;
		public static readonly bool ShowSettingsManageSessions = true;

		public static readonly bool ShowSettingsRateApp = true;
		public static readonly int ShowRateAppCount = 5;

		public static readonly bool ShowSettingsUpdateManagerApp = false;

		public static readonly bool ShowGoPro = true;
		public static readonly double AmountGoPro = 10;

		public static readonly bool ShowClearHistory = true;
		public static readonly bool ShowClearCache = true;

		public static readonly bool ShowHelp = true;
		public static readonly bool ShowTermsOfUse = true;
		public static readonly bool ShowAbout = true;
		public static readonly bool ShowDeleteAccount = true;

		//*********************************************************
		/// <summary>
		/// Currency
		/// CurrencyStatic = true : get currency from app not api 
		/// CurrencyStatic = false : get currency from api (default)
		/// </summary>
		public static readonly bool CurrencyStatic = false;
		public static readonly string CurrencyIconStatic = "$";
		public static readonly string CurrencyCodeStatic = "USD";


		//********************************************************* 
		public static readonly bool RentVideosSystem = true;

		//*********************************************************  
		public static readonly bool DonateVideosSystem = true;

		//*********************************************************  
		/// <summary>
		/// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
		/// 
		/// Add info keys in Payment Methods : https://prnt.sc/1z5bffc https://prnt.sc/1z5b0yj
		/// To find your merchant ID :  https://prnt.sc/1z59dy8
		///
		/// Tokenization Keys : https://prnt.sc/1z59smv
		/// </summary>
		public static readonly bool ShowPaypal = true;
		public static readonly string MerchantAccountId = "tester";

		public static readonly string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4ccmn4t7*******";
		public static readonly string ProductionTokenizationKey = "production_t2wns2y2_dfy45******";

		public static readonly bool ShowCreditCard = false;
		public static readonly bool ShowBankTransfer = true;

		/// <summary>
		/// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
		/// <uses-permission android:name="com.android.vending.BILLING" />
		/// </summary>
		public static readonly bool ShowInAppBilling = false;

		//*********************************************************   
		public static readonly bool ShowCashFree = false;

		/// <summary>
		/// Currencies : http://prntscr.com/u600ok
		/// </summary>
		public static readonly string CashFreeCurrency = "INR";

		/// <summary>
		/// Currencies : https://razorpay.com/accept-international-payments
		/// </summary>
		public static readonly string RazorPayCurrency = "INR";

		/// <summary>
		/// If you want RazorPay you should change id key in the analytic.xml file
		/// razorpay_api_Key >> .. line 18 
		/// </summary>
		public static readonly bool ShowRazorPay = false;

		public static readonly bool ShowPayStack = false;
		public static readonly bool ShowPaySera = false;
		public static readonly bool ShowSecurionPay = false;
		public static readonly bool ShowAuthorizeNet = false;
		public static readonly bool ShowIyziPay = false;
		public static readonly bool ShowAamarPay = false;

		/// <summary>
		/// FlutterWave get Api Keys From https://app.flutterwave.com/dashboard/settings/apis/live
		/// </summary>
		public static readonly bool ShowFlutterWave = true;
		public static readonly string FlutterWaveCurrency = "NGN";
		public static readonly string FlutterWavePublicKey = "FLWPUBK_TEST-9c877b3110438191127e631c89***";
		public static readonly string FlutterWaveEncryptionKey = "FLWSECK_TEST298f1f905***";

		//*********************************************************  

		public static readonly bool ShowVideoWithDynamicHeight = true;

		//********************************************************* 
		public static readonly bool ShowTextWithSpace = true;

	}
}