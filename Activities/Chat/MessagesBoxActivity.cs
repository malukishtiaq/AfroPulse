using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Newtonsoft.Json;
using PlayTube.Activities.Chat.Adapters;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.PlayTubeClient.Classes.Global;
using PlayTube.PlayTubeClient.Classes.Messages;
using PlayTube.PlayTubeClient.RestCalls;
using PlayTube.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using ActionMode = AndroidX.AppCompat.View.ActionMode;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Chat
{
	[Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	public class MessagesBoxActivity : AppCompatActivity, IOnClickListenerSelectedMessages
	{
		#region Variables Basic

		private ImageView ChatEmojiImage;
		private RelativeLayout RootView;
		private EmojiconEditText EmojiconEditTextView;
		private CircleButton ChatSendButton;
		private static Toolbar TopChatToolBar;
		public RecyclerView ChatBoxRecylerView;
		private LinearLayoutManager MLayoutManager;
		public static UserMessagesAdapter MAdapter;
		private string LastSeenUser = "", TypeChat = "", TaskWork = "";
		private string BeforeMessageId, FirstMessageId;
		private static string Userid;// to_id
		private static Timer Timer;
		private SwipeRefreshLayout SwipeRefreshLayout;
		private GetChatsObject.Data DataUser;
		private UserDataObject UserInfoData;
		private ActionModeCallback ModeCallback;
		private static ActionMode ActionMode;

		#endregion

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

				Window?.SetSoftInputMode(SoftInput.AdjustResize);
				base.OnCreate(savedInstanceState);

				//Window?.SetBackgroundDrawableResource(AppTools.IsTabDark() ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground3);

				// Set our view from the "MessagesBox_Layout" layout resource
				SetContentView(Resource.Layout.MessagesBoxLayout);

				var data = Intent?.GetStringExtra("UserId") ?? "Data not available";
				if (data != "Data not available" && !string.IsNullOrEmpty(data)) Userid = data; // to_id

				var type = Intent?.GetStringExtra("TypeChat") ?? "Data not available";
				if (type != "Data not available" && !string.IsNullOrEmpty(type))
				{
					TypeChat = type;
					string json = Intent?.GetStringExtra("UserItem");
					dynamic item;
					switch (type)
					{
						case "LastChat":
							item = JsonConvert.DeserializeObject<GetChatsObject.Data>(json);
							if (item != null) DataUser = item;
							break;
						case "Owner":
							item = JsonConvert.DeserializeObject<UserDataObject>(json);
							if (item != null) UserInfoData = item;
							break;
					}
				}

				//Get Value And Set Toolbar
				InitComponent();
				InitToolbar();
				SetRecyclerViewAdapters();

				//Set Title ToolBar and data chat user 
				loadData_ItemUser();

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

				if (Timer != null)
				{
					Timer.Enabled = true;
					Timer.Start();
				}
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

				if (Timer != null)
				{
					Timer.Enabled = false;
					Timer.Stop();
				}
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

		protected override void OnDestroy()
		{
			try
			{
				if (Timer != null)
				{
					Timer.Enabled = false;
					Timer.Stop();
				}

				base.OnDestroy();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			try
			{
				base.OnConfigurationChanged(newConfig);

				var currentNightMode = newConfig.UiMode & UiMode.NightMask;
				switch (currentNightMode)
				{
					case UiMode.NightNo:
						// Night mode is not active, we're using the light theme
						MainSettings.ApplyTheme(MainSettings.LightMode);
						break;
					case UiMode.NightYes:
						// Night mode is active, we're using dark theme
						MainSettings.ApplyTheme(MainSettings.DarkMode);
						break;
				}

				SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
				Window?.SetBackgroundDrawableResource(AppTools.IsTabDark() ? Resource.Drawable.chatBackground3_Dark : Resource.Drawable.chatBackground3);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}


		#region Functions

		private void InitComponent()
		{
			try
			{
				RootView = FindViewById<RelativeLayout>(Resource.Id.rootChatWindowView);

				ChatEmojiImage = FindViewById<ImageView>(Resource.Id.emojiIcon);
				EmojiconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
				ChatSendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
				ChatBoxRecylerView = FindViewById<RecyclerView>(Resource.Id.recyler);
				SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
				SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
				SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);

				ChatSendButton.Tag = "Text";
				ChatSendButton.SetImageResource(Resource.Drawable.pif_message_send);

				ModeCallback = new ActionModeCallback(this);

				var emojisIcon = new EmojIconActions(this, RootView, EmojiconEditTextView, ChatEmojiImage);
				emojisIcon.ShowEmojIcon();
				emojisIcon.SetIconsIds(Resource.Drawable.ic_action_keyboard, Resource.Drawable.icon_smile_vector);

				ChatEmojiImage.Visibility = ViewStates.Gone;
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
				TopChatToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
				if (TopChatToolBar != null)
				{
					TopChatToolBar.Title = "";
					TopChatToolBar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

					SetSupportActionBar(TopChatToolBar);
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

		private void SetRecyclerViewAdapters()
		{
			try
			{
				ChatBoxRecylerView.SetItemAnimator(null);
				MAdapter = new UserMessagesAdapter(this);
				MLayoutManager = new LinearLayoutManager(this);
				ChatBoxRecylerView.SetLayoutManager(MLayoutManager);
				ChatBoxRecylerView.SetAdapter(MAdapter);
				MAdapter.SetOnClickListener(this);
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
					ChatSendButton.Touch += Chat_sendButton_Touch;
				}
				else
				{
					ChatSendButton.Touch -= Chat_sendButton_Touch;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Set ToolBar and data chat user

		//Set ToolBar and data chat user
		private void loadData_ItemUser()
		{
			try
			{
				if (DataUser != null)
				{
					//SupportActionBar.Title = DataUser.User.Name;
					//SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(DataUser.Time));
					//LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(DataUser.Time));

					TextView toolbarUserName = FindViewById<TextView>(Resource.Id.toolbar_user_name);
					toolbarUserName.Text = DataUser.User.Name;


				}
				else if (UserInfoData != null)
				{
					//SupportActionBar.Title = UserInfoData.Name;
					//SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(UserInfoData.LastActive));
					//LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(UserInfoData.LastActive));


					TextView toolbarUserName = FindViewById<TextView>(Resource.Id.toolbar_user_name);
					toolbarUserName.Text = UserInfoData.Name;
				}

				Get_Messages();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private UserDataObject ConvertData()
		{
			try
			{
				UserDataObject userData = null;
				if (DataUser != null)
				{
					userData = DataUser.User;
				}
				else if (UserInfoData != null)
				{
					userData = UserInfoData;
				}

				return userData;
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return null;
			}
		}


		#endregion

		#region Get Messages

		//Get Messages Local Or Api
		private void Get_Messages()
		{
			try
			{
				BeforeMessageId = "0";
				MAdapter.Clear();

				SqLiteDatabase dbDatabase = new SqLiteDatabase();
				var localList = dbDatabase.GetMessagesList(UserDetails.UserId, Userid, BeforeMessageId);
				if (localList == "1") //Database.. Get Messages Local
				{
					MAdapter.BindEnd();

					//Scroll Down >> 
					ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);
					SwipeRefreshLayout.Refreshing = false;
					SwipeRefreshLayout.Enabled = false;

				}
				else //Or server.. Get Messages Api
				{
					SwipeRefreshLayout.Refreshing = true;
					SwipeRefreshLayout.Enabled = true;

					GetMessages_API();
				}

				//Set Event Scroll
				RecyclerViewOnScrollListener onScrollListener = new RecyclerViewOnScrollListener(MLayoutManager, SwipeRefreshLayout);
				onScrollListener.LoadMoreEvent += Messages_OnScroll_OnLoadMoreEvent;
				ChatBoxRecylerView.AddOnScrollListener(onScrollListener);
				TaskWork = "Working";

				//Run timer
				Timer = new Timer { Interval = AppSettings.MessageRequestSpeed, Enabled = true };
				Timer.Elapsed += TimerOnElapsed_MessageUpdater;
				Timer.Start();
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Get Messages From API 
		private async void GetMessages_API()
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					SwipeRefreshLayout.Refreshing = false;
					SwipeRefreshLayout.Enabled = false;

					Toast.MakeText(this, Resource.String.Lbl_CheckYourInternetConnection, ToastLength.Long)?.Show();
				}
				else
				{
					BeforeMessageId = "0";

					var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid, "30", "0");
					if (apiStatus == 200)
					{
						if (respond is GetUserMessagesObject result)
						{
							if (result.data.Messages.Count > 0)
							{
								MAdapter.MessageList = new ObservableCollection<GetUserMessagesObject.Message>(result.data.Messages.OrderBy(a => a.Time));
								MAdapter.BindEnd();

								//Insert to DataBase
								SqLiteDatabase dbDatabase = new SqLiteDatabase();
								dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

								//Scroll Down >> 
								ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

								SwipeRefreshLayout.Refreshing = false;
								SwipeRefreshLayout.Enabled = false;

							}
						}
					}
					else Methods.DisplayReportResult(this, respond);

					SwipeRefreshLayout.Refreshing = false;
					SwipeRefreshLayout.Enabled = false;

				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = false;

			}
		}

		#endregion

		//Timer Message Updater >> Get New Message
		private void TimerOnElapsed_MessageUpdater(object sender, ElapsedEventArgs e)
		{
			try
			{
				//Code get last Message id where Updater >>
				MessageUpdater();
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#region Updater

		private async void MessageUpdater()
		{
			try
			{
				if (TaskWork == "Working")
				{
					TaskWork = "Stop";

					if (!Methods.CheckConnectivity())
					{
						SwipeRefreshLayout.Refreshing = false;
						Toast.MakeText(this, Resource.String.Lbl_CheckYourInternetConnection, ToastLength.Long)?.Show();
					}
					else
					{
						int countList = MAdapter.MessageList.Count;
						string afterId = MAdapter.MessageList.LastOrDefault()?.Id ?? "";
						var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid, "30", "", afterId);
						if (apiStatus == 200)
						{
							if (respond is GetUserMessagesObject result)
							{
								int responseList = result.data.Messages.Count;
								if (responseList > 0)
								{
									if (countList > 0)
									{
										foreach (var item in result.data.Messages)
										{
											var check = MAdapter.MessageList.FirstOrDefault(a => a.Id == item.Id);
											if (check == null)
											{
												MAdapter.Add(item);
											}
										}
									}
									else
									{
										MAdapter.MessageList = new ObservableCollection<GetUserMessagesObject.Message>(result.data.Messages);
									}

									RunOnUiThread(() =>
									{
										try
										{
											var lastCountItem = MAdapter.ItemCount;
											if (countList > 0)
												MAdapter.NotifyItemRangeInserted(lastCountItem, MAdapter.MessageList.Count - 1);
											else
												MAdapter.NotifyDataSetChanged();

											//Insert to DataBase
											SqLiteDatabase dbDatabase = new SqLiteDatabase();
											dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);

											//Scroll Down >> 
											ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

											var lastMessage = MAdapter.MessageList.LastOrDefault();
											if (lastMessage != null)
											{
												var dataUser = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == lastMessage.FromId);
												if (dataUser != null)
												{
													dataUser.GetLastMessage.Text = lastMessage.Text;

													LastChatActivity.MAdapter.Move(dataUser);
													LastChatActivity.MAdapter.Update(dataUser);
												}
												if (AppSettings.RunSoundControl)
													Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
											}
										}
										catch (Exception e)
										{
											Methods.DisplayReportResultTrack(e);
										}
									});
								}
							}
						}
						else Methods.DisplayReportResult(this, respond);
					}
					TaskWork = "Working";
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public static void UpdateOneMessage(GetUserMessagesObject.Message message)
		{
			try
			{
				MAdapter.Update(message);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Load More

		private async void LoadMoreMessages()
		{
			try
			{
				//Run Load More Api 
				var local = LoadMoreMessagesDatabase();
				if (local == "1")
				{

				}
				else
				{
					var api = await LoadMoreMessagesApi();
					if (api == "1")
					{

					}
					else
					{
						SwipeRefreshLayout.Refreshing = false;
						SwipeRefreshLayout.Enabled = false;

					}
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		private string LoadMoreMessagesDatabase()
		{
			try
			{
				SqLiteDatabase dbDatabase = new SqLiteDatabase();
				var localList = dbDatabase.GetMessageList(UserDetails.UserId, Userid, FirstMessageId);
				if (localList?.Count > 0) //Database.. Get Messages Local
				{
					localList = new List<DataTables.MessageTb>(localList.OrderByDescending(a => a.Id));

					foreach (var messages in localList)
					{
						GetUserMessagesObject.Message m = new GetUserMessagesObject.Message
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

						MAdapter.Insert(m, FirstMessageId);
					}

					return "1";
				}
				else
				{
					return "0";
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
				return "0";
			}
		}

		private async Task<string> LoadMoreMessagesApi()
		{
			try
			{
				if (!Methods.CheckConnectivity())
				{
					SwipeRefreshLayout.Refreshing = false;
					Toast.MakeText(this, Resource.String.Lbl_CheckYourInternetConnection, ToastLength.Long)?.Show();
				}
				else
				{
					var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid, "30", FirstMessageId).ConfigureAwait(false);
					if (apiStatus == 200)
					{
						if (respond is GetUserMessagesObject result)
						{
							if (result.data.Messages.Count > 0)
							{
								var listApi = new ObservableCollection<GetUserMessagesObject.Message>();

								foreach (var messages in result.data.Messages.OrderBy(a => a.Time))
								{
									GetUserMessagesObject.Message message = new GetUserMessagesObject.Message
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

									MAdapter.Insert(message, FirstMessageId);

									listApi.Insert(0, message);

									SqLiteDatabase dbDatabase = new SqLiteDatabase();
									// Insert data user in database
									dbDatabase.InsertOrReplaceMessages(listApi);
								}
								return "1";
							}
							return "0";
						}
					}
					else Methods.DisplayReportResult(this, respond);
				}
				return "0";
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
				return "0";
			}
		}

		#endregion

		#region Events

		//Send Message type => "right_text"
		private void OnClick_OfSendButton()
		{
			try
			{
				var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				string timeNow = DateTime.Now.ToString("hh:mm");

				if (string.IsNullOrEmpty(EmojiconEditTextView.Text))
				{

				}
				else
				{
					//Here on This function will send Text Messages to the user 

					//remove \n in a string
					string replacement = Regex.Replace(EmojiconEditTextView.Text, @"\t|\n|\r", "");

					if (Methods.CheckConnectivity())
					{
						GetUserMessagesObject.Message message = new GetUserMessagesObject.Message
						{
							Id = unixTimestamp.ToString(),
							FromId = UserDetails.UserId,
							ToId = Userid,
							Text = replacement,
							Seen = "0",
							Time = unixTimestamp.ToString(),
							FromDeleted = "0",
							ToDeleted = "0",
							TextTime = timeNow,
							Position = "Right",
						};

						MAdapter.Add(message);

						UserDataObject userData = ConvertData();
						MessageController.SendMessageTask(this, Userid, EmojiconEditTextView.Text, unixTimestamp.ToString(), userData).ConfigureAwait(false);
					}
					else
					{
						Toast.MakeText(this, Resource.String.Lbl_CheckYourInternetConnection, ToastLength.Long)?.Show();
					}

					EmojiconEditTextView.Text = "";
				}

				ChatSendButton.Tag = "Text";
				ChatSendButton.SetImageResource(Resource.Drawable.pif_message_send);
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		//Event click send messages type text
		private void Chat_sendButton_Touch(object sender, View.TouchEventArgs e)
		{
			try
			{
				if (e.Event?.Action == MotionEventActions.Down)
				{
					OnClick_OfSendButton();
				}
				e.Handled = false;
			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		#endregion

		#region Scroll

		//Event Scroll #Messages
		private void Messages_OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
		{
			try
			{
				//Start Loader Get from Database or API Request >>
				SwipeRefreshLayout.Refreshing = true;
				SwipeRefreshLayout.Enabled = true;

				FirstMessageId = "0";

				//Code get first Message id where LoadMore >>
				var mes = MAdapter.MessageList.FirstOrDefault();
				if (mes != null)
				{
					FirstMessageId = mes.Id;
				}

				if (Convert.ToInt32(FirstMessageId) > 0)
				{
					LoadMoreMessages();
				}

				SwipeRefreshLayout.Refreshing = false;
				SwipeRefreshLayout.Enabled = false;

			}
			catch (Exception exception)
			{
				Methods.DisplayReportResultTrack(exception);
			}
		}

		private class RecyclerViewOnScrollListener : RecyclerView.OnScrollListener
		{
			public delegate void LoadMoreEventHandler(object sender, EventArgs e);

			public event LoadMoreEventHandler LoadMoreEvent;

			private readonly LinearLayoutManager LayoutManager;
			private readonly SwipeRefreshLayout SwipeRefreshLayout;

			public RecyclerViewOnScrollListener(LinearLayoutManager layoutManager, SwipeRefreshLayout swipeRefreshLayout)
			{
				LayoutManager = layoutManager;
				SwipeRefreshLayout = swipeRefreshLayout;
			}

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				try
				{
					base.OnScrolled(recyclerView, dx, dy);

					var visibleItemCount = recyclerView.ChildCount;
					var totalItemCount = recyclerView.GetAdapter().ItemCount;

					var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
					if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
					{
						//Load More  from API Request
						if (LoadMoreEvent != null) LoadMoreEvent(this, null);
						//Start Load More messages From Database
					}
					else
					{
						if (SwipeRefreshLayout.Refreshing)
						{
							SwipeRefreshLayout.Refreshing = false;
							SwipeRefreshLayout.Enabled = false;

						}
					}
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}

		#endregion

		#region Menu

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater?.Inflate(Resource.Menu.MessagesBox_Menu, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;
				case Resource.Id.menu_clear_chat:
					OnMenuClearChatClick();
					break;
			}

			return base.OnOptionsItemSelected(item);
		}

		private void OnMenuClearChatClick()
		{
			try
			{
				if (Methods.CheckConnectivity())
				{
					MAdapter.Clear();

					var userDelete = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.User.Id == Userid);
					if (userDelete != null)
					{
						LastChatActivity.MAdapter.Remove(userDelete);
					}

					SqLiteDatabase dbDatabase = new SqLiteDatabase();
					dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, Userid);
					dbDatabase.DeleteUserLastChat(Userid);

					PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Messages.DeleteChat(Userid) });

				}
				else
				{
					Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

		#region Toolbar & Selected

		public class ActionModeCallback : Java.Lang.Object, ActionMode.ICallback
		{
			private readonly MessagesBoxActivity Activity;
			public ActionModeCallback(MessagesBoxActivity activity)
			{
				Activity = activity;
			}

			public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
			{
				int id = item.ItemId;
				switch (id)
				{
					//if (id == Resource.Id.action_delete)
					//{
					//    DeleteItems();
					//    mode.Finish();
					//    return true;
					//}
					//else
					case Resource.Id.action_copy:
						CopyItems();
						mode.Finish();
						return true;
					case Android.Resource.Id.Home:
						{
							if (Timer != null)
							{
								Timer.Enabled = true;
								Timer.Start();
							}

							MAdapter.ClearSelections();

							TopChatToolBar.Visibility = ViewStates.Visible;
							ActionMode.Finish();

							return true;
						}
					default:
						return false;
				}
			}

			public bool OnCreateActionMode(ActionMode mode, IMenu menu)
			{
				SetSystemBarColor(Activity, AppSettings.MainColor);
				mode.MenuInflater?.Inflate(Resource.Menu.menuChat, menu);
				return true;
			}

			private void SetSystemBarColor(Activity act, string color)
			{
				try
				{
					if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
					{
						Window window = act.Window;
						window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
						window.ClearFlags(WindowManagerFlags.TranslucentStatus);
						window.SetStatusBarColor(Color.ParseColor(color));
					}
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public void OnDestroyActionMode(ActionMode mode)
			{
				try
				{
					MAdapter.ClearSelections();
					ActionMode = null;
					SetSystemBarColor(Activity, AppSettings.MainColor);

					if (Timer != null)
					{
						Timer.Enabled = true;
						Timer.Start();
					}

					TopChatToolBar.Visibility = ViewStates.Visible;
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}

			public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
			{
				return false;
			}

			//Copy Messages
			private void CopyItems()
			{
				try
				{
					if (Timer != null)
					{
						Timer.Enabled = true;
						Timer.Start();
					}

					if (TopChatToolBar.Visibility != ViewStates.Visible)
						TopChatToolBar.Visibility = ViewStates.Visible;

					string allText = "";
					List<int> selectedItemPositions = MAdapter.GetSelectedItems();
					for (int i = selectedItemPositions.Count - 1; i >= 0; i--)
					{
						var datItem = MAdapter.GetItem(selectedItemPositions[i]);
						if (datItem != null)
						{
							allText = allText + " \n" + datItem.Text;
						}
					}

					ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService(ClipboardService);
					ClipData clip = ClipData.NewPlainText("clipboard", allText);
					clipboard.PrimaryClip = clip;

					MAdapter.NotifyDataSetChanged();

					Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
				}
				catch (Exception e)
				{
					Methods.DisplayReportResultTrack(e);
				}
			}
		}

		public void OnItemClick(View view, GetUserMessagesObject.Message obj, int pos)
		{
			try
			{
				if (MAdapter.GetSelectedItemCount() > 0) // Add Select New Item 
				{
					EnableActionMode(pos);
				}
				else
				{
					if (Timer != null)
					{
						Timer.Enabled = true;
						Timer.Start();
					}

					if (TopChatToolBar.Visibility != ViewStates.Visible)
						TopChatToolBar.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		public void OnItemLongClick(View view, GetUserMessagesObject.Message obj, int pos)
		{
			EnableActionMode(pos);
		}

		private void EnableActionMode(int position)
		{
			if (ActionMode == null)
			{
				ActionMode = StartSupportActionMode(ModeCallback);
			}
			ToggleSelection(position);
		}

		private void ToggleSelection(int position)
		{
			try
			{
				MAdapter.ToggleSelection(position);
				int count = MAdapter.GetSelectedItemCount();

				if (count == 0)
				{
					if (Timer != null)
					{
						Timer.Enabled = true;
						Timer.Start();
					}

					TopChatToolBar.Visibility = ViewStates.Visible;
					ActionMode.Finish();
				}
				else
				{
					if (Timer != null)
					{
						Timer.Enabled = false;
						Timer.Stop();
					}

					TopChatToolBar.Visibility = ViewStates.Gone;
					ActionMode.SetTitle(count);
					ActionMode.Invalidate();
				}
			}
			catch (Exception e)
			{
				Methods.DisplayReportResultTrack(e);
			}
		}

		#endregion

	}
}