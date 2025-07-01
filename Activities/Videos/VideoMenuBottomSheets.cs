using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using PlayTube.Activities.Library;
using PlayTube.Activities.Models;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.Shorts;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Linq;
using Exception = System.Exception;

namespace PlayTube.Activities.Videos
{
	public class VideoMenuBottomSheets : BottomSheetDialogFragment
	{
		#region Variables Basic

		private LinearLayout MenuAddWatchLater, MenuDownload, MenuAddPlaylist, MenuRemoveFromPlaylist, MenuNotInterested, MenuShare, MenuReport, MenuEdit;
		private TextView TextWatchLater, TextNamePlaylist;
		private LibrarySynchronizer LibrarySynchronizer;
		private readonly VideoDataObject DataObject;
		private readonly IVideoMenuListener Listener;
		private readonly string NamePage = "home";
		#endregion

		#region General

		public VideoMenuBottomSheets(VideoDataObject item, IVideoMenuListener listener, string namePage = "home")
		{
			DataObject = item;
			Listener = listener;
			NamePage = namePage;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				// clone the inflater using the ContextThemeWrapper
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
				View view = localInflater?.Inflate(Resource.Layout.PopupVideoMoreLayout, container, false);
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
				InitComponent(view);
				LibrarySynchronizer = new LibrarySynchronizer(Activity);
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
				MenuAddWatchLater = view.FindViewById<LinearLayout>(Resource.Id.menu_AddWatchLater);
				TextWatchLater = view.FindViewById<TextView>(Resource.Id.text_watchlater);
				MenuDownload = view.FindViewById<LinearLayout>(Resource.Id.menu_download);
				MenuAddPlaylist = view.FindViewById<LinearLayout>(Resource.Id.menu_AddPlaylist);
				MenuRemoveFromPlaylist = view.FindViewById<LinearLayout>(Resource.Id.menu_RemoveFromPlaylist);
				TextNamePlaylist = view.FindViewById<TextView>(Resource.Id.text_namePlaylist);
				MenuNotInterested = view.FindViewById<LinearLayout>(Resource.Id.menu_not_interested);
				MenuShare = view.FindViewById<LinearLayout>(Resource.Id.menu_Share);
				MenuReport = view.FindViewById<LinearLayout>(Resource.Id.menu_Report);
				MenuEdit = view.FindViewById<LinearLayout>(Resource.Id.menu_edit);

				if (DataObject.IsOwner != null && DataObject.IsOwner.Value)
				{
					MenuNotInterested.Visibility = ViewStates.Gone;
					MenuReport.Visibility = ViewStates.Gone;
					MenuEdit.Visibility = ViewStates.Visible;
				}
				else
				{
					MenuEdit.Visibility = ViewStates.Gone;
				}


				if (NamePage == "WatchLater")
				{
					TextWatchLater.Text = GetText(Resource.String.Lbl_RemoveFromWatchLater);
				}

				if (NamePage == "SubPlayLists")
				{
					var instance = SubPlayListsVideosFragment.GetInstance();
					TextNamePlaylist.Text = GetText(Resource.String.Lbl_RemoveFrom) + " " + instance?.NamePlayList;

					MenuRemoveFromPlaylist.Visibility = ViewStates.Visible;
				}
				else
				{
					if (DataObject.IsPlaylist != null && DataObject.IsPlaylist.Value)
					{
						MenuRemoveFromPlaylist.Visibility = ViewStates.Visible;
					}
					else
					{
						MenuRemoveFromPlaylist.Visibility = ViewStates.Gone;
					}
				}

				if (DataObject.Source == "Uploaded")
				{
					MenuDownload.Visibility = ViewStates.Visible;
				}
				else
				{
					MenuDownload.Visibility = ViewStates.Gone;
				}

				MenuAddWatchLater.Click += MenuAddWatchLaterOnClick;
				MenuDownload.Click += MenuDownloadOnClick;
				MenuAddPlaylist.Click += MenuAddPlaylistOnClick;
				MenuRemoveFromPlaylist.Click += MenuRemoveFromPlaylistOnClick;
				MenuNotInterested.Click += MenuNotInterestedOnClick;
				MenuShare.Click += MenuShareOnClick;
				MenuReport.Click += MenuReportOnClick;
				MenuEdit.Click += MenuEditOnClick;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Event

		private void MenuEditOnClick(object sender, EventArgs e)
		{
			try
			{
				Bundle bundle = new Bundle();
				bundle.PutString("ItemDataVideo", JsonConvert.SerializeObject(DataObject));

				var editVideoFragment = new EditVideoFragment
				{
					Arguments = bundle
				};

				var globalContext = TabbedMainActivity.GetInstance();
				globalContext?.FragmentBottomNavigator.DisplayFragment(editVideoFragment);

				if (NamePage == "short")
				{
					ShortsVideoDetailsActivity.GetInstance()?.Finish();
				}

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuReportOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.AddReportVideo(DataObject);
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuShareOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.ShareVideo(DataObject);
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuNotInterestedOnClick(object sender, EventArgs e)
		{
			try
			{
				Listener?.RemoveVideo(DataObject);
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuAddPlaylistOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.AddToPlaylist(DataObject);
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private void MenuRemoveFromPlaylistOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.RemoveFromPlaylist(DataObject, NamePage);
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		private void MenuDownloadOnClick(object sender, EventArgs e)
		{
			try
			{
				if (PermissionsController.CheckPermissionStorage(Activity))
				{
					if (NamePage == "short")
					{
						var globalContext = ViewShortsVideoFragment.GetInstance();
						globalContext?.DownloadVideo();
					}
					else
					{
						var globalContext = TabbedMainActivity.GetInstance();
						globalContext?.VideoDataWithEventsLoader?.DownloadVideo();
					}
				}
				else
				{
					new PermissionsController(Activity).RequestPermission(100);
				}
				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private async void MenuAddWatchLaterOnClick(object sender, EventArgs e)
		{
			try
			{
				var (apiStatus, respond) = await RequestsAsync.Video.AddToWatchLaterVideosAsync(DataObject.Id);
				if (apiStatus == 200)
				{
					if (respond is MessageCodeObject result)
					{
						try
						{
							if (result.SuccessType.Contains("Removed"))
							{
								if (NamePage == "WatchLater")
								{
									var watchLaterPage = WatchLaterVideosFragment.GetInstance();
									var check = watchLaterPage?.MAdapter?.VideoList?.FirstOrDefault(a => a.Videos?.VideoAdClass.Id == DataObject.Id);
									if (check != null)
									{
										var index = watchLaterPage.MAdapter.VideoList.IndexOf(check);
										if (index != -1)
										{
											watchLaterPage.MAdapter.VideoList.Remove(check);
											watchLaterPage.MAdapter.NotifyDataSetChanged();

											watchLaterPage.ShowEmptyPage();
										}
									}
								}

								LibrarySynchronizer.RemovedFromWatchLater(DataObject);
								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_RemovedFromWatchLater), ToastLength.Short)?.Show();
							}
							else if (result.SuccessType.Contains("Added"))
							{
								LibrarySynchronizer.AddToWatchLater(DataObject);
								Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_AddedToWatchLater), ToastLength.Short)?.Show();
							}
							Dismiss();
						}
						catch (Exception exception)
						{
							Methods.DisplayReportResultTrack(exception);
							Dismiss();
						}
					}
				}
				else
				{
					Dismiss();
					Methods.DisplayReportResult(Activity, respond);
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