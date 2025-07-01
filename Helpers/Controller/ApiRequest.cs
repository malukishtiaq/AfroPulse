using Android.App;
using Android.Content;
using Android.Widget;
using AndroidX.Credentials;
using Bumptech.Glide;
using Com.Facebook;
using Com.Facebook.Login;
using Java.IO;
using Java.Lang;
using PlayTube.Activities.Default;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.OneSignalNotif;
using PlayTube.MediaPlayers.Exo;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Advertise;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.Classes.Video;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.Service;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static PlayTube.PlayTubeClient.Classes.Global.GetSettingsObject;
using Console = System.Console;
using Exception = System.Exception;
using Thread = System.Threading.Thread;
using Uri = Android.Net.Uri;

namespace PlayTube.Helpers.Controller
{
	internal static class ApiRequest
	{
		//Get Settings Api
		public static async Task GetSettings_Api(Activity activity)
		{
			if (Methods.CheckConnectivity())
			{
				if (UserDetails.IsLogin)
					await SetLangUserAsync();

				//site_settings
				var (apiStatus, respond) = await Current.GetSettingsAsync();
				if (apiStatus == 200)
				{
					if (respond is SiteSettingsObject settings)
					{
						ListUtils.MySettingsList = settings;

						if (AppSettings.OneSignalAppId != settings.PushId)
						{
							AppSettings.OneSignalAppId = settings.PushId;
							OneSignalNotification.Instance.RegisterNotificationDevice(activity);
						}

						//AppSettings.ShowButtonImport = string.IsNullOrWhiteSpace(settings.ImportSystem) ? AppSettings.ShowButtonImport : settings.ImportSystem == "on";
						//AppSettings.ShowButtonUpload = string.IsNullOrWhiteSpace(settings.UploadSystem) ? AppSettings.ShowButtonUpload : settings.UploadSystem == "on";

						//Insert MySettings in Database
						var sqlEntity = new SqLiteDatabase();
						sqlEntity.InsertOrUpdate_Settings(settings);

						await Task.Factory.StartNew(() =>
						{
							try
							{
								if (settings.Categories?.Count > 0)
								{
									//Categories >> New V.1.6
									var listCategories = settings.Categories.Select(cat => new Classes.Category
									{
										Id = cat.Key,
										Name = Methods.FunString.DecodeString(cat.Value),
										Color = "#212121",
										Image = CategoriesController.GetImageCategory(cat.Value),
										SubList = new List<Classes.Category>()
									}).ToList();

									CategoriesController.ListCategories.Clear();
									CategoriesController.ListCategories = new ObservableCollection<Classes.Category>(listCategories);
								}

								if (settings.SubCategories?.SubCategoriessList?.Count > 0)
								{
									//Sub Categories
									foreach (var sub in settings.SubCategories.Value.SubCategoriessList)
									{
										var subCategories = ListUtils.MySettingsList?.SubCategories?.SubCategoriessList?.FirstOrDefault(a => a.Key == sub.Key).Value;
										if (subCategories?.Count > 0)
										{
											var cat = CategoriesController.ListCategories.FirstOrDefault(a => a.Id == sub.Key);
											if (cat != null)
											{
												foreach (var pairs in subCategories.SelectMany(pairs => pairs))
												{
													cat.SubList.Add(new Classes.Category
													{
														Id = pairs.Key,
														Name = Methods.FunString.DecodeString(pairs.Value),
														Color = "#212121",
													});
												}
											}
										}
									}
								}

								if (settings.MoviesCategories?.Count > 0)
								{
									//Movies Categories
									var listMovies = settings.MoviesCategories.Select(cat => new Classes.Category
									{
										Id = cat.Key,
										Name = Methods.FunString.DecodeString(cat.Value),
										Color = "#212121",
										SubList = new List<Classes.Category>()
									}).ToList();

									CategoriesController.ListCategoriesMovies.Clear();
									CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Category>(listMovies);
								}
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
							}
						});
					}
				}
				else Methods.DisplayReportResult(activity, respond);
			}
		}

