using Newtonsoft.Json;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using System;

namespace PlayTube.Helpers.Models
{
	public static class UserDetails
	{
		//############# DONT'T MODIFY HERE #############
		//Auto Session bindable objects 
		//*********************************************************
		public static string AccessToken = "";

		public static string UserId = "";
		public static string Username = "";
		public static string FullName = "";
		public static string Password = "";
		public static string Email = "";
		public static string Cookie = "";
		public static string Status = "";
		public static string Avatar = "no_profile_image";
		public static string Cover = "";
		public static string DeviceId = "";
		public static string LangName = "";

		public static bool AutoNext = false;
		public static bool PipIsChecked = false;
		public static bool IsPauseWatchHistory = false;

		public static string AndroidId = "";//Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.AndroidId);
		public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

		public static bool IsLogin = false;

		public static string FilterTime = "all_time";
		public static string FilterTypeSortBy = "views";

		public static string StockSearchTerm = "";
		public static string StockLicenseType = "";
		public static string StockPriceMin = "";
		public static string StockPriceMax = "";

		public static string MoviesSearchTerm = "";
		public static string MoviesRating = "";
		public static string MoviesRelease = "";
		public static string MoviesCountry = "";
		public static string MoviesCategory = "";

		public static string PopupFilterPlaylistValue = "";

		public static void ClearAllValueUserDetails()
		{
			try
			{
				AccessToken = string.Empty;
				UserId = string.Empty;
				Username = string.Empty;
				FullName = string.Empty;
				Password = string.Empty;
				Email = string.Empty;
				Cookie = string.Empty;
				Status = string.Empty;
				Avatar = string.Empty;
				Cover = string.Empty;
				DeviceId = string.Empty;
				LangName = string.Empty;
				IsLogin = false;

				AutoNext = false;
				PipIsChecked = false;
				IsPauseWatchHistory = false;

				FilterTime = "all_time";
				FilterTypeSortBy = "views";

				StockSearchTerm = "";
				StockLicenseType = "";
				StockPriceMin = "";
				StockPriceMax = "";

				MoviesSearchTerm = "";
				MoviesRating = "";
				MoviesRelease = "";
				MoviesCountry = "";
				MoviesCategory = "";

				InitializePlayTube.UserId = string.Empty;
				Current.AccessToken = string.Empty;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}
	}
}