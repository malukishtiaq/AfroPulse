//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Newtonsoft.Json;
using PlayTube.Activities.Chat;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Messages;
using PlayTube.PlayTubeClient.Classes.Playlist;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static PlayTube.PlayTubeClient.Classes.Global.GetSettingsObject;

namespace PlayTube.SQLite
{
	public class SqLiteDatabase
	{
		//############# DON'T MODIFY HERE #############

		private static readonly string Folder = AppDomain.CurrentDomain.BaseDirectory; //Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		public static readonly string PathCombine = System.IO.Path.Combine(Folder, AppSettings.DatabaseName + "_.db");

		//############# CONNECTION #############

		#region DataBase Functions

		private SQLiteConnection OpenConnection()
		{
			try
			{
				var connection = new SQLiteConnection(PathCombine);
				return connection;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					OpenConnection();
				else
					Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public void Connect()
		{
			try
			{
				var connection = OpenConnection();
				connection?.CreateTable<DataTables.MySettingsTb>();
				connection?.CreateTable<DataTables.LoginTb>();
				connection?.CreateTable<DataTables.ChannelTb>();
				connection?.CreateTable<DataTables.WatchOfflineVideosTb>();
				connection?.CreateTable<DataTables.SubscriptionsChannelTb>();
				connection?.CreateTable<DataTables.LibraryItemTb>();
				connection?.CreateTable<DataTables.SharedVideosTb>();
				connection?.CreateTable<DataTables.LastChatTb>();
				connection?.CreateTable<DataTables.MessageTb>();

				AddLibrarySectionViews();
			}
			catch (SQLiteException e)
			{
				if (e.Message.Contains("database is locked"))
					Connect();
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Delete table 
		public void DropAll()
		{
			try
			{
				var connection = OpenConnection();
				connection.DropTable<DataTables.MySettingsTb>();
				connection.DropTable<DataTables.LoginTb>();
				connection.DropTable<DataTables.ChannelTb>();
				connection.DropTable<DataTables.WatchOfflineVideosTb>();
				connection.DropTable<DataTables.SubscriptionsChannelTb>();
				connection.DropTable<DataTables.LibraryItemTb>();
				connection.DropTable<DataTables.SharedVideosTb>();
				connection.DropTable<DataTables.LastChatTb>();
				connection.DropTable<DataTables.MessageTb>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					DropAll();
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		//############# CONNECTION #############

		#region Settings

		//Insert data Settings
		public void InsertOrUpdate_Settings(GetSettingsObject.SiteSettingsObject data)
		{
			try
			{
				var connection = OpenConnection();
				var resultChannelTb = connection.Table<DataTables.MySettingsTb>().FirstOrDefault();
				if (resultChannelTb == null)
				{
					var db = ClassMapper.Mapper?.Map<DataTables.MySettingsTb>(data);
					if (db != null)
					{
						db.AffiliateType = JsonConvert.SerializeObject(data.AffiliateType);
						db.CurrencyArray = JsonConvert.SerializeObject(data.CurrencyArray);
						db.CurrencySymbolArray = JsonConvert.SerializeObject(data.CurrencySymbolArray);
						db.Continents = JsonConvert.SerializeObject(data.Continents);
						db.Categories = JsonConvert.SerializeObject(data.Categories);

						if (data.SubCategories != null)
							db.SubCategories = JsonConvert.SerializeObject(data.SubCategories?.SubCategoriessList);

						db.MoviesCategories = JsonConvert.SerializeObject(data.MoviesCategories);

						connection.Insert(db);
					}
				}
				else
				{

					resultChannelTb = ClassMapper.Mapper?.Map<DataTables.MySettingsTb>(data);

					if (resultChannelTb != null)
					{
						resultChannelTb.AffiliateType = JsonConvert.SerializeObject(data.AffiliateType);
						resultChannelTb.CurrencyArray = JsonConvert.SerializeObject(data.CurrencyArray);
						resultChannelTb.CurrencySymbolArray = JsonConvert.SerializeObject(data.CurrencySymbolArray);
						resultChannelTb.Continents = JsonConvert.SerializeObject(data.Continents);
						resultChannelTb.Categories = JsonConvert.SerializeObject(data.Categories);

						if (data.SubCategories != null)
							resultChannelTb.SubCategories = JsonConvert.SerializeObject(data.SubCategories?.SubCategoriessList);

						resultChannelTb.MoviesCategories = JsonConvert.SerializeObject(data.MoviesCategories);
						resultChannelTb.ImportSystem = data.ImportSystem;
						resultChannelTb.UploadSystem = data.UploadSystem;

						connection.Update(resultChannelTb);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrUpdate_Settings(data);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get data setting 
		public GetSettingsObject.SiteSettingsObject Get_Settings()
		{
			try
			{
				var connection = OpenConnection();
				var data = connection.Table<DataTables.MySettingsTb>().FirstOrDefault();
				if (data != null)
				{
					var db = ClassMapper.Mapper?.Map<GetSettingsObject.SiteSettingsObject>(data);
					if (db != null)
					{
						db.CurrencyArray = new List<string>();
						db.CurrencySymbolArray = new GetSettingsObject.CurrencySymbolArray();
						db.Continents = new List<string>();
						db.Categories = new Dictionary<string, string>();
						db.SubCategories = new GetSettingsObject.SubCategoriesUnion();
						db.MoviesCategories = new Dictionary<string, string>();

						if (!string.IsNullOrEmpty(data.AffiliateType))
							db.AffiliateType = JsonConvert.DeserializeObject<AffiliateType>(data.AffiliateType);

						if (!string.IsNullOrEmpty(data.CurrencyArray))
							db.CurrencyArray = JsonConvert.DeserializeObject<List<string>>(data.CurrencyArray);

						if (!string.IsNullOrEmpty(data.CurrencySymbolArray))
							db.CurrencySymbolArray = JsonConvert.DeserializeObject<GetSettingsObject.CurrencySymbolArray>(data.CurrencySymbolArray);

						if (!string.IsNullOrEmpty(data.Continents))
							db.Continents = JsonConvert.DeserializeObject<List<string>>(data.Continents);

						if (!string.IsNullOrEmpty(data.Categories))
							db.Categories = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.Categories);

						if (!string.IsNullOrEmpty(data.SubCategories))
							db.SubCategories = new GetSettingsObject.SubCategoriesUnion
							{
								SubCategoriessList = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(data.SubCategories),
							};

						if (!string.IsNullOrEmpty(data.MoviesCategories))
							db.MoviesCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.MoviesCategories);

						Task.Factory.StartNew(() =>
						{
							try
							{
								if (db.Categories?.Count > 0)
								{
									//Categories >> New V.1.6
									var listCategories = db.Categories.Select(cat => new Classes.Category
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

								if (db.SubCategories?.SubCategoriessList?.Count > 0)
								{
									//Sub Categories
									foreach (var sub in db.SubCategories.Value.SubCategoriessList)
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

								if (db.MoviesCategories?.Count > 0)
								{
									//Movies Categories
									var listMovies = db.MoviesCategories.Select(cat => new Classes.Category
									{
										Id = cat.Key,
										Name = Methods.FunString.DecodeString(cat.Value),
										Color = "#212121",
										SubList = new List<Classes.Category>()
									}).ToList();

									CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Category>(listMovies);
								}
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
							}
						});

						return db;
					}

					return null;
				}

				return null;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_Settings();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return null;
				}
			}
		}

		//remove Settings
		public void remove_Settings()
		{
			try
			{
				var connection = OpenConnection();
				connection.DeleteAll<DataTables.MySettingsTb>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					remove_Settings();
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Login

		//Get data Login
		public DataTables.LoginTb Get_data_Login()
		{
			try
			{
				using var connection = OpenConnection();
				if (connection == null) return null;

				var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
				if (dataUser != null)
				{
					UserDetails.Username = dataUser.Username;
					UserDetails.FullName = dataUser.Username;
					UserDetails.Password = dataUser.Password;
					UserDetails.UserId = InitializePlayTube.UserId = dataUser.UserId;
					UserDetails.AccessToken = Current.AccessToken = dataUser.AccessToken;
					UserDetails.Status = dataUser.Status;
					UserDetails.Cookie = dataUser.Cookie;
					UserDetails.Email = dataUser.Email;
					UserDetails.DeviceId = dataUser.DeviceId;

					AppSettings.Lang = dataUser.Lang;

					ListUtils.DataUserLoginList = new ObservableCollection<DataTables.LoginTb>() { dataUser };

					if (!string.IsNullOrEmpty(Current.AccessToken))
						UserDetails.IsLogin = true;

					return dataUser;
				}
				else
				{
					UserDetails.IsLogin = false;
					return null;
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_data_Login();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return null;
				}
			}
		}

		public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
		{
			try
			{
				var connection = OpenConnection();
				var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
				if (dataUser != null)
				{
					dataUser.UserId = UserDetails.UserId;
					dataUser.AccessToken = UserDetails.AccessToken;
					dataUser.Cookie = UserDetails.Cookie;
					dataUser.Username = UserDetails.Username;
					dataUser.Password = UserDetails.Password;
					dataUser.Status = UserDetails.Status;
					dataUser.Lang = AppSettings.Lang;
					dataUser.DeviceId = UserDetails.DeviceId;
					dataUser.Email = UserDetails.Email;

					connection.Update(dataUser);
				}
				else
				{
					connection.Insert(db);
				}

				if (!string.IsNullOrEmpty(Current.AccessToken))
					UserDetails.IsLogin = true;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrUpdateLogin_Credentials(db);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region MyChannel

		//Insert Or Update data MyChannel

		public void InsertOrUpdate_DataMyChannel(UserDataObject channel)
		{
			try
			{
				var connection = OpenConnection();
				var resultChannelTb = connection.Table<DataTables.ChannelTb>().FirstOrDefault();
				if (resultChannelTb != null)
				{
					resultChannelTb = new DataTables.ChannelTb
					{
						Id = channel.Id,
						Username = channel.Username,
						Email = channel.Email,
						IpAddress = channel.IpAddress,
						FirstName = channel.FirstName,
						LastName = channel.LastName,
						Gender = channel.Gender,
						EmailCode = channel.EmailCode,
						DeviceId = channel.DeviceId,
						Language = channel.Language,
						Avatar = channel.Avatar,
						Cover = channel.Cover,
						Src = channel.Src,
						CountryId = channel.CountryId,
						Age = channel.Age,
						About = channel.About,
						Google = channel.Google,
						Facebook = channel.Facebook,
						Twitter = channel.Twitter,
						Instagram = channel.Instagram,
						Active = channel.Active,
						Admin = channel.Admin,
						Verified = channel.Verified,
						LastActive = channel.LastActive,
						Registered = channel.Registered,
						IsPro = channel.IsPro,
						Imports = channel.Imports,
						Uploads = channel.Uploads,
						Wallet = channel.Wallet,
						Balance = channel.Balance,
						VideoMon = channel.VideoMon,
						AgeChanged = channel.AgeChanged,
						DonationPaypalEmail = channel.DonationPaypalEmail,
						UserUploadLimit = channel.UserUploadLimit,
						TwoFactor = channel.TwoFactor,
						LastMonth = channel.LastMonth,
						ActiveTime = channel.ActiveTime,
						ActiveExpire = channel.ActiveExpire,
						PhoneNumber = channel.PhoneNumber,
						Address = channel.Address,
						City = channel.City,
						State = channel.State,
						Zip = channel.Zip,
						SubscriberPrice = channel.SubscriberPrice,
						Monetization = channel.Monetization,
						NewEmail = channel.NewEmail,
						TotalAds = channel.TotalAds,
						SuspendUpload = channel.SuspendUpload,
						SuspendImport = channel.SuspendImport,
						PaystackRef = channel.PaystackRef,
						ConversationId = channel.ConversationId,
						PointDayExpire = channel.PointDayExpire,
						Points = channel.Points,
						DailyPoints = channel.DailyPoints,
						Name = channel.Name,
						ExCover = channel.ExCover,
						Url = channel.Url,
						AboutDecoded = channel.AboutDecoded,
						FullCover = channel.FullCover,
						BalanceOr = channel.BalanceOr,
						NameV = channel.NameV,
						CountryName = channel.CountryName,
						GenderText = channel.GenderText,
						AmISubscribed = channel.AmISubscribed,
						SubscribeCount = channel.SubscribeCount,
						IsSubscribedToChannel = channel.IsSubscribedToChannel,
						Time = channel.Time,
						InfoFile = channel.InfoFile,
						GoogleTrackingCode = channel.GoogleTrackingCode,
						Newsletters = channel.Newsletters,
						ChannelNotify = channel.ChannelNotify,
						AamarpayTranId = channel.AamarpayTranId,
						CoinbaseCode = channel.CoinbaseCode,
						CoinbaseHash = channel.CoinbaseHash,
						CoinpaymentsTxnId = channel.CoinpaymentsTxnId,
						Discord = channel.Discord,
						FortumoHash = channel.FortumoHash,
						LinkedIn = channel.LinkedIn,
						Mailru = channel.Mailru,
						NgeniusRef = channel.NgeniusRef,
						PauseHistory = channel.PauseHistory,
						Qq = channel.Qq,
						SecurionpayKey = channel.SecurionpayKey,
						StripeSessionId = channel.StripeSessionId,
						TvCode = channel.TvCode,
						Vk = channel.Vk,
						Wechat = channel.Wechat,
						YoomoneyHash = channel.YoomoneyHash,
						FavCategory = JsonConvert.SerializeObject(channel.FavCategory),
					};

					if (resultChannelTb != null)
					{
						UserDetails.Avatar = resultChannelTb.Avatar;
						UserDetails.Cover = resultChannelTb.Cover;
						UserDetails.Username = resultChannelTb.Username;
						UserDetails.FullName = AppTools.GetNameFinal(resultChannelTb);

						connection.Update(resultChannelTb);
					}
				}
				else
				{
					var db = new DataTables.ChannelTb
					{
						Id = channel.Id,
						Username = channel.Username,
						Email = channel.Email,
						IpAddress = channel.IpAddress,
						FirstName = channel.FirstName,
						LastName = channel.LastName,
						Gender = channel.Gender,
						EmailCode = channel.EmailCode,
						DeviceId = channel.DeviceId,
						Language = channel.Language,
						Avatar = channel.Avatar,
						Cover = channel.Cover,
						Src = channel.Src,
						CountryId = channel.CountryId,
						Age = channel.Age,
						About = channel.About,
						Google = channel.Google,
						Facebook = channel.Facebook,
						Twitter = channel.Twitter,
						Instagram = channel.Instagram,
						Active = channel.Active,
						Admin = channel.Admin,
						Verified = channel.Verified,
						LastActive = channel.LastActive,
						Registered = channel.Registered,
						IsPro = channel.IsPro,
						Imports = channel.Imports,
						Uploads = channel.Uploads,
						Wallet = channel.Wallet,
						Balance = channel.Balance,
						VideoMon = channel.VideoMon,
						AgeChanged = channel.AgeChanged,
						DonationPaypalEmail = channel.DonationPaypalEmail,
						UserUploadLimit = channel.UserUploadLimit,
						TwoFactor = channel.TwoFactor,
						LastMonth = channel.LastMonth,
						ActiveTime = channel.ActiveTime,
						ActiveExpire = channel.ActiveExpire,
						PhoneNumber = channel.PhoneNumber,
						Address = channel.Address,
						City = channel.City,
						State = channel.State,
						Zip = channel.Zip,
						SubscriberPrice = channel.SubscriberPrice,
						Monetization = channel.Monetization,
						NewEmail = channel.NewEmail,
						TotalAds = channel.TotalAds,
						SuspendUpload = channel.SuspendUpload,
						SuspendImport = channel.SuspendImport,
						PaystackRef = channel.PaystackRef,
						ConversationId = channel.ConversationId,
						PointDayExpire = channel.PointDayExpire,
						Points = channel.Points,
						DailyPoints = channel.DailyPoints,
						Name = channel.Name,
						ExCover = channel.ExCover,
						Url = channel.Url,
						AboutDecoded = channel.AboutDecoded,
						FullCover = channel.FullCover,
						BalanceOr = channel.BalanceOr,
						NameV = channel.NameV,
						CountryName = channel.CountryName,
						GenderText = channel.GenderText,
						AmISubscribed = channel.AmISubscribed,
						SubscribeCount = channel.SubscribeCount,
						IsSubscribedToChannel = channel.IsSubscribedToChannel,
						Time = channel.Time,
						InfoFile = channel.InfoFile,
						GoogleTrackingCode = channel.GoogleTrackingCode,
						Newsletters = channel.Newsletters,
						ChannelNotify = channel.ChannelNotify,
						AamarpayTranId = channel.AamarpayTranId,
						CoinbaseCode = channel.CoinbaseCode,
						CoinbaseHash = channel.CoinbaseHash,
						CoinpaymentsTxnId = channel.CoinpaymentsTxnId,
						Discord = channel.Discord,
						FortumoHash = channel.FortumoHash,
						LinkedIn = channel.LinkedIn,
						Mailru = channel.Mailru,
						NgeniusRef = channel.NgeniusRef,
						PauseHistory = channel.PauseHistory,
						Qq = channel.Qq,
						SecurionpayKey = channel.SecurionpayKey,
						StripeSessionId = channel.StripeSessionId,
						TvCode = channel.TvCode,
						Vk = channel.Vk,
						Wechat = channel.Wechat,
						YoomoneyHash = channel.YoomoneyHash,
						FavCategory = JsonConvert.SerializeObject(channel.FavCategory),
					};

					if (db != null)
					{
						connection.Insert(db);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrUpdate_DataMyChannel(channel);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get Data My Channel
		public UserDataObject GetDataMyChannel()
		{
			try
			{
				var connection = OpenConnection();
				var channel = connection.Table<DataTables.ChannelTb>().FirstOrDefault();
				if (channel != null)
				{
					var db = new UserDataObject
					{
						Id = channel.Id,
						Username = channel.Username,
						Email = channel.Email,
						IpAddress = channel.IpAddress,
						FirstName = channel.FirstName,
						LastName = channel.LastName,
						Gender = channel.Gender,
						EmailCode = channel.EmailCode,
						DeviceId = channel.DeviceId,
						Language = channel.Language,
						Avatar = channel.Avatar,
						Cover = channel.Cover,
						Src = channel.Src,
						CountryId = channel.CountryId,
						Age = channel.Age,
						About = channel.About,
						Google = channel.Google,
						Facebook = channel.Facebook,
						Twitter = channel.Twitter,
						Instagram = channel.Instagram,
						Active = channel.Active,
						Admin = channel.Admin,
						Verified = channel.Verified,
						LastActive = channel.LastActive,
						Registered = channel.Registered,
						IsPro = channel.IsPro,
						Imports = channel.Imports,
						Uploads = channel.Uploads,
						Wallet = channel.Wallet,
						Balance = channel.Balance,
						VideoMon = channel.VideoMon,
						AgeChanged = channel.AgeChanged,
						DonationPaypalEmail = channel.DonationPaypalEmail,
						UserUploadLimit = channel.UserUploadLimit,
						TwoFactor = channel.TwoFactor,
						LastMonth = channel.LastMonth,
						ActiveTime = channel.ActiveTime,
						ActiveExpire = channel.ActiveExpire,
						PhoneNumber = channel.PhoneNumber,
						Address = channel.Address,
						City = channel.City,
						State = channel.State,
						Zip = channel.Zip,
						SubscriberPrice = channel.SubscriberPrice,
						Monetization = channel.Monetization,
						NewEmail = channel.NewEmail,
						TotalAds = channel.TotalAds,
						SuspendUpload = channel.SuspendUpload,
						SuspendImport = channel.SuspendImport,
						PaystackRef = channel.PaystackRef,
						ConversationId = channel.ConversationId,
						PointDayExpire = channel.PointDayExpire,
						Points = channel.Points,
						DailyPoints = channel.DailyPoints,
						Name = channel.Name,
						ExCover = channel.ExCover,
						Url = channel.Url,
						AboutDecoded = channel.AboutDecoded,
						FullCover = channel.FullCover,
						BalanceOr = channel.BalanceOr,
						NameV = channel.NameV,
						CountryName = channel.CountryName,
						GenderText = channel.GenderText,
						AmISubscribed = channel.AmISubscribed,
						SubscribeCount = channel.SubscribeCount,
						IsSubscribedToChannel = channel.IsSubscribedToChannel,
						Time = channel.Time,
						InfoFile = channel.InfoFile,
						GoogleTrackingCode = channel.GoogleTrackingCode,
						Newsletters = channel.Newsletters,
						ChannelNotify = channel.ChannelNotify,
						AamarpayTranId = channel.AamarpayTranId,
						CoinbaseCode = channel.CoinbaseCode,
						CoinbaseHash = channel.CoinbaseHash,
						CoinpaymentsTxnId = channel.CoinpaymentsTxnId,
						Discord = channel.Discord,
						FortumoHash = channel.FortumoHash,
						LinkedIn = channel.LinkedIn,
						Mailru = channel.Mailru,
						NgeniusRef = channel.NgeniusRef,
						PauseHistory = channel.PauseHistory,
						Qq = channel.Qq,
						SecurionpayKey = channel.SecurionpayKey,
						StripeSessionId = channel.StripeSessionId,
						TvCode = channel.TvCode,
						Vk = channel.Vk,
						Wechat = channel.Wechat,
						YoomoneyHash = channel.YoomoneyHash,
						FavCategory = new List<string>(),
					};

					if (db != null)
					{
						if (!string.IsNullOrEmpty(channel.FavCategory))
							db.FavCategory = JsonConvert.DeserializeObject<List<string>>(channel.FavCategory);
						UserDetails.Avatar = db.Avatar;
						UserDetails.Cover = db.Cover;
						UserDetails.Username = db.Username;
						UserDetails.FullName = AppTools.GetNameFinal(db);

						ListUtils.MyChannelList?.Clear();
						ListUtils.MyChannelList?.Add(db);

						return channel;
					}

					return null;
				}

				return null;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetDataMyChannel();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return null;
				}
			}
		}

		#endregion

		#region SubscriptionsChannel Videos

		//Insert SubscriptionsChannel Videos
		public void Insert_One_SubscriptionChannel(UserDataObject channel)
		{
			try
			{
				var connection = OpenConnection();
				if (channel != null)
				{
					var select = connection.Table<DataTables.SubscriptionsChannelTb>().FirstOrDefault(a => a.Id == channel.Id);
					if (select == null)
					{
						var db = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(channel);
						if (db != null)
						{
							db.FavCategory = JsonConvert.SerializeObject(channel.FavCategory);

							connection.Insert(db);
						}
					}
					else
					{
						select = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(channel);
						if (select != null)
						{
							select.FavCategory = JsonConvert.SerializeObject(channel.FavCategory);

							connection.Update(select);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Insert_One_SubscriptionChannel(channel);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Insert SubscriptionsChannel Videos
		public void InsertAllSubscriptionsChannel(ObservableCollection<UserDataObject> channelsList)
		{
			try
			{
				var connection = OpenConnection();
				var result = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();

				var list = new List<DataTables.SubscriptionsChannelTb>();
				foreach (var info in channelsList)
				{
					var db = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(info);
					if (db != null)
					{
						db.FavCategory = JsonConvert.SerializeObject(info.FavCategory);

						list.Add(db);
					}

					var update = result.FirstOrDefault(a => a.Id == info.Id);
					if (update != null)
					{
						update = db;

						update.FavCategory = JsonConvert.SerializeObject(info.FavCategory);

						connection.Update(update);
					}
				}

				if (list.Count <= 0) return;

				connection.BeginTransaction();
				//Bring new  
				var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
				if (newItemList.Count > 0)
					connection.InsertAll(newItemList);

				result = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();
				var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
				if (deleteItemList.Count > 0)
					foreach (var delete in deleteItemList)
						connection.Delete(delete);

				connection?.Commit();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertAllSubscriptionsChannel(channelsList);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Remove SubscriptionsChannel Videos
		public void RemoveSubscriptionsChannel(string subscriptionsChannelId)
		{
			try
			{
				var connection = OpenConnection();
				if (!string.IsNullOrEmpty(subscriptionsChannelId))
				{
					var select = connection.Table<DataTables.SubscriptionsChannelTb>().FirstOrDefault(a => a.Id == subscriptionsChannelId);
					if (select != null)
					{
						connection.Delete(select);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					RemoveSubscriptionsChannel(subscriptionsChannelId);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get SubscriptionsChannel Videos
		public ObservableCollection<UserDataObject> GetSubscriptionsChannel()
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();
				if (select.Count > 0)
				{
					var list = new ObservableCollection<UserDataObject>();
					foreach (var channel in select)
					{
						var db = new UserDataObject
						{
							Id = channel.Id,
							Username = channel.Username,
							Email = channel.Email,
							IpAddress = channel.IpAddress,
							FirstName = channel.FirstName,
							LastName = channel.LastName,
							Gender = channel.Gender,
							EmailCode = channel.EmailCode,
							DeviceId = channel.DeviceId,
							Language = channel.Language,
							Avatar = channel.Avatar,
							Cover = channel.Cover,
							Src = channel.Src,
							CountryId = channel.CountryId,
							Age = channel.Age,
							About = channel.About,
							Google = channel.Google,
							Facebook = channel.Facebook,
							Twitter = channel.Twitter,
							Instagram = channel.Instagram,
							Active = channel.Active,
							Admin = channel.Admin,
							Verified = channel.Verified,
							LastActive = channel.LastActive,
							Registered = channel.Registered,
							IsPro = channel.IsPro,
							Imports = channel.Imports,
							Uploads = channel.Uploads,
							Wallet = channel.Wallet,
							Balance = channel.Balance,
							VideoMon = channel.VideoMon,
							AgeChanged = channel.AgeChanged,
							DonationPaypalEmail = channel.DonationPaypalEmail,
							UserUploadLimit = channel.UserUploadLimit,
							TwoFactor = channel.TwoFactor,
							LastMonth = channel.LastMonth,
							ActiveTime = channel.ActiveTime,
							ActiveExpire = channel.ActiveExpire,
							PhoneNumber = channel.PhoneNumber,
							Address = channel.Address,
							City = channel.City,
							State = channel.State,
							Zip = channel.Zip,
							SubscriberPrice = channel.SubscriberPrice,
							Monetization = channel.Monetization,
							NewEmail = channel.NewEmail,
							TotalAds = channel.TotalAds,
							SuspendUpload = channel.SuspendUpload,
							SuspendImport = channel.SuspendImport,
							PaystackRef = channel.PaystackRef,
							ConversationId = channel.ConversationId,
							PointDayExpire = channel.PointDayExpire,
							Points = channel.Points,
							DailyPoints = channel.DailyPoints,
							Name = channel.Name,
							ExCover = channel.ExCover,
							Url = channel.Url,
							AboutDecoded = channel.AboutDecoded,
							FullCover = channel.FullCover,
							BalanceOr = channel.BalanceOr,
							NameV = channel.NameV,
							CountryName = channel.CountryName,
							GenderText = channel.GenderText,
							AmISubscribed = channel.AmISubscribed,
							SubscribeCount = channel.SubscribeCount,
							IsSubscribedToChannel = channel.IsSubscribedToChannel,
							Time = channel.Time,
							InfoFile = channel.InfoFile,
							GoogleTrackingCode = channel.GoogleTrackingCode,
							Newsletters = channel.Newsletters,
							ChannelNotify = channel.ChannelNotify,
							AamarpayTranId = channel.AamarpayTranId,
							CoinbaseCode = channel.CoinbaseCode,
							CoinbaseHash = channel.CoinbaseHash,
							CoinpaymentsTxnId = channel.CoinpaymentsTxnId,
							Discord = channel.Discord,
							FortumoHash = channel.FortumoHash,
							LinkedIn = channel.LinkedIn,
							Mailru = channel.Mailru,
							NgeniusRef = channel.NgeniusRef,
							PauseHistory = channel.PauseHistory,
							Qq = channel.Qq,
							SecurionpayKey = channel.SecurionpayKey,
							StripeSessionId = channel.StripeSessionId,
							TvCode = channel.TvCode,
							Vk = channel.Vk,
							Wechat = channel.Wechat,
							YoomoneyHash = channel.YoomoneyHash,
							FavCategory = new List<string>(),
						};

						if (db != null)
						{
							if (!string.IsNullOrEmpty(channel.FavCategory))
								db.FavCategory = JsonConvert.DeserializeObject<List<string>>(channel.FavCategory);

							list.Add(db);
						}
					}

					return list;
				}

				return new ObservableCollection<UserDataObject>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetSubscriptionsChannel();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<UserDataObject>();
				}
			}
		}

		#endregion

		#region WatchOffline Videos

		//Insert WatchOffline Videos
		public void Insert_WatchOfflineVideos(VideoDataObject video)
		{
			try
			{
				var connection = OpenConnection();
				if (video != null)
				{
					var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == video.Id);
					if (select == null)
					{
						var db = ClassMapper.Mapper?.Map<DataTables.WatchOfflineVideosTb>(video);
						db.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
						db.PlaylistData = JsonConvert.SerializeObject(video.PlaylistData);
						connection.Insert(db);
					}
					else
					{
						select = ClassMapper.Mapper?.Map<DataTables.WatchOfflineVideosTb>(video);
						select.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
						select.PlaylistData = JsonConvert.SerializeObject(video.PlaylistData);
						connection.Update(select);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Insert_WatchOfflineVideos(video);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Remove WatchOffline Videos
		public void Remove_WatchOfflineVideos(string watchOfflineVideosId)
		{
			try
			{
				var connection = OpenConnection();
				if (!string.IsNullOrEmpty(watchOfflineVideosId))
				{
					var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == watchOfflineVideosId);
					if (select != null)
					{
						connection.Delete(select);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Remove_WatchOfflineVideos(watchOfflineVideosId);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get WatchOffline Videos
		public ObservableCollection<VideoDataObject> Get_WatchOfflineVideos()
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.WatchOfflineVideosTb>().ToList();
				if (select.Count > 0)
				{
					var list = new ObservableCollection<VideoDataObject>();
					foreach (var item in select)
					{
						var db = ClassMapper.Mapper?.Map<VideoDataObject>(item);
						if (db != null)
						{
							if (!string.IsNullOrEmpty(item.Owner))
								db.Owner = new OwnerUnion
								{
									OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
								};

							if (!string.IsNullOrEmpty(item.PlaylistData))
								db.PlaylistData = JsonConvert.DeserializeObject<PlayListVideoObject>(item.PlaylistData);

							list.Add(db);
						}
					}

					return list;
				}

				return new ObservableCollection<VideoDataObject>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_WatchOfflineVideos();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<VideoDataObject>();
				}
			}
		}

		public VideoDataObject Get_LatestWatchOfflineVideos(string id)
		{
			try
			{
				var connection = OpenConnection();
				var item = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == id);
				if (item != null)
				{
					var db = ClassMapper.Mapper?.Map<VideoDataObject>(item);
					if (db != null)
					{
						if (!string.IsNullOrEmpty(item.Owner))
							db.Owner = new OwnerUnion
							{
								OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
							};

						if (!string.IsNullOrEmpty(item.PlaylistData))
							db.PlaylistData = JsonConvert.DeserializeObject<PlayListVideoObject>(item.PlaylistData);

						return db;
					}
				}

				return null;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_LatestWatchOfflineVideos(id);
				else
				{
					Methods.DisplayReportResultTrack(e);
					return null;
				}
			}
		}

		public void Update_WatchOfflineVideos(string videoid, string videopath)
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == videoid);
				if (select != null)
				{
					select.VideoLocation = videopath;
					connection.Update(select);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Update_WatchOfflineVideos(videoid, videopath);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Library Item

		private void AddLibrarySectionViews()
		{
			try
			{
				var connection = OpenConnection();
				var check = connection.Table<DataTables.LibraryItemTb>().ToList();

				if (check == null || check.Count == 0)
				{
					//translate text in the adapter
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "1",
						SectionText = Application.Context.GetText(Resource.String.Lbl_Subscriptions),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_subscriptions_vector,
						BackgroundColor = "#9C27B0",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "2",
						SectionText = Application.Context.GetText(Resource.String.Lbl_WatchLater),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_time_vector,
						BackgroundColor = "#2196F3",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "3",
						SectionText = Application.Context.GetText(Resource.String.Lbl_RecentlyWatched),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_recently_vector,
						BackgroundColor = "#E91E63",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "4",
						SectionText = Application.Context.GetText(Resource.String.Lbl_WatchOffline),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_download_vector,
						BackgroundColor = "#009688",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "5",
						SectionText = Application.Context.GetText(Resource.String.Lbl_PlayLists),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_playList_vector,
						BackgroundColor = "#F44336",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "6",
						SectionText = Application.Context.GetText(Resource.String.Lbl_Liked),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_likefilled_video_vector,
						BackgroundColor = "#3F51B5",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "7",
						SectionText = Application.Context.GetText(Resource.String.Lbl_Shared),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_share_post_vector,
						BackgroundColor = "#B71C1C",
					});
					ListUtils.LibraryList.Add(new Classes.LibraryItem
					{
						SectionId = "8",
						SectionText = Application.Context.GetText(Resource.String.Lbl_Paid),
						VideoCount = 0,
						BackgroundImage = "blackdefault",
						Icon = Resource.Drawable.icon_dollars_vector,
						BackgroundColor = "#6D4C41",
					});

					InsertLibraryItem(ListUtils.LibraryList);
				}
				else
				{
					ListUtils.LibraryList = new ObservableCollection<Classes.LibraryItem>();
					foreach (var item in check)
					{
						ListUtils.LibraryList.Add(new Classes.LibraryItem
						{
							SectionId = item.SectionId,
							SectionText = item.SectionText,
							VideoCount = item.VideoCount,
							BackgroundImage = item.BackgroundImage,
							Icon = item.Icon,
							BackgroundColor = item.BackgroundColor,
						});
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Insert data LibraryItem
		public void InsertLibraryItem(Classes.LibraryItem libraryItem)
		{
			try
			{
				var connection = OpenConnection();
				if (libraryItem == null)
					return;
				var select = connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
				if (select != null)
				{
					select.VideoCount = libraryItem.VideoCount;
					select.BackgroundImage = libraryItem.BackgroundImage;
					connection.Update(select);
				}
				else
				{
					var item = new DataTables.LibraryItemTb
					{
						SectionId = libraryItem.SectionId,
						SectionText = libraryItem.SectionText,
						VideoCount = libraryItem.VideoCount,
						BackgroundImage = libraryItem.BackgroundImage,
						Icon = libraryItem.Icon,
						BackgroundColor = libraryItem.BackgroundColor,
					};
					connection.Insert(item);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertLibraryItem(libraryItem);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Insert data LibraryItem
		public void InsertLibraryItem(ObservableCollection<Classes.LibraryItem> libraryList)
		{
			try
			{
				var connection = OpenConnection();
				if (libraryList?.Count == 0)
					return;
				if (libraryList != null)
				{
					foreach (var libraryItem in libraryList)
					{
						var select = connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
						if (select != null)
						{
							select.SectionId = libraryItem.SectionId;
							select.SectionText = libraryItem.SectionText;
							select.VideoCount = libraryItem.VideoCount;
							select.BackgroundImage = libraryItem.BackgroundImage;
							select.Icon = libraryItem.Icon;
							select.BackgroundColor = libraryItem.BackgroundColor;

							connection.Update(select);
						}
						else
						{
							var item = new DataTables.LibraryItemTb
							{
								SectionId = libraryItem.SectionId,
								SectionText = libraryItem.SectionText,
								VideoCount = libraryItem.VideoCount,
								BackgroundImage = libraryItem.BackgroundImage,
								Icon = libraryItem.Icon,
								BackgroundColor = libraryItem.BackgroundColor,
							};
							connection.Insert(item);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertLibraryItem(libraryList);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get data LibraryItem
		public ObservableCollection<DataTables.LibraryItemTb> Get_LibraryItem()
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.LibraryItemTb>().OrderBy(a => a.SectionId).ToList();
				if (select.Count > 0)
				{
					return new ObservableCollection<DataTables.LibraryItemTb>(select);
				}

				return new ObservableCollection<DataTables.LibraryItemTb>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_LibraryItem();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<DataTables.LibraryItemTb>();
				}
			}
		}

		#endregion

		#region Shared Videos

		//Insert Shared Videos
		public void Insert_SharedVideos(VideoDataObject video)
		{
			try
			{
				var connection = OpenConnection();
				if (video != null)
				{
					var select = connection.Table<DataTables.SharedVideosTb>().FirstOrDefault(a => a.VideoId == video.VideoId);
					if (select == null)
					{
						var db = ClassMapper.Mapper?.Map<DataTables.SharedVideosTb>(video);
						db.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
						db.PlaylistData = JsonConvert.SerializeObject(video.PlaylistData);
						connection.Insert(db);
					}
					else
					{
						select = ClassMapper.Mapper?.Map<DataTables.SharedVideosTb>(video);
						select.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
						select.PlaylistData = JsonConvert.SerializeObject(video.PlaylistData);
						connection.Update(select);
					}
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Insert_SharedVideos(video);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get Shared Videos
		public ObservableCollection<VideoDataObject> Get_SharedVideos()
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.SharedVideosTb>().ToList();
				if (select.Count > 0)
				{
					var list = new ObservableCollection<VideoDataObject>();
					foreach (var item in select)
					{
						var db = ClassMapper.Mapper?.Map<VideoDataObject>(item);
						if (db != null)
						{
							if (!string.IsNullOrEmpty(item.Owner))
								db.Owner = new OwnerUnion
								{
									OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
								};

							if (!string.IsNullOrEmpty(item.PlaylistData))
								db.PlaylistData = JsonConvert.DeserializeObject<PlayListVideoObject>(item.PlaylistData);

							list.Add(db);
						}
					}

					return list;
				}

				return new ObservableCollection<VideoDataObject>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return Get_SharedVideos();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<VideoDataObject>();
				}
			}
		}

		#endregion

		#region Last Chat

		//Insert data To Last Chat Table
		public void InsertOrReplaceLastChatTable(ObservableCollection<GetChatsObject.Data> usersContactList)
		{
			try
			{
				var connection = OpenConnection();
				var result = connection.Table<DataTables.LastChatTb>().ToList();
				List<DataTables.LastChatTb> list = new List<DataTables.LastChatTb>();
				foreach (var info in usersContactList)
				{
					var user = new DataTables.LastChatTb
					{
						Id = info.Id,
						UserOne = info.UserOne,
						UserTwo = info.UserTwo,
						Time = info.Time,
						TextTime = info.TextTime,
						GetCountSeen = info.GetCountSeen,
						UserDataJson = JsonConvert.SerializeObject(info.User),
						GetLastMessageJson = JsonConvert.SerializeObject(info.GetLastMessage),
					};

					list.Add(user);

					var update = result.FirstOrDefault(a => a.Id == info.Id);
					if (update != null)
					{
						update = user;
						connection.Update(update);
					}
				}

				if (list.Count > 0)
				{
					connection.BeginTransaction();
					//Bring new  
					var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
					if (newItemList.Count > 0)
					{
						connection.InsertAll(newItemList);
					}

					var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
					if (deleteItemList.Count > 0)
					{
						foreach (var delete in deleteItemList)
						{
							connection.Delete(delete);
						}
					}

					connection?.Commit();
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrReplaceLastChatTable(usersContactList);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get data To LastChat Table
		public ObservableCollection<GetChatsObject.Data> GetAllLastChat()
		{
			try
			{
				var connection = OpenConnection();
				var select = connection.Table<DataTables.LastChatTb>().ToList();
				if (select.Count > 0)
				{
					var list = select.Select(user => new GetChatsObject.Data
					{
						Id = user.Id,
						UserOne = user.UserOne,
						UserTwo = user.UserTwo,
						Time = user.Time,
						TextTime = user.TextTime,
						GetCountSeen = user.GetCountSeen,
						User = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
						GetLastMessage =
							JsonConvert.DeserializeObject<GetChatsObject.GetLastMessage>(user.GetLastMessageJson),
					}).ToList();

					return new ObservableCollection<GetChatsObject.Data>(list);
				}

				return new ObservableCollection<GetChatsObject.Data>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetAllLastChat();
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<GetChatsObject.Data>();
				}
			}
		}

		// Get data To LastChat Table By Id >> Load More
		public ObservableCollection<GetChatsObject.Data> GetLastChatById(int id, int nSize)
		{
			try
			{
				var connection = OpenConnection();
				var query = connection.Table<DataTables.LastChatTb>().Where(w => w.AutoIdLastChat >= id)
					.OrderBy(q => q.AutoIdLastChat).Take(nSize).ToList();
				if (query.Count > 0)
				{
					var list = query.Select(user => new GetChatsObject.Data
					{
						Id = user.Id,
						UserOne = user.UserOne,
						UserTwo = user.UserTwo,
						Time = user.Time,
						TextTime = user.TextTime,
						GetCountSeen = user.GetCountSeen,
						User = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
						GetLastMessage =
							JsonConvert.DeserializeObject<GetChatsObject.GetLastMessage>(user.GetLastMessageJson),
					}).ToList();

					if (list.Count > 0)
						return new ObservableCollection<GetChatsObject.Data>(list);
					return null;
				}

				return new ObservableCollection<GetChatsObject.Data>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetLastChatById(id, nSize);
				else
				{
					Methods.DisplayReportResultTrack(e);
					return new ObservableCollection<GetChatsObject.Data>();
				}
			}
		}

		//Remove data To LastChat Table
		public void DeleteUserLastChat(string userId)
		{
			try
			{
				var connection = OpenConnection();
				var user = connection.Table<DataTables.LastChatTb>().FirstOrDefault(c => c.UserTwo == userId);
				if (user != null)
				{
					connection.Delete(user);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					DeleteUserLastChat(userId);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Clear All data LastChat
		public void ClearLastChat()
		{
			try
			{
				var connection = OpenConnection();
				connection.DeleteAll<DataTables.LastChatTb>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					ClearLastChat();
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Message

		//Insert data To Message Table
		public void InsertOrReplaceMessages(ObservableCollection<GetUserMessagesObject.Message> messageList)
		{
			try
			{
				var connection = OpenConnection();
				var listOfDatabaseForInsert = new List<DataTables.MessageTb>();

				// get data from database
				var resultMessage = connection.Table<DataTables.MessageTb>().ToList();
				var listAllMessage = resultMessage.Select(messages => new GetUserMessagesObject.Message
				{
					Id = messages.Id,
					FromId = messages.FromId,
					ToId = messages.ToId,
					Text = messages.Text,
					Seen = messages.Seen,
					Time = messages.Time,
					FromDeleted = messages.FromDeleted,
					ToDeleted = messages.ToDeleted,
					TextTime = messages.TextTime,
					Position = messages.Position,
				}).ToList();

				foreach (var messages in messageList)
				{
					var maTb = new DataTables.MessageTb
					{
						Id = messages.Id,
						FromId = messages.FromId,
						ToId = messages.ToId,
						Text = messages.Text,
						Seen = messages.Seen,
						Time = messages.Time,
						FromDeleted = messages.FromDeleted,
						ToDeleted = messages.ToDeleted,
						TextTime = messages.TextTime,
						Position = messages.Position,
					};

					var dataCheck = listAllMessage.FirstOrDefault(a => a.Id == messages.Id);
					if (dataCheck != null)
					{
						var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
						if (checkForUpdate != null)
						{
							checkForUpdate.Id = messages.Id;
							checkForUpdate.FromId = messages.FromId;
							checkForUpdate.ToId = messages.ToId;
							checkForUpdate.Text = messages.Text;
							checkForUpdate.Seen = messages.Seen;
							checkForUpdate.Time = messages.Time;
							checkForUpdate.FromDeleted = messages.FromDeleted;
							checkForUpdate.ToDeleted = messages.ToDeleted;
							checkForUpdate.TextTime = messages.TextTime;
							checkForUpdate.Position = messages.Position;

							connection.Update(checkForUpdate);
						}
						else
						{
							listOfDatabaseForInsert.Add(maTb);
						}
					}
					else
					{
						listOfDatabaseForInsert.Add(maTb);
					}
				}

				connection.BeginTransaction();

				//Bring new  
				if (listOfDatabaseForInsert.Count > 0)
				{
					connection.InsertAll(listOfDatabaseForInsert);
				}

				connection?.Commit();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrReplaceMessages(messageList);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Update one Messages Table
		public void InsertOrUpdateToOneMessages(GetUserMessagesObject.Message messages)
		{
			try
			{
				var connection = OpenConnection();
				var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == messages.Id);
				if (data != null)
				{
					data.Id = messages.Id;
					data.FromId = messages.FromId;
					data.ToId = messages.ToId;
					data.Text = messages.Text;
					data.Seen = messages.Seen;
					data.Time = messages.Time;
					data.FromDeleted = messages.FromDeleted;
					data.ToDeleted = messages.ToDeleted;
					data.TextTime = messages.TextTime;
					data.Position = messages.Position;

					connection.Update(data);
				}
				else
				{
					var mdb = new DataTables.MessageTb
					{
						Id = messages.Id,
						FromId = messages.FromId,
						ToId = messages.ToId,
						Text = messages.Text,
						Seen = messages.Seen,
						Time = messages.Time,
						FromDeleted = messages.FromDeleted,
						ToDeleted = messages.ToDeleted,
						TextTime = messages.TextTime,
						Position = messages.Position,
					};

					//Insert  one Messages Table
					connection.Insert(mdb);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					InsertOrUpdateToOneMessages(messages);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Get data To Messages
		public string GetMessagesList(string fromId, string toId, string beforeMessageId)
		{
			try
			{
				var connection = OpenConnection();
				var beforeQ = "";
				if (beforeMessageId != "0")
				{
					beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
				}

				var query = connection.Query<DataTables.MessageTb>(
					"SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +
					toId + " and ToId=" + fromId + ")) " + beforeQ);
				var queryList = query
					.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
					.OrderBy(q => q.Time).TakeLast(35).ToList();
				if (queryList.Count > 0)
				{
					foreach (var messages in queryList)
					{
						var m = new GetUserMessagesObject.Message
						{
							Id = messages.Id,
							FromId = messages.FromId,
							ToId = messages.ToId,
							Text = messages.Text,
							Seen = messages.Seen,
							Time = messages.Time,
							FromDeleted = messages.FromDeleted,
							ToDeleted = messages.ToDeleted,
							TextTime = messages.TextTime,
							Position = messages.Position,
						};

						if (beforeMessageId == "0")
						{
							MessagesBoxActivity.MAdapter?.Add(m);
						}
						else
						{
							MessagesBoxActivity.MAdapter?.Insert(m, beforeMessageId);
						}
					}

					return "1";
				}

				return "0";
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetMessagesList(fromId, toId, beforeMessageId);
				else
				{
					Methods.DisplayReportResultTrack(e);
					return "0";
				}
			}
		}

		//Get data To where first Messages >> load more
		public List<DataTables.MessageTb> GetMessageList(string fromId, string toId, string beforeMessageId)
		{
			try
			{
				var connection = OpenConnection();
				var beforeQ = "";
				if (beforeMessageId != "0")
				{
					beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
				}

				var query = connection.Query<DataTables.MessageTb>(
					"SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +
					toId + " and ToId=" + fromId + ")) " + beforeQ);
				var queryList = query
					.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
					.OrderBy(q => q.Time).TakeLast(35).ToList();
				return queryList;
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					return GetMessageList(fromId, toId, beforeMessageId);
				else
				{
					Methods.DisplayReportResultTrack(e);
					return null;
				}
			}
		}

		//Remove data To Messages Table
		public void Delete_OneMessageUser(string messageId)
		{
			try
			{
				var connection = OpenConnection();
				var user = connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
				if (user != null)
				{
					connection.Delete(user);
				}
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					Delete_OneMessageUser(messageId);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		public void DeleteAllMessagesUser(string fromId, string toId)
		{
			try
			{
				var connection = OpenConnection();
				var query = connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + "))");
				Console.WriteLine(query);
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					DeleteAllMessagesUser(fromId, toId);
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		//Remove All data To Messages Table
		public void ClearAll_Messages()
		{
			try
			{
				var connection = OpenConnection();
				connection.DeleteAll<DataTables.MessageTb>();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("database is locked"))
					ClearAll_Messages();
				else
					Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion
	}
}