		private static async Task SetLangUserAsync()
		{
			try
			{
				if (string.IsNullOrEmpty(Current.AccessToken) || !AppSettings.SetLangUser)
					return;

				string lang = "";
				if (UserDetails.LangName.Contains("en"))
					lang = "english";
				else if (UserDetails.LangName.Contains("ar"))
					lang = "arabic";
				else if (UserDetails.LangName.Contains("de"))
					lang = "german";
				else if (UserDetails.LangName.Contains("el"))
					lang = "greek";
				else if (UserDetails.LangName.Contains("es"))
					lang = "spanish";
				else if (UserDetails.LangName.Contains("fr"))
					lang = "french";
				else if (UserDetails.LangName.Contains("it"))
					lang = "italian";
				else if (UserDetails.LangName.Contains("ja"))
					lang = "japanese";
				else if (UserDetails.LangName.Contains("nl"))
					lang = "dutch";
				else if (UserDetails.LangName.Contains("pt"))
					lang = "portuguese";
				else if (UserDetails.LangName.Contains("ro"))
					lang = "romanian";
				else if (UserDetails.LangName.Contains("ru"))
					lang = "russian";
				else if (UserDetails.LangName.Contains("sq"))
					lang = "albanian";
				else if (UserDetails.LangName.Contains("sr"))
					lang = "serbian";
				else if (UserDetails.LangName.Contains("tr"))
					lang = "turkish";
				//else
				//    lang = string.IsNullOrEmpty(UserDetails.LangName) ? AppSettings.Lang : "";

				await Task.Factory.StartNew(() =>
				{
					if (lang != "")
					{
						var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
						if (dataUser != null)
						{
							dataUser.Language = lang;

							var sqLiteDatabase = new SqLiteDatabase();
							sqLiteDatabase.InsertOrUpdate_DataMyChannel(dataUser);
						}

						if (Methods.CheckConnectivity())
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateLangAsync(lang) });
						else
							Toast.MakeText(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				});
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static async Task<UserDataObject> GetChannelData(Activity activity, string userId)
		{
			if (!Methods.CheckConnectivity()) return null;
			var (apiStatus, respond) = await RequestsAsync.Global.GetChannelInfoAsync(userId);
			if (apiStatus == 200)
			{
				if (respond is GetChannelInfoObject result)
				{
					if (userId == UserDetails.UserId)
					{
						UserDetails.Avatar = result.DataResult.Avatar;
						UserDetails.Cover = result.DataResult.Cover;
						UserDetails.Username = result.DataResult.Username;
						UserDetails.FullName = AppTools.GetNameFinal(result.DataResult);
						UserDetails.IsPauseWatchHistory = result.DataResult.PauseHistory == "1";

						ListUtils.MyChannelList?.Clear();
						ListUtils.MyChannelList?.Add(result.DataResult);

						activity?.RunOnUiThread(() =>
						{
							try
							{
								var profileImage1 = TabbedMainActivity.GetInstance()?.HomeFragment?.ProfileButton;
								if (profileImage1 != null)
									GlideImageLoader.LoadImage(activity, result.DataResult.Avatar, profileImage1, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

								var profileImage2 = TabbedMainActivity.GetInstance()?.TrendingFragment?.ProfileButton;
								if (profileImage2 != null)
									GlideImageLoader.LoadImage(activity, result.DataResult.Avatar, profileImage2, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						});

						var sqlEntity = new SqLiteDatabase();
						sqlEntity.InsertOrUpdate_DataMyChannel(result.DataResult);

						return result.DataResult;
					}

					return result.DataResult;
				}
			}
			else Methods.DisplayReportResult(activity, respond);
			return null;
		}

		//Get PlayLists Videos in API
		public static async Task PlayListsVideosApi(Activity activity, string offset = "0")
		{
			if (!UserDetails.IsLogin)
				return;

			if (Methods.CheckConnectivity())
			{
				var (apiStatus, respond) = await RequestsAsync.Playlist.GetMyPlaylistAsync(UserDetails.UserId, offset, "10");
				if (apiStatus == 200)
				{
					if (respond is GetPlaylistObject result)
					{
						if (result.AllPlaylist.Count > 0)
						{
							ListUtils.PlayListVideoObjectList = new ObservableCollection<PlayListVideoObject>(result.AllPlaylist);
						}
					}
				}
				else Methods.DisplayReportResult(activity, respond);
			}
		}

		//Get Ads Videos in API
		public static async Task AdsVideosApi(Activity activity)
		{
			if (!UserDetails.IsLogin)
				return;

			if (Methods.CheckConnectivity())
			{
				string offset = ListUtils.AdsVideoList.LastOrDefault()?.Id ?? "0";

				var (apiStatus, respond) = await RequestsAsync.Advertise.GetAdsAsync("5", offset);
				if (apiStatus == 200)
				{
					if (respond is GetAdsVideosListObject result)
					{
						if (result.VideoList.Count > 0)
						{
							await Task.Run(() =>
							{
								try
								{
									if (ListUtils.AdsVideoList.Count > 0)
									{
										foreach (var item in from item in result.VideoList let check = ListUtils.AdsVideoList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
										{
											ListUtils.AdsVideoList.Add(item);
										}
									}
									else
									{
										ListUtils.AdsVideoList = new ObservableCollection<VideoAdDataObject>(result.VideoList);
									}
								}
								catch (Exception e)
								{
									Methods.DisplayReportResultTrack(e);
								}
							}).ConfigureAwait(false);
						}
					}
				}
				else Methods.DisplayReportResult(activity, respond);
			}
		}

		public static async Task GetNotInterestedVideos()
		{
			if (!UserDetails.IsLogin)
				return;

			if (Methods.CheckConnectivity())
			{
				string offset = ListUtils.GlobalNotInterestedList.LastOrDefault()?.Id ?? "0";

				var (apiStatus, respond) = await RequestsAsync.Video.GetNotInterestedAsync("10", offset);
				if (apiStatus == 200)
				{
					if (respond is NotInterestedObject result && result.Data.Count > 0)
					{
						await Task.Run(() =>
						{
							try
							{
								foreach (var dataObject in from dataObject in result.Data where dataObject?.Video != null let check = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == dataObject?.Video?.Id) where check == null select dataObject)
								{
									ListUtils.GlobalNotInterestedList.Add(dataObject.Video);
								}
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						}).ConfigureAwait(false);
					}
				}
				//else Methods.DisplayReportResult(Activity, respond);
			}
		}

		public static async Task GetAllShortsVideo()
		{
			try
			{
				if (!Methods.CheckConnectivity())
					return;

				if (!AppSettings.ShowShorts || !UserDetails.IsLogin)
					return;

				var (apiStatus, respond) = await RequestsAsync.Video.GetShortsAsync();
				if (apiStatus != 200 || respond is not GetVideosListDataObject result || result.VideoList == null)
				{
					//Methods.DisplayReportResult(Activity, respond);
				}
				else
				{
					var respondList = result.VideoList?.Count;
					if (respondList > 0)
					{
						result.VideoList = AppTools.ListFilter(result.VideoList);

						foreach (var item in from item in result.VideoList let check = ListUtils.VideoShortsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
						{
							var checkViewed = ListUtils.VideoShortsViewsList.FirstOrDefault(a => a.Id == item.Id);
							if (checkViewed == null)
							{
								ListUtils.VideoShortsList.Add(new Classes.ShortsVideoClass()
								{
									Id = item.Id,
									Type = ItemType.ShortVideos,
									VideoData = item
								});
							}

							if (AdsGoogle.NativeAdsPool?.Count > 0 && ListUtils.VideoShortsList.Count % AppSettings.ShowAdNativeCount == 0)
							{
								ListUtils.VideoShortsList.Add(new Classes.ShortsVideoClass
								{
									Type = ItemType.AdMob4,
								});
							}
						}

						await Task.Factory.StartNew(() =>
						{
							try
							{
								var list = ListUtils.VideoShortsList.Where(a => a.Type == ItemType.ShortVideos).Take(5);
								foreach (var videoObject in list)
								{
									new PreCachingExoPlayerVideo(Application.Context).CacheVideosFiles(Uri.Parse(videoObject.VideoData?.VideoLocation));
								}
							}
							catch (Exception e)
							{
								Methods.DisplayReportResultTrack(e);
							}
						}).ConfigureAwait(false);
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}



		//======================================== 
		private static bool RunLogout;

		public static async Task Delete(Activity context)
		{
			try
			{
				if (RunLogout == false)
				{
					RunLogout = true;

					await RemoveData("Delete");

					context?.RunOnUiThread(() =>
					{
						Methods.Path.DeleteAll_FolderUser();

						SqLiteDatabase dbDatabase = new SqLiteDatabase();
						dbDatabase.DropAll();

						Runtime.GetRuntime()?.RunFinalization();
						Runtime.GetRuntime()?.Gc();
						TrimCache(context);

						ListUtils.ClearAllList();

						UserDetails.ClearAllValueUserDetails();

						dbDatabase.Connect();

						AppApiService.GetInstance()?.StopJob(context);
						context.StopService(new Intent(context, typeof(AppApiService)));

						MainSettings.SharedData?.Edit()?.Clear()?.Commit();
						MainSettings.AutoNext?.Edit()?.Clear()?.Commit();
						MainSettings.InAppReview?.Edit()?.Clear()?.Commit();

						Intent intent = new Intent(context, typeof(LoginActivity));
						intent.AddCategory(Intent.CategoryHome);
						intent.SetAction(Intent.ActionMain);
						intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
						context.StartActivity(intent);
						context.FinishAffinity();
						context.Finish();
					});

					RunLogout = false;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static async void Logout(Activity context)
		{
			try
			{
				if (RunLogout == false)
				{
					RunLogout = true;

					await RemoveData("Logout");

					context?.RunOnUiThread(() =>
					{
						try
						{
							context.DeleteDatabase(AppSettings.DatabaseName + "_.db");
							context.DeleteDatabase(SqLiteDatabase.PathCombine);

							SqLiteDatabase dbDatabase = new SqLiteDatabase();
							dbDatabase.DropAll();

							Runtime.GetRuntime()?.RunFinalization();
							Runtime.GetRuntime()?.Gc();
							TrimCache(context);

							Reset(context);
							UserDetails.ClearAllValueUserDetails();

							OneSignalNotification.Instance.UnRegisterNotificationDevice();

							dbDatabase.Connect();

							AppApiService.GetInstance()?.StopJob(context);

							MainSettings.SharedData?.Edit()?.Clear()?.Commit();
							MainSettings.InAppReview?.Edit()?.Clear()?.Commit();
							MainSettings.UgcPrivacy?.Edit()?.Clear()?.Commit();
							MainSettings.AutoNext?.Edit()?.Clear()?.Commit();

							GC.Collect();

							Intent intent = new Intent(context, typeof(LoginActivity));
							intent.AddCategory(Intent.CategoryHome);
							intent.SetAction(Intent.ActionMain);
							intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
							context.StartActivity(intent);
							context.FinishAffinity();
							context.Finish();
						}
						catch (Exception e)
						{
							Methods.DisplayReportResultTrack(e);
						}
					});

					RunLogout = false;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static void TrimCache(Activity context)
		{
			try
			{
				File dir = context?.CacheDir;
				if (dir != null && dir.IsDirectory)
				{
					DeleteDir(dir);
				}

				context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
				context?.DeleteDatabase(SqLiteDatabase.PathCombine);

				if (context?.IsDestroyed != false)
					return;

				Glide.Get(context)?.ClearMemory();
				new Thread(() =>
				{
					try
					{
						Glide.Get(context)?.ClearDiskCache();
					}
					catch (Exception e)
					{
						Methods.DisplayReportResultTrack(e);
					}
				}).Start();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static bool DeleteDir(File dir)
		{
			try
			{
				if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
				string[] children = dir.List();
				if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
				{
					return false;
				}

				// The directory is now empty so delete it
				return dir.Delete();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return false;
			}
		}

		private static void Reset(Activity context)
		{
			try
			{
				ListUtils.ClearAllList();

				Methods.Path.DeleteAll_FolderUser();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private static async Task RemoveData(string type)
		{
			try
			{
				switch (type)
				{
					case "Logout":
						{
							if (Methods.CheckConnectivity())
								await RequestsAsync.Auth.UserLogoutAsync();

							break;
						}
					case "Delete":
						{
							Methods.Path.DeleteAll_FolderUser();

							if (Methods.CheckConnectivity())
								await RequestsAsync.Auth.DeleteUserAsync(UserDetails.UserId, UserDetails.Password);
							break;
						}
				}

				if (AppSettings.ShowGoogleLogin && SocialLoginBaseActivity.CredentialManager != null)
				{
					SocialLoginBaseActivity.CredentialManager.ClearCredentialState(new ClearCredentialStateRequest(), null);
					SocialLoginBaseActivity.CredentialManager = null;
				}

				if (AppSettings.ShowFacebookLogin)
				{
					var accessToken = AccessToken.CurrentAccessToken;
					var isLoggedIn = accessToken != null && !accessToken.IsExpired;
					if (isLoggedIn && Profile.CurrentProfile != null)
					{
						LoginManager.Instance.LogOut();
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