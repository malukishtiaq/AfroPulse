using Android.App;
using Android.Widget;
using PlayTube.Activities.Chat;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Messages;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PlayTube.Helpers.Controller
{
	public static class MessageController
	{
		//############# DON'T MODIFY HERE #############
		//========================= Functions =========================

		public static async Task SendMessageTask(MessagesBoxActivity activity, string userId, string text, string hashId, UserDataObject userData)
		{
			try
			{
				var (apiStatus, respond) = await RequestsAsync.Messages.SendMessage(userId, text, hashId);
				if (apiStatus == 200)
				{
					if (respond is SendMessageObject result)
					{
						if (result.data != null)
						{
							UpdateLastIdMessage(result.data, userData);
						}
					}
				}
				else
				{
					Methods.DisplayReportResult(activity, respond);
					if (respond is ErrorObject error)
					{
						var errorText = error.errors.ErrorText;
						Toast.MakeText(Application.Context, errorText, ToastLength.Long)?.Show();
					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static void UpdateLastIdMessage(SendMessageObject.Data messages, UserDataObject userData)
		{
			try
			{
				GetUserMessagesObject.Message checker = MessagesBoxActivity.MAdapter.MessageList.FirstOrDefault(a => a.Id == messages.HashId);
				if (checker != null)
				{
					checker.Id = messages.Id;
					checker.FromId = messages.FromId;
					checker.ToId = messages.ToId;
					//checker.Text = messages.Text;
					checker.Seen = messages.Seen;
					checker.Time = messages.Time;
					checker.FromDeleted = messages.FromDeleted;
					checker.ToDeleted = messages.ToDeleted;
					checker.TextTime = messages.TextTime;
					checker.Position = "Right";

					var dataUser = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == messages.ToId);
					if (dataUser != null)
					{
						dataUser.GetLastMessage.Text = messages.Text;

						var index = LastChatActivity.MAdapter.UserList?.IndexOf(LastChatActivity.MAdapter.UserList?.FirstOrDefault(x => x.User.Id == messages.ToId));
						if (index > -1)
						{
							LastChatActivity.MAdapter.Move(dataUser);
							LastChatActivity.MAdapter.Update(dataUser);
						}
					}
					else
					{
						if (userData != null)
						{
							LastChatActivity.MAdapter?.Insert(new GetChatsObject.Data
							{
								Id = userData.Id,
								UserOne = "0",
								UserTwo = "0",
								Time = messages.Time,
								TextTime = messages.TextTime,
								User = userData,
								GetCountSeen = "0",
								GetLastMessage = new GetChatsObject.GetLastMessage
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
								},
							});
						}
					}

					SqLiteDatabase dbDatabase = new SqLiteDatabase();

					//Update All data users to database
					dbDatabase.InsertOrUpdateToOneMessages(checker);

					MessagesBoxActivity.UpdateOneMessage(checker);

					if (AppSettings.RunSoundControl)
						Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

	}
}