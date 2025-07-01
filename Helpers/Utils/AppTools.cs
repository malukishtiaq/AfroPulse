using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Helpers.Utils
{
	public static class AppTools
	{
		public static string GetNameFinal(UserDataObject dataUser)
		{
			try
			{
				if (dataUser == null)
					return "";

				if (!string.IsNullOrEmpty(dataUser.Name) || !string.IsNullOrWhiteSpace(dataUser.Name))
					return Methods.FunString.DecodeString(dataUser.Name);

				if (!string.IsNullOrEmpty(dataUser.FirstName) && !string.IsNullOrWhiteSpace(dataUser.LastName))
					return Methods.FunString.DecodeString(dataUser.FirstName + " " + dataUser.LastName);

				if (!string.IsNullOrEmpty(dataUser.Username) || !string.IsNullOrWhiteSpace(dataUser.Username))
					return Methods.FunString.DecodeString(dataUser.Username);

				return Methods.FunString.DecodeString(dataUser.Username);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return Methods.FunString.DecodeString(dataUser?.Username);
			}
		}

		public static string GetAboutFinal(UserDataObject dataUser)
		{
			try
			{
				if (!string.IsNullOrEmpty(dataUser.About) && !string.IsNullOrWhiteSpace(dataUser.About))
					return Methods.FunString.DecodeString(dataUser.About);

				return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
			}
		}

		public static (string, string) GetCurrency(string idCurrency)
		{
			try
			{
				if (AppSettings.CurrencyStatic) return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);

				var list = ListUtils.MySettingsList;
				if (list == null)
					return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);

				string currency;
				bool success = int.TryParse(idCurrency, out var number);
				if (success)
				{
					Console.WriteLine("Converted '{0}' to {1}.", idCurrency, number);
					currency = list.CurrencyArray[number] ?? AppSettings.CurrencyCodeStatic;
				}
				else
				{
					Console.WriteLine("Attempted conversion of '{0}' failed.", idCurrency ?? "<null>");
					currency = idCurrency;
				}

				if (list.CurrencySymbolArray != null)
				{
					string currencyIcon;
					switch (currency)
					{
						case "USD":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Usd) ? list.CurrencySymbolArray.Usd ?? "$" : "$";
							break;
						case "Jpy":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Jpy) ? list.CurrencySymbolArray.Jpy ?? "¥" : "¥";
							break;
						case "EUR":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Eur) ? list.CurrencySymbolArray.Eur ?? "€" : "€";
							break;
						case "TRY":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Try) ? list.CurrencySymbolArray.Try ?? "₺" : "₺";
							break;
						case "GBP":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Gbp) ? list.CurrencySymbolArray.Gbp ?? "£" : "£";
							break;
						case "RUB":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Rub) ? list.CurrencySymbolArray.Rub ?? "$" : "$";
							break;
						case "PLN":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Pln) ? list.CurrencySymbolArray.Pln ?? "zł" : "zł";
							break;
						case "ILS":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Ils) ? list.CurrencySymbolArray.Ils ?? "₪" : "₪";
							break;
						case "BRL":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Brl) ? list.CurrencySymbolArray.Brl ?? "R$" : "R$";
							break;
						case "INR":
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Inr) ? list.CurrencySymbolArray.Inr ?? "₹" : "₹";
							break;
						default:
							currencyIcon = !string.IsNullOrEmpty(list.CurrencySymbolArray.Usd) ? list.CurrencySymbolArray.Usd ?? "$" : "$";
							break;
					}

					return (currency, currencyIcon);
				}

				return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);
			}
		}

		public static Dictionary<string, string> GetPrivacyList(Activity activity)
		{
			try
			{
				var arrayAdapter = new Dictionary<string, string>
				{
					{"0", activity.GetString(Resource.String.Radio_public)},
					{"1", activity.GetString(Resource.String.Radio_private)},
					{"2", activity.GetString(Resource.String.Lbl_Unlisted)},
					{"3", activity.GetString(Resource.String.Lbl_Scheduled)},
					{"4", activity.GetString(Resource.String.Lbl_Subscribers)},
				};

				return arrayAdapter;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new Dictionary<string, string>();
			}
		}

		public static Dictionary<string, string> GetLicenseTypeStockList(Activity activity)
		{
			try
			{
				var arrayAdapter = new Dictionary<string, string>
				{
					{"rights_managed_license", activity.GetString(Resource.String.Lbl_rights_managed_license)},
					{"editorial_use_license", activity.GetString(Resource.String.Lbl_editorial_use_license)},
					{"royalty_free_license", activity.GetString(Resource.String.Lbl_royalty_free_license)},
					{"royalty_free_extended_license", activity.GetString(Resource.String.Lbl_royalty_free_extended_license)},
					{"creative_commons_license", activity.GetString(Resource.String.Lbl_creative_commons_license)},
					{"public_domain", activity.GetString(Resource.String.Lbl_public_domain)},
				};

				return arrayAdapter;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new Dictionary<string, string>();
			}
		}

		public static List<VideoDataObject> ListFilter(List<VideoDataObject> list, bool removeNotInterested = true)
		{
			try
			{
				//const string sDuration = "0:0";
				//const string mDuration = "00:00";
				//const string hDuration = "00:00:00";

				//list.RemoveAll(a => ((a.Duration == sDuration || a.Duration == mDuration || a.Duration == hDuration) && string.IsNullOrEmpty(a.Ok)) || string.IsNullOrEmpty(a.VideoId)); 
				list.RemoveAll(a => (a.Privacy == "1" || a.Privacy == "2") && a.UserId != UserDetails.UserId);
				list.RemoveAll(a => a.Approved != "1");

				List<VideoDataObject> result = list;
				if (removeNotInterested)
				{
					try
					{
						list.RemoveAll(c => ListUtils.GlobalNotInterestedList.Select(fc => fc.Id).Contains(c.Id));
					}
					catch
					{  //wael check later
						result = list.Except(ListUtils.GlobalNotInterestedList).ToList();
					}
				}

				foreach (var videoObject in result)
				{
					VideoFilter(videoObject);
				}

				return result;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new List<VideoDataObject>(list);
			}
		}

		public static VideoDataObject VideoFilter(VideoDataObject videoObject)
		{
			try
			{
				videoObject = SetLinkWithQuality(videoObject);
				if (string.IsNullOrEmpty(videoObject.CategoryName))
					videoObject.CategoryName = CategoriesController.GetCategoryName(videoObject);

				if (videoObject?.Owner?.OwnerClass != null)
				{
					videoObject.IsOwner = videoObject?.Owner?.OwnerClass?.Id == UserDetails.UserId;
				}

				return videoObject;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return videoObject;
			}
		}

		public static void ShowGlobalBadgeSystem(TextView videoTypeIcon, VideoDataObject item)
		{
			try
			{
				if (!string.IsNullOrEmpty(item.Twitch) && AppSettings.SetTwichTypeBadgeIcon)
				{
					videoTypeIcon.Visibility = ViewStates.Visible;
					FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoTypeIcon, IonIconsFonts.LogoTwitch);
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#6441A4"));
				}
				else if (!string.IsNullOrEmpty(item.Vimeo) && AppSettings.SetVimeoTypeBadgeIcon)
				{
					FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoTypeIcon, IonIconsFonts.LogoVimeo);
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#0D94CD"));
				}
				else if (!string.IsNullOrEmpty(item.Youtube) && AppSettings.SetYoutubeTypeBadgeIcon)
				{
					FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoTypeIcon, IonIconsFonts.LogoYoutube);
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#FF0000"));
				}
				else if (!string.IsNullOrEmpty(item.Ok) && AppSettings.SetOkTypeBadgeIcon)
				{
					videoTypeIcon.Text = "Ok";
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#F0932A"));
				}
				else if (!string.IsNullOrEmpty(item.Daily) && AppSettings.SetDailyMotionTypeBadgeIcon)
				{
					videoTypeIcon.Text = "d";
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#00d2f3"));
				}
				else if (!string.IsNullOrEmpty(item.Facebook) && AppSettings.SetFacebookTypeBadgeIcon)
				{
					videoTypeIcon.Text = "FB";
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#3b5999"));
				}
				else if (item.IsShort == "1" && AppSettings.ShowShorts)
				{
					FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoTypeIcon, IonIconsFonts.Videocam);
					videoTypeIcon.Visibility = ViewStates.Visible;
					videoTypeIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
				}
				else
				{
					videoTypeIcon.Visibility = ViewStates.Gone;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static VideoDataObject SetLinkWithQuality(VideoDataObject videoData)
		{
			try
			{
				if (videoData == null
					|| !string.IsNullOrEmpty(videoData.Twitch) || !string.IsNullOrEmpty(videoData.Vimeo) || !string.IsNullOrEmpty(videoData.Youtube)
					|| !string.IsNullOrEmpty(videoData.Ok) || !string.IsNullOrEmpty(videoData.Daily) || !string.IsNullOrEmpty(videoData.Facebook)
					|| videoData.VideoType == "VideoObject/youtube" || videoData.VideoLocation.Contains("Youtube") || videoData.VideoLocation.Contains("youtu"))
					return videoData;

				if (videoData.VideoLocation.Contains("converted"))
				{
					var explodeVideo = videoData.VideoLocation.Split("_video_");
					if (explodeVideo.Length > 0)
						videoData.VideoAuto = explodeVideo[1].Replace(".mp4", "").Replace("converted", "").Replace("_", "").Replace("shorts", "");

					if (videoData.The240P == "1" && string.IsNullOrEmpty(videoData.Video240P))
					{
						videoData.Video240P = explodeVideo[0] + "_video_240p_converted.mp4";
					}
					if (videoData.The360P == "1" && string.IsNullOrEmpty(videoData.Video360P))
					{
						videoData.Video360P = explodeVideo[0] + "_video_360p_converted.mp4";
					}
					if (videoData.The480P == "1" && string.IsNullOrEmpty(videoData.Video480P))
					{
						videoData.Video480P = explodeVideo[0] + "_video_480p_converted.mp4";
					}
					if (videoData.The720P == "1" && string.IsNullOrEmpty(videoData.Video720P))
					{
						videoData.Video720P = explodeVideo[0] + "_video_720p_converted.mp4";
					}
					if (videoData.The1080P == "1" && string.IsNullOrEmpty(videoData.Video1080P))
					{
						videoData.Video1080P = explodeVideo[0] + "_video_1080p_converted.mp4";
					}
					if (videoData.The4096P == "1" && string.IsNullOrEmpty(videoData.Video4096P))
					{
						videoData.Video4096P = explodeVideo[0] + "_video_4096p_converted.mp4";
					}
					if (videoData.The2048P == "1" && string.IsNullOrEmpty(videoData.Video2048P))
					{
						videoData.Video2048P = explodeVideo[0] + "_video_2048p_converted.mp4";
					}
				}
				return videoData;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return videoData;
			}
		}

		public static string GetLinkWithQuality(VideoDataObject videoData, string quality)
		{
			try
			{
				if (videoData == null
					|| !string.IsNullOrEmpty(videoData.Twitch) || !string.IsNullOrEmpty(videoData.Vimeo) || !string.IsNullOrEmpty(videoData.Youtube)
					|| !string.IsNullOrEmpty(videoData.Ok) || !string.IsNullOrEmpty(videoData.Daily) || !string.IsNullOrEmpty(videoData.Facebook)
					|| videoData.VideoType == "VideoObject/youtube" || videoData.VideoLocation.Contains("Youtube") || videoData.VideoLocation.Contains("youtu"))
					return videoData?.VideoLocation ?? "";

				if (quality.Contains("Auto") && !string.IsNullOrEmpty(videoData.VideoAuto))
				{
					return videoData.VideoAuto;
				}
				if (quality.Contains("240p") && videoData.The240P == "1" && !string.IsNullOrEmpty(videoData.Video240P))
				{
					return videoData.Video240P;
				}
				if (quality.Contains("360p") && videoData.The360P == "1" && !string.IsNullOrEmpty(videoData.Video360P))
				{
					return videoData.Video360P;
				}
				if (quality.Contains("480p") && videoData.The480P == "1" && !string.IsNullOrEmpty(videoData.Video480P))
				{
					return videoData.Video480P;
				}
				if (quality.Contains("720p") && videoData.The720P == "1" && !string.IsNullOrEmpty(videoData.Video720P))
				{
					return videoData.Video720P;
				}
				if (quality.Contains("1080p") && videoData.The1080P == "1" && !string.IsNullOrEmpty(videoData.Video1080P))
				{
					return videoData.Video1080P;
				}
				if (quality.Contains("4096p") && videoData.The4096P == "1" && !string.IsNullOrEmpty(videoData.Video4096P))
				{
					return videoData.Video4096P;
				}
				if (quality.Contains("2048p") && videoData.The2048P == "1" && !string.IsNullOrEmpty(videoData.Video2048P))
				{
					return videoData.Video2048P;
				}

				return videoData.VideoLocation ?? "";
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return videoData?.VideoLocation ?? "";
			}
		}

		private static Dictionary<string, float> GetListPlaybackSpeedData()
		{
			var playbackSpeeds = new Dictionary<string, float>
			{
				{ "0.25x", 0.25f },
				{ "0.50x", 0.50f },
				{ "0.75x", 0.75f },
				{ "Normal", 1f },
				{ "1.25x", 1.25f },
				{ "1.50x", 1.50f },
				{ "1.75x", 1.75f },
				{ "2.0x", 2.0f }
			};

			return playbackSpeeds;
		}

		public static List<string> GetListPlaybackSpeed()
		{
			var playbackSpeeds = GetListPlaybackSpeedData();

			return playbackSpeeds.Select(data => data.Key).ToList();
		}

		public static float GetSpeed(string selectedSpeed)
		{
			var playbackSpeeds = GetListPlaybackSpeedData();
			playbackSpeeds.TryGetValue(selectedSpeed, out float speed);

			return speed == 0f ? 1.0f : speed;
		}

		public static string GetSpeedText(float selectedSpeed)
		{

			var playbackSpeeds = GetListPlaybackSpeedData();
			var speedText = playbackSpeeds.Where(pair => pair.Value == selectedSpeed)
				   .Select(pair => pair.Key);

			return speedText?.FirstOrDefault();
		}

		public static List<string> GetListQualityVideo(VideoDataObject videoData)
		{
			try
			{
				var arrayAdapter = new List<string>();
				if (!string.IsNullOrEmpty(videoData.VideoAuto))
				{
					arrayAdapter.Add("Auto (" + videoData.VideoAuto + ")");
				}
				if (videoData.The240P == "1")
				{
					arrayAdapter.Add("240p");
				}
				if (videoData.The360P == "1")
				{
					arrayAdapter.Add("360p");
				}
				if (videoData.The480P == "1")
				{
					arrayAdapter.Add("480p");
				}
				if (videoData.The720P == "1")
				{
					arrayAdapter.Add("720p - HD");
				}
				if (videoData.The1080P == "1")
				{
					arrayAdapter.Add("1080p - HD");
				}
				if (videoData.The4096P == "1")
				{
					arrayAdapter.Add("4096p - 4K");
				}
				if (videoData.The2048P == "1")
				{
					arrayAdapter.Add("2048p - 4K");
				}

				return arrayAdapter;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new List<string>();
			}
		}

		public static bool CheckPictureInPictureAllowed(Activity activityContext)
		{
			try
			{
				bool opPictureInPictureAllowed = false;

				if ((int)Build.VERSION.SdkInt > 23)
				{
					var appOpsManager = (AppOpsManager)activityContext.GetSystemService(Context.AppOpsService);
					if (appOpsManager != null)
#pragma warning disable 618
						opPictureInPictureAllowed = AppOpsManagerMode.Allowed == appOpsManager.CheckOpNoThrow(AppOpsManager.OpstrPictureInPicture, Process.MyUid(), activityContext.PackageName);
#pragma warning restore 618
				}

				if (activityContext.PackageManager != null && activityContext.PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) && opPictureInPictureAllowed)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}

		public static Dictionary<string, string> GetCountryList(Activity activity)
		{
			try
			{
				var arrayAdapter = new Dictionary<string, string>
				{
					{"1",  activity.GetString(Resource.String.Lbl_country1)},
					{"2",  activity.GetString(Resource.String.Lbl_country2)},
					{"3",  activity.GetString(Resource.String.Lbl_country3)},
					{"4",  activity.GetString(Resource.String.Lbl_country4)},
					{"5",  activity.GetString(Resource.String.Lbl_country5)},
					{"6",  activity.GetString(Resource.String.Lbl_country6)},
					{"7",  activity.GetString(Resource.String.Lbl_country7)},
					{"8",  activity.GetString(Resource.String.Lbl_country8)},
					{"9",  activity.GetString(Resource.String.Lbl_country9)},
					{"10", activity.GetString(Resource.String.Lbl_country10)},
					{"11", activity.GetString(Resource.String.Lbl_country11)},
					{"12", activity.GetString(Resource.String.Lbl_country12)},
					{"13", activity.GetString(Resource.String.Lbl_country13)},
					{"14", activity.GetString(Resource.String.Lbl_country14)},
					{"15", activity.GetString(Resource.String.Lbl_country15)},
					{"16", activity.GetString(Resource.String.Lbl_country16)},
					{"17", activity.GetString(Resource.String.Lbl_country17)},
					{"18", activity.GetString(Resource.String.Lbl_country18)},
					{"19", activity.GetString(Resource.String.Lbl_country19)},
					{"20", activity.GetString(Resource.String.Lbl_country20)},
					{"21", activity.GetString(Resource.String.Lbl_country21)},
					{"22", activity.GetString(Resource.String.Lbl_country22)},
					{"23", activity.GetString(Resource.String.Lbl_country23)},
					{"24", activity.GetString(Resource.String.Lbl_country24)},
					{"25", activity.GetString(Resource.String.Lbl_country25)},
					{"26", activity.GetString(Resource.String.Lbl_country26)},
					{"27", activity.GetString(Resource.String.Lbl_country27)},
					{"28", activity.GetString(Resource.String.Lbl_country28)},
					{"29", activity.GetString(Resource.String.Lbl_country29)},
					{"30", activity.GetString(Resource.String.Lbl_country30)},
					{"31", activity.GetString(Resource.String.Lbl_country31)},
					{"32", activity.GetString(Resource.String.Lbl_country32)},
					{"34", activity.GetString(Resource.String.Lbl_country34)},
					{"35", activity.GetString(Resource.String.Lbl_country35)},
					{"36", activity.GetString(Resource.String.Lbl_country36)},
					{"37", activity.GetString(Resource.String.Lbl_country37)},
					{"38", activity.GetString(Resource.String.Lbl_country38)},
					{"39", activity.GetString(Resource.String.Lbl_country39)},
					{"40", activity.GetString(Resource.String.Lbl_country40)},
					{"41", activity.GetString(Resource.String.Lbl_country41)},
					{"42", activity.GetString(Resource.String.Lbl_country42)},
					{"43", activity.GetString(Resource.String.Lbl_country43)},
					{"44", activity.GetString(Resource.String.Lbl_country44)},
					{"45", activity.GetString(Resource.String.Lbl_country45)},
					{"46", activity.GetString(Resource.String.Lbl_country46)},
					{"47", activity.GetString(Resource.String.Lbl_country47)},
					{"48", activity.GetString(Resource.String.Lbl_country48)},
					{"49", activity.GetString(Resource.String.Lbl_country49)},
					{"50", activity.GetString(Resource.String.Lbl_country50)},
					{"51", activity.GetString(Resource.String.Lbl_country51)},
					{"52", activity.GetString(Resource.String.Lbl_country52)},
					{"53", activity.GetString(Resource.String.Lbl_country53)},
					{"54", activity.GetString(Resource.String.Lbl_country54)},
					{"55", activity.GetString(Resource.String.Lbl_country55)},
					{"56", activity.GetString(Resource.String.Lbl_country56)},
					{"57", activity.GetString(Resource.String.Lbl_country57)},
					{"58", activity.GetString(Resource.String.Lbl_country58)},
					{"59", activity.GetString(Resource.String.Lbl_country59)},
					{"60", activity.GetString(Resource.String.Lbl_country60)},
					{"61", activity.GetString(Resource.String.Lbl_country61)},
					{"62", activity.GetString(Resource.String.Lbl_country62)},
					{"63", activity.GetString(Resource.String.Lbl_country63)},
					{"64", activity.GetString(Resource.String.Lbl_country64)},
					{"65", activity.GetString(Resource.String.Lbl_country65)},
					{"66", activity.GetString(Resource.String.Lbl_country66)},
					{"67", activity.GetString(Resource.String.Lbl_country67)},
					{"68", activity.GetString(Resource.String.Lbl_country68)},
					{"69", activity.GetString(Resource.String.Lbl_country69)},
					{"70", activity.GetString(Resource.String.Lbl_country70)},
					{"71", activity.GetString(Resource.String.Lbl_country71)},
					{"72", activity.GetString(Resource.String.Lbl_country72)},
					{"73", activity.GetString(Resource.String.Lbl_country73)},
					{"74", activity.GetString(Resource.String.Lbl_country74)},
					{"75", activity.GetString(Resource.String.Lbl_country75)},
					{"76", activity.GetString(Resource.String.Lbl_country76)},
					{"77", activity.GetString(Resource.String.Lbl_country77)},
					{"78", activity.GetString(Resource.String.Lbl_country78)},
					{"79", activity.GetString(Resource.String.Lbl_country79)},
					{"80", activity.GetString(Resource.String.Lbl_country80)},
					{"81", activity.GetString(Resource.String.Lbl_country81)},
					{"82", activity.GetString(Resource.String.Lbl_country82)},
					{"83", activity.GetString(Resource.String.Lbl_country83)},
					{"84", activity.GetString(Resource.String.Lbl_country84)},
					{"85", activity.GetString(Resource.String.Lbl_country85)},
					{"86", activity.GetString(Resource.String.Lbl_country86)},
					{"87", activity.GetString(Resource.String.Lbl_country87)},
					{"88", activity.GetString(Resource.String.Lbl_country88)},
					{"89", activity.GetString(Resource.String.Lbl_country89)},
					{"90", activity.GetString(Resource.String.Lbl_country90)},
					{"91", activity.GetString(Resource.String.Lbl_country91)},
					{"92", activity.GetString(Resource.String.Lbl_country92)},
					{"93", activity.GetString(Resource.String.Lbl_country93)},
					{"94", activity.GetString(Resource.String.Lbl_country94)},
					{"95", activity.GetString(Resource.String.Lbl_country95)},
					{"96", activity.GetString(Resource.String.Lbl_country96)},
					{"97", activity.GetString(Resource.String.Lbl_country97)},
					{"98", activity.GetString(Resource.String.Lbl_country98)},
					{"99", activity.GetString(Resource.String.Lbl_country99)},
					{"100",activity.GetString(Resource.String.Lbl_country100)},
					{"101",activity.GetString(Resource.String.Lbl_country101)},
					{"102",activity.GetString(Resource.String.Lbl_country102)},
					{"103",activity.GetString(Resource.String.Lbl_country103)},
					{"104",activity.GetString(Resource.String.Lbl_country104)},
					{"105",activity.GetString(Resource.String.Lbl_country105)},
					{"106",activity.GetString(Resource.String.Lbl_country106)},
					{"107",activity.GetString(Resource.String.Lbl_country107)},
					{"108",activity.GetString(Resource.String.Lbl_country108)},
					{"109",activity.GetString(Resource.String.Lbl_country109)},
					{"110",activity.GetString(Resource.String.Lbl_country110)},
					{"111",activity.GetString(Resource.String.Lbl_country111)},
					{"112",activity.GetString(Resource.String.Lbl_country112)},
					{"113",activity.GetString(Resource.String.Lbl_country113)},
					{"114",activity.GetString(Resource.String.Lbl_country114)},
					{"115",activity.GetString(Resource.String.Lbl_country115)},
					{"116",activity.GetString(Resource.String.Lbl_country116)},
					{"117",activity.GetString(Resource.String.Lbl_country117)},
					{"118",activity.GetString(Resource.String.Lbl_country118)},
					{"119",activity.GetString(Resource.String.Lbl_country119)},
					{"120",activity.GetString(Resource.String.Lbl_country120)},
					{"121",activity.GetString(Resource.String.Lbl_country121)},
					{"122",activity.GetString(Resource.String.Lbl_country122)},
					{"123",activity.GetString(Resource.String.Lbl_country123)},
					{"124",activity.GetString(Resource.String.Lbl_country124)},
					{"125",activity.GetString(Resource.String.Lbl_country125)},
					{"126",activity.GetString(Resource.String.Lbl_country126)},
					{"127",activity.GetString(Resource.String.Lbl_country127)},
					{"128",activity.GetString(Resource.String.Lbl_country128)},
					{"129",activity.GetString(Resource.String.Lbl_country129)},
					{"130",activity.GetString(Resource.String.Lbl_country130)},
					{"131",activity.GetString(Resource.String.Lbl_country131)},
					{"132",activity.GetString(Resource.String.Lbl_country132)},
					{"133",activity.GetString(Resource.String.Lbl_country133)},
					{"134",activity.GetString(Resource.String.Lbl_country134)},
					{"135",activity.GetString(Resource.String.Lbl_country135)},
					{"136",activity.GetString(Resource.String.Lbl_country136)},
					{"137",activity.GetString(Resource.String.Lbl_country137)},
					{"138",activity.GetString(Resource.String.Lbl_country138)},
					{"139",activity.GetString(Resource.String.Lbl_country139)},
					{"140",activity.GetString(Resource.String.Lbl_country140)},
					{"141",activity.GetString(Resource.String.Lbl_country141)},
					{"142",activity.GetString(Resource.String.Lbl_country142)},
					{"143",activity.GetString(Resource.String.Lbl_country143)},
					{"144",activity.GetString(Resource.String.Lbl_country144)},
					{"145",activity.GetString(Resource.String.Lbl_country145)},
					{"146",activity.GetString(Resource.String.Lbl_country146)},
					{"147",activity.GetString(Resource.String.Lbl_country147)},
					{"148",activity.GetString(Resource.String.Lbl_country148)},
					{"149",activity.GetString(Resource.String.Lbl_country149)},
					{"150",activity.GetString(Resource.String.Lbl_country150)},
					{"151",activity.GetString(Resource.String.Lbl_country151)},
					{"152",activity.GetString(Resource.String.Lbl_country152)},
					{"153",activity.GetString(Resource.String.Lbl_country153)},
					{"154",activity.GetString(Resource.String.Lbl_country154)},
					{"155",activity.GetString(Resource.String.Lbl_country155)},
					{"156",activity.GetString(Resource.String.Lbl_country156)},
					{"157",activity.GetString(Resource.String.Lbl_country157)},
					{"158",activity.GetString(Resource.String.Lbl_country158)},
					{"159",activity.GetString(Resource.String.Lbl_country159)},
					{"160",activity.GetString(Resource.String.Lbl_country160)},
					{"161",activity.GetString(Resource.String.Lbl_country161)},
					{"162",activity.GetString(Resource.String.Lbl_country162)},
					{"163",activity.GetString(Resource.String.Lbl_country163)},
					{"164",activity.GetString(Resource.String.Lbl_country164)},
					{"165",activity.GetString(Resource.String.Lbl_country165)},
					{"166",activity.GetString(Resource.String.Lbl_country166)},
					{"167",activity.GetString(Resource.String.Lbl_country167)},
					{"168",activity.GetString(Resource.String.Lbl_country168)},
					{"169",activity.GetString(Resource.String.Lbl_country169)},
					{"170",activity.GetString(Resource.String.Lbl_country170)},
					{"171",activity.GetString(Resource.String.Lbl_country171)},
					{"172",activity.GetString(Resource.String.Lbl_country172)},
					{"173",activity.GetString(Resource.String.Lbl_country173)},
					{"174",activity.GetString(Resource.String.Lbl_country174)},
					{"175",activity.GetString(Resource.String.Lbl_country175)},
					{"176",activity.GetString(Resource.String.Lbl_country176)},
					{"177",activity.GetString(Resource.String.Lbl_country177)},
					{"178",activity.GetString(Resource.String.Lbl_country178)},
					{"179",activity.GetString(Resource.String.Lbl_country179)},
					{"180",activity.GetString(Resource.String.Lbl_country180)},
					{"181",activity.GetString(Resource.String.Lbl_country181)},
					{"182",activity.GetString(Resource.String.Lbl_country182)},
					{"183",activity.GetString(Resource.String.Lbl_country183)},
					{"184",activity.GetString(Resource.String.Lbl_country184)},
					{"185",activity.GetString(Resource.String.Lbl_country185)},
					{"186",activity.GetString(Resource.String.Lbl_country186)},
					{"187",activity.GetString(Resource.String.Lbl_country187)},
					{"188",activity.GetString(Resource.String.Lbl_country188)},
					{"189",activity.GetString(Resource.String.Lbl_country189)},
					{"190",activity.GetString(Resource.String.Lbl_country190)},
					{"191",activity.GetString(Resource.String.Lbl_country191)},
					{"192",activity.GetString(Resource.String.Lbl_country192)},
					{"193",activity.GetString(Resource.String.Lbl_country193)},
					{"194",activity.GetString(Resource.String.Lbl_country194)},
					{"195",activity.GetString(Resource.String.Lbl_country195)},
					{"196",activity.GetString(Resource.String.Lbl_country196)},
					{"197",activity.GetString(Resource.String.Lbl_country197)},
					{"198",activity.GetString(Resource.String.Lbl_country198)},
					{"199",activity.GetString(Resource.String.Lbl_country199)},
					{"200",activity.GetString(Resource.String.Lbl_country200)},
					{"201",activity.GetString(Resource.String.Lbl_country201)},
					{"202",activity.GetString(Resource.String.Lbl_country202)},
					{"203",activity.GetString(Resource.String.Lbl_country203)},
					{"204",activity.GetString(Resource.String.Lbl_country204)},
					{"205",activity.GetString(Resource.String.Lbl_country205)},
					{"206",activity.GetString(Resource.String.Lbl_country206)},
					{"207",activity.GetString(Resource.String.Lbl_country207)},
					{"208",activity.GetString(Resource.String.Lbl_country208)},
					{"209",activity.GetString(Resource.String.Lbl_country209)},
					{"210",activity.GetString(Resource.String.Lbl_country210)},
					{"211",activity.GetString(Resource.String.Lbl_country211)},
					{"212",activity.GetString(Resource.String.Lbl_country212)},
					{"213",activity.GetString(Resource.String.Lbl_country213)},
					{"214",activity.GetString(Resource.String.Lbl_country214)},
					{"215",activity.GetString(Resource.String.Lbl_country215)},
					{"216",activity.GetString(Resource.String.Lbl_country216)},
					{"217",activity.GetString(Resource.String.Lbl_country217)},
					{"218",activity.GetString(Resource.String.Lbl_country218)},
					{"219",activity.GetString(Resource.String.Lbl_country219)},
					{"220",activity.GetString(Resource.String.Lbl_country220)},
					{"221",activity.GetString(Resource.String.Lbl_country221)},
					{"222",activity.GetString(Resource.String.Lbl_country222)},
					{"223",activity.GetString(Resource.String.Lbl_country223)},
					{"224",activity.GetString(Resource.String.Lbl_country224)},
					{"225",activity.GetString(Resource.String.Lbl_country225)},
					{"226",activity.GetString(Resource.String.Lbl_country226)},
					{"227",activity.GetString(Resource.String.Lbl_country227)},
					{"228",activity.GetString(Resource.String.Lbl_country228)},
					{"229",activity.GetString(Resource.String.Lbl_country229)},
					{"230",activity.GetString(Resource.String.Lbl_country230)},
					{"231",activity.GetString(Resource.String.Lbl_country231)},
					{"232",activity.GetString(Resource.String.Lbl_country232)},
					{"233",activity.GetString(Resource.String.Lbl_country233)},
					{"238",activity.GetString(Resource.String.Lbl_country238)},
					{"239",activity.GetString(Resource.String.Lbl_country239)},
					{"240",activity.GetString(Resource.String.Lbl_country240)},
					{"241",activity.GetString(Resource.String.Lbl_country241)},
					{"242",activity.GetString(Resource.String.Lbl_country242)},
				};

				return arrayAdapter;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return new Dictionary<string, string>();
			}
		}

		public static bool GetStatusAds()
		{
			try
			{
				switch (AppSettings.ShowAds)
				{
					case ShowAds.AllUsers:
						return true;
					case ShowAds.UnProfessional:
						{
							var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
							if (isPro == "0")
								return true;
							else
								return false;
						}
					default:
						return false;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}

		public static bool IsTabDark()
		{
			try
			{
				return AppSettings.SetTabDarkTheme is TabTheme.Dark;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;

			}
		}

		public static async Task<bool> CheckWallet(double price)
		{
			try
			{
				await ApiRequest.GetChannelData(TabbedMainActivity.GetInstance(), UserDetails.UserId);

				var wallet = ListUtils.MyChannelList.FirstOrDefault()?.Wallet ?? "0";
				if (!string.IsNullOrEmpty(wallet) && wallet != "0")
				{
					bool isParable = double.TryParse(wallet, out var number);
					if (isParable)
					{
						if (number >= price)
						{
							return true;
						}

						return false;
					}
				}
				return false;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}
	}
}