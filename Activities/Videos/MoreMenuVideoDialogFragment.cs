using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Androidx.Media3.Common;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PlayTube.Activities.Videos
{
	public class MoreMenuVideoDialogFragment : BottomSheetDialogFragment, IDialogListCallBack
	{
		#region  Variables Basic

		private RelativeLayout ReportCopyRightLayout, ReportLayout, QualityLayout, HelpFeedbackLayout, MakeOfflineLayout, PlaybackSpeedLayout;
		private TextView ReportCopyRightIcon, ReportCopyRightText, ReportIcon, ReportText, QualityIcon, QualityText, HelpFeedbackIcon, HelpFeedbackText, MakeOfflineIcon, MakeOfflineText, PlaybackSpeedIcon, PlaybackSpeedText;
		private LibrarySynchronizer LibrarySynchronizer;
		private VideoDataWithEventsLoader VideoController;
		private OnClickAction OnClickActionFor;

		private enum OnClickAction
		{
			None,
			Quality,
			PlaybackSpeed
		}

		#endregion

		#region General

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				Context contextThemeWrapper = AppTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
				LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
				View view = localInflater?.Inflate(Resource.Layout.MoreMenuVideoDialogLayout, container, false);
				return view;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return null;
			}
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				base.OnViewCreated(view, savedInstanceState);
				LibrarySynchronizer = new LibrarySynchronizer(Activity);
				VideoController = VideoDataWithEventsLoader.GetInstance();

				InitComponent(view);
				AddOrRemoveEvent(true);
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
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Functions

		private void InitComponent(View view)
		{
			try
			{
				ReportCopyRightLayout = view.FindViewById<RelativeLayout>(Resource.Id.ReportCopyRightLayout);
				ReportCopyRightIcon = view.FindViewById<TextView>(Resource.Id.ReportCopyRightIcon);
				ReportCopyRightText = view.FindViewById<TextView>(Resource.Id.ReportCopyRightText);

				ReportLayout = view.FindViewById<RelativeLayout>(Resource.Id.ReportLayout);
				ReportIcon = view.FindViewById<TextView>(Resource.Id.ReportIcon);
				ReportText = view.FindViewById<TextView>(Resource.Id.ReportText);

				QualityLayout = view.FindViewById<RelativeLayout>(Resource.Id.QualityLayout);
				QualityIcon = view.FindViewById<TextView>(Resource.Id.QualityIcon);
				QualityText = view.FindViewById<TextView>(Resource.Id.QualityText);

				PlaybackSpeedLayout = view.FindViewById<RelativeLayout>(Resource.Id.PlaybackSpeedLayout);
				PlaybackSpeedIcon = view.FindViewById<TextView>(Resource.Id.PlaybackSpeedIcon);
				PlaybackSpeedText = view.FindViewById<TextView>(Resource.Id.PlaybackSpeedText);

				HelpFeedbackLayout = view.FindViewById<RelativeLayout>(Resource.Id.HelpFeedbackLayout);
				HelpFeedbackIcon = view.FindViewById<TextView>(Resource.Id.HelpFeedbackIcon);
				HelpFeedbackText = view.FindViewById<TextView>(Resource.Id.HelpFeedbackText);

				MakeOfflineLayout = view.FindViewById<RelativeLayout>(Resource.Id.MakeOfflineLayout);
				MakeOfflineIcon = view.FindViewById<TextView>(Resource.Id.MakeOfflineIcon);
				MakeOfflineText = view.FindViewById<TextView>(Resource.Id.MakeOfflineText);

				FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, ReportCopyRightIcon, FontAwesomeIcon.Copyright);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ReportIcon, IonIconsFonts.Flag);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, QualityIcon, IonIconsFonts.Settings);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, PlaybackSpeedIcon, IonIconsFonts.Speedometer);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, HelpFeedbackIcon, IonIconsFonts.HelpCircle);
				FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, MakeOfflineIcon, IonIconsFonts.Videocam);

				var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
				if (!AppSettings.AllowOfflineDownload || AppSettings.AllowDownloadProUser && isPro == "0")
					MakeOfflineLayout.Visibility = ViewStates.Gone;

				//Add string
				if (!string.IsNullOrEmpty(VideoController.VideoData.QualityVideoSelect))
					QualityText.Text = Context.GetString(Resource.String.Lbl_Quality) + " - " + VideoController.VideoData.QualityVideoSelect;
				else
					QualityText.Text = Context.GetString(Resource.String.Lbl_Quality) + " - " + Context.GetText(Resource.String.Lbl_Auto) + " (" + VideoController.VideoData.VideoAuto + ")";

				var speed = AppTools.GetSpeedText(VideoController.PlayerView.Player.PlaybackParameters.Speed);
				var speedText = string.IsNullOrWhiteSpace(speed)
					? Context.GetString(Resource.String.Lbl_Normal)
					: speed;
				PlaybackSpeedText.Text = Context.GetString(Resource.String.Lbl_Playback_Speed) + " - " + speedText;
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
					ReportCopyRightLayout.Click += ReportCopyRightLayoutOnClick;
					ReportLayout.Click += ReportLayoutOnClick;
					QualityLayout.Click += QualityLayoutOnClick;
					PlaybackSpeedLayout.Click += PlaybackSpeedOnClick;
					HelpFeedbackLayout.Click += HelpFeedbackLayoutOnClick;
					MakeOfflineLayout.Click += MakeOfflineLayoutOnClick;
				}
				else
				{
					ReportCopyRightLayout.Click -= ReportCopyRightLayoutOnClick;
					ReportLayout.Click -= ReportLayoutOnClick;
					QualityLayout.Click -= QualityLayoutOnClick;
					PlaybackSpeedLayout.Click -= PlaybackSpeedOnClick;
					HelpFeedbackLayout.Click -= HelpFeedbackLayoutOnClick;
					MakeOfflineLayout.Click -= MakeOfflineLayoutOnClick;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Events

		private EditText TxtMessage;
		private CheckBox CheckBox1, CheckBox2;

		//Report Copyright
		private void ReportCopyRightLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (UserDetails.IsLogin)
				{
					if (Methods.CheckConnectivity())
					{
						var dialog = new MaterialAlertDialogBuilder(Activity);

						dialog.SetTitle(Resource.String.Lbl_ReportCopyRight);

						View view = LayoutInflater.Inflate(Resource.Layout.ReportCopyRightLayout, null);
						dialog.SetView(view);
						dialog.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), (o, args) =>
						{
							try
							{
								if (TxtMessage != null && CheckBox2 != null && CheckBox1 != null && CheckBox1.Checked && CheckBox2.Checked && !string.IsNullOrEmpty(TxtMessage.Text) && !string.IsNullOrWhiteSpace(TxtMessage.Text))
								{
									PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.ReportCopyRightVideosAsync(VideoController.VideoData.Id, TxtMessage.Text) });
									Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_received_your_report), ToastLength.Short)?.Show();
								}
								else
									Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Short)?.Show();
							}
							catch (Exception exception)
							{
								Methods.DisplayReportResultTrack(exception);
							}
						});
						dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

						TxtMessage = view.FindViewById<EditText>(Resource.Id.MessageEditText);
						CheckBox1 = view.FindViewById<CheckBox>(Resource.Id.CheckBoxReportCopyRight1);
						CheckBox2 = view.FindViewById<CheckBox>(Resource.Id.CheckBoxReportCopyRight2);

						dialog.Show();

						Dismiss();
					}
					else
					{
						Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
					}
				}
				else
				{
					PopupDialogController dialog = new PopupDialogController(Activity, VideoController.VideoData, "Login");
					dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Warning), Context.GetText(Resource.String.Lbl_Please_sign_in_Report), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
				}
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}


		// Add To Watch Offline Video
		private void MakeOfflineLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				if (PermissionsController.CheckPermissionStorage(Activity))
				{
					VideoController?.DownloadVideo();
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

		//Help
		private void HelpFeedbackLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.OnMenuHelpClick();
				VideoController?.OnStop();

				Dismiss();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		// Quality Video
		private void QualityLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				OnClickActionFor = OnClickAction.Quality;
				var dialogList = new MaterialAlertDialogBuilder(Context);

				var arrayAdapter = AppTools.GetListQualityVideo(VideoController.VideoData);

				dialogList.SetTitle(Context.GetString(Resource.String.Lbl_Quality));
				dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				dialogList.SetPositiveButton(Context.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		// Playback Speed
		private void PlaybackSpeedOnClick(object sender, EventArgs e)
		{
			try
			{
				OnClickActionFor = OnClickAction.PlaybackSpeed;
				var dialogList = new MaterialAlertDialogBuilder(Context);

				var playbackSpeeds = AppTools.GetListPlaybackSpeed();

				dialogList.SetTitle(Context.GetString(Resource.String.Lbl_Playback_Speed));
				dialogList.SetItems(playbackSpeeds.ToArray(), new MaterialDialogUtils(playbackSpeeds, this));
				dialogList.SetPositiveButton(Context.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		//Report Video
		private void ReportLayoutOnClick(object sender, EventArgs e)
		{
			try
			{
				LibrarySynchronizer.AddReportVideo(VideoController.VideoData);

				Dismiss();
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
				switch (OnClickActionFor)
				{
					case OnClickAction.Quality:
						{
							VideoController.VideoData.QualityVideoSelect = itemString;
							QualityText.Text = Context.GetString(Resource.String.Lbl_Quality) + " - " + itemString;

							var url = AppTools.GetLinkWithQuality(VideoController.VideoData, itemString);
							if (!string.IsNullOrEmpty(url))
							{
								VideoController.StartPlayVideo(url, VideoController.PlayerView.Player.CurrentPosition);
							}

							break;
						}
					case OnClickAction.PlaybackSpeed:
						{
							PlaybackSpeedText.Text = Context.GetString(Resource.String.Lbl_Playback_Speed) + " - " + itemString;
							var playbackParameters = new PlaybackParameters(AppTools.GetSpeed(itemString));
							VideoController.ExoController.ChangePlaybackSpeed(playbackParameters);
							break;
						}
				}

				OnClickActionFor = OnClickAction.None;
				Dismiss();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

	}
}