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
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using Com.Google.Android.Gms.Ads;
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Playlist;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Playlist
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class CreateNewPlaylistActivity : BaseActivity
	{
		#region Variables Basic

		private EditText TxtNewplaylist, TxtDescription;
		private RadioButton RbPrivate, RbPublic;
		private TextView SaveTextView;
        private string Status = "";
		private AdView MAdView;
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

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
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
				var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (toolBar != null)
				{
					toolBar.Title = GetText(Resource.String.Lbl_Creat_New_Playlist);
					toolBar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(toolBar);
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
                    SaveTextView.Click += SaveTextViewOnClick;
				}
                else
				{
					//Event  
					RbPrivate.CheckedChange -= RbPrivateOnCheckedChange;
					RbPublic.CheckedChange -= RbPublicOnCheckedChange;
                    SaveTextView.Click -= SaveTextViewOnClick;
				}
            }
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private async void SaveTextViewOnClick(object sender, EventArgs e)
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

                    var (apiResult, respond) = await RequestsAsync.Playlist.CreatePlaylistAsync(TxtNewplaylist.Text, TxtDescription.Text, Status);

                    if (apiResult == 200)
                    {
                        if (respond is CreatePlaylistObject result)
                        { 
                            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
							 
                            var adapter = TabbedMainActivity.GetInstance()?.LibraryFragment?.MAdapter;
                            if (adapter != null)
                            {
                                PlayListVideoObject playLists = new PlayListVideoObject
                                {
                                    Id = result.PlaylistId,
                                    ListId = result.PlaylistUid,
                                    UserId = Convert.ToInt32(UserDetails.UserId),
                                    Name = TxtNewplaylist.Text,
                                    Description = TxtDescription.Text,
                                    Privacy = Convert.ToInt32(Status),
                                    Views = 0,
                                    Icon = "",
                                    Time = unixTimestamp,
                                    VideosList = new List<VideoDataObject>()
                                };

                                adapter.PlayListsList.Add(playLists);
                                adapter.NotifyItemInserted(adapter.PlayListsList.Count - 1);
                            }
							 
                            AndHUD.Shared.Dismiss();
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Created_successfully_playlist), ToastLength.Short)?.Show();

                            Finish();
                        }
                    }
                    else Methods.DisplayAndHudErrorResult(this, respond);
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
		#endregion

	}
}