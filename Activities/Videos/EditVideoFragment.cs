using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AT.Markushi.UI;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = System.Console;
using Exception = System.Exception;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Videos
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class EditVideoFragment : Fragment, IDialogListCallBack
	{
		#region Variables Basic

		public ImageView Image;
		private CircleButton BtnClose;
		private AppCompatButton BtnSelectImage, BtnSave;
		private TextView IconTitle, IconDescription, IconTags, IconCategory, IconPrivacy, IconAgeRestriction, DeleteVideoButton;
		private EditText TitleEditText, DescriptionEditText, TagsEditText, CategoryEditText, PrivacyEditText, AgeRestrictionEditText;
		private string Privacy = "0", TypeDialog = "", IdCategory = "", IdAgeRestriction = "";
		public string PathImage = "";
		private VideoDataObject VideoClass;
		private TabbedMainActivity GlobalContext;
		private static EditVideoFragment Instance;

		#endregion

		#region General

		public override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				GlobalContext = TabbedMainActivity.GetInstance();
				HasOptionsMenu = true;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				// Use this to return your custom view for this Fragment
				View view = inflater?.Inflate(Resource.Layout.EditVideoLayout, container, false);
				return view;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);
				Instance = this;

				//Get Value And Set Toolbar
				InitComponent(view);
				InitToolbar(view);
				AddOrRemoveEvent(true);

				SetData();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
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

		private void InitComponent(View view)
		{
			try
			{
				Image = view.FindViewById<ImageView>(Resource.Id.Image);
				BtnClose = view.FindViewById<CircleButton>(Resource.Id.ImageCircle);
				BtnSelectImage = view.FindViewById<AppCompatButton>(Resource.Id.btn_AddPhoto);

				IconTitle = view.FindViewById<TextView>(Resource.Id.IconTitle);
				TitleEditText = view.FindViewById<EditText>(Resource.Id.TitleEditText);

				IconDescription = view.FindViewById<TextView>(Resource.Id.IconDescription);
				DescriptionEditText = view.FindViewById<EditText>(Resource.Id.DescriptionEditText);

				IconTags = view.FindViewById<TextView>(Resource.Id.IconTags);
				TagsEditText = view.FindViewById<EditText>(Resource.Id.TagsEditText);

				IconCategory = view.FindViewById<TextView>(Resource.Id.IconCategory);
				CategoryEditText = view.FindViewById<EditText>(Resource.Id.CategoryEditText);

				IconPrivacy = view.FindViewById<TextView>(Resource.Id.IconPrivacy);
				PrivacyEditText = view.FindViewById<EditText>(Resource.Id.PrivacyEditText);

				IconAgeRestriction = view.FindViewById<TextView>(Resource.Id.IconAgeRestriction);
				AgeRestrictionEditText = view.FindViewById<EditText>(Resource.Id.AgeRestrictionEditText);

				BtnSave = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);
				DeleteVideoButton = view.FindViewById<TextView>(Resource.Id.DeleteVideoButton);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.TextWidth);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTags, FontAwesomeIcon.Tags);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.AudioDescription);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCategory, FontAwesomeIcon.LayerGroup);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPrivacy, FontAwesomeIcon.ShieldAlt);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAgeRestriction, FontAwesomeIcon.User);

				Methods.SetFocusable(CategoryEditText);
				Methods.SetFocusable(PrivacyEditText);
				Methods.SetFocusable(AgeRestrictionEditText);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private void InitToolbar(View view)
		{
			try
			{
				var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					string title = GetString(Resource.String.Lbl_EditVideo);
					GlobalContext.SetToolBar(toolbar, title);
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
					BtnClose.Click += BtnCloseOnClick;
					BtnSelectImage.Click += BtnSelectImageOnClick;
					BtnSave.Click += BtnSaveOnClick;
					CategoryEditText.Touch += CategoryEditTextOnClick;
					PrivacyEditText.Touch += PrivacyEditTextOnClick;
					AgeRestrictionEditText.Touch += AgeRestrictionEditTextOnClick;
					DeleteVideoButton.Click += DeleteVideoButtonOnClick;
				}
				else
				{
					BtnClose.Click -= BtnCloseOnClick;
					BtnSelectImage.Click -= BtnSelectImageOnClick;
					BtnSave.Click -= BtnSaveOnClick;
					CategoryEditText.Touch -= CategoryEditTextOnClick;
					PrivacyEditText.Touch -= PrivacyEditTextOnClick;
					AgeRestrictionEditText.Touch -= AgeRestrictionEditTextOnClick;
					DeleteVideoButton.Click -= DeleteVideoButtonOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static EditVideoFragment GetInstance()
		{
			try
			{
				return Instance;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}

		#endregion

		#region Events

		private async void DeleteVideoButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
				else
				{

					//Show a progress
					AndHUD.Shared.Show(Activity, Activity.GetString(Resource.String.Lbl_Loading));

					var (apiStatus, respond) = await RequestsAsync.Video.DeleteVideosAsync(VideoClass.Id); //Sent api 
					if (apiStatus.Equals(200))
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);

							var mAdapter = TabbedMainActivity.GetInstance()?.MyChannelFragment?.VideosFragment?.MAdapter;
							var data = mAdapter?.VideoList?.FirstOrDefault(a => a.Id == VideoClass.Id);
							if (data != null)
							{
								mAdapter?.VideoList.Remove(data);
								mAdapter?.NotifyDataSetChanged();
							}

							Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();
							AndHUD.Shared.Dismiss();

							GlobalContext.FragmentNavigatorBack();
						}
					}
					else
					{
						Methods.DisplayAndHudErrorResult(Activity, respond);
					}

					AndHUD.Shared.Dismiss();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				AndHUD.Shared.Dismiss();
			}
		}

		//Click Save data  
		private async void BtnSaveOnClick(object sender, EventArgs e)
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
				else
				{
					if (string.IsNullOrEmpty(TitleEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseEnterTitleVideo), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(DescriptionEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseEnterDescriptionVideo), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(TagsEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseEnterTags), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(CategoryEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseChooseCategory), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(PrivacyEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseChoosePrivacy), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(AgeRestrictionEditText.Text))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_PleaseChooseAgeRestriction), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(PathImage))
					{
						Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_Please_select_Image), ToastLength.Short)?.Show();
						return;
					}

					//Show a progress
					AndHUD.Shared.Show(Activity, Activity.GetString(Resource.String.Lbl_Loading));

					var dictionary = new Dictionary<string, string>
					{
						{"title", TitleEditText.Text},
						{"description", DescriptionEditText.Text},
						{"tags", TagsEditText.Text},
						{"category_id", IdCategory},
						{"privacy", Privacy},
						{"age_restriction", IdAgeRestriction},
                        //{"sub_category_id", ""},
                    };

					var (apiStatus, respond) = await RequestsAsync.Video.EditVideoAsync(VideoClass.Id, dictionary, PathImage); //Sent api 
					if (apiStatus.Equals(200))
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);

							var data = TabbedMainActivity.GetInstance()?.MyChannelFragment?.VideosFragment?.MAdapter?.VideoList?.FirstOrDefault(a => a.Id == VideoClass.Id);
							if (data != null)
							{
								data.Title = TitleEditText.Text;
								data.Description = DescriptionEditText.Text;
								data.Tags = TagsEditText.Text;
								data.CategoryName = CategoryEditText.Text;
								data.CategoryId = IdCategory;
								data.Privacy = Privacy;
								data.AgeRestriction = IdAgeRestriction;
								data.Thumbnail = PathImage;

								TabbedMainActivity.GetInstance()?.MyChannelFragment?.VideosFragment?.MAdapter?.NotifyDataSetChanged();
							}

							GlobalPlayerActivity.GetInstance()?.VideoDataWithEventsLoader?.SetDataVideo(data);
							TabbedMainActivity.GetInstance()?.VideoDataWithEventsLoader?.SetDataVideo(data);

							Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_YourVideoSuccessfullyEdited), ToastLength.Short)?.Show();
							AndHUD.Shared.Dismiss();

							GlobalContext.FragmentNavigatorBack();
						}
					}
					else
					{
						Methods.DisplayAndHudErrorResult(Activity, respond);
					}

					AndHUD.Shared.Dismiss();
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				AndHUD.Shared.Dismiss();
			}
		}

		//Open Gallery 
		private void BtnSelectImageOnClick(object sender, EventArgs e)
		{
			try
			{
				GlobalContext.OpenDialogGallery();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Remove image 
		private void BtnCloseOnClick(object sender, EventArgs e)
		{
			try
			{
				PathImage = "";
				GlideImageLoader.LoadImage(Activity, "Grey_Offline", Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Category
		private void CategoryEditTextOnClick(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                GlobalContext?.HideKeyboard();

                TypeDialog = "Category";

				var dialogList = new MaterialAlertDialogBuilder(Activity);

				var arrayAdapter = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();

				dialogList.SetTitle(GetText(Resource.String.Lbl_Category));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Privacy
		private void PrivacyEditTextOnClick(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                GlobalContext?.HideKeyboard();

                TypeDialog = "Privacy";

				var dialogList = new MaterialAlertDialogBuilder(Activity);

				var arrayAdapter = AppTools.GetPrivacyList(Activity).Select(item => item.Value).ToList();

				dialogList.SetTitle(GetText(Resource.String.Lbl_Privacy));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//AgeRestriction
		private void AgeRestrictionEditTextOnClick(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action != MotionEventActions.Up) return;
                GlobalContext?.HideKeyboard();

                TypeDialog = "AgeRestriction";

				var dialogList = new MaterialAlertDialogBuilder(Activity);

				var arrayAdapter = new List<string>
				{
					GetString(Resource.String.Lbl_AgeRestrictionText0),
					GetString(Resource.String.Lbl_AgeRestrictionText1)
				};

				dialogList.SetTitle(GetText(Resource.String.Lbl_AgeRestriction));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
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
				string text = itemString;

				switch (TypeDialog)
				{
					case "Category":
						IdCategory = CategoriesController.ListCategories[position]?.Id;
						CategoryEditText.Text = text;
						break;
					case "Privacy":
						Privacy = AppTools.GetPrivacyList(Activity)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();
						PrivacyEditText.Text = text;
						break;
					case "AgeRestriction":
						{
							if (text == GetString(Resource.String.Lbl_AgeRestrictionText0))
							{
								IdAgeRestriction = "1";
							}
							else if (text == GetString(Resource.String.Lbl_AgeRestrictionText1))
							{
								IdAgeRestriction = "2";
							}
							AgeRestrictionEditText.Text = text;
							break;
						}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

        #endregion

        private void SetData()
		{
			try
			{
				VideoClass = JsonConvert.DeserializeObject<VideoDataObject>(Arguments?.GetString("ItemDataVideo") ?? "");
				if (VideoClass == null) return;
				GlideImageLoader.LoadImage(Activity, VideoClass.Thumbnail, Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

				TitleEditText.Text = Methods.FunString.DecodeString(VideoClass.Title);
				DescriptionEditText.Text = Methods.FunString.DecodeString(VideoClass.Description);
				TagsEditText.Text = VideoClass.Tags;
				CategoryEditText.Text = CategoriesController.GetCategoryName(VideoClass);

				AgeRestrictionEditText.Text = GetString(VideoClass.AgeRestriction == "1" ? Resource.String.Lbl_AgeRestrictionText0 : Resource.String.Lbl_AgeRestrictionText1);

				PrivacyEditText.Text = AppTools.GetPrivacyList(Activity)?.FirstOrDefault(a => a.Key == VideoClass.Privacy).Value.ToString();
			 
				Privacy = VideoClass.Privacy;
				IdCategory = VideoClass.CategoryId;
				IdAgeRestriction = VideoClass.AgeRestriction;
				PathImage = VideoClass.Thumbnail;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

	}
}
