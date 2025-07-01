//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Newtonsoft.Json;
using PlayTube.Activities.Default;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.OneSignalNotif.Models;
using PlayTube.PlayTubeClient.Classes.Global;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities
{
	[Activity(Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true, Theme = "@style/SplashScreenTheme", Exported = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	[IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHosts = new[] { "@string/ApplicationUrlWeb", "@string/ApplicationShortUrl" }, DataPathPrefixes = new[] { "/watch/", "/shorts/" }, AutoVerify = true)]
	public class SplashScreenActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
					AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

				base.OnCreate(savedInstanceState);

				Task startupWork = new Task(FirstRunExcite);
				startupWork.Start();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void FirstRunExcite()
		{
			try
			{
				if (!string.IsNullOrEmpty(AppSettings.Lang))
				{
					LangController.SetApplicationLang(this, AppSettings.Lang);
				}
				else
				{
#pragma warning disable 618
					UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales?.Get(0)?.Language?.ToLower() ?? Resources?.Configuration?.Locale?.Language?.ToLower();
#pragma warning restore 618
					LangController.SetApplicationLang(this, UserDetails.LangName);
				}

				if (Intent?.Data != null)
				{
					if (Intent.Data.ToString()!.Contains("watch") || Intent.Data.ToString()!.Contains("shorts") || Intent.Data.ToString()!.Contains(GetText(Resource.String.ApplicationShortUrl)))
					{
						//https://demo.playtubescript.com/watch/JaELhCpu899UHew
						//https://ptub.app/JaELhCpu899UHew
						var videoId = Intent?.Data.ToString()!.Split("/").Last();

						if (!string.IsNullOrEmpty(UserDetails.AccessToken))
							UserDetails.IsLogin = true;

						var dataNotification = new OsObject.OsNotificationObject()
						{
							Type = "DeepLinks",
							Video = new VideoDataObject()
							{
								VideoId = videoId
							},
							Url = Intent?.Data.ToString()
						};

						var intent = new Intent(this, typeof(TabbedMainActivity));
						intent.PutExtra("NotificationObject", JsonConvert.SerializeObject(dataNotification));
						StartActivity(intent);
					}
					else
					{
						if (!string.IsNullOrEmpty(UserDetails.AccessToken))
							UserDetails.IsLogin = true;

						switch (UserDetails.Status)
						{
							case "Pending":
							case "Active":
								StartActivity(new Intent(this, typeof(TabbedMainActivity)));
								break;
							default:
								StartActivity(new Intent(this, typeof(FirstActivity)));
								break;
						}
					}
				}
				else
				{
					switch (UserDetails.Status)
					{
						case "Pending":
						case "Active":
							StartActivity(new Intent(this, typeof(TabbedMainActivity)));
							break;
						default:
							StartActivity(new Intent(this, typeof(FirstActivity)));
							break;
					}
				}

				OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
				Finish();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}
	}
}