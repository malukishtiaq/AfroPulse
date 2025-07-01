using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using Com.Google.Android.Gms.Ads;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Playlist
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class EditPlaylistActivity : BaseActivity
	{
		#region Variables Basic

		public EditText TxtNewplaylist, TxtDescription;
		public RadioButton RbPrivate, RbPublic;
		public TextView SaveTextView;
		public string Status = "", PlaylistId = "";
		public AdView MAdView;
		public PlayListVideoObject PlayListVideoObject;
		#endregion

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				Methods.App.FullScreenApp(this);

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				// Create your application here
				SetContentView(Resource.Layout.CreatNewPlaylistLayout);

				PlaylistId = Intent?.GetStringExtra("IdList");

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				GetDataPlayList();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		protected override void OnResume()
		{
			try
			{
				base.OnResume();
				AddOrRemoveEvent(true);

				AdsGoogle.LifecycleAdView(MAdView, "Resume");
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

				AdsGoogle.LifecycleAdView(MAdView, "Pause");
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
				AdsGoogle.LifecycleAdView(MAdView, "Destroy");
				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void GetDataPlayList()
		{
			try
			{
				PlayListVideoObject = JsonConvert.DeserializeObject<PlayListVideoObject>(Intent?.GetStringExtra("Item"));
				if (PlayListVideoObject != null)
				{
					TxtNewplaylist.Text = PlayListVideoObject.Name;
					TxtDescription.Text = PlayListVideoObject.Description;

					if (PlayListVideoObject.Privacy == 1)
					{
						RbPublic.Checked = true;
						RbPrivate.Checked = false;
						Status = "1";
					}
					else
					{
						RbPublic.Checked = false;
						RbPrivate.Checked = true;
						Status = "0";
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

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
				//Get values
				TxtNewplaylist = FindViewById<EditText>(Resource.Id.nameplaylist_Edit);
				TxtDescription = FindViewById<EditText>(Resource.Id.description_Edit);
				RbPrivate = (RadioButton)FindViewById(Resource.Id.radioPrivate);
				RbPublic = (RadioButton)FindViewById(Resource.Id.radioPublic);

				SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
				SaveTextView.Click += SaveTextViewOnClick;

				MAdView = FindViewById<AdView>(Resource.Id.adView);
				AdsGoogle.InitAdView(MAdView, null);
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
				//Set ToolBar
				var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolbar != null)
				{
					toolbar.Title = GetText(Resource.String.Lbl_Creat_New_Playlist);
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

		public void AddOrRemoveEvent(bool addEvent)
		{
			try
			{
				// true +=  // false -=
				if (addEvent)
				{
					//Event  
					RbPrivate.CheckedChange += RbPrivateOnCheckedChange;
					RbPublic.CheckedChange += RbPublicOnCheckedChange;
				}
				else
				{
					//Event  
					RbPrivate.CheckedChange -= RbPrivateOnCheckedChange;
					RbPublic.CheckedChange -= RbPublicOnCheckedChange;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private void SaveTextViewOnClick(object sender, EventArgs e)
		{
			SaveDataButtonOnClick();
		}

		//Public
		private void RbPublicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
		{
			try
			{
				bool isChecked = RbPublic.Checked;
				if (isChecked)
				{
					RbPrivate.Checked = false;
					Status = "1";
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void RbPrivateOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
		{
			try
			{
				bool isChecked = RbPrivate.Checked;
				if (isChecked)
				{
					RbPublic.Checked = false;
					Status = "0";
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async void SaveDataButtonOnClick()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					if (string.IsNullOrEmpty(TxtNewplaylist.Text))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(TxtDescription.Text))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_playlist_description), ToastLength.Short)?.Show();
						return;
					}

					if (string.IsNullOrEmpty(Status))
					{
						Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_playlist_Status), ToastLength.Short)?.Show();
						return;
					}

					//Show a progress
					AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

					var (apiResult, respond) = await RequestsAsync.Playlist.EditPlaylistAsync(PlaylistId, TxtNewplaylist.Text, TxtDescription.Text, Status);
					if (apiResult == 200)
					{
						if (respond is MessageObject result)
						{
							Console.WriteLine(result.Message);
							PlayListVideoObject playLists = new PlayListVideoObject
							{
								Id = PlayListVideoObject.Id,
								ListId = PlaylistId,
								UserId = PlayListVideoObject.UserId,
								Name = TxtNewplaylist.Text,
								Description = TxtDescription.Text,
								Privacy = Convert.ToInt32(Status),
								Views = PlayListVideoObject.Views,
								Icon = PlayListVideoObject.Icon,
								Time = PlayListVideoObject.Time,
							};

							var dataPlayList = ListUtils.PlayListVideoObjectList?.FirstOrDefault(q => q.ListId == PlaylistId);
							if (dataPlayList != null)
							{
								dataPlayList.Id = playLists.Id;
								dataPlayList = playLists;
							}

							Console.WriteLine(dataPlayList);
							AndHUD.Shared.Dismiss();
							Toast.MakeText(this, GetText(Resource.String.Lbl_Created_successfully_playlist), ToastLength.Short)?.Show();

							Intent intent = new Intent();
							intent.PutExtra("ItemPlaylist", JsonConvert.SerializeObject(playLists));
							SetResult(Result.Ok, intent);
							Finish();
						}
					}
					else Methods.DisplayReportResult(this, respond);
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
	}
}