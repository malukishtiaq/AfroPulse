//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Dialog;
using PlayTube.Activities.Default;
using PlayTube.Activities.Models;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Upgrade;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PlayTube.Helpers.Controller
{
	public class PopupDialogController : Object, IDialogListCallBack, IDialogInputCallBack
	{
		private readonly Activity ActivityContext;
		private readonly VideoDataObject Videodata;
		private readonly string TypeDialog;

		public PopupDialogController(Activity activity, VideoDataObject videoobje, string typeDialog)
		{
			ActivityContext = activity;
			Videodata = videoobje;
			TypeDialog = typeDialog;
		}

		public void ShowPlayListDialog()
		{
			try
			{
				List<string> arrayAdapter = new List<string>();
				MaterialAlertDialogBuilder dialogList = new MaterialAlertDialogBuilder(ActivityContext);

				dialogList.SetTitle(ActivityContext.GetText(Resource.String.Lbl_Select_One_Name));

				var count = ListUtils.PlayListVideoObjectList.Count;
				if (count > 0)
				{
					arrayAdapter.AddRange(ListUtils.PlayListVideoObjectList.Select(data => data.Name));

					dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
				}
				else
				{
					dialogList.SetMessage(Resource.String.Lbl_Empty_PlayLists);
				}

				dialogList.SetPositiveButton(ActivityContext.GetText(Resource.String.Lbl_Creat_new), (sender, args) =>
				{
					try
					{
						ActivityContext.StartActivity(new Intent(ActivityContext, typeof(CreateNewPlaylistActivity)));
					}
					catch (Exception e)
					{
						Methods.DisplayReportResultTrack(e);
					}
				});
				dialogList.SetNegativeButton(ActivityContext.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void ShowNormalDialog(string title, string content = null, string positiveText = null, string negativeText = null)
		{
			try
			{
				MaterialAlertDialogBuilder dialogList = new MaterialAlertDialogBuilder(ActivityContext);

				if (!string.IsNullOrEmpty(title))
					dialogList.SetTitle(title);

				if (!string.IsNullOrEmpty(content))
					dialogList.SetMessage(content);

				if (!string.IsNullOrEmpty(negativeText))
				{
					dialogList.SetNegativeButton(negativeText, new MaterialDialogUtils());
				}

				if (!string.IsNullOrEmpty(positiveText))
				{
					dialogList.SetPositiveButton(positiveText, (sender, args) =>
					{
						try
						{
							switch (TypeDialog)
							{
								case "Login":
									TabbedMainActivity.GetInstance()?.StopFragmentVideo();
									ActivityContext.StartActivity(new Intent(ActivityContext, typeof(LoginActivity)));
									break;
								case "Logout":
									TabbedMainActivity.GetInstance()?.VideoDataWithEventsLoader.OnDestroy();
									ApiRequest.Logout(ActivityContext);
									break;
								case "Upgrade":
									TabbedMainActivity.GetInstance()?.StopFragmentVideo();
									ActivityContext.StartActivity(new Intent(ActivityContext, typeof(GoProActivity)));
									break;
							}
						}
						catch (Exception e)
						{
							Methods.DisplayReportResultTrack(e);
						}
					});
				}

				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public void ShowEditTextDialog(string title, string content = null, string positiveText = null, string negativeText = null)
		{
			try
			{
				var dialogList = new MaterialAlertDialogBuilder(ActivityContext);

				EditText input = new EditText(ActivityContext);

				input.InputType = InputTypes.ClassText | InputTypes.TextFlagMultiLine;
				LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
				input.LayoutParameters = lp;

				dialogList.SetView(input);

				if (!string.IsNullOrEmpty(title))
					dialogList.SetTitle(title);

				if (!string.IsNullOrEmpty(content))
					dialogList.SetMessage(content);

				if (!string.IsNullOrEmpty(negativeText))
				{
					dialogList.SetNegativeButton(negativeText, new MaterialDialogUtils());
				}

				if (!string.IsNullOrEmpty(positiveText))
				{
					dialogList.SetPositiveButton(positiveText, new MaterialDialogUtils(input, this));
				}
				dialogList.Show();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public async void OnSelection(IDialogInterface dialog, int position, string itemString)
		{
			try
			{
				if (TypeDialog == "PlayList")
				{
					var dataPlaylist = ListUtils.PlayListVideoObjectList.FirstOrDefault(a => a.Name == itemString);
					if (dataPlaylist != null)
					{
						if (Methods.CheckConnectivity())
						{
							var (apiStatus, respond) = await RequestsAsync.Playlist.AddToListAsync(Videodata.Id, dataPlaylist.ListId);
							if (apiStatus == 200)
							{
								if (respond is string message)
								{
									if (message.Contains("Added"))
									{
										new LibrarySynchronizer(ActivityContext).AddToPlaylistVideo(Videodata);
										Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_added), ToastLength.Short)?.Show();
									}
								}
							}
							else Methods.DisplayReportResult(ActivityContext, respond);
						}
						else
						{
							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnInput(IDialogInterface dialog, string input)
		{
			try
			{
				if (TypeDialog == "Report")
				{
					if (input.Length > 0)
					{
						if (Methods.CheckConnectivity())
						{
							PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.ReportVideoAsync(Videodata.Id, input) });

							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_received_your_report), ToastLength.Short)?.Show();
						}
						else
						{
							Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
						}
					}
					else
					{
						Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_The_name_can_not_be_blank), ToastLength.Short)?.Show();
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