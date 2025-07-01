using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;
using Java.Util;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Channel
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class EditMyChannelActivity : BaseActivity, IDialogListCallBack
	{
		#region Variables Basic

		private ImageView ImageCover, ImageAvatar;
		private LinearLayout ChangeCoverLayout;
		private RelativeLayout ChangeAvatarLayout;

		private TextInputEditText TxtUsername, TxtFullName, TxtEmail, TxtAbout, TxtFavCategory, TxtGender, TxtAge, TxtCountry, TxtFacebook, TxtTwitter;
		private AppCompatButton SaveButton;

		private AdManagerAdView AdManagerAdView;
		private string ImageType, GenderStatus, Age, CountryId;
		private string CategoryId, CategoryName, DialogType;
		private List<string> CategorySelect = new List<string>();


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
				SetContentView(Resource.Layout.EditMyChannelLayout);

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				Get_Data_User();

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
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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

		protected override void OnDestroy()
		{
			try
			{
				AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
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
				ImageCover = FindViewById<ImageView>(Resource.Id.imageCover);
				ChangeCoverLayout = FindViewById<LinearLayout>(Resource.Id.ChangeCoverLayout);

				ImageAvatar = FindViewById<ImageView>(Resource.Id.imageAvatar);
				ChangeAvatarLayout = FindViewById<RelativeLayout>(Resource.Id.ChangeAvatarLayout);

				TxtUsername = FindViewById<TextInputEditText>(Resource.Id.usernameEdit);
				TxtFullName = FindViewById<TextInputEditText>(Resource.Id.FullNameEdit);
				TxtEmail = FindViewById<TextInputEditText>(Resource.Id.emailEdit);
				TxtAbout = FindViewById<TextInputEditText>(Resource.Id.aboutEdit);
				TxtFavCategory = FindViewById<TextInputEditText>(Resource.Id.favCategoryEdit);
				TxtGender = FindViewById<TextInputEditText>(Resource.Id.genderEdit);
				TxtAge = FindViewById<TextInputEditText>(Resource.Id.ageEdit);
				TxtCountry = FindViewById<TextInputEditText>(Resource.Id.countryEdit);
				TxtFacebook = FindViewById<TextInputEditText>(Resource.Id.facebookEdit);
				TxtTwitter = FindViewById<TextInputEditText>(Resource.Id.twitterEdit);

				SaveButton = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

				AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
				AdsGoogle.InitAdManagerAdView(AdManagerAdView);

				Methods.SetColorEditText(TxtUsername, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtFullName, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtEmail, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtAbout, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtFavCategory, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtGender, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtAge, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtCountry, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtFacebook, AppTools.IsTabDark() ? Color.White : Color.Black);
				Methods.SetColorEditText(TxtTwitter, AppTools.IsTabDark() ? Color.White : Color.Black);

				Methods.SetFocusable(TxtFavCategory);
				Methods.SetFocusable(TxtGender);
				Methods.SetFocusable(TxtAge);
				Methods.SetFocusable(TxtCountry);

				TxtFavCategory.SetMaxLines(1);
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
					ChangeCoverLayout.Click += ChangeCoverLayoutOnClick;
					ChangeAvatarLayout.Click += ChangeAvatarLayoutOnClick;
					TxtFavCategory.Touch += TxtFavCategoryOnTouch;
					TxtGender.Touch += TxtGenderOnTouch;
					TxtAge.Touch += TxtAgeOnTouch;
					TxtCountry.Touch += TxtCountryOnTouch;
					SaveButton.Click += SaveButtonOnClick;
				}
				else
				{
					ChangeCoverLayout.Click -= ChangeCoverLayoutOnClick;
					ChangeAvatarLayout.Click -= ChangeAvatarLayoutOnClick;
					TxtFavCategory.Touch -= TxtFavCategoryOnTouch;
					TxtGender.Touch -= TxtGenderOnTouch;
					TxtAge.Touch -= TxtAgeOnTouch;
					TxtCountry.Touch -= TxtCountryOnTouch;
					SaveButton.Click -= SaveButtonOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void ChangeAvatarLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				ImageType = "Avatar";
				OpenDialogGallery();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void ChangeCoverLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				ImageType = "Cover";
				OpenDialogGallery();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtFavCategoryOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                HideKeyboard();

                DialogType = "FavCategory";

				//var dialogList = new MaterialAlertDialogBuilder(this);

				//var arrayAdapter = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();
				//var arrayIndexAdapter = new List<int>();
				//if (CategorySelect?.Count > 0)
				//{
				//    arrayIndexAdapter.AddRange(CategorySelect.Select(t => CategoriesController.ListCategories.IndexOf(CategoriesController.ListCategories.FirstOrDefault(c => c.Id == t))));
				//}
				//else
				//{
				//    var local = ListUtils.MyChannelList?.FirstOrDefault();
				//    if (local?.FavCategory?.Count > 0)
				//    {
				//        arrayIndexAdapter.AddRange(local?.FavCategory.Select(t => CategoriesController.ListCategories.IndexOf(CategoriesController.ListCategories.FirstOrDefault(c => c.Id == t))));
				//    }
				//}

				//dialogList.SetTitle(GetText(Resource.String.Lbl_ChooseFavCategory))
				//    .SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this))
				//    .ItemsCallbackMultiChoice(arrayIndexAdapter.ToArray(), this)
				//    .AlwaysCallMultiChoiceCallback()
				//    .SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils())
				//    .Show();
				 
				var countriesArray = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();

				var checkedItems = new bool[countriesArray.Count];
				var selectedItems = new List<string>(countriesArray);

				var dialogList = new MaterialAlertDialogBuilder(this);

				dialogList.SetTitle(Resource.String.Lbl_ChooseFavCategory);
				dialogList.SetCancelable(false);
				dialogList.SetMultiChoiceItems(countriesArray.ToArray(), checkedItems, (o, args) =>
				{
					try
					{
						checkedItems[args.Which] = args.IsChecked;

						var text = selectedItems[args.Which] ?? "";
						Console.WriteLine(text);
					}
					catch (Exception exception)
					{
						Methods.DisplayReportResultTrack(exception);
					}
				});
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Save), (o, args) =>
                {
                    try
                    {
                        CategoryId = "";
                        CategoryName = "";
                        CategorySelect = new List<string>();

                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];

                                CategoryId += CategoriesController.ListCategories[i].Id + ",";
                                CategoryName += CategoriesController.ListCategories[i].Name + ",";

                                CategorySelect.Add(CategoryId);
                            }
                        }

                        TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetNeutralButton(Resource.String.Lbl_SelectAll, (o, args) =>
				{
					try
					{
						Arrays.Fill(checkedItems, true);

						CategoryId = "";
						CategoryName = "";
						CategorySelect = new List<string>();

						foreach (var item in CategoriesController.ListCategories)
						{
							CategoryId += item.Id + ",";
							CategoryName += item.Name + ",";

							CategorySelect.Add(CategoryId);
						}

						TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
					}
					catch (Exception ex)
					{
						Methods.DisplayReportResultTrack(ex);
					}
				});

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                HideKeyboard();
				 
                DialogType = "Country";

				var dialogList = new MaterialAlertDialogBuilder(this);

				var countriesArray = AppTools.GetCountryList(this);
				var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

				dialogList.SetTitle(GetText(Resource.String.Lbl_Country));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtAgeOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                HideKeyboard(); 

                DialogType = "Age";

				var arrayAdapter = Enumerable.Range(1, 99).Select(i => i.ToString()).ToList();
				var dialogList = new MaterialAlertDialogBuilder(this);

				dialogList.SetTitle(GetText(Resource.String.Lbl_Age));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                HideKeyboard();

                DialogType = "Gender";
				List<string> arrayAdapter = new List<string>();
				MaterialAlertDialogBuilder dialogList = new MaterialAlertDialogBuilder(this);

				arrayAdapter.Add(GetText(Resource.String.Radio_Male));
				arrayAdapter.Add(GetText(Resource.String.Radio_Female));

				dialogList.SetTitle(GetText(Resource.String.Lbl_Gender));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private async void SaveButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					//Show a progress
					AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

					string first = "", last = "";
					var name = TxtFullName.Text?.Split(' ');
					if (name?.Length > 0)
					{
						first = name.FirstOrDefault();
						last = name.LastOrDefault();
					}

					var dictionary = new Dictionary<string, string>
					{
						{"settings_type", "general"},
						{"username", TxtUsername.Text},
						{"email", TxtEmail.Text},
						{"first_name", first},
						{"last_name", last},
						{"about", TxtAbout.Text},
						{"facebook", TxtFacebook.Text},
						{"twitter", TxtTwitter.Text},
                        //{"google", TxtGoogle.Text},
                        {"gender", GenderStatus},
						{"age", Age},
						{"fav_category", CategoryId},
						{"country", CountryId}
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
								local.Username = UserDetails.Username = TxtUsername.Text;
								local.Email = UserDetails.Email = TxtEmail.Text;
								local.FirstName = first;
								local.LastName = last;
								local.About = TxtAbout.Text;
								local.Gender = GenderStatus;
								local.Facebook = TxtFacebook.Text;
								local.Twitter = TxtTwitter.Text;
								//local.Google = TxtGoogle.Text;
								local.FavCategory = CategorySelect;
								local.Age = Age;
								local.CountryId = CountryId;
								local.CountryName = TxtCountry.Text;

								var database = new SqLiteDatabase();
								database.InsertOrUpdate_DataMyChannel(local);
							}

							Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short)?.Show();
							AndHUD.Shared.Dismiss();

							Intent intent = new Intent();
							SetResult(Result.Ok, intent);
							Finish();
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
				AndHUD.Shared.Dismiss();
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Permissions && Result

		//Result
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			try
			{
				base.OnActivityResult(requestCode, resultCode, data);

				if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
				{
					var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
					if (listPath?.List?.Count > 0)
					{
						var filepath = listPath.List.FirstOrDefault();
						if (!string.IsNullOrEmpty(filepath))
						{
							//Do something with your Uri
							if (ImageType == "Avatar")
							{
								Glide.With(this).Load(filepath).Apply(GlideImageLoader.GetOptions(ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser)).Into(ImageAvatar);
							}
							else if (ImageType == "Cover")
							{
								Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageCover);
							}

							//Send image function
							if (Methods.CheckConnectivity())
								PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataImageAsync(filepath, ImageType.ToLower()) });
							else
								Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			try
			{
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

				if (requestCode == 108) //Image Picker
				{
					if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
					{
						//Open Image 
						OpenDialogGallery();
					}
					else
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		#endregion

		#region MaterialDialog

		public void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				switch (DialogType)
				{
					case "Gender":
						TxtGender.Text = itemString;
						GenderStatus = position == 0 ? "male" : "female";
						break;
					case "Age":
						TxtAge.Text = itemString;
						Age = itemString;
						break;
					case "Country":
						var countriesArray = AppTools.GetCountryList(this);
						var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
						if (check != null)
						{
							CountryId = check;
						}

						TxtCountry.Text = itemString;
						break;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Dialog Gallery

		public void OpenDialogGallery(bool allowVideo = false, bool allowMultiple = false)
		{
			try
			{
				OptionPixImage optionPixImage = OptionPixImage.GetOptionPixImage(allowVideo, allowMultiple);

				// Check if we're running on Android 5.0 or higher
				if ((int)Build.VERSION.SdkInt < 23)
				{
					Intent intent = new Intent(this, typeof(PixImagePickerActivity));
					intent.PutExtra("OptionPixImage", JsonConvert.SerializeObject(optionPixImage));
					StartActivityForResult(intent, PixImagePickerActivity.RequestCode);
				}
				else
				{
					if (PermissionsController.CheckPermissionStorage(this, "file") && ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
					{
						Intent intent = new Intent(this, typeof(PixImagePickerActivity));
						intent.PutExtra("OptionPixImage", JsonConvert.SerializeObject(optionPixImage));
						StartActivityForResult(intent, PixImagePickerActivity.RequestCode);
					}
					else
					{
						new PermissionsController(this).RequestPermission(108, "file");
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

        #endregion

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

        private async void Get_Data_User()
		{
			try
			{
				if (ListUtils.MyChannelList?.Count == 0)
					await ApiRequest.GetChannelData(this, UserDetails.UserId);

				var local = ListUtils.MyChannelList?.FirstOrDefault();
				if (local != null)
				{
					GlideImageLoader.LoadImage(this, local.Avatar, ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
					GlideImageLoader.LoadImage(this, local.Cover, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

					if (local.Gender == "male" || local.Gender == "Male")
					{
						GenderStatus = "male";
						TxtGender.Text = GetText(Resource.String.Radio_Male);
					}
					else
					{

						GenderStatus = "female";
						TxtGender.Text = GetText(Resource.String.Radio_Female);
					}

					TxtUsername.Text = local.Username;
					TxtEmail.Text = local.Email;
					TxtFullName.Text = local.FirstName + " " + local.LastName;
					TxtAbout.Text = local.About;
					TxtFacebook.Text = local.Facebook;
					TxtTwitter.Text = local.Twitter;

					TxtAge.Text = local.Age == "0" ? GetText(Resource.String.Lbl_Age) : local.Age;
					Age = local.Age;

					TxtCountry.Text = local.CountryName;
					CountryId = local.CountryId;

					if (local?.FavCategory?.Count > 0)
					{
						CategorySelect = local.FavCategory;
						foreach (var t in local.FavCategory)
						{
							CategoryId += t + ",";
							CategoryName += CategoriesController.ListCategories.FirstOrDefault(q => q.Id == t)?.Name + ",";
						}

						TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
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